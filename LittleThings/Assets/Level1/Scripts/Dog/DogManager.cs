using System.Collections;
using UnityEngine;

public class DogManager : MonoBehaviour
{
    public enum DogState { Idle, Curious, Playful, Excited, Fetching, Returning }

    [Header("References")]
    public BallTracker ballTracker;
    public FootprintAnimator footprintAnimator;
    public SoundController sounds;

    [Header("Dog Movement (simulation)")]
    public float walkSpeed = 1.2f;
    public float runSpeed = 4.5f;
    public float turnSpeed = 6f;

    [Header("Behavior Timers")]
    public float idleToCuriousAfter = 6f;
    public float curiousDurationMin = 4f;
    public float curiousDurationMax = 9f;
    public float playfulChancePerCycle = 0.25f;

    [Header("Fetch Settings")]
    public float fetchApproachDistance = 0.4f;
    public float pickupScrambleRadius = 0.4f;

    [Header("Randomness (Option B)")]
    public float pathNoise = 0.6f;

    // internal state
    DogState state = DogState.Idle;
    Vector3 simulatedPosition;
    Vector3 simulatedFacing = Vector3.forward;
    Vector3 idleCenter;

    Coroutine currentCoroutine;

    void Start()
    {
        idleCenter = ballTracker ? ballTracker.originTransform.position : Vector3.zero;
        simulatedPosition = idleCenter;

        footprintAnimator.ResetStepCycle(simulatedPosition);

        ballTracker.OnThrown += HandleBallThrown;
        ballTracker.OnStopped += HandleBallStopped;
        ballTracker.OnReturnedToOrigin += HandleBallReturnedToOrigin;

        SetState(DogState.Idle);
    }

    void OnDestroy()
    {
        ballTracker.OnThrown -= HandleBallThrown;
        ballTracker.OnStopped -= HandleBallStopped;
        ballTracker.OnReturnedToOrigin -= HandleBallReturnedToOrigin;
    }

    void SetState(DogState newState)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        state = newState;

        switch (state)
        {
            case DogState.Idle: currentCoroutine = StartCoroutine(IdleRoutine()); break;
            case DogState.Curious: currentCoroutine = StartCoroutine(CuriousRoutine()); break;
            case DogState.Playful: currentCoroutine = StartCoroutine(PlayfulRoutine()); break;
            case DogState.Excited: currentCoroutine = StartCoroutine(ExcitedRoutine()); break;
        }
    }

    // ---------------- EVENT HANDLERS ----------------

    void HandleBallThrown(Vector3 startPos, Vector3 velocity)
    {
        sounds?.PlayBark(1f);
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FetchRoutine());
    }

    void HandleBallStopped(Vector3 pos) { }
    void HandleBallReturnedToOrigin()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ReturnCelebrateRoutine());
    }

    // ---------------- BEHAVIOR ROUTINES ----------------

    IEnumerator IdleRoutine()
    {
        float timer = 0f;

        while (true)
        {
            Vector2 rnd = Random.insideUnitCircle * 5.6f;
            Vector3 target = idleCenter + new Vector3(rnd.x, 0, rnd.y);

            yield return StartCoroutine(MoveToRoutine(target, walkSpeed));

            if (Random.value < 0.35f)
            {
                footprintAnimator.SpawnCirclePattern(simulatedPosition, 1.25f, 10, 1.2f);
                sounds?.PlayYip(0.6f);
                footprintAnimator.PauseSteps(0.4f);
                yield return new WaitForSeconds(0.6f);
            }

            yield return new WaitForSeconds(10.4f);

            timer += 1f;
            //if (timer > idleToCuriousAfter) { SetState(DogState.Curious); yield break; }
            //if (Random.value < playfulChancePerCycle) { SetState(DogState.Playful); yield break; }
        }
    }

    IEnumerator CuriousRoutine()
    {
        float duration = Random.Range(curiousDurationMin, curiousDurationMax);
        float t = 0f;

        while (t < duration)
        {
            Vector2 rnd = Random.insideUnitCircle * (2f + Random.value * 4f);
            Vector3 target = idleCenter + new Vector3(rnd.x, 0, rnd.y);

            yield return StartCoroutine(MoveToRoutine(target, walkSpeed * 0.9f));

            sounds?.PlaySniff(0.5f);
            footprintAnimator.SpawnScramble(simulatedPosition, 0.25f, 5);
            footprintAnimator.PauseSteps(0.25f);
            yield return new WaitForSeconds(1f);

            t += 1f;
        }

        //SetState(DogState.Idle);
    }

    IEnumerator PlayfulRoutine()
    {
        float end = Time.time + (2f + Random.value * 2.5f);

        while (Time.time < end)
        {
            footprintAnimator.SpawnCirclePattern(simulatedPosition, 0.45f, 14, 0.9f);
            footprintAnimator.PauseSteps(0.2f);

            sounds?.PlayYip(0.8f);
            yield return new WaitForSeconds(1f);

            footprintAnimator.SpawnScramble(simulatedPosition, 0.6f, 8);
            footprintAnimator.PauseSteps(0.2f);

            yield return new WaitForSeconds(0.8f);
        }

        //SetState(DogState.Idle);
    }

    IEnumerator ExcitedRoutine()
    {
        sounds?.PlayBark(1f);
        footprintAnimator.SpawnScramble(simulatedPosition, 0.25f, 8);
        footprintAnimator.PauseSteps(0.2f);

        yield return new WaitForSeconds(0.4f);

        currentCoroutine = StartCoroutine(FetchRoutine());
    }

    IEnumerator FetchRoutine()
    {
        state = DogState.Fetching;

        float timeout = Time.time + 6f;

        while (Time.time < timeout)
        {
            Vector3 ballPos = ballTracker.transform.position;
            Vector3 toBall = (ballPos - simulatedPosition).WithY(0);
            float dist = toBall.magnitude;

            Vector3 noisyDir =
                (toBall.normalized +
                 Random.insideUnitSphere.WithY(0) * pathNoise * 0.15f).normalized;

            simulatedFacing =
                Vector3.Slerp(simulatedFacing, noisyDir, Time.deltaTime * turnSpeed);

            simulatedPosition += simulatedFacing * runSpeed * Time.deltaTime;

            footprintAnimator.StepAlongPath(simulatedPosition, simulatedFacing);

            if (dist <= fetchApproachDistance)
            {
                sounds?.PlaySniff(0.8f);

                footprintAnimator.SpawnScramble(ballPos, pickupScrambleRadius, 10);
                footprintAnimator.PauseSteps(0.2f);

                yield return new WaitForSeconds(0.4f);

                footprintAnimator.SpawnCirclePattern(ballPos, 0.25f, 10, 0.8f);
                footprintAnimator.PauseSteps(0.2f);

                sounds?.PlayBark(0.9f);

                StartCoroutine(ReturningRoutine());
                yield break;
            }

            yield return null;
        }

        //SetState(DogState.Curious);
    }

    IEnumerator ReturningRoutine()
    {
        state = DogState.Returning;

        while (Vector3.Distance(simulatedPosition, idleCenter) > 0.35f)
        {
            Vector3 dir = (idleCenter - simulatedPosition).WithY(0).normalized;

            Vector3 noisy =
                (dir + Random.insideUnitSphere.WithY(0) * 0.15f).normalized;

            simulatedFacing =
                Vector3.Slerp(simulatedFacing, noisy, Time.deltaTime * turnSpeed);

            simulatedPosition += simulatedFacing * (runSpeed * 0.85f) * Time.deltaTime;

            footprintAnimator.StepAlongPath(simulatedPosition, simulatedFacing);

            yield return null;
        }

        sounds?.PlayThud(1f);

        footprintAnimator.SpawnScramble(simulatedPosition, 0.25f, 6);
        footprintAnimator.PauseSteps(0.2f);

        footprintAnimator.SpawnCirclePattern(simulatedPosition, 0.18f, 8, 0.6f);
        footprintAnimator.PauseSteps(0.2f);

        yield return new WaitForSeconds(1f);

        //SetState(DogState.Idle);
    }

    IEnumerator ReturnCelebrateRoutine()
    {
        sounds?.PlayBark(1f);

        footprintAnimator.SpawnCirclePattern(idleCenter, 0.35f, 14, 0.9f);
        footprintAnimator.PauseSteps(0.2f);

        footprintAnimator.SpawnScramble(idleCenter, 0.5f, 8);
        footprintAnimator.PauseSteps(0.2f);

        yield return new WaitForSeconds(1.2f);

        //SetState(DogState.Idle);
    }

    // ---------------- MOVEMENT HELPER ----------------

    IEnumerator MoveToRoutine(Vector3 target, float speed)
    {
        while (Vector3.Distance(simulatedPosition, target) > 0.2f)
        {
            Vector3 to = (target - simulatedPosition).WithY(0);
            Vector3 dir = to.normalized;

            Vector3 noisy =
                (dir + Random.insideUnitSphere.WithY(0) * pathNoise * 0.1f).normalized;

            simulatedFacing =
                Vector3.Slerp(simulatedFacing, noisy, Time.deltaTime * turnSpeed);

            simulatedPosition += simulatedFacing * speed * Time.deltaTime;

            footprintAnimator.StepAlongPath(simulatedPosition, simulatedFacing);

            yield return null;
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        DebugDrawSimulatedDog();
        //Debug.Log($"DogPos: {simulatedPosition}, Facing: {simulatedFacing}");
    }

    void DebugDrawSimulatedDog()
    {
        Debug.DrawLine(simulatedPosition + Vector3.up * 0.05f,
                       simulatedPosition + simulatedFacing * 0.3f + Vector3.up * 0.05f,
                       Color.green);

        Debug.DrawLine(idleCenter + Vector3.up * 0.03f,
                       idleCenter + Vector3.forward * 0.2f + Vector3.up * 0.03f,
                       Color.yellow);
    }
#endif
}

public static class DogManagerHelpers
{
    public static Vector3 WithY(this Vector3 v, float y = 0f)
    {
        v.y = y;
        return v;
    }
}
