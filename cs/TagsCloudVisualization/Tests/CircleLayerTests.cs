using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using static TagsCloudVisualization.CircleLayer;

namespace TagsCloudVisualization.Tests
{
    public class CircleLayerTests
    {
        private CircleLayer currentLayer;
        private Size defaultRectangleSize;
        [SetUp]
        public void SetUp()
        {
            var startRadius = 5;
            var center = new Point(5, 5);
            currentLayer = new CircleLayer(center, startRadius);
            defaultRectangleSize = new Size(3, 4);
        }

        [Test]
        public void CircleLayer_InsertFirstForLayerRectangle_InTopRightSectorStart()
        {
            var possibleRectangleLocation = currentLayer.CalculateTopLeftRectangleCornerPosition(defaultRectangleSize);

            possibleRectangleLocation.Should().Be(GetCorrectRectangleLocationByExpectedSector(Sector.Top_Right));
        }

        [TestCase(1, Sector.Bottom_Right)]
        [TestCase(2, Sector.Bottom_Left)]
        [TestCase(3, Sector.Top_Left)]
        public void CircleLayer_InsertRectangleInNextSector_AfterSuccessInsertion(int insertionsCount, Sector expected)
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, insertionsCount);

            var possibleRectangleLocation = currentLayer.CalculateTopLeftRectangleCornerPosition(defaultRectangleSize);

            possibleRectangleLocation.Should().Be(GetCorrectRectangleLocationByExpectedSector(expected));
        }

        [Test]
        public void CircleLayer_GetNewLayer_AfterInsertionsOnAllSectors()
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);

            var nextLayer = currentLayer.OnSuccessInsertRectangle(new Rectangle(new Point(0, 0), defaultRectangleSize));

            nextLayer.Should().NotBeSameAs(currentLayer);
        }

        [Test]
        public void CircleLayer_RadiusNextCircleLayer_ShouldBeIntMaxDistanceFromCenterToInsertedRectangle()
        {
            currentLayer = GetLayerAfterFewInsertionsRectangleWithSameSize(currentLayer, 3);
            var nextRectangleLocation = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(4));

            var nextLayer = currentLayer.OnSuccessInsertRectangle(new Rectangle(nextRectangleLocation, new Size(2,2)));

            nextLayer.Radius.Should().Be(10);
        }

        private CircleLayer GetLayerAfterFewInsertionsRectangleWithSameSize(CircleLayer layer, int insertionsCount)
        {
            for (var i = 0; i < insertionsCount; i++)
            {
                var location = GetCorrectRectangleLocationByExpectedSector(GetSectorByInsertionsCount(i));
                var rectangleForInsert = new Rectangle(location, defaultRectangleSize);
                layer.OnSuccessInsertRectangle(rectangleForInsert);
            }
            return layer;
        }

        private Sector GetSectorByInsertionsCount(int count)
        {
            return (Sector)((count - 1) % 4);
        }

        private Point GetCorrectRectangleLocationByExpectedSector(Sector s)
        {
            switch (s)
            {
                case Sector.Top_Right: 
                    return new Point(currentLayer.Center.X, currentLayer.Center.Y - currentLayer.Radius - defaultRectangleSize.Height);
                case Sector.Bottom_Right: 
                    return new Point(currentLayer.Center.X + currentLayer.Radius, currentLayer.Center.Y);
                case Sector.Bottom_Left: 
                    return new Point(currentLayer.Center.X - defaultRectangleSize.Width, currentLayer.Center.Y + currentLayer.Radius);
                default: 
                    return new Point(currentLayer.Center.X - currentLayer.Radius - defaultRectangleSize.Width,
                    currentLayer.Center.Y - defaultRectangleSize.Height);
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
            var rectangleForInsertion = new Rectangle(new Point(-3, -2), new Size(4, 3));
            var intersectedRectangle = new Rectangle(new Point(-7, 1), new Size(8, 7));
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
    }
}
