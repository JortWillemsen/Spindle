namespace Engine;

public class Utils
{
    public static float Pi => 3.1415926535897932385f;
    public static float Infinity => float.PositiveInfinity;

    public static float DegreesToRadans(float degrees)
    {
        return degrees * Pi / 180f;
    }

    public static float RandomFloat(Random r)
    {
        return r.NextSingle();
    }

    public static float RandomFloat(Random r, float min, float max)
    {
        return min + (max - min) * RandomFloat(r);
    }

    public static Random GetRandom()
    {
        return new Random();
    }
}