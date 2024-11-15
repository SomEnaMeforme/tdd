using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using System.Drawing;
using System.Linq;
using static TagsCloudVisualization.CircleLayer;

namespace TagsCloudVisualization.Tests
{
    public class CircleLayerTests
    {
        private CircleLayer currentLayer;
        private Size defaultRectangleSize;
        private RectangleStorage storage;
        [SetUp]
        public void SetUp()
        {
            var startRadius = 5;
            var center = new Point(5, 5);
            storage = new RectangleStorage();
            currentLayer = new CircleLayer(center, startRadius, storage);
            defaultRectangleSize = new Size(3, 4);
        }

        [Test]
        public void CircleLayer_InsertFirstForLayerRectangle_InTopRightSectorStart()
        {
            var possibleRectangleLocation = currentLayer.CalculateTopLeftRectangleCornerPosition(defaultRectangleSize);

            possibleRectangleLocation.Should().Be(GetCorrectRectangleLocationByExpectedSector(Sector.Top_Right, defaultRectangleSize));
        }

        [TestCase(1, Sector.Bottom_Right)]
        [TestCase(2, Sector.Bottom_Left)]
        [TestCase(3, Sector.Top_Left)]
        public void CircleLayer_InsertRectangleInNextSector_AfterSuccessInsertion(int insertionsCount, Sector expected)
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, insertionsCount);

            var possibleRectangleLocation = currentLayer.CalculateTopLeftRectangleCornerPosition(defaultRectangleSize);

            possibleRectangleLocation.Should().Be(GetCorrectRectangleLocationByExpectedSector(expected, defaultRectangleSize));
        }
        

        [Test]
        public void CircleLayer_RadiusNextCircleLayer_ShouldBeIntMinDistanceFromCenterToInsertedRectangles()
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);
            var nextRectangleLocation = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(4), defaultRectangleSize);
            var insertedRectangleId = storage.Add(new Rectangle(nextRectangleLocation, new Size(2, 2)));

            currentLayer.OnSuccessInsertRectangle(insertedRectangleId);

            currentLayer.Radius.Should().Be(9);
        }

        private CircleLayer GetLayerAfterFewInsertionsRectangleWithSameSize(CircleLayer layer, int additionsCount)
        {
            layer = GetLayerAfterFewInsertionsRectangle(layer, additionsCount,
                new Size[additionsCount].Select(x => defaultRectangleSize).ToArray());
            return layer;
        }

        private Sector GetSectorByInsertionsCount(int count)
        {
            return (Sector)((count - 1) % 4);
        }

        private Point GetCorrectRectangleLocationByExpectedSector(Sector s, Size size)
        {
            switch (s)
            {
                case Sector.Top_Right: 
                    return new Point(currentLayer.Center.X, currentLayer.Center.Y - currentLayer.Radius - size.Height);
                case Sector.Bottom_Right: 
                    return new Point(currentLayer.Center.X + currentLayer.Radius, currentLayer.Center.Y);
                case Sector.Bottom_Left: 
                    return new Point(currentLayer.Center.X - size.Width, currentLayer.Center.Y + currentLayer.Radius);
                default: 
                    return new Point(currentLayer.Center.X - currentLayer.Radius - size.Width,
                    currentLayer.Center.Y - size.Height);
            }
        }

        [Test]
        public void CircleLayer_RectangleWithNewPositionAfterIntersection_ShouldNotIntersectSameRectangle()
        {
            var rectangleForInsertion = new Rectangle(new Point(5, -1), new Size(5, 1));
            var intersectedRectangle = new Rectangle(new Point(8, -6), new Size(8, 7));

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);

            new Rectangle(newPosition, rectangleForInsertion.Size).IntersectsWith(intersectedRectangle).Should()
                .BeFalse();
        }

        [Test]
        public void GetPositionWithoutIntersection_ShouldPlaceBottomLeftCornerOnCircle_WhenFoundIntersectionInTopRightSector()
        {
            var rectangleForInsertion = new Rectangle(new Point(5, -1), new Size(5, 1));
            var intersectedRectangle = new Rectangle(new Point(8, -6), new Size(8, 7));

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);
            var bottomLeftCorner = new Point(newPosition.X, newPosition.Y + intersectedRectangle.Height);

            CurrentLayerContainsPoint(bottomLeftCorner).Should().BeTrue();
        }

        public static IEnumerable<TestCaseData> SimpleIntersectionInSector
        {
            get
            {
                yield return new TestCaseData(
                    new Rectangle(new Point(5, -1), new Size(5, 1)), 
                    new Rectangle(new Point(8, -6), new Size(8, 7)),
                    new Point(9, 1), 0).SetName("WhenFoundIntersectionInTopRightSector");
                yield return new TestCaseData(
                    new Rectangle(new Point(8, 9), new Size(5, 1)),
                    new Rectangle(new Point(10, 5), new Size(8, 7)),
                    new Point(5, 10), 1).SetName("WhenFoundIntersectionInBottomRightSector");
                yield return new TestCaseData(
                    new Rectangle(new Point(-3, 9), new Size(5, 3)),
                    new Rectangle(new Point(-7, 8), new Size(8, 7)),
                    new Point(-5, 5), 2).SetName("WhenFoundIntersectionInBottomLeftSector");
                yield return new TestCaseData(
                    new Rectangle(new(-3, -2), new(4, 3)),
                    new Rectangle(new(-7, 1), new(8, 7)),
                    new Point(1, -3), 3).SetName("WhenFoundIntersectionInTopLeftSector");
            }
        }

        [TestCaseSource("SimpleIntersectionInSector")]
        public void GetPositionWithoutIntersection_ReturnCorrectRectanglePosition(Rectangle forInsertion, Rectangle intersected, Point expected, int additionsCount)
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, additionsCount);

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(forInsertion, intersected);

            newPosition.Should().Be(expected);
        }

        private bool CurrentLayerContainsPoint(Point p)
        {
            return (p.X - currentLayer.Center.X) * (p.X - currentLayer.Center.X) +
                (p.Y - currentLayer.Center.Y) * (p.Y - currentLayer.Center.Y) == currentLayer.Radius * currentLayer.Radius;
        }

        [Test]
        public void CircleLayer_RadiusNextCircleLayer_ShouldBeCeilingToInt()
        {
            var sizes = new Size[]
            {
                new (8,1), new(7,8), new (4,4), new (4,4), new(4,4)
            };
            var nextLayer = GetLayerAfterFewInsertionsRectangle(currentLayer, sizes.Length, sizes);

            nextLayer.Radius.Should().Be(10);
        }

        public static IEnumerable<TestCaseData> GetDataForIntersectionTests
        {
            get
            {
                yield return new TestCaseData(new Size[]
                        { new(1, 1), new(5, 8), new(4, 4), new(4, 4), new(4, 4) }, 
                        new Rectangle(new(11, 5), new(6, 6)),
                        new Rectangle(new(10, 5), new(5, 8)), 
                        new Point(-1, 12)).SetName("ChangeCornerPositionForSector_WhenMoveRectangleClockwise");
                yield return new TestCaseData(new Size[]
                            { new (1,1), new(1,8), new (4,2), new (4, 9), 
                                new(1,1), new(1,1), new(1,1)},
                            new Rectangle(new(-10, 2), new(8, 3)),
                            new Rectangle(new(-4, -4), new (4, 9)),
                            new Point(5, -7)).SetName("CreateNewCircle_IfNeedMoveRectangleFromLastSector");
                yield return new TestCaseData(new Size[]
                    {  new (1,1), new(1,8), new (50, 50), new(1,1), new(1,1), new(1,1)},
                    new Rectangle(new(4, 10), new(1, 1)),
                    new Rectangle(new(-50, 10), new(50, 50)),
                    new Point(-2, 9)).SetName("GetCorrectPosition_WhenRectanglesSidesMatch");

            }
        }

        [TestCaseSource("GetDataForIntersectionTests")]
        public void GetPositionWithoutIntersection_Should(Size[] sizes, Rectangle forInsertion, Rectangle intersected, Point expected)
        {
            var fullLayer = GetLayerAfterFewInsertionsRectangle(currentLayer, sizes.Length, sizes);

            var newPosition = fullLayer.GetRectanglePositionWithoutIntersection(forInsertion, intersected);

            newPosition.Should().Be(expected);
        }
        
        private CircleLayer GetLayerAfterFewInsertionsRectangle(CircleLayer layer, int insertionsCount, Size[] sizes)
        {
            for (var i = 1; i <= insertionsCount; i++)
            {
                var location = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(i), sizes[i - 1]);
                var rectangleForInsert = new Rectangle(location, sizes[i - 1]);
                layer.OnSuccessInsertRectangle(storage.Add(rectangleForInsert));
            }
            return layer;
        }

    }
}
