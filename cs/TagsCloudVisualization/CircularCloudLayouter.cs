using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TagsCloudVisualization;

public class CircularCloudLayouter
{
    private readonly Point center;
    private readonly List<RectangleWrapper> storage = [];
    private readonly BruteForceNearestFinder nearestFinder;

    public CircleLayer CurrentLayer { get; }

    public CircularCloudLayouter(Point center)
    {
        this.center = center;
        nearestFinder = new BruteForceNearestFinder();
        CurrentLayer = new(center, storage);
    }

    internal CircularCloudLayouter(Point center, List<RectangleWrapper> storage) : this(center)
    {
        this.storage = storage;
        CurrentLayer = new(center, storage);
    }

    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        ValidateRectangleSize(rectangleSize);
        
        var firstRectanglePosition = CurrentLayer.CalculateTopLeftRectangleCornerPosition(rectangleSize);
        var forInsertion = new Rectangle(firstRectanglePosition, rectangleSize);
        var rectangleWithOptimalPosition = OptimiseRectanglePosition(forInsertion);
        storage.Add(rectangleWithOptimalPosition);
        CurrentLayer.OnSuccessInsertRectangle();
        return rectangleWithOptimalPosition;
    }

    private Rectangle OptimiseRectanglePosition(Rectangle forInsertion)
    {
        PutRectangleOnCircleWithoutIntersection(forInsertion);
        return TryMoveRectangleCloserToCenter(forInsertion);
    }

    public Rectangle PutRectangleOnCircleWithoutIntersection(RectangleWrapper forOptimise)
    {
        var intersected = GetRectangleIntersection(forOptimise);

        while (intersected != null && intersected.Value != default)
        {
            var possiblePosition = CurrentLayer.GetRectanglePositionWithoutIntersection(forOptimise, intersected.Value);
            forOptimise.Location = possiblePosition;
            intersected = GetRectangleIntersection(forOptimise);
        }

        return forOptimise;
    }

    internal Rectangle TryMoveRectangleCloserToCenter(RectangleWrapper rectangleForMoving)
    {
        var toCenter = GetDirectionsForMovingToCenter(rectangleForMoving);
        foreach (var direction in toCenter)
        {
            rectangleForMoving.Location = MoveToCenterByDirection(rectangleForMoving, direction);
        }
        return rectangleForMoving;
    }


    private Point MoveToCenterByDirection(Rectangle forMoving, Direction toCenter)
    {
        var nearest = nearestFinder.FindNearestByDirection(forMoving, toCenter, storage.Select(r => (Rectangle)r));
        if (nearest == null) return forMoving.Location;
        var distanceCalculator = nearestFinder.GetMinDistanceCalculatorBy(toCenter);
        var distanceForMove = distanceCalculator(nearest.Value, forMoving);
        return MoveByDirection(forMoving.Location, distanceForMove, toCenter);
    }

    private static Point MoveByDirection(Point forMoving, int distance, Direction toCenter)
    {
        var factorForDistanceByX = toCenter switch
        {
            Direction.Left => -1,
            Direction.Right => 1,
            _ => 0
        };
        var factorForDistanceByY = toCenter switch
        {
            Direction.Top => -1,
            Direction.Bottom => 1,
            _ => 0
        };
        forMoving.X += distance * factorForDistanceByX;
        forMoving.Y += distance * factorForDistanceByY;

        return forMoving;
    }

    private List<Direction> GetDirectionsForMovingToCenter(Rectangle forMoving)
    {
        var directions = new List<Direction>();
        if (forMoving.Bottom < center.Y) directions.Add(Direction.Bottom);
        if (forMoving.Left > center.X) directions.Add(Direction.Left);
        if (forMoving.Right < center.X) directions.Add(Direction.Right);
        if (forMoving.Top > center.Y) directions.Add(Direction.Top);
        return directions;
    }

    private static void ValidateRectangleSize(Size forInsertion)
    {
        if (forInsertion.Width <= 0 || forInsertion.Height <= 0)
            throw new ArgumentException($"Rectangle has incorrect size: width = {forInsertion.Width}, height = {forInsertion.Height}");
    }

    private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
    {
        if (storage.Count == 0) return null;
        return storage.Select(r => (Rectangle)r).FirstOrDefault(r => forInsertion != r
                                             && forInsertion.IntersectsWith(r));
    }
}