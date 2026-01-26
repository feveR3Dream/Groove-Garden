using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Record : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecordSO recordSO;


    // References
    public AudioClip RecordTrack { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }


    private void OnValidate()
    {
        UpdateRecordAppearance();
    }


    private void Awake()
    {
        UpdateRecordAppearance();
    }


    private void UpdateRecordAppearance()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

        if (recordSO != null)
        {
            SpriteRenderer.sprite = recordSO.picture;
            SpriteRenderer.sortingOrder = 2;
            RecordTrack = recordSO.track;

            transform.localScale = Vector3.one * Global.RecordHandlingSize;
        }
    }
}