namespace Engine;

public class Interval
{
    public float Min;
    public float Max;

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

    /// <summary>
    /// Method that can expand an interval by some delta.
    /// This is mainly used when calculating edge cases of the bounding boxes.
    /// </summary>
    /// <param name="delta">Amount to expand the interval with</param>
    /// <returns>An interval expanded with the delta parameter</returns>
    public Interval Expand(float delta)
    {
        var padding = delta / 2;
        return new Interval(Min - padding, Max + padding);
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
