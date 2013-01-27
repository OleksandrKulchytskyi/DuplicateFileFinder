using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileHelper.Common
{
	public interface IFileHasherFinder
	{
		Task<IEnumerable<DuplicateItem>> DoSearch(string fpath, string pattern, bool includeSubFolders);
	}

	public class FileHasherFinder : IFileHasherFinder
	{
		private const int _maxTolerance = 2;

		public Task<IEnumerable<DuplicateItem>> DoSearch(string fpath, string pattern, bool includeSubFolders)
		{
			TaskCompletionSource<IEnumerable<DuplicateItem>> tcs = new TaskCompletionSource<IEnumerable<DuplicateItem>>();
			if (System.IO.Directory.Exists(fpath) && !string.IsNullOrEmpty(pattern))
			{
				BlockingCollection<DuplicateItem> blockColl = null;
				string path = fpath;
				CancellationTokenSource cts = new CancellationTokenSource();
				Task.Factory.StartNew(new Action(() =>
				{
					blockColl = new System.Collections.Concurrent.BlockingCollection<DuplicateItem>();
					IEnumerable<string> data = Directory.EnumerateFiles(path, pattern, includeSubFolders == true ?
																		SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
					var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cts.Token };
					using (ThreadLocal<LoopSt> retryCount = new ThreadLocal<LoopSt>(() => new LoopSt() { Exceptional = false, Count = 0 }))
					{
						try
						{
							Parallel.ForEach(data, parallelOption, file =>
							{
								retryCount.Value.Exceptional = false;
								do
								{
									if (parallelOption.CancellationToken.IsCancellationRequested)
										parallelOption.CancellationToken.ThrowIfCancellationRequested();

									if (retryCount.Value.Exceptional)
									{
#if DEBUG
										System.Diagnostics.Debug.WriteLine("Grant access for {0}", file);
#endif
										GrantAccess(file);
									}
									try
									{
										if (System.IO.File.Exists(file))
										{
											int flen = 0;
											DuplicateItem item = new DuplicateItem();
											item.ShaCode = GetHash(file, out flen);
											item.PathToFile = file;
											item.Size = flen;
											blockColl.TryAdd(item, new TimeSpan(0, 0, 0, 0, 100));
										}
										retryCount.Value.Exceptional = false;
										retryCount.Value.Count = 0;
									}
									catch (UnauthorizedAccessException) { retryCount.Value.Exceptional = true; retryCount.Value.Count++; }
									catch (IOException) { }
									catch (InvalidOperationException) { }
								} while (retryCount.Value.Exceptional && (retryCount.Value.Count <= _maxTolerance));
							});
						}
						catch (OperationCanceledException) { tcs.SetCanceled(); }
					}
					blockColl.CompleteAdding();
					if (!cts.IsCancellationRequested)
						tcs.TrySetResult(blockColl.GetConsumingEnumerable());
					else
						tcs.TrySetResult(null);

				}), TaskCreationOptions.LongRunning);
			}
			else
				tcs.TrySetResult(null);

			return tcs.Task;
		}

		private string GetHash(string filePath, out int len)
		{
			byte[] data = new byte[524288];
			using (SHA256Managed sha = new SHA256Managed())
			using (FileStream fs = new FileStream(filePath, FileMode.Open))
			{
				len = (int)fs.Length;
				int bytesRead = fs.Read(data, 0, data.Length);
				byte[] hash = sha.ComputeHash(data, 0, bytesRead);
				return BitConverter.ToString(hash).Replace("-", String.Empty);
			}
		}

		private static void GrantAccess(string filepath)
		{

			var fs = File.GetAccessControl(filepath);
			var sid = fs.GetOwner(typeof(SecurityIdentifier));
			var ntAccount = new NTAccount(Environment.UserDomainName, Environment.UserName);
			try
			{
				var currentRules = fs.GetAccessRules(true, false, typeof(NTAccount));
				foreach (var r in currentRules.OfType<FileSystemAccessRule>())
				{
					Console.WriteLine(string.Format("{0} {1}", r.AccessControlType, r.FileSystemRights));
				}
				var newRule = new FileSystemAccessRule(ntAccount, FileSystemRights.FullControl, AccessControlType.Allow);
				fs.AddAccessRule(newRule);
				File.SetAccessControl(filepath, fs);
			}
			catch { }
			finally { fs = null; sid = null; ntAccount = null; }
		}
	}


	public class DuplicateItem : IEquatable<DuplicateItem>
	{
		[Description("File path")]
		public string PathToFile { get; set; }

		private float myVar;
		[Description("Size (Mb)")]
		public float Size
		{
			get { return myVar; }
			set
			{
				if (value > 0)
					myVar = (float)((value / 1024.0) / 1024.0);
			}
		}

		[Description("SHA code")]
		public string ShaCode { get; set; }

		public bool Equals(DuplicateItem other)
		{
			if (other == null)
				return false;
			return this.ShaCode.Equals(other.ShaCode, StringComparison.OrdinalIgnoreCase);
		}
	}

	class LoopSt
	{
		public int Count { get; set; }
		public bool Exceptional { get; set; }
	}
}
