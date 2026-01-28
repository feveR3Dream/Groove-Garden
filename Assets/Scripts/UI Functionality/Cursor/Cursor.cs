using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask recordLayer;

    [Header("Values")]
    [SerializeField] private float followMouseSpeed;
    [Space]
    [SerializeField] private float alphaFadeSpeed;
    [SerializeField] private float resizeSpeed;

    // References
    private VinylCover currentSelectedRecord = null;
    private SpriteRenderer spriteRenderer = null;
    private Color savedColor;
    private Camera cam;

    // Coroutines
    private Coroutine draggingCoroutine = null;

    // Scripts
    private Record record; // This is your "Memory" of what you are holding

    private void OnEnable()
    {
        EventDispatcher.Instance.Subscribe<RecordToDrag>(ProcessRecord);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.Unsubscribe<RecordToDrag>(ProcessRecord);
    }

    private void Awake()
    {
        if (recordLayer.value != LayerMask.GetMask("Record"))
            Debug.Log("Assign layer \"Record\" only");

        cam = Camera.main;
    }

    private void Update()
    {
        MouseInput();
    }

    private void ProcessRecord(RecordToDrag reference)
    {
        // CASE 1: Stop Command (Null Record)
        if (reference.recordGO == null)
        {
            StopDragging(); // FIX: Don't pass null. Use the cached 'this.record'
            return;
        }

        // CASE 2: Start Command (Valid Record)
        record = reference.recordGO.GetComponent<Record>();

        float alphaTarget = 0.5f;
        float resizeTarget = Global.RecordDraggingSize;

        record.UpdateRecordAlpha(alphaTarget, alphaFadeSpeed);
        record.UpdateRecordSize(resizeTarget, resizeSpeed);

        // Start following the MOUSE
        DragRecord(record.gameObject, true);
    }

    private void DragRecord(GameObject recordGO, bool followMouse)
    {
        if (draggingCoroutine != null)
        {
            StopCoroutine(draggingCoroutine);
            draggingCoroutine = null;
        }

        draggingCoroutine = StartCoroutine(DraggingRecord(recordGO, followMouse));
    }

    private IEnumerator DraggingRecord(GameObject recordGO, bool followMouse)
    {
        while (recordGO != null) // Check if object is still alive
        {
            // LOGIC SPLIT: Mouse vs Center
            Vector2 targetPos;
            if (followMouse)
            {
                targetPos = cam.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                targetPos = RecordManager.Instance.ShowRecordTransform.position;
            }

            // Move
            recordGO.transform.position = Vector2.Lerp(recordGO.transform.position, targetPos, followMouseSpeed * Time.deltaTime);

            // STOP CONDITION: If returning to center, stop when we arrive
            if (!followMouse && Vector2.Distance(recordGO.transform.position, targetPos) <= 0.01f)
            {
                recordGO.transform.position = targetPos; // Snap to finish

                break; // Exit loop
            }

            yield return null;
        }

        draggingCoroutine = null;
        // Optionally clear the reference only when fully returned
        if (!followMouse) record = null;
    }

    private void StopDragging()
    {
        // FIX: Use 'this.record' because the event sent us null
        if (this.record != null)
        {
            // Reset visuals
            this.record.UpdateRecordAlpha(1f, alphaFadeSpeed);
            this.record.UpdateRecordSize(Global.RecordHandlingSize, resizeSpeed);

            // Start returning to CENTER (followMouse = false)
            DragRecord(this.record.gameObject, false);
        }
        else
        {
            // Only stop if we have nothing to return
            if (draggingCoroutine != null) StopCoroutine(draggingCoroutine);
        }
    }



    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, recordLayer);

            if (hit.collider != null)
            {
                VinylCover tempSelectedRecord = hit.collider.GetComponent<VinylCover>();
                if (tempSelectedRecord == RecordManager.Instance.CurrentVinylRecord)
                {
                    currentSelectedRecord = null;
                    return;
                }
                else
                {
                    currentSelectedRecord = tempSelectedRecord;
                    
                    if (spriteRenderer == null)
                        spriteRenderer = currentSelectedRecord.GetComponent<SpriteRenderer>();                    
                }

                currentSelectedRecord.SavedColor = spriteRenderer.color;
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
            }

        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, recordLayer);

            if (hit.collider != null &&
                currentSelectedRecord != null)
            {
                currentSelectedRecord.UpdateCoverAlpha(1);
            }
            else
            {
                if (spriteRenderer == null ||
                    currentSelectedRecord == null) return;

                currentSelectedRecord.UpdateCoverAlpha(0.75f);
                currentSelectedRecord = null;
                Debug.Log("Nothing hit at mouse position.");
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            if (currentSelectedRecord != null)
            {
                Debug.Log("Selected record!");
                RecordManager.Instance.EquipRecordCover(currentSelectedRecord);

                currentSelectedRecord.UpdateCoverAlpha(1);
                currentSelectedRecord = null;
            }
        }
    }
}