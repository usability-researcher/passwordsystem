using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharInputPanelController : MonoBehaviour
{
    private const int AllCheckCharCount = 12;
    private const int AllowedRetries = 2;

    private static CharInputPanelController instance;

    private const string InputLimitedCharsText = "Type 1st character for {0} last seen objects ({1})";
    private const string InputAllCharsText = "Type 1st character for every seen objects (12)";

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private Text correctText;

    [SerializeField]
    private Text incorrectText;

    [SerializeField]
    private Text retryText;

    [SerializeField]
    private Text notificationText;

    [SerializeField]
    private Text titleText;

    [SerializeField]
    private InputField inputField;

    private bool wasLastQuizCorrect = false;
    private bool waitingForInput = false;
    private uint tries = 0;

    private bool allowRetryForever = false;

    #region Properties
    public static bool WasLastQuizCorrect { get { return instance.wasLastQuizCorrect; } }
    
    private uint TriesLeft { get { return AllowedRetries - tries; } }
    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void Update()
    {
        if (waitingForInput && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown("return"))) Submit();

#if UNITY_EDITOR
        Simulate();
#endif
    }

    #endregion

#if UNITY_EDITOR

    private void Simulate()
    {
        if (waitingForInput)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.C)) SimulateCorrectQuiz();
                if (Input.GetKeyDown(KeyCode.F)) SimulateQuizFail();
                if (Input.GetKeyDown(KeyCode.R)) SimulateQuizRetry();
            }
        }
    }

    private void SimulateCorrectQuiz()
    {
        wasLastQuizCorrect = true;
        FinishWaitingForInput();
    }

    private void SimulateQuizFail()
    {
        wasLastQuizCorrect = false;
        FinishWaitingForInput();
    }

    private void SimulateQuizRetry()
    {
        wasLastQuizCorrect = false;
        RetryInput();
    }
#endif

    #region Events

    public void Submit()
    {
        StartCoroutine(SubmitRoutine());
    }

    private IEnumerator SubmitRoutine()
    {
        bool correct = TourController.TestCharInput(inputField.text.ToLower());
        wasLastQuizCorrect = correct;

        retryText.gameObject.SetActive(false);
        if (correct) correctText.gameObject.SetActive(true);
        else DisplayIncorrect();

        yield return new WaitForSeconds(2f);

        if (correct)
        {
            FinishWaitingForInput();
        }
        else
        {
            if (allowRetryForever)
            {
                RetryInput();
            }
            else
            {
                tries++;

                if (tries > AllowedRetries) FinishWaitingForInput();
                else RetryInput();
            }
        }
    }
    #endregion

    #region Public Interface

    public static void SetupForAllCharInput()
    {
        instance.allowRetryForever = false;

        instance.titleText.text = InputAllCharsText;
        instance.inputField.characterLimit = AllCheckCharCount;
        EnableInput();
    }

    public static void SetupForLimitedCharInput(int charSize)
    {
        instance.allowRetryForever = false;

        string text = string.Format(InputLimitedCharsText, charSize, charSize);

        Debug.LogFormat("SetupForLimitedCharInput charSize: {0} text: {1}", charSize, text);

        instance.titleText.text = text;
        instance.inputField.characterLimit = charSize;
        EnableInput();
    }

    public static IEnumerator WaitUntilInputEnded()
    {
        while (instance.waitingForInput)
        {
            yield return null;
        }
    }

    public static IEnumerator WaitUntilInputAccepted()
    {
        while (instance.waitingForInput)
        {
            yield return null;
        }
    }
    #endregion

    #region Private Interface

    private void FinishWaitingForInput()
    {
        instance.tries = 0;
        instance.waitingForInput = false;
        DisableInput();
    }

    private static void RetryInput()
    {
        Debug.Log("Retrying input!");

        instance.wasLastQuizCorrect = false;

        InitInput();

        instance.inputField.text = string.Empty;

        instance.retryText.gameObject.SetActive(true);
        instance.retryText.text = "Try Again! Tries Left: " + instance.TriesLeft+1;
        ForceInputSelected();

        instance.incorrectText.gameObject.SetActive(false);
        instance.notificationText.gameObject.SetActive(false);
    }

    private static void EnableInput()
    {
        instance.wasLastQuizCorrect = false;
        InitInput();
    }

    private static void InitInput()
    {
        instance.waitingForInput = true;
        instance.panel.SetActive(true);

        ForceInputSelected();
    }

    private static void ForceInputSelected()
    {
        EventSystem.current.SetSelectedGameObject(instance.inputField.gameObject, null);
        instance.inputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private static void DisableInput()
    {
        instance.panel.SetActive(false);
        instance.correctText.gameObject.SetActive(false);
        instance.incorrectText.gameObject.SetActive(false);
        instance.notificationText.gameObject.SetActive(false);
        instance.retryText.gameObject.SetActive(false);
        instance.inputField.text = string.Empty;
    }

    private void DisplayIncorrect()
    {
        if (tries >= AllowedRetries)
        {
            notificationText.gameObject.SetActive(true);
        }
        else
        {
            incorrectText.gameObject.SetActive(true);
        }
    }
    #endregion
}
