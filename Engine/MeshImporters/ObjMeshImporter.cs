using Engine.BoundingBoxes;
using Engine.Geometry;
using Engine.Materials;
using System.Numerics;

namespace Engine.MeshImporters;

public class ObjMeshImporter : MeshImporter
{
    public Material Material { get; }

    /// <inheritdoc />
    public ObjMeshImporter(string filePath, Material material) : base(filePath)
    {
        Material = material;
    }

    /// <inheritdoc />
    /// <remarks>Assumes the file format is: vertices and then faces.</remarks>
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
                    vertices.Add(ParseVector3(line[2..])); // Skip "v "
                    break;

                case 'f':
                    int[] indices = ParseInts(line[2..]); // Skip "f "
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
                default:
                    throw new ArgumentException("This .obj syntax is not supported. Only vertices and faces are.");
            }
        }

        return geometries;
    }
}

public class Triangle : Geometry.Geometry // TODO: this is a placeholder
{
    public Vector3 V1 { get; private set; }
    public Vector3 V2 { get; private set; }
    public Vector3 V3 { get; private set; }

    // Assumes anti-clockwise vertex order
    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Material material) : base(v1, material)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    /// <inheritdoc />
    public override Vector3 GetNormalAt(Vector3 pointOnGeometry) => throw new NotImplementedException();

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection,
        ref IntersectionDebugInfo intersectionDebugInfo) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override IBoundingBox GetBoundingBox() => throw new NotImplementedException();
}
