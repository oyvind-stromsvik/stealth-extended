using UnityEngine;
using System.Collections;

public class StarField : MonoBehaviour {

	// The number of stars in the starfield.
	public int numberOfStars;

	// Local variables and references.
	ParticleSystem.Particle[] particles;
	int particleCount;
	
	void Start () {
		// Create a particle system with the specified number of particles.
		// The shape, look and feel of the particle system is defined in the
		// inspector.
		particles = new ParticleSystem.Particle[numberOfStars];
		GetComponent<ParticleSystem>().Emit(numberOfStars);
		particleCount = GetComponent<ParticleSystem>().GetParticles(particles);
	}

	void Update () {
		// Get the particles in the system so we can read/update their values.
		particleCount = GetComponent<ParticleSystem>().GetParticles(particles);

		// Set the particle life back to full when a particle is close to dying.
		// This makes each particle cycle through the effects defined in the
		// inspector indefinitely. This will enable the stars to actually remain
		// in place rather than die and spawn somewhere else.
		for (int i = 0; i < particles.Length; i++) {
    		if (particles[i].remainingLifetime <= 0.1f) {
    			particles[i].remainingLifetime = particles[i].startLifetime;
    		}
		}

		// Set the particles back again with the updated values.
		GetComponent<ParticleSystem>().SetParticles(particles, particleCount);
	}
}