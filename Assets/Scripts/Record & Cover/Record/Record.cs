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

    // Values
    private Vector2 orgPosition;
    private float alphaFadeSpeed = 10f;
    private float resizeSpeed = 10f;

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
            SpriteRenderer.sortingOrder = Global.SpawnedInSortingOrder;
            RecordTrack = recordSO.track;

            transform.localScale = Vector3.one * Global.RecordDiskSpawnedSize;
        }
    }

    private void UpdateRecordAlpha(float alpha, float fadeSpeed)
    {
        alpha = Mathf.Clamp01(alpha); 

        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
            alphaCoroutine = null;
        }

        alphaCoroutine = StartCoroutine(UpdatingRecordAlpha(alpha, fadeSpeed));
    }

    private IEnumerator UpdatingRecordAlpha(float alpha, float fadeSpeed)
    {
        Color targetColor = SpriteRenderer.color;
        while (SpriteRenderer.color.a != alpha)
        {
            targetColor.a = alpha; 

            Color tempColor = Color.Lerp(SpriteRenderer.color, targetColor, fadeSpeed * Time.deltaTime);
            SpriteRenderer.color = tempColor;

            yield return null;
        }

        SpriteRenderer.color = targetColor;
        alphaCoroutine = null;
    }


    private void UpdateRecordSize(float size, float resizeSpeed)
    {
        Vector3 targetSize = Vector3.one * size;

        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
            resizeCoroutine = null;
        }

        resizeCoroutine = StartCoroutine(ResizingRecord(targetSize, resizeSpeed));
    }

    private IEnumerator ResizingRecord(Vector3 targetSize, float resizeSpeed)
    {   
        while (!Mathf.Approximately(transform.localScale.magnitude, targetSize.magnitude))
        {
            Vector3 tempScale = Vector3.Lerp(transform.localScale, targetSize, resizeSpeed * Time.deltaTime);
            transform.localScale = tempScale;

            yield return null;
        }

        transform.localScale = targetSize;
        resizeCoroutine = null;
    }

    public void MoveTo(RecordMoveTo moveTo, float moveToSpeed) // This just moves the record
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(MovingTo(moveTo, moveToSpeed));
    }

    private IEnumerator MovingTo(RecordMoveTo moveTo, float moveToSpeed) // Now we can use Vector3 hehehe 
    {
        if (moveTo == RecordMoveTo.To_Mouse)
        {
            RecordManager.Instance.SetRecordMoveable(true);
            SpriteRenderer.sortingOrder = Global.HandlingSortingOrder;

            UpdateRecordAlpha(0.5f, alphaFadeSpeed);
            UpdateRecordSize(Global.RecordDiskHandlingSize, resizeSpeed);
        }
        else if (moveTo == RecordMoveTo.To_Turntable)
        {
            UpdateRecordAlpha(1f, alphaFadeSpeed);
            UpdateRecordSize(Global.RecordDiskHandlingSize, resizeSpeed);
        }
        else if (moveTo == RecordMoveTo.To_Spawned_Pos)
        {
            RecordManager.Instance.SetRecordMoveable(false);
            SpriteRenderer.sortingOrder = Global.SpawnedInSortingOrder;

            TurntableControl.Instance.EquipRecord = false;

            Debug.Log($"Equip Record: {TurntableControl.Instance.EquipRecord}");

            UpdateRecordAlpha(1f, alphaFadeSpeed);
            UpdateRecordSize(Global.RecordDiskSpawnedSize, resizeSpeed);
        }

        Vector2 targetPos = this.transform.position;

        while (true)
        {
            if (moveTo == RecordMoveTo.To_Mouse)
            {
                if (RecordManager.Instance.CurrentRecord != null)
                    targetPos = cam.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (moveTo == RecordMoveTo.To_Turntable)
            {
                targetPos = TurntableControl.Instance.RecordPlacementPosition;
            }
            else if (moveTo == RecordMoveTo.To_Spawned_Pos)
            {
                targetPos = orgPosition;
            }

            Vector2 tempPos = Vector2.Lerp(transform.position, targetPos, moveToSpeed * Time.deltaTime);
            transform.position = tempPos;

            if (moveTo != RecordMoveTo.To_Mouse &&
                Vector2.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;

                //if (moveTo == RecordMoveTo.To_Spawned_Pos) Destroy(gameObject);
                break;
            }

            yield return null;
        }

        moveCoroutine = null;
    }

    // MOVE RECORD FOLLOW MOUSE TO THIS SCRIPT
}