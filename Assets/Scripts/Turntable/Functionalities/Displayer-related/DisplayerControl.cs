using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayerControl : MonoBehaviour
{

    [Header("Play & Pause Toggle Reference")]
    [SerializeField] private TextMeshProUGUI toggleText;


    [Header("Track & Power Reference")]
    [SerializeField] private TextMeshProUGUI recordNameAndPowerText;


    [Header("Volume Reference")]
    [SerializeField] private TextMeshProUGUI volumeText;


    [Header("RPM Reference")]
    [SerializeField] private TextMeshProUGUI rpmTitle;
    [SerializeField] private TextMeshProUGUI rpmText;


    [Header("Reverb Reference")]
    [SerializeField] private TextMeshProUGUI reverbTitle;
    [SerializeField] private TextMeshProUGUI reverbText;


    [Header("On & Off Sequence Time Values")]
    [SerializeField] private float onSequenceTimer = 2f; // 2/3  time for "---", 1/3 time to display "ON" 
    [SerializeField] private float offSequenceTimer = 1f; // Time to fade away "Off" text.


    [Header("Read Record Timer")]
    [Tooltip("How long does it take the display to read the record?")]
    [SerializeField] private float readRecordTimer = 1f;


    [Header("Helper Time Values")]
    [SerializeField] private float ellipsisUpdateTimer = 0.35f;
    [SerializeField] private float textBlinkingTimer = 0.5f;
    [SerializeField] private float textScrollingPhase1Timer = 0.5f;
    [SerializeField] private float timeBetweenPhase= 0.5f;
    [SerializeField] private float textScrollingPhase2Timer = 0.5f;


    [Header("Helper Text Display Values")]
    [Tooltip("How much char can \"nameDisplayAndPowerText\" display on the screen?")]
    [SerializeField] private int maximumDisplayedChar;

    
    // Variables
    private bool displayerOn = false;


    // General Coroutines
    private Coroutine displayerCoroutine = null;
    private Coroutine renderRecordCoroutine = null;


    // Effect Coroutines
    private Coroutine toggleBlinkingCoroutine = null;
    private Coroutine dpScrollingCoroutine = null;


    // Dictionary
    private Dictionary<TextMeshProUGUI, Coroutine> blinkingTextDictionary = new Dictionary<TextMeshProUGUI, Coroutine>();
    private Dictionary<TextMeshProUGUI, Coroutine> scrollingTextDictionary = new Dictionary<TextMeshProUGUI, Coroutine>();


    private void Awake()
    {
        ResetAllText();
    }


    private void ResetAllText()
    {
        toggleText.text = "";
        recordNameAndPowerText.text = "";
        volumeText.text = "";
        rpmTitle.text = "";
        rpmText.text = "";
        reverbTitle.text = "";
        reverbText.text = "";
    }


    public void UpdateRPMDisplay(RPM currentRPM)
    {
        rpmTitle.text = "RPM";

        if (currentRPM == RPM.Slowed) rpmText.text = "Slowed";
        if (currentRPM == RPM.Normal) rpmText.text = "Normal";
        if (currentRPM == RPM.SpedUp) rpmText.text = "Sped-up";
    }


    public void UpdatePlayPauseDisplay()
    {
        if (!TurntableManager.Instance.TurnedOn) return;

        if (!TurntableManager.Instance.TonearmOnRecord)
        {
            if (!TurntableManager.Instance.EquipRecord)
            {
                DisableTextBlinkingEffect(toggleText, Global.DisplayerTextOptions.ToggleText.EmptyTrackText);
            }
            else
            {
                DisableTextBlinkingEffect(toggleText, Global.DisplayerTextOptions.ToggleText.MissingTonearm);
            }
        }
        else
        {
            if (TurntableManager.Instance.TurntableSystem.IsPlayingMusic)
            {
                DisableTextBlinkingEffect(toggleText, Global.DisplayerTextOptions.ToggleText.Play);
            }
            else
            {
                EnableTextBlinkingEffect(toggleText, toggleBlinkingCoroutine, Global.DisplayerTextOptions.ToggleText.Pause);
            }


            Debug.Log("Tonearm on record!");
        }

        Debug.Log("Rendered");
    }


    public void UpdatePowerDisplay(bool isOn)
    {
        if (isOn) SequenceOnHandler(hyphenCount: 9);
        else SequenceOffHandler();
    }


    public void UpdateRecordDisplay(bool turntableEquipRecord)
    {
        RenderRecordName(turntableEquipRecord);
    }


    public void UpdateReverbDisplay(bool isReverb)
    {
        reverbTitle.text = "Reverb";

        if (isReverb)
        {
            reverbText.color = Color.green;
            reverbText.text = "On";
        }
        else
        {
            reverbText.color = Color.red;
            reverbText.text = "Off";
        }
    }


    public void UpdateVolumeDisplay(float rawVolume)
    {
        int convertedVolume = (int)Mathf.Lerp(0, 100, rawVolume);

        volumeText.text = "VOL: " + convertedVolume;
    }


    private void RenderRecordName(bool hasRecord)
    {
        if (renderRecordCoroutine != null)
        {
            StopCoroutine(renderRecordCoroutine);
            renderRecordCoroutine = null;
        }

        renderRecordCoroutine = StartCoroutine(RenderingRecordName(hasRecord));
    }
    private IEnumerator RenderingRecordName(bool hasRecord)
    {

        if (!hasRecord)
        {
            toggleText.text = Global.DisplayerTextOptions.ToggleText.EmptyTrackText;
            DisableTextScrollingEffect(recordNameAndPowerText, Global.DisplayerTextOptions.DisplayAndPowerText.EmptyTrackText);

            yield break;
        }

        toggleText.text = Global.DisplayerTextOptions.ToggleText.ReadingRecord;

        float totalElapsedTime = 0f;
        float animationTimer = 0f;
        int currentEllipsisAmount = 0;
        int maxEllipsisAmount = 3;

        while (totalElapsedTime < readRecordTimer)
        {
            totalElapsedTime += Time.deltaTime;
            animationTimer += Time.deltaTime;

            if (animationTimer >= ellipsisUpdateTimer)
            {
                animationTimer = 0f;
                currentEllipsisAmount++;

                if (currentEllipsisAmount > maxEllipsisAmount) currentEllipsisAmount = 0;

                string ellipsis = RepeatTextGenerator(currentEllipsisAmount, textToRepeat: ".");
                recordNameAndPowerText.text = "Reading" + ellipsis;
            }

            yield return null;
        }

        UpdatePlayPauseDisplay();

        TurntableManager.Instance.RecordRead = true;

        string recordNameAndArtist = RecordManager.Instance.CurrentVinylRecord.RecordName + " - " + RecordManager.Instance.CurrentVinylRecord.ArtistName;
        EnableTextScrollEffect(recordNameAndPowerText, recordNameAndArtist, maximumDisplayedChar);
    }


    // If turntable is suddenly turned off
    private void StopRenderRecordName()
    {
        if (renderRecordCoroutine != null)
        {
            StopCoroutine(renderRecordCoroutine);
            renderRecordCoroutine = null;
        }
    }


    #region Helper Functions
    //----------------------
    private string RepeatTextGenerator(int amount, string textToRepeat)
    {
        var tempString = new TextMeshProUGUI();
        tempString.text = "";

        for (int i = 0; i < amount; i++)
        {
            tempString.text += textToRepeat;
        }

        return tempString.text;
    }


    private void EnableTextScrollEffect(TextMeshProUGUI textRef, string setText, int maxDisplayedChar)
    {
        // 1. Check if it's already running.
        if (scrollingTextDictionary.ContainsKey(textRef)) return;

        // 2. Start the coroutine using the dictionary. 
        // (Remember: No passing empty coroutine variables as parameters!)
        textRef.text = setText;
        Coroutine newRoutine = StartCoroutine(InitializingScrollingEffect(textRef, setText, maxDisplayedChar));
        scrollingTextDictionary.Add(textRef, newRoutine);
    }

    private IEnumerator InitializingScrollingEffect(TextMeshProUGUI textRef, string originalText, int maxDisplayedChar)
    {
        // Pre-effect check: If it fits, we sit. No scrolling needed.
        if (originalText.Length <= maxDisplayedChar)
        {
            textRef.text = originalText;
            yield break;
        }

        while (textRef != null)
        {
            // Reset the text at the start of the sequence
            string currentText = originalText;
            textRef.text = currentText;


            yield return new WaitForSeconds(timeBetweenPhase);


            // Phase 1
            while (currentText.Length > maxDisplayedChar)
            {
                currentText = currentText.Remove(0, 1);
                textRef.text = currentText;
                yield return new WaitForSeconds(textScrollingPhase1Timer);
            }

            yield return new WaitForSeconds(timeBetweenPhase);
            

            // Phase 2
            while (currentText.Length > 0)
            {
                currentText = currentText.Remove(0, 1);
                textRef.text = currentText;
                yield return new WaitForSeconds(textScrollingPhase2Timer);
            }

            
            // Phase 3
            string spaces = new string(' ', 12);
            currentText = spaces + originalText;
            textRef.text = currentText;


            for (int i = 0; i < 12; i++)
            {
                currentText = currentText.Remove(0, 1);
                textRef.text = currentText;
                yield return new WaitForSeconds(textScrollingPhase2Timer);
            }
        }
    }


    private void DisableTextScrollingEffect(TextMeshProUGUI textRef, string resetText)
    {
        if (scrollingTextDictionary.TryGetValue(textRef, out Coroutine coroutineRef))
        {
            if (coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
            }
            scrollingTextDictionary.Remove(textRef);
        }

        textRef.enabled = true;
        textRef.text = resetText;
    }



    private void EnableTextBlinkingEffect(TextMeshProUGUI textRef, Coroutine coroutineRef, string setText)
    {
        if (blinkingTextDictionary.ContainsKey(textRef)) return;
        
        textRef.text = setText;
        Coroutine newCoroutine = StartCoroutine(InitiatingBlinkingEffect(textRef));

        blinkingTextDictionary.Add(textRef, newCoroutine);
        
    }
    private IEnumerator InitiatingBlinkingEffect(TextMeshProUGUI textRef)
    {
        while (textRef != null)
        {
            yield return new WaitForSeconds(textBlinkingTimer);
            textRef.enabled = !textRef.enabled;
        }
    }


    private void DisableTextBlinkingEffect(TextMeshProUGUI textRef, string resetText)
    {
        if (blinkingTextDictionary.TryGetValue(textRef, out Coroutine coroutineRef))
        {
            if (coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
                coroutineRef = null;
            }

            blinkingTextDictionary.Remove(textRef);    
        }

        textRef.enabled = true;
        textRef.text = resetText;
    }


    //--------
    #endregion


    private void SequenceOnHandler(int hyphenCount = 0)
    {
        if (!displayerOn) 
        { 
            displayerOn = true; 

            if (displayerCoroutine != null)
            {
                StopCoroutine(displayerCoroutine);
                displayerCoroutine = null;
            }

            displayerCoroutine = StartCoroutine(HandlingSequenceOn(hyphenCount));
        }
    }
    private IEnumerator HandlingSequenceOn(int hyphenCount = 0)
    {
        Color textColor = recordNameAndPowerText.color;
        textColor.a = 1;

        recordNameAndPowerText.text = "";
        recordNameAndPowerText.color = textColor;

        float hyphenSpawnTimer = onSequenceTimer * (2f / 3f);
        float onTextSpawnTimer = onSequenceTimer - hyphenSpawnTimer;

        int convertedHyphenCount = Mathf.Abs(hyphenCount); // Can't be negative
        int currentHyphenCount = 0;

        float timePerHyphen = hyphenSpawnTimer / convertedHyphenCount;
        
        while (currentHyphenCount < convertedHyphenCount)
        {
            recordNameAndPowerText.text += "-";
            currentHyphenCount++;
            yield return new WaitForSeconds(timePerHyphen);
        }

        recordNameAndPowerText.text = "On";
        yield return new WaitForSeconds(onTextSpawnTimer);

        TurntableManager.Instance.TurnedOn = true; // Moved from PowerController.cs

        KnobManager.Instance.PowerControl.UpdatePowerStatus();

        displayerCoroutine = null;
    }


    private void SequenceOffHandler()
    {
        if (displayerOn)
        {
            displayerOn = false;

            StopRenderRecordName();

            if (displayerCoroutine != null)
            {
                StopCoroutine(displayerCoroutine);
                displayerCoroutine = null;
            }

            displayerCoroutine = StartCoroutine(HandlingSequenceOff());
        }
    }
    private IEnumerator HandlingSequenceOff()
    {
        ResetAllText();
        DisableTextScrollingEffect(recordNameAndPowerText, Global.DisplayerTextOptions.DisplayAndPowerText.PowerOff);
        DisableTextBlinkingEffect(toggleText, "");

        TurntableManager.Instance.RecordRead = false;

        float currentTimer = 0f;
        Color textColor = recordNameAndPowerText.color;

        float startAlpha = textColor.a;

        while (currentTimer < offSequenceTimer)
        {
            currentTimer += Time.deltaTime;

            float percent = currentTimer / offSequenceTimer;

            textColor.a = Mathf.Lerp(startAlpha, 0f, percent);
            recordNameAndPowerText.color = textColor;

            yield return null;
        }

        textColor.a = 0f;
        recordNameAndPowerText.color = textColor;

        recordNameAndPowerText.text = "";

        textColor.a = 1f;
        recordNameAndPowerText.color = textColor;

        displayerCoroutine = null;
    }
}
