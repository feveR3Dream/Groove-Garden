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


    [Header("Values")]
    [SerializeField] private float transitionSpeed = 2.5f;    // Camera transition speed for transitioning
    [SerializeField] private float browsingSpeed = 2.5f;      // Camera pan speed for browsing at SELECTION
    [SerializeField] private float opacitySpeed = 2.5f;


    [Header("References")]
    [SerializeField] private Transform leftCameraLimit;       // These values must be within
    [SerializeField] private Transform rightCameraLimit;     //  left & right & top & bottom's 
    [SerializeField] private Transform topCameraLimit;      //   shelf limit
    [SerializeField] private Transform bottomCameraLimit;  //
    [Space]
    [SerializeField] private Transform leftShelfLimit;    
    [SerializeField] private Transform rightShelfLimit;
    [SerializeField] private Transform topShelfLimit;
    [SerializeField] private Transform bottomShelfLimit;


    [Header("Turntable Reference")]
    [SerializeField] private Transform turntable;

    // References
    private Screen currentScreen = Screen.Turntable;    // Default screen
    private Direction currentHoverDirection = Direction.None;
    private Camera currentCamera;


    // Values
    private const float dimTextAlpha = 0.1f;
    private const float highlightTextAlpha = 0.25f;

    private Vector2 turntableScreenPos;
    private Vector2 selectionScreenPos; // Will work on later.
    private bool transitioning;
    private bool canBrowse;


    private void OnEnable()
    {
        rightButton?.onClick.AddListener(() => Transition(Direction.Right));
        leftButton?.onClick.AddListener(() => Transition(Direction.Left));

        //AddHoverListener(rightButton, () => OnHover(rightText, true), () => OnHover(rightText, false));
        //AddHoverListener(leftButton, () => OnHover(leftText, true), () => OnHover(leftText, false));
        //AddHoverListener(bottomButton, () => OnHover(bottomText, true), () => OnHover(bottomText, false));

        //AddHoverListener(leftButton, () => OnHover(Direction.Left));
        //AddHoverListener(rightButton, () => OnHover(Direction.Right));

        AddHoverListener(leftButton, () => currentHoverDirection = Direction.Left, () => currentHoverDirection = Direction.None);
        AddHoverListener(rightButton, () => currentHoverDirection = Direction.Right, () => currentHoverDirection = Direction.None);
    }


    private void OnDisable()
    {
        rightButton?.onClick.RemoveListener(() => Transition(Direction.Right));
        leftButton?.onClick.RemoveListener(() => Transition(Direction.Left));
    }


    private void Awake()
    {
        selectionScreenPos = Camera.main.transform.position;
        currentCamera = Camera.main;

        SetButtonState(rightButton, rightText, true, interaction: true);
        SetButtonState(leftButton, leftText, false, interaction: false);
        SetButtonState(bottomButton, bottomText, false, interaction: false);
    }


    private void Update()
    {
        if (currentScreen == Screen.Selection &&
            !transitioning)
        {
            Corrector();

            if (currentHoverDirection != Direction.None)
            {
                ProcessContinuousHover(currentHoverDirection);
            }
        }
    }


    private void ProcessContinuousHover(Direction direction)
    {
        float speed = browsingSpeed * Time.deltaTime;

        if (direction == Direction.Left)
        {
            // "Look Ahead" Logic:
            // We only move Left if we are NOT currently touching the Left Wall
            // (Use a small buffer like 0.01f to prevent floating point flicker)
            if (leftCameraLimit.position.x > leftShelfLimit.position.x + 0.01f)
            {
                currentCamera.transform.position += Vector3.left * speed;
            }
        }
        else if (direction == Direction.Right)
        {
            // We only move Right if we are NOT currently touching the Right Wall
            if (rightCameraLimit.position.x < rightShelfLimit.position.x - 0.01f)
            {
                currentCamera.transform.position += Vector3.right * speed;
            }
        }
    }

    private void Corrector()
    {
        // This function stays exactly as you wrote it. 
        // Its job is just to push the camera back if it accidentally teleported out of bounds.
        // I removed the "canBrowse" logic from here because ProcessContinuousHover now handles its own permissions.

        Vector3 correction = Vector3.zero;

        if (leftCameraLimit.position.x <= leftShelfLimit.position.x)
            correction += Vector3.right;

        else if (rightCameraLimit.position.x >= rightShelfLimit.position.x)
            correction += Vector3.left;

        else if (topCameraLimit.position.y >= topShelfLimit.position.y)
            correction += Vector3.down;

        else if (bottomCameraLimit.position.y <= bottomShelfLimit.position.y)
            correction += Vector3.up;

        // Apply correction if needed
        if (correction != Vector3.zero)
        {
            currentCamera.transform.position += correction * transitionSpeed * Time.deltaTime;
        }
    }


    private void Transition(Direction direction)
    {
        StartCoroutine(Transitioning(direction));
    }


    private IEnumerator Transitioning(Direction direction)
    {
        // If camera at TURNTABLE, move right to SELECTION until condition suggest stopping
        // If camera at SELECTION, move left to TURNTABLE's position

        transitioning = true;

        bool leftON = false;
        bool rightInteractable = false;

        SetButtonState(leftButton, leftText, false, interaction: false);
        SetButtonState(rightButton, rightText, false, interaction: false);
        SetButtonState(bottomButton, bottomText, false, interaction: false);

        if (currentScreen == Screen.Turntable)
        {
            if (direction == Direction.Left) yield return null;
            while (leftCameraLimit.position.x <= leftShelfLimit.position.x)
            {
                // Move camera to the right
                currentCamera.transform.position += Vector3.right * transitionSpeed * Time.deltaTime;
                yield return null;
            }   

            currentScreen = Screen.Selection;
            leftON = true;
            rightInteractable = false;
        }

        else if (currentScreen == Screen.Selection)
        {
            if (direction == Direction.Left) yield return null;
            while (currentCamera.transform.position.x > turntableScreenPos.x)
            {
                // Move camera to the right
                currentCamera.transform.position += Vector3.left * transitionSpeed * Time.deltaTime;
                yield return null;  
            }

            currentScreen = Screen.Turntable;
            leftON = false;
            rightInteractable = true;
        }


        SetButtonState(leftButton, leftText, leftON, interaction: leftON);
        SetButtonState(rightButton, rightText, true, interaction: rightInteractable);
        if (GameManager.Instance.CurrentVinylRecord != null)
            SetButtonState(bottomButton, bottomText, true, interaction: true);

        transitioning = false;
    }


    private void SetButtonState(Button button, TextMeshProUGUI text, bool isVisible, bool interaction = true)
    {
        if (button == null) return;

        button.interactable = interaction && isVisible;

        Opacity opacity = isVisible ? Opacity.Show : Opacity.Hide;
        SetOpacity(button, text, opacity);
    }


    private void SetOpacity(Button button, TextMeshProUGUI text, Opacity opacity)
    {
        Image image = button.GetComponent<Image>();
        if (image == null || text == null) return;

        float target = opacity == Opacity.Show ? dimTextAlpha : 0f;

        Color tempButtonColor = image.color;
        tempButtonColor.a = target;
        image.color = tempButtonColor;

        Color tempTextColor = text.color;
        tempTextColor.a = target;
        text.color = tempTextColor;
    }


    //private void Corrector()
    //{
    //    if (leftCameraLimit.position.x <= leftShelfLimit.position.x)
    //    {
    //        currentCamera.transform.position += Vector3.right * transitionSpeed * Time.deltaTime;
    //    } 
    //    else if (rightCameraLimit.position.x >= rightShelfLimit.position.x)
    //    {
    //        currentCamera.transform.position += Vector3.left * transitionSpeed * Time.deltaTime;
    //    }
    //    else if (topCameraLimit.position.y >= topShelfLimit.position.y)
    //    {
    //        currentCamera.transform.position += Vector3.down * transitionSpeed * Time.deltaTime;
    //    }
    //    else if (bottomCameraLimit.position.y <= bottomShelfLimit.position.y)
    //    {
    //        currentCamera.transform.position += Vector3.up * transitionSpeed * Time.deltaTime;  
    //    }

    //    if (leftCameraLimit.position.x > leftShelfLimit.position.x &&
    //        rightCameraLimit.position.x < rightShelfLimit.position.x &&
    //        topCameraLimit.position.y < topShelfLimit.position.y &&
    //        bottomCameraLimit.position.y > bottomShelfLimit.position.y)
    //    {
    //        canBrowse = true;
    //    }
    //    else
    //    {
    //        canBrowse = false;
    //    }
    //}


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