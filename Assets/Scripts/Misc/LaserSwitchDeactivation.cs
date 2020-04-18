using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LaserSwitchDeactivation : MonoBehaviour {

    public AudioClip successSound;
    public AudioClip clickSound;
    public AudioClip errorSound;
    public GameObject guiCanvas;
    public GameObject guiButtons;
    public GameObject guiField;
    public GameObject guiInteractText;
    public GameObject guiLight;

    public string initialText = "ENTER PASSCODE";

    public string errorText = "WRONG PASSCODE";

    public string successText = "LASER DEACTIVATED";

    public string hackText = "FILL ALL THE CIRCLES";

    public bool hackable = false;

    public int code;

    public Text textComponent;

    public Transform cameraAnchor;

	// Reference to the laser that can we turned off at this switch.
	public GameObject laser;	
	
	// The screen's material to show the laser has been unloacked.
	public Material unlockedMat;		 	
	
	// Reference to the player.
    private GameObject player;

    Transform cameraOriginalAnchor;

    bool inUse = false;
    bool inRange = false;
    bool unlocked = false;
    bool hackMode = false;
    int animatingCounter = 0;

    Transform hackButton;
    Transform zeroButton;

    List<Number> numbers;

    class Number {
        public int id;
        public Transform transform;
        public bool status;
        public Number[] neighbors;
    }

	void Awake() {
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag(Tags.player);

        cameraOriginalAnchor = Camera.main.transform.parent;
	}

    void Start() {
        // Get the transforms for the numbers 1-9.
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        numbers = new List<Number>();
        int i = 0;
        foreach (Transform child in allChildren) {
            if (child.tag == "Number") {
                Number number = new Number();
                number.id = i;
                number.transform = child;
                number.status = false;
                numbers.Add(number);
                i++;
            }

            if (child.name == "GridCell-Hack") {
                hackButton = child;
            }

            if (child.name == "GridCell-0") {
                zeroButton = child;
            }
        }

        if (!hackable) {
            hackButton.GetComponent<Button>().interactable = false;
            hackButton.GetComponent<Animator>().enabled = false;
            hackButton.Find("Background").GetComponent<Image>().color = Color.white;
        }

        foreach (Number number in numbers) {
            number.neighbors = NumberNeighbors(number.id);
        }

        // Disable the GUI and the light, we toggle them when the player is in range.
        guiCanvas.SetActive(false);
        guiLight.SetActive(false);
        guiButtons.SetActive(false);
        guiField.SetActive(false);
    }

	void OnTriggerEnter(Collider other) {
		// If the colliding gameobject is the player...
		if (other.gameObject == player) {
			// ... and the switch button is pressed...
            inRange = true;

            // Enable the GUI and the light.
            guiCanvas.SetActive(true);
            guiLight.SetActive(true);
		}
	}

    void OnTriggerExit(Collider other) {
        // If the colliding gameobject is the player...
        if (other.gameObject == player) {
            // ... and the switch button is pressed...
            inRange = false;

            // Disable the GUI and the light.
            guiCanvas.SetActive(false);
            guiLight.SetActive(false);
        }
    }

    void Update() {
        if (!inRange) {
            return;
        }

        if (inUse) {
            // Unlock the cursor.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetButtonDown("Switch")) {
            if (inUse) {
                RepositionCamera(false);
            }
            else {
                RepositionCamera(true);
            }
        }
    }

    void RepositionCamera(bool derp) {
        if (derp) {
            Camera.main.GetComponentInParent<AutoCam>().enabled = false;
            Camera.main.transform.parent = cameraAnchor;

            // Unlock the cursor.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable the interact text and show the keypad.
            guiButtons.SetActive(true);
            guiField.SetActive(true);
            guiInteractText.SetActive(false);

            player.GetComponent<ThirdPersonCharacter>().Disable(true);
        }
        else {
            Camera.main.transform.parent = cameraOriginalAnchor;
            Camera.main.GetComponentInParent<AutoCam>().enabled = true;
            
            // Lock the cursor.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Enable the interact text and hide the keypad.
            guiButtons.SetActive(false);
            guiField.SetActive(false);
            guiInteractText.SetActive(true);

            player.GetComponent<ThirdPersonCharacter>().Disable(false);
        }
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.identity;
        inUse = derp;
    }
	
	void LaserDeactivation() {
		// Deactivate the laser GameObject.
		laser.SetActive(false);
		
		// Store the renderer component of the screen.
		Renderer screen = transform.Find("prop_switchUnit_screen_001").GetComponent<Renderer>();
		
		// Change the material of the screen to the unlocked material.
		screen.material = unlockedMat;
		
		// Play switch deactivation audio clip.
        AudioManager.instance.PlaySound(GetComponent<AudioSource>(), successSound);
	}

    public void ButtonCallback(int id) {
        if (animatingCounter > 0) {
            return;
        }

        // If this terminal is unlocked don't run any code.
        if (unlocked) {
            return;
        }

        AudioManager.instance.PlaySoundOneShot(GetComponent<AudioSource>(), clickSound);

        // For when we are hacking the terminal.
        if (hackMode) {
            int numberId = id - 1;

            // Toggle the status of the pressed number and the surrounding numbers.
            ToggleNumberStatus(numbers[numberId], true);

            // If any dot isn't filled we haven't succeeded yet.
            bool allDotsAreFilled = true;
            foreach (Number number in numbers) {
                if (number.status == false) {
                    allDotsAreFilled = false;
                }
            }

            // If we have succeeded the hacking run the same code as 
            // when you type in the correct passcode.
            if (allDotsAreFilled) {
                textComponent.text = successText;
                LaserDeactivation();

                unlocked = true;
            }

            // We don't want to run the code below when we're in hacking mode.
            return;
        }

        if (textComponent.text == initialText || textComponent.text == errorText) {
            textComponent.text = "";
        }
        textComponent.text += id.ToString();

        if (textComponent.text == code.ToString()) {
            textComponent.text = successText;
            LaserDeactivation();

            unlocked = true;
            return;
        }

        if (textComponent.text.Length >= code.ToString().Length) {
            AudioManager.instance.PlaySoundOneShot(GetComponent<AudioSource>(), errorSound);
            textComponent.text = errorText;
        }
    }

    void ToggleNumberStatus(Number number, bool recursion) {
        float fadeDuration = 0.25f;

        number.status = !number.status;

        if (number.status) {
            StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotActive").GetComponent<Image>(), 0f, 1f, fadeDuration));
        }
        else {
            StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotActive").GetComponent<Image>(), 1f, 0f, fadeDuration));
        }

        if (recursion) {
            foreach (Number neighbor in number.neighbors) {
                ToggleNumberStatus(neighbor, false);
            }
        }
    }

    public void ButtonCallbackExit() {
        AudioManager.instance.PlaySoundOneShot(GetComponent<AudioSource>(), clickSound);
        RepositionCamera(false);
    }

    public void ButtonCallbackHack() {
        if (animatingCounter > 0) {
            return;
        }

        // If this terminal is unlocked don't run any code.
        if (unlocked) {
            return;
        }

        AudioManager.instance.PlaySoundOneShot(GetComponent<AudioSource>(), clickSound);

        hackMode = !hackMode;

        if (hackMode) {
            // Firstly, fade out the numbers.
            foreach (Number number in numbers) {
                StartCoroutine(FadeAlphaFromToValue(number.transform.Find("Label").GetComponent<Text>(), 1f, 0f, 0.25f));
            }

            // Secondly, fade in the circles for the hacking mini-game.
            foreach (Number number in numbers) {
                StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotInactive").GetComponent<Image>(), 0f, 1f, 0.25f, 0.25f));
            }

            // Lastly, activate a random number of hacking dots.
            foreach (Number number in numbers) {
                if (Random.value > 0.5f) {
                    StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotActive").GetComponent<Image>(), 0f, 1f, 0.5f, 0.5f));
                    number.status = true;
                }
            }

            hackButton.GetComponentInChildren<Text>().text = "STOP";

            zeroButton.GetComponent<Button>().interactable = false;
            zeroButton.GetComponent<Animator>().enabled = false;
            zeroButton.Find("Background").Find("Label").GetComponent<Text>().text = "";

            textComponent.text = hackText;
        }
        else {
            // Firstly, deactivate all active the hacking dots.
            foreach (Number number in numbers) {
                if (number.status) {
                    StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotActive").GetComponent<Image>(), 1f, 0f, 0.25f));
                    number.status = false;
                }
            }

            // Secondly, fade out the circles for the hacking mini-game.
            foreach (Number number in numbers) {
                StartCoroutine(FadeAlphaFromToValue(number.transform.Find("HackDotInactive").GetComponent<Image>(), 1f, 0f, 0.25f, 0.25f));
            }

            // Lastly, fade in the numbers.
            foreach (Number number in numbers) {
                StartCoroutine(FadeAlphaFromToValue(number.transform.Find("Label").GetComponent<Text>(), 0f, 1f, 0.5f, 0.5f));
            }

            hackButton.GetComponentInChildren<Text>().text = "HACK";

            zeroButton.GetComponent<Button>().interactable = true;
            zeroButton.GetComponent<Animator>().enabled = true;
            zeroButton.Find("Background").Find("Label").GetComponent<Text>().text = "0";

            textComponent.text = "";
        }
    }

    /**
     * Fade the color alpha for a graphic component
     * over a duration after an optional delay.
     * 
     * @param Graphic graphic
     *   The graphic component containing the color attribute we want to modify.
     * @param float from
     *   The alpha value to fade from.
     * @param float to
     *   The alpha value to fade to.
     * @param float time
     *   The duration for the fade.
     * @param float delay
     *   The delay before we start fading. Useful if you first want to fade out
     *   something before you fade in something else.
     */
    IEnumerator FadeAlphaFromToValue(Graphic graphic, float from, float to, float time, float delay = 0f) {
        yield return new WaitForSeconds(delay);

        animatingCounter++;

        for (float t = 0f; t < 1f; t += Time.deltaTime / time) {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(from, to, t));
            graphic.color = newColor;
            yield return null;
        }

        animatingCounter--;
    }

    /**
     * Returns the neighboring numbers for any number on the keypad grid.
     * Used in the hacking mini-game to know which dots to hide/show.
     */
    Number[] NumberNeighbors(int id) {
        Number[] neighbors;

        switch (id) {
            case 0:
                neighbors = new Number[2];
                neighbors[0] = numbers[1];
                neighbors[1] = numbers[3];
                break;

            case 1:
                neighbors = new Number[3];
                neighbors[0] = numbers[0];
                neighbors[1] = numbers[2];
                neighbors[2] = numbers[4];
                break;

            case 2:
                neighbors = new Number[2];
                neighbors[0] = numbers[1];
                neighbors[1] = numbers[5];
                break;

            case 3:
                neighbors = new Number[3];
                neighbors[0] = numbers[0];
                neighbors[1] = numbers[4];
                neighbors[2] = numbers[6];
                break;

            case 4:
                neighbors = new Number[4];
                neighbors[0] = numbers[1];
                neighbors[1] = numbers[3];
                neighbors[2] = numbers[5];
                neighbors[3] = numbers[7];
                break;

            case 5:
                neighbors = new Number[3];
                neighbors[0] = numbers[2];
                neighbors[1] = numbers[4];
                neighbors[2] = numbers[8];
                break;

            case 6:
                neighbors = new Number[2];
                neighbors[0] = numbers[3];
                neighbors[1] = numbers[7];
                break;

            case 7:
                neighbors = new Number[3];
                neighbors[0] = numbers[4];
                neighbors[1] = numbers[6];
                neighbors[2] = numbers[8];
                break;

            default:
                neighbors = new Number[2];
                neighbors[0] = numbers[5];
                neighbors[1] = numbers[7];
                break;
        }

        return neighbors;
    }
}
