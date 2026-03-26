using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerManager : MonoBehaviour
{
    public static DisplayerManager Instance { get; private set; }


    [Header("Displayer Reference")]
    [SerializeField] private Displayer displayer;

    public Displayer Displayer => displayer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

}
