using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const string TourGameManagerReady = "TourGameManagerReady";
    private const string TourDone = "TourDone";

    private const string FirstLetterDecoration = "<b><color=red>{0}</color></b>";

    private const float PauseTimeScale = 0.0001f;

    private static GameManager instance;

    public static readonly char[] CharSet = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };


    [SerializeField]
    private int sceneToReloadIndex = 0;

#if DEBUG_GAMEMANAGER
    [SerializeField]
    private string overridenCharSet = "aaaaaaaaaaaa";
#else
    private string overridenCharSet = null;
#endif

    [SerializeField]
    private Text arrowText;

    [SerializeField]
    private ArrowBehaviour arrow;

    [SerializeField]
    private PointOfInterest[] tourPointsOfInterest;

    [SerializeField]
    private SceneObject[] sceneObjects;

    [SerializeField]
    private Text sceneObjectInspectionText;

    [SerializeField]
    private Text pauseText;

    private string loadedCharSet;
    private Dictionary<char, SceneObject> sceneObjectMap = new Dictionary<char, SceneObject>();
    private List<SceneObjectBehaviour> placedSceneObjects = new List<SceneObjectBehaviour>();
    private bool hasPlacedSceneObjects = false;

    #region Properties
    public static bool IsLoaded { get { return instance != null; } }
    public static bool HasPlacedSceneObjects { get { return instance.hasPlacedSceneObjects; } }

    private bool IsPause
    {
        get { return Time.timeScale <= PauseTimeScale; }
        set
        {
            if (value) Time.timeScale = PauseTimeScale;
            else Time.timeScale = 1f;
            pauseText.gameObject.SetActive(value);
        }
    }

    public static Camera Camera { get { return Camera.main; } }
    public static Text ArrowText { get { return instance.arrowText; } }
    public static ArrowBehaviour Arrow { get { return instance.arrow; } }
    public static PointOfInterest[] TourPointsOfInterest { get { return instance.tourPointsOfInterest; } }

    public static string LoadedCharSet
    {
        get { return instance.loadedCharSet; }
        set
        {
            instance.loadedCharSet = value;
            instance.PlaceSceneObjects();
        }
    }
    #endregion

#if DEBUG_FINISH_URI
    private IEnumerator Exp()
    {
        yield return new WaitForSeconds(3f);

        TriggerEnd();
    }
#endif

    #region MonoBehaviour
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance.gameObject);
    }

    private void Start()
    {
        PopulateSceneObjectMap();

        Application.ExternalCall(TourGameManagerReady);

        if (LoadedCharSet == null)
        {
            if (overridenCharSet == null || overridenCharSet == string.Empty) LoadedCharSet = RandomCharSet();
            else LoadedCharSet = overridenCharSet;
        }

#if DEBUG_FINISH_URI
        StartCoroutine(Exp());
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) IsPause = !IsPause;
    }

    #endregion

    #region Public Interface

    public static void TriggerEnd()
    {
        Debug.Log("<color=cyan> Trigerred end </color>");
        Application.ExternalCall(TourDone);
    }

    public void OverrideSceneObjectString(string val)
    {
        LoadedCharSet = val;
    }

    public static void Reload()
    {
        SceneManager.LoadScene(instance.sceneToReloadIndex);
    }

    public static void ActivateAllSceneObjects()
    {
#if DEBUG_GAMEMANAGER
        Debug.Log("ActiveAllSceneObjects for " + instance.placedSceneObjects.Count + " objecs!");
#endif

        for (int i = 0; i < instance.placedSceneObjects.Count; i++)
        {
            GameObject placedSceneObject = instance.placedSceneObjects[i].gameObject;
            placedSceneObject.SetActive(true);
        }
    }

    public static void InspectObjectAtIndex(int currentPointOfInterestIndex)
    {
        SceneObjectBehaviour sceneObjectBehaviour = instance.placedSceneObjects[currentPointOfInterestIndex];
        sceneObjectBehaviour.StartInspection();
        instance.sceneObjectInspectionText.text = instance.GetSceneObjectBehaviourDisplayName(sceneObjectBehaviour);
    }

    public static void UninspectObjectAtIndex(int currentPointOfInterestIndex)
    {
        SceneObjectBehaviour sceneObjectBehaviour = instance.placedSceneObjects[currentPointOfInterestIndex];
        sceneObjectBehaviour.StopInspection();
        instance.sceneObjectInspectionText.text = string.Empty;
    }

    public static void ResetObjectAtIndex(int currentPointOfInterestIndex, float time = 0f)
    {
        SceneObjectBehaviour sceneObjectBehaviour = instance.placedSceneObjects[currentPointOfInterestIndex];
        sceneObjectBehaviour.QueueResetRotation(time);
    }
    #endregion

    #region Private Interface

    private string GetSceneObjectBehaviourDisplayName(SceneObjectBehaviour sceneObjectBehaviour)
    {
        string displayName = sceneObjectBehaviour.DisplayName;
        string firstLetter = displayName.Substring(0, 1);

        string firstPart = string.Format(FirstLetterDecoration, firstLetter);
        string secondPart = displayName.Substring(1);

        return firstPart + secondPart;
    }

    private string RandomCharSet()
    {
        StringBuilder strBuilder = new StringBuilder();
        int[] indices = Sacristan.Utils.RandomUtils.GenerateNonRepeatingRandomIndexSequence(12, 0, sceneObjects.Length);

        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            strBuilder.Append(CharSet[index]);
        }

        return strBuilder.ToString();
    }

    private void PlaceSceneObjects()
    {
        Debug.Log("GameManager: Placing scene objects from string " + LoadedCharSet);

        for (int i = 0; i < LoadedCharSet.Length; i++)
        {
            char ch = LoadedCharSet[i];
            SceneObject sceneObject = sceneObjectMap[ch];
            PlaceSceneObjectAtPointOfInterestIndex(sceneObject, i);
        }

        hasPlacedSceneObjects = true;
    }

    private void PlaceSceneObjectAtPointOfInterestIndex(SceneObject sceneObject, int pointOfInterestIndex)
    {
        PointOfInterest pointOfInterest = tourPointsOfInterest[pointOfInterestIndex];
        GameObject spawnedGameObject = Instantiate(sceneObject.assignedPrefab, pointOfInterest.SpawnPosition, pointOfInterest.SpawnRotation);
        SceneObjectBehaviour sceneObjectBehaviour = spawnedGameObject.GetComponent<SceneObjectBehaviour>();
        sceneObjectBehaviour.SceneObject = sceneObject;
        sceneObjectBehaviour.HandlePlacement(pointOfInterest);

        spawnedGameObject.SetActive(false);
        placedSceneObjects.Add(sceneObjectBehaviour);
    }

    /// <summary>
    /// Create a dictionary map to cache mapping
    /// </summary>
    private void PopulateSceneObjectMap()
    {
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            SceneObject sceneObject = sceneObjects[i];

            char key = sceneObject.assignedChar;

            if (sceneObjectMap.ContainsKey(key)) Debug.LogError("SceneObjectMap already contains key: " + key + ". Make sure that configuration chars are unique");
            else sceneObjectMap.Add(key, sceneObject);
        }
    }
    #endregion
}
