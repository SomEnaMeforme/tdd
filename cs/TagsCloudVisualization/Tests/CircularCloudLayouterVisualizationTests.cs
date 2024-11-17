using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace TagsCloudVisualization.Tests;

public class CircularCloudLayouterVisualizationTests
{
    private readonly Size ImageSize = new(1000, 1000);

    [Test]
    public void GenerateImage()
    {
        var center = new Point(ImageSize.Width / 2, ImageSize.Height / 2);
        var visualizator = new CircularCloudVisualization(GenerateRectangles(center), ImageSize);
        visualizator.CreateImage();
    }

    private RectangleStorage GenerateRectangles(Point center)
    {
        var rnd = new Random();
        var storage = new RectangleStorage();
        var layouter = new CircularCloudLayouter(center, storage);
        for (var i = 0; i < 41; i++) layouter.PutNextRectangle(new Size(rnd.Next(50, 100), rnd.Next(50, 100)));

        return storage;
    }

    [Test]
    public void GenerateImageWithSaveEveryStep()
    {
        var rnd = new Random();
        var center = new Point(ImageSize.Width / 2, ImageSize.Height / 2);
        var visualizator = new CircularCloudVisualization(new RectangleStorage(), ImageSize);
        var layouter = new CircularCloudLayouter(center, new RectangleStorage());
        visualizator.CreateImageWithSaveEveryStep(layouter,
            new Size[41].Select(x => new Size(15, 100)).ToArray());
    }
}