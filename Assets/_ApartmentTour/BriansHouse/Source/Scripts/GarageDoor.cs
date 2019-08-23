using UnityEngine;
using System.Collections;

public class GarageDoor : MonoBehaviour {

	Animator anim;
	static string BOOL = "openDoor";
	public AudioClip garageSound;

	[HideInInspector]
	public bool playerInRange;

	void Start () {
		anim = GetComponent<Animator> ();
	}

	void Update(){
		if (playerInRange) {
			if (Input.GetKeyDown (KeyCode.E)) {
				if (garageSound != null) 			AudioSource.PlayClipAtPoint (garageSound, transform.position);
				if (isGarageOpen ()) 				CloseGarage ();
				else 								OpenGarage ();
			}
		}
	}

	void OpenGarage() {
		anim.SetBool (BOOL, true);
	}

	void CloseGarage() {
		anim.SetBool (BOOL, false);
	}

	bool isGarageOpen() {
		return anim.GetBool (BOOL);
	}

}
