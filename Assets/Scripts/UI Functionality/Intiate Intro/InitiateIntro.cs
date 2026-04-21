using System.Collections;
using TMPro;
using UnityEngine;

public class InitiateIntro : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root canvas to turn off when the intro is completely done.")]
    [SerializeField] private Canvas introCanvas;

    [Tooltip("Add a CanvasGroup component to your Intro Panel and drop it here.")]
    [SerializeField] private CanvasGroup introCanvasGroup;

    [SerializeField] private TextMeshProUGUI introText;

    [Header("Timing Settings (Seconds)")]
    [SerializeField] private float textFadeInDuration = 1.5f;
    [SerializeField] private float textHoldDuration = 2.0f;
    [SerializeField] private float overallFadeOutDuration = 1.5f;

    private void Start()
    {
        // Start the intro sequence immediately when the game loads
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // 1. INITIAL SETUP
        // Make sure the panel is completely opaque, but the text starts invisible
        introCanvasGroup.alpha = 1f;
        introCanvasGroup.blocksRaycasts = true; // Block clicks during intro

        Color textColor = introText.color;
        textColor.a = 0f;
        introText.color = textColor;

        // 2. FADE THE TEXT IN
        float elapsed = 0f;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;

            // Just standard Lerp here, no fancy curves needed for a smooth fade!
            textColor.a = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);
            introText.color = textColor;

            yield return null;
        }
        // Snap to exactly 1
        textColor.a = 1f;
        introText.color = textColor;

        // 3. HOLD (Let the player read the text)
        yield return new WaitForSeconds(textHoldDuration);

        // 4. FADE EVERYTHING OUT
        elapsed = 0f;
        while (elapsed < overallFadeOutDuration)
        {
            elapsed += Time.deltaTime;

            // Fading the CanvasGroup fades BOTH the black background and the text simultaneously
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / overallFadeOutDuration);

            yield return null;
        }
        introCanvasGroup.alpha = 0f;

        // 5. CLEANUP
        // Turn off the Canvas entirely so it stops processing UI checks in the background!
        if (introCanvas != null)
        {
            introCanvas.gameObject.SetActive(false);
        }
    }
}