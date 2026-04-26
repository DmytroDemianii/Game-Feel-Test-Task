using UnityEngine;
using DG.Tweening;

public class BonusBlock : MonoBehaviour, IInteractable
{
    [Header("Hit Stop Settings")]
    [SerializeField] private float _hitStopScale = 0.1f;
    [SerializeField] private float _duration = 0.2f;
    [SerializeField] private bool _reduceTimeScaleEffect = true;

    [Header("Punch Settings")]
    [SerializeField] private Vector3 _punchScale = new Vector3(0.15f, 0.15f, 0.15f);
    [SerializeField] private float _jumpHeight = 0.2f;

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem _effectPrefab;
    [SerializeField] private bool _spawnEffectOnHit = true;

    public void OnHit()
    {
        transform.DOComplete();
        DOTween.Kill("HitStop");

        if (_spawnEffectOnHit && _effectPrefab != null)
        {
            _effectPrefab.Play();
        }

        Sequence hitSequence = DOTween.Sequence();

        hitSequence.Append(transform.DOPunchScale(_punchScale, _duration, 10, 1f));
        hitSequence.Join(transform.DOPunchPosition(new Vector3(0, _jumpHeight, 0), _duration, 0, 0));

        if (_reduceTimeScaleEffect)
        {
            Time.timeScale = _hitStopScale;

            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, _duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetId("HitStop");
        }

        hitSequence.SetUpdate(true);
    }
}