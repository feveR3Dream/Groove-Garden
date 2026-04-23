using System.Collections;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }
    public VinylCover CurrentVinylRecord { get; private set; } = null;

    [Header("References")]
    [SerializeField] private Transform bottomCamLimit;
    [SerializeField] private Transform hideRecordTransform;
    [Space]
    [SerializeField] private Transform leftCamLimit;
    [SerializeField] private Transform hideCoverTransform;
    [Space]
    [SerializeField] private Transform vinylShelfTransform;
    [Space]
    [SerializeField] private Transform recordDiskContainer;

    [Header("Time-Based Values (Seconds)")]
    [SerializeField] private float offset;
    [SerializeField] private float coverMoveDuration = 0.5f;
    [SerializeField] private float unsheatheDuration = 0.4f;
    [SerializeField] private float recordMoveDuration = 0.3f;
    public float RecordMoveDuration => recordMoveDuration;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // References
    public RecordExamine CurrentRecordExamine { get; private set; } = RecordExamine.None;
    public RecordSide CurrentRecordSide { get; private set; } = RecordSide.Front;
    public RecordToggle CurrentRecordToggle { get; private set; } = RecordToggle.Hide;
    public Record CurrentRecord { get; private set; } = null;
    public Transform ShowRecordTransform { get; private set; }

    private Camera cam;
    private SpriteRenderer spriteRenderer;

    // Values
    public bool RecordMoveable { get; private set; } = false;
    private bool canEquip = true;
    private int orgSortingOrder = 0;

    // Coroutines
    private Coroutine activeMovementCoroutine = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = Camera.main;
        ShowRecordTransform = cam.transform;

        if (hideRecordTransform != null && bottomCamLimit != null)
        {
            hideRecordTransform.position = new Vector2(bottomCamLimit.position.x, bottomCamLimit.position.y - Mathf.Abs(offset));
        }

        if (hideCoverTransform != null && leftCamLimit != null)
        {
            hideCoverTransform.position = new Vector2(leftCamLimit.position.x - Mathf.Abs(offset), leftCamLimit.position.y);
        }
    }

    // Pick up record from selection screen
    public void EquipRecordCover(VinylCover record)
    {
        if (!canEquip) return;

        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        canEquip = false;
        CurrentVinylRecord = record;
        CurrentVinylRecord.transform.SetParent(hideRecordTransform);
        CurrentVinylRecord.SpriteRenderer.sortingOrder = Global.SortingValue.EquippedSortingOrder;

        activeMovementCoroutine = StartCoroutine(EquippingCoroutine());
    }

    private IEnumerator EquippingCoroutine()
    {
        yield return RecordCoverTransforming(Vector2.zero, true, coverMoveDuration, Global.SizeValue.RecordCoverHandlingSize);

        UIManager.Instance.UpdateRecordCoverEquip(true);
        UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to access vinyl cover");
        activeMovementCoroutine = null;
    }

    // For inspecting & interacting with the record 
    public void RecordCoverTransition(Direction direction)    // LEFT/RIGHT BUTTON PRESSED
    {
        if (direction == Direction.None) return;
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        activeMovementCoroutine = StartCoroutine(RecordCoverTransitioning(direction, CurrentRecordExamine));
    }

    private IEnumerator RecordCoverTransitioning(Direction direction, RecordExamine examine)
    {
        UIManager.Instance.LeftRightCoverInspectButtonEnable(false);

        if (direction == Direction.Left)
        {
            if (examine == RecordExamine.Default)
            {
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to flip back vinyl record");
                CurrentRecordExamine = RecordExamine.Info;
                CurrentRecordSide = RecordSide.Back;
            }
        }
        else if (direction == Direction.Right)
        {
            CurrentRecordSide = RecordSide.Front;

            if (examine == RecordExamine.Default)
            {
                if (UIManager.Instance.CurrentScreen == Screen.Turntable)
                {
                    UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to sheathe back record in cover");
                    CurrentRecordExamine = RecordExamine.Unsheathe;
                }
                else if (UIManager.Instance.CurrentScreen == Screen.Selection)
                {
                    CurrentRecordExamine = RecordExamine.Return;
                }
            }
        }

        if (CurrentRecordExamine == RecordExamine.Unsheathe)
        {
            yield return StartCoroutine(UnsheathingRecordToggle(true));
        }
        else if (CurrentRecordExamine == RecordExamine.Return)
        {
            ReturnRecordCover();
        }
        else
        {
            yield return StartCoroutine(FlipRecordCover(CurrentRecordSide, CurrentRecordExamine));
        }

        activeMovementCoroutine = null;
    }

    // Return the record back to vinyl shelf on selection screen
    private void ReturnRecordCover()
    {
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        CurrentVinylRecord.transform.SetParent(vinylShelfTransform);

        spriteRenderer = CurrentVinylRecord.GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.sortingOrder = orgSortingOrder;

        activeMovementCoroutine = StartCoroutine(ReturningRecordCover());
    }

    private IEnumerator ReturningRecordCover()
    {
        yield return RecordCoverTransforming(CurrentVinylRecord.OrgPosition, false, coverMoveDuration, Global.SizeValue.RecordCoverShelfSize);

        CurrentVinylRecord = null;
        canEquip = true;

        UIManager.Instance.UpdateRecordCoverEquip(false);
        UIManager.Instance.UpdateRecordCoverHidden(false);

        CurrentRecordToggle = RecordToggle.Hide;

        activeMovementCoroutine = null;
    }

    // Like the name implies, take the cover off to reveal disk
    private IEnumerator UnsheathingRecordToggle(bool unsheathe)
    {
        Vector2 targetPos = unsheathe ? (Vector2)hideCoverTransform.position : (Vector2)ShowRecordTransform.position;

        if (!unsheathe && CurrentRecord != null)
        {
            CurrentRecord.MoveTo(RecordMoveTo.To_Spawned_Pos, recordMoveDuration);
        }

        if (CurrentVinylRecord.RecordDisk != null && CurrentRecord == null)
        {
            GameObject recordDisk = CurrentVinylRecord.RecordDisk;
            GameObject recordGO = Instantiate(recordDisk, (Vector2)ShowRecordTransform.position, Quaternion.identity, recordDiskContainer);

            CurrentRecord = recordGO.GetComponent<Record>();
        }

        yield return RecordCoverTransforming(targetPos, false, unsheatheDuration, Global.SizeValue.RecordCoverHandlingSize);

        if (unsheathe && CurrentRecord != null)
        {
            CurrentRecord.MoveTo(RecordMoveTo.To_Mouse, recordMoveDuration);
        }
        else if (!unsheathe)
        {
            CurrentRecordExamine = RecordExamine.Default;

            UIManager.Instance.LeftRightCoverInspectButtonEnable(true);

            if (UIManager.Instance.CurrentScreen == Screen.Turntable)
            {
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.LeftRecordText, "Press to see track info");
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.RightRecordText, "Press to unsheathe record");
            }

            Destroy(CurrentRecord.gameObject);
            CurrentRecord = null;
        }
    }

    private IEnumerator FlipRecordCover(RecordSide side, RecordExamine examine)
    {
        if (CurrentRecordExamine != examine)
            CurrentRecordExamine = examine;

        yield return RecordCoverTransforming(Vector2.zero, true, coverMoveDuration, Global.SizeValue.RecordCoverHandlingSize);

        if (side == RecordSide.Back) CurrentVinylRecord.IsFrontCover(false);
        else if (side == RecordSide.Front) CurrentVinylRecord.IsFrontCover(true);

        yield return RecordCoverTransforming((Vector2)ShowRecordTransform.position, false, coverMoveDuration, Global.SizeValue.RecordCoverHandlingSize);

        if (CurrentRecordExamine == RecordExamine.Default)
        {
            UIManager.Instance.LeftRightCoverInspectButtonEnable(true);

            if (UIManager.Instance.CurrentScreen == Screen.Turntable)
            {
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.LeftRecordText, "Press to see track info");
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.RightRecordText, "Press to unsheathe record");
            }
        }
    }

    // Bottom button pressed
    public void ToggleCover()
    {
        if (CurrentVinylRecord == null) return;
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        if (CurrentRecordToggle == RecordToggle.Hide)
        {
            CurrentRecordToggle = RecordToggle.Show;
            activeMovementCoroutine = StartCoroutine(TogglingRecord(true));

            if (UIManager.Instance.CurrentScreen == Screen.Selection)
            {
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.RightRecordText, "Press to put back record on shelf");
            }
        }
        else if (CurrentRecordToggle == RecordToggle.Show)
        {
            if (CurrentRecordExamine == RecordExamine.Info)
            {
                // Back to Default
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to hide vinyl cover");
                activeMovementCoroutine = StartCoroutine(FlipRecordCover(RecordSide.Front, RecordExamine.Default));
            }
            else if (CurrentRecordExamine == RecordExamine.Unsheathe)
            {
                // Sheathe Record and Back to Default
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to hide vinyl cover");
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.RightRecordText, "Press to unsheathe record");
                activeMovementCoroutine = StartCoroutine(UnsheathingRecordToggle(false));
            }
            else
            {
                // Switch off Record
                CurrentRecordToggle = RecordToggle.Hide;
                activeMovementCoroutine = StartCoroutine(TogglingRecord(false));
            }

            return;
        }
    }

    private IEnumerator TogglingRecord(bool show)
    {
        Vector2 destination = show ? (Vector2)ShowRecordTransform.position : (Vector2)hideRecordTransform.position;
        CurrentRecordExamine = show ? RecordExamine.Default : RecordExamine.None;

        if (show)
        {
            if (CurrentRecordExamine == RecordExamine.Default)
            {
                UIManager.Instance.LeftRightCoverInspectButtonEnable(true);
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.LeftRecordText, "Press to see track info");
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.RightRecordText, "Press to unsheathe record");
                UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to hide vinyl record");
            }
        }
        else
        {
            CurrentVinylRecord.IsFrontCover(true);
            UIManager.Instance.LeftRightCoverInspectButtonEnable(false);
            UIManager.Instance.SetButtonTextContent(UIManager.Instance.BottomText, "Press to access vinyl cover");
        }

        UIManager.Instance.UpdateRecordCoverHidden(show);

        yield return RecordCoverTransforming(destination, false, coverMoveDuration, Global.SizeValue.RecordCoverHandlingSize);

        activeMovementCoroutine = null;
    }

    public void StopTogglingRecord()
    {
        if (activeMovementCoroutine != null)
        {
            StopCoroutine(activeMovementCoroutine);
            activeMovementCoroutine = null;
        }

        if (CurrentVinylRecord != null)
        {
            CurrentVinylRecord.transform.parent = hideRecordTransform;
            CurrentVinylRecord.transform.localPosition = Vector2.zero; // Reset position
        }
    }

    public void SetRecordMoveable(bool moveable)
    {
        this.RecordMoveable = moveable;
    }

    // The Math refactor for fixed-time Lerping with Animation Curves
    private IEnumerator RecordCoverTransforming(Vector2 targetPos, bool isLocal, float duration, float? targetScale = null)
    {
        float elapsed = 0f;
        Transform t = CurrentVinylRecord.transform;

        Vector3 startPos = isLocal ? t.localPosition : t.position;
        Vector3 startScale = t.localScale;
        Vector3 endScale = targetScale.HasValue ? new Vector3(targetScale.Value, targetScale.Value, 1f) : startScale;

        // Run until the timer hits the exact target duration
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float percent = elapsed / duration;
            float curveMovePercent = moveCurve.Evaluate(percent);

            if (isLocal) t.localPosition = Vector3.LerpUnclamped(startPos, targetPos, curveMovePercent);
            else t.position = Vector3.LerpUnclamped(startPos, (Vector3)targetPos, curveMovePercent);

            if (targetScale.HasValue)
            {
                float curveScalePercent = scaleCurve.Evaluate(percent);
                t.localScale = Vector3.LerpUnclamped(startScale, endScale, curveScalePercent);
            }

            yield return null;
        }

        // Final Snap to guarantee mathematical perfection
        if (isLocal) t.localPosition = targetPos;
        else t.position = (Vector3)targetPos;

        if (targetScale.HasValue) t.localScale = endScale;
    }
}