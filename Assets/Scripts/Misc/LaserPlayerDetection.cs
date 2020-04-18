using UnityEngine;
using System.Collections;

public class LaserPlayerDetection : MonoBehaviour {	
	
	// Reference to the player.
    private GameObject player;					
	
	// Reference to the global last sighting of the player.
    private LastPlayerSighting lastPlayerSighting;		

    void Awake()  {
		// Setting up references.
		player = GameObject.FindGameObjectWithTag(Tags.player);
		lastPlayerSighting = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<LastPlayerSighting>();
    }

    void OnTriggerStay(Collider other) {
		// If the beam is on...
        if (GetComponent<Renderer>().enabled) {
			// ... and if the colliding gameobject is the player...
            if (other.gameObject == player) {
				// ... set the last global sighting of the player to the colliding object's position.
                lastPlayerSighting.lastPlayerPosition = other.transform.position;
			}
		}
    }
}