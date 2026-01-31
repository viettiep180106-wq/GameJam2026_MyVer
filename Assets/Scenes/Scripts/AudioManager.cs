using System.Collections.Generic;
using UnityEngine;

public enum GameSound
{
    bg,
    chat,
    clickButton,
    clock3sLast,
    lose,
    releaseItem,
    score,
    throwMask,
    win,
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Clips")]
    public AudioClip bgClip;
    public AudioClip chatClip, clickButtonClip, clock3sLastClip, loseClip, releaseItemClip, scoreClip, throwMaskClip, winClip;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _poolSize = 10;
    private List<AudioSource> _sfxPool = new List<AudioSource>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float _bgmVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private bool _isMuted = false;

    private Dictionary<GameSound, AudioClip> _soundMap = new Dictionary<GameSound, AudioClip>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitPool();
        MapSounds();
    }

    private void Start()
    {
        ApplyVolume();
        PlayBGM(); // Phát nhạc nền mặc định
    }

    private void InitPool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource src = Instantiate(_audioSourcePrefab, transform);
            _sfxPool.Add(src);
        }
    }

    private void MapSounds()
    {
        _soundMap[GameSound.bg] = bgClip;
        _soundMap[GameSound.chat] = chatClip;
        _soundMap[GameSound.clickButton] = clickButtonClip;
        _soundMap[GameSound.clock3sLast] = clock3sLastClip;
        _soundMap[GameSound.lose] = loseClip;
        _soundMap[GameSound.releaseItem] = releaseItemClip;
        _soundMap[GameSound.score] = scoreClip;
        _soundMap[GameSound.throwMask] = throwMaskClip;
        _soundMap[GameSound.win] = winClip;
    }

    // Phát nhạc nền (BGM) - Sử dụng nguồn riêng để loop
    public void PlayBGM()
    {
        if (bgClip == null) return;
        _bgmSource.clip = bgClip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    // Hàm Play tổng quát cho SFX (Sử dụng Pool)
    public void Play(GameSound sound, float volumeMultiplier = 1f)
    {
        if (_isMuted || !_soundMap.ContainsKey(sound)) return;

        AudioSource src = _sfxPool.Find(s => !s.isPlaying);
        if (src == null) src = _sfxPool[0];

        // Volume thực tế = Master * SFX * Multiplier
        src.volume = _masterVolume * _sfxVolume * volumeMultiplier;
        src.pitch = 1f;
        src.PlayOneShot(_soundMap[sound]);
    }

    // --- Volume Management ---
    public void SetMasterVolume(float val) { _masterVolume = val; ApplyVolume(); }
    public void SetBGMVolume(float val) { _bgmVolume = val; ApplyVolume(); }
    public void SetSFXVolume(float val) { _sfxVolume = val; ApplyVolume(); }
    public void SetMute(bool mute) { _isMuted = mute; ApplyVolume(); }

    private void ApplyVolume()
    {
        _bgmSource.mute = _isMuted;
        _bgmSource.volume = _masterVolume * _bgmVolume;

        foreach (var src in _sfxPool)
        {
            src.mute = _isMuted;
            // Chỉ cập nhật volume cho các nguồn đang phát để tránh giật lag
            if (src.isPlaying) src.volume = _masterVolume * _sfxVolume;
        }
    }
}