using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using System.Collections;

public class Pause : MonoBehaviour {

    // Store a reference to the Game Object PausePanel.
    public GameObject pausePanel;
    public Text brightnessText;
    public Text volumeText;
    public Text startResumeText;

    // Boolean to check if the game is paused or not.
    [HideInInspector]		
    public bool isPaused;

    bool hasStarted = false;
    float originalVignette;
    float originalBlur;
    float originalDistance;
    Brightness brightness;
    VignetteAndChromaticAberration vignette;

    void Start() {
        vignette = Camera.main.GetComponent<VignetteAndChromaticAberration>();
        originalVignette = vignette.intensity;
        originalBlur = vignette.blur;
        originalDistance = vignette.blurDistance;
        brightness = Camera.main.GetComponent<Brightness>();
        DoPause();
    }

    // Update is called once per frame
    void Update() {
        //Check if the Cancel button in Input Manager is down this frame (default is Escape key) and that game is not paused, and that we're not in main menu
        if (Input.GetButtonDown("Cancel") && !isPaused) {
            //Call the DoPause function to pause the game
            DoPause();
        }
        //If the button is pressed and the game is paused and not in main menu
        else if (Input.GetButtonDown("Cancel") && isPaused) {
            //Call the UnPause function to unpause the game
            UnPause();
        }
    }

    public void DoPause() {
        if (hasStarted) {
            startResumeText.text = "Resume";
        }

        //Set isPaused to true
        isPaused = true;
        //Set time.timescale to 0, this will cause animations and physics to stop updating
        Time.timeScale = 0;
        //call the ShowPausePanel function of the ShowPanels script
        pausePanel.SetActive(true);

        // unloock the cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        vignette.intensity = originalVignette;
        vignette.blur = originalBlur;
        vignette.blurDistance = originalDistance;

        GetComponent<AudioSource>().Play();
    }

    public void UnPause() {
        //Set isPaused to false
        isPaused = false;
        //Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
        Time.timeScale = 1;
        //call the HidePausePanel function of the ShowPanels script
        pausePanel.SetActive(false);

        // Lock the cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(FadeIn(1f));

        hasStarted = true;

        GetComponent<AudioSource>().Stop();
    }

    IEnumerator FadeIn(float time) {
        for (float t = 0f; t < 1f; t += Time.deltaTime / time) {
            vignette.intensity = Mathf.Lerp(originalVignette, 0, t);
            vignette.blur = Mathf.Lerp(originalBlur, 0, t);
            vignette.blur = Mathf.Lerp(originalDistance, 0, t);
            yield return null;
        }

        vignette.intensity = 0;
        vignette.blur = 0;
        vignette.blurDistance = 0;
    }

    public void SetBrightness(float value) {
        brightness.brightness = value;
        brightnessText.text = "Brightness " + value.ToString("0.0");
    }

    public void SetVolume(float value) {
        AudioListener.volume = value;
        volumeText.text = "Volume " + value.ToString("0.0");
    }

    public void Quit() {
        Application.Quit();
    }
}
