using UnityEngine;
using System.Collections;

public class Interactions : MonoBehaviour {

	//Handles object carrying and lightswitch

	Camera cam;
	[Range(1, 5)]
	public float rayDistance;
	[Range(5, 15)]
	public float throwForce;
	public Texture2D crosshair, eButton;
	Transform FPS_parentSlot;

	float pot;
	bool moving;
	bool droppable = false;

	GameObject carriedObject = null;
	GameObject lastRecognized = null;
	Door lastDoor = null;
	GarageDoor lastGarageDoor = null;

	public bool showDemoMessage;

	void Start () {
		if (GameObject.Find ("Canvas_DemoMessage")) {
			Destroy (GameObject.Find ("Canvas_DemoMessage").gameObject);
		}
		cam = Camera.main;
		if (cam == null) {
			Debug.LogError ("Main camera tag not found in scene!");
			Destroy (this.gameObject);
		} 
		if (!cam.hdr)
			cam.hdr = true;		
		pot = eButton.width;
		//Create a dummy slot where the picked up object will be parented when picked up
		if (FPS_parentSlot == null || !FPS_parentSlot.gameObject.activeInHierarchy) {
			GameObject g = new GameObject();
			g.transform.SetParent(cam.transform);
			g.transform.localPosition = new Vector3(0f, -0.196f, 1.091f);
			g.transform.rotation = Quaternion.identity;
			g.name = "carrySpot";
			FPS_parentSlot = g.transform;
		}
	}
	
	void Update () {

		Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, rayDistance)) {
			if (hit.transform.GetComponent<Pickupable>()) {
				Deselect();
				Recognize(hit.transform);
				if (Input.GetKeyDown(KeyCode.E) && !carriedObject) {
					PickUpObject(hit.transform);
					StartCoroutine(delay(0.3f));
				}
			}
			else {
				Deselect();
				if (hit.transform.GetComponent<SwitchLight> ()) {
					Recognize (hit.transform);
					if (Input.GetKeyDown (KeyCode.E)) {
						SwitchLight sl = hit.transform.GetComponent<SwitchLight> ();
						sl.SwitchThisLight ();
					}
				} else if (hit.transform.GetComponent<Door> ()) {
					Recognize (hit.transform);
					lastDoor = hit.transform.GetComponent<Door> ();
					lastDoor.playerInRange = true;
				}
				else if (hit.transform.GetComponent<GarageDoor>()) {
					Recognize(hit.transform);
					lastGarageDoor = hit.transform.GetComponent<GarageDoor> ();
					lastGarageDoor.playerInRange = true;
				}
			}
		}
		else {
			Deselect();
		}

		if (carriedObject && droppable) {
			RotateObject();
			DropObject();
		}
	}

	void FixedUpdate() {
		if (moving) {
			MeshFilter mf = FPS_parentSlot.GetComponentInChildren<MeshFilter>();
			if (mf != null) {
				mf.transform.localPosition = Vector3.Slerp(mf.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
			}
		}
	}

	void Recognize (Transform transform) {
		transform.GetComponent<Renderer>().material.color = Color.green;
		lastRecognized = transform.gameObject;
	}

	void Deselect() {
		if (lastRecognized) {
			lastRecognized.GetComponent<Renderer>().material.color = Color.white;
			lastRecognized = null;
		}
		if (lastDoor) {
			lastDoor.playerInRange = false;
			lastDoor = null;
		}
		if (lastGarageDoor) {
			lastGarageDoor.playerInRange = false;
			lastGarageDoor = null;
		}
		//TODO extended code

	}

	void RotateObject () {
		carriedObject.transform.RotateAround(carriedObject.transform.position, carriedObject.transform.up, Time.deltaTime * 90f);
	}

	void DropObject() {
		Deselect();
		if (Input.GetKeyDown(KeyCode.E)) {
			moving = false;
			Rigidbody rb = carriedObject.GetComponent<Rigidbody>();
			rb.transform.parent = null;
			rb.isKinematic = false;
			carriedObject = null;
			rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
		}
	}

	void PickUpObject (Transform transform) {
		Deselect();
		if (!carriedObject) {
			Rigidbody rb = transform.GetComponent<Rigidbody>();
			carriedObject = transform.gameObject;
			rb.isKinematic = true;
			rb.transform.SetParent(FPS_parentSlot);
			moving = true;
			rb.transform.rotation = Quaternion.identity;
		}
	}

	IEnumerator delay(float secs) {
		droppable = false;
		yield return new WaitForSeconds(secs);
		droppable = true;
	}

	void OnGUI() {
		Rect rect = new Rect(Screen.width/2, Screen.height/2, crosshair.width, crosshair.height);
		GUI.DrawTexture(rect, crosshair);
		if (lastRecognized) {
			Rect buttonRect = new Rect(Screen.width - pot, Screen.height - pot, pot, pot);
			GUI.DrawTexture(buttonRect, eButton);
		}
		if (showDemoMessage) {
			GUI.Label(new Rect(20, 20, 300, 50), "Use the light switches in the rooms! And remember - China breaks!");
		}
				                          
	}
}
