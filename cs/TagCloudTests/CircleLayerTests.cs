using System.Drawing;
using FluentAssertions;
using NUnit.Framework;
using TagsCloudVisualization;

namespace TagCloudTests;

public class CircleLayerTests
{
    private CircleLayer currentLayer;

    [SetUp]
    public void SetUp()
    {
        var center = new Point(5, 5);
        currentLayer = new CircleLayer(center);
    }

    [Test]
    public void GetNextRectanglePosition_ShouldTryPutRectangleOnCircle()
    {
        var firstRectangleLocation = currentLayer.GetNextRectanglePosition();
        var secondRectangleLocation = currentLayer.GetNextRectanglePosition();

        var radius = currentLayer.Center.CalculateDistanceBetween(firstRectangleLocation);

        radius.Should().Be(currentLayer.Center.CalculateDistanceBetween(secondRectangleLocation));
    }

    [Test]
    public void GetNextRectanglePosition_MustExpandCircle_WhenTryPutRectangleOnFullCircle()
    {
        var firstPoint = currentLayer.GetNextRectanglePosition();
        for (int i = 0; i < 360; i++)
        {
            currentLayer.GetNextRectanglePosition();
        }

        var finalPoint = currentLayer.GetNextRectanglePosition();

        finalPoint.CalculateDistanceBetween(currentLayer.Center).Should()
            .BeGreaterThan(firstPoint.CalculateDistanceBetween(currentLayer.Center));
    }
}