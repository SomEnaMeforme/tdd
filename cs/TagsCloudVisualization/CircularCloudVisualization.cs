using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TagsCloudVisualization
{
    public class CircularCloudVisualization
    {
        private RectangleStorage rectangleStorage;
        private const string FILE_PATH = "./Images/";

        public CircularCloudVisualization(RectangleStorage rectangles)
        {
            rectangleStorage = rectangles;
        }

        public void CreateImage()
        {
            var fileName = Path.Combine(Path.GetTempPath(), "testImage1010.png");
            using (var image = new Bitmap(1000, 1000))
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    Pen pen = new Pen(Color.Black);
                    graphics.DrawRectangles(pen, rectangleStorage.GetAll().ToArray());
                }
                image.Save(fileName, ImageFormat.Png);
            }
        }

        private void CreateFile(string filePath)
        {

        }
    }
}
