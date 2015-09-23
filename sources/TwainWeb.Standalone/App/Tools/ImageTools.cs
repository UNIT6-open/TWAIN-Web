using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using TwainWeb.Standalone.App.Models;
using TwainWeb.Standalone.App.Models.Request;
using Encoder = System.Drawing.Imaging.Encoder;

namespace TwainWeb.Standalone.App.Tools
{
	public static class ImageTools
	{
		public static string CreateFilename(string prefix, string counter, bool isPackage, GlobalDictionaries.SaveAsValues saveAs, ImageFormat imgFormat)
		{
			var sb = new StringBuilder();
			sb.Append(prefix);
			sb.Append((String.IsNullOrEmpty(counter) ? "" : ("_" + counter)));
			sb.Append(".");
			sb.Append((!isPackage || saveAs == GlobalDictionaries.SaveAsValues.Pictures
					? GlobalDictionaries.SearchingKeyInImgFormats(imgFormat)
					: GlobalDictionaries.DefaultFileFormats[saveAs]));

			return sb.ToString();
		}

		public static void CompressAndSaveImage(Image image, string filename, CompressionFormat compressionFormat)
		{
			var jgpEncoder = GetEncoder(compressionFormat.ImgFormat);
			var myEncoder = Encoder.Quality;
			var parameters = new EncoderParameters(1);
			parameters.Param[0] = new EncoderParameter(myEncoder, compressionFormat.Compression);

			image.Save(filename, jgpEncoder, parameters);			
		}

		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			var codecs = ImageCodecInfo.GetImageDecoders();
			foreach (var codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}
	}
}
