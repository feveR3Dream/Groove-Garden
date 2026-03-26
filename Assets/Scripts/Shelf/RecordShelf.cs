using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RecordShelf : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Sorter> sorterList = new List<Sorter>();
    [Space]
    [SerializeField] private Transform recordCoverContainer;


    [Header("Values")]
    [SerializeField] private float spacing = 5f;


    private void Start()
    {
        // Safe to run automatically in Play Mode
        RefreshShelf();
    }

    // Right-click the component title in Inspector and select "Refresh Shelf" to test!
    [ContextMenu("Refresh Shelf")]
    public void RefreshShelf()
    {
        int childCount = recordCoverContainer.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = recordCoverContainer.GetChild(i).gameObject;

            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        if (Application.isPlaying) Global.SortingValue.SorterDictionary.Clear();

        OrganizeRecordRow();
    }

    private void OrganizeRecordRow()
    {
        if (sorterList == null || sorterList.Count == 0) return;

        foreach (Sorter sorter in sorterList)
        {
            if (sorter.SpawnRow == null) continue;

            List<GameObject> currentBatchList = new List<GameObject>();

            for (int i = 0; i < sorter.RecordList.Count; i++)
            {
                Vector2 spawnPos = (Vector2)sorter.SpawnRow.position + (Vector2.right * i * spacing);

                if (i == 0 && (spawnPos.x <= UIManager.Instance.LeftShelfLimit.position.x || spawnPos.x >= UIManager.Instance.RightShelfLimit.position.x))
                {
                    Debug.LogWarning($"SpawnRow '{sorter.SpawnRow.name}' is outside shelf limits!");
                    break;
                }

                if (spawnPos.x >= UIManager.Instance.RightShelfLimit.position.x) break;

                GameObject prefab = sorter.RecordList[i];
                if (prefab != null)
                {
                    GameObject newRecord = Instantiate(prefab, spawnPos, Quaternion.identity, recordCoverContainer);

                    currentBatchList.Add(newRecord);
                }
            }

            if (Application.isPlaying)
            {
                Global.SortingValue.SorterDictionary.Add(sorter, currentBatchList);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (sorterList.Count == 0 || sorterList == null) return;

        foreach (Sorter sorter in sorterList)
        {
            if (sorter.SpawnRow == null) continue;
            Gizmos.DrawWireCube(sorter.SpawnRow.position, Vector3.one * 2.87f);
        }
    }
}

[Serializable]
public class Sorter
{
    public Transform SpawnRow;
    public List<GameObject> RecordList = new List<GameObject>();
}
