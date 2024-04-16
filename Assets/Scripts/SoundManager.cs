using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class SoundManager : MonoBehaviour
{
    public enum Channel
    {
        Master,
        SFX,
        Music,
        Voice,
        Enemies
    }
    private static SoundManager instance;
    public static SoundManager Instance { get 
        { 
            if (instance == null) instance = FindObjectOfType<SoundManager>();
            return instance;
        }
    }
    private AudioSource audioSource;
    public static readonly Dictionary<Channel, string> ChannelVolumeKeys = new Dictionary<Channel, string> {
        { Channel.Master, "MasterVolume"},
        { Channel.SFX, "SFXVolume"},
        { Channel.Music ,"MusicVolume"},
        { Channel.Voice, "VoiceVolume"},
        { Channel.Enemies, "EnemiesVolume"}
    };

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public float GetChannelVolume(Channel channel)
    {
        float volume = PlayerPrefs.GetFloat(ChannelVolumeKeys[channel], 1);
        if (channel != Channel.Master)
            volume *= PlayerPrefs.GetFloat(ChannelVolumeKeys[Channel.Master], 1.0f);
        return volume;
    }
    public void SetChannelVolume(Channel channel, float volume)
    {
        PlayerPrefs.SetFloat(ChannelVolumeKeys[channel], volume);
    }

    public void PlayGlobal(AudioClip clip, Channel channel = Channel.Master, float volumeFactor = 1.0f)
    {
        if (audioSource == null) return;
        float volume = GetChannelVolume(channel);
        audioSource.PlayOneShot(clip, volume * volumeFactor);
    }
}
