using System;
using System.Drawing;

namespace TagsCloudVisualization;

public class CircleLayer
{
    public Point Center { get; }
    public int Radius { get; private set; }

    private const int DeltaRadius = 5;
    private int currentPositionDegrees;
    
    public  CircleLayer(Point center)
    {
        Center = center;
        Radius = 5;
    } 

    private void Expand()
    {
        Radius += DeltaRadius;
    }

    public Point GetNextRectanglePosition()
    {
        currentPositionDegrees = GetNextPosition();
        var positionAngleInRadians = currentPositionDegrees * Math.PI / 180.0;
        return new Point(Center.X + (int)Math.Ceiling(Radius * Math.Cos(positionAngleInRadians)), 
            Center.Y + (int)Math.Ceiling(Radius * Math.Sin(positionAngleInRadians)));
    }

    private int GetNextPosition()
    {
        if (currentPositionDegrees++ > 360)
        {
            currentPositionDegrees = 0;
            Expand();
        }
        return currentPositionDegrees;
    }
}