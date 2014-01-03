using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UpdateLiveTile.Classes
{
    class Helper
    {
        public static bool SaveElement(UIElement element, string path)
        {
            try
            {
                element.Measure(new Size(336, 336));
                element.Arrange(new Rect(0, 0, 336, 336));
                var bmp = new WriteableBitmap(336, 336);
                bmp.Render(element, null);
                bmp.Invalidate();

                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isf.DirectoryExists("/CustomLiveTiles"))
                    {
                        isf.CreateDirectory("/CustomLiveTiles");
                    }
                    if(isf.FileExists(path))
                        isf.DeleteFile(path);
                    using (var stream = isf.OpenFile(path, FileMode.Create))
                    {
                        bmp.SaveJpeg(stream, 336, 366, 0, 100);
                    }
                }
            }
            catch (Exception)
            {
                Thread.Sleep(500);
                return false;
            }
            return true;
        }
    }
}
