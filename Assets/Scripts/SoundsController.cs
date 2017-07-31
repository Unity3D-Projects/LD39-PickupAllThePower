using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsController : MonoBehaviour
{
    public AudioClip[] clips;

    private AudioSource[] _audios;
    private int _index;

    void Awake()
    {
        _audios = GetComponents<AudioSource>();
    }

    void Start()
    {
        _index = 0;
    }

    public void Play(string name)
    {
        AudioClip clip = null;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == name)
            {
                clip = clips[i];
                break;
            }
        }

        if (clip != null)
        {
            var audioSource = _audios[_index];
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = clip;
            audioSource.Play();

            _index = (_index + 1) % _audios.Length;
        }
    }
}
