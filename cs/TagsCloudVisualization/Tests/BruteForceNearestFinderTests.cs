using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Drawing;

namespace TagsCloudVisualization.Tests
{
    public class BruteForceNearestFinderTests
    {
        private BruteForceNearestFinder finder;
        [SetUp]
        public void SetUp()
        {
            finder = new BruteForceNearestFinder();
        }
        [Test]
        public void FindNearest_ShouldReturnNull_BeforeAnyInsertions()
        {
            var rectangleForFind = new Rectangle(new Point(5, 7), new Size(4, 2));

            finder.FindNearestByDirection(rectangleForFind, Direction.Top).Should().BeNull();
        }

        [TestCase(0, 4, TestName = "WhenWidthZero")]
        [TestCase(3, 0, TestName = "WhenHeightZero")]
        [TestCase(-3, 4, TestName = "WhenWidthIsNegative")]
        [TestCase(3, -4, TestName = "WhenHeightNegative")]
        [TestCase(-3, -4, TestName = "WhenWidthAndHeightNegative")]
        [TestCase(0, 0, TestName = "WhenWidthAndHeightIsZero")]
        public void Insert_ShouldThrow(int width, int height)
        {
            var rectangleForInsert = new Rectangle(new Point(2, 2), new Size(width, height));

            ShouldThrow((finder, rectangle) => finder.Insert(rectangle), rectangleForInsert);
        }

        [TestCase(0, 4, TestName = "WhenWidthZero")]
        [TestCase(3, 0, TestName = "WhenHeightZero")]
        [TestCase(-3, 4, TestName = "WhenWidthIsNegative")]
        [TestCase(3, -4, TestName = "WhenHeightNegative")]
        [TestCase(-3, -4, TestName = "WhenWidthAndHeightNegative")]
        [TestCase(0, 0, TestName = "WhenWidthAndHeightIsZero")]
        public void FindNearest_ShouldThrow(int width, int height)
        {
            var rectangleForFind = new Rectangle(new Point(2, 2), new Size(width, height));

            ShouldThrow((finder, rectangle) => finder.FindNearestByDirection(rectangle, Direction.Top), rectangleForFind);
        }

        public void ShouldThrow(Action<BruteForceNearestFinder, Rectangle> callFinderMethod, Rectangle incorrectRectangle)
        {
            Action act = () => callFinderMethod(finder, incorrectRectangle);

            act.Should().Throw<ArgumentException>();
        }

        [TestCase(4, 10, Direction.Top)]
        [TestCase(2, 7, Direction.Top, true)]
        [TestCase(2, 7, Direction.Right)]
        public void FindNearest_ShouldReturnNearestRectangleByDirection_ForArgumentRectangle(int x, int y, Direction direction, bool isFirstRectNearest = false)
        {
            var addedRectangle1 = new Rectangle(new Point(2, 2), new Size(3, 4));
            var addedRectangle2 = new Rectangle(new Point(5, 7), new Size(4, 2));
            var rectangleForFind = new Rectangle(new Point(x, y), new Size(2, 1));

            finder.Insert(addedRectangle1);
            finder.Insert(addedRectangle2);

            finder.FindNearestByDirection(rectangleForFind, direction).Should().Be(isFirstRectNearest ? addedRectangle1 : addedRectangle2);
        }
    }
}
