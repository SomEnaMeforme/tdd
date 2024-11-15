using FluentAssertions;
using NUnit.Framework;
using System.Drawing;
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
        public void CircleLayer_ShouldCreateNewLayer_AfterInsertionsOnAllSectors()
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);
            var insertedRectangleId = storage.Add(new Rectangle(new Point(0, 0), defaultRectangleSize));

            var nextLayer = currentLayer.OnSuccessInsertRectangle(insertedRectangleId);

            nextLayer.Should().NotBeSameAs(currentLayer);
        }

        [Test]
        public void CircleLayer_RadiusNextCircleLayer_ShouldBeIntMinDistanceFromCenterToInsertedRectangle()
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);
            var nextRectangleLocation = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(4), defaultRectangleSize);
            var insertedRectangleId = storage.Add(new Rectangle(nextRectangleLocation, new Size(2, 2)));

            var nextLayer = currentLayer.OnSuccessInsertRectangle(insertedRectangleId);

            nextLayer.Radius.Should().Be(9);
        }

        private CircleLayer GetLayerAfterFewInsertionsRectangleWithSameSize(CircleLayer layer, int insertionsCount)
        {
            layer = GetLayerAfterFewInsertionsRectangleWithDifferentSize(layer, insertionsCount,
                new Size[insertionsCount].Select(x => defaultRectangleSize).ToArray());
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
        public void GetPositionOnCircleWithoutIntersection_ShouldPlaceBottomLeftCornerOnCircle_WhenFoundIntersectionInTopRightSector_AndIntersectedRectangleCanPlaceWithIntCoordinate()
        {
            var rectangleForInsertion = new Rectangle(new Point(5, -1), new Size(5, 1));
            var intersectedRectangle = new Rectangle(new Point(8, -6), new Size(8, 7));

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);
            var bottomLeftCorner = new Point(newPosition.X, newPosition.Y + intersectedRectangle.Height);

            CurrentLayerContainsPoint(bottomLeftCorner).Should().BeTrue();
        }

        [Test]
        public void GetPositionOnCircleWithoutIntersection_ReturnCorrectRectanglePosition_WhenFoundIntersectionInTopRightSector()
        {
            var rectangleForInsertion = new Rectangle(new Point(5, -1), new Size(5, 1));
            var intersectedRectangle = new Rectangle(new Point(8, -6), new Size(8, 7));
            var expected = new Point(9, 1);

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);

            newPosition.Should().Be(expected);
        }


        [Test]
        public void GetPositionOnCircleWithoutIntersection_ReturnCorrectRectanglePosition_WhenFoundIntersectionInBottomRightSector()
        {
            var rectangleForInsertion = new Rectangle(new Point(8, 9), new Size(5, 1));
            var intersectedRectangle = new Rectangle(new Point(10, 5), new Size(8, 7));
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 1);
            var expected = new Point(5, 10);

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);

            newPosition.Should().Be(expected);
        }


        [Test]
        public void GetPositionOnCircleWithoutIntersection_ReturnCorrectRectanglePosition_WhenFoundIntersectionInBottomLeftSector()
        {
            var rectangleForInsertion = new Rectangle(new Point(-3, 9), new Size(5, 3));
            var intersectedRectangle = new Rectangle(new Point(-7, 8), new Size(8, 7));
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 2);
            var expected = new Point(-5, 5);

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);

            newPosition.Should().Be(expected);
        }

        [Test]
        public void GetPositionOnCircleWithoutIntersection_ReturnCorrectRectanglePosition_WhenFoundIntersectionInTopLeftSector()
        {
            var rectangleForInsertion = new Rectangle(new (-3, -2), new (4, 3));
            var intersectedRectangle = new Rectangle(new (-7, 1), new (8, 7));
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);
            var expected = new Point(1, -3);

            var newPosition = currentLayer.GetRectanglePositionWithoutIntersection(rectangleForInsertion, intersectedRectangle);

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
            var nextLayer = GetLayerAfterFewInsertionsRectangleWithDifferentSize(currentLayer, sizes.Length, sizes);

            nextLayer.Radius.Should().Be(10);
        }

        [Test]
        public void GetPositionOnCircleWithoutIntersection_ShouldChangeCornerPositiomForSector_WhenMoveRectangleClockwise()
        {
            var sizesForInsertions = new Size[]
            {
                new (1,1), new(5,8), new (4,4), new (4,4), new(4,4)
            };
            var fullLayer = GetLayerAfterFewInsertionsRectangleWithDifferentSize(currentLayer, sizesForInsertions.Length,
                sizesForInsertions);
            var forInsertion = new Rectangle(new (11, 5), new (6,6));
            var intersected = new Rectangle(new(10, 5),new(5, 8));

            var newPosition = fullLayer.GetRectanglePositionWithoutIntersection(forInsertion, intersected);

            newPosition.Should().Be(new Point(-1, 12));
        }

        private CircleLayer GetLayerAfterFewInsertionsRectangleWithDifferentSize(CircleLayer layer, int insertionsCount, Size[] sizes)
        {
            for (var i = 1; i <= insertionsCount; i++)
            {
                var location = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(i), sizes[i - 1]);
                var rectangleForInsert = new Rectangle(location, sizes[i - 1]);
                layer = layer.OnSuccessInsertRectangle(storage.Add(rectangleForInsert));
            }
            return layer;
        }


        [Test]
        public void GetPositionOnCircleWithoutIntersection_ShouldCreateNewCircle_IfNeedMoveRectangleFromLastSector()
        {
            var intersectedSize = new Size(4, 9);
            var sizesForInsertions = new Size[]
            {
                new (1,1), new(1,8), new (4,2), intersectedSize, 
                new(1,1), new(1,1), new(1,1)
            };
            var fullLayer = GetLayerAfterFewInsertionsRectangleWithDifferentSize(currentLayer, sizesForInsertions.Length,
                sizesForInsertions);
            var forInsertion = new Rectangle(new(-8,2), new(8, 3));
            var intersected = new Rectangle(new(-4, -4), intersectedSize);

            var newPosition = fullLayer.GetRectanglePositionWithoutIntersection(forInsertion, intersected);

            newPosition.Should().Be(new Point(5, -5));
        }
    }
}
