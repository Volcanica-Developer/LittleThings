using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tips a 3D box in the direction of arrow key presses. Simple mechanism: rotate + move in the same direction,
/// with matching speeds so it looks like a natural tip.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TippableBox : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Duration of one 90° tip in seconds")]
    public float tipDuration = 0.35f;

    BoxCollider boxCollider;
    bool isTipping;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (isTipping) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame)
            TryTip(Vector3.right, -Vector3.forward);   // move +X, rot -Z
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
            TryTip(Vector3.left, Vector3.forward);    // move -X, rot +Z
        else if (keyboard.upArrowKey.wasPressedThisFrame)
            TryTip(Vector3.forward, Vector3.right);   // move +Z, rot +X
        else if (keyboard.downArrowKey.wasPressedThisFrame)
            TryTip(Vector3.back, -Vector3.right);     // move -Z, rot -X
    }

    void TryTip(Vector3 moveDirection, Vector3 rotationAxis)
    {
        StartCoroutine(TipCoroutine(moveDirection, rotationAxis));
    }

    IEnumerator TipCoroutine(Vector3 moveDirection, Vector3 rotationAxis)
    {
        isTipping = true;

        Bounds bounds = boxCollider != null ? boxCollider.bounds : new Bounds(transform.position, transform.lossyScale);
        Vector3 extents = bounds.extents;

        // Movement: move in direction (half size) + drop (half height)
        float moveDist = Mathf.Abs(moveDirection.x) > 0.01f ? extents.x : extents.z;
        Vector3 moveOffset = moveDirection.normalized * moveDist + Vector3.down * extents.y;

        Quaternion startRot = transform.rotation;
        Vector3 startPos = transform.position;
        // Use world-space axes: X and Z only — no Y rotation
        Vector3 axisWorld = rotationAxis.normalized;
        Quaternion endRot = startRot * Quaternion.AngleAxis(90f, axisWorld);
        Vector3 endPos = startPos + moveOffset;

        float elapsed = 0f;

        while (elapsed < tipDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tipDuration);
            t = t * t * (3f - 2f * t); // smooth step — same t for rotate & move

            // Interpolate angle on the axis instead of Slerp — keeps rotation on X/Z only, no Y drift
            float angle = 90f * t;
            transform.rotation = startRot * Quaternion.AngleAxis(angle, axisWorld);
            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.rotation = endRot;
        transform.position = endPos;
        isTipping = false;
    }
}
