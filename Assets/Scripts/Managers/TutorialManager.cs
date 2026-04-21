using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial Pictures Reference")]
    [SerializeField] private Sprite[] listOfTutorials;

    [Header("Tutorial Panel Reference")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImageDisplay; // The actual UI Image component

    [Header("Tutorial Button References")]
    [SerializeField] private Button tutorialButton;      // Toggles the whole panel
    [SerializeField] private Button nextButton;          // Right button
    [SerializeField] private Button prevButton;          // Left button

    // Values
    private bool isVisible = false;
    public bool IsVisible { get => isVisible; }

    private int currentTutorialIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        tutorialPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Wire up all our buttons in code so we don't have to drag-and-drop in the Inspector
        tutorialButton?.onClick.AddListener(ToggleTutorialPanel);
        nextButton?.onClick.AddListener(NextTutorial);
        prevButton?.onClick.AddListener(PreviousTutorial);
    }

    private void OnDisable()
    {
        // Always clean up your listeners!
        tutorialButton?.onClick.RemoveListener(ToggleTutorialPanel);
        nextButton?.onClick.RemoveListener(NextTutorial);
        prevButton?.onClick.RemoveListener(PreviousTutorial);
    }

    private void ToggleTutorialPanel()
    {
        isVisible = !isVisible;
        tutorialPanel.SetActive(isVisible);

        // When the player opens the tutorial, reset to the first page and update the display
        if (isVisible)
        {
            currentTutorialIndex = 0;
            UpdateTutorialDisplay();
        }
    }

    private void NextTutorial()
    {
        // Prevent going out of bounds
        if (currentTutorialIndex < listOfTutorials.Length - 1)
        {
            currentTutorialIndex++;
            UpdateTutorialDisplay();
        }
    }

    private void PreviousTutorial()
    {
        // Prevent going into negative numbers
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            UpdateTutorialDisplay();
        }
    }

    private void UpdateTutorialDisplay()
    {
        // Failsafe: Don't do anything if you forgot to assign sprites in the Inspector
        if (listOfTutorials == null || listOfTutorials.Length == 0) return;

        // 1. Swap the sprite
        tutorialImageDisplay.sprite = listOfTutorials[currentTutorialIndex];

        // --- DELETED SetNativeSize() HERE! ---
        // Let the Unity Inspector RectTransform handle the size now!

        // 2. Smart Buttons: Disable the 'Previous' button if we are on the first page, 
        // and disable the 'Next' button if we are on the last page.
        if (prevButton != null)
            prevButton.interactable = (currentTutorialIndex > 0);

        if (nextButton != null)
            nextButton.interactable = (currentTutorialIndex < listOfTutorials.Length - 1);
    }
}