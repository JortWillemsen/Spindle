using System.Numerics;
using System.Runtime.InteropServices;

namespace Gpu;

public struct ClTriangle
{
    public ClFloat3 V1;
    public ClFloat3 V2;
    public ClFloat3 V3;
    public ClFloat3 Normal;
}

[StructLayout(LayoutKind.Sequential)]
public struct ClFloat3
{
    public float X;
    public float Y;
    public float Z;
    public float Padding;

    public static ClFloat3 FromVector3(Vector3 v)
    {
        return new ClFloat3 { X = v.X, Y = v.Y, Z = v.Z };
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"<{X}, {Y}, {Z}>";
    }
}

public struct ClSphere
{
    public ClFloat3 Position;
    public float Radius;
}

public struct ClRay
{
    public ClFloat3 Direction;
    public ClFloat3 Origin;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ClRay: (Origin {Origin}, Dir {Direction})";
    }
}

public struct ClIntersectionResult
{
    public bool Hit;
    public ClFloat3 HitPoint;
    public ClFloat3 Normal;
    public float T;
    public int Material;
}

public struct ClSceneInfo
{
    public int NumSpheres;
    public int NumTriangles;
}

public struct ClExtensionRay {
 // TODO: Add whatever you need
}

public struct ClShadowRay {
    // TODO: Add whatever you need
}
