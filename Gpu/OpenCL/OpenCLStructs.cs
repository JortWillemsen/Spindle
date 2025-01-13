using System.Numerics;
using System.Runtime.InteropServices;

namespace Gpu.OpenCL;

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

    public Vector3 ToVector3() => new(X, Y, Z);

    public override string ToString() => $"<{X}, {Y}, {Z}>";
}

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct ClTriangle
{
    public ClFloat3 V1; // 16 bits
    public ClFloat3 V2; // 16 bits
    public ClFloat3 V3; // 16 bits
    public uint Material; // 4 bits // TODO this is new, check if reading works
    // 12 empty bits

    public override string ToString() =>
        $"ClTriangle (V1: {V1}, V2: {V2}, V3: {V3}, Material: {Material})";
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct ClSphere
{
    public ClFloat3 Position; // 16 bits
    public float Radius; // 4 bits
    public uint Material; // 4 bits TODO this is new, check if reading works
    // 8 bits empty

    public override string ToString() =>
        $"ClSphere: (Pos: {Position}, Radius: {Radius}, Material: {Material}>";
}

[StructLayout(LayoutKind.Sequential, Size = 48)]
public struct ClRay
{
    public ClFloat3 Direction; // 16 bits
    public ClFloat3 Origin; // 16 bits
    public float T; // 4 bits
    public uint Object_id; // 4 bits
    // 8 empty bits TODO could be used to store more info? Or perhaps compact memory somehow?

    public override string ToString() =>
        $"ClRay: (Origin {Origin}, Dir {Direction}, T: {T}, Object_id: {Object_id})";
}

// [StructLayout(LayoutKind.Explicit, Size = 64)]
// public struct ClIntersection
// {
//     [FieldOffset(0)]  public bool Hit; // 16 bits (OpenCL takes 16 bites, C# 4, hence offset)
//     [FieldOffset(16)] public ClFloat3 HitPoint; // 16 bits
//     [FieldOffset(32)] public ClFloat3 Normal; // 16 bits
//     [FieldOffset(48)] public float T; // 4 bits
//     [FieldOffset(52)] public uint Material; // 4 bits
//     // 8 bits empty
//
//     /// <inheritdoc />
//     public override string ToString()
//     {
//         return $"IntersectionResult: (HitPoint: {HitPoint}, Normal: {Normal}, T: {T}, Material: {Material})";
//     }
// }

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct ClSceneInfo
{
    public int NumSpheres; // 4 bits
    public int NumTriangles; // 4 bits

    public override string ToString() =>
        $"ClSceneInfo: (Spheres: {NumSpheres}, Triangles: {NumTriangles})";
}

// [StructLayout(LayoutKind.Sequential)] // TODO find size
// public struct ClExtensionRay {
//  // TODO: Add whatever you need
// }

// [StructLayout(LayoutKind.Sequential)] // TODO find size
// public struct ClShadowRay {
//     // TODO: Add whatever you need
// }

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct ClMaterial // Depending on type, not all fields are used
{
    public ClFloat3 Color; // 16 bits
    public float Albedo; // 4 bits
    public MaterialType Type; // 4 bits
    // 12 empty bits

    public override string ToString() =>
        $"ClMaterial: Type: {Type}, Albedo: {Albedo} Color: {Color})";
}

public enum MaterialType : uint // 4 bits
{
    Diffuse = 1,
    Reflective = 2
}
