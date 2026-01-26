using System.Collections;
using Unity.VisualScripting;
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


    [Header("Values")]
    [SerializeField] private float offset;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float unsheatheSpeed = 5f;


    // References
    public RecordExamine CurrentRecordExamine { get; private set; } = RecordExamine.None;
    public RecordSide CurrentRecordSide { get; private set; } = RecordSide.Front;
    public RecordToggle CurrentRecordToggle { get; private set; } = RecordToggle.Hide;

    private Camera currentCamera;
    private SpriteRenderer spriteRenderer;
    private Transform showRecordTransform;

    private Record currentHandlingRecord;    // Used once a record is instantiated


    // Values
    private bool canEquip = true;
    private int orgSortingOrder = 0;
    private int equippedSortingOrder = 3;


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

        currentCamera = Camera.main;
        showRecordTransform = currentCamera.transform;

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
    public void EquipRecord(VinylCover record)
    {
        if (!canEquip) return;
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        canEquip = false;
        CurrentVinylRecord = record;
        CurrentVinylRecord.transform.SetParent(hideRecordTransform);

        spriteRenderer = CurrentVinylRecord.GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.sortingOrder = equippedSortingOrder;

        activeMovementCoroutine = StartCoroutine(EquippingCoroutine());
    }

    private IEnumerator EquippingCoroutine()
    {
        yield return RecordTransforming(Vector2.zero, true, transitionSpeed, Global.RecordHandlingSize);

        UIManager.Instance.UpdateRecordEquip(true);
        activeMovementCoroutine = null;
    }



    // For inspecting & interacting with the record 
    public void RecordTransition(Direction direction)
    {
        if (direction == Direction.None) return;
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        activeMovementCoroutine = StartCoroutine(RecordTransitioning(direction, CurrentRecordExamine));
    }

    private IEnumerator RecordTransitioning(Direction direction, RecordExamine examine)
    {
        UIManager.Instance.UpdateRecordInteraction(false);

        if (direction == Direction.Left)
        {
            if (examine == RecordExamine.Default)
            {
                CurrentRecordExamine = RecordExamine.Info;
                CurrentRecordSide = RecordSide.Back;
            }
        }
        else if (direction == Direction.Right)
        {
            CurrentRecordSide = RecordSide.Front;

            if (examine == RecordExamine.Info)
                CurrentRecordExamine = RecordExamine.Default;
            else if (examine == RecordExamine.Default)
            {
                if (UIManager.Instance.CurrentScreen == Screen.Turntable)
                    CurrentRecordExamine = RecordExamine.Unsheathe;
                else if (UIManager.Instance.CurrentScreen == Screen.Selection)
                    CurrentRecordExamine = RecordExamine.Return;
            }
        }


        if (CurrentRecordExamine == RecordExamine.Unsheathe)
        {
            yield return StartCoroutine(UnsheatheRecord());
        }
        else if (CurrentRecordExamine == RecordExamine.Return)
        {
            ReturnRecord();
        }
        else
        {
            yield return StartCoroutine(FlipRecord(CurrentRecordSide, CurrentRecordExamine));
        }

        activeMovementCoroutine = null;
    }



    // Return the record back to vinyl shelf on selection screen
    private void ReturnRecord()
    {
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        CurrentVinylRecord.transform.SetParent(vinylShelfTransform);

        spriteRenderer = CurrentVinylRecord.GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.sortingOrder = orgSortingOrder;

        activeMovementCoroutine = StartCoroutine(ReturningRecord());
    }

    private IEnumerator ReturningRecord()
    {
        yield return RecordTransforming(CurrentVinylRecord.OrgPosition, false, transitionSpeed, Global.RecordShelfSize);

        CurrentVinylRecord = null;
        canEquip = true;

        UIManager.Instance.UpdateRecordEquip(false);
        UIManager.Instance.UpdateRecordHidden(false);

        CurrentRecordToggle = RecordToggle.Hide;

        activeMovementCoroutine = null;
    }



    // Like the name implies, take the cover off to reveal disk
    private IEnumerator UnsheatheRecord()
    {
        currentHandlingRecord = Instantiate(CurrentVinylRecord.RecordDisk, (Vector2) showRecordTransform.position, Quaternion.identity);
        
        yield return RecordTransforming((Vector2) hideCoverTransform.position, false, unsheatheSpeed, Global.RecordHandlingSize);
        RecordInteractable(true, currentHandlingRecord);
    }



    // Purpose is for the disk to follow mouse position
    private void RecordInteractable(bool interactable, Record record)
    {

        RecordToDrag temp = new RecordToDrag
        { 
            record = interactable ? record : null
        };

        if (!interactable)
        {
            if (currentHandlingRecord)
            {
                Destroy(currentHandlingRecord);
                currentHandlingRecord = null;
            }
        }

        EventDispatcher.Instance.SendEvent(temp);
    }


    private IEnumerator FlipRecord(RecordSide side, RecordExamine examine)
    {
        if (CurrentRecordExamine != examine) 
            CurrentRecordExamine = examine;

        yield return RecordTransforming(Vector2.zero, true, transitionSpeed, Global.RecordHandlingSize);

        if (side == RecordSide.Back) CurrentVinylRecord.IsFrontCover(false);
        else if (side == RecordSide.Front) CurrentVinylRecord.IsFrontCover(true);

        yield return RecordTransforming((Vector2)showRecordTransform.position, false, transitionSpeed, Global.RecordHandlingSize);

        if (CurrentRecordExamine == RecordExamine.Default)
            UIManager.Instance.UpdateRecordInteraction(true);
    }


    public void ToggleRecord()
    {
        if (CurrentVinylRecord == null) return;
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        if (CurrentRecordToggle == RecordToggle.Hide)
        {
            CurrentRecordToggle = RecordToggle.Show;
            activeMovementCoroutine = StartCoroutine(TogglingRecord(true));
        }
        else if (CurrentRecordToggle == RecordToggle.Show)
        {
            if (CurrentRecordExamine == RecordExamine.Info)
            {
                Debug.Log("Back to Default");
                activeMovementCoroutine = StartCoroutine(FlipRecord(RecordSide.Front, RecordExamine.Default));
                return;
            }
            else
            {
                Debug.Log("Switch off Record");
                CurrentRecordToggle = RecordToggle.Hide;
                activeMovementCoroutine = StartCoroutine(TogglingRecord(false));
            }
        }
    }


    private IEnumerator TogglingRecord(bool show)
    {
        Vector2 destination = show ? (Vector2)showRecordTransform.position : (Vector2)hideRecordTransform.position;

        yield return RecordTransforming(destination, false, transitionSpeed, Global.RecordHandlingSize);

        activeMovementCoroutine = null;
        CurrentRecordExamine = show ? RecordExamine.Default : RecordExamine.None;

        if (show)
        {
            if (CurrentRecordExamine == RecordExamine.Default)
                UIManager.Instance.UpdateRecordInteraction(true);
        }
        else
        {
            CurrentVinylRecord.IsFrontCover(true);
            UIManager.Instance.UpdateRecordInteraction(false);
        }

        UIManager.Instance.UpdateRecordHidden(show);
    }



    private IEnumerator RecordTransforming(Vector2 targetPos, bool isLocal, float speed, float? targetScale = null)
    {
        Transform t = CurrentVinylRecord.transform;

        while (true)
        {
            // 1. Move
            if (isLocal)
                t.localPosition = Vector3.Lerp(t.localPosition, targetPos, speed * Time.deltaTime);
            else
                t.position = Vector3.Lerp(t.position, targetPos, speed * Time.deltaTime);

            // 2. Scale (if requested)
            if (targetScale.HasValue)
            {
                t.localScale = Vector3.Lerp(t.localScale, Vector3.one * targetScale.Value, speed * Time.deltaTime);
            }

            // 3. Check Distance
            float dist = isLocal ? Vector3.Distance(t.localPosition, targetPos) : Vector3.Distance(t.position, targetPos);

            if (dist < 0.01f) break;
            yield return null;
        }

        // Snap Final Values
        if (isLocal) t.localPosition = targetPos;
        else t.position = targetPos;

        if (targetScale.HasValue) t.localScale = Vector3.one * targetScale.Value;
    }
}