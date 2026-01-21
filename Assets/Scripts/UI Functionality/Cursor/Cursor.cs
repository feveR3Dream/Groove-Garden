using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask recordLayer;    // Assign layer "Record"
    
    
    // References
    private VinylRecord currentSelectedRecord = null;
    private SpriteRenderer spriteRenderer = null;
    private Color savedColor;


    // Scripts
    private RecordManager recordManager;


    private void Awake()
    {
        if (recordLayer.value != LayerMask.GetMask("Record"))
            Debug.Log("Assign layer \"Record\" only");

        recordManager = GetComponent<RecordManager>();
        if (recordManager == null) Debug.Log("Can't find HandleRecord");
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, recordLayer);

            if (hit.collider != null)
            {
                currentSelectedRecord = hit.collider.GetComponent<VinylRecord>();
                spriteRenderer = currentSelectedRecord.GetComponent<SpriteRenderer>();

                savedColor = spriteRenderer.color;
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
            }
            else
            {
                Debug.Log("Nothing hit at mouse position.");
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, recordLayer);

            if (hit.collider != null &&
                hit.collider.GetComponent<VinylRecord>() == currentSelectedRecord)
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
                //GameManager.Instance.EquipRecord(currentSelectedRecord);

                spriteRenderer.color = savedColor;
                currentSelectedRecord = null;
            }
        }
    }
}
