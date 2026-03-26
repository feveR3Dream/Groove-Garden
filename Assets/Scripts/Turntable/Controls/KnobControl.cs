using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobControl : MonoBehaviour, IKnobInteractable
{
    // References
    public SpriteRenderer Renderer { get; private set; }
    private Camera cam;


    // Values
    public float AlphaFadeSpeed { get; private set; } = 10f;
    private float angleOffset;
    public bool CanCalculateOffset = true;


    // Coroutines
    private Coroutine correctRotationCoroutine = null;
    private Coroutine alphaCoroutine = null;


    private void Awake()
    {
        cam = Camera.main;
        Renderer = GetComponent<SpriteRenderer>();
    }


    public void UpdateKnobAlpha(float targetAlpha, float alphaFadeSpeed)
    {
        targetAlpha = Mathf.Clamp01(targetAlpha);

        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
            alphaCoroutine = null;
        }

        alphaCoroutine = StartCoroutine(AdjustingAlpha(targetAlpha, alphaFadeSpeed));
    }
    private IEnumerator AdjustingAlpha(float targetAlpha, float alphaFadeSpeed)
    {
        Color currentColor = Renderer.color;
        Color targetColor = currentColor;
        targetColor.a = targetAlpha;

        while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
        {
            currentColor = Color.Lerp(currentColor, targetColor, alphaFadeSpeed * Time.deltaTime);
            Renderer.color = currentColor;

            yield return null;
        }

        Renderer.color = targetColor;
        alphaCoroutine = null;
    }


    public void SetAngleOffset()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = (mousePos - (Vector2)transform.position).normalized;
        Vector2 selfDir = -(Vector2)transform.up;

        angleOffset = Vector2.SignedAngle(selfDir, mouseDir);
    }


    public void UpdateRotationClamped(Transform transformRef, float minAngle, float maxAngle, float rotationSpeed)
    {
        if (CanCalculateOffset)
        {
            CanCalculateOffset = false;
            SetAngleOffset();
        }

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = (mousePos - (Vector2)transformRef.position).normalized;

        float rawAngle =
            -Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg - 90f + angleOffset;

        float clockwiseAngle = NormalizeClockwise(rawAngle);
        float clampedAngle = ClampAngleCircular(clockwiseAngle, minAngle, maxAngle);

        Quaternion target = Quaternion.Euler(0f, 0f, -clampedAngle);
        transformRef.rotation = Quaternion.Lerp(transformRef.rotation, target, rotationSpeed * Time.deltaTime);

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
        if (correctRotationCoroutine != null)
        {
            StopCoroutine(correctRotationCoroutine);
            correctRotationCoroutine = null;
        }
    }
    public void CorrectRotation(GameObject goRef, float resetAngle, float rotationSpeed)
    {
        StopResetRotation();
        correctRotationCoroutine = StartCoroutine(CorrectingRotation(goRef, resetAngle, rotationSpeed));
    }
    private IEnumerator CorrectingRotation(GameObject goRef, float resetAngle, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, -resetAngle);

        while (Quaternion.Angle(goRef.transform.rotation, targetRotation) > 0.1f)
        {
            goRef.transform.rotation = Quaternion.Lerp(goRef.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        goRef.transform.rotation = targetRotation;

        CanCalculateOffset = true;
        correctRotationCoroutine = null;
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

    public void KnobInteracted(MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Down)
        {
            //TurntableManager.Instance.KnobDown(this);
            TurntableManager.Instance.KnobDown(this);
        }
        else if (mouseButton == MouseButton.Hold)
        {
            //TurntableManager.Instance.KnobHold(this);
            TurntableManager.Instance.KnobHold(this);
        }
        else if (mouseButton == MouseButton.Up)
        {
            //TurntableManager.Instance.KnobUp(this);
            TurntableManager.Instance.KnobUp(this);
        }
    }
}