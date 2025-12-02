using UnityEngine;

public struct SwipeData
{
    public Vector2 StartPos;
    public Vector2 EndPos;
    public Vector2 Direction;     // Normalized
    public float Distance;        // In pixels
    public float Duration;        // In seconds
    public float Speed;           // Distance / Duration
    public float Angle;           // 0-360 degrees
}
