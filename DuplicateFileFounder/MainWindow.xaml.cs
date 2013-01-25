using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace DuplicateFileFounder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private BlockingCollection<DuplicateItem> blockColl = null;
		List<string> _listExtensions = null;

		public MainWindow()
		{
			InitializeComponent();
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


		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_listExtensions.Add("*.jpg");
			_listExtensions.Add("*.pdf");
			_listExtensions.Add("*.djvu");
			_listExtensions.Add("*.mp3");
			_listExtensions.Add("*.avi");
			_listExtensions.Add("*.epub");
			_listExtensions.Add("*.wmv");
			_listExtensions.Add("*.mp4");

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
				if(blockColl!=null && blockColl.Count>0)
				{
					blockColl = null;
					dg1.ItemsSource = null;
					GC.Collect();
				}

				string path = FolderPath;

				IsBusy = true;
				var mainTask = Task.Factory.StartNew(new Action(() =>
					{
						blockColl = new System.Collections.Concurrent.BlockingCollection<DuplicateItem>();
						IEnumerable<string> data = System.IO.Directory.EnumerateFiles(path, ext, System.IO.SearchOption.AllDirectories);

						Parallel.ForEach(data, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
							{
								try
								{
									if (System.IO.File.Exists(file))
									{
										int flen = 0;
										DuplicateItem item = new DuplicateItem();

										item.ShaCode = GetHash(file, out flen);
										item.PathToFile = file;
										item.Size = flen;
										blockColl.Add(item);
									}
								}
								catch (UnauthorizedAccessException) { }
								catch (IOException) { }
								catch (InvalidOperationException) { }
							});
						blockColl.CompleteAdding();
					}), TaskCreationOptions.LongRunning);

				mainTask.ContinueWith(prevTask =>
					{
						IsBusy = false;
						prevTask.Dispose();
						dg1.ItemsSource = new ObservableCollection<DuplicateItem>(blockColl.AsEnumerable());
						blockColl.Dispose();
						blockColl = null;
					}, System.Threading.CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext());

				mainTask.ContinueWith(prevTask =>
				{
					IsBusy = false;
					dg1.ItemsSource = null;
					prevTask.Dispose();
					blockColl.Dispose();
					blockColl = null;
				}, System.Threading.CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext());
			}
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
