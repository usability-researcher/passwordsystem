//#define DEBUG_TOUR_CONTROLLER

using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourController : MonoBehaviour
{
    private enum TourStatus { None, Tour_1, Tour_2, Tour_3, Done }

    private static TourController instance;

    private const string SpeedFieldName = "speed";
    private const string DelayFieldName = "delay";

#if DEBUG_TOUR_CONTROLLER
    private const float DefaultSpeed = 7.5f;
    private const float DefaultDelay = 2f;
    private const float Tour23Delay = 3f;
#else
    private const float DefaultSpeed = 2.5f;
    private const float DefaultDelay = 7f;
    private const float Tour23Delay = 8f;
#endif
    private const float UninspectWaitTime = 0.8f;

    private static readonly List<int> IllegalIndices = new List<int>() { 21 };

    private static readonly int[] Indices = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
    private static readonly List<int> PointOfInterestIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 7, 10, 11, 13, 16, 17, 20 };
    private static readonly List<int> NonDelayIndices = new List<int>() { 0, 6, 8, 9, 12, 14, 15, 18, 19 };

    private static readonly float[] DistancesTour2 = new float[]
{
        0f,
        0.1951644f,
        0.3282736f,
        0.5160415f,
        1f
};

    private static readonly float[] DistancesTour3 = new float[]
    {
        0f,
        0.3282736f,
        1f
    };

    private static readonly int[] QuizRestartIndicesTour2 = new int[] { 0, 3, 7, 13 };
    private static readonly int[] QuizRestartIndicesTour3 = new int[] { 0, 7 };

    private float[] Distances
    {
        get
        {
            switch (tourStatus)
            {
                case TourStatus.Tour_2:
                    return DistancesTour2;
                case TourStatus.Tour_3:
                    return DistancesTour3;
                default:
                    return new float[0];

            }
        }
    }

    private int[] QuizRestartIndices
    {
        get
        {
            switch (tourStatus)
            {
                case TourStatus.Tour_2:
                    return QuizRestartIndicesTour2;
                case TourStatus.Tour_3:
                    return QuizRestartIndicesTour3;
                default:
                    return new int[0];

            }
        }
    }

    private float PathProgress
    {
        get { return bGCcCursorChangeLinear.Cursor.DistanceRatio; }
        set { bGCcCursorChangeLinear.Cursor.DistanceRatio = value; }
    }

    [SerializeField]
    private BGCcCursorChangeLinear bGCcCursorChangeLinear;

#if DEBUG_TOUR_CONTROLLER
    [SerializeField]
#endif
    private TourStatus tourStatus = TourStatus.Tour_1;
    private int currentPointOfInterestIndex = 0;
    private int pointReached = 0;
    private int quizPoint = 0;
    private bool wasCorrect;

    private int QuizFrequency
    {
        get
        {
            switch (tourStatus)
            {
                case TourStatus.Tour_2:
                    return 3;
                case TourStatus.Tour_3:
                    return 6;
                default:
                    return 0;
            }
        }
    }

    public void OnSplinePointReached()
    {
        pointReached++;
        Debug.Log("<color=blue> OnSplinePointReached </color>" + pointReached);

        if (IllegalIndices.Contains(pointReached))
        {
            Debug.LogErrorFormat("Received illegal point index: {0}", pointReached);
        }

        if (!NonDelayIndices.Contains(pointReached)) StartCoroutine(HandleSplinePoint());
    }

    private void ResetToPreviousSequence()
    {
        Debug.Log("Resetting To Prev Sequence");
        currentPointOfInterestIndex -= QuizFrequency - 1;
        quizPoint--;

        pointReached = QuizRestartIndices[quizPoint];
        Debug.LogFormat("Reseting currentPointOfInterestIndex: {0} pointReached: {1}", currentPointOfInterestIndex, pointReached);

        float sectionDistance = GetSectionDistance(quizPoint);

        Debug.Log("Calculated section distance: " + sectionDistance);
        PathProgress = sectionDistance;

        bGCcCursorChangeLinear.Stopped = false;
    }

    private void RestartPath()
    {
        PathProgress = 0f;
    }


    #region Properties
    private BGCurve BGCurve
    {
        get
        {
            return bGCcCursorChangeLinear.Curve;
        }
    }

    private PointOfInterest CurrentPointOfInterest
    {
        get
        {
            return GameManager.TourPointsOfInterest[currentPointOfInterestIndex];
        }
    }

    private Vector3 CurrentPointOfInterestPosition
    {
        get
        {
            return CurrentPointOfInterest.transform.position;
        }
    }

    private Vector3 MovementPosition
    {
        get
        {
            Vector3 movementPos = Vector3.zero;
            return new Vector3(movementPos.x, transform.position.y, movementPos.z);
        }
    }

    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
    }

    private void Start()
    {
        UpdatePointDelays();
        StartCoroutine(TourStateMachine());
        ContinueMovement();
    }
    #endregion

    #region State Machine

    private IEnumerator TourStateMachine()
    {
        while (!GameManager.HasPlacedSceneObjects) yield return null;

        TourStatus lastKnownTourStatus = TourStatus.None;

        while (true)
        {
            if (lastKnownTourStatus != tourStatus)
            {
                switch (tourStatus)
                {
                    case TourStatus.Tour_2:
                    case TourStatus.Tour_3:
                        GameManager.ActivateAllSceneObjects();
                        break;
                    case TourStatus.Done:
                        StopMovement();
                        CharInputPanelController.SetupForAllCharInput();
                        StartCoroutine(WaitUntilFinalQuizAnsweredAndHandleLogic());
                        break;
                }
#if DEBUG_TOUR_CONTROLLER
                Debug.Log(string.Format("TourStateMachine state changed from {0} to {1}", lastKnownTourStatus, tourStatus));
#endif
                lastKnownTourStatus = tourStatus;
            }

            yield return null;
        }
    }

    #endregion

    #region Public Interface: Char Tests

    public static bool TestCharInput(string input)
    {
        if (instance.tourStatus == TourStatus.Done) return TestCharAllInput(input);
        else return TestCharLimitedInput(input);
    }

    private static bool TestCharLimitedInput(string input)
    {
        string testString = GameManager.LoadedCharSet;

        testString = testString.Substring(instance.currentPointOfInterestIndex - (instance.QuizFrequency - 1), instance.QuizFrequency);
        return input == testString;
    }

    private static bool TestCharAllInput(string input)
    {
        return input == GameManager.LoadedCharSet;
    }
    #endregion

    #region Movement speed handles between points

    private IEnumerator WaitUntilFinalQuizAnsweredAndHandleLogic()
    {
        yield return CharInputPanelController.WaitUntilInputAccepted();

        if (CharInputPanelController.WasLastQuizCorrect)
        {
            Debug.Log("Final all char quiz succeeeded. Triggering end...");
            GameManager.TriggerEnd();
        }
        else
        {
            Debug.Log("Final all char quiz failed. Restarting Tour...");

            tourStatus = TourStatus.Tour_3;

            ResetTourVariables();

            PathProgress = 0f;
            ContinueMovement();
        }
        //GameManager.Reload();
    }

    private IEnumerator HandleSplinePoint()
    {
        StopMovement();
        yield return HandleAtPoint();
        yield return WaitForCharInputIfRequired();

        bool canIncrementPointofIndex = true;

        if (DoRequireQuizAtThisPoint())
        {
            quizPoint++;
            canIncrementPointofIndex = HandleQuiz();
        }

        Debug.Log("HandleSplinePoint canIncrementPointofIndex: " + canIncrementPointofIndex);

        if (canIncrementPointofIndex) IncrementPointOfInterestIndex();
        if (tourStatus != TourStatus.Done) ContinueMovement();
    }

    private bool DoRequireQuizAtThisPoint()
    {
        if (QuizFrequency <= 0) return false;
        return (currentPointOfInterestIndex != 0 && (currentPointOfInterestIndex + 1) % QuizFrequency == 0);
    }

    private bool HandleQuiz()
    {
        switch (tourStatus)
        {
            case TourStatus.Tour_2:
            case TourStatus.Tour_3:
                bool result = CharInputPanelController.WasLastQuizCorrect;

                if (!result)
                {
                    ResetToPreviousSequence();
                }

                Debug.Log("Quiz correct: " + result);

                return result;
            default:
                return true;
        }

    }

    private float GetSectionDistance(int index)
    {
        return Distances[index];
    }

    private IEnumerator WaitForCharInputIfRequired()
    {
        switch (tourStatus)
        {
            case TourStatus.Tour_2:
            case TourStatus.Tour_3:
                if (DoRequireQuizAtThisPoint())
                {
                    CharInputPanelController.SetupForLimitedCharInput(QuizFrequency);
#if DEBUG_TOUR_CONTROLLER
                    Debug.Log("TourController: Waiting until input accepted");
#endif
                    //yield return CharInputPanelController.WaitUntilInputAccepted();
                    yield return CharInputPanelController.WaitUntilInputEnded();

#if DEBUG_TOUR_CONTROLLER
                    Debug.Log("TourController: Input accepted");
#endif
                }

                break;
        }

        yield return null;
    }

    private IEnumerator HandleAtPoint()
    {
        yield return HandleAtPointOfInterest();
        StartCoroutine(HandlePostAtPointOfInterest());
    }

    private void SetSpeed(float speed)
    {
        for (int i = 0; i < BGCurve.PointsCount; i++)
        {
            BGCurve[i].SetField<float>(SpeedFieldName, speed);
        }
    }

    private void UpdatePointDelays()
    {
        for (int i = 0; i < BGCurve.PointsCount; i++)
        {
            float delay = 0f;
            if (!NonDelayIndices.Contains(i))
            {
                switch (tourStatus)
                {
                    case TourStatus.Tour_1:
                        delay = DefaultDelay;
                        break;
                    case TourStatus.Tour_2:
                    case TourStatus.Tour_3:
                        delay = Tour23Delay;
                        break;
                }
            }
            BGCurve[i].SetField<float>(DelayFieldName, delay);
        }

        //BGCurve[20].SetField<float>(DelayFieldName, 0f);
    }

    #endregion

    #region Functionality Methods

    private void IncrementPointOfInterestIndex()
    {
        if (++currentPointOfInterestIndex >= GameManager.TourPointsOfInterest.Length)
        {
            switch (tourStatus)
            {
                case TourStatus.Tour_1:
#if DEBUG_TOUR_CONTROLLER
                    Debug.Log("Tour1 -> Tour2");
#endif
                    tourStatus = TourStatus.Tour_2;
                    break;
                case TourStatus.Tour_2:
#if DEBUG_TOUR_CONTROLLER
                    Debug.Log("Tour2 -> Tour3");
#endif
                    tourStatus = TourStatus.Tour_3;
                    break;
                case TourStatus.Tour_3:
#if DEBUG_TOUR_CONTROLLER
                    Debug.Log("Tour3 -> GameOver.");
#endif
                    tourStatus = TourStatus.Done;
                    StopMovement();
                    break;
            }

            if (tourStatus != TourStatus.Done)
            {
                ResetTourVariables();
                RestartPath();
            }
        }
    }

    private void ResetTourVariables()
    {
        quizPoint = 0;
        currentPointOfInterestIndex = 0;
        pointReached = 0;
    }


    #endregion

    #region Inspection logic

    private IEnumerator HandleAtPointOfInterest()
    {
        switch (tourStatus)
        {
            case TourStatus.Tour_1:
                yield return HandleAtPointOfInterest_Tour1();
                break;
            case TourStatus.Tour_2:
                yield return HandleAtPointOfInterest_Tour2();
                break;
            case TourStatus.Tour_3:
                yield return HandleAtPointOfInterest_Tour3();
                break;
        }
    }

    private IEnumerator HandlePostAtPointOfInterest()
    {
        switch (tourStatus)
        {
            case TourStatus.Tour_1:
                yield return HandlePostAtPointOfInterest_Tour1();
                break;
            case TourStatus.Tour_2:
                yield return HandlePostAtPointOfInterest_Tour2();
                break;
            case TourStatus.Tour_3:
                yield return HandlePostAtPointOfInterest_Tour3();
                break;
        }
    }

    private IEnumerator HandleAtPointOfInterest_Tour1()
    {
        SetArrow();
        yield return new WaitForSeconds(DefaultDelay);
    }

    private IEnumerator HandleAtPointOfInterest_Tour2()
    {
        GameManager.InspectObjectAtIndex(currentPointOfInterestIndex);
        yield return new WaitForSeconds(Tour23Delay - 1);
        GameManager.UninspectObjectAtIndex(currentPointOfInterestIndex);
        yield return new WaitForSeconds(UninspectWaitTime);

        GameManager.ResetObjectAtIndex(currentPointOfInterestIndex, 1f);
    }

    private IEnumerator HandleAtPointOfInterest_Tour3()
    {
        GameManager.InspectObjectAtIndex(currentPointOfInterestIndex);
        yield return new WaitForSeconds(Tour23Delay - 1);
        GameManager.UninspectObjectAtIndex(currentPointOfInterestIndex);
        yield return new WaitForSeconds(UninspectWaitTime);

        GameManager.ResetObjectAtIndex(currentPointOfInterestIndex, 1f);
    }

    private IEnumerator HandlePostAtPointOfInterest_Tour1()
    {
        UnsetArrow();
        yield return null;
    }

    private IEnumerator HandlePostAtPointOfInterest_Tour2()
    {
        yield return null;

        //throw new System.NotImplementedException();
    }

    private IEnumerator HandlePostAtPointOfInterest_Tour3()
    {
        yield return null;
        //throw new System.NotImplementedException();
    }

    #endregion

    #region Actions

    private void StopMovement()
    {
#if DEBUG_TOUR_CONTROLLER
        Debug.Log("StopMovement");
#endif
        SetSpeed(0f);
    }

    private void ContinueMovement()
    {
#if DEBUG_TOUR_CONTROLLER
        Debug.Log("ContinueMovement");
#endif
        SetSpeed(DefaultSpeed);
    }

    private void SetArrow()
    {
        Vector3 pos = CurrentPointOfInterestPosition + Vector3.up * 0.25f;
        Vector3 textPos = pos + Vector3.up * 0.85f;

        GameManager.Arrow.transform.position = pos;
        GameManager.Arrow.gameObject.SetActive(true);

        GameManager.ArrowText.text = CurrentPointOfInterest.Name;
        WorldScreenPosUpdate worldScreenPosUpdate = GameManager.ArrowText.GetComponent<WorldScreenPosUpdate>();
        worldScreenPosUpdate.WorldPos = textPos;

        GameManager.ArrowText.gameObject.SetActive(true);
    }

    private void UnsetArrow()
    {
        GameManager.Arrow.gameObject.SetActive(false);
        GameManager.ArrowText.gameObject.SetActive(false);
    }
    #endregion
}
