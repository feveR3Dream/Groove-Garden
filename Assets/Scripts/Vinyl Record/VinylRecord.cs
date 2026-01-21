using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VinylRecord : MonoBehaviour
{
    public Vector2 OrgPosition { get; private set; }


    [Header("References")]
    [SerializeField] private VinylRecordSO vinylRecordSO;


    // References
    private SpriteRenderer spriteRenderer;


    private void OnValidate()
    {
        UpdateVinylAppearance();
    }

    private void Awake()
    {
        OrgPosition = transform.position;   
    }

    private void UpdateVinylAppearance()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (vinylRecordSO != null)
        {
            spriteRenderer.sprite = vinylRecordSO.cover;

            this.transform.localScale = Vector3.one * Global.RecordShelfSize;
        }
        else
        {
            Debug.LogWarning("No SO.");
        }
    }
}