using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TurntableInDepthSystem : MonoBehaviour
{

    // CONTAIN:
    // 1. Playing & pausing & stopping track
    // 2. Turntable sound effect functionalities
    // 3. Add more here...


    // References
    public AudioSource VinylSpeaker { get; private set; }
    private AudioClip currentTrack;


    // Variables
    //[HideInInspector] public float CurrentTimeMark = 0f; // Determine time mark of track to play
    private bool isPlayingMusic = false;
    private float currentSpinSpeed;


    // Coroutines
    private Coroutine adjustRPMCoroutine = null;


    private void Awake()
    {
        VinylSpeaker = GetComponent<AudioSource>();    
    }


    public void PlayRecord(Record record, float spinSpeed)
    {
        if (record == null) { return; }

        #region SPINNIN RECORD (Get it?)
        Transform recordTransform = record.transform;

        if (TurntableControl.Instance.UpdatedRPM)
        {
            TurntableControl.Instance.UpdatedRPM = false;
            ApplyNewRPM(spinSpeed);
        }

        recordTransform.Rotate(0f, 0f, currentSpinSpeed * Time.deltaTime);
        #endregion

        if (TurntableControl.Instance.CanPlayMusic)
        {
            if (!isPlayingMusic)
            {
                isPlayingMusic = true;
                currentTrack = record.RecordTrack;

                PlayMusic(currentTrack, TurntableControl.Instance.GetTimeMark());
            }
        }

    }


    public void StopRecord()
    {
        if (isPlayingMusic)
        {
            isPlayingMusic = false;
            StopMusic();    
        }
    }


    // We pass the track AND the time we want to start at
    private void PlayMusic(AudioClip track, float startTimeMark)
    {
        // 1. Give it the track first
        VinylSpeaker.clip = track;

        // 2. Now that it has a track, tell it where to skip to
        VinylSpeaker.time = startTimeMark;

        // 3. Drop the needle
        VinylSpeaker.Play();
    }


    private void StopMusic()
    {
        if (VinylSpeaker != null)
        {
            VinylSpeaker.Stop();
        }
    }


    public void UpdateVolume(float volume)
    {
        VinylSpeaker.volume = volume;
    }


    private void ApplyNewRPM(float spinSpeed)
    {
        if (adjustRPMCoroutine != null)
        {
            StopCoroutine(adjustRPMCoroutine);
            adjustRPMCoroutine = null;
        }

        adjustRPMCoroutine = StartCoroutine(ApplyingNewRPM(spinSpeed));
    }
    private IEnumerator ApplyingNewRPM(float spinSpeed)
    {
        float currentTime = 0f;

        while (currentTime < Global.MaxInterpolationTime)
        {
            currentTime += Time.deltaTime;

            float percent = currentTime / Global.MaxInterpolationTime;
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, spinSpeed, percent);

            yield return null;
        }

        currentSpinSpeed = spinSpeed;
        adjustRPMCoroutine = null;
    }

}
