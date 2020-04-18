using UnityEngine;
using System.Collections;

public class DoorAnimation : MonoBehaviour {

    // Whether or not a key is required.
    public bool requireKey;
    // Clip to play when the doors open or close.
    public AudioClip doorSwishClip;
    // Clip to play when the player doesn't have the key for the door.
    public AudioClip accessDeniedClip;

    // Reference to the animator component.
    Animator anim;
    // Reference to the HashIDs script.				
    HashIDs hash;
    // Reference to the player GameObject.			
    GameObject player;
    // Reference to the PlayerInventory script.			
    PlayerInventory playerInventory;
    // The number of colliders present that should open the doors.
    int count;								

    void Awake() {
        // Setting up the references.
        anim = GetComponent<Animator>();
        hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
        player = GameObject.FindGameObjectWithTag(Tags.player);
        playerInventory = player.GetComponent<PlayerInventory>();
    }

    void OnTriggerEnter(Collider other) {
        // If the triggering gameobject is the player...
        if (other.gameObject == player) {
            // ... if this door requires a key...
            if (requireKey) {
                // ... if the player has the key...
                if (playerInventory.hasKey) {
                    // ... increase the count of triggering objects.
                    count++;
                }
                else {
                    // If the player doesn't have the key play the access denied audio clip.
                    AudioManager.instance.PlaySound(GetComponent<AudioSource>(), accessDeniedClip);
                }
            }
            else {
                // If the door doesn't require a key, increase the count of triggering objects.
                count++;
            }
        }
        // If the triggering gameobject is an enemy...
        else if (other.gameObject.tag == Tags.enemy) {
            // ... if the triggering collider is a capsule collider...
            if (other is CapsuleCollider) {
                if (!requireKey) {
                    // ... increase the count of triggering objects.
                    count++;
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        // If the leaving gameobject is the player or an enemy and the collider is a capsule collider...
        if (other.gameObject == player || (other.gameObject.tag == Tags.enemy && other is CapsuleCollider)) {
            // decrease the count of triggering objects.
            count = Mathf.Max(0, count - 1);
        }
    }

    void Update() {
        // Set the open parameter.
        anim.SetBool(hash.openBool, count > 0);

        // If the door is opening or closing...
        if (anim.IsInTransition(0) && !GetComponent<AudioSource>().isPlaying) {
            // ... play the door swish audio clip.
            AudioManager.instance.PlaySound(GetComponent<AudioSource>(), doorSwishClip);
        }
    }
}
