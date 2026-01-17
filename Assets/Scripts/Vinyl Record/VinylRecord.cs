using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VinylRecord : MonoBehaviour
{
    [SerializeField] private VinylRecordSO vinylRecordSO;
    private SpriteRenderer spriteRenderer;

    private void OnValidate()
    {
        UpdateVinylAppearance();
    }

    private void Awake()
    {

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