using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

    // The nav mesh agent's speed when patrolling.
    public float patrolSpeed = 2f;
    // The nav mesh agent's speed when chasing.			
    public float chaseSpeed = 5f;
    // The amount of time to wait when the last sighting is reached.			
    public float chaseWaitTime = 5f;
    // The amount of time to wait when the patrol way point is reached.			
    public float patrolMinWaitTime = 1f;
    public float patrolMaxWaitTime = 5f;
    // An array of transforms for the patrol route.			
    public Transform[] patrolWayPoints;

    // Reference to the EnemySight script.
    EnemySight enemySight;
    // Reference to the nav mesh agent.				
    UnityEngine.AI.NavMeshAgent nav;

    // Reference to the PlayerHealth script.				
    PlayerHealth playerHealth;
    // Reference to the last global sighting of the player.			
    LastPlayerSighting lastPlayerSighting;
    // A timer for the chaseWaitTime.
    float chaseTimer;
    // A timer for the patrolWaitTime.				
    float patrolTimer;
    // A counter for the way point array.				
    int wayPointIndex;
    float patrolWaitTime;			

    void Awake() {
        // Setting up the references.
        enemySight = GetComponent<EnemySight>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        playerHealth = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<PlayerHealth>();
        lastPlayerSighting = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<LastPlayerSighting>();
    }

    void Update() {
        if (playerHealth.health <= 0) {
            return;
        }

        // If the player is in shooting range we shoot him.
        if (enemySight.playerInShootingRange) {
            Shooting();
        }
        // If the player has triggered an alarm, run to investigate.
        else if (lastPlayerSighting.lastPlayerPosition != lastPlayerSighting.resetPosition) {
            Chasing(true);
        }
        // If the player has been sighted or heard we investigate or chase.
        else if (enemySight.lastPlayerPosition != lastPlayerSighting.resetPosition) {
            Chasing();
        }
        // Otherwise patrol.
        else {
            Patrolling();
        }
    }

    void Shooting() {
        // Stop the enemy where it is.
        nav.Stop();
    }

    void Chasing(bool alarm = false) {
        nav.Resume();

        // Either use the global alarm position or our own position for the player.
        Vector3 lastPlayerPosition = alarm ? lastPlayerSighting.lastPlayerPosition : enemySight.lastPlayerPosition;

        // Create a vector from the enemy to the last known position of the player.
        Vector3 sightingDeltaPos = lastPlayerPosition - transform.position;

        // If the the last personal sighting of the player is not close...
        if (sightingDeltaPos.sqrMagnitude > 4f) {
            // ... set the destination for the NavMeshAgent to the last personal sighting of the player.
            nav.destination = lastPlayerPosition;
        }

        // Set the appropriate speed for the NavMeshAgent.
        if (enemySight.playerSeen || alarm) {
            nav.speed = chaseSpeed;
        }
        else {
            nav.speed = patrolSpeed;
        }

        // If near the last personal sighting...
        if (nav.remainingDistance < nav.stoppingDistance) {
            // ... increment the timer.
            chaseTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (chaseTimer >= chaseWaitTime) {
                // ... reset last global sighting, the last personal sighting and the timer.
                lastPlayerSighting.lastPlayerPosition = lastPlayerSighting.resetPosition;
                enemySight.lastPlayerPosition = lastPlayerSighting.resetPosition;
                enemySight.playerSeen = false;
                chaseTimer = 0f;
            }
        }
        else {
            // If not near the last sighting personal sighting of the player, reset the timer.
            chaseTimer = 0f;
        }
    }

    void Patrolling() {
        if (patrolWayPoints.Length == 0) {
            return;
        }

        nav.Resume();

        // Set an appropriate speed for the NavMeshAgent.
        nav.speed = patrolSpeed;

        // If near the next waypoint or there is no destination...
        if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance) {
            if (patrolTimer == 0) {
                patrolWaitTime = Random.Range(patrolMinWaitTime, patrolMaxWaitTime);
            }

            // ... increment the timer.
            patrolTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (patrolTimer >= patrolWaitTime) {
                // ... increment the wayPointIndex.
                if (wayPointIndex == patrolWayPoints.Length - 1) {
                    wayPointIndex = 0;
                }
                else {
                    wayPointIndex++;
                }

                // Reset the timer.
                patrolTimer = 0;
            }
        }
        else {
            // If not near a destination, reset the timer.
            patrolTimer = 0;
        }

        // Set the destination to the patrolWayPoint.
        nav.destination = patrolWayPoints[wayPointIndex].position;
    }
}
