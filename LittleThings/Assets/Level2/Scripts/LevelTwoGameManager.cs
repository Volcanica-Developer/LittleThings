using System.Collections;
using UnityEngine;

public class LevelTwoGameManager : MonoBehaviour
{
    [Header("Eyes (paired with holes by index: eye[0] <-> hole[0])")]
    public GameObject[] eyes = new GameObject[4];

    [Header("Holes (paired with eyes by index: eye[i] <-> hole[i])")]
    public GameObject[] holes = new GameObject[4];

    [Header("Cheese Cube")]
    public GameObject cheeseCube;

    [Header("Blink Settings")]
    [Tooltip("Time between eye blink toggle (on/off)")]
    public float blinkInterval = 0.5f;

    int currentIndex;
    Coroutine blinkCoroutine;
    Vector3 cheeseCubeStartPosition;
    Quaternion cheeseCubeStartRotation;
    bool isComplete;

    void Start()
    {
        if (cheeseCube != null)
        {
            cheeseCubeStartPosition = cheeseCube.transform.position;
            cheeseCubeStartRotation = cheeseCube.transform.rotation;
        }

        currentIndex = 0;
        isComplete = false;

        // Register hole triggers (each hole needs Collider with isTrigger = true)
        for (int i = 0; i < holes.Length && i < 4; i++)
        {
            if (holes[i] == null) continue;
            var ht = holes[i].GetComponent<HoleTrigger>();
            if (ht == null)
                ht = holes[i].AddComponent<HoleTrigger>();
            ht.Initialize(i, this, cheeseCube);
        }

        // Ensure only the first eye blinks initially; others are off
        for (int i = 0; i < eyes.Length && i < 4; i++)
        {
            if (eyes[i] != null)
                eyes[i].SetActive(i == 0);
        }

        StartBlinking();
    }

    /// <summary>
    /// Called by HoleTrigger when cheese cube enters a hole. Only accepts if it's the current target hole.
    /// </summary>
    public void OnCheeseCubeInHole(int holeIndex)
    {
        if (isComplete || holeIndex != currentIndex) return;

        StopBlinking();
        if (eyes[currentIndex] != null)
            eyes[currentIndex].SetActive(true);

        StartCoroutine(ResetCheeseCubeAndAdvance());
    }

    void StartBlinking()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        if (currentIndex < eyes.Length && eyes[currentIndex] != null)
            blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    IEnumerator BlinkRoutine()
    {
        while (currentIndex < eyes.Length && eyes[currentIndex] != null)
        {
            bool active = eyes[currentIndex].activeSelf;
            eyes[currentIndex].SetActive(!active);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    IEnumerator ResetCheeseCubeAndAdvance()
    {
        if (cheeseCube != null)
        {
            cheeseCube.SetActive(false);
            cheeseCube.transform.position = cheeseCubeStartPosition;
            cheeseCube.transform.rotation = cheeseCubeStartRotation;
        }

        yield return null;

        if (cheeseCube != null)
            cheeseCube.SetActive(true);

        currentIndex++;

        if (currentIndex >= 4)
        {
            isComplete = true;
            Debug.Log("[LevelTwoGameManager] All four eye-hole pairs completed! Level finished.");
            yield break;
        }

        // Next eye starts blinking
        if (eyes[currentIndex] != null)
            eyes[currentIndex].SetActive(true);
        StartBlinking();
    }
}
