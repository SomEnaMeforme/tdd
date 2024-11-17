using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static TagsCloudVisualization.CircleLayer;

namespace TagsCloudVisualization
{
    public class CircularCloudVisualization
    {
        private readonly Color BACKGROUND_COLOR = Color.White;
        private readonly Color RECTANGLE_COLOR = Color.DarkBlue;
        private readonly Size ImageSize;
        private RectangleStorage rectangleStorage;

        public CircularCloudVisualization(RectangleStorage rectangles, Size size)
        {
            rectangleStorage = rectangles;
            ImageSize = size;
        }

        public void CreateImage()
        {
            var filePath = Path.Combine(Path.GetTempPath(), "testImage1010.png");
            using (var image = new Bitmap(ImageSize.Width, ImageSize.Height))
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(BACKGROUND_COLOR);
                    DrawGrid(graphics);
                    Pen pen = new Pen(RECTANGLE_COLOR);
                    var rectangles = rectangleStorage.GetAll();
                    graphics.DrawRectangle(new Pen(Color.Brown),rectangles.First());
                    graphics.DrawRectangles(pen, rectangles.Skip(1).ToArray());
                }
                image.Save(filePath, ImageFormat.Png);
            }
        }

        private void DrawGrid(Graphics g, int cellsCount = 100, int cellSize = 10)
        {
            Pen p = new Pen(Color.DarkGray);

            for (int y = 0; y < cellsCount; ++y)
            {
                g.DrawLine(p, 0, y * cellSize, cellsCount * cellSize, y * cellSize);
            }

            for (int x = 0; x < cellsCount; ++x)
            {
                g.DrawLine(p, x * cellSize, 0, x * cellSize, cellsCount * cellSize);
            }
        }

        public void CreateImageWithSaveEveryStep(CircularCloudLayouter layouter, Size[] sizes)
        {
            var startName = "testImageStep";
            var extension = ".png";
            using (var image = new Bitmap(ImageSize.Width, ImageSize.Height))
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(BACKGROUND_COLOR);
                    DrawGrid(graphics);
                    Pen pen = new Pen(RECTANGLE_COLOR);
                    for (var i = 0; i < sizes.Length; i++)
                    {
                        var r = layouter.PutNextRectangle(sizes[i]);
                        var currentFileName = $"{startName}{i}{extension}";
                        graphics.DrawRectangle(new Pen(GetColorBySector(layouter.CurrentLayer.CurrentSector)), r);
                        var filePath = Path.Combine(Path.GetTempPath(), currentFileName);
                        image.Save(filePath, ImageFormat.Png);
                    }
                }

            }
        }

        private Color GetColorBySector(CircleLayer.Sector s)
        {
            switch (s)
            {
                case Sector.Top_Right:
                    return Color.Chartreuse;
                case Sector.Bottom_Right:
                    return Color.Brown;
                case Sector.Bottom_Left:
                    return Color.DeepPink;
                default:
                    return Color.DodgerBlue;
            }
        }
    }
}
