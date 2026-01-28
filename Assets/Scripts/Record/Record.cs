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


    // Coroutines
    private Coroutine alphaCoroutine = null;
    private Coroutine resizeCoroutine = null;


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
            SpriteRenderer.sortingOrder = Global.SpawnedInSortingOrder;
            RecordTrack = recordSO.track;

            transform.localScale = Vector3.one * Global.RecordHandlingSize;
        }
    }

    public void UpdateRecordAlpha(float alpha, float fadeSpeed)
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


    public void UpdateRecordSize(float size, float resizeSpeed)
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
}