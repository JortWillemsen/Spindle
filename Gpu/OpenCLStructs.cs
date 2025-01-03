using System.Numerics;

namespace Gpu;

public struct ClTriangle
{
    public Vector3 V1;
    public Vector3 V2;
    public Vector3 V3;
    public Vector3 Normal;
}

public struct ClSphere
{
    public Vector3 Origin;
    public float Radius;
}

public struct ClRay
{
    public Vector3 Origin;
    public Vector3 Direction;
}

public struct ClSceneInfo
{
    public int ImageWidth;
    public int ImageHeight;
    public int NumSpheres;
    public int NumTriangles;
}
