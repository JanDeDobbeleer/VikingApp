using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;

namespace UpdateLiveTile.Classes
{
    public enum Tile
    {
        Front,
        Back,
        Small
    }

    class Helper
    {
        public static bool SaveElement(UIElement element, Tile tile)
        {
            try
            {
                var side = (tile == Tile.Small) ? 159 : 336;
                element.Measure(new Size(side, side));
                element.Arrange(new Rect(0, 0, side, side));
                var bmp = new WriteableBitmap(side, side);
                bmp.Render(element, null);
                bmp.Invalidate();
                var name = tile.ToString() + Guid.NewGuid() + ".png";
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isf.DirectoryExists("/CustomLiveTiles"))
                    {
                        isf.CreateDirectory("/CustomLiveTiles");
                    }
                    using (var myFileStream = isf.CreateFile("shared/shellcontent/" + name))
                    {
                        // Encode WriteableBitmap object to a PNG stream.
                        bmp.WritePng(myFileStream);
                    }
                    var filesTodelete =
                             from f in isf.GetFileNames("shared/shellcontent/" + tile + "*").AsQueryable()
                             where !f.EndsWith(name)
                             select f;
                    foreach (var file in filesTodelete)
                    {
                        isf.DeleteFile("shared/shellcontent/" + file);
                    }
                }
                SaveTilePart("isostore:/Shared/ShellContent/" + name, tile);
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
                if (tile == Tile.Small)
                {
                    SaveTilePart(GetDefaultSmallTile(), tile);
                }
                else
                {
                    SaveTilePart((tile == Tile.Front) ? GetDefaultFrontTile() : GetDefaultBackTile(), tile);
                }
                return false;
            }
            return true;
        }

        private static string GetDefaultBackTile()
        {
            return (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                ? "/Assets/336x336empty.png"
                : "/Assets/336x336redempty.png";
        }

        private static string GetDefaultFrontTile()
        {
            return (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                ? "/Assets/336x336.png"
                : "/Assets/336x336red.png";
        }

        private static string GetDefaultSmallTile()
        {
            return (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                ? "/Assets/159x159.png"
                : "/Assets/159x159red.png";
        }

        private static void SaveTilePart(string filename, Tile tile)
        {
            var newTile = new FlipTileData
            {
                Count = 0,
                BackContent = string.Empty,
                Title = string.Empty,
                BackTitle = string.Empty
            };
            switch (tile)
            {
                case Tile.Front:
                    newTile.BackgroundImage = new Uri(filename, UriKind.Absolute);
                    break;
                case Tile.Back:
                    newTile.BackBackgroundImage = new Uri(filename, UriKind.Absolute);
                    break;
                case Tile.Small:
                    newTile.SmallBackgroundImage = new Uri(filename, UriKind.Absolute);
                    break;
            }
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }
    }
}
