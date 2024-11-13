using System.Drawing;

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
    public int Radius { get; }


    private Sector currentSector;
    private readonly List<Rectangle> layerRectangles;
    private CircleLayer prevLayer;

    public CircleLayer(Point center, int radius)
    {
        Center = center;
        Radius = radius;
        currentSector = Sector.Top_Right;
        layerRectangles = new List<Rectangle>();
    }

    public CircleLayer OnSuccessInsertRectangle(Rectangle inserted)
    {
        currentSector = currentSector == Sector.Top_Left ? Sector.Top_Right : currentSector + 1;
        layerRectangles.Add(inserted);
        if (currentSector == Sector.Top_Right) 
            return CreateNextLayer();
        return this;
    }

    private CircleLayer CreateNextLayer()
    {
        var nextLayer = new CircleLayer(Center, CalculateRadiusForNextLayer());
        nextLayer.prevLayer = this;
        return nextLayer;
    }

    private int CalculateRadiusForNextLayer()
    {
        var prevSector = Sector.Top_Right - 1;
        return layerRectangles.Select(r => CalculateDistanceBetweenCenterAndRectangleBySector(r, prevSector + 1)).Max();
    }

    private int CalculateDistanceBetweenCenterAndRectangleBySector(Rectangle r, Sector s)
    {
        switch (s)
        {
            case Sector.Top_Right: 
                return CalculateDistanceBetweenPoints(Center, new Point(r.X + r.Width, r.Y));
            case Sector.Bottom_Right:
                return CalculateDistanceBetweenPoints(Center, new Point(r.X + r.Width, r.Y + r.Height));
            case Sector.Bottom_Left:
                return CalculateDistanceBetweenPoints(Center, new Point(r.X - r.Width, r.Y + r.Height));
            default: 
                return CalculateDistanceBetweenPoints(Center, new Point(r.X - r.Width, r.Y));
        }
    }

    private int CalculateDistanceBetweenPoints(Point p1, Point p2)
    {
        return (int)Math.Ceiling(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)));
    }

    public Point CalculateTopLeftRectangleCornerPosition(Size rectangleSize)
    {
        var relevantForSectorPosition = GetPositionRelevantForSector();
        switch (currentSector)
        {
            case Sector.Top_Right:
                return new Point(relevantForSectorPosition.X, relevantForSectorPosition.Y - rectangleSize.Height);
            case Sector.Bottom_Right: 
                return relevantForSectorPosition;
            case Sector.Bottom_Left:
                return new Point(relevantForSectorPosition.X - rectangleSize.Width, relevantForSectorPosition.Y);
            default:
                return new Point(relevantForSectorPosition.X - rectangleSize.Width,
                    relevantForSectorPosition.Y - rectangleSize.Height);
        }
    }

    private Point GetPositionRelevantForSector()
    {
        switch (currentSector)
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
        return CalculateNewPositionWithoutIntersectionBySector(currentSector, forInsertion, intersected);
    }

    private Point CalculateNewPositionWithoutIntersectionBySector(Sector s, Rectangle forInsertion, Rectangle intersected)
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
        var distanceOnAxisForBringBackOnCircle = Math.Abs(getAxisForBringBackOnCircle(nearestForCenterCorner) - getAxisForBringBackOnCircle(Center));
        return (int)Math.Ceiling(Math.Sqrt(Radius * Radius - distanceOnStaticAxis * distanceOnStaticAxis)) - distanceOnAxisForBringBackOnCircle;
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
            ? r.Right < Center.X ? -1 : 1 
            : r.Bottom < Center.Y ? -1 : 1;
    }
    private int CalculateMoveMultiplierForMoveClockwise(bool isAxisForMoveIsX, Rectangle r)
    {
        return isAxisForMoveIsX
            ? r.Left > Center.X ? -1 : 1
            : r.Bottom > Center.Y ? -1 : 1;
    }

    private int CalculateDistanceForMovingBySector (Sector s, Rectangle forInsertion, Rectangle intersected)
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