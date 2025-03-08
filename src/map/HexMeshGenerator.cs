using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public struct HexTile
{
    public float Height;
    public Color Color;
}

public static class HexMeshGenerator
{
    // Chunk size (32x32 grid)
    public const int ChunkSize = 32;

    // Hexagon constants
    public static float HexSize = 1.0f;
    public static float HexHeight = 0.5f;

    // Cached values for performance
    private static readonly float HexWidth;
    private static readonly float HexDepth;
    private static readonly Vector3[] HexCornerOffsets;

    static HexMeshGenerator()
    {
        // For flat-topped hexes (using axial coordinates)
        HexWidth = HexSize * 2f;
        HexDepth = HexSize * Mathf.Sqrt(3f);

        // Pre-calculate corner offsets
        HexCornerOffsets = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Pi / 3f * i;
            HexCornerOffsets[i] = new Vector3(
                HexSize * Mathf.Cos(angle),
                0f,
                HexSize * Mathf.Sin(angle)
            );
        }
    }

    /// <summary>
    /// Generate a mesh for a chunk of hexagons based on height data
    /// </summary>
    /// <param name="heightmap">Height values for each hex in the chunk</param>
    /// <param name="chunkPosition">World position of the chunk</param>
    /// <returns>Generated ArrayMesh</returns>
    public static ArrayMesh GenerateChunkMesh(ReadOnlySpan<HexTile> heightmap, Vector2 chunkPosition)
    {
        if (heightmap.Length != ChunkSize * ChunkSize)
        {
            throw new ArgumentException($"Heightmap must contain {ChunkSize * ChunkSize} elements");
        }

        // Calculate vertices capacity (approximate):
        // Each hex has: 1 center vertex + 6 top vertices + 6 side bottom vertices
        // Not all vertices need to be duplicated since we're in a grid
        int estimatedVertexCount = ChunkSize * ChunkSize * 13;

        // Lists to store mesh data
        var vertices = new List<Vector3>(estimatedVertexCount);
        var normals = new List<Vector3>(estimatedVertexCount);
        var indices = new List<int>(ChunkSize * ChunkSize * 30); // 10 triangles per hex (approximate)
        var colors = new List<Color>(estimatedVertexCount);

        // Process each hex in the chunk
        for (int q = 0; q < ChunkSize; q++)
        {
            for (int r = 0; r < ChunkSize; r++)
            {
                int index = r * ChunkSize + q;
                float height = heightmap[index].Height;

                Vector2 hexCoord = new HexCoord(q, r).ToWorld();
                Vector3 centerPos = new(hexCoord.X, height * HexHeight, hexCoord.Y);
                centerPos.X += chunkPosition.X;
                centerPos.Z += chunkPosition.Y;


                AddHexToMesh(centerPos, height, heightmap[index].Color, vertices, normals, indices, colors);
            }
        }

        // Create the mesh
        var arrMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = colors.ToArray();

        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return arrMesh;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddHexToMesh(Vector3 center, float height, Color color, List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Color> colors)
    {
        // Start of the current hex's vertices in the array
        int baseIndex = vertices.Count;

        // Add center vertex of the top face
        vertices.Add(center);
        colors.Add(color);
        normals.Add(Vector3.Up);

        // Add top perimeter vertices
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(center + HexCornerOffsets[i]);
            colors.Add(color);
            normals.Add(Vector3.Up);
        }

        // Add side bottom vertices
        for (int i = 0; i < 6; i++)
        {
            Vector3 bottomVertex = center + HexCornerOffsets[i];
            bottomVertex.Y = 0; // Bottom of the hex
            vertices.Add(bottomVertex);
            colors.Add(color);
            normals.Add(Vector3.Up); // Will be overridden below
        }

        // Add top face triangles (center to each edge)
        for (int i = 0; i < 6; i++)
        {
            indices.Add(baseIndex); // Center
            indices.Add(baseIndex + 1 + i);
            indices.Add(baseIndex + 1 + ((i + 1) % 6));
        }

        // Add side faces (each is a quad made of 2 triangles)
        for (int i = 0; i < 6; i++)
        {
            int nextI = (i + 1) % 6;

            // Top vertices of this side
            int topA = baseIndex + 1 + i;
            int topB = baseIndex + 1 + nextI;

            // Bottom vertices of this side
            int bottomA = baseIndex + 7 + i;
            int bottomB = baseIndex + 7 + nextI;

            // First triangle
            indices.Add(topA);
            indices.Add(bottomA);
            indices.Add(topB);

            // Second triangle
            indices.Add(bottomA);
            indices.Add(bottomB);
            indices.Add(topB);

            // Calculate side normal
            Vector3 sideVector1 = vertices[topB] - vertices[topA];
            Vector3 sideVector2 = vertices[bottomA] - vertices[topA];
            Vector3 normal = sideVector1.Cross(sideVector2).Normalized();

            // Update normals for the side vertices
            normals[bottomA] = normal;
            // We don't override the top vertices' normals as they're already set to Up
        }
    }
}
