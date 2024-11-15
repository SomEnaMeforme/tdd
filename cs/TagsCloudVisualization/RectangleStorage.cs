using System.Collections.Generic;
using System.Drawing;

namespace TagsCloudVisualization;

public class RectangleStorage
{
    private readonly List<RectangleWrapper> elements = new();

    public int Add(Rectangle r)
    {
        elements.Add(r);
        return elements.Count - 1;
    }

    public IEnumerable<Rectangle> GetAll()
    {
        foreach (var rectangle in elements) yield return rectangle;
    }

    public RectangleWrapper GetById(int id)
    {
        return elements[id];
    }

    public class RectangleWrapper
    {
        public RectangleWrapper(Rectangle v)
        {
            Value = v;
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

        public static implicit operator RectangleWrapper(Rectangle v)
        {
            return new RectangleWrapper(v);
        }

        public static implicit operator Rectangle(RectangleWrapper r)
        {
            return r.Value;
        }

        public override bool Equals(object? obj)
        {
            return (obj as RectangleWrapper)?.Value.Equals(Value) ?? (obj as Rectangle?)?.Equals(Value) ?? false;
        }
    }
}