using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TagsCloudVisualization
{
    public static class BruteForceNearestFinder
    {
        public static Rectangle? FindNearestByDirection(Rectangle r, Direction direction, List<Rectangle> rectangles)
        {
            if (rectangles.FirstOrDefault() == default)
                return null;
            var calculator = GetMinDistanceCalculatorBy(direction);
            var nearestByDirection = rectangles
                .Select(possibleNearest => (Distance: calculator(possibleNearest, r), Nearest: possibleNearest ))
                .Where(el => el.Distance >= 0)
                .ToList();

            return nearestByDirection.Count > 0 ? nearestByDirection.MinBy(el => el.Distance).Nearest : null;
        }

        public static Func<Rectangle, Rectangle, int> GetMinDistanceCalculatorBy(Direction direction)
        {
            return direction switch
            {
                Direction.Left => (possibleNearest, rectangleForFind) => rectangleForFind.Left - possibleNearest.Right,
                Direction.Right => (possibleNearest, rectangleForFind) => possibleNearest.Left - rectangleForFind.Right,
                Direction.Top => (possibleNearest, rectangleForFind) => rectangleForFind.Top - possibleNearest.Bottom,
                _ => (possibleNearest, rectangleForFind) => possibleNearest.Top - rectangleForFind.Bottom,
            };
        }
    }
}
