using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldScreenPosUpdate : MonoBehaviour
{
    private Vector3 worldPos;
    private bool worldPosSet = false;
    private RectTransform _rectTransform;

    #region Properties
    public Vector3 WorldPos
    {
        set
        {
            worldPos = value;
            worldPosSet = true;
        }
    }

    private Vector3 ScreenPos
    {
        get { return GameManager.Camera.WorldToScreenPoint(worldPos);  }
    }
    #endregion

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateRoutine());
    }

    private void OnDisable()
    {
        worldPosSet = false;
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            _rectTransform.position = ScreenPos;
            yield return new WaitForEndOfFrame();
        }
    }
}
