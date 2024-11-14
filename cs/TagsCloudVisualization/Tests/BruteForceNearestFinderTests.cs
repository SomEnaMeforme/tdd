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

        [TestCase(4, 10, Direction.Top)]
        [TestCase(2, 7, Direction.Top, true)]
        [TestCase(2, 7, Direction.Right)]
        public void FindNearest_ShouldReturnNearestRectangleByDirection_ForArgumentRectangle(int x, int y, Direction direction, bool isFirstNearest = false)
        {
            var addedRectangle1 = new Rectangle(new Point(2, 2), new Size(3, 4));
            var addedRectangle2 = new Rectangle(new Point(5, 7), new Size(4, 2));
            var rectangleForFind = new Rectangle(new Point(x, y), new Size(2, 1));

            finder.Insert(addedRectangle1);
            finder.Insert(addedRectangle2);

            finder.FindNearestByDirection(rectangleForFind, direction).Should().Be(isFirstNearest ? addedRectangle1 : addedRectangle2);
        }
    }
}
