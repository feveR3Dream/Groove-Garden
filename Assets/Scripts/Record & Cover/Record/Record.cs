using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Record : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecordSO recordSO;

    // References
    public AudioClip RecordTrack { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    private Camera cam;

    [Header("Time-Based Values (Seconds)")]
    private Vector2 orgPosition;
    private float alphaFadeDuration = 0.2f; // Exposed so you can tweak it!
    private float resizeDuration = 0.2f;    // Exposed so you can tweak it!


    // Animation Curve Value
    private AnimationCurve easeOutCurve = new AnimationCurve(
    new Keyframe(time: 0f, value: 0f, inTangent: 0f, outTangent: 2f),
    new Keyframe(time: 1f, value: 1f, inTangent: 0f, outTangent: 0f)
    );


    // Coroutines
    private Coroutine alphaCoroutine = null;
    private Coroutine resizeCoroutine = null;
    private Coroutine moveCoroutine = null;

    private void OnValidate()
    {
        UpdateRecordAppearance();
    }

    private void Awake()
    {
        UpdateRecordAppearance();
        orgPosition = transform.position;
        cam = Camera.main;
    }

    private void UpdateRecordAppearance()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

        if (recordSO != null)
        {
            SpriteRenderer.sprite = recordSO.picture;
            SpriteRenderer.sortingLayerName = "Shelf, Covers & Album Names";
            SpriteRenderer.sortingOrder = Global.SortingValue.SpawnedInSortingOrder;
            RecordTrack = recordSO.track;

            transform.localScale = Vector3.one * Global.SizeValue.RecordDiskSpawnedSize;
        }
    }

    private void UpdateRecordAlpha(float targetAlpha, float duration)
    {
        targetAlpha = Mathf.Clamp01(targetAlpha);

        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
        }

        alphaCoroutine = StartCoroutine(UpdatingRecordAlpha(targetAlpha, duration));
    }

    private IEnumerator UpdatingRecordAlpha(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color startColor = SpriteRenderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            
            // THE MAGIC SAUCE: Apply the Alpha Curve
            float curvePercent = easeOutCurve.Evaluate(percent);

            SpriteRenderer.color = Color.LerpUnclamped(startColor, targetColor, curvePercent);
            yield return null;
        }

        SpriteRenderer.color = targetColor;
        alphaCoroutine = null;
    }

    private void UpdateRecordSize(float targetSize, float duration)
    {
        Vector3 targetScale = Vector3.one * targetSize;

        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
        }

        resizeCoroutine = StartCoroutine(ResizingRecord(targetScale, duration));
    }

    private IEnumerator ResizingRecord(Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // THE MAGIC SAUCE: Apply the Resize Curve
            float curvePercent = easeOutCurve.Evaluate(percent);

            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curvePercent);
            yield return null;
        }

        transform.localScale = targetScale;
        resizeCoroutine = null;
    }

    public void MoveTo(RecordMoveTo moveTo, float moveDuration)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MovingTo(moveTo, moveDuration));
    }

    private IEnumerator MovingTo(RecordMoveTo moveTo, float duration)
    {
        // 1. Setup Phase: Configure UI, Sorting, Alpha, and Size based on the target
        if (moveTo == RecordMoveTo.To_Mouse)
        {
            UIManager.Instance.PlacingRecordDown = true;
            RecordManager.Instance.SetRecordMoveable(true);

            SpriteRenderer.sortingLayerName = "Turntable & Record";
            SpriteRenderer.sortingOrder = Global.SortingValue.SpawnedInSortingOrder;

            UpdateRecordAlpha(0.5f, alphaFadeDuration);
            UpdateRecordSize(Global.SizeValue.RecordDiskHandlingSize, resizeDuration);
        }
        else if (moveTo == RecordMoveTo.To_Turntable)
        {
            UIManager.Instance.PlacingRecordDown = false;

            SpriteRenderer.sortingLayerName = "Turntable & Record";
            SpriteRenderer.sortingOrder = Global.SortingValue.HandlingSortingOrder;

            UpdateRecordAlpha(1f, alphaFadeDuration);
            UpdateRecordSize(Global.SizeValue.RecordDiskHandlingSize, resizeDuration);
        }
        else if (moveTo == RecordMoveTo.To_Spawned_Pos)
        {
            UIManager.Instance.PlacingRecordDown = false;
            RecordManager.Instance.SetRecordMoveable(false);

            SpriteRenderer.sortingLayerName = "Shelf, Covers & Album Names";
            SpriteRenderer.sortingOrder = Global.SortingValue.SpawnedInSortingOrder;

            TurntableManager.Instance.EquipRecord = false;

            UpdateRecordAlpha(1f, alphaFadeDuration);
            UpdateRecordSize(Global.SizeValue.RecordDiskSpawnedSize, resizeDuration);
        }

        // 2. Movement Phase
        if (moveTo == RecordMoveTo.To_Mouse)
        {
            // INFINITE LOOP: Smoothly follow the mouse forever until interrupted
            // We intentionally DO NOT use an Animation Curve here because SmoothDamp handles physics-based easing dynamically.
            Vector2 velocity = Vector2.zero;
            float smoothDampTime = 0.05f;

            while (true)
            {
                if (RecordManager.Instance.CurrentRecord != null)
                {
                    Vector2 targetPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    transform.position = Vector2.SmoothDamp(transform.position, targetPos, ref velocity, smoothDampTime);
                }
                yield return null;
            }
        }
        else
        {
            // FIXED TARGET LOOP: Strict time-based Lerp to a stationary point with Animation Curves
            float elapsed = 0f;
            Vector2 startPos = transform.position;
            Vector2 targetPos = (moveTo == RecordMoveTo.To_Turntable) ? ButtonManager.Instance.RecordPlacementPos : orgPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / duration;

                // THE MAGIC SAUCE: Apply the Move Curve
                float curvePercent = easeOutCurve.Evaluate(percent);

                transform.position = Vector2.LerpUnclamped(startPos, targetPos, curvePercent);
                yield return null;
            }

            // Guarantee exact final position
            transform.position = targetPos;
        }

        moveCoroutine = null;
    }
}