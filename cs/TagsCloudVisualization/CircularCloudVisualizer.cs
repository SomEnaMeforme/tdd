using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TagsCloudVisualization
{
    public class CircularCloudVisualizer
    {
        private readonly Color backgroundColor = Color.White;
        private readonly Color rectangleColor = Color.DarkBlue;
        private readonly Size imageSize;
        private readonly RectangleStorage rectangleStorage;

        public CircularCloudVisualizer(RectangleStorage rectangles, Size imageSize)
        {
            rectangleStorage = rectangles;
            this.imageSize = imageSize;
        }

        public void CreateImage(string? filePath = null, bool withSaveSteps = false)
        {
            var rectangles = NormalizeSizes(rectangleStorage.GetAll());

            using var image = new Bitmap(imageSize.Width, imageSize.Height);
            using var graphics = Graphics.FromImage(image);
            graphics.Clear(backgroundColor);
            var pen = new Pen(rectangleColor);
            
            for (var i = 0; i < rectangles.Length; i++)
            {
                var nextRectangle = rectangles[i];
                graphics.DrawRectangle(pen, nextRectangle);
                if (withSaveSteps)
                {
                    SaveImage(image, $"{filePath}Step{1}");
                }
            }
            SaveImage(image, filePath);
        }

        private void SaveImage(Bitmap image, string? filePath = null)
        {
            var rnd = new Random();
            filePath ??= Path.Combine(Path.GetTempPath(), $"testImage{rnd.Next()}.png");
            image.Save(filePath, ImageFormat.Png);
        }

        private Rectangle[] NormalizeSizes(IEnumerable<Rectangle> source)
        {
            var sourceToArray = source.ToArray();

            var xLength = sourceToArray.Max(r => r.Right) - sourceToArray.Min(r => r.Left);
            var yLength = sourceToArray.Max(r => r.Bottom) - sourceToArray.Min(r => r.Top);

            var factorX = GetNormalizeFactorByAxis(imageSize.Width, xLength);
            var factorY = GetNormalizeFactorByAxis(imageSize.Height, yLength);

            return sourceToArray.Select(r => new Rectangle(
                    new Point(r.X * factorX, r.Y * factorY),
                    new Size(r.Width * factorX, r.Height * factorY)))
                .ToArray();
        }

        private static int GetNormalizeFactorByAxis(int imageSizeOnAxis, int rectanglesSizeOnAxis)
        {
            const int boundShift = 10;
            double imageSizeOnAxisWithShiftForBounds = imageSizeOnAxis - boundShift;
            var stretchFactor = (int)Math.Floor(imageSizeOnAxisWithShiftForBounds / rectanglesSizeOnAxis);
            return imageSizeOnAxisWithShiftForBounds > rectanglesSizeOnAxis ? stretchFactor : 1;
        }
    }
}
