using UnityEngine;
using System.Collections;

public class LiftTrigger : MonoBehaviour {

    public AudioClip liftSound;
    // Time since the player entered the lift before the doors close.
    public float timeToDoorsClose = 2f;
    // Time since the player entered the lift before it starts to move.		
    public float timeToLiftStart = 3f;
    // Time since the player entered the lift before the level ends.			
    public float timeToEndLevel = 6f;
    // The speed at which the lift moves.	
    public float liftSpeed = 3f;

    // Reference to the player.
    GameObject player;
    // Reference to the SceneFadeInOut script.		
    SceneFadeInOut sceneFadeInOut;
    // Reference to LiftDoorsTracking script.
    LiftDoorsTracking liftDoorsTracking;
    // Whether the player is in the lift or not.
    bool playerInLift;
    // Timer to determine when the lift moves and when the level ends.			
    float timer;								

    void Awake() {
        // Setting up references.
        player = GameObject.FindGameObjectWithTag(Tags.player);
        sceneFadeInOut = GameObject.FindGameObjectWithTag(Tags.fader).GetComponent<SceneFadeInOut>();
        liftDoorsTracking = GetComponent<LiftDoorsTracking>();
    }

    void OnTriggerEnter(Collider other) {
        // If the colliding gameobject is the player...
        if (other.gameObject == player) {
            // ... the player is in the lift.
            playerInLift = true;
        }
    }

    void OnTriggerExit(Collider other) {
        // If the player leaves the trigger area...
        if (other.gameObject == player) {
            // ... reset the timer, the player is no longer in the lift and unparent the player from the lift.
            playerInLift = false;
            timer = 0;
        }
    }

    void Update() {
        // If the player is in the lift...
        if (playerInLift) {
            // ... activate the lift.
            LiftActivation();
        }

        // If the timer is less than the time before the doors close...
        if (timer < timeToDoorsClose) {
            // ... the inner doors should follow the outer doors.
            liftDoorsTracking.DoorFollowing();
        }
        else {
            // Otherwise the doors should close.
            liftDoorsTracking.CloseDoors();
        }
    }

    void LiftActivation() {
        // Increment the timer by the amount of time since the last frame.
        timer += Time.deltaTime;

        // If the timer is greater than the amount of time before the lift should start...
        if (timer >= timeToLiftStart) {
            // ... stop the player and the camera moving and parent the player to the lift.
            player.GetComponent<ThirdPersonCharacter>().Disable(true);
            player.transform.parent = transform;

            // Move the lift upwards.
            transform.Translate(Vector3.up * liftSpeed * Time.deltaTime);

            // If the audio clip isn't playing...
            if (!GetComponent<AudioSource>().isPlaying) {
                // ... play the clip.
                AudioManager.instance.PlaySound(GetComponent<AudioSource>(), liftSound);
            }

            // If the timer is greater than the amount of time before the level should end...
            if (timer >= timeToEndLevel) {
                // ... call the EndScene function.
                sceneFadeInOut.EndScene();
            }
        }
    }
}
