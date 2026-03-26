using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour, IButtonInteractable
{

    // References
    public SpriteRenderer Renderer { get; private set; }


    // Values
    [HideInInspector] public bool TurnedOn = false;
    public float PressedSize { get; private set; } = 0.9f;
    public float IdleSizeON { get; private set; } = 0.95f;
    public float IdleSizeOFF { get; private set; } = 1f;
    public float ResizeSpeed { get; private set; } = 25f;
    public float AlphaFadeSpeed { get; private set; } = 15f;


    // Alpha Values
    public float ButtonPressedOnAlpha { get; private set; } = 0.75f;
    public float ButtonPressedOffAlpha { get; private set; } = 0.5f;
    public float ButtonIdleOnAlpha { get; private set; } = 0.6f;
    public float ButtonIdleOffAlpha { get; private set; } = 1f;


    // Coroutines
    private Coroutine alphaCoroutine = null;
    private Coroutine resizeCoroutine = null;


    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }


    public void UpdateButtonAlpha(float targetAlpha, float alphaFadeSpeed)
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
            //TurntableManager.Instance.ButtonDown(this);
            TurntableManager.Instance.ButtonDown(this);
        }
        else if (mouseButton == MouseButton.Hold)
        {
            //TurntableManager.Instance.ButtonHold(this);
            TurntableManager.Instance.ButtonHold(this);
        }
        else if (mouseButton == MouseButton.Up)
        {
            if (registered) TurnedOn = !TurnedOn;
            
            //TurntableManager.Instance.ButtonUp(this);
            TurntableManager.Instance.ButtonUp(this);
        }
    }
}
