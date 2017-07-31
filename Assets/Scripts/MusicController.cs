using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public AudioClip[] clips;
    public float StartVolume = 0.1f;

    private AudioSource _audio;
    private int _index;
    private float _fadeTime = 5.0f;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        _index = 0;
        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        while (true)
        {
            _audio.clip = clips[_index];
            _audio.volume = StartVolume;
            _audio.Play();

            yield return new WaitForSeconds(_audio.clip.length - 5);

            if (_audio.volume > 0.0f)
            {
                _audio.volume -= StartVolume * Time.deltaTime / _fadeTime;

                yield return null;
            }

            _audio.Stop();
            _audio.volume = StartVolume;

            _index = (_index + 1) % clips.Length;
        }
    }
}
