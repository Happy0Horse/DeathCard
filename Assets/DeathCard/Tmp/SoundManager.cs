using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public List<AudioClip> musicClips;
    public List<AudioClip> sfxClips;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(int index)
    {
        if (index >= 0 && index < musicClips.Count)
        {
            musicSource.clip = musicClips[index];
            musicSource.Play();
        }
    }

    public void PlaySFX(int index)
    {
        if (index >= 0 && index < sfxClips.Count)
        {
            sfxSource.PlayOneShot(sfxClips[index]);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }
}
