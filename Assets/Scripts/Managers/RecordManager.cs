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
    public GameObject CurrentRecordGO { get; private set; } = null;   // MAKE THIS GET, PRIVATE SET
    public Transform ShowRecordTransform { get; private set; }

    private Camera cam;
    private SpriteRenderer spriteRenderer;


    // Values
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

        CurrentVinylRecord.SpriteRenderer.sortingOrder = Global.EquippedSortingOrder;
        //spriteRenderer = CurrentVinylRecord.GetComponent<SpriteRenderer>();
        //if (spriteRenderer) spriteRenderer.sortingOrder = equippedSortingOrder;

        activeMovementCoroutine = StartCoroutine(EquippingCoroutine());
    }

    private IEnumerator EquippingCoroutine()
    {
        yield return RecordCoverTransforming(Vector2.zero, true, transitionSpeed, Global.RecordHandlingSize);

        UIManager.Instance.UpdateRecordCoverEquip(true);
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
        yield return RecordCoverTransforming(CurrentVinylRecord.OrgPosition, false, transitionSpeed, Global.RecordShelfSize);

        CurrentVinylRecord = null;
        canEquip = true;

        UIManager.Instance.UpdateRecordCoverEquip(false);
        UIManager.Instance.UpdateRecordCoverHidden(false);

        CurrentRecordToggle = RecordToggle.Hide;

        activeMovementCoroutine = null;
    }


    // Like the name implies, take the cover off to reveal disk
    private IEnumerator UnsheathingRecordToggle(bool unsheathe)    // FIX THIS LATER
    {
        Vector2 targetPos = unsheathe ? (Vector2) hideCoverTransform.position : (Vector2) ShowRecordTransform.position;
        if (!unsheathe) RecordCoverInteractable(false, CurrentRecordGO);

        if (CurrentVinylRecord.RecordDisk != null &&
            CurrentRecordGO == null)
        {
            GameObject recordDisk = CurrentVinylRecord.RecordDisk;
            CurrentRecordGO = Instantiate(recordDisk, (Vector2)ShowRecordTransform.position, Quaternion.identity);
        }

        yield return RecordCoverTransforming(targetPos, false, unsheatheSpeed, Global.RecordHandlingSize);
        if (unsheathe) RecordCoverInteractable(true, CurrentRecordGO);
        else
        {
            CurrentRecordExamine = RecordExamine.Default;

            UIManager.Instance.UpdateRecordInteraction(true);

            Destroy(CurrentRecordGO);
            CurrentRecordGO = null;
        }
    }

    
    // Purpose is for the disk to follow mouse position
    private void RecordCoverInteractable(bool interactable, GameObject recordGO)
    {
        RecordToDrag temp = new RecordToDrag
        { 
            recordGO = interactable ? recordGO : null
        };

        EventDispatcher.Instance.SendEvent(temp);
    }

    private IEnumerator FlipRecordCover(RecordSide side, RecordExamine examine)
    {
        if (CurrentRecordExamine != examine) 
            CurrentRecordExamine = examine;

        yield return RecordCoverTransforming(Vector2.zero, true, transitionSpeed, Global.RecordHandlingSize);

        if (side == RecordSide.Back) CurrentVinylRecord.IsFrontCover(false);
        else if (side == RecordSide.Front) CurrentVinylRecord.IsFrontCover(true);

        yield return RecordCoverTransforming((Vector2)ShowRecordTransform.position, false, transitionSpeed, Global.RecordHandlingSize);

        if (CurrentRecordExamine == RecordExamine.Default)
            UIManager.Instance.UpdateRecordInteraction(true);
    }

    public void ToggleCover()    // BOTTOM BUTTON PRESSED
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
                activeMovementCoroutine = StartCoroutine(FlipRecordCover(RecordSide.Front, RecordExamine.Default));
            }
            else if (CurrentRecordExamine == RecordExamine.Unsheathe)
            {
                Debug.Log("Sheathe Record and Back to Default");
                activeMovementCoroutine = StartCoroutine(UnsheathingRecordToggle(false));
            }
            else
            {
                Debug.Log("Switch off Record");
                CurrentRecordToggle = RecordToggle.Hide;
                activeMovementCoroutine = StartCoroutine(TogglingRecord(false));
            }

            return;
        }
    }

    private IEnumerator TogglingRecord(bool show)
    {
        Vector2 destination = show ? (Vector2)ShowRecordTransform.position : (Vector2)hideRecordTransform.position;

        yield return RecordCoverTransforming(destination, false, transitionSpeed, Global.RecordHandlingSize);

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

        UIManager.Instance.UpdateRecordCoverHidden(show);
    }




    private IEnumerator RecordCoverTransforming(Vector2 targetPos, bool isLocal, float speed, float? targetScale = null)
    {
        // 1. Scale
        CurrentVinylRecord.UpdateCoverSize(targetScale.Value, speed);

        Transform t = CurrentVinylRecord.transform;
        while (true)
        {
            // 2. Move
            if (isLocal)
                t.localPosition = Vector3.Lerp(t.localPosition, targetPos, speed * Time.deltaTime);
            else
                t.position = Vector3.Lerp(t.position, targetPos, speed * Time.deltaTime);

            // 3. Check Distance
            float dist = isLocal ? Vector3.Distance(t.localPosition, targetPos) : Vector3.Distance(t.position, targetPos);

            if (dist < 0.01f) break;
            yield return null;
        }

        // Snap Final Values
        if (isLocal) t.localPosition = targetPos;
        else t.position = targetPos;

    }
}