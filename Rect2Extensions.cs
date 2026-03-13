using Godot;
using System;

public static class Rect2Extensions
{
    private static bool SegmentsIntersectExcludingLastJoint(Vector2 lastJoint, Vector2 nextJoint, Vector2 a1, Vector2 a2)
    {
        var intersection = Geometry.SegmentIntersectsSegment2d(lastJoint, nextJoint, a1, a2);
        return intersection != null && (Vector2)intersection != lastJoint;
    }

    public static bool HasPointInclusive(this Rect2 rect, Vector2 point)
    {
        return point.x >= rect.Position.x
            && point.x <= rect.Position.x + rect.Size.x
            && point.y >= rect.Position.y
            && point.y <= rect.Position.y + rect.Size.y;
    }

    public static Rect2 ExpandToInclude(this Rect2 rect, Vector2 point)
    {
        Vector2 min = new Vector2(
            Math.Min(rect.Position.x, point.x),
            Math.Min(rect.Position.y, point.y)
        );

        Vector2 max = new Vector2(
            Math.Max(rect.End.x, point.x),
            Math.Max(rect.End.y, point.y)
        );

        return new Rect2(min, max - min);
    }

    public static bool IntersectsSegment(this Rect2 rect, Vector2 a, Vector2 b)
    {
        // Check if either endpoint is inside the rectangle
        if (rect.HasPointInclusive(a) || rect.HasPointInclusive(b))
            return true;

        // Define rectangle corners
        Vector2 p1 = rect.Position;
        Vector2 p2 = new Vector2(rect.Position.x + rect.Size.x, rect.Position.y);
        Vector2 p3 = rect.Position + rect.Size;
        Vector2 p4 = new Vector2(rect.Position.x, rect.Position.y + rect.Size.y);

        // Check for intersection with each edge
        return Geometry.SegmentIntersectsSegment2d(a, b, p1, p2) != null ||
               Geometry.SegmentIntersectsSegment2d(a, b, p2, p3) != null ||
               Geometry.SegmentIntersectsSegment2d(a, b, p3, p4) != null ||
               Geometry.SegmentIntersectsSegment2d(a, b, p4, p1) != null;
    }

    public static bool IntersectsSegmentExclusiveOfLastJoint(this Rect2 rect, Vector2 lastJoint, Vector2 nextJoint)
    {
        // Check if the next endpoint is inside the rectangle
        if (rect.HasPointInclusive(nextJoint))
            return true;

        // Define rectangle corners
        Vector2 p1 = rect.Position;
        Vector2 p2 = new Vector2(rect.Position.x + rect.Size.x, rect.Position.y);
        Vector2 p3 = rect.Position + rect.Size;
        Vector2 p4 = new Vector2(rect.Position.x, rect.Position.y + rect.Size.y);

        // Check for intersection with each edge, excluding the AABB edge that has the lastJoint on it
        return SegmentsIntersectExcludingLastJoint(lastJoint, nextJoint, p1, p2) ||
               SegmentsIntersectExcludingLastJoint(lastJoint, nextJoint, p2, p3) ||
               SegmentsIntersectExcludingLastJoint(lastJoint, nextJoint, p3, p4) ||
               SegmentsIntersectExcludingLastJoint(lastJoint, nextJoint, p4, p1);
    }
}