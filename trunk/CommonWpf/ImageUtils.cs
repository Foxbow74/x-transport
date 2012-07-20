using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClientCommonWpf
{
	public static class ImageUtils
	{
		public static BitmapSource Source(this Bitmap _bmp)
		{
			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(_bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}

		public static BitmapSource SourceDisabled(this Bitmap _bmp)
		{
			var orgBmp = _bmp.Source();
			if (orgBmp.Format == PixelFormats.Bgra32)
			{
				var orgPixels = new byte[orgBmp.PixelHeight * orgBmp.PixelWidth * 4];
				var newPixels = new byte[orgPixels.Length];
				orgBmp.CopyPixels(orgPixels, orgBmp.PixelWidth * 4, 0);
				for (var i = 3; i < orgPixels.Length; i += 4)
				{
					var grayVal = (orgPixels[i - 3] + orgPixels[i - 2] + orgPixels[i - 1]) / 6 + 128; 

					//if (grayVal != 0) grayVal = grayVal / 3;
					newPixels[i] = (byte)(orgPixels[i]);
					newPixels[i - 3] = (byte)grayVal;
					newPixels[i - 2] = (byte)grayVal;
					newPixels[i - 1] = (byte)grayVal;
				}
				return BitmapSource.Create(orgBmp.PixelWidth, orgBmp.PixelHeight,
					96, 96, PixelFormats.Bgra32, null, newPixels,
					orgBmp.PixelWidth * 4);
			}
			return orgBmp;
		}
	}
}