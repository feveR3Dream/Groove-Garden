using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VinylCover : MonoBehaviour
{
    public Vector2 OrgPosition { get; private set; }


    [Header("References")]
    public VinylCoverSO VinylRecordSO;
    public GameObject RecordDisk;


    // References
    public SpriteRenderer SpriteRenderer { get; private set; }
    private Color savedColor;
    public Color SavedColor
    {
        get { return savedColor; }
        set { savedColor = value; }
    }


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

            this.transform.localScale = Vector3.one * Global.RecordCoverShelfSize;
        }
        else
        {
            Debug.LogWarning("No SO.");
        }
    }


    public void UpdateCoverAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        Color tempColor = SavedColor;
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
}