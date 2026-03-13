using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class RandomizedLine
{
    private static RandomNumberGenerator s_rng = new RandomNumberGenerator();

    private static (string, Vector2) GetRandomDirection(string currentDirection = "")
    {
        // Only allow tangential directions if a current direction exists
        (int randomLower, int randomUpper) =
            ((currentDirection == "east" || currentDirection == "west")
                ? (3, 4) // (north, south)
                : ((currentDirection == "north" || currentDirection == "south")
                    ? (1, 2) // (east, west)
                    : (1, 4) // (all directions)
                )
            );

        switch (s_rng.RandiRange(randomLower, randomUpper))
        {
            case 1: return ("east", new Vector2(1, 0));
            case 2: return ("west", new Vector2(-1, 0));
            case 3: return ("north", new Vector2(0, -1));
            default: return ("south", new Vector2(0, 1));
        }
    }

    private static (string, Vector2) GetOppositeDirection(string currentDirection)
    {
        switch (currentDirection)
        {
            case "west": return ("east", new Vector2(1, 0));
            case "east": return ("west", new Vector2(-1, 0));
            case "south": return ("north", new Vector2(0, -1));
            default: return ("south", new Vector2(0, 1));
        }
    }

    private static Vector2 GetDirectionVector(string direction)
    {
        switch (direction)
        {
            case "east": return new Vector2(1, 0);
            case "west": return new Vector2(-1, 0);
            case "north": return new Vector2(0, -1);
            default: return new Vector2(0, 1);
        }
    }

    // Returns a tuple with
    // [0] a randomized line path stored as a list of vertices and
    // [1] the rectangle that encapsulates the line
    public static (List<Vector2>, Rect2) GenerateLineWithShell(int minSegments, int maxSegments, int minSegmentLength, int maxSegmentLength, int segmentPadding)
    {
        Debug.Assert(minSegments > 0 && minSegments <= maxSegments);
        Debug.Assert(minSegmentLength > 1 && minSegmentLength <= maxSegmentLength);
        Debug.Assert(segmentPadding >= 0 && minSegmentLength > segmentPadding + 1);

        // Initialize new line with starting point and a new shell
        List<Vector2> lineJoints = new List<Vector2> { Vector2.Zero };
        List<Vector2> paddedLineJoints = new List<Vector2>() { Vector2.Zero };
        Rect2 aabbShell = new Rect2(0, 0, 0, 0);
        Rect2 lastAabbShell = new Rect2(0, 0, 0, 0);
        Rect2 paddedAabbShell = new Rect2(0, 0, 0, 0);

        // Initialize number of segments
        s_rng.Randomize();
        //s_rng.Seed = 13653972695476564938;
        ulong seed = s_rng.Seed;
        GD.Print($"Seed: {seed}");
        int numberOfSegments = s_rng.RandiRange(minSegments, maxSegments);

        // Initialize helpers
        string lastDirection = "", nextDirection = "";
        Vector2 lastJoint = Vector2.Zero, nextJoint = Vector2.Zero;

        // Loop through and add the segments
        for (int i = 0; i < numberOfSegments; ++i)
        {
            // Create a new random segment
            int nextSegmentLength = s_rng.RandiRange(minSegmentLength, maxSegmentLength) - 1;
            (nextDirection, nextJoint) = GetRandomDirection(lastDirection);
            nextJoint = lastJoint + (nextJoint * nextSegmentLength);

            // If there is a segment padding, calculate its AABB Shell
            if (segmentPadding > 0)
            {
                paddedAabbShell = lastAabbShell;
                paddedAabbShell.Position += new Vector2(-segmentPadding, -segmentPadding);
                paddedAabbShell.Size += new Vector2(segmentPadding * 2, segmentPadding * 2);
            }

            // If the segment intersects the previous AABB shell, or
            // if the segment padding is violated,
            // flip its direction
            if (lastAabbShell.IntersectsSegmentExclusiveOfLastJoint(lastJoint, nextJoint)
                || segmentPadding > 0 && paddedAabbShell.HasPointInclusive(nextJoint))
            {
                (nextDirection, nextJoint) = GetOppositeDirection(nextDirection);
                nextJoint = lastJoint + (nextJoint * nextSegmentLength);
            }

            // Update the line and shell
            lineJoints.Add(nextJoint);
            lastAabbShell = aabbShell;
            aabbShell = aabbShell.ExpandToInclude(nextJoint);

            // Update the history
            lastDirection = nextDirection;
            lastJoint = nextJoint;
        }

        return (lineJoints, aabbShell);
    }
}
