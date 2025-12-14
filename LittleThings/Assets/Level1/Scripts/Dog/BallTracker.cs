using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallTracker : MonoBehaviour
{
    public event Action<Vector3, Vector3> OnThrown; // startPos, initialVelocity
    public event Action<Vector3> OnStopped;
    public event Action OnReturnedToOrigin;

    [Header("Detection")]
    [Tooltip("m/s: consider thrown if speed exceeds this")]
    public float thrownVelocityThreshold = 1.0f;

    [Tooltip("m/s: consider stopped when below this speed")]
    public float stopVelocityThreshold = 0.2f;

    [Tooltip("seconds: must remain slow for this long to count as stopped")]
    public float stopTimeRequired = 0.25f;

    [Tooltip("meters: within this of origin counts as returned")]
    public float originSnapDistance = 0.5f;

    [Header("Origin (assign in inspector)")]
    public Transform originTransform;

    Rigidbody rb;

    // internal
    bool wasInMotion = false;
    float slowTimer = 0f;
    Vector3 lastSeenVelocity = Vector3.zero;

    // small grace window: if a swipe applies force in Update, allow the next FixedUpdate to catch it
    const float graceWindow = 0.06f; // seconds
    float lastPotentialThrowTime = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (originTransform == null)
        {
            GameObject g = new GameObject("BallOrigin");
            g.transform.position = Vector3.zero;
            originTransform = g.transform;
        }
    }

    // Call this from your swipe/throw code immediately after applying force to the rigidbody.
    // This helps the tracker catch throws applied in Update() before the next FixedUpdate.
    public void NotifyPotentialThrow()
    {
        lastPotentialThrowTime = Time.time;
    }

    void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;

        // Debug trace for dev
        // Debug.Log($"[BallTracker] FixedUpdate speed={speed:F3}, wasInMotion={wasInMotion}");

        // RISING EDGE: detect throw when speed crosses threshold OR when a notify was recently received
        bool graceTriggered = (Time.time - lastPotentialThrowTime) <= graceWindow && speed > (stopVelocityThreshold * 0.9f);
        if (!wasInMotion)
        {
            if (speed >= thrownVelocityThreshold || graceTriggered)
            {
                wasInMotion = true;
                slowTimer = 0f;
                lastSeenVelocity = rb.linearVelocity;
                Vector3 startPos = transform.position;
                Vector3 initialVel = rb.linearVelocity;
                Debug.Log($"[BallTracker] OnThrown fired (speed={speed:F2})");
                OnThrown?.Invoke(startPos, initialVel);
            }
        }

        // If in motion, check for stop condition
        if (wasInMotion)
        {
            if (speed <= stopVelocityThreshold)
            {
                slowTimer += Time.fixedDeltaTime;
                if (slowTimer >= stopTimeRequired)
                {
                    wasInMotion = false;
                    slowTimer = 0f;
                    Debug.Log("[BallTracker] OnStopped fired");
                    OnStopped?.Invoke(transform.position);

                    // returned to origin?
                    if (originTransform != null &&
                        Vector3.Distance(transform.position, originTransform.position) <= originSnapDistance)
                    {
                        Debug.Log("[BallTracker] OnReturnedToOrigin fired");
                        OnReturnedToOrigin?.Invoke();
                    }
                }
            }
            else
            {
                slowTimer = 0f;
            }
        }

        // keep last seen vel updated
        lastSeenVelocity = rb.linearVelocity;
    }

    // Optional: if other scripts set velocity directly (rb.velocity = ...), call NotifyPotentialThrow() after.
}
