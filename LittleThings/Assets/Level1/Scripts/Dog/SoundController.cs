using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource audioSource; // can be a dedicated AudioSource
    public AudioClip barkClip;
    public AudioClip pantClip;
    public AudioClip sniffClip;
    public AudioClip thudClip;
    public AudioClip playfulYip;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D
            audioSource.playOnAwake = false;
        }
    }

    public void PlayBark(float volume = 1f)
    {
        PlayOneShot(barkClip, volume);
    }

    public void PlayPant(float volume = 1f)
    {
        PlayOneShot(pantClip, volume);
    }

    public void PlaySniff(float volume = 1f)
    {
        PlayOneShot(sniffClip, volume);
    }

    public void PlayThud(float volume = 1f)
    {
        PlayOneShot(thudClip, volume);
    }

    public void PlayYip(float volume = 1f)
    {
        PlayOneShot(playfulYip, volume);
    }

    void PlayOneShot(AudioClip clip, float vol)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, vol);
    }
}
