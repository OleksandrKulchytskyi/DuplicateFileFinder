using FileHelper.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DuplicateFileFounder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<string> _listExtensions = null;
		[Import(typeof(IFileHasherFinder))]
		private IFileHasherFinder _finderCore;


		public MainWindow()
		{
			InitializeComponent();

			AggregateCatalog aggregateCatalogue = new AggregateCatalog();
			aggregateCatalogue.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly()));
			aggregateCatalogue.Catalogs.Add(new AssemblyCatalog(typeof(IFileHasherFinder).Assembly));
			//aggregateCatalogue.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));
			
			CompositionContainer container = new CompositionContainer(aggregateCatalogue);
			container.ComposeParts(this);
			
			_listExtensions = new List<string>();
			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

		public string FolderPath
		{
			get { return (string)GetValue(FolderPathProperty); }
			set { SetValue(FolderPathProperty, value); }
		}

		public static readonly DependencyProperty FolderPathProperty =
			DependencyProperty.Register("FolderPath", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsBusy.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsBusyProperty =
			DependencyProperty.Register("IsBusy", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_listExtensions.Add("*.jpg");
			_listExtensions.Add("*.pdf");
			_listExtensions.Add("*.djvu");
			_listExtensions.Add("*.mp3");
			_listExtensions.Add("*.avi");
			_listExtensions.Add("*.epub");
			_listExtensions.Add("*.wmv");
			_listExtensions.Add("*.mp4");
			_listExtensions.Add("*.cs");
			_listExtensions.Add("*.cpp");
			_listExtensions.Add("*.js");

			_listExtensions.Sort();

			cmbExt.ItemsSource = _listExtensions;
		}

		private void dg1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			if (e.Column != null)
			{
				if (e.PropertyDescriptor != null)
				{
					Type t = typeof(DuplicateItem);
					System.Reflection.PropertyInfo p = t.GetProperty(e.PropertyName);
					object[] data = p.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
					if (data != null)
					{
						if ((data[0] as System.ComponentModel.DescriptionAttribute).Description != null)
						{
							e.Column.Header = (data[0] as System.ComponentModel.DescriptionAttribute).Description;
						}
					}
				}
			}
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			string ext = cmbExt.SelectedItem as string;
			if (System.IO.Directory.Exists(FolderPath) && !string.IsNullOrEmpty(ext))
			{
				if (dg1.ItemsSource != null && (dg1.ItemsSource as ObservableCollection<DuplicateItem>).Count > 0)
				{
					(dg1.ItemsSource as ObservableCollection<DuplicateItem>).Clear();
					dg1.ItemsSource = null;
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				}

				string path = FolderPath;
				IsBusy = true;

				var mainTask = _finderCore.DoSearch(path, ext, true);

				mainTask.ContinueWith(prevTask =>
					{
						IsBusy = false;
						prevTask.Dispose();
						dg1.ItemsSource = new ObservableCollection<DuplicateItem>(mainTask.Result);
					}, System.Threading.CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext());

				mainTask.ContinueWith(prevTask =>
				{
					IsBusy = false;
					dg1.ItemsSource = null;
					prevTask.Dispose();
				}, System.Threading.CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void btnGroup_Click(object sender, RoutedEventArgs e)
		{
			ListCollectionView view = new ListCollectionView(dg1.ItemsSource as ObservableCollection<DuplicateItem>);
			view.GroupDescriptions.Add(new PropertyGroupDescription("ShaCode"));
			dg1.ItemsSource = view;
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (dg1.SelectedItem != null && dg1.SelectedItem is DuplicateItem)
			{
				string caption = string.Format("Are you sure to delete \r\n {0}", System.IO.Path.GetFileName((dg1.SelectedItem as DuplicateItem).PathToFile));
				if (MessageBox.Show(this, caption, "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						File.Delete((dg1.SelectedItem as DuplicateItem).PathToFile);
					}
					catch (UnauthorizedAccessException) { }
					catch (IOException) { }
				}
			}
		}

		private void miExit_Click(object send, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}
	}
}