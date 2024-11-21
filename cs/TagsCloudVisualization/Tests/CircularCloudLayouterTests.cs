using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace TagsCloudVisualization.Tests;

public class CircularCloudLayouterTests
{
    private CircularCloudLayouter layouter;
    private Point defaultCenter;
    private List<Rectangle> storage;

    public static IEnumerable<TestCaseData> IntersectionTestsData
    {
        get
        {
            yield return new TestCaseData(new Size[]
                {
                    new(1, 1), new(1, 1), new(1, 1), new(400, 400),
                    new(1, 4), new(6, 6)
                })
                .SetName("WhenAddedSmallRectanglesWithOneVeryBig");
            yield return new TestCaseData(new Size[]
                {
                    new(100, 100), new(123, 121), new(100, 100), new(400, 400),
                    new(100, 400), new(600, 128)
                })
                .SetName("WhenAddedBigRectangles");
            yield return new TestCaseData(new Size[] { new(4, 4), new(4, 4), new(4, 4), new(4, 4), new(4, 4) })
                .SetName("WhenAddedRectanglesHasSameSize");
        }
    }

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

            var visualizer = new CircularCloudVisualizer(st as List<Rectangle> ?? [], new Size(1000, 1000));
            var pathFile = Path.Combine(Directory.GetCurrentDirectory(), TestContext.CurrentContext.Test.Name);
            visualizer.CreateImage(pathFile);
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

    [TestCaseSource(nameof(IntersectionTestsData))]
    public void PutRectangleOnCircleWithoutIntersection_ShouldPutRectangleWithoutIntersection(Size[] sizes)
    {
        layouter = InsertFewRectangles(layouter, sizes, storage);
        var last = layouter.PutRectangleWithoutIntersection(new Size(3, 3));

        storage.Where(addedRectangle => addedRectangle.IntersectsWith(last)).Should().BeEmpty();
    }

    private static CircularCloudLayouter InsertFewRectangles(CircularCloudLayouter layouter, Size[] sizes,
        List<Rectangle> storage)
    {
        for (var i = 0; i < sizes.Length; i++)
        {
            var forInsertion = layouter.PutNextRectangle(sizes[i]);
            storage.Add(forInsertion);
        }

        return layouter;
    }

    [Test]
    public void PutNextRectangle_ShouldTryMoveRectangleCloserToCenter_WhenItPossible()
    {
        var firstRectangleSize = new Size(6, 4);
        var secondRectangleSize = new Size(4, 4);

        var first = layouter.PutNextRectangle(firstRectangleSize);
        var second = layouter.PutNextRectangle(secondRectangleSize);
        var hasEqualBound = first.Right == second.Left || first.Top == second.Bottom
                                                       || second.Right == first.Left || second.Top == first.Bottom;

        first.IntersectsWith(second).Should().BeFalse();
        hasEqualBound.Should().BeTrue();
    }
}