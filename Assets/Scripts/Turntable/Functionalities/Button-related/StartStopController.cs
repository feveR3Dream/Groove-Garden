using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ButtonControl))]
public class StartStopController : MonoBehaviour
{

    // References
    private ButtonControl buttonControl;


    // Start Stop Button Values
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;


    // Playing Record & Anything Relevant Coroutines
    private Coroutine startStopRecordCoroutine = null;



    private void Awake()
    {
        buttonControl = GetComponent<ButtonControl>();
    }


    public void StartStopRecord()
    {
        if (startStopRecordCoroutine != null)
        {
            StopCoroutine(startStopRecordCoroutine);
            startStopRecordCoroutine = null;
        }

        if (!KnobManager.Instance.TonearmControl.TonearmAtTheEnd)
        {
            if (!isPlaying)
            {
                isPlaying = true;

                startStopRecordCoroutine = StartCoroutine(ProcessingRecord());
            }
            else
            {
                isPlaying = false;
                if (startStopRecordCoroutine != null)
                {
                    StopCoroutine(startStopRecordCoroutine);
                    startStopRecordCoroutine = null;
                }

                KnobManager.Instance.TonearmControl.StopTonearm();
                TurntableManager.Instance.TurntableSystem.StopRecord();
            }
        }
        else ResetStartStopButton();

        TurntableManager.Instance.DisplayerControl.UpdatePlayPauseDisplay();
    }


    private void ResetStartStopButton()
    {
        if (startStopRecordCoroutine != null)
        {
            StopCoroutine(startStopRecordCoroutine);
            startStopRecordCoroutine = null;
        }

        isPlaying = false;
        buttonControl.TurnedOn = false;

        buttonControl.UpdateButtonAlpha(buttonControl.ButtonIdleOffAlpha, buttonControl.AlphaFadeSpeed);
        buttonControl.UpdateButtonSize(buttonControl.IdleSizeOFF, buttonControl.ResizeSpeed);
        
    }


    // HANDLES PLAYING TRACK FROM RECORD---<< MAIN >>
    private IEnumerator ProcessingRecord()
    {
        while (true)
        {
            if (TurntableManager.Instance.TurnedOn &&
                RecordManager.Instance.CurrentRecord != null)
            {
                if (!KnobManager.Instance.TonearmControl.TonearmAtTheEnd)
                {
                    if (TurntableManager.Instance.EquipRecord && TurntableManager.Instance.RecordRead && buttonControl.TurnedOn)
                    {
                        TurntableManager.Instance.TurntableSystem.PlayRecord(RecordManager.Instance.CurrentRecord, KnobManager.Instance.RPMControl.CurrentRecordSpinSpeed, KnobManager.Instance.RPMControl.TargetRPM);
                        if (TurntableManager.Instance.TonearmOnRecord) KnobManager.Instance.TonearmControl.TrackTonearm();
                    }
                }
                else
                {
                    TurntableManager.Instance.TurntableSystem.StopRecord();
                    KnobManager.Instance.TonearmControl.StopTonearm();
                    StartStopRecord();
                }
            }

            yield return null;
        }
    }

}
