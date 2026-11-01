using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AudioCategory
{
    EnterMusic,
    Music,
    SoundEffect
}

[System.Serializable]
public class AudioInfo 
{
    [HideInInspector]
    public AudioSource Source;
    public AudioClip Clip;
    public string name;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool isLoop;
    public AudioCategory category;
}
