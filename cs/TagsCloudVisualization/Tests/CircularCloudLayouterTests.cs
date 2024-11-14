﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentAssertions;

namespace TagsCloudVisualization.Tests
{
    public class CircularCloudLayouterTests
    {

        private CircularCloudLayouter layouter;
        private Point defaultCenter;

        [SetUp]
        public void SetUp()
        {
            defaultCenter = new Point(5, 5);
            layouter = new CircularCloudLayouter(defaultCenter);
        }

        [Test]
        public void GetRectangles_ShouldBeEmpty_BeforePutAnyRectangles()
        {
            layouter.GetRectangles()
                .Should().BeEmpty();
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
        public void PutNextRectangle_ShouldCreateFirstCircleLayer_AfterPutFirstRectangle()
        {
            var firstRectangleSize = new Size(6, 4);

            layouter.PutNextRectangle(firstRectangleSize);

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
        public void PutNextRectangle_ShouldAddRectangleToLayouter_AfterPut()
        {
            var firstRectangleSize = new Size(4, 4);

            layouter.PutNextRectangle(firstRectangleSize);

            layouter.GetRectangles()
                .Should().NotBeEmpty().And.HaveCount(1);
        }

        [Test]
        public void PutNextRectangle_ShouldUseCircleLayer_ForChoosePositionForRectangle()
        {
            var firstRectangleSize = new Size(4, 4);
            var expectedRadius = 7;
            var expected = new Point(defaultCenter.X, defaultCenter.Y - expectedRadius);

            layouter.PutNextRectangle(firstRectangleSize);
            var secondRectangleLocation = layouter.PutNextRectangle(firstRectangleSize).Location;

            secondRectangleLocation.Should().Be(expected);
        }

        [Test]
        public void PutNextRectangle_ShouldPutRectangleWithoutIntersection_WhenNeedOneMoveForDeleteIntersection()
        {
            var firstRectangleSize = new Size(6, 4);
            var expected = new Point(9, 1);

            layouter.PutNextRectangle(firstRectangleSize);
            layouter.PutNextRectangle(new Size(4, 4));
            layouter.PutNextRectangle(new Size(4, 4));
            layouter.PutNextRectangle(new Size(4, 4));
            layouter.PutNextRectangle(new Size(4, 4));

            var rectangleLocation = layouter.PutNextRectangle(new Size(3, 3)).Location;

            rectangleLocation.Should().Be(expected);
        }
    }
}
