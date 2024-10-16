using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PolygonCollider2D))]
public class FanSkillController : MonoBehaviour
{
    private float _radius = 6f;
    private float _spreadAngle = 60f;
    private float _duration = 3f;
    private float _particleDelay = 0.05f;
    
    public float ParticleDelay
    {
        get => _particleDelay;
        set => _particleDelay = value;
    }

    private PolygonCollider2D _polygonCollider;
    private bool _isCharmMode;
    public bool IsCharmMode
    {
        get => _isCharmMode;
        set => _isCharmMode = value;
    }
    public void Init(float radius_, float spreadAngle_, float duration_)
    {
        _radius = radius_;
        _spreadAngle = spreadAngle_;
        _duration = duration_;

        _polygonCollider = GetComponent<PolygonCollider2D>();
        StartCoroutine(SpreadFan());
    }

    private IEnumerator SpreadFan()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            float current_radius = Mathf.Lerp(0f, _radius, elapsedTime / _duration);
            DrawFan(current_radius);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Optionally destroy or disable the GameObject after the spread is complete
        Destroy(gameObject);
    }

    private void DrawFan(float current_radius)
    {
        int segments = 50;
        float angleStep = _spreadAngle / segments;

        Vector2[] points = new Vector2[segments + 2];

        points[0] = Vector2.zero;

        for (int i = 1; i <= segments + 1; i++)
        {
            float angle = (i - 1) * angleStep - _spreadAngle / 2f;
            Vector2 position = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)) * current_radius;

            points[i] = position;
        }

        _polygonCollider.SetPath(0, points);

        Vector3 randomPosition = GetRandomPositionInFan(current_radius);

        if (IsCharmMode)
        {
            SpawnCharmingParticle(randomPosition);
        }
        else
        {
            SpawnHealingParticle(randomPosition);
        }
    }

    private void SpawnCharmingParticle(Vector3 randomPosition)
    {
        GameObject _charmingParticlePrefab = Resources.Load<GameObject>("Prefabs/Effects/CharmingParticleEffect");
        GameObject _charmingParticle = Instantiate(_charmingParticlePrefab, transform.position + randomPosition, Quaternion.identity);
        //Set lifetime of _charmingParticle
        var mainModule = _charmingParticle.GetComponent<ParticleSystem>().main;
        mainModule.startLifetime = _duration;
        
        Destroy(_charmingParticle, _duration);

        // Delay between particle spawns
        StartCoroutine(ParticleDelayCO());
    }
    
    private void SpawnHealingParticle(Vector3 randomPosition)
    {
        GameObject _healingParticlePrefab = Resources.Load<GameObject>("Prefabs/Effects/HealingParticleEffect");
        GameObject _healingParticle = Instantiate(_healingParticlePrefab, transform.position + randomPosition, Quaternion.identity);
        //Set lifetime of _healingParticle
        var mainModule = _healingParticle.GetComponent<ParticleSystem>().main;
        mainModule.startLifetime = _duration;
        
        Destroy(_healingParticle, _duration);

        // Delay between particle spawns
        StartCoroutine(ParticleDelayCO());
    }
    
    
    private IEnumerator ParticleDelayCO()
    {
        yield return new WaitForSeconds(_particleDelay);
    }

    private Vector3 GetRandomPositionInFan(float current_radius)
    {
        float randomAngle = Random.Range(-_spreadAngle / 2f, _spreadAngle / 2f);
        float randomRadius = Random.Range(0f, current_radius);

        // Convert polar coordinates to Cartesian coordinates
        float x = randomRadius * Mathf.Cos(Mathf.Deg2Rad * randomAngle);
        float y = randomRadius * Mathf.Sin(Mathf.Deg2Rad * randomAngle);

        // Rotate the random position by the current rotation of the fan
        Vector3 randomPosition = new Vector3(x, y, 0f);
        randomPosition = transform.rotation * randomPosition;

        return randomPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCharmMode)
        {
            if (other.TryGetComponent(out Unit unit))
            {
                if (unit.CurrentFaction == Faction.Undead) return;
                if (unit.IsDead) return;
                var charmSounds = new List<string> {"Heavy Running  Grass footsteps 1", "Heavy Running  Grass footsteps 2"};
                ManagerRoot.Sound.PlaySfx(charmSounds[Random.Range(0, charmSounds.Count)]);
                ManagerRoot.Sound.PlaySfx("Body Flesh 1", 1f);
                ManagerRoot.Sound.PlaySfx("Body Flesh 2", 1f);
                ManagerRoot.Sound.PlaySfx("Body Flesh 10", 1f);
                unit.TakeCharm(5f, null);
            }
        }
        else
        {
            if (other.TryGetComponent(out Unit unit))
            {
                if (unit.CurrentFaction == Faction.Undead) return;
                if (unit.IsDead) return;
                ManagerRoot.Sound.PlaySfx("Powerup upgrade 6", 1f);
                unit.TakeHeal(40f, null);
            }
        }
        
    }
}
