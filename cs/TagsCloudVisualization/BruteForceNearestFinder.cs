using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace TagsCloudVisualization
{
    public class BruteForceNearestFinder
    {
        private List<Rectangle> rectangles = new();

        public void Insert(Rectangle r)
        {
            if (RectangleHasInсorrectSize(r))
                throw new ArgumentException($"Rectangle has incorrect size: width = {r.Width}, height = {r.Height}");
            rectangles.Add(r);
        }

        public Rectangle? FindNearestByDirection(Rectangle r, Direction direction)
        {
            if (RectangleHasInсorrectSize(r))
                throw new ArgumentException($"Rectangle has incorrect size: width= {r.Width}, height={r.Height}");
            if (rectangles.Count == 0)
                return null;
            var calculator = GetMinDistanceCalculatorBy(direction);
            return rectangles.Select(currentRectangle => (distance: calculator(currentRectangle, r), CurrentEl: currentRectangle))
                .Where(el => el.distance > 0)
                .MinBy(el => el.distance).CurrentEl;
        }

        private Func<Rectangle, Rectangle, int> GetMinDistanceCalculatorBy(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left: return (possibleNearest, rectangleForFind) => rectangleForFind.X - possibleNearest.X;
                case Direction.Right: return (possibleNearest, rectangleForFind) => possibleNearest.X - rectangleForFind.X;
                case Direction.Top: return (possibleNearest, rectangleForFind) => rectangleForFind.Y - possibleNearest.Y;
                default: return (possibleNearest, rectangleForFind) => possibleNearest.Y - rectangleForFind.Y;
            }
        }

        private bool RectangleHasInсorrectSize(Rectangle r)
        {
            return r.Width <= 0 || r.Height <= 0;
        }
    }
}
