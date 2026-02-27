using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves a 3D box in the direction of arrow key presses (no rotation).
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
            TryTip(Vector3.right);
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
            TryTip(Vector3.left);
        else if (keyboard.upArrowKey.wasPressedThisFrame)
            TryTip(Vector3.forward);
        else if (keyboard.downArrowKey.wasPressedThisFrame)
            TryTip(Vector3.back);
    }

    void TryTip(Vector3 moveDirection)
    {
        StartCoroutine(TipCoroutine(moveDirection));
    }

    IEnumerator TipCoroutine(Vector3 moveDirection)
    {
        isTipping = true;

        Bounds bounds = boxCollider != null ? boxCollider.bounds : new Bounds(transform.position, transform.lossyScale);
        Vector3 extents = bounds.extents;

        // Movement: move in direction (half size) + drop (half height)
        float moveDist = Mathf.Abs(moveDirection.x) > 0.01f ? extents.x : extents.z;
        Vector3 moveOffset = moveDirection.normalized * moveDist + Vector3.down * extents.y;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + moveOffset;

        float elapsed = 0f;

        while (elapsed < tipDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tipDuration);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.position = endPos;
        isTipping = false;
    }
}
