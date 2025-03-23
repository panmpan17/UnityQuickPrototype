using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="MPack/Audio Clip Set")]
public class AudioClipSet : ScriptableObject
{
    public AudioClip[] Clips;
    public ClipPlayMode Mode;
    [Range(0, 1)]
    public float Volume = 1;
    [System.NonSerialized]
    private int _index = 0;

    public enum ClipPlayMode { Sequence, Random }

    public AudioClip ChooseOneClip()
    {
        if (Clips.Length == 1)
            return Clips[0];
        
        if (Mode == ClipPlayMode.Random)
            return Clips[Random.Range(0, Clips.Length)];

        if (_index >= Clips.Length)
            _index = 0;
        return Clips[_index++];
    }

    public void PlayClipAtPoint(Vector3 position)
    {
        AudioClipSetExtension.PlayClipAtPoint(this, position, Volume);
    }
}

public static class AudioClipSetExtension
{
    static List<AudioSource> sm_audioSources = new List<AudioSource>();

    public static void PlayOneShot(this AudioSource audioSource, AudioClipSet clipSet, float volume=1)
    {
        if (clipSet && clipSet.Clips.Length > 0)
            audioSource.PlayOneShot(clipSet.ChooseOneClip(), clipSet.Volume * volume);
    }

    public static void Play(this AudioSource audioSource, AudioClipSet clipSet)
    {
        if (!clipSet)
            return;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = clipSet.ChooseOneClip();
        audioSource.volume = clipSet.Volume;
        audioSource.Play();
    }

    public static void PlayClipAtPoint(AudioClipSet clipSet, Vector3 position, float volume=1)
    {
        AudioClip clip = clipSet.ChooseOneClip();

        for (int i = sm_audioSources.Count - 1; i >= 0; i--)
        {
            if (!sm_audioSources[i].isPlaying)
            {
                var audioSource = sm_audioSources[i];
                audioSource.clip = clip;
                audioSource.volume = clipSet.Volume * volume;
                audioSource.Play();
                return;
            }
        }

        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.clip = clip;
        newAudioSource.spatialBlend = 1f;
        newAudioSource.volume = clipSet.Volume * volume;
        newAudioSource.Play();
        sm_audioSources.Add(newAudioSource);
    }
}