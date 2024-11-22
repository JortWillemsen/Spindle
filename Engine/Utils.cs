using System.Numerics;

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

    public static int RgbToInt(Vector3 rgb)
    { 
        var i = (int) rgb.X;
        i = (i << 8) + (int) rgb.Y;
        i = (i << 8) + (int) rgb.Z;

        return i;
    }
    
    public static Vector3 IntToRgb(int value)
    {
        var red =   ( value >>  0 ) & 255;
        var green = ( value >>  8 ) & 255;
        var blue =  ( value >> 16 ) & 255;
        
        return new Vector3(red, green, blue); 
    }

    public static Vector3 RandomVectorNormalized()
    {
        var random = new Random();

        return new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle());
    }
}