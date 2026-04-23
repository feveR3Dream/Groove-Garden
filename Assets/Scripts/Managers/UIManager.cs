using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get;  private set; }

    [Header("UI Elements")]
    public Button RightButton;   // Used mostly for transitioning
    public Button LeftButton;   //  and moving left <-> right
    public Button BottomButton;     // Access current vinyl record
    public Button RightRecordButton;
    public Button LeftRecordButton;
    [Space]
    public TextMeshProUGUI RightText;
    public TextMeshProUGUI LeftText;
    public TextMeshProUGUI BottomText;
    public TextMeshProUGUI RightRecordText;
    public TextMeshProUGUI LeftRecordText;
    public TextMeshProUGUI GuideText;    


    [Header("Values")]
    [SerializeField] private float transitionSpeed = 2.5f;    // Camera transition speed for transitioning


    [Header("Hover & Opacity Settings")]
    [Range(0f, 1f)] public float DimButtonAlpha = 0.25f;
    [Range(0f, 1f)] public float HighlightButtonAlpha = 0.75f;
    [Space]
    [Range(0f, 1f)] public float DimTextAlpha = 0.25f;
    [Range(0f, 1f)] public float HighlightTextAlpha = 0.75f;


    [Header("References")]
    public Transform LeftCameraLimit;       // These values must be within
    public Transform RightCameraLimit;     //  left & right & top & bottom's 
    public Transform TopCameraLimit;      //   shelf limit
    public Transform BottomCameraLimit;  //
    [Space]
    public Transform LeftShelfLimit;
    public Transform RightShelfLimit;
    public Transform TopShelfLimit;
    public Transform BottomShelfLimit;


    [Header("Scripts")]
    [SerializeField] private RecordManager recordManager;


    // References
    public Screen CurrentScreen { get; private set; } = Screen.Turntable;    // Default screen
    public Direction CurrentHoverDirection { get; private set; } = Direction.None;

    private Camera cam;

    private Dictionary<TextMeshProUGUI, Color> textColorSaver = new Dictionary<TextMeshProUGUI, Color>();


    // Values
    private bool isTransitioning = false;
    public bool IsTransitioning { get => isTransitioning; }

    [HideInInspector] public bool PlacingRecordDown = false;

    private const float dimTextAlpha = 0.25f;
    private const float highlightTextAlpha = 0.9f;

    private Vector2 turntableScreenPos;


    // Coroutines
    private Coroutine transitioningCoroutine = null;



    private void OnEnable()
    {
        RightButton?.onClick.AddListener(() => Transition(Direction.Right));
        LeftButton?.onClick.AddListener(() => Transition(Direction.Left));
        BottomButton?.onClick.AddListener(RecordManager.Instance.ToggleCover);
        LeftRecordButton?.onClick.AddListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Left));
        RightRecordButton?.onClick.AddListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Right));


        AddHoverListener(LeftButton, 
            () => { CurrentHoverDirection = Direction.Left; OnHover(LeftButton, LeftText, Color.white, highlightTextAlpha); }, 
            () => { CurrentHoverDirection = Direction.None; OffHover(LeftButton, LeftText); });
        AddHoverListener(RightButton, 
            () => { CurrentHoverDirection = Direction.Right; OnHover(RightButton, RightText, Color.white, highlightTextAlpha); }, 
            () => { CurrentHoverDirection = Direction.None; OffHover(RightButton, RightText); });
        AddHoverListener(BottomButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(BottomButton, BottomText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(BottomButton, BottomText); });
        AddHoverListener(LeftRecordButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(LeftRecordButton, LeftRecordText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(LeftRecordButton, LeftRecordText); });
        AddHoverListener(RightRecordButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(RightRecordButton, RightRecordText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(RightRecordButton, RightRecordText); });
    
    }


    private void OnDisable()
    {
        RightButton?.onClick.RemoveListener(() => Transition(Direction.Right));
        LeftButton?.onClick.RemoveListener(() => Transition(Direction.Left));
        BottomButton?.onClick.RemoveListener(RecordManager.Instance.ToggleCover);
        LeftRecordButton?.onClick.RemoveListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Left));
        RightRecordButton?.onClick.RemoveListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Right));

    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cam = Camera.main;

        SetButtonState(RightButton, RightText, true, interaction: true);
        SetButtonState(LeftButton, LeftText, false, interaction: false);
        SetButtonState(BottomButton, BottomText, false, interaction: false);
        SetButtonState(LeftRecordButton, LeftRecordText, false, interaction: false);
        SetButtonState(RightRecordButton, RightRecordText, false, interaction: false);

        SetButtonTextContent(RightText, "Press to go to vinyl selection area");
        GuideText.gameObject.SetActive(false);
    }


    private void OnHover(Button button, TextMeshProUGUI text, Color color, float unusedAlphaParam)
    {
        if (isTransitioning || !button.interactable) return;

        // Save original text color
        if (!textColorSaver.ContainsKey(text)) textColorSaver.Add(text, text.color);

        // Fade the Text
        Color tempTextColor = color;
        tempTextColor.a = HighlightTextAlpha; // Using your new Inspector variable
        text.color = tempTextColor;

        // Fade the Button Image
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color tempBgColor = buttonImage.color;
            tempBgColor.a = HighlightButtonAlpha; // Using your new Inspector variable
            buttonImage.color = tempBgColor;
        }
    }


    private void OffHover(Button button, TextMeshProUGUI text)
    {
        if (isTransitioning || !button.interactable) return;

        // Reset the Text
        if (textColorSaver.TryGetValue(text, out Color colorResult))
        {
            text.color = colorResult;
        }

        // Reset the Button Image back to the Inspector "Dim" value
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color tempBgColor = buttonImage.color;
            tempBgColor.a = DimButtonAlpha;
            buttonImage.color = tempBgColor;
        }
    }


    public void UpdateRecordCoverEquip(bool equipped)
    {
        if (equipped)
        {
            SetButtonState(BottomButton, BottomText, true, interaction: true);
        }
        else
        {
            SetButtonState(BottomButton, BottomText, false, interaction: false);
        }
    }


    public void LeftRightCoverInspectButtonEnable(bool interactable)
    {
        SetButtonState(LeftRecordButton, LeftRecordText, interactable, interaction: interactable);
        SetButtonState(RightRecordButton, RightRecordText, interactable, interaction: interactable);
    }


    public void UpdateRecordCoverHidden(bool hide)
    {
        if (hide)
        {
            SetButtonState(RightButton, RightText, false, interaction: false);
            if (CurrentScreen == Screen.Selection)
                SetButtonState(LeftButton, LeftText, false, interaction: false);
        }
        else 
        {
            SetButtonState(RightButton, RightText, true, interaction: true);
            if (CurrentScreen == Screen.Selection)
                SetButtonState(LeftButton, LeftText, true, interaction: true);
        }
    }


    private void Transition(Direction direction)
    {
        if (transitioningCoroutine != null)
            StopCoroutine(transitioningCoroutine);

        transitioningCoroutine = StartCoroutine(Transitioning(direction));
    }


    private IEnumerator Transitioning(Direction direction)
    {
        // If camera at TURNTABLE, move right to SELECTION until condition suggest stopping
        // If camera at SELECTION, move left to TURNTABLE's position

        RecordManager.Instance.StopTogglingRecord();

        // FIX: Stop the function immediately if the direction is invalid for the current screen.
        if (CurrentScreen == Screen.Turntable && direction == Direction.Left) yield break;
        if (CurrentScreen == Screen.Selection && direction == Direction.Right) yield break;

        OffHover(LeftButton, LeftText);
        OffHover(RightButton, RightText);
        GuideText.gameObject.SetActive(false);

        isTransitioning = true; // move this to top if it doesn't work

        bool leftON = false;
        bool rightInteractable = false;
        bool transitioned = false;

        SetButtonState(LeftButton, LeftText, false, interaction: false);
        SetButtonState(RightButton, RightText, false, interaction: false);
        SetButtonState(BottomButton, BottomText, false, interaction: false);

        if (CurrentScreen == Screen.Turntable)
        {
            if (direction == Direction.Right)
            {
                while (LeftCameraLimit.position.x <= LeftShelfLimit.position.x 
                    && TopCameraLimit.position.x <= TopShelfLimit.position.x)
                {
                    // Move camera to the right
                    cam.transform.position += Vector3.right * transitionSpeed * Time.deltaTime;
                    yield return null;
                }   

                CurrentScreen = Screen.Selection;

                GuideText.gameObject.SetActive(true);

                leftON = true;
                rightInteractable = false;
                transitioned = true;
            }
        }

        else if (CurrentScreen == Screen.Selection)
        {
            if (direction == Direction.Left)
            {
                while (cam.transform.position.x > turntableScreenPos.x)
                {
                    // Move camera to the right
                    cam.transform.position += Vector3.left * transitionSpeed * Time.deltaTime;
                    yield return null;  
                }

                CurrentScreen = Screen.Turntable;

                leftON = false;
                rightInteractable = true;
                transitioned = true;
            }
        }

        if (transitioned)
        {
            SetButtonState(LeftButton, LeftText, leftON, interaction: leftON);
            SetButtonState(RightButton, RightText, true, interaction: true);
            if (RecordManager.Instance.CurrentVinylRecord != null)
                SetButtonState(BottomButton, BottomText, true, interaction: true);

            SetButtonTextContent(LeftText, "Hover here to move left\n\nPress to go back to turntable");

            if (!rightInteractable) SetButtonTextContent(RightText, "Hover here to move right");
            else SetButtonTextContent(RightText, "Press to go to vinyl selection area");

            isTransitioning = false;
        }

        transitioningCoroutine = null;
    }


    public void SetButtonTextContent(TextMeshProUGUI text, string content)
    {
        text.text = content;
    }


    private void SetButtonState(Button button, TextMeshProUGUI text, bool isVisible, bool interaction = true)
    {
        if (button == null) return;

        button.interactable = interaction && isVisible;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = isVisible;
        }

        // Handle the fading
        Opacity opacity = isVisible ? Opacity.Visible : Opacity.Transparent;
        SetOpacity(button, text, opacity);
    }


    private void SetOpacity(Button button, TextMeshProUGUI text, Opacity opacity)
    {
        Image image = button.GetComponent<Image>();
        if (image == null || text == null) return;

        // 1. Calculate the two separate targets based on your new Inspector variables
        float targetButtonAlpha = opacity == Opacity.Visible ? DimButtonAlpha : 0f;
        float targetTextAlpha = opacity == Opacity.Visible ? DimTextAlpha : 0f;

        // 2. Apply to the Button Background
        Color tempButtonColor = image.color;
        tempButtonColor.a = targetButtonAlpha;
        image.color = tempButtonColor;

        // 3. Apply to the Text
        Color tempTextColor = text.color;
        tempTextColor.a = targetTextAlpha;
        text.color = tempTextColor;
    }


    private void AddHoverListener(Button button, System.Action onEnter, System.Action onExit)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        // Pointer Entry
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };

        // FIX: Check if the button is interactable before invoking!
        enterEntry.callback.AddListener((data) =>
        {
            if (button.interactable) onEnter.Invoke();
        });

        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => onExit.Invoke());

        trigger.triggers.Add(exitEntry);
    }
}