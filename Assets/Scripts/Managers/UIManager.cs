using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button rightButton;   // Used mostly for transitioning
    public Button leftButton;   //  and moving left <-> right
    public Button bottomButton;     // Access current vinyl record
    [Space]
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI bottomText;


    //[Header("References")]
    //[SerializeField] // Add travel point here.


    private enum Screen
    {
        Turntable,
        Selection
    }
    private Screen currentScreen = Screen.Turntable; // Default


    private void OnEnable()
    {
        rightButton?.onClick.AddListener(() => Transition(currentScreen));
        leftButton?.onClick.AddListener(() => Transition(currentScreen));

        AddHoverListener(rightButton, () => OnHover(rightText, true), () => OnHover(rightText, false));
        AddHoverListener(leftButton, () => OnHover(leftText, true), () => OnHover(leftText, false));
        AddHoverListener(bottomButton, () => OnHover(bottomText, true), () => OnHover(bottomText, false));
    }


    private void OnDisable()
    {
        rightButton?.onClick.RemoveListener(() => Transition(currentScreen));
        leftButton?.onClick.RemoveListener(() => Transition(currentScreen));
    }


    private void OnHover(TextMeshProUGUI label, bool isEntering)
    {
        if (label == null) return;

        label.color = isEntering ? Color.yellow : Color.white;
        label.transform.localScale = isEntering ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one;
    }

    private void UpdateScreen(Screen screen)
    {
        if (screen == Screen.Turntable)
        {
            leftButton?.gameObject.SetActive(false);
            rightButton?.gameObject.SetActive(true);
        }
        else if (screen == Screen.Selection) 
        {
            leftButton?.gameObject.SetActive(true);
            rightButton?.gameObject.SetActive(true);
        }
    }

    private void Transition(Screen screen)
    {
        // Set currentScreen & turn off all button
        // Wait until finish transitioning
        // Turn on/fade in all button according to current screen

        if (screen == Screen.Turntable)
        {
            leftButton?.gameObject.SetActive(false);
            rightButton?.gameObject.SetActive(true);
        }
        else if (screen == Screen.Selection)
        {
            leftButton?.gameObject.SetActive(true);
            rightButton?.gameObject.SetActive(true);
        }
    }


    private void AddHoverListener(Button button, System.Action onEnter, System.Action onExit)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        // Pointer entry
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => onEnter.Invoke());
        trigger.triggers.Add(enterEntry);

        // Pointer exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => onExit.Invoke());
        trigger.triggers.Add(exitEntry);
    }
}