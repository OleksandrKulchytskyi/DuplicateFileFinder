﻿using System;
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
			//var task = finder.DoSearch(@"d:\ebooks", "*.pdf", true);
			var task = finder.DoSearch(@"D:\eBooks", "*.pdf", true);
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

				{

				}
			}
		}
	}
}
