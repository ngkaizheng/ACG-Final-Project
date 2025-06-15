using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System.Collections.Generic;

public class Gun : NetworkBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private ParticleSystem ImpactParticleSystem;
    [SerializeField] private TrailRenderer BulletTrail;

    [Header("Combat Settings")]
    [SerializeField] private float ShootDelay = 0.1f;
    [SerializeField] private float spreadAngle = 5f;
    [SerializeField] private float Speed = 100;
    [SerializeField] private LayerMask HitMask;
    [SerializeField] private LayerMask BounceMask;
    [SerializeField] private float BounceDistance = 10f;
    [SerializeField] private int DamagePerShot = 10;
    [SerializeField] private float MaxDistance = 75f;

    [Networked, OnChangedRender(nameof(OnShootInfoChanged))] private ShotInfo ShootInfo { get; set; }
    [Networked] private TickTimer ShootCooldown { get; set; }
    [Networked] private NetworkBool BouncingBullets { get; set; }

    public void Shoot()
    {
        if (!ShootCooldown.ExpiredOrNotRunning(Runner)) return;

        ShootCooldown = TickTimer.CreateFromSeconds(Runner, ShootDelay);

        Vector3 direction = GetSpreadDirection();

        // Server-side damage calculation
        if (Object.HasStateAuthority)
        {
            CalculateShotDamage(direction);
        }
    }

    private void OnShootInfoChanged(NetworkBehaviourBuffer previous)
    {
        Debug.Log($"OnShootInfoChanged: {ShootInfo.Direction}, {ShootInfo.BulletSpawnPoint}");
        PlayShootingEffects(ShootInfo.Direction, ShootInfo.BulletSpawnPoint, ShootInfo.MadeImpact);
    }

    private void PlayShootingEffects(Vector3 direction, Vector3 BulletSpawnPoint, bool madeImpact)
    {
        ShootingSystem.Play();

        Ray ray = new Ray(BulletSpawnPoint, direction);
        // bool madeImpact = Physics.Raycast(ray, out var hit, MaxDistance, HitMask);
        Physics.Raycast(ray, out var hit, MaxDistance, HitMask);

        Vector3 endPoint = madeImpact ? hit.point : BulletSpawnPoint + direction * MaxDistance;
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint, Quaternion.identity);

        StartCoroutine(AnimateTrail(trail, endPoint, madeImpact ? hit.normal : Vector3.zero, madeImpact, hit));
    }

    private Vector3 GetSpreadDirection()
    {
        Quaternion spreadRotation = Quaternion.Euler(
            0,
            Random.Range(-spreadAngle, spreadAngle),
            0
        );
        return spreadRotation * BulletSpawnPoint.forward;
    }

    private void CalculateShotDamage(Vector3 direction)
    {
        Ray ray = new Ray(BulletSpawnPoint.position, direction);

        if (Physics.Raycast(ray, out var hit, MaxDistance, HitMask))
        {

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"Hit {hit.collider.gameObject.name} with damage {DamagePerShot}");
                damageable.TakeDamage(DamagePerShot, Object.InputAuthority);
            }
            ShootInfo = new ShotInfo
            {
                Direction = direction,
                BulletSpawnPoint = BulletSpawnPoint.position,
                MadeImpact = true,
                HitPosition = hit.point,
                HitNormal = hit.normal,
                HitObjectId = hit.collider.GetComponentInParent<NetworkObject>()?.Id ?? default(NetworkId)
            };
        }
        else
        {
            Vector3 missPoint = BulletSpawnPoint.position + direction * MaxDistance;
            ShootInfo = new ShotInfo
            {
                Direction = direction,
                BulletSpawnPoint = BulletSpawnPoint.position,
                MadeImpact = false
            };
        }

    }

    private IEnumerator AnimateTrail(TrailRenderer trail, Vector3 endPoint, Vector3 normal, bool madeImpact, RaycastHit hit = default)
    {
        Vector3 startPosition = trail.transform.position;
        Vector3 direction = (endPoint - startPosition).normalized;

        float distance = Vector3.Distance(startPosition, endPoint);
        float startingDistance = distance;

        while (distance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, endPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * Speed;

            yield return null;
        }

        trail.transform.position = endPoint;


        if (madeImpact)
        {
            Vector3 impactPos = endPoint + normal * 0.01f;
            Instantiate(ImpactParticleSystem, impactPos, Quaternion.LookRotation(normal), hit.transform);
            // REMOVE bounce logic and Physics.Raycast from here!
        }
        // if (madeImpact)
        // {
        //     // Instantiate impact particle system at the hit point
        //     Vector3 impactPos = endPoint + normal * 0.01f; // Offset by 0.01 units
        //     Instantiate(ImpactParticleSystem, impactPos, Quaternion.LookRotation(normal), hit.transform);

        //     if (BouncingBullets && BounceDistance > 0 && ((BounceMask.value & (1 << hit.collider.gameObject.layer)) != 0))
        //     {
        //         Vector3 bounceDirection = Vector3.Reflect(direction, normal);

        //         if (Physics.Raycast(endPoint, bounceDirection, out RaycastHit newHit, BounceDistance, HitMask))
        //         {
        //             yield return StartCoroutine(AnimateTrail(
        //                 trail,
        //                 newHit.point,
        //                 newHit.normal,
        //                 true,
        //                 newHit
        //             ));
        //         }
        //         else
        //         {
        //             yield return StartCoroutine(AnimateTrail(
        //                 trail,
        //                 endPoint + bounceDirection * BounceDistance,
        //                 Vector3.zero,
        //                 false
        //             ));
        //         }
        //     }
        // }

        Destroy(trail.gameObject, trail.time);
    }
}

public struct ShotInfo : INetworkStruct
{
    public Vector3 Direction;
    public Vector3 BulletSpawnPoint;
    public bool MadeImpact;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
    public NetworkId HitObjectId;
}