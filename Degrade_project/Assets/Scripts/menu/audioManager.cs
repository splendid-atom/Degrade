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
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("mastervolume"))
        {
            master.value = PlayerPrefs.GetFloat("mastervolume");
        }
        else
        {
            master.value = 10f;
        }
        if (PlayerPrefs.HasKey("musicvolume"))
        {
            music.value = PlayerPrefs.GetFloat("musicvolume");
        }
        else
        {
            music.value = 10f;
        }
        if (PlayerPrefs.HasKey("soundvolume"))
        {
            sound.value = PlayerPrefs.GetFloat("soundvolume");
        }
        else
        {
            sound.value = 20f;
        }
        setMasterVolume(master.value);
        setMusicVolume(music.value);
        setSoundVolume(sound.value);
    }

    // Update is called once per frame
    public void setMasterVolume(float volume)
    {
        audioMixer.SetFloat("Vmaster", volume);
        PlayerPrefs.SetFloat("mastervolume", volume);
        PlayerPrefs.Save();
    }
    public void setMusicVolume(float volume)
    {
        audioMixer.SetFloat("Vmusic", volume);
        PlayerPrefs.SetFloat("musicvolume", volume);
        PlayerPrefs.Save();
    }
    public void setSoundVolume(float volume)
    {
        audioMixer.SetFloat("Vsound", volume);
        PlayerPrefs.SetFloat("soundvolume", volume);
        PlayerPrefs.Save();
    }
}
