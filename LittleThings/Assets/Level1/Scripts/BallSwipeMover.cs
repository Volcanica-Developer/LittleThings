using UnityEngine;

public class BallSwipeMover : MonoBehaviour
{
    public float powerMultiplier = 0.02f;
    private SwipeDetector detector;
    private Rigidbody rb;

    void Start()
    {
        detector = FindFirstObjectByType<SwipeDetector>();
        rb = GetComponent<Rigidbody>();

        detector.OnSwipeDetected += OnSwipe;
    }

    void OnSwipe(SwipeData swipe)
    {
        // Convert 2D direction into world (camera downwards)
        Vector3 worldDir = new Vector3(swipe.Direction.x, 0, swipe.Direction.y);

        // Speed = pixels per second * multiplier
        float force = swipe.Speed * powerMultiplier;

        rb.AddForce(worldDir * force, ForceMode.Impulse);
    }
}
