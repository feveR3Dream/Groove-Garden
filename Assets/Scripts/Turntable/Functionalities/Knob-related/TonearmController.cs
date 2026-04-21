using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(KnobControl))]
public class TonearmController : MonoBehaviour
{

    [Header("Tonearm Value")]
    [Range(0f, 360f)]
    [SerializeField] private float defaultTonearmAngle;
    [Range(0f, 360f)]
    [SerializeField] private float startTonearmAngle;
    [Range(0f, 360f)]
    [SerializeField] private float endTonearmAngle;


    [Header("Shadow Reference")]
    [SerializeField] private SpriteRenderer shadowRenderer;


    [Header("References")]
    [SerializeField] private Transform interactionPoint;


    // References
    private KnobControl knobControl;


    // Values
    private bool tonearmAtTheEnd = false;
    public bool TonearmAtTheEnd => tonearmAtTheEnd;


    // Coroutine
    private Coroutine moveTonearmCoroutine = null;
    private Coroutine fadeShadowCoroutine = null;



    private void Awake()
    {
        knobControl = GetComponent<KnobControl>();  
    }


    private void Start()
    {
        TurntableManager.Instance.InitializeKnobAngle(this.gameObject, defaultTonearmAngle);
    }


    public void LiftTonearm()
    {
        TurntableManager.Instance.TonearmOnRecord = false;
        TurntableManager.Instance.TurntableSystem.StopRecord();

        StopTonearm();

        Collider2D platterCollider = ButtonManager.Instance.PlatterGO.GetComponent<Collider2D>();
        if (!platterCollider.enabled) platterCollider.enabled = true;

        knobControl.StopResetRotation();
        knobControl.UpdateKnobAlpha(0f, knobControl.AlphaFadeSpeed);
        FadeTonearmShadow(0.5f, knobControl.AlphaFadeSpeed);

        interactionPoint.gameObject.SetActive(true);

        if (TurntableManager.Instance.RecordRead) 
            TurntableManager.Instance.DisplayerControl.UpdatePlayPauseDisplay();
    }


    public void MoveTonearm(float rotateSpeed)
    {
        knobControl.UpdateRotationClamped(transform, defaultTonearmAngle, endTonearmAngle, rotateSpeed);
    }

    
    public void ReleasedTonearm(float rotateSpeed)
    {
        LayerMask convertedLayer = 1 << gameObject.layer; // All turntable component share the same layer.
        RaycastHit2D[] hits = Physics2D.RaycastAll(interactionPoint.position, Vector2.zero, 10f, convertedLayer);

        foreach (RaycastHit2D hit in hits)
        {
            // HIT ITSELF
            if (hit.collider.gameObject == this.gameObject) continue;


            // HIT PLATTER (or RECORD PLACEMENT POSITION)
            if (hit.collider.gameObject == ButtonManager.Instance.RecordPlacementControl.gameObject)
            {
                GameObject platter = ButtonManager.Instance.RecordPlacementControl.gameObject;
                Record record = RecordManager.Instance.CurrentRecord;

                if (record == null)
                {
                    knobControl.CorrectRotation(knobControl.gameObject, defaultTonearmAngle, rotateSpeed);
                }
                else
                {
                    tonearmAtTheEnd = IsTonearmAtTheEnd();
                    TurntableManager.Instance.TonearmOnRecord = true;

                    Collider2D platterCollider = platter.GetComponent<Collider2D>();
                    if (platterCollider != null)
                        platterCollider.enabled = false;
                    
                    if (TurntableManager.Instance.RecordRead)
                        TurntableManager.Instance.DisplayerControl.UpdatePlayPauseDisplay();
                }

                knobControl.UpdateKnobAlpha(1f, knobControl.AlphaFadeSpeed);
                FadeTonearmShadow(1f, knobControl.AlphaFadeSpeed);

                interactionPoint.gameObject.SetActive(false);

                return;
            }
        }

        knobControl.CorrectRotation(knobControl.gameObject, defaultTonearmAngle, rotateSpeed);
        knobControl.UpdateKnobAlpha(1f, knobControl.AlphaFadeSpeed);
        FadeTonearmShadow(1f, knobControl.AlphaFadeSpeed);

        interactionPoint.gameObject.SetActive(false);
        
    }


    public void TrackTonearm()
    {
        if (moveTonearmCoroutine == null)
            moveTonearmCoroutine = StartCoroutine(TrackingTonearm());
    }
    private IEnumerator TrackingTonearm()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, 360f - endTonearmAngle);

        Record record = RecordManager.Instance.CurrentRecord;
        TurntableInDepthSystem system = TurntableManager.Instance.TurntableSystem;

        if (record == null || system == null) yield break;

        float totalDuration = record.RecordTrack.length;
        float currentTimeMark = system.VinylSpeaker.time;
        float duration = totalDuration - currentTimeMark;

        if (duration <= 0f)
        {
            moveTonearmCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * KnobManager.Instance.RPMControl.CurrentRPMPitch;
            float percent = elapsedTime / duration;

            transform.rotation = Quaternion.Lerp(startRotation, endRotation, percent);
            yield return null;
        }

        transform.rotation = endRotation;
        tonearmAtTheEnd = true;

        moveTonearmCoroutine = null;
    }

    public void StopTonearm()
    {
        if (moveTonearmCoroutine != null)
        {
            Record record = RecordManager.Instance.CurrentRecord;
            TurntableInDepthSystem system = TurntableManager.Instance.TurntableSystem;

            if (record == null || system == null) return;

            float totalDuration = record.RecordTrack.length;
            float currentTimeMark = system.VinylSpeaker.time;
            float duration = totalDuration - currentTimeMark;

            Debug.Log($"Stop Track Length: {duration}");

            StopCoroutine(moveTonearmCoroutine);
            moveTonearmCoroutine = null;
        }
    }


    #region HELPER FUNCTIONALITIES
    //----------------------------

    private bool IsTonearmAtTheEnd()
    {
        Vector2 dir = transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = knobControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle;

        float percent = Mathf.InverseLerp(startTonearmAngle, endTonearmAngle, convertedAngle);

        return percent >= 0.995f;
    }


    public float GetTimeMark()
    {
        Vector2 dir = transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = knobControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle;

        float percent = Mathf.InverseLerp(startTonearmAngle, endTonearmAngle, convertedAngle);

        Record record = RecordManager.Instance.CurrentRecord;
        float recordTimeMark = record.RecordTrack.length * percent;

        return recordTimeMark;
    }

    //--------
    #endregion


    #region EXTRA
    //-----------

    private void FadeTonearmShadow(float targetAlpha, float speed)
    {
        if (shadowRenderer == null) return;

        if (fadeShadowCoroutine != null)
        {
            StopCoroutine(fadeShadowCoroutine);
            fadeShadowCoroutine = null;
        }

        fadeShadowCoroutine = StartCoroutine(FadingTonearmShadow(targetAlpha, speed));
    }
    private IEnumerator FadingTonearmShadow(float targetAlpha, float speed)
    {
        Color currentColor = shadowRenderer.color;
        Color targetColor = currentColor;
        targetColor.a = targetAlpha;

        while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
        {
            currentColor = Color.Lerp(currentColor, targetColor, speed * Time.deltaTime);
            shadowRenderer.color = currentColor;

            yield return null;
        }

        shadowRenderer.color = targetColor;
        fadeShadowCoroutine = null;
    }



    //--------
    #endregion


    private void OnDrawGizmos()
    {
        Vector2 defaultTonearmDir = new Vector2(Mathf.Cos(-(defaultTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(defaultTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + defaultTonearmDir * 2.5f);

        Vector2 startTonearmDir = new Vector2(Mathf.Cos(-(startTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(startTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + startTonearmDir * 2.5f);

        Vector2 endTonearmDir = new Vector2(Mathf.Cos(-(endTonearmAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(endTonearmAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + endTonearmDir * 2.5f);
    }
}
