using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TagsCloudVisualization;

public class CircleLayer
{
    public enum Sector
    {
        Top_Right,
        Bottom_Right,
        Bottom_Left,
        Top_Left
    }

    public Point Center { get; }
    public int Radius { get; private set; }

    private Sector currentSector;
    private readonly RectangleStorage storage;
    private readonly List<int> layerRectangles = new();

    public CircleLayer(Point center, int radius, RectangleStorage storage)
    {
        Center = center;
        Radius = radius;
        currentSector = Sector.Top_Right;
        this.storage = storage;
    }

    public void OnSuccessInsertRectangle(int addedRectangleId)
    {
        currentSector = GetNextClockwiseSector();
        layerRectangles.Add(addedRectangleId);
        if (ShouldCreateNewCircle())
            CreateNextLayerAndChangeCurrentOnNext();
    }

    private bool ShouldCreateNewCircle()
    {
        return currentSector == Sector.Top_Right;
    }

    private Sector GetNextClockwiseSector()
    {
        return currentSector == Sector.Top_Left ? Sector.Top_Right : currentSector + 1;
    }

    private void CreateNextLayerAndChangeCurrentOnNext()
    {
        var nextLayer = new CircleLayer(Center, CalculateRadiusForNextLayer(), storage);
        ChangeCurrentLayerBy(nextLayer);
    }

    private void ChangeCurrentLayerBy(CircleLayer next)
    {
        Radius = next.Radius;
        currentSector = next.currentSector;
        var rectanglesForNextRadius = RemoveRectangleInCircle();
        layerRectangles.Clear();
        layerRectangles.AddRange(rectanglesForNextRadius);
    }

    private int CalculateRadiusForNextLayer()
    {
        return layerRectangles
            .Select(id => CalculateDistanceBetweenCenterAndRectangle(storage.GetById(id)))
            .Min();
    }

    private List<int> RemoveRectangleInCircle()
    {
        return layerRectangles
            .Where(id => CalculateDistanceBetweenCenterAndRectangle(storage.GetById(id)) > Radius)
            .ToList();
    }

    private int CalculateDistanceBetweenCenterAndRectangle(Rectangle r)
    {
        var d1 = CalculateDistanceBetweenPoints(Center, new Point(r.Right, r.Top));
        var d2 = CalculateDistanceBetweenPoints(Center, new Point(r.Right, r.Bottom));
        var d3 = CalculateDistanceBetweenPoints(Center, new Point(r.Left, r.Bottom));
        var d4 = CalculateDistanceBetweenPoints(Center, new Point(r.Left, r.Top));
        return Math.Max(Math.Max(d1, d2), Math.Max(d3, d4));
    }

    private int CalculateDistanceBetweenPoints(Point p1, Point p2)
    {
        return (int)Math.Ceiling(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)));
    }

    public Point CalculateTopLeftRectangleCornerPosition(Size rectangleSize)
    {
        var rectangleStartPositionOnCircle = GetStartSectorPointOnCircleBySector(currentSector);
        switch (currentSector)
        {
            case Sector.Top_Right:
                return new Point(rectangleStartPositionOnCircle.X,
                    rectangleStartPositionOnCircle.Y - rectangleSize.Height);
            case Sector.Bottom_Right:
                return rectangleStartPositionOnCircle;
            case Sector.Bottom_Left:
                return new Point(rectangleStartPositionOnCircle.X - rectangleSize.Width,
                    rectangleStartPositionOnCircle.Y);
            default:
                return new Point(rectangleStartPositionOnCircle.X - rectangleSize.Width,
                    rectangleStartPositionOnCircle.Y - rectangleSize.Height);
        }
    }

    private Point GetStartSectorPointOnCircleBySector(Sector s)
    {
        switch (s)
        {
            case Sector.Top_Right:
                return new Point(Center.X, Center.Y - Radius);
            case Sector.Bottom_Right:
                return new Point(Center.X + Radius, Center.Y);
            case Sector.Bottom_Left:
                return new Point(Center.X, Center.Y + Radius);
            default:
                return new Point(Center.X - Radius, Center.Y);
        }
    }

    public Point GetRectanglePositionWithoutIntersection(Rectangle forInsertion, Rectangle intersected)
    {
        var nextPosition = CalculateNewPositionWithoutIntersectionBySector(currentSector, forInsertion, intersected);
        if (IsNextPositionMoveToAnotherSector(nextPosition, forInsertion.Size))
        {
            currentSector = GetNextClockwiseSector();
            if (ShouldCreateNewCircle()) CreateNextLayerAndChangeCurrentOnNext();
            nextPosition = CalculateTopLeftRectangleCornerPosition(forInsertion.Size);
        }

        return nextPosition;
    }

    private bool IsNextPositionMoveToAnotherSector(Point next, Size forInsertionSize)
    {
        return IsRectangleIntersectSymmetryAxis(new Rectangle(next, forInsertionSize));
    }

    private bool IsRectangleIntersectSymmetryAxis(Rectangle r)
    {
        return (r.Left < Center.X && r.Right > Center.X) || (r.Bottom > Center.Y && r.Top < Center.Y);
    }

    private Point CalculateNewPositionWithoutIntersectionBySector(Sector s, Rectangle forInsertion,
        Rectangle intersected)
    {
        var distanceForMoving = CalculateDistanceForMovingBySector(s, forInsertion, intersected);
        var isMovingAxisIsX = IsMovingAxisIsXBySector(s);
        var nearestForCenterCorner =
            CalculateCornerNearestForCenterAfterMove(s, distanceForMoving, forInsertion);
        var distanceForBringBackOnCircle =
            CalculateDeltaForBringRectangleBackOnCircle(nearestForCenterCorner, isMovingAxisIsX);
        distanceForMoving *= CalculateMoveMultiplierForMoveClockwise(isMovingAxisIsX, forInsertion);
        distanceForBringBackOnCircle *= CalculateMoveMultiplierForMoveFromCenter(!isMovingAxisIsX, forInsertion);
        return isMovingAxisIsX
            ? new Point(forInsertion.X + distanceForMoving, forInsertion.Y + distanceForBringBackOnCircle)
            : new Point(forInsertion.X + distanceForBringBackOnCircle, forInsertion.Y + distanceForMoving);
    }

    private int CalculateDeltaForBringRectangleBackOnCircle(Point nearestForCenterCorner, bool isMovingAxisIsX)
    {
        Func<Point, int> getAxisForBringBackOnCircle = isMovingAxisIsX ? p => p.Y : p => p.X;
        Func<Point, int> getStaticAxis = isMovingAxisIsX ? p => p.X : p => p.Y;

        var distanceOnStaticAxis = Math.Abs(getStaticAxis(nearestForCenterCorner) - getStaticAxis(Center));
        var distanceOnAxisForBringBackOnCircle = Math.Abs(getAxisForBringBackOnCircle(nearestForCenterCorner) -
                                                          getAxisForBringBackOnCircle(Center));
        return (int)Math.Ceiling(Math.Sqrt(Radius * Radius - distanceOnStaticAxis * distanceOnStaticAxis)) -
               distanceOnAxisForBringBackOnCircle;
    }

    private Point CalculateCornerNearestForCenterAfterMove(Sector s, int distanceForMoving, Rectangle r)
    {
        var isAxisForMoveIsX = IsMovingAxisIsXBySector(s);
        var moveMultiplier = CalculateMoveMultiplierForMoveClockwise(isAxisForMoveIsX, r);
        distanceForMoving *= moveMultiplier;
        var nearestCorner = GetCornerNearestForCenterBySector(s, r);
        return isAxisForMoveIsX
            ? new Point(nearestCorner.X + distanceForMoving, nearestCorner.Y)
            : new Point(nearestCorner.X, nearestCorner.Y + distanceForMoving);
    }

    private int CalculateMoveMultiplierForMoveFromCenter(bool isAxisForMoveIsX, Rectangle r)
    {
        return isAxisForMoveIsX
            ? r.Right <= Center.X ? -1 : 1
            : r.Bottom <= Center.Y
                ? -1
                : 1;
    }

    private int CalculateMoveMultiplierForMoveClockwise(bool isAxisForMoveIsX, Rectangle r)
    {
        return isAxisForMoveIsX
            ? r.Left > Center.X ? -1 : 1
            : r.Bottom > Center.Y
                ? -1
                : 1;
    }

    private int CalculateDistanceForMovingBySector(Sector s, Rectangle forInsertion, Rectangle intersected)
    {
        switch (s)
        {
            case Sector.Top_Right:
                return Math.Abs(forInsertion.Top - intersected.Bottom);
            case Sector.Bottom_Right:
                return Math.Abs(forInsertion.Right - intersected.Left);
            case Sector.Bottom_Left:
                return Math.Abs(forInsertion.Bottom - intersected.Top);
            default:
                return Math.Abs(forInsertion.Left - intersected.Right);
        }
    }

    private Point GetCornerNearestForCenterBySector(Sector s, Rectangle r)
    {
        switch (s)
        {
            case Sector.Top_Right:
                return new Point(r.Left, r.Bottom);
            case Sector.Bottom_Right:
                return new Point(r.Left, r.Top);
            case Sector.Bottom_Left:
                return new Point(r.Right, r.Top);
            default:
                return new Point(r.Right, r.Bottom);
        }
    }

    private bool IsMovingAxisIsXBySector(Sector s)
    {
        return s == Sector.Bottom_Right || s == Sector.Top_Left;
    }
}