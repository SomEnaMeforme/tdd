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
                var intersected = GetRectangleIntersection(resultRectangle);
                while (intersected != new Rectangle())
                {
                    possiblePosition =
                        CurrentLayer.GetRectanglePositionWithoutIntersection(resultRectangle, intersected.Value);
                    resultRectangle = new Rectangle(possiblePosition, rectangleSize);
                    intersected = GetRectangleIntersection(resultRectangle);
                }
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

        private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
        {
            return rectanglesLocation
                .FirstOrDefault(forInsertion.IntersectsWith);
        }

        private Rectangle?[] GetNearestByAllDirectionsFor(Rectangle r)
        {
            return new []
            {
                nearestFinder.FindNearestByDirection(r, Direction.Bottom),
                nearestFinder.FindNearestByDirection(r, Direction.Top),
                nearestFinder.FindNearestByDirection(r, Direction.Left),
                nearestFinder.FindNearestByDirection(r, Direction.Right)
            };
        }

        private void CreateFirstLayer(Size firstRectangle)
        {
            var radius = Math.Ceiling(Math.Sqrt(firstRectangle.Width* firstRectangle.Width + firstRectangle.Height* firstRectangle.Height) / 2.0);
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
