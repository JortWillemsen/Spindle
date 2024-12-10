using System.Numerics;

namespace Engine.MeshImporters;

public abstract class MeshImporter
{
    public string FilePath { get; private set; }

    protected MeshImporter(string filePath)
    {
        FilePath = filePath;
    }

    public abstract List<Geometry.Geometry> Import();

    /// <summary>
    /// Parses a string like "[-]n [-]n [-]n" into a Vector3, where n is a float.
    /// </summary>
    /// <param name="line">The string in said format.</param>
    /// <returns></returns>
    protected static Vector3 ParseVector3(string line)
    {
        float[] coordinates = line.Split(' ').Select(float.Parse).ToArray();
        return new Vector3(coordinates[0], coordinates[1], coordinates[2]);
    }

    /// <summary>
    /// Parses a string like "[-]n( [-n])*" into an array of any length, where n is an integer.
    /// </summary>
    /// <param name="line">The string in said format.</param>
    /// <returns></returns>
    protected static int[] ParseInts(string line) => line.Split(' ').Select(int.Parse).ToArray();
}
