using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsCloudVisualization
{
    public class CircularCloudLayouter
    {
        private List<Rectangle> rectanglesLocation = new ();
        private readonly Point center;
        private BruteForceNearestFinder nearestFinder;

        public CircleLayer CurrentLayer { get; private set; }

        public CircularCloudLayouter(Point center)
        {
            this.center = center;
            nearestFinder = new BruteForceNearestFinder();
        }
        public Rectangle PutNextRectangle(Size rectangleSize)
        {
            Rectangle resultRectangle;
            if (IsFirstRectangle())
            {
                CreateFirstLayer(rectangleSize);
                resultRectangle = PutRectangleToCenter(rectangleSize);
            }
            else
            {
                var possiblePosition = CurrentLayer.CalculateTopLeftRectangleCornerPosition(rectangleSize);
                resultRectangle = new Rectangle(possiblePosition, rectangleSize);
            }
            OnSuccessInsertion(resultRectangle);
            return resultRectangle;
        }

        private void OnSuccessInsertion(Rectangle r)
        {
            rectanglesLocation.Add(r);
            nearestFinder.Insert(r);
            if (IsNotFirstInsertion())
                CurrentLayer.OnSuccessInsertRectangle(r);
        }

        private void CreateFirstLayer(Size firstRectangle)
        {
            var radius = Math.Ceiling(Math.Max(firstRectangle.Width, firstRectangle.Height) / 2.0);
            CurrentLayer = new CircleLayer(center, (int)radius);
        }

        private Rectangle PutRectangleToCenter(Size rectangleSize)
        {
            var rectangleX = center.X - rectangleSize.Width / 2;
            var rectangleY = center.Y - rectangleSize.Height / 2;

            return new Rectangle(new Point(rectangleX, rectangleY), rectangleSize);
        }
        
        private bool IsFirstRectangle()
        {
            return rectanglesLocation.Count == 0;
        }

        private bool IsNotFirstInsertion()
        {
            return rectanglesLocation.Count > 1;
        }

        public IEnumerable<Rectangle> GetRectangles()
        {
            foreach (var rectangle in rectanglesLocation)
            {
                yield return rectangle;
            }
        }
    }
}
