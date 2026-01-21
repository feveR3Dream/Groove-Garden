using System.Collections;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }
    public VinylRecord CurrentVinylRecord { get; private set; } = null;

    [Header("References")]
    [SerializeField] private Transform bottomCamLimit;
    [SerializeField] private Transform hideRecordTransform;
    

    [Header("Values")]
    [SerializeField] private float heightDifference;
    [SerializeField] private float speed = 5f;


    // References
    private Camera currentCamera;
    private SpriteRenderer spriteRenderer;
    private Transform showRecordTransform;


    // Values
    private bool recordShown = false;
    private bool canEquip = true;    // If true, can select vinyl record off the shelf
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

        // 1. Setup positions safely
        if (hideRecordTransform != null && bottomCamLimit != null)
        {
            // Initial position for the shelf
            hideRecordTransform.position = new Vector2(bottomCamLimit.position.x, bottomCamLimit.position.y - Mathf.Abs(heightDifference));
        }
    }

    public void EquipRecord(VinylRecord record)
    {
        if (!canEquip) return;

        // Stop any existing movement immediately
        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);

        canEquip = false;
        CurrentVinylRecord = record;
        CurrentVinylRecord.transform.SetParent(hideRecordTransform);

        // Sorting Order
        spriteRenderer = CurrentVinylRecord.GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.sortingOrder = equippedSortingOrder;

        // Start the new movement
        activeMovementCoroutine = StartCoroutine(EquippingCoroutine());
    }

    private IEnumerator EquippingCoroutine()
    {
        // Target is the Parent (0,0 local)
        while (Vector2.Distance(CurrentVinylRecord.transform.localPosition, Vector2.zero) > 0.01f)
        {
            // Move Logic
            CurrentVinylRecord.transform.localPosition = Vector2.Lerp(CurrentVinylRecord.transform.localPosition, Vector2.zero, speed * Time.deltaTime);

            // Scale logic
            Vector3 targetScale = Vector3.one * Global.RecordHandlingSize;
            CurrentVinylRecord.transform.localScale = Vector3.Lerp(CurrentVinylRecord.transform.localScale, targetScale, speed * Time.deltaTime);

            yield return null;
        }

        // Snap to finish
        CurrentVinylRecord.transform.localPosition = Vector2.zero;
        canEquip = true;
        activeMovementCoroutine = null;

        recordShown = false;

        EquipStatus temp = new EquipStatus { equipped = true };
        EventDispatcher.Instance.SendEvent(temp);
    }

    public void ToggleRecord()
    {
        if (CurrentVinylRecord == null) return;

        recordShown = !recordShown;

        if (activeMovementCoroutine != null) StopCoroutine(activeMovementCoroutine);
        activeMovementCoroutine = StartCoroutine(TogglingRecord(recordShown));
    }

    private IEnumerator TogglingRecord(bool show)
    {
        // Determine targets based on World Position
        // Note: Moving to currentCamera.transform.position is weird (center of screen), 
        // but I kept it as per your code. Usually you want an offset (e.g. Camera Y - 2).
        Vector2 destination = show ? (Vector2)showRecordTransform.position : hideRecordTransform.position;

        while (Vector2.Distance(CurrentVinylRecord.transform.position, destination) > 0.01f)
        {
            CurrentVinylRecord.transform.position = Vector2.Lerp(CurrentVinylRecord.transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        CurrentVinylRecord.transform.position = destination;
        activeMovementCoroutine = null;

        ToggleRecord temp = new ToggleRecord { hide = show };
        EventDispatcher.Instance.SendEvent(temp);
    }
}