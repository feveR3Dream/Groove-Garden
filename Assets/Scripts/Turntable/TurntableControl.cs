using System.Collections;
using UnityEngine;

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
    private Record record;
    private Camera cam;


    // Values
    [HideInInspector] public bool EquipRecord = false; // Main values that checks music playability
    private bool turntableOn = false; // VERY IMPORTANT


    // Coroutines
    private Coroutine powerKnobCoroutine = null;
    private Coroutine rpmKnobCoroutine = null;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = Camera.main;

        // Add Button Component
        if (platter != null) platter.AddComponent<ButtonControl>();
        if (startStopButton != null) startStopButton.AddComponent<ButtonControl>();
        if (reverbButton != null) reverbButton.AddComponent<ButtonControl>();

        // Add Knob Component
        if (tonearm != null) tonearm.AddComponent<KnobControl>(); 
        if (powerKnob != null) powerKnob.AddComponent<KnobControl>(); 
        if (rpmKnob != null) rpmKnob.AddComponent<KnobControl>(); 
        if (volumeKnob != null) volumeKnob.AddComponent<KnobControl>();

        // Initialization
        InitializeKnobAngle(tonearm, minTonearmAngle);
        InitializeKnobAngle(powerKnob, powerOffKnobAngle);
        InitializeKnobAngle(rpmKnob, normalRPMKnobAngle);
        InitializeKnobAngle(volumeKnob, maxVolumeKnobAngle);
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

        // NEXT MISSION HERE

        //#region POWER KNOB
        //else if (knobControl.gameObject == powerKnob)
        //{
        //    // Add something here in the future
        //}

        //#endregion


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
        }
        #endregion


        #region RPM KNOB
        else if (knobControl.gameObject == rpmKnob)
        {
            knobControl.UpdateRotationClamped(knobControl.gameObject, slowedRPMKnobAngle, spedUpRPMKnobAngle, rotationSpeed);
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
        }
        #endregion


        #region RPM KNOB
        else if (knobControl.gameObject == rpmKnob)
        {
            ProcessRPMKnob(rpmKnob, slowedRPMKnobAngle, normalRPMKnobAngle, spedUpRPMKnobAngle, rotationSpeed);
        }
        #endregion
    }


    // POWER KNOB FUNCTIONALITY
    private void ProcessPowerKnob(GameObject powerKnobGO, float minAngle, float maxAngle, float rotationSpeed)
    {
        if (powerKnobCoroutine != null)
        {
            StopCoroutine(powerKnobCoroutine);
            powerKnobCoroutine = null;
        }

        powerKnobCoroutine = StartCoroutine(ProcessingPowerKnob(powerKnobGO, minAngle, maxAngle, rotationSpeed));
    }
    private IEnumerator ProcessingPowerKnob(GameObject powerKnobGO, float minAngle, float maxAngle, float rotationSpeed)
    {
        KnobControl knobControl = powerKnobGO.GetComponent<KnobControl>();
        float currentAngle = knobControl.NormalizeClockwise(-powerKnobGO.transform.eulerAngles.z);

        float toMin = Mathf.Abs(Mathf.DeltaAngle(currentAngle, minAngle));
        float toMax = Mathf.Abs(Mathf.DeltaAngle(currentAngle, maxAngle));

        float targetAngle = toMin < toMax ? minAngle : maxAngle;
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

        powerKnobGO.transform.rotation = targetRot;
        powerKnobCoroutine = null;
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

        Quaternion targetRot = Quaternion.Euler(0f, 0f, -targetAngle);

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



public class ButtonControl : MonoBehaviour, IButtonInteractable
{

    // References
    public SpriteRenderer Renderer { get; private set; }


    // Values
    public bool TurnedOn { get; private set; } = false;
    private Color orgColor;
    public float PressedSize { get; private set; } = 0.9f;
    public float IdleSizeON { get; private set; } = 0.95f;
    public float IdleSizeOFF { get; private set; } = 1f;
    public float ResizeSpeed { get; private set; } = 50f;


    // Coroutines
    private Coroutine resizeCoroutine = null;


    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        orgColor = Renderer.color;
    }


    public void UpdateButtonAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        Color tempColor = orgColor;
        tempColor.a = alpha;
        Renderer.color = tempColor;
    }


    public void UpdateButtonSize(float size, float resizeSpeed)
    {
        Vector3 targetSize = Vector3.one * size;

        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
            resizeCoroutine = null;
        }

        resizeCoroutine = StartCoroutine(ResizingCover(targetSize, resizeSpeed));
    }


    public IEnumerator ResizingCover(Vector3 targetSize, float resizeSpeed)
    {
        while (!Mathf.Approximately(transform.localScale.magnitude, targetSize.magnitude))
        {
            Vector3 tempScale = Vector3.Lerp(transform.localScale, targetSize, resizeSpeed * Time.deltaTime);
            transform.localScale = tempScale;

            yield return null;
        }

        transform.localScale = targetSize;
        resizeCoroutine = null;
    }


    public void ButtonInteracted(bool registered, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Down)
        {
            TurntableControl.Instance.ButtonDown(this);
        }
        else if (mouseButton == MouseButton.Hold)
        {
            TurntableControl.Instance.ButtonHold(this);
        }
        else if (mouseButton == MouseButton.Up)
        {
            TurntableControl.Instance.ButtonUp(this);

            if (registered) TurnedOn = !TurnedOn;
        }
    }
}



public class KnobControl : MonoBehaviour, IKnobInteractable
{
    // References
    public SpriteRenderer Renderer { get; private set; }
    private Camera cam;


    // Values
    private float angleOffset;
    private Color orgColor;


    // Coroutines
    private Coroutine resetCoroutine = null;


    private void Awake()
    {
        cam = Camera.main;
        Renderer = GetComponent<SpriteRenderer>();

        if (Renderer != null)
            orgColor = Renderer.color;
    }


    public void SetAngleOffset()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = (mousePos - (Vector2)transform.position).normalized;
        Vector2 selfDir = -(Vector2)transform.up;

        angleOffset = Vector2.SignedAngle(selfDir, mouseDir);
    }


    public void UpdateRotationClamped(GameObject goRef, float minAngle, float maxAngle, float rotationSpeed)
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)goRef.transform.position).normalized;

        float rawAngle =
            -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + angleOffset;

        float clockwiseAngle = NormalizeClockwise(rawAngle);
        float clampedAngle = ClampAngleCircular(clockwiseAngle, minAngle, maxAngle);

        Quaternion target = Quaternion.Euler(0f, 0f, -clampedAngle);
        goRef.transform.rotation = Quaternion.Lerp(goRef.transform.rotation, target, rotationSpeed * Time.deltaTime);

    }


    public void UpdateRotation(GameObject goRef, float rotationSpeed)
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)goRef.transform.position).normalized;

        float rawAngle =
            -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + angleOffset;

        float clockwiseAngle = NormalizeClockwise(rawAngle);

        Quaternion target = Quaternion.Euler(0f, 0f, -clockwiseAngle);
        goRef.transform.rotation = Quaternion.Lerp(goRef.transform.rotation, target, rotationSpeed * Time.deltaTime);
    }


    public void StopResetRotation()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }
    public void ResetRotation(GameObject goRef, float resetAngle, float rotationSpeed)
    {
        StopResetRotation();
        resetCoroutine = StartCoroutine(ResettingRotation(goRef, resetAngle, rotationSpeed));
    }
    private IEnumerator ResettingRotation(GameObject goRef, float resetAngle, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, -resetAngle);

        while (Quaternion.Angle(goRef.transform.rotation, targetRotation) > 0.1f)
        {
            Debug.Log($"Value: {NormalizeClockwise(-goRef.transform.eulerAngles.z)}");
            goRef.transform.rotation = Quaternion.Lerp(goRef.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        goRef.transform.rotation = targetRotation;
        resetCoroutine = null;
    }



    // FOR EVERY KNOB RELATED FUNCTIONALITY
    public float NormalizeClockwise(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }


    // FOR CLAMPING BETWEEN MIN & MAX ANGLE
    public float ClampAngleCircular(float angle, float min, float max)
    {
        if (IsAngleBetween(angle, min, max))
            return angle;

        float toMin = Mathf.Abs(Mathf.DeltaAngle(angle, min));
        float toMax = Mathf.Abs(Mathf.DeltaAngle(angle, max));

        return toMin < toMax ? min : max;
    }
    public bool IsAngleBetween(float angle, float min, float max)
    {
        if (min <= max)
            return angle >= min && angle <= max;

        // Wrap-around case (ex: 300 → 40)
        return angle >= min || angle <= max;
    }


    public void UpdateKnobAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        Color temp = orgColor;
        temp.a = alpha;
        Renderer.color = temp;
    }

    public void KnobInteracted(MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Down)
            TurntableControl.Instance.KnobDown(this);
        else if (mouseButton == MouseButton.Hold)
            TurntableControl.Instance.KnobHold(this);
        else if (mouseButton == MouseButton.Up)
            TurntableControl.Instance.KnobUp(this);
    }
}
