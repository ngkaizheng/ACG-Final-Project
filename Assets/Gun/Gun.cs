using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.1f;
    [SerializeField]
    private float Speed = 100;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private bool BouncingBullets;
    [SerializeField]
    private float BounceDistance = 10f;

    private float LastShootTime;


    // public void Shoot()
    // {
    //     if (LastShootTime + ShootDelay < Time.time)
    //     {
    //         ShootingSystem.Play();

    //         // Get a ray from the center of the viewport (screen)
    //         Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    //         Vector3 targetPoint = ray.GetPoint(75); // 75 units away
    //         Vector3 direction = (targetPoint - BulletSpawnPoint.position).normalized;

    //         TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

    //         if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Mask))
    //         {
    //             StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, BounceDistance, true, hit));
    //         }
    //         else
    //         {
    //             StartCoroutine(SpawnTrail(trail, targetPoint, Vector3.zero, BounceDistance, false));
    //         }

    //         LastShootTime = Time.time;
    //     }
    // }
    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            ShootingSystem.Play();

            // Get mouse position in screen space
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            // Set z to distance from camera to gun (or ground plane)
            float z = Mathf.Abs(Camera.main.transform.position.y - BulletSpawnPoint.position.y);
            mouseScreenPos.z = z;

            // Convert mouse position to world position
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            // Calculate direction from gun to mouse world position
            Vector3 direction = (mouseWorldPos - BulletSpawnPoint.position).normalized;

            // Create a ray from the gun towards the mouse world position
            Ray ray = new Ray(BulletSpawnPoint.position, direction);

            TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Mask))
            {
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, BounceDistance, true, hit));
            }
            else
            {
                Vector3 targetPoint = BulletSpawnPoint.position + direction * 75f; // 75 units away
                StartCoroutine(SpawnTrail(trail, targetPoint, Vector3.zero, BounceDistance, false));
            }

            LastShootTime = Time.time;
        }
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

            if (BouncingBullets && BounceDistance > 0)
            {
                Vector3 bounceDirection = Vector3.Reflect(direction, HitNormal);

                if (Physics.Raycast(HitPoint, bounceDirection, out RaycastHit Hit, BounceDistance, Mask))
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
