using UnityEngine;

public class StarfieldGenerator : MonoBehaviour
{
    private const int STARS = 3000;
    private const float DISTANCE = 35.0f;

    private ParticleSystem system;
    private ParticleSystem.Particle[] particles;

    private void Start()
    {
        if (GameObject.FindGameObjectsWithTag("Stars").Length > 1)
        {
            Destroy(gameObject);
        } else {
            particles = new ParticleSystem.Particle[STARS];

            for (int i = 0; i < STARS; i++)
            {
                particles[i].position = Random.insideUnitSphere * DISTANCE;
                while (Vector3.Distance(Vector3.zero, particles[i].position) < DISTANCE * 0.9f)
                {
                    particles[i].position = Random.insideUnitSphere * DISTANCE;
                }
                particles[i].startSize = Random.Range(0.01f, 0.05f);
                particles[i].startColor = new Color(1, 1, 1, 1);
            }

            system = gameObject.GetComponent<ParticleSystem>();
            system.SetParticles(particles, particles.Length);
            DontDestroyOnLoad(this.gameObject);
        }

    }
}
