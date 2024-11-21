using System.Collections;
using Engine.Geometry.Interfaces;

namespace Engine.Geometry;

public class Scene : Hittable
{
    public List<Hittable> Hittables { get; private set; }

    public Scene(params Hittable[] hittables)
    {
        Hittables = hittables.ToList();
    }

    public void Add(Hittable obj)
    {
        Hittables.Add(obj);
    }

    public HitRecord Hit(Ray r, Interval rayT)
    {
        HitRecord record = new FailRecord();

        var closest = rayT.Max;

        foreach (var obj in Hittables)
        {
            if (obj.Hit(r, new Interval(rayT.Min, closest)) is SuccessRecord success)
            {
                closest = success.Length;

                record = success;
            }
        }

        return record;
    }
}