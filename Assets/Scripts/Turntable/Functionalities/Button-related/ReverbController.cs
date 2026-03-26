using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ButtonControl))]
public class ReverbController : MonoBehaviour
{

    // Values
    private bool isReverb = false;
    public bool IsReverb => isReverb;


    private void Start()
    {
        TurntableManager.Instance.TurntableSystem.ProcessReverb(false);
    }


    public void Reverb()
    {
        TurntableManager.Instance.TurntableSystem.ProcessReverb(isReverb = !isReverb);

        DisplayerManager.Instance.Displayer.UpdateReverbDisplay(isReverb);
    }

}
