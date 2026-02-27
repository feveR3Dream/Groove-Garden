using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// Enum stays exactly the same...
public enum SoundType
{
    // FOR UI
    Button_Hover,
    Button_Selection,

}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }


    [Header("Sound List")]
    [SerializeField] private SoundList[] soundList;


    [Header("3D Settings")]
    [SerializeField] private float maxSoundDistance = 20f;
    [SerializeField] private float spatialBlend = 1f;


    private AudioSource main2DSource; // For UI / OneShots


    // CHANNEL 1: SCENE LOOPS (Sirens, Machines) - Dies on Load
    private Dictionary<SoundType, AudioSource> activeSceneLoops = new Dictionary<SoundType, AudioSource>();


    // CHANNEL 2: GLOBAL TRACKS (Music/Ambience) - Persists automatically
    private Dictionary<SoundType, AudioSource> currentAmbienceSources = new Dictionary<SoundType, AudioSource>();


    // CHANNEL 3: OCCASIONAL SOUNDS (Random Loops)
    private Dictionary<SoundType, Coroutine> activeOccasionalRoutines = new Dictionary<SoundType, Coroutine>();



    void Start()
    {
        main2DSource = GetComponent<AudioSource>();
        main2DSource.spatialBlend = 0f;
    }

    void OnValidate()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++) soundList[i].name = names[i];
    }



    #region Scene Management
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllSceneLoops();
        StopAllOccasionalSounds();

        Button[] buttons = GameObject.FindObjectsOfType<Button>(true); // 'true' includes inactive ones too!
        Debug.Log($"Found {buttons.Length} buttons. Injecting sound chips...");

        foreach (Button btn in buttons)
        {
            if (btn.gameObject.GetComponent<ButtonSoundInject>() == null)
            {
                btn.gameObject.AddComponent<ButtonSoundInject>();
            }
        }
    }

    private void StopAllSceneLoops()
    {
        foreach (var kvp in activeSceneLoops)
        {
            if (kvp.Value != null) Destroy(kvp.Value.gameObject);
        }
        activeSceneLoops.Clear();
    }
    #endregion


    #region Channel 1: One Shots & Scene Loops
    public void PlaySound2D(SoundType sound, float volume = 1)
    {
        AudioClip clip = GetRandomClip(sound);
        if (clip != null) main2DSource.PlayOneShot(clip, volume);
    }

    public void PlaySound3D(SoundType sound, Vector3 position, float volume = 1)
    {
        AudioClip clip = GetRandomClip(sound);
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio_" + sound);
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;

        source.spatialBlend = spatialBlend;
        source.maxDistance = maxSoundDistance;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();
        Destroy(tempGO, clip.length + 0.1f);
    }

    // These die when you leave the scene
    public void PlaySceneLoop(SoundType sound, Transform parent, float volume = 1)
    {
        if (activeSceneLoops.ContainsKey(sound)) return; // Already playing this loop

        AudioSource source = CreateAudioObject(sound, parent, volume, true, parent != null);
        activeSceneLoops.Add(sound, source);
    }

    public void StopSceneLoop(SoundType sound)
    {
        if (activeSceneLoops.TryGetValue(sound, out AudioSource source))
        {
            if (source != null) Destroy(source.gameObject);
            activeSceneLoops.Remove(sound);
        }
    }

    #endregion


    #region Channel 2: Global Ambience & Music (Persistent)
    public void PlayAmbience(SoundType sound, float volume = 1)
    {
        if (currentAmbienceSources.ContainsKey(sound))
        {
            // Optional: You could fade the volume here if you wanted to update it!
            return;
        }

        AudioSource audio = CreateAudioObject(sound, transform, volume, true, false);
        currentAmbienceSources.Add(sound, audio);
    }

    public void StopAmbience(SoundType sound)
    {
        if (!currentAmbienceSources.TryGetValue(sound, out AudioSource audioSource))
        {
            return; // Nothing to stop
        }

        if (audioSource != null)
        {
            Destroy(audioSource.gameObject);
        }

        currentAmbienceSources.Remove(sound);
    }
    #endregion


    #region Channel 3: Occasional Sounds
    public void PlayOccasionalSound2D(SoundType sound, int percentage, float checkInterval = 5f, float volume = 1f, bool allowOverwrite = true)
    {
        // THE NEW LOGIC:
        if (!allowOverwrite)
        {
            if (activeOccasionalRoutines.ContainsKey(sound)) return;
        }

        StopOccasionalSound(sound); // Kill old (only if we didn't return above)

        Coroutine routine = StartCoroutine(OccasionalRoutine2D(sound, percentage, checkInterval, volume));
        activeOccasionalRoutines.Add(sound, routine);
    }

    private IEnumerator OccasionalRoutine2D(SoundType sound, int percentage, float interval, float volume)
    {
        while (true)
        {
            // Wait first, so we don't spam sound immediately upon start
            yield return new WaitForSeconds(interval);

            if (UnityEngine.Random.Range(0, 101) <= percentage)
            {
                AudioSource source = CreateAudioObject(sound, this.transform, volume, false, false);
                if (source != null)
                {
                    yield return new WaitForSeconds(source.clip.length);
                    Destroy(source.gameObject);
                }
            }
        }
    }

    public void PlayOccasionalSound3D(SoundType sound, Transform parent, int percentage, float checkInterval = 5f, float volume = 1f, bool allowOverwrite = true)
    {
        if (!allowOverwrite)
        {
            if (activeOccasionalRoutines.ContainsKey(sound)) return;
        }

        StopOccasionalSound(sound);

        if (parent == null)
        {
            Debug.LogWarning($"Trying to play 3D Occasional Sound {sound} but parent is null!");
            return;
        }

        Coroutine routine = StartCoroutine(OccasionalRoutine3D(sound, parent, percentage, checkInterval, volume));
        activeOccasionalRoutines.Add(sound, routine);
    }

    private IEnumerator OccasionalRoutine3D(SoundType sound, Transform parent, int percentage, float interval, float volume)
    {
        while (true)
        {
            if (parent == null)
            {
                activeOccasionalRoutines.Remove(sound);
                yield break;
            }

            yield return new WaitForSeconds(interval);

            if (parent == null) break;

            if (UnityEngine.Random.Range(0, 101) <= percentage)
            {
                AudioSource source = CreateAudioObject(sound, parent, volume, false, true);
                if (source != null)
                {
                    yield return new WaitForSeconds(source.clip.length);
                    if (source != null)
                        Destroy(source.gameObject);
                }
            }
        }
    }

    public void StopOccasionalSound(SoundType sound)
    {
        if (activeOccasionalRoutines.TryGetValue(sound, out Coroutine routine))
        {
            if (routine != null) StopCoroutine(routine);
            activeOccasionalRoutines.Remove(sound);
        }
    }

    public void StopAllOccasionalSounds()
    {
        foreach (var kvp in activeOccasionalRoutines)
        {
            if (kvp.Value != null) StopCoroutine(kvp.Value);
        }
        activeOccasionalRoutines.Clear();
    }
    #endregion


    #region Helpers
    private AudioSource CreateAudioObject(SoundType sound, Transform parent, float volume, bool loop, bool is3D)
    {
        AudioClip clip = GetRandomClip(sound);
        if (clip == null) return null;

        GameObject obj = new GameObject("Audio_" + sound);
        obj.transform.SetParent(parent != null ? parent : transform);
        obj.transform.localPosition = Vector3.zero;

        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.loop = loop;

        if (is3D)
        {
            src.spatialBlend = spatialBlend;
            src.maxDistance = maxSoundDistance;
            src.rolloffMode = AudioRolloffMode.Linear;
        }
        else src.spatialBlend = 0f;

        src.Play();
        return src;
    }

    private AudioClip GetRandomClip(SoundType sound)
    {
        int index = (int)sound;

        // 1. Safety Check: Is the index valid?
        if (index < 0 || index >= soundList.Length)
        {
            Debug.LogError($"SoundManager: Missing sound config for [{sound}]. Update the SoundManager inspector!");
            return null;
        }

        // 2. Safety Check: Is the clip array empty?
        AudioClip[] clips = soundList[index].Sounds;
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No audio clips assigned for [{sound}]!");
            return null;
        }

        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    #endregion

}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}


public class ButtonSoundInject : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Button btn = GetComponent<Button>();
        if (btn != null && btn.interactable)
        {
            if (btn.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                SoundManager.Instance.PlaySound2D(SoundType.Button_Hover, 1f);  // Used to be 0.5
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Button btn = GetComponent<Button>();
        if (btn != null && btn.interactable)
        {
            if (btn.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                SoundManager.Instance.PlaySound2D(SoundType.Button_Selection, 0.7f); // Used  to be 0.2
            }
        }
    }
}

