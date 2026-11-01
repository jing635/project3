using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public List<AudioInfo> audioinfos;
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
            return;
        }

        foreach (AudioInfo Info in audioinfos)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.clip = Info.Clip;
            source.volume = Info.volume;
            source.loop = Info.isLoop;

            Info.Source = source;
       
        }

        

    }

    //设置音乐大小的方法
    public void SetMusicVolume(float volume)
    {
        foreach (AudioInfo info in ByCategory(AudioCategory.Music))
        {
            info.Source.volume = volume;
        }
    }

    //设置音效大小的方法
    public void SetSoundVolume(float volume)
    {
        foreach (AudioInfo info in ByCategory(AudioCategory.SoundEffect))
        {
            info.Source.volume = volume;
        }
    }

    //设置入场音效
    public void SetEnterVolume(float volume)
    {
        foreach (AudioInfo info in ByCategory(AudioCategory.EnterMusic))
        {
            info.Source.volume = volume;
        }
    }

    //找到特定类别的音乐
    private List<AudioInfo> ByCategory(AudioCategory category)
    {
        List<AudioInfo> infos = new List<AudioInfo>();
        foreach (AudioInfo info in audioinfos)
        {
            if(info.category == category)
            {
                infos.Add(info);
            }
        }
        return infos;

    }

    //播放指定的音乐
    public void PlayMusic(string name)
    {
        foreach (AudioInfo info in audioinfos)
        {
            if(info.name == name)
            {
                info.Source.Play();
            }
        }

    }

    //停止指定音乐
    public void StopMusic(string name)
    {
        foreach (AudioInfo info in audioinfos)
        {
            if (info.name == name)
            {
                info.Source.Stop();
            }
        }

    }






}
