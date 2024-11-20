using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TagsCloudVisualization;

public class CircularCloudLayouter
{
    private readonly Point center;
    private readonly RectangleStorage storage = new();
    private readonly BruteForceNearestFinder nearestFinder;

    public CircleLayer CurrentLayer { get; }

    public CircularCloudLayouter(Point center)
    {
        this.center = center;
        nearestFinder = new BruteForceNearestFinder();
        CurrentLayer = new(center, storage);
    }

    internal CircularCloudLayouter(Point center, RectangleStorage storage) : this(center)
    {
        this.storage = storage;
        CurrentLayer = new(center, storage);
    }

    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        ValidateRectangleSize(rectangleSize);
        var firstRectanglePosition = CurrentLayer.CalculateTopLeftRectangleCornerPosition(rectangleSize);
        var id = SaveRectangle(firstRectanglePosition, rectangleSize);
        var rectangleWithOptimalPosition = OptimiseRectanglePosition(id);
        return rectangleWithOptimalPosition;
    }

    private int SaveRectangle(Point firstLocation, Size rectangleSize)
    {
        var id = storage.Add(new Rectangle(firstLocation, rectangleSize));
        return id;
    }

    private Rectangle OptimiseRectanglePosition(int id)
    {
        PutRectangleOnCircleWithoutIntersection(id);
        return TryMoveRectangleCloserToCenter(id);
    }

    public Rectangle PutRectangleOnCircleWithoutIntersection(int id)
    {
        var forOptimise = storage.GetById(id);
        var intersected = GetRectangleIntersection(forOptimise);

        while (intersected != default && intersected.Value != default)
        {
            var possiblePosition = CurrentLayer.GetRectanglePositionWithoutIntersection(forOptimise, intersected.Value);
            forOptimise.Location = possiblePosition;
            intersected = GetRectangleIntersection(forOptimise);
        }

        CurrentLayer.OnSuccessInsertRectangle(id);
        return forOptimise;
    }

    internal Rectangle TryMoveRectangleCloserToCenter(int id)
    {
        var rectangleForMoving = storage.GetById(id);
        var toCenter = GetDirectionsForMovingToCenter(rectangleForMoving);
        foreach (var direction in toCenter)
        {
            rectangleForMoving.Location = MoveToCenterByDirection(rectangleForMoving, direction);
        }
        return rectangleForMoving;
    }


    private Point MoveToCenterByDirection(Rectangle forMoving, Direction toCenter)
    {
        var nearest = nearestFinder.FindNearestByDirection(forMoving, toCenter, storage.GetAll());
        if (nearest == null) return forMoving.Location;
        var distanceCalculator = nearestFinder.GetMinDistanceCalculatorBy(toCenter);
        var distanceForMove = distanceCalculator(nearest.Value, forMoving);
        return MoveByDirection(forMoving.Location, distanceForMove, toCenter);
    }

    private Point MoveByDirection(Point forMoving, int distance, Direction toCenter)
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

    private void ValidateRectangleSize(Size forInsertion)
    {
        if (forInsertion.Width <= 0 || forInsertion.Height <= 0)
            throw new ArgumentException($"Rectangle has incorrect size: width = {forInsertion.Width}, height = {forInsertion.Height}");
    }

    private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
    {
        return storage.GetAll()
            .FirstOrDefault(r => forInsertion.IntersectsWith(r) && forInsertion != r);
    }
}