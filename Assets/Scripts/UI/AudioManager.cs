using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicAudioSource, sfxAudioSource;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }       
    }

    private void Start()
    {
        PlayMusic("Theme");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
        else
        {
            musicAudioSource.clip = s.clip;
            musicAudioSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
        else
        {
            sfxAudioSource.PlayOneShot(s.clip);
        }
    }

    public static void PlaySFXGlobal(string name)
    {
        if (instance == null)
        {
            Debug.LogWarning($"AudioManager.instance is null. Cannot play SFX: {name}");
            return;
        }
        instance.PlaySFX(name);
    }
}
