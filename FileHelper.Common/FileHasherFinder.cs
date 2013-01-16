using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
					try
					{
						Parallel.ForEach(data, parallelOption, file =>
						{
							if (parallelOption.CancellationToken.IsCancellationRequested)
								parallelOption.CancellationToken.ThrowIfCancellationRequested();

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
							}
							catch (UnauthorizedAccessException) { }
							catch (IOException) { }
							catch (InvalidOperationException) { }
						});
					}
					catch (OperationCanceledException) { tcs.SetCanceled(); }
					blockColl.CompleteAdding();
					if (!cts.IsCancellationRequested)
						tcs.TrySetResult(blockColl.GetConsumingEnumerable());
					else
						tcs.TrySetResult(null);

				}), TaskCreationOptions.LongRunning);
			}
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
	}


	public class DuplicateItem
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
	}
}
