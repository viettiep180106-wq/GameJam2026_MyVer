using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource _BGMSource;
    [SerializeField] private AudioSource _SFXSource;
    [Header("Auido Clip")]
    public List<AudioClip> _ClipAudio = new List<AudioClip>();
    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float _MasterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _BGMVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _SFXVolume = 1f;

    private void Start()
    {
        ApplyVolume();
    }

    // ✅ GET
    public float MasterVolume => _MasterVolume;
    public float BGMVolume => _BGMVolume;
    public float SFXVolume => _SFXVolume;

    // ✅ SET
    public void SetMasterVolume(float value)
    {
        _MasterVolume = value;
        ApplyVolume();
    }

    public void SetBGMVolume(float value)
    {
        _BGMVolume = value;
        ApplyVolume();
    }

    public void SetSFXVolume(float value)
    {
        _SFXVolume = value;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        _BGMSource.volume = _MasterVolume * _BGMVolume;
        _SFXSource.volume = _MasterVolume * _SFXVolume;
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        _BGMSource.clip = clip;
        _BGMSource.loop = loop;
        _BGMSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _SFXSource.PlayOneShot(clip);
    }
}
