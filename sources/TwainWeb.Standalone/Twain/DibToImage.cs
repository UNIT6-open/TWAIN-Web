using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;


namespace TwainWeb.Standalone.Twain
{

    internal sealed class DibToImage {

        
        /// <summary>
        /// Get .NET 'Bitmap' object from memory DIB via 'scan0' constructor.
        /// </summary>
        /// <param name="dibPtr">Pointer to memory DIB, starting with BITMAPINFOHEADER.</param>
        public static Bitmap WithScan(IntPtr dibPtr) {
            BITMAPINFOHEADER _bmi=(BITMAPINFOHEADER)Marshal.PtrToStructure(dibPtr,typeof(BITMAPINFOHEADER));
            if(_bmi.biCompression!=0) {
                throw new ArgumentException("Invalid bitmap format (non-RGB)","BITMAPINFOHEADER.biCompression");
            }

            PixelFormat _fmt=PixelFormat.Undefined;
            switch(_bmi.biBitCount) {
                case 32:
                    _fmt=PixelFormat.Format32bppRgb;
                    break;
                case 24:
                    _fmt=PixelFormat.Format24bppRgb;
                    break;
                case 16:
                    _fmt=PixelFormat.Format16bppRgb555;
                    break;
                case 8:
                    _fmt=PixelFormat.Format8bppIndexed;
                    break;
                case 4:
                    _fmt=PixelFormat.Format4bppIndexed;
                    break;
                case 1:
                    _fmt=PixelFormat.Format1bppIndexed;
                    break;
            }

            int _scan0=((int)dibPtr)+_bmi.biSize+(_bmi.biClrUsed*4);	// pointer to pixels
            int _stride=(((_bmi.biWidth*_bmi.biBitCount)+31)&~31)>>3;	// bytes/line
            if(_bmi.biHeight>0) {									// bottom-up
                _scan0+=_stride*(_bmi.biHeight-1);
                _stride=-_stride;
            }
            using(Bitmap _tmp_bitmap=new Bitmap(_bmi.biWidth,Math.Abs(_bmi.biHeight),_stride,_fmt,(IntPtr)_scan0)) {// '_tmp' is wired to scan0 (unfortunately)
                if(_tmp_bitmap.Palette.Entries.Length>0) {
                    ColorPalette _palette=_tmp_bitmap.Palette;
                    for(int i=0,_ptr=dibPtr.ToInt32()+_bmi.biSize;i<_palette.Entries.Length;i++,_ptr+=4) {
                        _palette.Entries[i]=((RGBQUAD)Marshal.PtrToStructure((IntPtr)_ptr,typeof(RGBQUAD))).ToColor();
                    }
                    _tmp_bitmap.Palette=_palette;
                }

                return new Bitmap(_tmp_bitmap); // 'result' is a copy (stand-alone)
            }
        }

        
        [StructLayout(LayoutKind.Sequential,Pack=2)]
        private class BITMAPINFOHEADER {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential,Pack=1)]
        private class RGBQUAD {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Reserved;

            public Color ToColor() {
                return Color.FromArgb(this.Red,this.Green,this.Blue);
            }
        }    
    }
}
