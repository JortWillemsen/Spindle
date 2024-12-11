using Engine.BoundingBoxes;
using Engine.Geometry;
using Engine.Materials;
using System.Numerics;

namespace Engine.MeshImporters;

public class ObjMeshImporter : MeshImporter
{
    public Material Material { get; }

    /// <inheritdoc />
    public ObjMeshImporter(string filePath, Vector3 targetPosition, Material material) : base(filePath, targetPosition)
    {
        Material = material;
    }

    /// <inheritdoc />
    public override List<Geometry.Geometry> Import()
    {
        List<Geometry.Geometry> geometries = new();
        List<Vector3> vertices = new(); // Will be combined into geometries

        using StreamReader textReader = File.OpenText(FilePath);

        while (textReader.ReadLine() is { } line)
        {
            switch (line[0])
            {
                case 'v':
                    if (line[1] is 't' or 'n') continue;
                    vertices.Add(ParseVector3(line[2..]) + TargetPosition); // Skip "v "
                    break;

                case 'f':
                    int[] indices = ParseVertexInts(line[2..]); // Skip "f "
                    switch (indices.Length)
                    {
                        case 3:
                            geometries.Add(new Triangle(
                                 vertices[indices[0] - 1], // obj file format is 1-based
                                 vertices[indices[1] - 1],
                                 vertices[indices[2] - 1],
                                 Material));
                            break;
                        default:
                            throw new NotImplementedException("We do not yet support meshes other than triangles.");
                    }
                    break;
            }
        }

        return geometries;
    }

    /// <summary>
    /// Parses a string like "[-]n(/+*)*( [-n(/+*)*])*" into an array of any length, where n is an integer.
    /// Example: -10/20/30 10/20/30 -1/2/3 => [-10, 10, -1]
    /// </summary>
    /// <param name="line">The string in said format.</param>
    /// <returns></returns>
    protected static int[] ParseVertexInts(string line) => line.Split(' ').Select(x => x.Split('/').First()).Select(int.Parse).ToArray();
}
