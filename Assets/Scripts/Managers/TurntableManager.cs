using System.Collections;
using UnityEngine;


[RequireComponent(typeof(TurntableInDepthSystem))]
public class TurntableManager : MonoBehaviour
{
    public static TurntableManager Instance { get; private set; }

    [Header("Button Reference")]
    [Tooltip("Record placement position, select platter from the turntable")]
    [SerializeField] private GameObject platter; 
    public Vector2 RecordPlacementPosition => platter.transform.position; // Record script access property
    [SerializeField] private GameObject startStopButton;
    [SerializeField] private GameObject reverbButton;


    [Header("Tonearm Reference")]
    [SerializeField] private GameObject tonearm;
    [SerializeField] private Transform interactionPoint;


    [Header("Knob Reference")]
    [SerializeField] private GameObject powerKnob;
    [SerializeField] private GameObject powerLight;
    [Space]
    [SerializeField] private GameObject rpmKnob;
    [SerializeField] private GameObject slowedLight;
    [SerializeField] private GameObject normalLight;
    [SerializeField] private GameObject spedUpLight;
    [Space]
    [SerializeField] private GameObject volumeKnob;


    [Header("Layer References")]
    [SerializeField] private LayerMask turntableLayer;


    [Header("Knob Rotation Speed Value")]
    [SerializeField] private float rotationSpeed;


    [Header("Playback Pitch Values")]
    [SerializeField] private float slowPitchValue;
    public float SlowedPitchValue => slowPitchValue;

    [SerializeField] private float normalPitchValue;
    public float NormalPitchValue => normalPitchValue;  
    
    [SerializeField] private float spedUpPitchValue;
    public float SpedUpPitchValue => spedUpPitchValue;


    [Header("Tonearm Value")]
    [Range(0f, 360f)]
    [SerializeField] private float defaultTonearmAngle;
    [Range(0f, 360f)]
    [SerializeField] private float startTonearmAngle;
    [Range(0f, 360f)]
    [SerializeField] private float endTonearmAngle;


    [Header("Power Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float powerOffKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float powerOnKnobAngle;


    [Header("RPM Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float slowedRPMKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float normalRPMKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float spedUpRPMKnobAngle;


    [Header("Volume Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float minVolumeKnobAngle; // 0
    [Range(0f, 360f)]
    [SerializeField] private float maxVolumeKnobAngle; // 1


    // References
    public RPM TargetRPM { get; private set; } = RPM.Normal;
    public Power CurrentPower { get; private set; } = Power.Off;
    private TurntableInDepthSystem turntableSystem;


    // REALLY IMPORTANT
    private Record record;
    private Camera cam;


    // Values
    [HideInInspector] public bool TurnedOn = false;
    [HideInInspector] public bool CanPlayMusic = false;
    [HideInInspector] public bool UpdatedRPM = true;
    [HideInInspector] public bool EquipRecord = false; // Main values that checks music playability
   
    private float currentRecordSpinSpeed = Global.NormalSpinSpeed;
    private float currentRPMPitch = 1f;
    private RPM currentRPM = RPM.None;


    // Start Stop Button Values
    private bool resetStartStopButton = true;
    private bool tonearmAtTheEnd = false;
    private bool isPlaying = false;


    // Revern Button Values
    private bool isReverb = false;


    // Turntable Setting Coroutines
    private Coroutine powerKnobCoroutine = null;
    private Coroutine rpmKnobCoroutine = null;


    // Playing Record & Anything Relevant Coroutines
    private Coroutine startStopRecordCoroutine = null;
    private Coroutine moveTonearmCoroutine = null;
    private Coroutine rpmPitchCoroutine = null;


    // Scripts
    private LightControl powerLightControl;
    private LightControl slowedLightControl;
    private LightControl normalLightControl;
    private LightControl spedUpLightControl;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = Camera.main;
        turntableSystem = this.GetComponent<TurntableInDepthSystem>();

        // Add ButtonControl Component
        if (platter != null) platter.AddComponent<ButtonControl>();
        if (startStopButton != null) startStopButton.AddComponent<ButtonControl>();
        if (reverbButton != null) reverbButton.AddComponent<ButtonControl>();

        // Add KnobControl Component
        if (tonearm != null) tonearm.AddComponent<KnobControl>(); 
        if (powerKnob != null) powerKnob.AddComponent<KnobControl>(); 
        if (rpmKnob != null) rpmKnob.AddComponent<KnobControl>(); 
        if (volumeKnob != null) volumeKnob.AddComponent<KnobControl>();

        // Add LightControl Component
        if (powerLight != null) { powerLight.AddComponent<LightControl>(); powerLightControl = powerLight.GetComponent<LightControl>();  }
        if (slowedLight != null) { slowedLight.AddComponent<LightControl>(); slowedLightControl = slowedLight.GetComponent<LightControl>(); }
        if (normalLight != null) { normalLight.AddComponent<LightControl>(); normalLightControl = normalLight.GetComponent<LightControl>(); }
        if (spedUpLight != null) { spedUpLight.AddComponent<LightControl>(); spedUpLightControl = spedUpLight.GetComponent<LightControl>(); }

        // Knob Angle Initialization
        InitializeKnobAngle(tonearm, defaultTonearmAngle);
        InitializeKnobAngle(powerKnob, powerOffKnobAngle);
        InitializeKnobAngle(rpmKnob, normalRPMKnobAngle);
        InitializeKnobAngle(volumeKnob, (maxVolumeKnobAngle + minVolumeKnobAngle) / 2);

        // Turntable System Initialization
        UpdatePowerStatus(CurrentPower);
        UpdateRPMStatus(TargetRPM);

        // Variable Initialization
        currentRPMPitch = NormalPitchValue;

    }


    private void InitializeKnobAngle(GameObject knobGO, float angleValue)
    {
        knobGO.transform.rotation = Quaternion.Euler(0f, 0f, -angleValue);
    }


    private void Start()
    {
        turntableSystem.ProcessReverb(isReverb);
    }



    #region CORE FUNCTIONALITIES
    //--------------------------


    #region BUTTON FUNCTIONALITIES
    public void ButtonDown(ButtonControl buttonControl)
    {
        // FOR FUTURE IMPLEMENTATION
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

        if (buttonControl.gameObject != platter)
            buttonControl.UpdateButtonSize(buttonControl.PressedSize, buttonControl.ResizeSpeed);
    }
    public void ButtonUp(ButtonControl buttonControl)
    {
        #region FOR PLATTER
        if (buttonControl.gameObject == platter)
        {
            buttonControl.UpdateButtonAlpha(buttonControl.ButtonIdleOffAlpha, buttonControl.AlphaFadeSpeed);
            RecordInteraction();
        }
        #endregion

        #region FOR OTHERS
        else 
        {
            { 
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
            } // Button Animation


            #region FOR START STOP BUTTON
            if (buttonControl.gameObject == startStopButton)
            {
                StartStopRecord(tonearmAtTheEnd = false);
            }
            #endregion


            #region FOR REVERB BUTTON
            if (buttonControl.gameObject == reverbButton)
            {
                turntableSystem.ProcessReverb(isReverb = !isReverb);
            }
            #endregion
        }
        #endregion
    }
    private void RecordInteraction() // For moving record in/out of the turntable
    {
        if (RecordManager.Instance.CurrentVinylRecord != null &&
            RecordManager.Instance.RecordMoveable)
        {
            record = RecordManager.Instance.CurrentRecord;

            EquipRecord = !EquipRecord;
            RecordMoveTo recordMoveTo = EquipRecord ? RecordMoveTo.To_Turntable : RecordMoveTo.To_Mouse;

            record.MoveTo(recordMoveTo, RecordManager.Instance.RecordMoveSpeed);

            if (!EquipRecord) 
            {
                UIManager.Instance.UpdateRecordCoverEquip(true);
                record = null; 
            }
            else
            {
                UIManager.Instance.UpdateRecordCoverEquip(false);

            }
        }

        Debug.Log($"Equip Record: {EquipRecord}");
    }
    #endregion



    #region KNOB FUNCTIONALITIES
    public void KnobDown(KnobControl knobControl)
    {
        #region TONEARM 
        if (knobControl.gameObject == tonearm)
        {
            CanPlayMusic = false;

            StopTonearm();
            turntableSystem.StopRecord();

            Collider2D platterCollider = platter.GetComponent<Collider2D>();
            if (!platterCollider.enabled)
                platterCollider.enabled = true;

            knobControl.StopResetRotation();
            knobControl.UpdateKnobAlpha(0.25f, knobControl.AlphaFadeSpeed);

            interactionPoint.gameObject.SetActive(true);
        }
        #endregion

        knobControl.SetAngleOffset();
    }
    public void KnobHold(KnobControl knobControl)
    {
        #region TONEARM
        if (knobControl.gameObject == tonearm)
        {
            knobControl.UpdateRotationClamped(knobControl.transform, defaultTonearmAngle, endTonearmAngle, rotationSpeed);
        }
        #endregion


        #region POWER KNOB
        else if (knobControl.gameObject == powerKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.transform, powerOffKnobAngle, powerOnKnobAngle, rotationSpeed);

            KnobIsOffAngle(knobControl.gameObject, powerOffKnobAngle, powerOnKnobAngle);
            UpdatePowerStatus(CurrentPower);
        }
        #endregion


        #region RPM KNOB
        else if (knobControl.gameObject == rpmKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.transform, slowedRPMKnobAngle, spedUpRPMKnobAngle, rotationSpeed);

            TargetRPMRotation(knobControl.gameObject, slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle);
            UpdateRPMStatus(TargetRPM);
        }
        #endregion


        #region VOLUME KNOB
        else if (knobControl.gameObject == volumeKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.transform, minVolumeKnobAngle, maxVolumeKnobAngle, rotationSpeed);
            ProcessVolumeKnob(knobControl, minVolumeKnobAngle, maxVolumeKnobAngle);
        }
        #endregion
    }
    public void KnobUp(KnobControl knobControl)
    {
        #region TONEARM
        if (knobControl.gameObject == tonearm)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(interactionPoint.position, Vector2.zero, 10f, turntableLayer);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == tonearm) continue;

                if (hit.collider.gameObject == platter)
                {
                    if (record == null)
                    {
                        knobControl.ResetRotation(knobControl.gameObject, defaultTonearmAngle, rotationSpeed);
                    }
                    else
                    {
                        tonearmAtTheEnd = IsTonearmAtTheEnd();
                        CanPlayMusic = true;

                        Collider2D platterCollider = platter.GetComponent<Collider2D>();
                        if (platterCollider != null)
                            platterCollider.enabled = false;
                    }

                    knobControl.UpdateKnobAlpha(1f, knobControl.AlphaFadeSpeed);

                    interactionPoint.gameObject.SetActive(false);

                    return;
                }
            }

            knobControl.ResetRotation(knobControl.gameObject, defaultTonearmAngle, rotationSpeed);
            knobControl.UpdateKnobAlpha(1f, knobControl.AlphaFadeSpeed);

            interactionPoint.gameObject.SetActive(false);
        }
        #endregion


        #region POWER KNOB
        else if (knobControl.gameObject == powerKnob)
        {
            ProcessPowerKnob(powerKnob, powerOffKnobAngle, powerOnKnobAngle, rotationSpeed);
            UpdatePowerStatus(CurrentPower);
        }
        #endregion


        #region RPM KNOB
        else if (knobControl.gameObject == rpmKnob)
        {
            ProcessRPMKnob(rpmKnob, slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle, rotationSpeed);
            UpdateRPMStatus(TargetRPM);
        }
        #endregion
    }
    #endregion


    //--------
    #endregion



    #region IN-DEPTH SYSTEM FUNCTIONALITIES
    //-------------------------------------

    #region TONEARM FUNCTIONS
    private void MoveTonearm()
    {
        if (moveTonearmCoroutine == null)
            moveTonearmCoroutine = StartCoroutine(MovingTonearm());
    }
    private IEnumerator MovingTonearm()
    { 
        Quaternion startRotation = tonearm.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, 360f - endTonearmAngle);

        float totalDuration = record.RecordTrack.length;
        float currentTimeMark = turntableSystem.VinylSpeaker.time;
        float duration = totalDuration - currentTimeMark;

        if (duration <= 0f)
        {
            moveTonearmCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * currentRPMPitch;
            float percent = elapsedTime / duration;

            tonearm.transform.rotation = Quaternion.Lerp(startRotation, endRotation, percent);
            yield return null;
        }

        tonearm.transform.rotation = endRotation;
        tonearmAtTheEnd = true;

        moveTonearmCoroutine = null;
    }

    private void StopTonearm()
    {
        if (moveTonearmCoroutine != null)
        {
            float totalDuration = record.RecordTrack.length;
            float currentTimeMark = turntableSystem.VinylSpeaker.time;
            float duration = totalDuration - currentTimeMark;

            Debug.Log($"Stop Track Length: {duration}");

            StopCoroutine(moveTonearmCoroutine);
            moveTonearmCoroutine = null;
        }
    }


    #endregion



    #region POWER KNOB FUNCTIONS
    private void ProcessPowerKnob(GameObject powerKnobGO, float offAngle, float onAngle, float rotationSpeed)
    {
        if (powerKnobCoroutine != null)
        {
            StopCoroutine(powerKnobCoroutine);
            powerKnobCoroutine = null;
        }

        powerKnobCoroutine = StartCoroutine(ProcessingPowerKnob(powerKnobGO, offAngle, onAngle, rotationSpeed));
    }
    private IEnumerator ProcessingPowerKnob(GameObject powerKnobGO, float offAngle, float onAngle, float rotationSpeed)
    {
        float targetAngle = KnobIsOffAngle(powerKnobGO, offAngle, onAngle) ? offAngle : onAngle;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, -targetAngle);

        while (Quaternion.Angle(powerKnobGO.transform.rotation, targetRot) > 0.1f)
        {
            powerKnobGO.transform.rotation =
                Quaternion.Lerp(
                    powerKnobGO.transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime);

            yield return null;
        }

        UpdatePowerStatus(CurrentPower);

        powerKnobGO.transform.rotation = targetRot;
        powerKnobCoroutine = null;
    }
    private void UpdatePowerStatus(Power currentPower)  // Turntable power system power handler
    {
        if (currentPower == Power.Off) 
        {
            TurnedOn = false;

            turntableSystem.StopRecord();

            powerLightControl.Renderer.color = Color.black;
            powerLightControl.LightSetting.color = Color.black;
        }
        else if (currentPower == Power.On)  
        {
            TurnedOn = true;

            powerLightControl.Renderer.color = Color.red;
            powerLightControl.LightSetting.color = Color.red;
        }
    }
    // Helper
    private bool KnobIsOffAngle(GameObject goRef, float minAngle, float maxAngle)
    {
        KnobControl knobControl = goRef.GetComponent<KnobControl>();
        if (knobControl == null) return false;

        float currentAngle = knobControl.NormalizeClockwise(-goRef.transform.eulerAngles.z);

        float toMin = Mathf.Abs(Mathf.DeltaAngle(currentAngle, minAngle));
        float toMax = Mathf.Abs(Mathf.DeltaAngle(currentAngle, maxAngle));

        if (toMin < toMax)
        {
            CurrentPower = Power.Off;
            ResetRPMStatus();
        }
        else
        {
            CurrentPower = Power.On;
            UpdateRPMStatus(TargetRPM);
        }

        return toMin < toMax;
    }
    #endregion



    #region RPM KNOB FUNCTIONS
    private void ProcessRPMKnob(GameObject rpmKnobGO,
        float slowedAngle,
        float normalAngle,
        float spedUpAngle,
        float rotationSpeed)
    {
        if (rpmKnobCoroutine != null)
        {
            StopCoroutine(rpmKnobCoroutine);
            rpmKnobCoroutine = null;
        }

        rpmKnobCoroutine = StartCoroutine(ProcessingRPMKnob(rpmKnobGO, slowedAngle, normalAngle, spedUpAngle, rotationSpeed));
    }
    private IEnumerator ProcessingRPMKnob(
        GameObject rpmKnobGO,
        float slowedAngle,
        float normalAngle,
        float spedUpAngle,
        float rotationSpeed)
    {
        Quaternion targetRot = Quaternion.Euler(0f, 0f, -TargetRPMRotation(rpmKnobGO, slowedAngle, normalAngle, spedUpAngle));

        while (Quaternion.Angle(rpmKnobGO.transform.rotation, targetRot) > 0.1f)
        {
            rpmKnobGO.transform.rotation =
                Quaternion.Lerp(
                    rpmKnobGO.transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime);

            yield return null;
        }

        rpmKnobGO.transform.rotation = targetRot;
        rpmKnobCoroutine = null;
    }
    private void UpdateRPMStatus(RPM targetRPM)
    {
        if (CurrentPower == Power.On)
        {
            if (targetRPM == RPM.Slowed && currentRPM != RPM.Slowed)
            {
                currentRecordSpinSpeed = Global.SlowedSpinSpeed;
                currentRPM = RPM.Slowed;
                UpdateRPMPitch(currentRPM);

                // Light Settings
                slowedLightControl.LightSetting.color = Color.red;
                slowedLightControl.Renderer.color = Color.red;
            }
            else if (targetRPM == RPM.Normal && currentRPM != RPM.Normal)
            {
                currentRecordSpinSpeed = Global.NormalSpinSpeed;
                currentRPM = RPM.Normal;
                UpdateRPMPitch(currentRPM);

                // Light Settings
                normalLightControl.LightSetting.color = Color.red;
                normalLightControl.Renderer.color = Color.red;
            }
            else if (targetRPM == RPM.SpedUp && currentRPM != RPM.SpedUp)
            {
                currentRecordSpinSpeed = Global.SpedUpSpinSpeed;
                currentRPM = RPM.SpedUp;
                UpdateRPMPitch(currentRPM);

                // Light Settings
                spedUpLightControl.LightSetting.color = Color.red;
                spedUpLightControl.Renderer.color = Color.red;
            }

            UpdatedRPM = true;
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

        while (currentTime < Global.MaxInterpolationTime)
        {
            currentTime += Time.deltaTime;
            float percent = currentTime / Global.MaxInterpolationTime;

            // Record playback speed
            currentRPMPitch = Mathf.Lerp(currentRPMPitch, targetPitch, percent);

            yield return null;
        }

        currentRPMPitch = targetPitch;
        rpmPitchCoroutine = null;
    }

    private void ResetRPMStatus()
    {
        currentRPM = RPM.None;

        slowedLightControl.LightSetting.color = Color.black;
        slowedLightControl.Renderer.color = Color.black;

        normalLightControl.LightSetting.color = Color.black;
        normalLightControl.Renderer.color = Color.black;

        spedUpLightControl.LightSetting.color = Color.black;
        spedUpLightControl.Renderer.color = Color.black;
    }
    private float TargetRPMRotation(GameObject rpmKnobGO, float slowedAngle, float normalAngle, float spedUpAngle)
    {
        KnobControl knobControl = rpmKnobGO.GetComponent<KnobControl>();
        float currentAngle = knobControl.NormalizeClockwise(-rpmKnobGO.transform.eulerAngles.z);

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
    #endregion



    #region VOLUME KNOB FUNCTIONS
    private void ProcessVolumeKnob(KnobControl knobControl, float minVolumeAngle, float maxVolumeAngle)
    {
        Vector2 dir = knobControl.transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = knobControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle; 

        float volume = Mathf.InverseLerp(minVolumeAngle, maxVolumeAngle, convertedAngle);
        turntableSystem.UpdateVolume(volume);
    }
    #endregion



    #region START STOP BUTTON FUNCTIONS
    private void StartStopRecord(bool tonearmAtEnd = false)
    {
        if (startStopRecordCoroutine != null)
        {
            StopCoroutine(startStopRecordCoroutine);
            startStopRecordCoroutine = null;
        }

        if (!isPlaying)
        {
            isPlaying = true;

            resetStartStopButton = true;
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

            StopTonearm();
            turntableSystem.StopRecord();

            // Only for when the tonearm reaches the end
            if (tonearmAtEnd) ResetStartStopButton();
        }
    }

    private void ResetStartStopButton()
    {
        if (resetStartStopButton)
        {
            resetStartStopButton = false;

            ButtonControl btn = startStopButton.GetComponent<ButtonControl>();

            isPlaying = false;
            btn.TurnedOn = false;

            btn.UpdateButtonAlpha(btn.ButtonIdleOffAlpha, btn.AlphaFadeSpeed);
            btn.UpdateButtonSize(btn.IdleSizeOFF, btn.ResizeSpeed);
        }
    }

    // HANDLES PLAYING TRACK FROM RECORD---<< MAIN >>
    private IEnumerator ProcessingRecord()
    {
        while (true)
        {
            if (TurnedOn && 
                record != null)
            {
                if (!tonearmAtTheEnd)
                {
                    turntableSystem.PlayRecord(record, currentRecordSpinSpeed, TargetRPM);
                    if (CanPlayMusic) MoveTonearm();
                }
                else
                {
                    turntableSystem.StopRecord();
                    StartStopRecord(tonearmAtTheEnd = true);
                    StopTonearm();
                }
            }

            yield return null;
        }
    }

    #endregion


    
    // REVERB FUNCTIONALITY IN TURNTABLE_IN_DEPTH_SYSTEM SCRIPT

    //--------
    #endregion



    #region HELPER FUNCTIONALITIES
    //----------------------------


    #region GET TRACK TIME MARK 
    public float GetTimeMark()
    {
        KnobControl tonearmControl = tonearm.GetComponent<KnobControl>();

        Vector2 dir = tonearm.transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = tonearmControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle;

        float percent = Mathf.InverseLerp(startTonearmAngle, endTonearmAngle, convertedAngle);
        float recordTimeMark = record.RecordTrack.length * percent;

        return recordTimeMark;
    }
    #endregion



    #region CHECK TONEARM END AT END CONDITION
    private bool IsTonearmAtTheEnd()
    {
        KnobControl tonearmControl = tonearm.GetComponent<KnobControl>();

        Vector2 dir = tonearm.transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = tonearmControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle;

        float percent = Mathf.InverseLerp(startTonearmAngle, endTonearmAngle, convertedAngle);

        // THE FIX: Check if we are at 99.5% or higher, instead of exactly 100%
        return percent >= 0.995f;
    }
    #endregion


    //--------
    #endregion


    private void OnDrawGizmos()
    {
        #region TONEARM
        Vector2 defaultTonearmDir = new Vector2(Mathf.Cos(-(defaultTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(defaultTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)tonearm.transform.position, (Vector2)tonearm.transform.position + defaultTonearmDir * 2.5f);

        Vector2 startTonearmDir = new Vector2(Mathf.Cos(-(startTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(startTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)tonearm.transform.position, (Vector2)tonearm.transform.position + startTonearmDir * 2.5f);

        Vector2 endTonearmDir = new Vector2(Mathf.Cos(-(endTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(endTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)tonearm.transform.position, (Vector2)tonearm.transform.position + endTonearmDir * 2.5f);
        #endregion


        #region POWER KNOB
        Vector2 powerOffKnobDir = new Vector2(Mathf.Cos(-(powerOffKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(powerOffKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)powerKnob.transform.position, (Vector2)powerKnob.transform.position + powerOffKnobDir * 2.5f);

        Vector2 powerOnKnobDir = new Vector2(Mathf.Cos(-(powerOnKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(powerOnKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)powerKnob.transform.position, (Vector2)powerKnob.transform.position + powerOnKnobDir * 2.5f);
        #endregion


        #region RPM KNOB
        Vector2 slowedRPMKnobDir = new Vector2(Mathf.Cos(-(slowedRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(slowedRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)rpmKnob.transform.position, (Vector2)rpmKnob.transform.position + slowedRPMKnobDir * 2.5f);

        Vector2 normalRPMKnobDir = new Vector2(Mathf.Cos(-(normalRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(normalRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)rpmKnob.transform.position, (Vector2)rpmKnob.transform.position + normalRPMKnobDir * 2.5f);

        Vector2 spedUpRPMKnobDir = new Vector2(Mathf.Cos(-(spedUpRPMKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(spedUpRPMKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)rpmKnob.transform.position, (Vector2)rpmKnob.transform.position + spedUpRPMKnobDir * 2.5f);
        #endregion


        #region VOLUME KNOB
        Vector2 minVolumeKnobDir = new Vector2(Mathf.Cos(-(minVolumeKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(minVolumeKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)volumeKnob.transform.position, (Vector2)volumeKnob.transform.position + minVolumeKnobDir * 2.5f);

        Vector2 maxVolumeKnobDir = new Vector2(Mathf.Cos(-(maxVolumeKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(maxVolumeKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)volumeKnob.transform.position, (Vector2)volumeKnob.transform.position + maxVolumeKnobDir * 2.5f);
        #endregion
    }
}