using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FootprintAnimator : MonoBehaviour
{
    [Header("Footprint Prefab")]
    public GameObject footprintPrefab; // simple sprite/quad that faces +Z (toe)
    public Transform footprintParent;

    [Header("Footprint Settings")]
    public float baseSpacing = 0.35f;     // reference spacing (m) at walk
    public float minSpacing = 0.15f;
    public float maxSpacing = 0.6f;
    public float footprintLife = 6f;
    public float fadeDuration = 1.5f;
    public float lateralOffset = 0.18f;   // distance from center to foot

    [Header("Timing / Rhythm")]
    public float minStepInterval = 0.09f; // fastest step (seconds)
    public float maxStepInterval = 0.36f; // slowest step (seconds)
    public float walkReference = 1.5f;    // m/s reference
    public float runReference = 4.0f;     // m/s reference
    public float minMoveSpeedToStep = 0.05f; // don't step if almost stationary
    public float minMovement = 0.03f;   // must move 3 cm to count
    public float minTime = 0.15f;      // min time between steps

    // internal state
    bool leftNext = true;
    Vector3 lastSpawnPos;
    Vector3 lastPosition;         // to compute instantaneous speed
    float lastStepTime = -10f;
    float stepInterval = 0.22f;
    bool initialized = false;

    private void Awake()
    {
        if (footprintParent == null)
        {
            GameObject p = new GameObject("Footprints");
            p.transform.SetParent(transform, false);
            footprintParent = p.transform;
        }
    }

    /// <summary>
    /// Must be called to initialize/reset the footstep cycle position.
    /// </summary>
    public void ResetStepCycle(Vector3 pos)
    {
        lastSpawnPos = pos;
        lastPosition = pos;
        leftNext = true;
        lastStepTime = Time.time; // small delay before next step
        initialized = true;
    }

    /// <summary>
    /// Pause automatic steps for 'duration' seconds (used after special patterns)
    /// </summary>
    public void PauseSteps(float duration)
    {
        lastStepTime = Time.time + duration;
    }

    /// <summary>
    /// Call continuously while "dog" is moving. worldDirection must be normalized horizontal
    /// </summary>
    public void StepAlongPath(Vector3 worldPosition, Vector3 worldDirection)
    {
        if (!initialized)
        {
            ResetStepCycle(worldPosition);
            return;
        }

        // movement check
        float distMoved = Vector3.Distance(worldPosition, lastSpawnPos);
        if (distMoved < minMovement)
            return; // NOT enough movement, DO NOT step

        // time check
        if (Time.time - lastStepTime < minTime)
            return;

        // direction check
        if (worldDirection.sqrMagnitude < 0.001f)
            return;

        worldDirection = worldDirection.normalized;

        SpawnFootprint(worldPosition, worldDirection, leftNext);
        leftNext = !leftNext;

        lastSpawnPos = worldPosition;
        lastStepTime = Time.time;
    }

    void SpawnFootprint(Vector3 centerPos, Vector3 forward, bool left)
    {
        if (footprintPrefab == null) return;

        // compute right vector (based on forward)
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 offset = right * (left ? -lateralOffset : lateralOffset);

        Vector3 spawnPos = centerPos + offset + Vector3.up * 0.01f; // tiny y-off to avoid z-fighting
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

        GameObject go = Instantiate(footprintPrefab, spawnPos, rot, footprintParent);
        var fp = go.GetComponent<Footprint>();
        if (fp == null) fp = go.AddComponent<Footprint>();
        fp.Setup(footprintLife, fadeDuration);
    }

    // Special patterns -------------------------------------------------
    public void SpawnCirclePattern(Vector3 center, float radius, int steps = 12, float duration = 1.5f)
    {
        StartCoroutine(CirclePatternCoroutine(center, radius, steps, duration));
    }

    IEnumerator CirclePatternCoroutine(Vector3 center, float radius, int steps, float duration)
    {
        if (steps <= 0) yield break;
        float wait = Mathf.Max(0.01f, duration / steps);

        for (int i = 0; i < steps; i++)
        {
            float ang = (float)i / steps * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 pos = center + dir * radius;
            SpawnFootprint(pos, dir, (i % 2 == 0));
            yield return new WaitForSeconds(wait);
        }

        // prevent immediate automatic step after this manual pattern
        PauseSteps(wait + 0.05f);
        // also set lastSpawnPos to last pattern position
        lastSpawnPos = center + new Vector3(Mathf.Cos(0) * radius, 0, Mathf.Sin(0) * radius);
        lastPosition = lastSpawnPos;
    }

    public void SpawnScramble(Vector3 center, float radius, int blips = 6)
    {
        StartCoroutine(ScrambleCoroutine(center, radius, blips));
    }

    IEnumerator ScrambleCoroutine(Vector3 center, float radius, int blips)
    {
        blips = Mathf.Max(1, blips);
        for (int i = 0; i < blips; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(rnd.x, 0f, rnd.y);
            Vector3 dir = Random.insideUnitSphere;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
            dir.Normalize();
            SpawnFootprint(pos, dir, Random.value > 0.5f);
            yield return new WaitForSeconds(0.06f + Random.value * 0.12f);
        }

        // pause a short moment so StepAlongPath won't immediately spawn a step
        PauseSteps(0.12f + Random.value * 0.12f);

        // update lastSpawnPos so spacing logic won't fire immediately
        lastSpawnPos = center;
        lastPosition = center;
    }
}
