namespace Engine;

public class Vector3
{
    private readonly float[] _e = new float[3] { 0, 0, 0};

    public float X() { return _e[0]; }
    public float Y() { return _e[1]; }
    public float Z() { return _e[2]; }

    public Vector3(float x, float y, float z)
    {
        this._e[0] = x;
        this._e[1] = y;
        this._e[2] = z;
    }

    public static Vector3 Zero()
    {
        return new Vector3(0f, 0f, 0f);
    }

    public static Vector3 Random()
    {
        var r = Utils.GetRandom();

        return new Vector3(Utils.RandomFloat(r), Utils.RandomFloat(r), Utils.RandomFloat(r));
    }
    
    public static Vector3 Random(float min, float max)
    {
        var r = Utils.GetRandom();
        
        return new Vector3(Utils.RandomFloat(r, min, max), Utils.RandomFloat(r, min, max), Utils.RandomFloat(r, min, max));
    }

    public static float Dot(Vector3 a, Vector3 b)
    {
        return
            a.X() * b.X() +
            a.Y() * b.Y() +
            a.Z() * b.Z();
    }
    
    public Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.Y() * b.Z() - a.Z() * b.Y(),
            a.Z() * b.X() - a.X() * b.Z(),
            a.X() * b.Y() - a.Y() * b.X());
    }

    public Vector3 Reflect(Vector3 normal)
    {
        return this - 2 * Dot(this, normal) * normal;
    }
    
    public Vector3 Refract(Vector3 n, float rIndex) {
        var cosTheta = Math.Min(Vector3.Dot(-this, n), 1f);
        var rOutPerp =  rIndex * (this + cosTheta * n);
        
        var rOutParallel = - (float) Math.Sqrt(Math.Abs(1f - rOutPerp.LengthSquared())) * n;
        return rOutPerp + rOutParallel;
    }
    
    public bool NearZero()
    {
        const float s = 1e-8f;
        return Math.Abs(_e[0]) < s && Math.Abs(_e[1]) < s && Math.Abs(_e[2]) < s;
    }

    public static Vector3 Unit(Vector3 a)
    {
        return a / a.Length();
    }

    public static Vector3 UnitRandom()
    {
        // Randomly create a vector, if it lies within the unit sphere, we normalize it.
        while (true)
        {
            var p = Random(-1, 1);
            var lengthSquared = p.LengthSquared();
            if (1e-80 < lengthSquared && lengthSquared <= 1)
            {
                return p / (float) Math.Sqrt(lengthSquared);
            }
        }
    }

    public static Vector3 UnitRandomOnHemisphere(Vector3 normal)
    {
        var onUnitSphere = UnitRandom();

        if (Vector3.Dot(onUnitSphere, normal) > 0f)
            return onUnitSphere;
        
        return -onUnitSphere;
    }
    
    public static Vector3 operator + (Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.X() + b.X(),
            a.Y() + b.Y(),
            a.Z() + b.Z());
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.X() - b.X(),
            a.Y() - b.Y(),
            a.Z() - b.Z());
    }

    public static Vector3 operator -(Vector3 a)
    {
        return a * -1;
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.X() * b.X(),
            a.Y() * b.Y(),
            a.Z() * b.Z());
    }
    
    public static Vector3 operator *(Vector3 a, float d)
    {
        return new Vector3(
            a.X() * d,
            a.Y() * d,
            a.Z() * d);
    }
    
    public static Vector3 operator *(float d, Vector3 a)
    {
        return a * d;
    }

    public static Vector3 operator /(Vector3 a, float d)
    {
        return (1 / d) * a;
    }
    
    public static Vector3 operator /(float d, Vector3 a)
    {
        return a * (1 / d);
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSquared());
    }

    public float LengthSquared()
    {
        return _e[0] * _e[0] + _e[1] * _e[1] + _e[2] * _e[2];
    }

    public override string ToString()
    {
        return _e[0] + " " + _e[1] + " " + _e[2];
    }
}

