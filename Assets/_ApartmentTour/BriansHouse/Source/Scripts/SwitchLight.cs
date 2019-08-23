using UnityEngine;
using System.Collections;

public class SwitchLight : MonoBehaviour {

	public GameObject lamp;
	public typeOfLight lightType;
	public ReflectionProbe roomReflectionProbe;
	public AudioClip lightOn;
	public AudioClip lightOff;

	[Range(1f, 10f)] public float GI_Strength = 6f;
	Light lampLight;
	Renderer lampRenderer;

	public bool isOn = true;

	public enum typeOfLight {
		CeilingLamp,
		TableLamp,
		Fluorescent
	}

	void Start(){
		lampLight = lamp.GetComponentInChildren<Light> ();

		//Check if all components are set right
		if (lampLight == null) {
			Debug.Log ("No light component found nested for this house lamp. Continuing without this lightswitch..");
			Destroy (this);
		} else {
			if (lampLight.isBaked) {
				Debug.Log ("This light has already been baked into lightmap. Realtime light switching won't work on this instance. If this is intentional, ignore this message");
				Destroy (this);
			}
		}
		if (roomReflectionProbe == null) {
			Debug.Log ("No reflection probe has been assigned for this lightswitch! This lightswitch won't function..");
			Destroy (this);
		} else {
			if (roomReflectionProbe.mode != UnityEngine.Rendering.ReflectionProbeMode.Realtime) {
				Debug.Log ("This reflection probe is set to baked and lightswitch won't work in this room. If this is intentional (for example a baked scene), ignore this message, everything is good!");
				Destroy (this);
			}
		}
		lampRenderer = lamp.GetComponent<Renderer> ();
	}

	public void SwitchThisLight() {
		if (!isOn) {
			lampLight.enabled = true;
			lampLight.intensity = lightIntensity ();
			AudioSource.PlayClipAtPoint (lightOn, transform.position);
			Color color = Color.white * Mathf.LinearToGammaSpace (GI_Strength);
			lampRenderer.material.SetColor ("_EmissionColor", color);
			DynamicGI.SetEmissive (lampRenderer, color);
			DynamicGI.UpdateEnvironment ();
			roomReflectionProbe.RenderProbe ();
			isOn = true;
			StartCoroutine (afterBake ());
		} else {
			lampLight.enabled = false;
			AudioSource.PlayClipAtPoint (lightOn, transform.position);
			Color color = Color.white * Mathf.LinearToGammaSpace (0.00001f);
			lampRenderer.material.SetColor ("_EmissionColor", color);
			DynamicGI.SetEmissive (lampRenderer, color);
			DynamicGI.UpdateEnvironment ();
			roomReflectionProbe.RenderProbe ();
			isOn = false;
			StartCoroutine (afterBake ());
		}
	}

	//Rebake reflection probe one more time
	IEnumerator afterBake(){
		yield return new WaitForSeconds (0.5f);
		roomReflectionProbe.RenderProbe ();
	}

	private float lightIntensity() {
		if (GI_Strength <= 2) {
			return 0.4f;
		} else if (GI_Strength <= 4) {
			return 1f;
		} else if (GI_Strength <= 6) {
			return 1.5f;
		} else {
			return 1.7f;
		}
	}

	private float range(){
		if (lightType.Equals (typeOfLight.CeilingLamp)) {
			return 8;
		} else if (lightType.Equals (typeOfLight.TableLamp)) {
			return 5;
		} else if (lightType.Equals (typeOfLight.Fluorescent)) {
			return 3;
		} else {
			throw new UnityException ("Not implemented enum value");
		}
	}

}
