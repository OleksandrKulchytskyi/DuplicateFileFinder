using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileHelper.Common;

namespace Finder.test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			IFileHasherFinder finder = new FileHasherFinder();
			var task = finder.DoSearch(@"D:\ebooks", "*.pdf", true);
			var task2 = finder.DoSearch(@"D:\Dropbox\SkyDrive\eBooks", "*.pdf", true);

			try
			{
				System.Threading.Tasks.Task.WaitAll(task, task2);
			}
			catch (AggregateException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			if ((!task.IsFaulted && !task.IsCanceled))
			{
				Assert.IsNotNull(task.Result);

				var list1 = task.Result.ToList();
				var list2 = task2.Result.ToList();
				int count1 = list1.Count();
				int count2 = list2.Count();
				//var list3 = task3.Result;

				var grouping = from item in list1
							   group item by item.ShaCode into groups
							   orderby groups.Key
							   where groups.Count() > 1
							   select groups;

				var gpList1 = grouping.ToList();
				if (gpList1 != null)
				{
				}

				var grouping2 = from item in list2
								group item by item.ShaCode into groups
								orderby groups.Key
								where groups.Count() > 1
								select groups;

				var gpList2 = grouping2.ToList();
				if (gpList2 != null)
				{
				}

				var duplicate = (from item in list1
								 from item2 in list2
								 where item.ShaCode.Equals(item2.ShaCode)
								 select new { SHA = item.ShaCode, PATH1 = item.PathToFile, PATH2 = item2.PathToFile }).ToList();
				if (duplicate != null)
				{
				}
			}
		}

		[TestMethod]
		public void TestMethod2()
		{
			IFileHasherFinder finder = new FileHasherFinder();
			var task = finder.DoSearch(@"D:\eBooks", "*.pdf", true);

			try
			{
				System.Threading.Tasks.Task.WaitAll(task);
			}
			catch (AggregateException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			if ((!task.IsFaulted && !task.IsCanceled))
			{
				Assert.IsNotNull(task.Result);

				var list1 = task.Result.ToList();

				int count1 = list1.Count();
				//var list3 = task3.Result;

				var grouping = from item in list1
							   group item by item.ShaCode into groups
							   orderby groups.Key
							   where groups.Count() > 1
							   select new { SHA = groups.Key, Data = groups };

				var gpList1 = grouping.ToList();
				if (gpList1 != null)
				{
					foreach (var val in gpList1)
					{
						string files = string.Join(Environment.NewLine, from file in val.Data select file.PathToFile);
						if (files != null)
						{

						}
					}
				}
			}
		}

		[TestMethod]
		public void TestMethodReadAsync()
		{
			var task2 = FileHasherFinder.ReadFileAsync(@"D:\eBooks\Professional Visual Studio 2010.pdf", 1024);
			try
			{
				task2.Wait();
			}
			catch (AggregateException ex)
			{ }
			catch (Exception ex)
			{ }
			if (task2 != null)
			{

			}

			var task = FileHasherFinder.ReadBufferAsync(@"D:\eBooks\Professional Visual Studio 2010.pdf", 1024);
			try
			{
				task.Wait();
			}
			catch (AggregateException ex)
			{ }
			catch (Exception ex)
			{ }
		}
	}
}
