using UnityEditor;
using UnityEngine;

public static class SceneObjectMenu {

    [MenuItem("Assets/Create/Scene Objects/Scene Object")]
    public static void CreateSceneObject()
    {
        SceneObject asset = ScriptableObject.CreateInstance<SceneObject>();

        AssetDatabase.CreateAsset(asset, "Assets/NewSceneObject.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Create/Scene Objects/Alphabet Scene Objects")]
    public static void CreateAlphabetSceneObjects()
    {
        for(int i=0; i < GameManager.CharSet.Length; i++)
        {
            char letter = GameManager.CharSet[i];

            SceneObject asset = ScriptableObject.CreateInstance<SceneObject>();
            asset.assignedChar = letter;

            AssetDatabase.CreateAsset(asset, string.Format("Assets/{0}.asset", letter));
            AssetDatabase.SaveAssets();
        }
    }
}
