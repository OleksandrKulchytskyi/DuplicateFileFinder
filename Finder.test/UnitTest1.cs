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
<<<<<<< HEAD
			//var task = finder.DoSearch(@"d:\ebooks", "*.pdf", true);
			var task = finder2.DoSearch(@"D:\Dropbox\SkyDrive\eBooks", "*.pdf", true);
			//var task3 = finder.DoSearch(@"D:\downloads", "*.*", true);
			try
			{
				System.Threading.Tasks.Task.WaitAll(task);//, task3);
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

				var list1 = task.Result;
				//var list2 = task2.Result;
				int count1 = list1.Count();
				//int count2 = list2.Count();
				//var list3 = task3.Result;

				var grouping = from item in list1
							   group item by item.ShaCode into groups
							   orderby groups.Key
							   where groups.Count() > 1
							   select groups;

				if (grouping.ToList() != null)
=======
			var task = finder.DoSearch(@"d:\ebooks", "*.jpg", true);
			//var task2 = finder2.DoSearch(@"D:\Dropbox\SkyDrive\eBooks", "*.*", true);
			//var task3 = finder.DoSearch(@"D:\downloads", "*.*", true);

			task.Wait();

			if (!task.IsFaulted)
			{
				int count = task.Result.Count();
				var data = from item in task.Result
							   group item by item.ShaCode into grouping
							   orderby grouping.Key
							   where grouping.Count() < 1
							   select grouping;

				if(data.ToList()!=null)
>>>>>>> 7e4d6dc9b5954e37c14887bff244dba96ebc800c
				{

				}
			}
			//try
			//{
			//	System.Threading.Tasks.Task.WaitAll(task, task2, task3);
			//}
			//catch (AggregateException ex)
			//{
			//	System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
			//}
			//catch (Exception ex)
			//{
			//	System.Diagnostics.Debug.WriteLine(ex.Message);
			//}
			//if ((!task.IsFaulted && !task.IsCanceled) && (!task2.IsFaulted && !task2.IsCanceled))
			//{
			//	Assert.IsNotNull(task.Result);
			//	Assert.IsNotNull(task2.Result);

<<<<<<< HEAD

				//var overall2 = (from item1 in list2
				//				from item2 in list3
				//				where item1.ShaCode.Equals(item2.ShaCode, StringComparison.OrdinalIgnoreCase)
				//				select new { Path1 = item2.PathToFile, Path2 = item1.PathToFile, }).ToList();
				//if (overall.Count > 0)
				//{

				//}
			}
			else
				Assert.Fail();
=======
			//	var list1 = task.Result;
			//	var list2 = task2.Result;
			//	int count1 = list1.Count();
			//	int count2 = list2.Count();
			//	var list3 = task3.Result;

			//	Assert.AreNotEqual(count1, count2);

			//	var overall = (from item1 in list1
			//				   from item2 in list2
			//				   where item1.Equals(item2)
			//				   select new { Path1 = item2.PathToFile, Path2 = item1.PathToFile, }).ToList();
			//	if (overall.Count > 0)
			//	{

			//	}

			//	var overall2 = (from item1 in list2
			//					from item2 in list3
			//					where item1.ShaCode.Equals(item2.ShaCode, StringComparison.OrdinalIgnoreCase)
			//					select new { Path1 = item2.PathToFile, Path2 = item1.PathToFile, }).ToList();
			//	if (overall.Count > 0)
			//	{

			//	}
			//}
			//else
			//	Assert.Fail();
>>>>>>> 7e4d6dc9b5954e37c14887bff244dba96ebc800c
		}
	}
}
