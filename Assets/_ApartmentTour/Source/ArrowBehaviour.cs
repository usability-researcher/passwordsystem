using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    private const float MovementSpeed = 0.33f;
    private const float MovementRange = 0.5f;
    private const float RotationTowardsPlayerSpeed = 1f;

    private float originalY;

    private void OnEnable()
    {
        originalY = transform.position.y;
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        originalY = transform.position.y;

        while (true)
        {
            float y = Mathf.PingPong(Time.time * MovementSpeed, MovementRange) + originalY;
            Vector3 newPos = new Vector3(transform.position.x, y, transform.position.z);
            transform.position = newPos;
            yield return new WaitForEndOfFrame();
        }
    }

}
