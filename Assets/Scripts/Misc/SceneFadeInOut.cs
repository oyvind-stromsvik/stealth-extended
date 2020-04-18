using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneFadeInOut : MonoBehaviour {
	
	// Speed that the screen fades to and from black.
	public float fadeSpeed = 1.5f;			
	
	// Whether or not the scene is still fading in.
	private bool sceneStarting = true;		
	
	void Awake() {
		// Set the texture so that it is the the size of the screen and covers it.
		GetComponent<GUITexture>().pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
	}
	
	void Update() {
		// If the scene is starting call the StartScene function.
		if (sceneStarting) {
			StartScene();
		}
	}

	void StartScene() {
		// Make sure the texture is enabled.
		//GetComponent<GUITexture>().enabled = true;
		
		// Fade the texture to clear.
		// Lerp the color of the texture between itself and transparent.
		GetComponent<GUITexture>().color = Color.Lerp(GetComponent<GUITexture>().color, Color.clear, fadeSpeed * Time.deltaTime);
		
		// If the texture is almost clear.
		if (GetComponent<GUITexture>().color.a <= 0.05f) {
			// Set the color to clear and disable the GUITexture.
			GetComponent<GUITexture>().color = Color.clear;
			GetComponent<GUITexture>().enabled = false;
			
			// The scene is no longer starting.
			sceneStarting = false;
		}
	}
	
	public void EndScene() {
		// Make sure the texture is enabled.
		GetComponent<GUITexture>().enabled = true;
		
		// Start fading towards black.
		// Lerp the color of the texture between itself and black.
		GetComponent<GUITexture>().color = Color.Lerp(GetComponent<GUITexture>().color, Color.black, fadeSpeed * Time.deltaTime);
		
		// If the screen is almost black reload the level.
		if (GetComponent<GUITexture>().color.a >= 0.95f) {
            SceneManager.LoadScene(0);
		}
	}
}
