using System.Collections.Generic;
using System.Drawing;

namespace TagsCloudVisualization;

public class RectangleWrapper
{
    public RectangleWrapper(Rectangle value)
    {
        Value = value;
    }

    private Rectangle Value { get; set; }

    public Size Size
    {
        get => Value.Size;
        set => Value = new Rectangle(Location, value);
    }

    public Point Location
    {
        get => Value.Location;
        set => Value = new Rectangle(value, Size);
    }

    public int Top => Value.Top;
    public int Bottom => Value.Bottom;
    public int Left => Value.Left;
    public int Right => Value.Right;

    public static implicit operator RectangleWrapper(Rectangle value)
    {
        return new RectangleWrapper(value);
    }

    public static implicit operator Rectangle(RectangleWrapper wrapper)
    {
        return wrapper.Value;
    }

    public override bool Equals(object? obj)
    {
        return (obj as RectangleWrapper)?.Value.Equals(Value) ?? (obj as Rectangle?)?.Equals(Value) ?? false;
    }
}
