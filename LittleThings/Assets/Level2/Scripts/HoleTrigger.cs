using UnityEngine;

/// <summary>
/// Attach to a hole GameObject with a Collider (isTrigger = true).
/// Detects when the cheese cube enters this hole and notifies the LevelTwoGameManager.
/// The cheese cube needs a Collider and (for 3D) a Rigidbody to trigger OnTriggerEnter.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoleTrigger : MonoBehaviour
{
    int holeIndex;
    LevelTwoGameManager manager;
    GameObject cheeseCube;

    public void Initialize(int index, LevelTwoGameManager gameManager, GameObject cube)
    {
        holeIndex = index;
        manager = gameManager;
        cheeseCube = cube;

        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (manager == null || cheeseCube == null) return;
        if (other.gameObject != cheeseCube) return;

        manager.OnCheeseCubeInHole(holeIndex);
    }
}
