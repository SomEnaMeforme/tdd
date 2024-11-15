using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TagsCloudVisualization
{
    public class CircularCloudLayouter
    {
        private readonly RectangleStorage storage = new ();
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
            ValidateRectangleSize(rectangleSize);
            Point firstRectanglePosition;
            var isFirstRectangle = IsFirstRectangle();
            if (isFirstRectangle)
            {
                CreateFirstLayer(rectangleSize);
                firstRectanglePosition = PutRectangleToCenter(rectangleSize);
            }
            else
            {
                firstRectanglePosition = CurrentLayer.CalculateTopLeftRectangleCornerPosition(rectangleSize);
            }
            var id = SaveRectangle(firstRectanglePosition, rectangleSize);
            var rectangleWithOptimalPosition = OptimiseRectanglePosition(id, isFirstRectangle);
            return rectangleWithOptimalPosition;
        }

        public Rectangle OptimiseRectanglePosition(int id, bool isFirstRectangle)
        {
            if (isFirstRectangle) return storage.GetById(id);
            return PutRectangleOnCircleWithoutIntersection(id);
        }

        private int SaveRectangle(Point firstLocation, Size rectangleSize)
        {
            var id = storage.Add(new Rectangle(firstLocation, rectangleSize));
            return id;
        }

        public Rectangle PutRectangleOnCircleWithoutIntersection(int id)
        {
            var r = storage.GetById(id);
            var intersected = GetRectangleIntersection(r);
            while (intersected != new Rectangle())
            {
                var possiblePosition =
                    CurrentLayer.GetRectanglePositionWithoutIntersection(r, intersected.Value);
                r = new Rectangle(possiblePosition, r.Size);
                intersected = GetRectangleIntersection(r);
            }
            CurrentLayer.OnSuccessInsertRectangle(id);
            return r;
        }

        private void ValidateRectangleSize(Size s)
        {
            if (s.Width <= 0 || s.Height <= 0)
            {
                throw new ArgumentException($"Rectangle has incorrect size: width = {s.Width}, height = {s.Height}");
            }
        }

        private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
        {
            return storage.GetAll()
                .FirstOrDefault(r => forInsertion.IntersectsWith(r) && forInsertion != r);
        }

        private Rectangle?[] GetNearestByAllDirectionsFor(Rectangle r)
        {
            var rectangles = this.storage.GetAll();
            return new []
            {
                nearestFinder.FindNearestByDirection(r, Direction.Bottom, rectangles),
                nearestFinder.FindNearestByDirection(r, Direction.Top, rectangles),
                nearestFinder.FindNearestByDirection(r, Direction.Left, rectangles),
                nearestFinder.FindNearestByDirection(r, Direction.Right, rectangles)
            };
        }

        private void CreateFirstLayer(Size firstRectangle)
        {
            var radius = Math.Ceiling(Math.Sqrt(firstRectangle.Width* firstRectangle.Width + firstRectangle.Height* firstRectangle.Height) / 2.0);
            CurrentLayer = new CircleLayer(center, (int)radius, storage);
        }

        private Point PutRectangleToCenter(Size rectangleSize)
        {
            var rectangleX = center.X - rectangleSize.Width / 2;
            var rectangleY = center.Y - rectangleSize.Height / 2;

            return new Point(rectangleX, rectangleY);
        }
        
        private bool IsFirstRectangle()
        {
            return storage.GetAll().FirstOrDefault() == default;
        }

        public IEnumerable<Rectangle> GetRectangles()
        {
            foreach (var rectangle in storage.GetAll())
            {
                yield return rectangle;
            }
        }
    }
}
