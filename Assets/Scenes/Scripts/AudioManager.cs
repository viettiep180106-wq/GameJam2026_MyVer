using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;

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
    public AudioClip chatClip;
    public AudioClip clickButtonClip;
    public AudioClip clock3sLastClip;
    public AudioClip loseClip;
    public AudioClip releaseItemClip;
    public AudioClip scoreClip;
    public AudioClip throwMaskClip;
    public AudioClip winClip;


    [Header("Audio Source Pool")]
    public AudioSource audioSourcePrefab;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private Dictionary<GameSound, AudioClip> soundMap = new Dictionary<GameSound, AudioClip>();

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float _MasterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _BGMVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _SFXVolume = 1f;

    [Header("Global Volume & Mute")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    public bool isMuted = false;

    void Awake()
    {
            DontDestroyOnLoad(gameObject);

            // Tạo pool AudioSource
            for (int i = 0; i < 10; i++)
            {
                AudioSource src = Instantiate(audioSourcePrefab, transform);
                audioSources.Add(src);
            }

            // Map enum -> clip
            soundMap[GameSound.bg] = bgClip;
            soundMap[GameSound.chat] = chatClip;
            soundMap[GameSound.clickButton] = clickButtonClip;
            soundMap[GameSound.clock3sLast] = clock3sLastClip;
            soundMap[GameSound.lose] = loseClip;
            soundMap[GameSound.releaseItem] = releaseItemClip;
            soundMap[GameSound.score] = scoreClip;
            soundMap[GameSound.throwMask] = throwMaskClip;
            soundMap[GameSound.win] = winClip;
    }

    private void Start()
    {
        Play(GameSound.bg);
        Debug.Log("sound");
    }


    public void Play(GameSound sound, float volum = -1f)
    {
        if (isMuted) return;

        if (soundMap.ContainsKey(sound))
        {
            AudioClip clip = soundMap[sound];
            AudioSource src = audioSources.Find(s => !s.isPlaying);
            if (src == null) src = audioSources[0];

            src.volume = volum == -1 ? sfxVolume : volum;
            src.pitch = 1f;
            src.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound not found " + sound);
        }
    }
}

