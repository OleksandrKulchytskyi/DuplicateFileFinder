using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DuplicateFileFounder
{
	internal class DuplicateItem
	{
		[Description("File path")]
		public string PathToFile { get; set; }
		
		private float myVar;
		[Description("Size (Mb)")]
		public float Size
		{
			get { return myVar; }
			set {
				if (value > 0)
					myVar = (float)((value / 1024.0) / 1024.0);
			}
		}
		
		[Description("SHA code")]
		public string ShaCode { get; set; }
	}
}
