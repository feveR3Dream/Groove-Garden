using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LeaveGame : MonoBehaviour
{
    [Header("Confirm Quit Button")]
    [SerializeField] private Button confirmQuitButton;
    [SerializeField] private CanvasGroup confirmCanvasGroup;

    [Header("Animation Settings")]
    [Tooltip("This is now in Unity World Units, not pixels! E.g., -2f or -3f")]
    [SerializeField] private float worldYOffset = -2f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Internal References
    private Button mainButton;
    private RectTransform confirmRect;
    private bool isShowingConfirm = false;
    private Coroutine toggleCoroutine = null;

    private void Awake()
    {
        mainButton = GetComponent<Button>();
        confirmRect = confirmQuitButton.GetComponent<RectTransform>();

        if (confirmCanvasGroup != null)
        {
            confirmCanvasGroup.alpha = 0f;
            confirmCanvasGroup.interactable = false;
            confirmCanvasGroup.blocksRaycasts = false;

            // Snap the child exactly to the parent's world position on startup
            confirmRect.position = mainButton.transform.position;
        }
    }

    private void OnEnable()
    {
        mainButton.onClick.AddListener(ToggleConfirmButton);
        confirmQuitButton.onClick.AddListener(ActuallyQuitGame);
    }

    private void OnDisable()
    {
        mainButton.onClick.RemoveListener(ToggleConfirmButton);
        confirmQuitButton.onClick.RemoveListener(ActuallyQuitGame);
    }

    private void ToggleConfirmButton()
    {
        if (confirmCanvasGroup == null) return;

        isShowingConfirm = !isShowingConfirm;

        if (toggleCoroutine != null) StopCoroutine(toggleCoroutine);
        toggleCoroutine = StartCoroutine(AnimateConfirmButton(isShowingConfirm));
    }

    private IEnumerator AnimateConfirmButton(bool show)
    {
        confirmCanvasGroup.interactable = show;
        confirmCanvasGroup.blocksRaycasts = show;

        float elapsed = 0f;
        float startAlpha = confirmCanvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;

        // DYNAMIC CALCULATION: 
        // We calculate this right now so it adapts to screen resizes instantly.
        Vector3 currentHiddenPos = mainButton.transform.position;
        Vector3 currentShownPos = currentHiddenPos + new Vector3(0, worldYOffset, 0);

        // We use .position to move it in pure world space
        Vector3 startPos = confirmRect.position;
        Vector3 targetPos = show ? currentShownPos : currentHiddenPos;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / animationDuration;

            float curvePercent = animationCurve.Evaluate(percent);

            confirmCanvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, targetAlpha, curvePercent);

            // Apply the interpolated position to the world position
            confirmRect.position = Vector3.LerpUnclamped(startPos, targetPos, curvePercent);

            yield return null;
        }

        confirmCanvasGroup.alpha = targetAlpha;
        confirmRect.position = targetPos;
        toggleCoroutine = null;
    }

    private void ActuallyQuitGame()
    {
        Debug.Log("<color=red>GAME QUIT INITIATED!</color> Shutting down...");
        Application.Quit();
    }
}