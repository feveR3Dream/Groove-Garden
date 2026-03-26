using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KnobControl))]
public class VolumeController : MonoBehaviour
{

    [Header("Volume Knob")]
    [Range(0f, 360f)]
    [SerializeField] private float minVolumeKnobAngle; // 0
    [Range(0f, 360f)]
    [SerializeField] private float maxVolumeKnobAngle; // 1


    // References
    private KnobControl knobControl;


    // Values
    private float volume;


    private void Awake()
    {
        knobControl = GetComponent<KnobControl>();
    }


    private void Start()
    {
        TurntableManager.Instance.InitializeKnobAngle(this.gameObject, (minVolumeKnobAngle + maxVolumeKnobAngle) / 2f);
        ProcessVolumeKnob();
    }


    public void TurnKnob(float rotateSpeed)
    {
        knobControl.UpdateRotationClamped(transform, minVolumeKnobAngle, maxVolumeKnobAngle, rotateSpeed);
        ProcessVolumeKnob();
    }


    public void ProcessVolumeKnob()
    {
        Vector2 dir = transform.up;
        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float clockwiseAngle = knobControl.NormalizeClockwise(rawAngle);
        float convertedAngle = 360 - clockwiseAngle;

        volume = Mathf.InverseLerp(minVolumeKnobAngle, maxVolumeKnobAngle, convertedAngle);
        TurntableManager.Instance.TurntableSystem.UpdateVolume(volume);

        if (TurntableManager.Instance.TurnedOn) DisplayerManager.Instance.Displayer.UpdateVolumeDisplay(volume);
    }


    public void ReleasedVolumeKnob()
    {
        knobControl.CanCalculateOffset = true;
    }


    private void OnDrawGizmos()
    {
        Vector2 minVolumeKnobDir = new Vector2(Mathf.Cos(-(minVolumeKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(minVolumeKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + minVolumeKnobDir * 2.5f);

        Vector2 maxVolumeKnobDir = new Vector2(Mathf.Cos(-(maxVolumeKnobAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin(-(maxVolumeKnobAngle + 90f) * Mathf.Deg2Rad));
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + maxVolumeKnobDir * 2.5f);
    }
}
