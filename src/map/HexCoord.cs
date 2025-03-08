using Godot;
using System;
using System.Collections.Generic;

// code adapted from https://www.redblobgames.com/grids/hexagons/
// Axial coordinates for flat-topped hexagons (q,r)
// q = x axis, r = y axis (diagonal)
public struct HexCoord
{
    public int Q; // column
    public int R; // row

    // For flat-topped hexagons, these are the 6 directions in axial coordinates
    public static readonly HexCoord[] Neighbors = [
        new(+1, 0), new(+1, -1), new(0, -1),
        new(-1, 0), new(-1, +1), new(0, +1),
    ];

    // Diagonals for flat-topped hexagons in axial coordinates
    public static readonly HexCoord[] Diagonals = [
        new(+2, -1), new(+1, -2), new(-1, -1),
        new(-2, +1), new(-1, +2), new(+1, +1),
    ];

    public HexCoord(int q, int r)
    {
        Q = q;
        R = r;
    }
    // Constructor from Vector2I (useful for grid operations)
    public HexCoord(Vector2I vector)
    {
        Q = vector.X;
        R = vector.Y;
    }

    // Helper to get the S coordinate for cubic operations (not stored)
    private int S => -Q - R;

    // Manhattan distance for axial coordinates
    public int DistanceTo(HexCoord other)
    {
        // In axial, distance is max of the absolute differences across all 3 dimensions
        // including the implicit s-coordinate
        return Math.Max(Math.Max(
            Math.Abs(Q - other.Q),
            Math.Abs(R - other.R)),
            Math.Abs(S - other.S));
    }

    // Appends all hexagonal coordinates within distance N to the provided list
    public void WithinRange(int distance, List<HexCoord> results)
    {
        for (int q = -distance; q <= distance; q++)
        {
            int r1 = Math.Max(-distance, -q - distance);
            int r2 = Math.Min(distance, -q + distance);
            for (int r = r1; r <= r2; r++)
            {
                results.Add(this + new HexCoord(q, r));
            }
        }
    }

    // Returns an enumerable of all hexagonal coordinates within distance N
    public IEnumerable<HexCoord> WithinRange(int distance)
    {
        for (int q = -distance; q <= distance; q++)
        {
            int r1 = Math.Max(-distance, -q - distance);
            int r2 = Math.Min(distance, -q + distance);
            for (int r = r1; r <= r2; r++)
            {
                yield return this + new HexCoord(q, r);
            }
        }
    }

    // Returns all hexes that form a ring at exactly the given radius from the center
    public IEnumerable<HexCoord> Ring(int radius)
    {
        if (radius <= 0)
        {
            if (radius == 0)
            {
                yield return this; // Special case: radius 0 is just the center
            }
            yield break;
        }

        // Start at the position radius away in the first direction
        var hex = this + (Neighbors[4] * radius);

        // For each of the 6 directions
        for (int direction = 0; direction < 6; direction++)
        {
            // Move radius times in each direction, generating the ring
            for (int step = 0; step < radius; step++)
            {
                yield return hex;
                hex = hex + Neighbors[direction];
            }
        }
    }

    // Rounds a fractional hex coordinate to the nearest integer hex coordinate
    public static HexCoord Round(Vector2 frac)
    {
        // Convert to cube coordinates for rounding
        float q = frac.X;
        float r = frac.Y;
        float s = -q - r;

        int qi = (int)Math.Round(q);
        int ri = (int)Math.Round(r);
        int si = (int)Math.Round(s);

        double qDiff = Math.Abs(qi - q);
        double rDiff = Math.Abs(ri - r);
        double sDiff = Math.Abs(si - s);

        // Adjust the component with the largest difference
        if (qDiff > rDiff && qDiff > sDiff)
        {
            qi = -ri - si;
        }
        else if (rDiff > sDiff)
        {
            ri = -qi - si;
        }

        return new HexCoord(qi, ri);
    }

    // Utility method to convert from World coordinates to hex coordinates
    // For flat-topped hexagons
    public static HexCoord FromWorld(Vector2 point)
    {
        float q = (3f / 2f * point.X);
        float r = (MathF.Sqrt(3f) / 2f * point.X - MathF.Sqrt(3f) * point.Y);

        return Round(new Vector2(q, r));
    }

    // Vector with x,y World coordinates for this hex (flat-topped)
    public Vector2 ToWorld()
    {
        float x = (3f / 2f * Q);
        float y = (MathF.Sqrt(3f) / 2f * Q + MathF.Sqrt(3f) * R);
        return new Vector2(x, y);
    }

    public static HexCoord Add(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.Q + b.Q, a.R + b.R);
    }

    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        return Add(a, b);
    }

    public static HexCoord Subtract(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.Q - b.Q, a.R - b.R);
    }

    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        return Subtract(a, b);
    }

    public static HexCoord Multiply(HexCoord a, int k)
    {
        return new HexCoord(a.Q * k, a.R * k);
    }

    public static HexCoord operator *(HexCoord a, int k)
    {
        return Multiply(a, k);
    }

    public static HexCoord operator *(int k, HexCoord a)
    {
        return Multiply(a, k);
    }

    public static HexCoord Divide(HexCoord a, int k)
    {
        return new HexCoord(a.Q / k, a.R / k);
    }

    public static HexCoord operator /(HexCoord a, int k)
    {
        return Divide(a, k);
    }

    public override string ToString()
    {
        return $"({Q},{R})";
    }

    public override bool Equals(object obj)
    {
        return obj is HexCoord coord && Q == coord.Q && R == coord.R;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }
}
