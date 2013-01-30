using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FileHelper.Common
{
	public static class StreamUtils
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetDiskFreeSpaceW")]
		private static extern bool GetDiskFreeSpace(string lpRootName, out int lpSectorsPerCluster, out int lpBytesPerSector,
													out int lpNiumberOfFreeClusters, out int lpTotalNumberOfClusters);

		public static int GetClusterSize(string path)
		{
			int sectorsPerCluster;
			int bytesPerSector;
			int freeClusters;
			int totalClusters;
			int clusterSize = 0;
			if (GetDiskFreeSpace(Path.GetPathRoot(path), out sectorsPerCluster, out bytesPerSector, out freeClusters, out totalClusters))
				clusterSize = bytesPerSector * sectorsPerCluster;
			return clusterSize;
		}

		public static void CopyStream(this Stream input, Stream output)
		{
			byte[] buffer = new byte[8 * 1024];
			int len;
			while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, len);
			}
		}

		public static void CopyStreamToFile(this Stream input, string fileName)
		{
			using (Stream file = File.OpenWrite(fileName))
			{
				input.CopyStream(file);
			}
		}
	}
}
