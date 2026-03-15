using System.Numerics;

namespace PixelFactory.Engine.Core.Rendering;

public sealed class IsometricCamera : ICamera
{
    private float _size;
    private float _aspectRatio;
    private Vector3 _target;
    private float _pitch = 35.264f * MathF.PI / 180f;
    private float _yaw = 45f * MathF.PI / 180f;
    private float _distance = 20f;

    public IsometricCamera(float size = 10f, float aspectRatio = 16f / 9f)
    {
        _size = size;
        _aspectRatio = aspectRatio;
        _target = Vector3.Zero;
    }

    public Vector3 Target
    {
        get => _target;
        set => _target = value;
    }

    public float Size
    {
        get => _size;
        set => _size = value;
    }

    public float AspectRatio
    {
        get => _aspectRatio;
        set => _aspectRatio = value;
    }

    public float Pitch
    {
        get => _pitch;
        set => _pitch = value;
    }

    public float Yaw
    {
        get => _yaw;
        set => _yaw = value;
    }

    public float Distance
    {
        get => _distance;
        set => _distance = value;
    }

    public Matrix4x4 ViewMatrix
    {
        get
        {
            var direction = new Vector3(
                MathF.Cos(_pitch) * MathF.Sin(_yaw),
                MathF.Sin(_pitch),
                MathF.Cos(_pitch) * MathF.Cos(_yaw)
            );

            var eye = _target + direction * _distance;
            return Matrix4x4.CreateLookAt(eye, _target, Vector3.UnitY);
        }
    }

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            var halfWidth = _size * _aspectRatio * 0.5f;
            var halfHeight = _size * 0.5f;
            return Matrix4x4.CreateOrthographicOffCenter(
                -halfWidth, halfWidth,
                -halfHeight, halfHeight,
                0.1f, 100f);
        }
    }
}
