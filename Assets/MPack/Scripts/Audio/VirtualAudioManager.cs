using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPack {
    public class VirtualAudioManager : MonoBehaviour
    {
        public static VirtualAudioManager ins;

        protected Dictionary<AudioIDEnum, AudioPreset.EnumToAudio> clipDicts;

        public bool dontDestroyOnLoad;

        public AudioSource oneShotAudioSrc;
        protected PrefabPool<AudioOneShotPlayer> oneShotPlayerPool;
        protected float oneShotVolume = 1;

        public AudioSource bgmAudioSrc, secondaryBgmAudioSrc;

        [SerializeField]
        protected AudioPreset defaultPreset;
        [SerializeField]
        protected float soundInRange = 3;

        [SerializeField]
        private AudioOneShotPlayer oneShotAudioSourcePrefab;

        private List<OneShotLoopPlayer> loopPlayers = new List<OneShotLoopPlayer>();

        private AudioListener listener;

        public AudioListener Listener {
            get {
                if (listener == null)
                    listener = FindObjectOfType<AudioListener>();
                
                return listener;
            }
        }

        protected virtual void Awake() {
            if (ins != null)
            {
                Destroy(gameObject);
                return;
            }

            ins = this;

            clipDicts = new Dictionary<AudioIDEnum, AudioPreset.EnumToAudio>();

            if (oneShotAudioSourcePrefab == null)
            {
                oneShotPlayerPool = new PrefabPool<AudioOneShotPlayer>(delegate {
                    GameObject obj = new GameObject("AudioOneShotPlayer");
                    AudioOneShotPlayer player = obj.AddComponent<AudioOneShotPlayer>();
                    player.Volume = oneShotAudioSrc.volume;
                    return player;
                }, true, "AudioCollects");
            }
            else
            {
                oneShotPlayerPool = new PrefabPool<AudioOneShotPlayer>(oneShotAudioSourcePrefab, true, "AudioCollects");
            }

            if (defaultPreset != null) LoadAudioPreset(defaultPreset);

            if (oneShotAudioSrc == null)
                oneShotAudioSrc = gameObject.AddComponent<AudioSource>();

            if (bgmAudioSrc == null) {
                bgmAudioSrc = gameObject.AddComponent<AudioSource>();
                bgmAudioSrc.loop = true;
            }
            if (secondaryBgmAudioSrc == null) {
                secondaryBgmAudioSrc = gameObject.AddComponent<AudioSource>();
                secondaryBgmAudioSrc.loop = true;
            }

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

    #region Audio Load/ Unload
        public void LoadAudioPreset(AudioPreset preset, bool overrideExist=false) {
            for (int i = 0; i < preset.Audios.Length; i++)
                LoadAudio(preset.Audios[i], overrideExist);
        }

        public void LoadAudio(AudioPreset.EnumToAudio audioSet, bool overrideExist = false)
        {
            if (clipDicts.ContainsKey(audioSet.Type))
            {
                if (!overrideExist)
                {
                #if UNITY_EDITOR
                    Debug.LogWarningFormat("Audio '{0}' already exist", audioSet.Type);
                #endif
                }
                else clipDicts[audioSet.Type] = audioSet;
            }
            else clipDicts.Add(audioSet.Type, audioSet);
        }

        public void LoadAudio(AudioIDEnum ID, AudioClip clip, float volume=1, bool overrideExist=false) {
            if (clipDicts.ContainsKey(ID))
            {
                if (!overrideExist)
                {
                #if UNITY_EDITOR
                    Debug.LogWarningFormat("Audio '{0}' already exist", ID);
                #endif
                }
                else clipDicts[ID] = new AudioPreset.EnumToAudio(ID, clip, volume);
            }
            else clipDicts.Add(ID, new AudioPreset.EnumToAudio(ID, clip, volume));
        }

        public void UnloadAudioPreset(AudioPreset preset) {
            for (int i = 0; i < preset.Audios.Length; i++)
            {
                AudioIDEnum ID = preset.Audios[i].Type;

                if (clipDicts.ContainsKey(ID)) clipDicts.Remove(ID);
            }
        }

        public void UnloadAudio(AudioIDEnum ID) {
            if (clipDicts.ContainsKey(ID)) clipDicts.Remove(ID);
        }
    #endregion

    #region Audio One Shot
        public void PlayOneShot(AudioIDEnum ID, float volumeMultiplier = 1) {
            if (clipDicts.ContainsKey(ID)) {
                oneShotAudioSrc.PlayOneShot(clipDicts[ID].Clip, clipDicts[ID].Volume * volumeMultiplier);
            }
            else {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
            }
        }

        public void PlayOneShot(AudioClip clip, float volumeMultiplier = 1) {
            oneShotAudioSrc.PlayOneShot(clip, volumeMultiplier);
        }
    #endregion

    #region Gameobject's Audio One Shot
        public AudioOneShotPlayer PlayOneShotAtPosition(AudioIDEnum ID, Vector3 position, float volumeMultiplier=1) {
            if (!clipDicts.ContainsKey(ID)) {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
                return null;
            }

            Vector2 delta = position - Listener.transform.position;
            if (delta.sqrMagnitude > soundInRange * soundInRange)
                return null;

            AudioOneShotPlayer player = oneShotPlayerPool.Get();
            player.transform.position = position;
            player.Play(clipDicts[ID].Clip, (_player) => oneShotPlayerPool.Put(_player), clipDicts[ID].Volume * volumeMultiplier);
            return player;
        }

        public AudioOneShotPlayer PlayOneShotAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier=1) {
            Vector2 delta = position - Listener.transform.position;
            if (delta.sqrMagnitude > soundInRange * soundInRange)
                return null;

            AudioOneShotPlayer player = oneShotPlayerPool.Get();
            player.transform.position = position;
            player.Play(clip, (_player) => oneShotPlayerPool.Put(_player), volumeMultiplier);
            return player;
        }
    #endregion
    
    #region Background Music
        public void PlayBgm(AudioClip bgmClip, bool overrideCurrentBGM=false) {
            if (bgmAudioSrc.isPlaying) {
                if (!overrideCurrentBGM) return;
                bgmAudioSrc.Stop();
                bgmAudioSrc.clip = null;
            }
            if (secondaryBgmAudioSrc.isPlaying) {
                if (!overrideCurrentBGM) return;
                secondaryBgmAudioSrc.Stop();
                secondaryBgmAudioSrc.clip = null;
            }

            bgmAudioSrc.clip = bgmClip;
            bgmAudioSrc.Play();
        }

        public void BlendNewBgm(AudioClip bgmClip, float fadeOut=0.5f, float fadeOutDelay=0,
                                float fadeIn=0.5f, float fadeInDelay=0.25f) {
            if (!bgmAudioSrc.isPlaying && !secondaryBgmAudioSrc.isPlaying) {
                PlayBgm(bgmClip);
                return;
            }
            // if (bgmAudioSrc.isPlaying && secondaryBgmAudioSrc.isPlaying)
            // TODO: Handle if two bgm audio source both playing

            if (bgmAudioSrc.isPlaying) {
                secondaryBgmAudioSrc.clip = bgmClip;
                StartCoroutine(FadeAudioSource(bgmAudioSrc, 0, fadeOut, fadeOutDelay, stopAfterFade: true));
                secondaryBgmAudioSrc.volume = 0;
                StartCoroutine(FadeAudioSource(secondaryBgmAudioSrc, bgmAudioSrc.volume, fadeIn, fadeInDelay, playerAfterDelay: true, returnVolume: false));
            }
            else {
                bgmAudioSrc.clip = bgmClip;
                StartCoroutine(FadeAudioSource(secondaryBgmAudioSrc, 0, fadeOut, fadeOutDelay, stopAfterFade: true));
                bgmAudioSrc.volume = 0;
                StartCoroutine(FadeAudioSource(bgmAudioSrc, secondaryBgmAudioSrc.volume, fadeIn, fadeInDelay, playerAfterDelay: true, returnVolume: false));
            }
        }
    #endregion

    #region Audio Source Fadeout Control
        public IEnumerator FadeAudioSource(AudioSource src, float targetVolume, float fadeTime,
                                    float delayTime=0, bool returnVolume=true, bool playerAfterDelay=false, bool stopAfterFade=false) {
            if (delayTime > 0) yield return new WaitForSeconds(delayTime);
            if (playerAfterDelay) src.Play();

            float time = 0;
            float originVolume = src.volume;

            while (time < fadeTime) {
                yield return null;
                time += Time.deltaTime;
                src.volume = Mathf.Lerp(originVolume, targetVolume, time / fadeTime);
            }

            if (stopAfterFade) src.Stop();
            if (returnVolume) src.volume = originVolume;
            else src.volume = targetVolume;
        }
        #endregion

    #region Volume Change
    public void ChangeBgmVolume(float volume)
    {
        bgmAudioSrc.volume = volume;
        secondaryBgmAudioSrc.volume = volume;
    }

    public void ChangeSoundVolume(float volume)
    {
        oneShotAudioSrc.volume = volume;

        for (int i = 0; i < oneShotPlayerPool.AliveObjs.Count; i++)
            oneShotPlayerPool.AliveObjs[i].Volume = volume;

        for (int i = 0; i < oneShotPlayerPool.PoolObjs.Count; i++)
            oneShotPlayerPool.PoolObjs[i].Volume = volume;
    }
        #endregion


    #region One Shot Loop Player
    private void Update()
    {
        for (int i = 0; i < loopPlayers.Count; i++)
            loopPlayers[i].Update(this);
    }

    public void PlayLoop(OneShotLoopPlayer player)
    {
        loopPlayers.Add(player);
    }

    public void StopLoop(OneShotLoopPlayer player)
    {
        loopPlayers.Remove(player);
    }
    #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
        if (Listener != null)
        {
            Gizmos.color = new Color(1, 1, 1, 0.8f);
            Gizmos.DrawWireSphere(Listener.transform.position, soundInRange);
        }
    }
#endif
    }
}