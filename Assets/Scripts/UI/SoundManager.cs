using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] Slider musicSlider, sfxSlider;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("musicVolume") && !PlayerPrefs.HasKey("sfxVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1f);
            PlayerPrefs.SetFloat("sfxVolume", 1f);
            Load();
        }
        else
        {
            Load();
        }
    }
    public void ChangeVolume()
    {
        AudioManager.instance.musicAudioSource.volume = musicSlider.value;
        AudioManager.instance.sfxAudioSource.volume = sfxSlider.value;
        Save();
    }

    private void Load()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
    }
}
