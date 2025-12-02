using UnityEngine;

public class BallSwipeMover : MonoBehaviour
{
    public float powerMultiplier = 0.02f;
    private Rigidbody rb;
    private SwipeDetector swipeDetector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        swipeDetector = FindFirstObjectByType<SwipeDetector>();

        swipeDetector.OnSwipe += MoveBall;
    }

    void MoveBall(SwipeData swipe)
    {
        Vector3 worldDir = new Vector3(swipe.Direction.x, Mathf.Abs(swipe.Direction.x) + Mathf.Abs(swipe.Direction.y), swipe.Direction.y);

        float force = swipe.Speed * powerMultiplier;

        rb.AddForce(worldDir * force, ForceMode.Impulse);
    }
}
