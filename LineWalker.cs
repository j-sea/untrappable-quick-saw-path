using Godot;
using System;
using System.Collections.Generic;

public class LineWalker : Node
{
    [Export]
    public int minSegments { get; set; } = 5;

    [Export]
    public int maxSegments { get; set; } = 10;

    [Export]
    public int minSegmentLength { get; set; } = 5;

    [Export]
    public int maxSegmentLength { get; set; } = 15;

    [Export]
    public int segmentPadding { get; set; } = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        const char WALL = '#';
        const char LINE = '+';
        const char JOINT = 'O';
        const char START = 'S';
        const char EXIT = 'E';

        int padding = 5;
        (List<Vector2> lineJoints, Rect2 aabbShell) = RandomizedLine.GenerateLineWithShell(minSegments, maxSegments, minSegmentLength, maxSegmentLength, segmentPadding);

        #region Draw the randomized line

        char[] map =  new char[((int)aabbShell.Size.x + 1 + padding * 2) * ((int)aabbShell.Size.y + 1 + padding * 2)];
        for (int i = 0; i < map.Length; ++i) map[i] = WALL;

        for (int i = 1; i < lineJoints.Count; ++i)
        {
            Vector2 offsetVector = new Vector2(-aabbShell.Position.x + padding, -aabbShell.Position.y + padding);
            Vector2 lastJoint = lineJoints[i - 1] + offsetVector;
            Vector2 nextJoint = lineJoints[i] + offsetVector;

            // Vertical line
            if (lastJoint.x == nextJoint.x)
            {
                int x = (int)lastJoint.x;
                for (int y = (int)Math.Min(lastJoint.y, nextJoint.y), yMax = (int)Math.Max(lastJoint.y, nextJoint.y); y <= yMax; ++y)
                {
                    map[y * ((int)aabbShell.Size.x + 1 + padding * 2) + x] = LINE;
                }
            }
            // Horizontal line
            else
            {
                int y = (int)lastJoint.y;
                for (int x = (int)Math.Min(lastJoint.x, nextJoint.x), xMax = (int)Math.Max(lastJoint.x, nextJoint.x); x <= xMax; ++x)
                {
                    map[y * ((int)aabbShell.Size.x + 1 + padding * 2) + x] = LINE;
                }
            }

            if (i == 1)
            {
                map[(int)lastJoint.y * ((int)aabbShell.Size.x + 1 + padding * 2) + (int)lastJoint.x] = START;
            }
            else
            {
                map[(int)lastJoint.y * ((int)aabbShell.Size.x + 1 + padding * 2) + (int)lastJoint.x] = JOINT;
            }

            if (i == lineJoints.Count - 1)
            {
                map[(int)nextJoint.y * ((int)aabbShell.Size.x + 1 + padding * 2) + (int)nextJoint.x] = EXIT;
            }
            else
            {
                map[(int)nextJoint.y * ((int)aabbShell.Size.x + 1 + padding * 2) + (int)nextJoint.x] = JOINT;
            }
        }

        string mapPrintout = "";
        for (int i = 0, iSize = map.Length; i < iSize; ++i)
        {
            if (i % ((int)aabbShell.Size.x + 1 + padding * 2) == 0)
            {
                mapPrintout += '\n';
            }
            mapPrintout += map[i];
        }
        GD.Print(mapPrintout);

        #endregion
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
