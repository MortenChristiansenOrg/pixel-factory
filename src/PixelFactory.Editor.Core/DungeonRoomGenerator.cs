using System.Numerics;
using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Editor.Core;

public static class DungeonRoomGenerator
{
    public static MeshData Generate(float width = 4f, float depth = 4f, float wallHeight = 2.5f)
    {
        var vertices = new List<VertexPositionColor>();
        var indices = new List<ushort>();

        var floorColor = new Vector4(0.35f, 0.30f, 0.25f, 1f);
        var backWallColor = new Vector4(0.30f, 0.28f, 0.25f, 1f);
        var leftWallColor = new Vector4(0.22f, 0.20f, 0.18f, 1f);
        var rightWallColor = new Vector4(0.25f, 0.23f, 0.20f, 1f);

        var hw = width / 2f;
        var hd = depth / 2f;

        // Floor (2 triangles)
        AddQuad(vertices, indices,
            new Vector3(-hw, 0, hd),
            new Vector3(hw, 0, hd),
            new Vector3(hw, 0, -hd),
            new Vector3(-hw, 0, -hd),
            floorColor);

        // Back wall
        AddQuad(vertices, indices,
            new Vector3(-hw, 0, -hd),
            new Vector3(hw, 0, -hd),
            new Vector3(hw, wallHeight, -hd),
            new Vector3(-hw, wallHeight, -hd),
            backWallColor);

        // Left wall
        AddQuad(vertices, indices,
            new Vector3(-hw, 0, hd),
            new Vector3(-hw, 0, -hd),
            new Vector3(-hw, wallHeight, -hd),
            new Vector3(-hw, wallHeight, hd),
            leftWallColor);

        // Right wall
        AddQuad(vertices, indices,
            new Vector3(hw, 0, -hd),
            new Vector3(hw, 0, hd),
            new Vector3(hw, wallHeight, hd),
            new Vector3(hw, wallHeight, -hd),
            rightWallColor);

        return new MeshData
        {
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray(),
        };
    }

    private static void AddQuad(
        List<VertexPositionColor> vertices,
        List<ushort> indices,
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        Vector4 color)
    {
        var baseIndex = (ushort)vertices.Count;
        vertices.Add(new VertexPositionColor(v0, color));
        vertices.Add(new VertexPositionColor(v1, color));
        vertices.Add(new VertexPositionColor(v2, color));
        vertices.Add(new VertexPositionColor(v3, color));

        indices.Add(baseIndex);
        indices.Add((ushort)(baseIndex + 1));
        indices.Add((ushort)(baseIndex + 2));
        indices.Add(baseIndex);
        indices.Add((ushort)(baseIndex + 2));
        indices.Add((ushort)(baseIndex + 3));
    }
}
