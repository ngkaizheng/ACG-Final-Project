// using UnityEngine;

// public class SunRotation : MonoBehaviour
// {
//     [Header("Day/Night Cycle")]
//     public float dayDurationInSeconds = 60f; // Real seconds for full cycle
//     public Gradient lightColor; // Color gradient from day to night
//     public float minIntensity = 0.3f;
//     public float maxIntensity = 1f;

//     private Light sunLight;
//     private float rotationAngle;

//     void Start()
//     {
//         sunLight = GetComponent<Light>();
//     }

//     void Update()
//     {
//         // Calculate progress through day (0-1)
//         float dayProgress = Mathf.Repeat(Time.time / dayDurationInSeconds, 1f);

//         // Rotate 360 degrees based on day progress
//         transform.rotation = Quaternion.Euler(
//             Mathf.Lerp(0, 360, dayProgress),
//             170, // Standard Y rotation for directional light
//             0
//         );

//         // Adjust light properties
//         sunLight.color = lightColor.Evaluate(dayProgress);
//         sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
//             Mathf.Sin(dayProgress * Mathf.PI)); // Brighter at midday
//     }
// }