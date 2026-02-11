using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TurntableControl : MonoBehaviour
{
    public static TurntableControl Instance { get; private set; }

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
    [SerializeField] private SpriteRenderer powerLight;
    [Space]
    [SerializeField] private GameObject rpmKnob;
    [SerializeField] private SpriteRenderer slowedLight;
    [SerializeField] private SpriteRenderer normalLight;
    [SerializeField] private SpriteRenderer spedUpLight;
    [Space]
    [SerializeField] private GameObject volumeKnob;


    [Header("Layer References")]
    [SerializeField] private LayerMask turntableLayer;


    [Header("Speed Value")]
    [SerializeField] private float rotationSpeed;


    [Header("Tonearm Value")]
    [Range(0f, 360f)]
    [SerializeField] private float minTonearmAngle;
    [Range(0f, 360f)]
    [SerializeField] private float maxTonearmAngle;


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
    [SerializeField] private float minVolumeKnobAngle;
    [Range(0f, 360f)]
    [SerializeField] private float maxVolumeKnobAngle;


    [Header("Values")]
    [SerializeField] private float buttonPressedOnAlpha = 0.75f;
    [SerializeField] private float buttonPressedOffAlpha = 0.5f;
    [Space]
    [SerializeField] private float buttonIdleOnAlpha = 0.6f;
    [SerializeField] private float buttonIdleOffAlpha = 1f;


    // References
    public RPM CurrentRPM { get; private set; } = RPM.Normal;
    public Power CurrentPower { get; private set; } = Power.Off;
    private Record record;
    private Camera cam;


    // Values
    [HideInInspector] public bool EquipRecord = false; // Main values that checks music playability


    // Coroutines
    private Coroutine powerKnobCoroutine = null;
    private Coroutine rpmKnobCoroutine = null;


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

        // Initialization
        InitializeKnobAngle(tonearm, minTonearmAngle);
        InitializeKnobAngle(powerKnob, powerOffKnobAngle);
        InitializeKnobAngle(rpmKnob, normalRPMKnobAngle);
        InitializeKnobAngle(volumeKnob, maxVolumeKnobAngle);

        UpdatePowerStatus(CurrentPower);
        UpdateRPMStatus(CurrentRPM);
    }


    private void InitializeKnobAngle(GameObject knobGO, float angleValue)
    {
        knobGO.transform.rotation = Quaternion.Euler(0f, 0f, -angleValue);
    }


    public void ButtonDown(ButtonControl buttonControl)
    {
        // FOR FUTURE IMPLEMENTATION
    }
    public void ButtonHold(ButtonControl buttonControl)
    {
        if (!buttonControl.TurnedOn)
        {
            buttonControl.UpdateButtonAlpha(buttonPressedOnAlpha);
        }
        else
        {
            buttonControl.UpdateButtonAlpha(buttonPressedOffAlpha);
        }

        if (buttonControl.gameObject != platter)
            buttonControl.UpdateButtonSize(buttonControl.PressedSize, buttonControl.ResizeSpeed);
    }
    public void ButtonUp(ButtonControl buttonControl)
    {
        #region FOR PLATTER
        if (buttonControl.gameObject == platter)
        {
            buttonControl.UpdateButtonAlpha(buttonIdleOffAlpha);
            RecordInteraction();
        }
        #endregion

        #region FOR OTHERS
        else // For debug
        {
            if (!buttonControl.TurnedOn)
            {
                buttonControl.UpdateButtonAlpha(buttonIdleOnAlpha);
                buttonControl.UpdateButtonSize(buttonControl.IdleSizeON, buttonControl.ResizeSpeed);
            }
            else
            {
                buttonControl.UpdateButtonAlpha(buttonIdleOffAlpha);
                buttonControl.UpdateButtonSize(buttonControl.IdleSizeOFF, buttonControl.ResizeSpeed);
            }
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



    public void KnobDown(KnobControl knobControl)
    {
        #region TONEARM 
        if (knobControl.gameObject == tonearm)
        {
            Collider2D platterCollider = platter.GetComponent<Collider2D>();
            if (!platterCollider.enabled)
                platterCollider.enabled = true;

            knobControl.StopResetRotation();
        }
        #endregion

        knobControl.SetAngleOffset();
    }
    public void KnobHold(KnobControl knobControl)
    {
        #region TONEARM
        if (knobControl.gameObject == tonearm)
        {
            knobControl.UpdateRotationClamped(knobControl.gameObject, minTonearmAngle, maxTonearmAngle, rotationSpeed);
        }
        #endregion


        #region POWER KNOB
        else if (knobControl.gameObject == powerKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.gameObject, powerOffKnobAngle, powerOnKnobAngle, rotationSpeed);

            UpdatePowerStatus(CurrentPower);
        }
        #endregion


        #region RPM KNOB
        else if (knobControl.gameObject == rpmKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.gameObject, slowedRPMKnobAngle, spedUpRPMKnobAngle, rotationSpeed);

            TargetRPMRotation(knobControl.gameObject, slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle);
            UpdateRPMStatus(CurrentRPM);
        }
        #endregion


        #region VOLUME KNOB
        else if (knobControl.gameObject == volumeKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.gameObject, minVolumeKnobAngle, maxVolumeKnobAngle, rotationSpeed);
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
                        knobControl.ResetRotation(knobControl.gameObject, minTonearmAngle, rotationSpeed);
                    }
                    else
                    {
                        Collider2D platterCollider = platter.GetComponent<Collider2D>();
                        if (platterCollider != null)
                            platterCollider.enabled = false;
                    }

                    return;
                }
            }

            knobControl.ResetRotation(knobControl.gameObject, minTonearmAngle, rotationSpeed);
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
            UpdateRPMStatus(CurrentRPM);
        }
        #endregion
    }


    // POWER KNOB FUNCTIONALITY
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
            powerLightControl.Renderer.color = Color.black;
            powerLightControl.LightSetting.color = Color.black;
        }
        else if (currentPower == Power.On)  
        {
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
            UpdateRPMStatus(CurrentRPM);
        }

        return toMin < toMax;
    }



    // RPM KNOB FUNCTIONALITY
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
    private void UpdateRPMStatus(RPM currentRPM)
    {
        if (CurrentPower == Power.On)
        {
            ResetRPMStatus();

            // Light Setting
            if (currentRPM == RPM.Slowed)
            {
                slowedLightControl.LightSetting.color = Color.red;
                slowedLightControl.Renderer.color = Color.red;
            }
            else if (currentRPM == RPM.Normal)
            {
                normalLightControl.LightSetting.color = Color.red;
                normalLightControl.Renderer.color = Color.red;
            }
            else if (currentRPM == RPM.SpedUp)
            {
                spedUpLightControl.LightSetting.color = Color.red;
                spedUpLightControl.Renderer.color = Color.red;
            }
        }
    }
    private void ResetRPMStatus()
    {
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

        Debug.Log($"Euler Angle Debug: {rpmKnobGO.transform.eulerAngles.z}");

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
        if (targetAngle == slowedAngle) CurrentRPM = RPM.Slowed;
        else if (targetAngle == normalAngle) CurrentRPM = RPM.Normal;
        else if (targetAngle == spedUpAngle) CurrentRPM = RPM.SpedUp;
        
        return targetAngle;
    }


    private void OnDrawGizmos()
    {
        #region TONEARM
        Vector2 minTonearmDir = new Vector2(Mathf.Cos(-(minTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(minTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)tonearm.transform.position, (Vector2)tonearm.transform.position + minTonearmDir * 2.5f);

        Vector2 maxTonearmDir = new Vector2(Mathf.Cos(-(maxTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(maxTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)tonearm.transform.position, (Vector2)tonearm.transform.position + maxTonearmDir * 2.5f);
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