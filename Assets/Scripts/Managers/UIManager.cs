using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get;  private set; }

    [Header("UI Elements")]
    public Button rightButton;   // Used mostly for transitioning
    public Button leftButton;   //  and moving left <-> right
    public Button bottomButton;     // Access current vinyl record
    public Button rightRecordButton;
    public Button leftRecordButton;
    [Space]
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI bottomText;
    public TextMeshProUGUI rightRecordText;
    public TextMeshProUGUI leftRecordText;
    public TextMeshProUGUI guideText;    


    [Header("Values")]
    [SerializeField] private float transitionSpeed = 2.5f;    // Camera transition speed for transitioning


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
    public bool IsTransitioning { get; private set; }

    private const float dimTextAlpha = 0.25f;
    private const float highlightTextAlpha = 0.5f;

    private Vector2 turntableScreenPos;


    // Coroutines
    private Coroutine transitioningCoroutine = null;



    private void OnEnable()
    {
        rightButton?.onClick.AddListener(() => Transition(Direction.Right));
        leftButton?.onClick.AddListener(() => Transition(Direction.Left));
        bottomButton?.onClick.AddListener(RecordManager.Instance.ToggleCover);
        leftRecordButton?.onClick.AddListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Left));
        rightRecordButton?.onClick.AddListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Right));


        AddHoverListener(leftButton, 
            () => { CurrentHoverDirection = Direction.Left; OnHover(leftButton, leftText, Color.white, highlightTextAlpha); }, 
            () => { CurrentHoverDirection = Direction.None; OffHover(leftButton, leftText); });
        AddHoverListener(rightButton, 
            () => { CurrentHoverDirection = Direction.Right; OnHover(rightButton, rightText, Color.white, highlightTextAlpha); }, 
            () => { CurrentHoverDirection = Direction.None; OffHover(rightButton, rightText); });
        AddHoverListener(bottomButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(bottomButton, bottomText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(bottomButton, bottomText); });
        AddHoverListener(leftRecordButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(leftRecordButton, leftRecordText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(leftRecordButton, leftRecordText); });
        AddHoverListener(rightRecordButton,
    () => { CurrentHoverDirection = Direction.None; OnHover(rightRecordButton, rightRecordText, Color.white, highlightTextAlpha); },
    () => { CurrentHoverDirection = Direction.None; OffHover(rightRecordButton, rightRecordText); });
    
    }


    private void OnDisable()
    {
        rightButton?.onClick.RemoveListener(() => Transition(Direction.Right));
        leftButton?.onClick.RemoveListener(() => Transition(Direction.Left));
        bottomButton?.onClick.RemoveListener(RecordManager.Instance.ToggleCover);
        leftRecordButton?.onClick.RemoveListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Left));
        rightRecordButton?.onClick.RemoveListener(() => RecordManager.Instance.RecordCoverTransition(Direction.Right));

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

        SetButtonState(rightButton, rightText, true, interaction: true);
        SetButtonState(leftButton, leftText, false, interaction: false);
        SetButtonState(bottomButton, bottomText, false, interaction: false);
        SetButtonState(leftRecordButton, leftRecordText, false, interaction: false);
        SetButtonState(rightRecordButton, rightRecordText, false, interaction: false);

        SetButtonTextContent(rightText, "Press to go to vinyl selection area");
        guideText.gameObject.SetActive(false);
    }


    private void OnHover(Button button, TextMeshProUGUI text, Color color, float alpha)
    {
        if (IsTransitioning || !button.interactable) return;
        if (!textColorSaver.ContainsKey(text)) textColorSaver.Add(text, text.color);

        float clampedAlpha = Mathf.Clamp01(alpha);
        Color tempColor = color;
        tempColor.a = clampedAlpha;

        text.color = tempColor;
    }


    private void OffHover(Button button, TextMeshProUGUI text)
    {
        if (IsTransitioning || !button.interactable) return;
        if (textColorSaver.TryGetValue(text, out Color colorResult))
        {
            text.color = colorResult;
        }
    }


    public void UpdateRecordCoverEquip(bool equipped)
    {
        if (equipped)
        {
            SetButtonState(bottomButton, bottomText, true, interaction: true);
        }
        else
        {
            SetButtonState(bottomButton, bottomText, false, interaction: false);
        }
    }


    public void UpdateRecordInteraction(bool interactable)
    {
        SetButtonState(leftRecordButton, leftRecordText, interactable, interaction: interactable);
        SetButtonState(rightRecordButton, rightRecordText, interactable, interaction: interactable);
    }


    public void UpdateRecordCoverHidden(bool hide)
    {
        if (hide)
        {
            Debug.Log("Not Hidden");
            SetButtonState(rightButton, rightText, false, interaction: false);
            if (CurrentScreen == Screen.Selection)
                SetButtonState(leftButton, leftText, false, interaction: false);
        }
        else 
        {
            Debug.Log("Hidden");
            SetButtonState(rightButton, rightText, true, interaction: true);
            if (CurrentScreen == Screen.Selection)
                SetButtonState(leftButton, leftText, true, interaction: true);
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

        // FIX: Stop the function immediately if the direction is invalid for the current screen.
        if (CurrentScreen == Screen.Turntable && direction == Direction.Left) yield break;
        if (CurrentScreen == Screen.Selection && direction == Direction.Right) yield break;

        OffHover(leftButton, leftText);
        OffHover(rightButton, rightText);
        guideText.gameObject.SetActive(false);

        IsTransitioning = true; // move this to top if it doesn't work

        bool leftON = false;
        bool rightInteractable = false;
        bool transitioned = false;

        SetButtonState(leftButton, leftText, false, interaction: false);
        SetButtonState(rightButton, rightText, false, interaction: false);
        SetButtonState(bottomButton, bottomText, false, interaction: false);

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

                guideText.gameObject.SetActive(true);

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
            SetButtonState(leftButton, leftText, leftON, interaction: leftON);
            SetButtonState(rightButton, rightText, true, interaction: true);
            if (RecordManager.Instance.CurrentVinylRecord != null)
                SetButtonState(bottomButton, bottomText, true, interaction: true);

            SetButtonTextContent(leftText, "Hover here to move left\n\nPress to go back to turntable");

            if (!rightInteractable) SetButtonTextContent(rightText, "Hover here to move right");
            else SetButtonTextContent(rightText, "Press to go to vinyl selection area");

            IsTransitioning = false;
        }

        transitioningCoroutine = null;
    }


    private void SetButtonTextContent(TextMeshProUGUI text, string content)
    {
        text.text = content;
    }


    private void SetButtonState(Button button, TextMeshProUGUI text, bool isVisible, bool interaction = true)
    {
        if (button == null) return;

        button.interactable = interaction && isVisible;

        Opacity opacity = isVisible ? Opacity.Visible : Opacity.Transparent;
        SetOpacity(button, text, opacity);
    }


    private void SetOpacity(Button button, TextMeshProUGUI text, Opacity opacity)
    {
        Image image = button.GetComponent<Image>();
        if (image == null || text == null) return;

        float target = opacity == Opacity.Visible ? dimTextAlpha : 0f;

        Color tempButtonColor = image.color;
        tempButtonColor.a = target;
        image.color = tempButtonColor;

        Color tempTextColor = text.color;
        tempTextColor.a = target;
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