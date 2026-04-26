using UnityEngine;
using DG.Tweening;

public class SimpleCharacterCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);
    [SerializeField] private float _smoothSpeed = 5f;


    [Header("Shake Effect Settings")]
    [SerializeField] private float _defaultShakeDuration = 0.2f;
    [SerializeField] private float _defaultShakeStrength = 0.5f;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90f;

    [Header("Zoom Effect Settings")]
    [SerializeField] private float _defaultZoomDuration = 0.2f;
    [SerializeField] private float _defaultZoomStrength = 10f;

    private Vector3 _shakeOffset;
    private PlayerCollisionHandler _collisionHandler;
    private Tween _shakeTween;

    private float _baseFOV;
    private Sequence _zoomSequence;

    void Start()
    {
        _baseFOV = Camera.main.fieldOfView;
        _collisionHandler = _target.GetComponent<PlayerCollisionHandler>();
        if (_collisionHandler != null)
        {
            _collisionHandler.OnHeadBump += Shake;
        }
        else
        {
            Debug.LogWarning("PlayerCollisionHandler component not found on the camera. Shake will not work.");
        }
    }

    void LateUpdate()
    {
        if (_target == null)
            return;

        // Розрахунок базової позиції
        Vector3 targetPosition = _target.position + _offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position - _shakeOffset, targetPosition, _smoothSpeed * Time.deltaTime);

        // Додаємо офсет шейкінгу до фінальної позиції
        transform.position = smoothedPosition + _shakeOffset;

        transform.LookAt(_target);
    }

    public void Shake()
    {
        Shake(_defaultShakeDuration, _defaultShakeStrength);
        ApplyZoomEffect(_defaultZoomDuration, _defaultZoomStrength);
    }

    private void ApplyZoomEffect(float duration, float strength)
    {
        _zoomSequence?.Kill();
        _zoomSequence = DOTween.Sequence();

        _zoomSequence.Append(Camera.main.DOFieldOfView(_baseFOV - strength, duration * 0.5f).SetEase(Ease.OutQuad))
                    .Append(Camera.main.DOFieldOfView(_baseFOV, duration * 0.5f).SetEase(Ease.InQuad))
                    .OnKill(() => Camera.main.fieldOfView = _baseFOV);
    }

    private void Shake(float duration, float strength)
    {
        if (_shakeTween != null && _shakeTween.IsActive())
            return;

        _shakeOffset = Vector3.zero;

        _shakeTween = DOTween.Shake(() => _shakeOffset, x => _shakeOffset = x, duration, strength, _shakeVibrato, _shakeRandomness)
            .OnComplete(() => _shakeOffset = Vector3.zero);
    }
}