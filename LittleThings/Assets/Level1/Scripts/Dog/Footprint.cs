using UnityEngine;

public class Footprint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    float life;
    float fadeDuration;
    float spawnTime;

    public void Setup(float lifeSeconds, float fade)
    {
        if(sr == null)
        {
            sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        }

        life = lifeSeconds;
        fadeDuration = fade;
        spawnTime = Time.time;
        // optionally random tint/alpha jitter
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
    }

    void Update()
    {
        float elapsed = Time.time - spawnTime;
        if (elapsed >= life)
        {
            float fadeT = (elapsed - life) / fadeDuration;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(fadeT));
                sr.color = c;
            }

            if (fadeT >= 1f) Destroy(gameObject);
        }
    }
}
