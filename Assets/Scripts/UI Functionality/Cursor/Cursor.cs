using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask recordLayer;    // Assign layer "Record"


    [Header("Values")]
    [SerializeField] private float speed;
    
    
    // References
    private VinylCover currentSelectedRecord = null;
    private SpriteRenderer spriteRenderer = null;
    private Color savedColor;

    private Record item;


    // Values
    private bool hasRecord = false; // enhance this  later


    // Coroutines
    private Coroutine toMouseCoroutine;


    private void OnEnable()
    {
        EventDispatcher.Instance.Subscribe<RecordToDrag>(MoveToMouse);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.Unsubscribe<RecordToDrag>(MoveToMouse);
    }


    private void MoveToMouse(RecordToDrag reference)
    {
        if (toMouseCoroutine != null)
            StopCoroutine(toMouseCoroutine);

        toMouseCoroutine = StartCoroutine(MovingToMouse(reference.recordGO));
    }

    private IEnumerator MovingToMouse(GameObject recordGO)
    {
        if (item == null) yield break;

        //this.item = item;
        //item.SpriteRenderer.color 
        
        // ADD COLOR FUNCTIONS WITHIN RECORD & VINYL COVER, DON'T CHANGE THEM HERE!!!!

        //while (true)
        //{
            
        //}
    }


    private void Awake()
    {
        if (recordLayer.value != LayerMask.GetMask("Record"))
            Debug.Log("Assign layer \"Record\" only");
    }


    void Update()
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
                else currentSelectedRecord = tempSelectedRecord;
                
                spriteRenderer = currentSelectedRecord.GetComponent<SpriteRenderer>();

                savedColor = spriteRenderer.color;
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
            }

        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, recordLayer);

            if (hit.collider != null &&
                hit.collider.GetComponent<VinylCover>() == currentSelectedRecord)
            {
                Color tempColor = savedColor;
                tempColor.a = 0.75f;
                spriteRenderer.color = tempColor;
            }
            else
            {
                if (spriteRenderer == null ||
                    currentSelectedRecord == null) return;

                spriteRenderer.color = savedColor;
                spriteRenderer = null;
                currentSelectedRecord = null;
                Debug.Log("Nothing hit at mouse position.");
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            if (currentSelectedRecord != null)
            {
                Debug.Log("Selected record!");
                RecordManager.Instance.EquipRecord(currentSelectedRecord);
                hasRecord = true;

                spriteRenderer.color = savedColor;
                currentSelectedRecord = null;
            }
        }
    }
}
