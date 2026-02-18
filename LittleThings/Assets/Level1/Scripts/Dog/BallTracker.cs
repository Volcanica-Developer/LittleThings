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

    public float slowdownSpeed = 10f;

    Rigidbody rb;

    // internal
    bool wasInMotion = false;
    bool slowingDown = false;
    float slowTimer = 0f;
    Vector3 lastSeenVelocity = Vector3.zero;
    Vector3 slowdownAngularVelocity = Vector3.zero;
    Vector3 slowdownLinearVelocity = Vector3.zero;

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
                slowingDown = false;
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
                    slowingDown = true;
                    slowTimer = 0f;
                    Debug.Log("[BallTracker] OnStopped fired");
                    OnStopped?.Invoke(transform.position);
                    slowdownAngularVelocity = rb.angularVelocity;
                    slowdownLinearVelocity = rb.linearVelocity;
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

        if (slowingDown)
        {
            Debug.Log("[BallTracker] Slowing down: " + rb.linearVelocity.sqrMagnitude);
            rb.angularVelocity = Vector3.Lerp(slowdownAngularVelocity, Vector3.zero, slowdownSpeed * Time.deltaTime);
            rb.linearVelocity = Vector3.Lerp(slowdownLinearVelocity, Vector3.zero, slowdownSpeed * Time.deltaTime);
            if(rb.linearVelocity.sqrMagnitude <= 0.1f)
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                slowingDown = false;
                wasInMotion = false;
            }
        }

        // keep last seen vel updated
        lastSeenVelocity = rb.linearVelocity;
    }

    /// <summary>
    /// Call when the ball is dropped at origin (e.g. by the dog). Resets position, velocity and internal state.
    /// Does not invoke OnReturnedToOrigin (caller handles celebration).
    /// </summary>
    public void NotifyDroppedAtOrigin()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (originTransform != null)
            transform.position = originTransform.position;
        wasInMotion = false;
        slowingDown = false;
        slowTimer = 0f;
    }

    // Optional: if other scripts set velocity directly (rb.velocity = ...), call NotifyPotentialThrow() after.
}
