using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(AudioReverbFilter))]
public class TurntableInDepthSystem : MonoBehaviour
{

    // CONTAIN:
    // 1. Playing & pausing & stopping track
    // 2. Turntable sound effect functionalities
    // 3. Add more here...


    // References
    public AudioSource VinylSpeaker { get; private set; }
    public AudioReverbFilter ReverbFilter { get; private set; }
    private AudioClip currentTrack;


    // Variables
    //[HideInInspector] public float CurrentTimeMark = 0f; // Determine time mark of track to play
    private bool isPlayingMusic = false;
    private float currentSpinSpeed;


    // Coroutines
    private Coroutine adjustRPMCoroutine = null;
    private Coroutine reverbCoroutine = null;


    private void Awake()
    {  
        StartCoroutine(InitializingAudioComponent());
    }

    private IEnumerator InitializingAudioComponent()
    {
        VinylSpeaker = GetComponent<AudioSource>();

        ReverbFilter = GetComponent<AudioReverbFilter>();
        ReverbFilter.reverbPreset = AudioReverbPreset.Generic;

        yield return null;

        ReverbFilter.reverbPreset = AudioReverbPreset.User;
        ReverbFilter.reverbLevel = -10000f;
        ReverbFilter.room = -10000f;
    }


    #region PLAYING / STOPPING RECORD LOGIC
    //-------------------------------------

    public void PlayRecord(Record record, float spinSpeed, RPM rpm)
    {
        if (record == null) { return; }

        #region SPINNIN RECORD (Get it?)
        Transform recordTransform = record.transform;

        if (TurntableManager.Instance.UpdatedRPM)
        {
            TurntableManager.Instance.UpdatedRPM = false;
            ApplyNewRPM(spinSpeed, rpm);
        }

        recordTransform.Rotate(0f, 0f, -currentSpinSpeed * Time.deltaTime);
        #endregion


        #region PLAY MUSIC
        if (TurntableManager.Instance.CanPlayMusic)
        {
            if (!isPlayingMusic)
            {
                isPlayingMusic = true;
                currentTrack = record.RecordTrack;

                PlayMusic(currentTrack, TurntableManager.Instance.GetTimeMark());
            }
        }
        #endregion
    }


    public void StopRecord()
    {
        if (isPlayingMusic)
        {
            isPlayingMusic = false;
            PauseMusic();    
        }
    }


    // We pass the track AND the time we want to start at
    private void PlayMusic(AudioClip track, float startTimeMark)
    {
        VinylSpeaker.clip = track;

        float safeTimeMark = Mathf.Clamp(startTimeMark, 0f, track.length - 0.01f);
        VinylSpeaker.time = safeTimeMark;

        VinylSpeaker.Play();
    }


    private void PauseMusic()
    {
        if (VinylSpeaker != null)
        {
            VinylSpeaker.Pause();
        }
    }

    //--------
    #endregion



    #region TURNTABLE SETTING LOGIC
    //-----------------------------

    public void UpdateVolume(float volume)
    {
        VinylSpeaker.volume = volume;
    }


    private void ApplyNewRPM(float spinSpeed, RPM rpm)
    {
        if (adjustRPMCoroutine != null)
        {
            StopCoroutine(adjustRPMCoroutine);
            adjustRPMCoroutine = null;
        }

        adjustRPMCoroutine = StartCoroutine(ApplyingNewRPM(spinSpeed, rpm));
    }
    private IEnumerator ApplyingNewRPM(float spinSpeed, RPM rpm)
    {
        float currentTime = 0f;
        
        float currentPitch;
        if (rpm == RPM.Slowed) currentPitch = TurntableManager.Instance.SlowedPitchValue;
        else if (rpm == RPM.Normal) currentPitch = TurntableManager.Instance.NormalPitchValue;
        else if (rpm == RPM.SpedUp) currentPitch = TurntableManager.Instance.SpedUpPitchValue;
        else currentPitch = 1f;


        while (currentTime < Global.MaxInterpolationTime)
        {
            currentTime += Time.deltaTime;
            float percent = currentTime / Global.MaxInterpolationTime;

            // Disk spin speed
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, spinSpeed, percent);

            // Track pitch (playback speed)
            if (VinylSpeaker.clip != null)
            {
                VinylSpeaker.pitch = Mathf.Lerp(VinylSpeaker.pitch, currentPitch, percent);
            }

            yield return null;
        }

        currentSpinSpeed = spinSpeed;
        VinylSpeaker.pitch = currentPitch;

        adjustRPMCoroutine = null;
    }

    public void ProcessReverb(bool isReverb)
    {
        if (reverbCoroutine != null)
        {
            StopCoroutine(reverbCoroutine);
            reverbCoroutine = null;
        }

        reverbCoroutine = StartCoroutine(ProcessingReverb(isReverb));
    }
    private IEnumerator ProcessingReverb(bool isReverb)
    {
        float currentTime = 0f;
        
        // For Reverb Level
        float targetReverbValue = isReverb ? 200f : -10000f;

        // For 
        float targetRoomValue = isReverb ? -1000f : -10000f;

        while (currentTime < Global.MaxInterpolationTime)
        {
            currentTime += Time.deltaTime;
            float percent = currentTime / Global.MaxInterpolationTime;

            // Reverb Level
            float tempReverbValue = Mathf.Lerp(ReverbFilter.reverbLevel, targetReverbValue, percent); 
            ReverbFilter.reverbLevel = tempReverbValue;

            // Room
            float tempRoomValue = Mathf.Lerp(ReverbFilter.room, targetRoomValue, percent);
            ReverbFilter.room = tempRoomValue;

            yield return null;
        }

        ReverbFilter.reverbLevel = targetReverbValue;
        ReverbFilter.room = targetRoomValue;

        reverbCoroutine = null;
    }


    //--------
    #endregion
}
