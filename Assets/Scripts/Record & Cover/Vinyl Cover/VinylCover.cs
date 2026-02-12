using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VinylCover : MonoBehaviour, IButtonInteractable
{
    public Vector2 OrgPosition { get; private set; }


    [Header("References")]
    public VinylCoverSO VinylRecordSO;
    public GameObject RecordDisk;


    // References
    public SpriteRenderer SpriteRenderer { get; private set; }
    private Color savedColor;


    // Coroutines
    private Coroutine resizeCoroutine = null;



    private void OnValidate()
    {
        UpdateCoverAppearance();
    }

    private void Awake()
    {
        OrgPosition = transform.position;
        UpdateCoverAppearance();
    }

    private void UpdateCoverAppearance()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

        if (VinylRecordSO != null)
        {
            SpriteRenderer.sprite = VinylRecordSO.frontCover;
            SpriteRenderer.sortingOrder = Global.OnShelfSortingOrder;
            SpriteRenderer.sortingLayerID = SortingLayer.layers[3].id; // "Shelf, Covers & Album Names"

            this.transform.localScale = Vector3.one * Global.RecordCoverShelfSize;
        }
        else
        {
            Debug.LogWarning("No SO.");
        }
    }


    private void UpdateCoverAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        Color tempColor = savedColor;
        tempColor.a = alpha;
        SpriteRenderer.color = tempColor;
    }


    public void UpdateCoverSize(float size, float resizeSpeed)
    {
        Vector3 targetSize = Vector3.one * size;

        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
            resizeCoroutine = null;
        }

        resizeCoroutine = StartCoroutine(ResizingCover(targetSize, resizeSpeed));
    }

    private IEnumerator ResizingCover(Vector3 targetSize, float resizeSpeed)
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


    public void IsFrontCover(bool isFront)
    {
        if (isFront) SpriteRenderer.sprite = VinylRecordSO.frontCover;
        else SpriteRenderer.sprite = VinylRecordSO.backCover;
    }


    public void ButtonInteracted(bool registered, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Down)
        {
            if (RecordManager.Instance.CurrentVinylRecord != null) return;
            savedColor = SpriteRenderer.color;
        }

        else if (mouseButton == MouseButton.Hold)
        {
            if (RecordManager.Instance.CurrentVinylRecord != null) return;
            UpdateCoverAlpha(0.75f);
        }

        else if (mouseButton == MouseButton.Up)
        {
            if (RecordManager.Instance.CurrentVinylRecord != null) return;
            UpdateCoverAlpha(1f);

            if (registered)
            {
                RecordManager.Instance.EquipRecordCover(this);
            }
        } 

    }
}