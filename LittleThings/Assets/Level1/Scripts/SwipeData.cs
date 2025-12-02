using UnityEngine;

public struct SwipeData
{
    public Vector2 StartPos;
    public Vector2 EndPos;
    public Vector2 Direction;   // normalized
    public float Distance;      // px
    public float Duration;      // seconds
    public float Speed;         // px/sec
    public float Angle;         // degrees 0-360
}
