using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VinylCover : MonoBehaviour
{
    public Vector2 OrgPosition { get; private set; }


    [Header("References")]
    public VinylCoverSO VinylRecordSO;
    public Record RecordDisk;


    // References
    private SpriteRenderer spriteRenderer;


    private void OnValidate()
    {
        UpdateVinylAppearance();
    }

    private void Awake()
    {
        OrgPosition = transform.position;
        UpdateVinylAppearance();
    }

    private void UpdateVinylAppearance()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (VinylRecordSO != null)
        {
            spriteRenderer.sprite = VinylRecordSO.frontCover;
            this.transform.localScale = Vector3.one * Global.RecordShelfSize;
        }
        else
        {
            Debug.LogWarning("No SO.");
        }
    }

    public void IsFrontCover(bool isFront)
    {
        if (isFront) spriteRenderer.sprite = VinylRecordSO.frontCover;
        else spriteRenderer.sprite = VinylRecordSO.backCover;
    }
}