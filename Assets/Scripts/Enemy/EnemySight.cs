using UnityEngine;
using System.Collections;

public class EnemySight : MonoBehaviour {

    // Number of degrees, centred on forward, for the enemy see.
    public float fieldOfViewAngle = 110f;
    // Has to be less than the collider radius.
    public float shootingRange = 5f;
    // Whether the player has been seen by us or not. Only resets when we stop chasing.
    public bool playerSeen;
    // Whether or not the player is currently in shooting range.
    public bool playerInShootingRange;
    // Whether or not the player is currently sighted.
    public bool playerHeard;
    // Last place this enemy spotted the player.
    public Vector3 lastPlayerPosition;

    // Reference to the NavMeshAgent component.
    UnityEngine.AI.NavMeshAgent nav;
    // Reference to the sphere collider trigger component.
    SphereCollider col;
    // Reference to the Animator.
    Animator anim;
    // Reference to the player.
    GameObject player;
    // Reference to the player's health script.
    PlayerHealth playerHealth;
    // Reference to the HashIDs.
    HashIDs hash;
    // Reference to last global sighting of the player.
    LastPlayerSighting lastPlayerSighting;

    void Awake() {
        // Setting up the references.
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        col = GetComponent<SphereCollider>();
        anim = GetComponent<Animator>();
        lastPlayerSighting = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<LastPlayerSighting>();
        player = GameObject.FindGameObjectWithTag(Tags.player);
        playerHealth = player.GetComponent<PlayerHealth>();
        hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();

        // Set the personal sighting and the previous sighting to the reset position.
        lastPlayerPosition = lastPlayerSighting.resetPosition;
    }

    void Update() {
        // If the player is alive...
        if (playerHealth.health > 0f) {
            // ... set the animator parameter to whether the player is in sight or not.
            anim.SetBool(hash.playerInShootingRangeBool, playerInShootingRange);
        }
        else {
            // ... set the animator parameter to false.
            anim.SetBool(hash.playerInShootingRangeBool, false);
        }
    }

    void OnTriggerStay(Collider other) {
        // If the player has entered the trigger sphere...
        if (other.gameObject == player) {

            // By default the player is not in shooting range.
            playerInShootingRange = false;

            // Create a vector from the enemy to the player and store the angle between it and forward.
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            // If the angle between forward and where the player is, is less than half the angle of view...
            if (angle < fieldOfViewAngle * 0.5f) {
                Vector3 rayOrigin = transform.position + 2 * transform.up;
                Vector3 rayDestination = other.GetComponent<ThirdPersonCharacter>().head.position;
                Vector3 rayDirection = rayDestination - rayOrigin;

                // ... and if a raycast towards the player hits something...
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, rayDirection.normalized, out hit, col.radius)) {
                    // ... and if the raycast hits the player...
                    if (hit.collider.gameObject == player) {
                        // ... the player is in sight.
                        playerSeen = true;
                        // Set the last global sighting is the players current position.
                        lastPlayerPosition = player.transform.position;

                        // Keep updating the global position if we're in alarm mode.
                        if (lastPlayerSighting.lastPlayerPosition != lastPlayerSighting.resetPosition) {
                            lastPlayerSighting.lastPlayerPosition = lastPlayerPosition;
                        }

                        float distance = (lastPlayerPosition - transform.position).magnitude;
                        if (distance <= shootingRange) {
                            playerInShootingRange = true;
                        }
                    }
                }
            }

            if (player.GetComponent<ThirdPersonCharacter>().state == "run") {
                // ... and if the player is within hearing range...
                if (CalculatePathLength(player.transform.position) <= col.radius) {
                    // ... set the last personal sighting of the player to the player's current position.
                    lastPlayerPosition = player.transform.position;
                    playerHeard = true;
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        // If the player leaves the trigger zone...
        if (other.gameObject == player) {
            // ... the player is not in sight.
            playerInShootingRange = false;
            playerHeard = false;
        }
    }

    float CalculatePathLength(Vector3 targetPosition) {
        // Create a path and set it based on a target position.
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        if (nav.enabled) {
            nav.CalculatePath(targetPosition, path);
        }

        // Create an array of points which is the length of the number of corners in the path + 2.
        Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

        // The first point is the enemy's position.
        allWayPoints[0] = transform.position;

        // The last point is the target position.
        allWayPoints[allWayPoints.Length - 1] = targetPosition;

        // The points inbetween are the corners of the path.
        for (int i = 0; i < path.corners.Length; i++) {
            allWayPoints[i + 1] = path.corners[i];
        }

        // Create a float to store the path length that is by default 0.
        float pathLength = 0f;

        // Increment the path length by an amount equal to the distance between each waypoint and the next.
        for (int i = 0; i < allWayPoints.Length - 1; i++) {
            pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
        }

        return pathLength;
    }
}
