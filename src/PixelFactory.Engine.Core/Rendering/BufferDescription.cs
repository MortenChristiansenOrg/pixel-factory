namespace PixelFactory.Engine.Core.Rendering;

public enum BufferUsage
{
    Vertex,
    Index,
    Constant,
}

public readonly record struct BufferDescription(int SizeInBytes, BufferUsage Usage);
