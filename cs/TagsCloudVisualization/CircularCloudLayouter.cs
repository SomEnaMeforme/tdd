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

    public CircularCloudLayouter(Point center)
    {
        this.center = center;
        nearestFinder = new BruteForceNearestFinder();
    }

    internal CircularCloudLayouter(Point center, RectangleStorage storage) : this(center)
    {
        this.storage = storage;
    }

    public CircleLayer CurrentLayer { get; private set; }

    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        ValidateRectangleSize(rectangleSize);
        Point firstRectanglePosition;
        var isFirstRectangle = IsFirstRectangle();
        if (isFirstRectangle)
        {
            CreateFirstLayer(rectangleSize);
            firstRectanglePosition = PutRectangleToCenter(rectangleSize);
        }
        else
        {
            firstRectanglePosition = CurrentLayer.CalculateTopLeftRectangleCornerPosition(rectangleSize);
        }

        var id = SaveRectangle(firstRectanglePosition, rectangleSize);
        var rectangleWithOptimalPosition = OptimiseRectanglePosition(id, isFirstRectangle);
        return rectangleWithOptimalPosition;
    }

    private int SaveRectangle(Point firstLocation, Size rectangleSize)
    {
        var id = storage.Add(new Rectangle(firstLocation, rectangleSize));
        return id;
    }

    public Rectangle OptimiseRectanglePosition(int id, bool isFirstRectangle)
    {
        if (isFirstRectangle) return storage.GetById(id);
        PutRectangleOnCircleWithoutIntersection(id);
        return TryMoveRectangleCloserToCenter(id);
    }

    public Rectangle PutRectangleOnCircleWithoutIntersection(int id)
    {
        var r = storage.GetById(id);
        var intersected = GetRectangleIntersection(r);

        while (RectangleHasIntersection(intersected))
        {
            var possiblePosition = CurrentLayer.GetRectanglePositionWithoutIntersection(r, intersected.Value);
            r.Location = possiblePosition;
            intersected = GetRectangleIntersection(r);
        }

        CurrentLayer.OnSuccessInsertRectangle(id);
        return r;
    }

    private bool RectangleHasIntersection(Rectangle? intersected)
    {
        return intersected != default && intersected.Value != default;
    }


    public Rectangle TryMoveRectangleCloserToCenter(int id)
    {
        var rectangleForMoving = storage.GetById(id);
        var directionsForMoving = GetDirectionsForMovingToCenter(rectangleForMoving);
        var distancesForMove = directionsForMoving
            .Select(d => (Nearest: nearestFinder.FindNearestByDirection(rectangleForMoving, d, storage.GetAll()),
                Direction: d))
            .Where(tuple => tuple.Nearest != null)
            .Select(t => (
                DistanceCalculator: nearestFinder.GetMinDistanceCalculatorBy(t.Direction), t.Nearest, t.Direction))
            .Select(t => (Distance: t.DistanceCalculator(t.Nearest.Value, rectangleForMoving), t.Direction))
            .ToArray();
        rectangleForMoving.Location = MoveByDirections(rectangleForMoving.Location, distancesForMove);
        return rectangleForMoving;
    }

    private Point MoveByDirections(Point p, (int Distance, Direction Direction)[] t)
    {
        foreach (var moveInfo in t)
        {
            var factorForDistanceByX = moveInfo.Direction == Direction.Left ? -1 :
                moveInfo.Direction == Direction.Right ? 1 : 0;
            var factorForDistanceByY = moveInfo.Direction == Direction.Top ? -1 :
                moveInfo.Direction == Direction.Bottom ? 1 : 0;
            p.X += moveInfo.Distance * factorForDistanceByX;
            p.Y += moveInfo.Distance * factorForDistanceByY;
        }

        return p;
    }

    private List<Direction> GetDirectionsForMovingToCenter(Rectangle r)
    {
        var directions = new List<Direction>();
        if (r.Bottom < center.Y) directions.Add(Direction.Bottom);
        if (r.Left > center.X) directions.Add(Direction.Left);
        if (r.Right < center.X) directions.Add(Direction.Right);
        if (r.Top > center.Y) directions.Add(Direction.Top);
        return directions;
    }

    private void ValidateRectangleSize(Size s)
    {
        if (s.Width <= 0 || s.Height <= 0)
            throw new ArgumentException($"Rectangle has incorrect size: width = {s.Width}, height = {s.Height}");
    }

    private Rectangle? GetRectangleIntersection(Rectangle forInsertion)
    {
        return storage.GetAll()
            .FirstOrDefault(r => forInsertion.IntersectsWith(r) && forInsertion != r);
    }

    private void CreateFirstLayer(Size firstRectangle)
    {
        var radius = Math.Ceiling(Math.Sqrt(firstRectangle.Width * firstRectangle.Width +
                                            firstRectangle.Height * firstRectangle.Height) / 2.0);
        CurrentLayer = new CircleLayer(center, (int)radius, storage);
    }

    private Point PutRectangleToCenter(Size rectangleSize)
    {
        var rectangleX = center.X - rectangleSize.Width / 2;
        var rectangleY = center.Y - rectangleSize.Height / 2;

        return new Point(rectangleX, rectangleY);
    }

    private bool IsFirstRectangle()
    {
        return storage.GetAll().FirstOrDefault() == default;
    }
}