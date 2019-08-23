using UnityEngine;

public class SceneObject : ScriptableObject
{
    [SerializeField]
    public char assignedChar;

    [SerializeField]
    public GameObject assignedPrefab;

    [SerializeField]
    public string displayName;
}
