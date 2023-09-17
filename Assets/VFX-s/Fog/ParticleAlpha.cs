using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class ParticleAlpha : MonoBehaviour
    {
        public float maxAngle = 45f; // Maximum angle of influence from spotlight facing direction
        public float minimumAlfa =0;
        private ParticleSystem particleSystem;
        public List<Light> spotlights;

        void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
            spotlights = GameManager.Instance.listFlashLights; // Retrieve initial list from GameManager
        }

        void Update()
        {
            // Check if the list of spotlights in GameManager has changed during runtime

            List<Light> spotlights = GameManager.Instance.listFlashLights;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            int numParticlesAlive = particleSystem.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                float alpha = 0;

                foreach (Light spotlight in spotlights)
                {
                    if(spotlight == null)
                        continue;
                    Vector3 toParticle = particles[i].position - spotlight.transform.position;
                    float angleToSpotlight = Vector3.Angle(spotlight.transform.forward, toParticle);
                    float normalizedAngle = Mathf.Clamp01(angleToSpotlight / maxAngle);
                    float normalizedRange = Mathf.Clamp01(1 - (toParticle.magnitude / spotlight.range)); // Inverse effect with distance from spotlight
                    float spotlightAlpha = Mathf.Clamp01(1 - normalizedAngle) * normalizedRange;

                    if(spotlightAlpha>alpha)
                        alpha = spotlightAlpha;
                }

                Color particleColor = particles[i].startColor;
                particleColor.a = alpha;
                particles[i].startColor = particleColor;
            }

            particleSystem.SetParticles(particles, numParticlesAlive);
        }

    }
}