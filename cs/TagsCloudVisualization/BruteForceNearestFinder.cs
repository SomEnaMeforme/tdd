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
            rectangles.Add(r);
        }

        public Rectangle? FindNearestByDirection(Rectangle r, Direction direction)
        {
            if (rectangles.Count == 0)
                return null;
            var calculator = GetMinDistanceCalculatorBy(direction);
            var nearestByDirection = rectangles
                .Select(possibleNearest => (Distance: calculator(possibleNearest, r), CurrentEl: possibleNearest))
                .Where(el => el.Distance >= 0).ToList();

            return nearestByDirection.Count > 0 ? nearestByDirection.MinBy(el => el.Distance).CurrentEl : null;
        }

        private Func<Rectangle, Rectangle, int> GetMinDistanceCalculatorBy(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left: return (possibleNearest, rectangleForFind) => possibleNearest.Right - rectangleForFind.Left;
                case Direction.Right: return (possibleNearest, rectangleForFind) => possibleNearest.Left - rectangleForFind.Right;
                case Direction.Top: return (possibleNearest, rectangleForFind) => rectangleForFind.Top - possibleNearest.Bottom;
                default: return (possibleNearest, rectangleForFind) => possibleNearest.Top - rectangleForFind.Bottom;
            }
        }
    }
}
