using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    [SerializeField]
    private string _name;

    [SerializeField]
    private Transform inspectionOrigin;

    #region Properties
    public Transform InspectionOrigin { get { return inspectionOrigin; } }
    public string Name { get { return _name; } }
    public Vector3 SpawnPosition { get { return transform.position; } }
    public Quaternion SpawnRotation { get { return transform.rotation; } }
    #endregion

}
