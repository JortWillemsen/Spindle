namespace Engine;

public class Interval
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public Interval()
    {
        Min = -Utils.Infinity;
        Max = Utils.Infinity;
    }

    public Interval(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public static Interval Empty()
    {
        return new Interval(Utils.Infinity, -Utils.Infinity);
    }

    public static Interval Universe()
    {
        return new Interval(-Utils.Infinity, Utils.Infinity);
    }

    public float Size =>  Max - Min;

    public bool Contains(float x)
    {
        return Min <= x && x <= Max;
    }

    public bool Surrounds(float x)
    {
        return Min < x && x < Max;
    }

    public float Clamp(float x)
    { 
        return x < Min ? Min : x > Max ? Max : x;
    }
}