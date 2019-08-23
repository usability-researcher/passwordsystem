using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour {

	//Raycaster identifies this class
	
	void Start() {
		if (this.gameObject.isStatic) {
			Destroy (this);
		}
	}
}
