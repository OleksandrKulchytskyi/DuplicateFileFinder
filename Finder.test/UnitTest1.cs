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
			IFileHasherFinder finder2 = new FileHasherFinder();
			var task = finder.DoSearch(@"d:\ebooks", "*.*", true);
			var task2 = finder2.DoSearch(@"D:\Dropbox\SkyDrive\eBooks", "*.*", true);
			var task3 = finder.DoSearch(@"D:\downloads", "*.*", true);
			try
			{
				System.Threading.Tasks.Task.WaitAll(task, task2, task3);
			}
			catch (AggregateException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			if ((!task.IsFaulted && !task.IsCanceled) && (!task2.IsFaulted && !task2.IsCanceled))
			{
				Assert.IsNotNull(task.Result);
				Assert.IsNotNull(task2.Result);

				var list1 = task.Result;
				var list2 = task2.Result;
				int count1 = list1.Count();
				int count2 = list2.Count();
				var list3 = task3.Result;

				Assert.AreNotEqual(count1, count2);

				var overall = (from item1 in list1
							   from item2 in list2
							   where item1.ShaCode.Equals(item2.ShaCode, StringComparison.OrdinalIgnoreCase)
							   select new { Path1 = item2.PathToFile, Path2 = item1.PathToFile, }).ToList();
				if (overall.Count > 0)
				{

				}

				var overall2 = (from item1 in list2
								from item2 in list3
								where item1.ShaCode.Equals(item2.ShaCode, StringComparison.OrdinalIgnoreCase)
								select new { Path1 = item2.PathToFile, Path2 = item1.PathToFile, }).ToList();
				if (overall.Count > 0)
				{

				}
			}
			else
				Assert.Fail();
		}
	}
}
