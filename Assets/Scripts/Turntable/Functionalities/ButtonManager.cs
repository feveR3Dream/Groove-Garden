using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }


    [Header("Button Controllers")]
    [SerializeField] private RecordPlacementController recordPlacementControl;
    [SerializeField] private StartStopController startStopControl;
    [SerializeField] private ReverbController reverbControl;


    public RecordPlacementController RecordPlacementControl => recordPlacementControl;
    public StartStopController StartStopControl => startStopControl;
    public ReverbController ReverbControl => reverbControl;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    #region GETTERS
    //-------------

    public GameObject PlatterGO => recordPlacementControl?.gameObject;
    public Vector2 RecordPlacementPos => (Vector2) PlatterGO.transform.position;
    public GameObject StartStopGO => startStopControl?.gameObject;
    public GameObject ReverbGO => reverbControl?.gameObject;

    //--------
    #endregion
}