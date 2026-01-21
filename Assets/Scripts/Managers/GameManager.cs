using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public VinylRecord CurrentVinylRecord { get; private set; } = null;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        
    }

    public void EquipRecord(VinylRecord record)
    {
        CurrentVinylRecord = record;
        Debug.Log(CurrentVinylRecord.name);
    }

    public void RemoveRecord()
    {
        CurrentVinylRecord = null;
    }
}
