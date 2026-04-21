using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(KnobControl))]
public class RPMController : MonoBehaviour
{

    [Header("RPM Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float slowedRPMKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float normalRPMKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float spedUpRPMKnobAngle;


    [Header("Playback Pitch Values")]
    [SerializeField] private float slowPitchValue;
    [SerializeField] private float normalPitchValue;
    [SerializeField] private float spedUpPitchValue;

    public float SlowedPitchValue => slowPitchValue;
    public float NormalPitchValue => normalPitchValue;
    public float SpedUpPitchValue => spedUpPitchValue;


    // References
    private KnobControl knobControl;


    // Values
    [HideInInspector] public RPM CurrentRPM = RPM.None;
    [HideInInspector] public RPM TargetRPM = RPM.Normal;

    [HideInInspector] public float CurrentRPMPitch = 1f;

    [HideInInspector] public bool UpdatedRPM = true; // Modified in RPMController.cs --> Validate new RPM
    
    private float currentRecordSpinSpeed = Global.RecordPlayingValue.NormalSpinSpeed;
    public float CurrentRecordSpinSpeed => currentRecordSpinSpeed;  


    // Coroutine
    private Coroutine rpmPitchCoroutine = null;


    // Scripts
    [SerializeField] private LightControl slowedLightControl;
    [SerializeField] private LightControl normalLightControl;
    [SerializeField] private LightControl spedUpLightControl;


    private void Awake()
    {
        knobControl = GetComponent<KnobControl>();
    }


    void Start()
    {
        CurrentRPMPitch = KnobManager.Instance.RPMControl.NormalPitchValue;

        TurntableManager.Instance.InitializeKnobAngle(this.gameObject, normalRPMKnobAngle);
        ResetRPMStatus();
    }


    public void TurnKnob(float rotateSpeed)
    {
        knobControl.UpdateRotationClamped(transform, slowedRPMKnobAngle, spedUpRPMKnobAngle, rotateSpeed);

        TargetRPMRotation(slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle);
        UpdateRPMStatus();
    }


    public void ProcessRPMKnob(float rotateSpeed)
    {
        float targetAngle = TargetRPMRotation(slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle);
        knobControl.CorrectRotation(gameObject, targetAngle, rotateSpeed);
        UpdateRPMStatus();
    }


    public void UpdateRPMStatus()
    {
        if (TurntableManager.Instance.TurnedOn)
        {
            if (TargetRPM == RPM.Slowed && CurrentRPM != RPM.Slowed)
            {
                currentRecordSpinSpeed = Global.RecordPlayingValue.SlowedSpinSpeed;
                CurrentRPM = RPM.Slowed;

                UpdateRPMPitch(CurrentRPM);

                // Light Settings
                slowedLightControl.LightSetting.color = Color.red;
                slowedLightControl.Renderer.color = Color.red;
            }
            else if (TargetRPM == RPM.Normal && CurrentRPM != RPM.Normal)
            {
                currentRecordSpinSpeed = Global.RecordPlayingValue.NormalSpinSpeed;
                CurrentRPM = RPM.Normal;

                UpdateRPMPitch(CurrentRPM);

                // Light Settings
                normalLightControl.LightSetting.color = Color.red;
                normalLightControl.Renderer.color = Color.red;
            }
            else if (TargetRPM == RPM.SpedUp && CurrentRPM != RPM.Normal)
            {
                currentRecordSpinSpeed = Global.RecordPlayingValue.SpedUpSpinSpeed;
                CurrentRPM = RPM.SpedUp;

                UpdateRPMPitch(CurrentRPM);

                // Light Settings
                spedUpLightControl.LightSetting.color = Color.red;
                spedUpLightControl.Renderer.color = Color.red;
            }
            else Debug.Log("Called");

            UpdatedRPM = true;
            TurntableManager.Instance.DisplayerControl.UpdateRPMDisplay(TargetRPM);
        }

    }


    private void UpdateRPMPitch(RPM currentRPM)
    {
        ResetRPMStatus();

        if (rpmPitchCoroutine != null)
        {
            StopCoroutine(rpmPitchCoroutine);
            rpmPitchCoroutine = null;
        }

        rpmPitchCoroutine = StartCoroutine(UpdatingRPMPitch(currentRPM));
    }


    private IEnumerator UpdatingRPMPitch(RPM currentRPM)
    {
        float currentTime = 0f;
        float targetPitch;

        if (currentRPM == RPM.Slowed) targetPitch = SlowedPitchValue;
        else if (currentRPM == RPM.Normal) targetPitch = NormalPitchValue;
        else if (currentRPM == RPM.SpedUp) targetPitch = SpedUpPitchValue;
        else targetPitch = 1f;

        while (currentTime < Global.RecordPlayingValue.MaxInterpolationTime)
        {
            currentTime += Time.deltaTime;
            float percent = currentTime / Global.RecordPlayingValue.MaxInterpolationTime;

            // Record playback speed
            float currentPitch = CurrentRPMPitch;
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, percent);
            CurrentRPMPitch = currentPitch;


            yield return null;
        }

        CurrentRPMPitch = targetPitch;
        rpmPitchCoroutine = null;
    }


    public void ResetRPMStatus()
    {
        CurrentRPM = RPM.None;

        slowedLightControl.LightSetting.color = Color.black;
        slowedLightControl.Renderer.color = Color.black;

        normalLightControl.LightSetting.color = Color.black;
        normalLightControl.Renderer.color = Color.black;

        spedUpLightControl.LightSetting.color = Color.black;
        spedUpLightControl.Renderer.color = Color.black;
    }


    private float TargetRPMRotation(float slowedAngle, float normalAngle, float spedUpAngle)
    {
        float currentAngle = knobControl.NormalizeClockwise(-transform.eulerAngles.z);

        float toSlowed = Mathf.Abs(Mathf.DeltaAngle(currentAngle, slowedAngle));
        float toNormal = Mathf.Abs(Mathf.DeltaAngle(currentAngle, normalAngle));
        float toSpedUp = Mathf.Abs(Mathf.DeltaAngle(currentAngle, spedUpAngle));

        float targetAngle = slowedAngle;
        float smallest = toSlowed;

        if (toNormal < smallest)
        {
            smallest = toNormal;
            targetAngle = normalAngle;
        }

        if (toSpedUp < smallest)
        {
            targetAngle = spedUpAngle;
        }

        // Light Settings
        if (targetAngle == slowedAngle) TargetRPM = RPM.Slowed;
        else if (targetAngle == normalAngle) TargetRPM = RPM.Normal;
        else if (targetAngle == spedUpAngle) TargetRPM = RPM.SpedUp;

        return targetAngle;
    }


    private void OnDrawGizmos()
    {
        Vector2 slowedRPMKnobDir = new Vector2(Mathf.Cos(-(slowedRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(slowedRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + slowedRPMKnobDir * 2.5f);

        Vector2 normalRPMKnobDir = new Vector2(Mathf.Cos(-(normalRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(normalRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + normalRPMKnobDir * 2.5f);

        Vector2 spedUpRPMKnobDir = new Vector2(Mathf.Cos(-(spedUpRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(spedUpRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + spedUpRPMKnobDir * 2.5f);
    }
}
