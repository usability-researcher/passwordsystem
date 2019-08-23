//#define DEBUG_DOOR

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshFilter))]
public class Door : MonoBehaviour {

	public enum rotOrient
	{
		Y_Axis_Up,
		Z_Axis_Up
	}

	public rotOrient rotationOrientation;
	public float doorOpenAngle = 90.0f;
	public float doorClosedAngle = 0.0f;
	[Range(1,15)] public float speed = 10.0f;

	public AudioClip doorOpenSound;
	public AudioClip doorCloseSound;

	Quaternion doorOpen = Quaternion.identity;
	Quaternion doorClosed = Quaternion.identity;

	bool doorStatus = false;
	[HideInInspector]
	public bool playerInRange = false;

	void Start() {
		if (this.gameObject.isStatic) {
#if DEBUG_DOOR
            Debug.Log ("This door has been set to static and won't be openable. Doorscript has been removed.");
#endif
            Destroy(this);
		}
		if (rotationOrientation.Equals (rotOrient.Z_Axis_Up)) {
			doorOpen = Quaternion.Euler (transform.localEulerAngles.x, transform.localEulerAngles.y, doorOpenAngle);
			doorClosed = Quaternion.Euler (transform.localEulerAngles.x, transform.localEulerAngles.y, doorClosedAngle);
		} else {
			doorOpen = Quaternion.Euler (transform.localEulerAngles.x, doorOpenAngle, transform.localEulerAngles.z);
			doorClosed = Quaternion.Euler (transform.localEulerAngles.x, doorClosedAngle, transform.localEulerAngles.z);
		}
	}

	void Update() {

		if (playerInRange) {
			if (Input.GetKeyDown (KeyCode.E)) {
				if (doorStatus) {
					StartCoroutine (this.moveDoor (doorClosed));
					if (doorCloseSound != null)					AudioSource.PlayClipAtPoint (doorCloseSound, this.transform.position);
				} else {
					StartCoroutine (this.moveDoor (doorOpen));
					if (doorOpenSound != null)					AudioSource.PlayClipAtPoint (doorOpenSound, this.transform.position);
				}
			}
		}

	}

	IEnumerator moveDoor(Quaternion target) {
		while (Quaternion.Angle (transform.localRotation, target) > 0.5f) {
			transform.localRotation = Quaternion.Slerp (transform.localRotation, target, Time.deltaTime * speed);
			yield return null;
		}
		doorStatus = !doorStatus;
		yield return null;
	}
}
