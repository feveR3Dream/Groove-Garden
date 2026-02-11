using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
