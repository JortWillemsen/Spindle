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
}

public struct ClSceneInfo
{
    public int ImageWidth;
    public int ImageHeight;
    public int NumSpheres;
    public int NumTriangles;
}
