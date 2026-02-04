using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurntableControl : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject diskPlacement;

    // References
    private SpriteRenderer renderer;

    // Values
    private Color orgColor;
    private float fadedAlpha = 0.5f;
    private float fadeSpeed = 5f; // Increased speed for MoveTowards

    // Coroutines
    private Coroutine alphaCoroutine = null;

    private void Awake()
    {

    }

    public void ClickTurntableSetting(GameObject go)
    {

    }

    private void AdjustAlpha(SpriteRenderer renderer, float targetAlpha, float fadeSpeed)
    {
        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
            alphaCoroutine = null;
        }

        alphaCoroutine = StartCoroutine(AdjustingAlpha(renderer, targetAlpha, fadeSpeed));
    }

    private IEnumerator AdjustingAlpha(SpriteRenderer renderer, float targetAlpha, float fadeSpeed)
    {
        Color tempColor = renderer.color;

        while (!Mathf.Approximately(renderer.color.a, targetAlpha))
        {
            float newAlpha = Mathf.MoveTowards(renderer.color.a, targetAlpha, fadeSpeed * Time.deltaTime);

            tempColor.a = newAlpha;
            renderer.color = tempColor;

            yield return null;
        }

        alphaCoroutine = null;
    }

}