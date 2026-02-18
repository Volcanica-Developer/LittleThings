using UnityEngine;

/// <summary>
/// Keeps the ball within the camera's viewport bounds by clamping its position each frame.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallViewportClamp : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Leave empty to use Camera.main")]
    public Camera cam;

    [Header("Bounds")]
    [Tooltip("Margin from viewport edge (world units). Keeps ball slightly inside so it stays visible.")]
    public float margin = 0.5f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 pos = transform.position;
        GetViewportBoundsAtHeight(pos.y, out float minX, out float maxX, out float minZ, out float maxZ);

        minX += margin;
        maxX -= margin;
        minZ += margin;
        maxZ -= margin;

        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);

        if (pos.x != clampedX || pos.z != clampedZ)
        {
            transform.position = new Vector3(clampedX, pos.y, clampedZ);
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                if (pos.x != clampedX) vel.x = 0f;
                if (pos.z != clampedZ) vel.z = 0f;
                rb.linearVelocity = vel;
            }
        }
    }

    void GetViewportBoundsAtHeight(float worldY, out float minX, out float maxX, out float minZ, out float maxZ)
    {
        var plane = new Plane(Vector3.up, new Vector3(0, worldY, 0));

        minX = float.MaxValue;
        maxX = float.MinValue;
        minZ = float.MaxValue;
        maxZ = float.MinValue;

        var corners = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        foreach (Vector2 vp in corners)
        {
            Ray ray = cam.ViewportPointToRay(vp);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 world = ray.GetPoint(enter);
                if (world.x < minX) minX = world.x;
                if (world.x > maxX) maxX = world.x;
                if (world.z < minZ) minZ = world.z;
                if (world.z > maxZ) maxZ = world.z;
            }
        }

        if (minX > maxX) { minX = -100f; maxX = 100f; }
        if (minZ > maxZ) { minZ = -100f; maxZ = 100f; }
    }
}
