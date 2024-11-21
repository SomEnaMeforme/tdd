using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using NUnit.Framework.Interfaces;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace TagsCloudVisualization.Tests
{
    public class CircularCloudLayouterTests
    {
        private CircularCloudLayouter layouter;
        private Point defaultCenter;
        private List<RectangleWrapper> storage;

        [SetUp]
        public void SetUp()
        {
            defaultCenter = new Point(5, 5);
            storage = [];
            layouter = new CircularCloudLayouter(defaultCenter, storage);
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                var testObj = TestContext.CurrentContext.Test.Parent?.Fixture as CircularCloudLayouterTests;
                var info = typeof(CircularCloudLayouterTests)
                    .GetField("storage", BindingFlags.NonPublic | BindingFlags.Instance);
                var st = info?.GetValue(testObj);

                var visualizator = new CircularCloudVisualizer(st as List<RectangleWrapper> ?? [], new Size(1000, 1000));
                var pathFile = Path.Combine(Directory.GetCurrentDirectory(), TestContext.CurrentContext.Test.Name);
                visualizator.CreateImage(pathFile);
                TestContext.Out.WriteLine($"Tag cloud visualization saved to file {pathFile}");
            }
        }

        [TestCase(0, 4, TestName = "WhenWidthZero")]
        [TestCase(3, 0, TestName = "WhenHeightZero")]
        [TestCase(-3, 4, TestName = "WhenWidthIsNegative")]
        [TestCase(3, -4, TestName = "WhenHeightNegative")]
        [TestCase(-3, -4, TestName = "WhenWidthAndHeightNegative")]
        [TestCase(0, 0, TestName = "WhenWidthAndHeightIsZero")]
        public void Insert_ShouldThrow(int width, int height)
        {
            var inCorrectSize = new Size(width, height);

            Action act = () => layouter.PutNextRectangle(inCorrectSize);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PutNextRectangle_ShouldAddRectangleToCenter_WhenRectangleFirst()
        {
            var firstRectangleSize = new Size(6, 4);
            var expected = new Rectangle(new Point(2, 3), firstRectangleSize);

            var nextRectangle = layouter.PutNextRectangle(firstRectangleSize);

            nextRectangle
                .Should().Be(expected);
        }

        [Test]
        public void PutNextRectangle_ShouldCreateFirstCircleLayer_AfterCreation()
        {
            layouter.CurrentLayer.Should().NotBeNull();
        }

        [TestCase(6, 4, 4)]
        [TestCase(4, 6, 4)]
        [TestCase(2, 2, 2)]
        [TestCase(5, 9, 6)]
        public void PutNextRectangle_ShouldCreateFirstCircleLayer_WithRadiusEqualHalfDiagonalFirstRectangleRoundToInt(int height, int width, int expected)
        {
            var firstRectangleSize = new Size(width, height);

            layouter.PutNextRectangle(firstRectangleSize);

            layouter.CurrentLayer.Radius.Should().Be(expected);
        }

        [Test]
        public void PutRectangleOnCircleWithoutIntersection_ShouldUseCircleLayer_ForChoosePositionForRectangle()
        {
            var firstRectangleSize = new Size(4, 4);
            var expectedRadius = 7;
            layouter = new CircularCloudLayouter(defaultCenter, storage);
            var expected = new Point(defaultCenter.X, defaultCenter.Y - expectedRadius);
            layouter.PutNextRectangle(firstRectangleSize);
            var rPos = layouter.CurrentLayer.CalculateTopLeftRectangleCornerPosition(firstRectangleSize);

            ;
            var secondRectangleLocation = layouter.PutRectangleOnCircleWithoutIntersection(new(new(rPos, firstRectangleSize))).Location;

            secondRectangleLocation.Should().Be(expected);
        }

        [Test]
        public void PutRectangleOnCircleWithoutIntersection_ShouldPutRectangleWithoutIntersection()
        {
            var firstRectangleSize = new Size(6, 4);
            var expected = new Point(14, 1);
            layouter = new CircularCloudLayouter(defaultCenter, storage);
            var sizes = new Size[] { new(4, 7), new(4, 4), new(4, 4), new(4, 4) };
            layouter.PutNextRectangle(firstRectangleSize);
            layouter = InsertionsWithoutCompress(4, layouter, sizes, storage);
            var rectangleWithIntersection =
                new Rectangle(layouter.CurrentLayer.CalculateTopLeftRectangleCornerPosition(new(3, 3)), new(3, 3));

            var rectangleLocation = layouter.PutRectangleOnCircleWithoutIntersection(rectangleWithIntersection).Location;

            rectangleLocation.Should().Be(expected);
        }

        private static CircularCloudLayouter InsertionsWithoutCompress(int insertionsCount, CircularCloudLayouter l, Size[] sizes, List<RectangleWrapper> storage)
        {
            for (var i = 0; i < insertionsCount; i++)
            {
                var pos = l.CurrentLayer.CalculateTopLeftRectangleCornerPosition(sizes[i]);
                var forInsertion = new Rectangle(pos, sizes[i]);
                storage.Add(forInsertion);
                l.PutRectangleOnCircleWithoutIntersection(forInsertion);
            }

            return l;
        }

        [Test]
        public void PutNextRectangle_ShouldTryMoveRectangleCloserToCenter_WhenItPossible()
        {
            var firstRectangleSize = new Size(6, 4);
            var secondRectangleSize = new Size(4, 4);
            var expectedSecondRectangleLocation = new Point(5, -1);

            layouter.PutNextRectangle(firstRectangleSize);
            var second = layouter.PutNextRectangle(secondRectangleSize);

            second.Location.Should().Be(expectedSecondRectangleLocation);
        }
    }
}