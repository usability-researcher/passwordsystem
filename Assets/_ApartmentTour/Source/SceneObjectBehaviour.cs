using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectBehaviour : MonoBehaviour
{
    private const float TolerableDistnce = 0.001f;

    private const float InspectionPlacementDragTime = 2f;
    private const float Speed = 20f;

    private bool inspecting = false;
    private SceneObject sceneObject;

    [SerializeField]
    private float inspectionDistanceFromCamera = 0.35f;

    [SerializeField]
    private float placementOffset = 0f;

    #region Cache
    private Camera _camera;
    private Vector3 _originalPlacement;
    private Vector3 _lastCameraPos;
    private Vector3 _inspectionPlacement;
    private PointOfInterest _pointOfInterest;
    private Quaternion _lookRotation;
    #endregion

    #region Properties
    public string DisplayName
    {
        get { return SceneObject.displayName; }
    }

    public SceneObject SceneObject
    {
        get { return sceneObject; }
        set { sceneObject = value; }
    }

    private Vector3 InspectionPlacement
    {
        get
        {
            _lastCameraPos = _camera.transform.position;
            Vector3 forwardDistance = _camera.transform.forward * inspectionDistanceFromCamera;
            _inspectionPlacement = _camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, _camera.nearClipPlane)) + forwardDistance;

            return _inspectionPlacement;
        }
    }
    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        _camera = Camera.main;
        _originalPlacement = transform.position;

        StartCoroutine(Inspect());
    }
    #endregion

    public void HandlePlacement(PointOfInterest pointOfInterest)
    {
        _pointOfInterest = pointOfInterest;

        transform.position += Vector3.up * placementOffset;

        CalculateRotationTowardsInspection();
        ResetRotation();
    }

    public void StartInspection()
    {
        inspecting = true;
    }

    public void StopInspection()
    {
        inspecting = false;
    }

    public void QueueResetRotation(float time)
    {
        StartCoroutine(QueueResetRotationRoutine(time));
    }

    private IEnumerator QueueResetRotationRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        ResetRotation();
    }

    private void ResetRotation()
    {
        transform.localRotation = _lookRotation;
    }

    private IEnumerator Inspect()
    {
        while (true)
        {
            Vector3 targetPos = inspecting ? InspectionPlacement : _originalPlacement;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * InspectionPlacementDragTime);

            if (inspecting)
            {
                if (Vector3.Distance(transform.position, targetPos) < TolerableDistnce) transform.Rotate(0, Speed * Time.deltaTime, 0);
            }
            yield return null;
        }
    }

    private void CalculateRotationTowardsInspection()
    {
        Transform target = _pointOfInterest.InspectionOrigin;

        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;

        Quaternion q = Quaternion.LookRotation(lookPos, Vector3.up);
        _lookRotation = Quaternion.Euler(0, q.eulerAngles.y - 90, 0);
    }
}
