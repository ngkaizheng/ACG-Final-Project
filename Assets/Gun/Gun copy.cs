// using System.Collections;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class Gun : MonoBehaviour
// {
//     [SerializeField]
//     private ParticleSystem ShootingSystem;
//     [SerializeField]
//     private Transform BulletSpawnPoint;
//     [SerializeField]
//     private ParticleSystem ImpactParticleSystem;
//     [SerializeField]
//     private TrailRenderer BulletTrail;
//     [SerializeField]
//     private float ShootDelay = 0.1f;
//     [SerializeField]
//     private float spreadAngle = 5f; // Spread angle in degrees
//     [SerializeField]
//     private float Speed = 100;
//     [SerializeField]
//     private LayerMask HitMask;
//     [SerializeField]
//     private LayerMask BounceMask;
//     private bool BouncingBullets;
//     [SerializeField]
//     private float BounceDistance = 10f;

//     private float LastShootTime;

//     public void Shoot()
//     {
//         if (LastShootTime + ShootDelay < Time.time)
//         {
//             ShootingSystem.Play();

//             Vector3 direction = BulletSpawnPoint.forward;

//             // Add random spread (in degrees)
//             Quaternion spreadRotation = Quaternion.Euler(
//                 0,
//                 Random.Range(-spreadAngle, spreadAngle),
//                 0
//             );
//             direction = spreadRotation * direction;

//             Ray ray = new Ray(BulletSpawnPoint.position, direction);

//             TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

//             if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, HitMask))
//             {
//                 StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, BounceDistance, true, hit));
//             }
//             else
//             {
//                 Vector3 targetPoint = BulletSpawnPoint.position + direction * 75f; // 75 units away
//                 StartCoroutine(SpawnTrail(trail, targetPoint, Vector3.zero, BounceDistance, false));
//             }

//             LastShootTime = Time.time;
//         }
//     }

//     private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, float BounceDistance, bool MadeImpact, RaycastHit hit = default)
//     {
//         Vector3 startPosition = Trail.transform.position;
//         Vector3 direction = (HitPoint - Trail.transform.position).normalized;

//         float distance = Vector3.Distance(Trail.transform.position, HitPoint);
//         float startingDistance = distance;

//         while (distance > 0)
//         {
//             Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (distance / startingDistance));
//             distance -= Time.deltaTime * Speed;

//             yield return null;
//         }

//         Trail.transform.position = HitPoint;

//         if (MadeImpact)
//         {
//             // Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal), hit.transform);
//             Vector3 impactPos = HitPoint + HitNormal * 0.01f; // Offset by 0.01 units
//             Instantiate(ImpactParticleSystem, impactPos, Quaternion.LookRotation(HitNormal), hit.transform);

//             if (BouncingBullets && BounceDistance > 0 && ((BounceMask.value & (1 << hit.collider.gameObject.layer)) != 0))
//             {
//                 Vector3 bounceDirection = Vector3.Reflect(direction, HitNormal);

//                 if (Physics.Raycast(HitPoint, bounceDirection, out RaycastHit Hit, BounceDistance, HitMask))
//                 {
//                     yield return StartCoroutine(SpawnTrail(
//                         Trail,
//                         Hit.point,
//                         Hit.normal,
//                         BounceDistance - Vector3.Distance(Hit.point, HitPoint),
//                         true
//                     ));
//                 }
//                 else
//                 {
//                     yield return StartCoroutine(SpawnTrail(
//                         Trail,
//                         HitPoint + bounceDirection * BounceDistance,
//                         Vector3.zero,
//                         0,
//                         false
//                     ));
//                 }
//             }
//         }

//         Destroy(Trail.gameObject, Trail.time);
//     }
// }
