using UnityEngine;
using System.Collections;

public class KeyPickup : MonoBehaviour {
	
	// Audioclip to play when the key is picked up.
	public AudioClip keyGrab;							
	
	// Reference to the player.
	private GameObject player;		
	
	// Reference to the player's inventory.
	private PlayerInventory playerInventory;		
	
	void Awake() {
		// Setting up the references.
		player = GameObject.FindGameObjectWithTag(Tags.player);
		playerInventory = player.GetComponent<PlayerInventory>();
	}
	
    void OnTriggerEnter(Collider other) {
		// If the colliding gameobject is the player...
		if(other.gameObject == player) {
			// ... play the clip at the position of the key...
            AudioManager.instance.PlaySoundAtPosition(keyGrab, transform.position);
			
			// ... the player has a key ...
			playerInventory.hasKey = true;
			
			// ... and destroy this gameobject.
        	Destroy(gameObject);
		}
    }
}
