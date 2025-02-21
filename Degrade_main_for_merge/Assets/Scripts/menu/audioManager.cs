using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class audioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider master;
    public Slider music;
    public Slider sound;

    void Start()
    {
        if (PlayerPrefs.HasKey("mastervolume"))
        {
            master.value = PlayerPrefs.GetFloat("mastervolume");
        }
        else
        {
            master.value = 15f;
        }
        if (PlayerPrefs.HasKey("musicvolume"))
        {
            music.value = PlayerPrefs.GetFloat("musicvolume");
        }
        else
        {
            music.value = 15f;
        }
        if (PlayerPrefs.HasKey("soundvolume"))
        {
            sound.value = PlayerPrefs.GetFloat("soundvolume");
        }
        else
        {
            sound.value = 15f;
        }
        setMasterVolume(master.value);
        setMusicVolume(music.value);
        setSoundVolume(sound.value);
    }

    public void setMasterVolume(float volume)
    {
        if (volume == -25)
        {
            audioMixer.SetFloat("Vmaster", -80);
        }
        else audioMixer.SetFloat("Vmaster", volume);
        PlayerPrefs.SetFloat("mastervolume", volume);
        PlayerPrefs.Save();
    }
    public void setMusicVolume(float volume)
    {
        if (volume == -25)
        {
            audioMixer.SetFloat("Vmusic", -80);
        }
        else audioMixer.SetFloat("Vmusic", volume);
        PlayerPrefs.SetFloat("musicvolume", volume);
        PlayerPrefs.Save();
    }
    public void setSoundVolume(float volume)
    {
        if (volume == -25)
        {
            audioMixer.SetFloat("Vsound", -25);
        }
        else audioMixer.SetFloat("Vsound", volume);
        PlayerPrefs.SetFloat("soundvolume", volume);
        PlayerPrefs.Save();
    }
}
