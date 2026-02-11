using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D), typeof(SpriteRenderer))]
public class LightControl : MonoBehaviour
{
    // Reference
    [HideInInspector] public Light2D LightSetting;
    [HideInInspector] public SpriteRenderer Renderer;

    private void Awake()
    {
        LightSetting = GetComponent<Light2D>();
        if (LightSetting != null)   // Just in case 
        {
            LightSetting.intensity = 5f;
            LightSetting.falloffIntensity = 1.0f;
        }

        Renderer = GetComponent<SpriteRenderer>();

    }
}
