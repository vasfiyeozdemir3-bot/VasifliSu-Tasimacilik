using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Sahneler arası yaşayan müzik ve efekt yöneticisi. İlk erişimde kendini oluşturur.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        static AudioManager _instance;
        public static AudioManager I
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    go.AddComponent<AudioManager>();
                }
                return _instance;
            }
        }

        AudioSource _music;
        AudioSource _sfx;

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            ApplyVolumes();
        }

        public void ApplyVolumes()
        {
            if (_music != null) _music.volume = SaveSystem.MusicVolume * 0.45f;
            if (_sfx != null) _sfx.volume = SaveSystem.SfxVolume;
            AudioListener.volume = SaveSystem.MasterVolume;
        }

        public void PlayMusic()
        {
            var a = GameAssets.I;
            if (a == null || a.music == null || _music == null) return;
            if (_music.clip == a.music && _music.isPlaying) return;
            _music.clip = a.music;
            _music.Play();
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfx == null) return;
            _sfx.PlayOneShot(clip, volume);
        }

        public static void Click() { if (GameAssets.I != null) I.Play(GameAssets.I.click, 0.7f); }
    }
}
