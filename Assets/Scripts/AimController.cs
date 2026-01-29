using CustomInspector;
using DG.Tweening;
using System;
using UnityEngine;

public class AimController : MonoBehaviour
{
    [Header("General")]
    [Button(nameof(ChangeAimPreferences))]
    [SerializeField] private AimPrefs aimPrefs;

    [Header("Movement Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float stopDistance = 0.05f;

    [Header("Recoil Settings")]
    public float minRecoilDistance = 1f;
    public float maxRecoilDistance = 3f;

    [Header("Target Detection")]
    [SerializeField] private float detectionRadius = 0.5f; // радиус проверки
    [SerializeField] private LayerMask targetLayer;      // слой, на котором находятся цели

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip shootingClip;
    private void ChangeAimPreferences()
    {
        ChangeAimPrefs(aimPrefs);
    }

    private Vector2 velocity;
    private CursorController cursorController;

    private void Start()
    {
        cursorController ??= FindFirstObjectByType<CursorController>();
    }

    void Update()
    {
        Vector2 cursorWorldPos = cursorController.transform.position;
        Vector2 toTarget = cursorWorldPos - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        if (distance > stopDistance)
        {
            Vector2 desiredVelocity = toTarget.normalized * maxSpeed;
            Vector2 steering = desiredVelocity - velocity;
            steering = Vector2.ClampMagnitude(steering, acceleration * Time.deltaTime);
            velocity += steering;
        }
        else
        {
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * acceleration);
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
            audioSource.PlayOneShot(shootingClip);
        }
    }

    void Shoot()
    {
        // получаем все коллайдеры в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Target>(out Target target))
            {
                target.GetHitted();
            }
        }

        // Текущая позиция прицела
        Vector2 currentPosition = transform.position;

        // Случайное направление и расстояние
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        float randomDistance = UnityEngine.Random.Range(minRecoilDistance, maxRecoilDistance);

        Vector2 newPosition = currentPosition + randomDirection * randomDistance;
        transform.position = newPosition;

        // Обнуляем скорость, чтобы прицел не "летел" после телепорта
        velocity = Vector2.zero;
    }

    public void SetAimPrefs(AimPrefs ap)
    {
        aimPrefs = ap;

        ApplyAimPrefs();
    }

    private void ChangeAimPrefs(AimPrefs ap)
    {
        ap.acceleration = acceleration;
        ap.maxSpeed = maxSpeed;
        ap.stopDistance = stopDistance;

        ap.minRecoilDistance = minRecoilDistance;
        ap.maxRecoilDistance = maxRecoilDistance;
    }

    private void ApplyAimPrefs()
    {
        acceleration = aimPrefs.acceleration;
        maxSpeed = aimPrefs.maxSpeed;
        stopDistance = aimPrefs.stopDistance;

        minRecoilDistance = aimPrefs.minRecoilDistance;
        maxRecoilDistance = aimPrefs.maxRecoilDistance;
    }

    // Чтобы радиус был виден в сцене
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

[Serializable]
public class AimPrefs
{
    [Header("Movement Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float stopDistance = 0.05f;

    [Header("Recoil Settings")]
    public float minRecoilDistance = 1f;
    public float maxRecoilDistance = 3f;
}
