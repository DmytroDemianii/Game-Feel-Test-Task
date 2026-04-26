using System;
using DamageNumbersPro;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private DamageNumber _damageNumberPrefab;
    [SerializeField] private float _hitCooldown = 0.2f;

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem[] _effectPrefab;
    [SerializeField] private bool _spawnEffectOnHit = true;

    public event Action OnHeadBump;
    
    private float _lastHitTime;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.time < _lastHitTime + _hitCooldown) return;

        if (hit.point.y > transform.position.y + 1.5f)
        {
            if (Vector3.Dot(hit.normal, Vector3.down) > 0.8f)
            {
                _lastHitTime = Time.time;

                OnHeadBump?.Invoke();
                SpawnHitEffect(hit.point);

                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    ExecuteHit(interactable, hit.point);
                }
            }
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (!_spawnEffectOnHit || _effectPrefab == null || _effectPrefab.Length == 0) return;

        for (int i = 0; i < _effectPrefab.Length; i++)
        {
            var effect = _effectPrefab[i];

            if (effect != null && !effect.gameObject.activeSelf)
            {
                effect.transform.position = position;
                effect.gameObject.SetActive(true);
                effect.Play();
                break; 
            }
        }
    }

    private void ExecuteHit(IInteractable interactable, Vector3 hitPoint)
    {
        interactable.OnHit();
        SpawnDamageNumber(hitPoint, 1);
    }

    private void SpawnDamageNumber(Vector3 position, int value)
    {
        if (_damageNumberPrefab != null)
        {
            _damageNumberPrefab.Spawn(position, "+" + value);
        }
    }
}