﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TagsCloudVisualization;

public class CircularCloudLayouter
{
    private readonly List<Rectangle> storage;
    private readonly CloudCompressor compressor;
    
    public CirclePositionDistributor CurrentLayer { get; }

    public CircularCloudLayouter(Point center) : this(center, [])
    { }

    public CircularCloudLayouter(Point center, List<Rectangle> storage)
    {
        this.storage = storage;
        CurrentLayer = new(center);
        compressor = new(center, storage);
    }

    public Rectangle PutNextRectangle(Size nextRectangle)
    {
        ValidateRectangleSize(nextRectangle);

        var inserted = PutRectangleWithoutIntersection(nextRectangle);
        var rectangleWithOptimalPosition =  compressor.CompressCloudAfterInsertion(inserted);

        storage.Add(rectangleWithOptimalPosition);

        return rectangleWithOptimalPosition;
    }
    
    public Rectangle PutRectangleWithoutIntersection(Size forInsertionSize)
    {
        var firstRectanglePosition = CurrentLayer.GetNextPosition();
        var forInsertion = new Rectangle(firstRectanglePosition, forInsertionSize);
        var intersected = GetRectangleIntersection(forInsertion);

        while (intersected != null && intersected.Value != default)
        {
            var possiblePosition = CurrentLayer.GetNextPosition();
            forInsertion.Location = possiblePosition;
            intersected = GetRectangleIntersection(forInsertion);
        }

        return forInsertion;
    }

    private static void ValidateRectangleSize(Size forInsertion)
    {
        if (forInsertion.Width <= 0 || forInsertion.Height <= 0)
            throw new ArgumentException($"Rectangle has incorrect size: width = {forInsertion.Width}, height = {forInsertion.Height}");
    }

    private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
    {
        if (storage.Count == 0) return null;
        return storage.FirstOrDefault(r => forInsertion != r
                                             && forInsertion.IntersectsWith(r));
    }
}