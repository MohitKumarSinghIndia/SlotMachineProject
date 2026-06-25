using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public enum SoundType
    {
        ButtonClick,
        SpinStart,
        ReelStop,
        Scatter,
        Anticipation,
        FreeSpinStart,
        FreeSpinEnd,
        BigWin,
        MegaWin,
        SuperWin,
        SensationalWin,
        WishGranted,
        CoinCount,
        Transition
    }

    public class SoundController : MonoBehaviour
    {
        public static SoundController Instance { get; private set; }

        private const string SoundPrefKey = "SoundEnabled";
        private const string SFXPrefKey = "SFXEnabled";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip spinStartClip;
        [SerializeField] private AudioClip reelStopClip;
        [SerializeField] private AudioClip scatterClip;
        [SerializeField] private AudioClip anticipationClip;
        [SerializeField] private AudioClip freeSpinStartClip;
        [SerializeField] private AudioClip freeSpinEndClip;
        [SerializeField] private AudioClip bigWinClip;
        [SerializeField] private AudioClip megaWinClip;
        [SerializeField] private AudioClip superWinClip;
        [SerializeField] private AudioClip sensationalWinClip;
        [SerializeField] private AudioClip wishGrantedClip;
        [SerializeField] private AudioClip coinCountClip;
        [SerializeField] private AudioClip transitionClip;

        [Header("Music Clips")]
        [SerializeField] private AudioClip baseGameMusic;
        [SerializeField] private AudioClip freeSpinMusic;
        [SerializeField] private AudioClip introMusic;

        [Header("Volumes")]
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyVolumes();
            PlayIntroMusic();
        }

        private void Start()
        {
            bool musicEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;
            bool sfxEnabled = PlayerPrefs.GetInt(SFXPrefKey, 1) == 1;

            SetSoundEnabled(musicEnabled);
            SetSFXEnabled(sfxEnabled);
        }

        public void SetMusicEnabled(bool enabled)
        {
            SetSoundEnabled(enabled);

            PlayerPrefs.SetInt(SoundPrefKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSfxEnabled(bool enabled)
        {
            SetSFXEnabled(enabled);

            PlayerPrefs.SetInt(SFXPrefKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        private void SetSoundEnabled(bool enabled)
        {
            if (musicSource != null)
            {
                musicSource.mute = !enabled;
            }
        }

        private void SetSFXEnabled(bool enabled)
        {
            if (sfxSource != null)
            {
                sfxSource.mute = !enabled;
            }
        }

        private void ApplyVolumes()
        {
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }

            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void PlaySound(SoundType soundType)
        {
            AudioClip clip = GetClip(soundType);
            PlaySFX(clip);
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            if (sfxSource.isPlaying)
            {
                sfxSource.Stop();
            }

            sfxSource.clip = clip;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
        }

        public void PlayLoopingSound(SoundType soundType)
        {
            AudioClip clip = GetClip(soundType);

            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.Stop();

            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
        }

        public void StopSFX()
        {
            if (sfxSource == null)
            {
                return;
            }

            sfxSource.Stop();
            sfxSource.loop = false;
            sfxSource.clip = null;
        }

        public void PlayBaseGameMusic()
        {
            PlayMusic(baseGameMusic, true);
        }

        public void PlayFreeSpinMusic()
        {
            PlayMusic(freeSpinMusic, true);
        }

        public void PlayIntroMusic()
        {
            PlayMusic(introMusic, true);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource == null)
            {
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource == null)
            {
                return;
            }

            musicSource.Stop();
            musicSource.clip = null;
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);

            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        private AudioClip GetClip(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.ButtonClick: return buttonClickClip;
                case SoundType.SpinStart: return spinStartClip;
                case SoundType.ReelStop: return reelStopClip;
                case SoundType.Scatter: return scatterClip;
                case SoundType.Anticipation: return anticipationClip;
                case SoundType.FreeSpinStart: return freeSpinStartClip;
                case SoundType.FreeSpinEnd: return freeSpinEndClip;
                case SoundType.BigWin: return bigWinClip;
                case SoundType.MegaWin: return megaWinClip;
                case SoundType.SuperWin: return superWinClip;
                case SoundType.SensationalWin: return sensationalWinClip;
                case SoundType.WishGranted: return wishGrantedClip;
                case SoundType.CoinCount: return coinCountClip;
                case SoundType.Transition: return transitionClip;
                default: return null;
            }
        }
    }
}