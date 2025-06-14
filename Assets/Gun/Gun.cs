using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

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

    [Networked] private TickTimer ShootCooldown { get; set; }
    [Networked] private NetworkBool BouncingBullets { get; set; }

    public void Shoot()
    {
        if (!ShootCooldown.ExpiredOrNotRunning(Runner)) return;

        ShootCooldown = TickTimer.CreateFromSeconds(Runner, ShootDelay);

        Vector3 direction = GetSpreadDirection();
        // Local visual effects
        // PlayShootingEffects(direction);

        // Server-side damage calculation
        // if (Object.HasStateAuthority)
        // {
        CalculateShotDamage(direction);
        // }

        // ShootingSystem.Play();

        // Vector3 direction = BulletSpawnPoint.forward;

        // // Add random spread (in degrees)
        // Quaternion spreadRotation = Quaternion.Euler(
        //     0,
        //     Random.Range(-spreadAngle, spreadAngle),
        //     0
        // );
        // direction = spreadRotation * direction;

        // Ray ray = new Ray(BulletSpawnPoint.position, direction);

        // TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

        // if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, HitMask))
        // {
        //     StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, BounceDistance, true, hit));
        // }
        // else
        // {
        //     Vector3 targetPoint = BulletSpawnPoint.position + direction * 75f; // 75 units away
        //     StartCoroutine(SpawnTrail(trail, targetPoint, Vector3.zero, BounceDistance, false));
        // }
    }

    // private void PlayShootingEffects(Vector3 direction)
    // {
    //     ShootingSystem.Play();
    //     StartCoroutine(RenderBulletTrail(direction));
    // }
    private void PlayShootingEffects(Vector3 direction, Vector3 endPoint, Vector3 normal, bool madeImpact, RaycastHit hit)
    {
        ShootingSystem.Play();
        StartCoroutine(RenderBulletTrail(direction, endPoint, normal, madeImpact, hit));
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

    private IEnumerator RenderBulletTrail(Vector3 direction, Vector3 endPoint, Vector3 normal, bool madeImpact, RaycastHit hit)
    {
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
        yield return StartCoroutine(AnimateTrail(trail, endPoint, normal, madeImpact, hit));
    }

    // private IEnumerator RenderBulletTrail(Vector3 direction)
    // {
    //     // Your existing trail rendering logic
    //     Ray ray = new Ray(BulletSpawnPoint.position, direction);
    //     bool madeImpact = Physics.Raycast(ray, out var hit, float.MaxValue, HitMask);

    //     Vector3 endPoint = madeImpact ? hit.point : BulletSpawnPoint.position + direction * 75f;
    //     TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

    //     yield return StartCoroutine(AnimateTrail(trail, endPoint, madeImpact ? hit.normal : Vector3.zero, madeImpact, hit));
    // }

    private void CalculateShotDamage(Vector3 direction)
    {
        Ray ray = new Ray(BulletSpawnPoint.position, direction);

        // if (Runner.LagCompensation.Raycast(1
        //     ray.origin,
        //     ray.direction,
        //     float.MaxValue,
        //     Object.InputAuthority,
        //     out var hit,
        //     HitMask))
        // {
        //     // Try to get IDamageable from the hit object or its parents
        //     var damageable = hit.GameObject.GetComponentInParent<IDamageable>();
        //     if (damageable != null)
        //     {
        //         Debug.Log($"Hit {hit.GameObject.name} with damage {DamagePerShot}");
        //         damageable.TakeDamage(DamagePerShot, Object.InputAuthority);
        //     }
        //     PlayShootingEffects(direction, hit.Point, hit.Normal, true, hit);
        // }
        // else
        // {
        //     // No hit, use a far point
        //     Vector3 missPoint = BulletSpawnPoint.position + direction * 75f;
        //     PlayShootingEffects(direction, missPoint, Vector3.zero, false, default);
        // }
        if (Physics.Raycast(ray, out var hit, float.MaxValue, HitMask))
        {
            if (Object.HasStateAuthority)
            {
                // Try to get IDamageable from the hit object or its parents
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    Debug.Log($"Hit {hit.collider.gameObject.name} with damage {DamagePerShot}");
                    damageable.TakeDamage(DamagePerShot, Object.InputAuthority);
                }
            }
            PlayShootingEffects(direction, hit.point, hit.normal, true, hit);
        }
        else
        {
            // No hit, use a far point
            Vector3 missPoint = BulletSpawnPoint.position + direction * 75f;
            PlayShootingEffects(direction, missPoint, Vector3.zero, false, default);
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

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, float BounceDistance, bool MadeImpact, RaycastHit hit = default)
    {
        Vector3 startPosition = Trail.transform.position;
        Vector3 direction = (HitPoint - Trail.transform.position).normalized;

        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float startingDistance = distance;

        while (distance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * Speed;

            yield return null;
        }

        Trail.transform.position = HitPoint;

        if (MadeImpact)
        {
            // Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal), hit.transform);
            Vector3 impactPos = HitPoint + HitNormal * 0.01f; // Offset by 0.01 units
            Instantiate(ImpactParticleSystem, impactPos, Quaternion.LookRotation(HitNormal), hit.transform);

            if (BouncingBullets && BounceDistance > 0 && ((BounceMask.value & (1 << hit.collider.gameObject.layer)) != 0))
            {
                Vector3 bounceDirection = Vector3.Reflect(direction, HitNormal);

                if (Physics.Raycast(HitPoint, bounceDirection, out RaycastHit Hit, BounceDistance, HitMask))
                {
                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        Hit.point,
                        Hit.normal,
                        BounceDistance - Vector3.Distance(Hit.point, HitPoint),
                        true
                    ));
                }
                else
                {
                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        HitPoint + bounceDirection * BounceDistance,
                        Vector3.zero,
                        0,
                        false
                    ));
                }
            }
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
