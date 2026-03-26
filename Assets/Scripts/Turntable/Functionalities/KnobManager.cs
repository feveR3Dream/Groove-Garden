using UnityEngine;

public class KnobManager : MonoBehaviour
{
    public static KnobManager Instance { get; private set; }


    [Header("Knob Controllers")]
    [SerializeField] private TonearmController tonearmControl;
    [SerializeField] private VolumeController volumeControl;
    [SerializeField] private PowerController powerControl;
    [SerializeField] private RPMController rpmControl;


    [Header("Knob Rotation Speed Value")]
    [SerializeField] private float rotationSpeed = 50f;


    public TonearmController TonearmControl => tonearmControl;
    public VolumeController VolumeControl => volumeControl;
    public PowerController PowerControl => powerControl;
    public RPMController RPMControl => rpmControl;

    public float RotationSpeed => rotationSpeed;    


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

    public GameObject TonearmGO => tonearmControl?.gameObject;
    public GameObject VolumeGO => volumeControl?.gameObject;
    public GameObject PowerGO => powerControl?.gameObject;
    public GameObject RPM_GO => rpmControl?.gameObject;

    //--------
    #endregion
}