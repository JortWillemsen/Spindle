using Engine.Materials;

namespace Engine.Geometry.Interfaces;

public interface Hittable
{
    public HitRecord Hit(Ray r, Interval t);
}

public abstract class HitRecord { }

public class SuccessRecord : HitRecord
{
    public float Length { get; private set; }
    public Vector3 Point { get; private set; }
    public Vector3 Normal { get; private set; }
    public bool FrontFacing { get; private set; }
    public Geometry Geometry { get; private set; }

    public SuccessRecord(float length, Vector3 point, Vector3 outwardNormal, Ray r, Geometry geometry)
    {
        Length = length;
        Point = point;
        Normal = outwardNormal;
        Geometry = geometry;
        SetFaceNormal(outwardNormal, r);
    }

    public void SetFaceNormal(Vector3 outwardNormal, Ray r)
    {
        var ff = Vector3.Dot(r.Direction, outwardNormal) < 0;
        FrontFacing = ff;
        Normal = ff ? outwardNormal : -outwardNormal;
    }
}

public class FailRecord : HitRecord { }