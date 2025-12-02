using UnityEngine;
using System;

public class SwipeDetector : MonoBehaviour
{
    public float minSwipeDistance = 20f;

    public event Action<SwipeData> OnSwipeDetected;

    private Vector2 startPos;
    private float startTime;

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            startPos = t.position;
            startTime = Time.time;
        }
        else if (t.phase == TouchPhase.Ended)
        {
            ProcessSwipe(t.position, Time.time - startTime);
        }
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            startTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ProcessSwipe(Input.mousePosition, Time.time - startTime);
        }
    }

    void ProcessSwipe(Vector2 endPos, float duration)
    {
        Vector2 delta = endPos - startPos;
        float distance = delta.magnitude;

        if (distance < minSwipeDistance)
            return;

        SwipeData data = new SwipeData()
        {
            StartPos = startPos,
            EndPos = endPos,
            Direction = delta.normalized,
            Distance = distance,
            Duration = duration,
            Speed = distance / duration,
            Angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
        };

        if (data.Angle < 0)
            data.Angle += 360;

        OnSwipeDetected?.Invoke(data);
    }
}
