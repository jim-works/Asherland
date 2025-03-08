using Godot;
using System;

public partial class MapGenerator : Node3D
{
    [Export] public PackedScene ChunkScene;
    [Export] public FastNoiseLite Noise;
    [Export] public Vector2[] Spline;

    public override void _Ready()
    {
        base._Ready();
        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                GenerateMesh(new(x, y));
            }
        }
    }


    public void GenerateMesh(Vector2I origin)
    {
        // Prepare heightmap for a 32x32 chunk
        HexTile[] heightmap = new HexTile[HexMeshGenerator.ChunkSize * HexMeshGenerator.ChunkSize];


        // Fill heightmap with values (example: simple sine wave)
        for (int q = 0; q < HexMeshGenerator.ChunkSize; q++)
        {
            for (int r = 0; r < HexMeshGenerator.ChunkSize; r++)
            {
                float noise = Noise.GetNoise2D(q + origin.X * HexMeshGenerator.ChunkSize, r + origin.Y * HexMeshGenerator.ChunkSize);
                float height = 0f;

                // If noise is less than the first point in the spline, use the first point's height
                if (noise <= Spline[0].X)
                {
                    height = Spline[0].Y;
                }
                // If noise is greater than the last point, use the last point's height
                else if (noise >= Spline[^1].X)
                {
                    height = Spline[^1].Y;
                }
                else
                {
                    // Find the two spline points to interpolate between
                    for (int i = 0; i < Spline.Length - 1; i++)
                    {
                        if (noise >= Spline[i].X && noise <= Spline[i + 1].X)
                        {
                            // Linear interpolation: t = (noise - x0) / (x1 - x0)
                            float t = (noise - Spline[i].X) / (Spline[i + 1].X - Spline[i].X);

                            // height = y0 + t * (y1 - y0)
                            height = Spline[i].Y + t * (Spline[i + 1].Y - Spline[i].Y);
                            break;
                        }
                    }
                }
                int index = r * HexMeshGenerator.ChunkSize + q;
                heightmap[index] = new HexTile
                {
                    Height = MathF.Max(0f, height),
                    Color = (height > 10f ? Colors.White
                        : (height > 4f ? Colors.LightGray
                            : (height > 0f ? Colors.PaleGreen
                                : (height > -4f ? Colors.RoyalBlue
                                    : Colors.RoyalBlue.Darkened(0.2f)))))
                };
            }
        }

        Vector2 chunkPosition = new HexCoord((Vector2I)(origin * HexMeshGenerator.ChunkSize)).ToWorld();
        ArrayMesh mesh = HexMeshGenerator.GenerateChunkMesh(heightmap, chunkPosition);

        // Instantiate a new chunk
        var chunk = ChunkScene.Instantiate<MeshInstance3D>();
        chunk.Mesh = mesh;
        AddChild(chunk);
    }
}
