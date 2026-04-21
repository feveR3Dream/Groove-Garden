using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TurntableInDepthSystem))]
public class TurntableManager : MonoBehaviour
{
    public static TurntableManager Instance { get; private set; }


    [Header("Displayer Reference")]
    [SerializeField] private DisplayerControl displayerControl;
    public DisplayerControl DisplayerControl => displayerControl;


    // Turntable System Var
    private TurntableInDepthSystem turntableSystem;
    public TurntableInDepthSystem TurntableSystem => turntableSystem;


    // <Important> Values
    [HideInInspector] public bool TurnedOn = false; // Will be modified in PowerController.cs --> Validate power ON
    
    [HideInInspector] public bool EquipRecord = false; // Modified in Record.cs --> Validate music playability

    [HideInInspector] public bool RecordRead = false;


    // Tonearm On Record Check --> Validate tonearm movement during track playback
    [HideInInspector] public bool TonearmOnRecord = false;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        Initialization();
    }


    private void Initialization()
    {
        turntableSystem = GetComponent<TurntableInDepthSystem>();

        // ADD MORE HERE
    }


    public void InitializeKnobAngle(GameObject knobGO, float angleValue)
    {
        knobGO.transform.rotation = Quaternion.Euler(0f, 0f, -angleValue);
    }


    #region BUTTON REGISTRATION
    //---------------------

    public void ButtonDown(ButtonControl buttonControl)
    {

    }

    public void ButtonHold(ButtonControl buttonControl)
    {
        if (!buttonControl.TurnedOn)
        {
            buttonControl.UpdateButtonAlpha(buttonControl.ButtonPressedOnAlpha, buttonControl.AlphaFadeSpeed);
        }
        else
        {
            buttonControl.UpdateButtonAlpha(buttonControl.ButtonPressedOffAlpha, buttonControl.AlphaFadeSpeed);
        }

        if (buttonControl.gameObject != ButtonManager.Instance.PlatterGO)
            buttonControl.UpdateButtonSize(buttonControl.PressedSize, buttonControl.ResizeSpeed);
    }

    public void ButtonUp(ButtonControl buttonControl)
    {
        // FOR PLATTER
        if (buttonControl.gameObject == ButtonManager.Instance.PlatterGO)
        {
            buttonControl.UpdateButtonAlpha(buttonControl.ButtonIdleOffAlpha, buttonControl.AlphaFadeSpeed);
            RecordInteraction();

            Debug.Log("Stacked");
        }

        
        // FOR OTHERS
        else
        {

            // GENERAL
            if (buttonControl.TurnedOn)
            {
                buttonControl.UpdateButtonAlpha(buttonControl.ButtonIdleOnAlpha, buttonControl.AlphaFadeSpeed);
                buttonControl.UpdateButtonSize(buttonControl.IdleSizeON, buttonControl.ResizeSpeed);
            }
            else
            {
                buttonControl.UpdateButtonAlpha(buttonControl.ButtonIdleOffAlpha, buttonControl.AlphaFadeSpeed);
                buttonControl.UpdateButtonSize(buttonControl.IdleSizeOFF, buttonControl.ResizeSpeed);
            }
            


            // START STOP BUTTON
            if (buttonControl.gameObject == ButtonManager.Instance.StartStopGO)
            {
                ButtonManager.Instance.StartStopControl.StartStopRecord();
            }
            


            // REVERB BUTTON
            if (buttonControl.gameObject == ButtonManager.Instance.ReverbGO)
            {
                ButtonManager.Instance.ReverbControl.Reverb();
            }
            
        }
        
    }

    private void RecordInteraction() // For moving record in/out of the turntable
    {
        if (/*RecordManager.Instance.CurrentVinylRecord != null*/
            RecordManager.Instance.CurrentRecord != null &&
            RecordManager.Instance.RecordMoveable)
        {

            Record record = RecordManager.Instance.CurrentRecord;  // shorter for readability

            EquipRecord = !EquipRecord;
            RecordMoveTo recordMoveTo = EquipRecord ? RecordMoveTo.To_Turntable : RecordMoveTo.To_Mouse;

            record.MoveTo(recordMoveTo, RecordManager.Instance.RecordMoveDuration);

            if (!EquipRecord)
            {
                UIManager.Instance.UpdateRecordCoverEquip(true);
            }
            else
            {
                UIManager.Instance.UpdateRecordCoverEquip(false);
            }

            if (TurnedOn) TurntableManager.Instance.DisplayerControl.UpdateRecordDisplay(EquipRecord);
        }

        Debug.Log($"Equip Record: {EquipRecord}");
    }


    //--------
    #endregion



    #region KNOB REGISTRATION
    //-----------------------

    public void KnobDown(KnobControl knobControl)
    {
        // TONEARM
        if (knobControl.gameObject == KnobManager.Instance.TonearmGO)
        {
            KnobManager.Instance.TonearmControl.LiftTonearm();
        }

    }

    public void KnobHold(KnobControl knobControl)
    {

        // TONEARM
        if (knobControl.gameObject == KnobManager.Instance.TonearmControl.gameObject)
        {
            KnobManager.Instance.TonearmControl.MoveTonearm(KnobManager.Instance.RotationSpeed);
        }


        // POWER
        if (knobControl.gameObject == KnobManager.Instance.PowerControl.gameObject)
        {
            KnobManager.Instance.PowerControl.TurnKnob(KnobManager.Instance.RotationSpeed);
        }


        // RPM
        if (knobControl.gameObject == KnobManager.Instance.RPMControl.gameObject)
        {
            KnobManager.Instance.RPMControl.TurnKnob(KnobManager.Instance.RotationSpeed);
        }


        // VOLUME
        if (knobControl.gameObject == KnobManager.Instance.VolumeControl.gameObject)
        {
            KnobManager.Instance.VolumeControl.TurnKnob(KnobManager.Instance.RotationSpeed);
        }

    }

    public void KnobUp(KnobControl knobControl)
    {
        // TONEARM
        if (knobControl.gameObject == KnobManager.Instance.TonearmControl.gameObject)
        {
            KnobManager.Instance.TonearmControl.ReleasedTonearm(KnobManager.Instance.RotationSpeed);
        }


        // POWER
        if (knobControl.gameObject == KnobManager.Instance.PowerControl.gameObject)
        {
            KnobManager.Instance.PowerControl.ProcessPowerKnob(KnobManager.Instance.RotationSpeed);
        }


        // RPM
        if (knobControl.gameObject == KnobManager.Instance.RPMControl.gameObject)
        {
            KnobManager.Instance.RPMControl.ProcessRPMKnob(KnobManager.Instance.RotationSpeed);
        }


        // VOLUME
        if (knobControl.gameObject == KnobManager.Instance.VolumeControl.gameObject)
        {
            KnobManager.Instance.VolumeControl.ReleasedVolumeKnob();
        }

    }

    //--------
    #endregion

}
