using System.Numerics;
using System.Runtime.InteropServices;

namespace Gpu;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct ClTriangle
{
    public ClFloat3 V1; // 16 bits
    public ClFloat3 V2; // 16 bits
    public ClFloat3 V3; // 16 bits
    public ClFloat3 Normal; // 16 bits
}

[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct ClFloat3
{
    public float X; // 4 bits
    public float Y; // 4 bits
    public float Z; // 4 bits
    // 4 bits empty

    public static ClFloat3 FromVector3(Vector3 v)
    {
        return new ClFloat3 { X = v.X, Y = v.Y, Z = v.Z };
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"<{X}, {Y}, {Z}>";
    }
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct ClSphere
{
    public ClFloat3 Position; // 16 bits
    public float Radius; // 4 bits
    // 12 bits empty
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct ClRay
{
    public ClFloat3 Direction; // 16 bits
    public ClFloat3 Origin; // 16 bits

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ClRay: (Origin {Origin}, Dir {Direction})";
    }
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct ClIntersection
{
    [FieldOffset(0)] public bool Hit; // 16 bits (OpenCL takes 16 bytes, C# 4, hence offset)
    [FieldOffset(16)] public ClFloat3 HitPoint; // 16 bits
    [FieldOffset(32)] public ClFloat3 Normal; // 16 bits
    [FieldOffset(48)] public float T; // 4 bits
    [FieldOffset(52)] public uint Material; // 4 bits
    // 8 bits empty

    /// <inheritdoc />
    public override string ToString()
    {
        return $"IntersectionResult: (HitPoint: {HitPoint}, Normal: {Normal}, T: {T}, Material: {Material})";
    }
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct ClSceneInfo
{
    public int NumSpheres; // 4 bits
    public int NumTriangles; // 4 bits
}

[StructLayout(LayoutKind.Sequential)] // TODO find size
public struct ClExtensionRay {
 // TODO: Add whatever you need
}

[StructLayout(LayoutKind.Sequential)] // TODO find size
public struct ClShadowRay {
    // TODO: Add whatever you need
}
