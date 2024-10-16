using UnityEngine;

public class ParticleSimulation : MonoBehaviour
{
    private double lastTime;
    private ParticleSystem[] particleSystems;
    void Start()
    {
        // 시작 시에 모든 자식 오브젝트를 검색하여 시스템을 찾음
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        lastTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        float deltaTime = Time.realtimeSinceStartup - (float)lastTime;
        foreach (var particle in particleSystems)
        {
            particle.Simulate(deltaTime, true, false);
        }
        lastTime = Time.realtimeSinceStartup;
    }
    
}