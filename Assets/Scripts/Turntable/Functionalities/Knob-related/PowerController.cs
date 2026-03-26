using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(KnobControl))]
public class PowerController : MonoBehaviour
{

    [Header("Power Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float powerOffKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float powerOnKnobAngle;


    // References
    private KnobControl knobControl;
    private Power currentPower = Power.Off;
    public Power CurrentPower => currentPower;


    // Scripts
    [SerializeField] private LightControl powerLightControl;


    private void Awake()
    {
        knobControl = GetComponent<KnobControl>();
    }


    private void Start()
    {
        UpdatePowerStatus();
        TurntableManager.Instance.InitializeKnobAngle(this.gameObject, powerOffKnobAngle);
    }


    public void TurnKnob(float rotateSpeed) 
    {
        knobControl.UpdateRotationClamped(transform, powerOffKnobAngle, powerOnKnobAngle, rotateSpeed);

        IsPowerOff(powerOffKnobAngle, powerOnKnobAngle); // To set CurrentPower status
        UpdatePowerStatus();
    }


    public void ProcessPowerKnob(float rotateSpeed)
    {
        float targetAngle = IsPowerOff(powerOffKnobAngle, powerOnKnobAngle) ? powerOffKnobAngle : powerOnKnobAngle;
        knobControl.CorrectRotation(gameObject, targetAngle, rotateSpeed);
        
        UpdatePowerStatus();
    }


    public void UpdatePowerStatus()  // Turntable power system handler
    {
        if (currentPower == Power.Off)
        {
            TurntableManager.Instance.TurnedOn = false;
            TurntableManager.Instance.TurntableSystem.StopRecord();
            
            KnobManager.Instance.TonearmControl.StopTonearm();
            KnobManager.Instance.RPMControl.ResetRPMStatus();

            DisplayerManager.Instance.Displayer.UpdatePowerDisplay(isOn: false);

            powerLightControl.Renderer.color = Color.black;
            powerLightControl.LightSetting.color = Color.black;
        }
        else if (currentPower == Power.On)
        {
            // TurnedOn is now managed by Displayer.cs

            if (TurntableManager.Instance.TurnedOn)
            {
                // RPM DISPLAY
                KnobManager.Instance.RPMControl.ProcessRPMKnob(KnobManager.Instance.RotationSpeed);

                // VOLUME DISPLAY
                KnobManager.Instance.VolumeControl.ProcessVolumeKnob();

                // REVERB DISPLAY
                DisplayerManager.Instance.Displayer.UpdateReverbDisplay(ButtonManager.Instance.ReverbControl.IsReverb);

                // READ RECORD DISPLAY
                DisplayerManager.Instance.Displayer.UpdateRecordDisplay(TurntableManager.Instance.EquipRecord);

                //// PLAY & PAUSE DISPLAY
                //DisplayerManager.Instance.Displayer.UpdatePlayPauseDisplay();
            }

            DisplayerManager.Instance.Displayer.UpdatePowerDisplay(isOn: true); 

            powerLightControl.Renderer.color = Color.red;
            powerLightControl.LightSetting.color = Color.red;
        }
    }


    // Helper
    private bool IsPowerOff(float offAngle, float onAngle)
    {
        if (knobControl == null) return false;

        float currentAngle = knobControl.NormalizeClockwise(-transform.eulerAngles.z);

        float toMin = Mathf.Abs(Mathf.DeltaAngle(currentAngle, offAngle));
        float toMax = Mathf.Abs(Mathf.DeltaAngle(currentAngle, onAngle));

        if (toMin < toMax) currentPower = Power.Off;
        else currentPower = Power.On;

        return toMin < toMax;
    }

    private void OnDrawGizmos()
    {
        Vector2 powerOffKnobDir = new Vector2(Mathf.Cos(-(powerOffKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(powerOffKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + powerOffKnobDir * 2.5f);

        Vector2 powerOnKnobDir = new Vector2(Mathf.Cos(-(powerOnKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(powerOnKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + powerOnKnobDir * 2.5f);
    }
}
