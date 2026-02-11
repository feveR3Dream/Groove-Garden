using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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