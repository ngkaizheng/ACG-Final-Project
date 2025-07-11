using UnityEngine;
using Fusion;

[ExecuteAlways]
public class SunRotation : NetworkBehaviour
{
    [Header("Day/Night Cycle")]
    public float dayDurationInSeconds = 60f; // Real seconds for full cycle
    public Gradient lightColor; // Color gradient from day to night
    public float minIntensity = 0.3f;
    public float maxIntensity = 1f;

    [Range(0.05f, 0.95f)]
    public float dayPortion = 0.7f; // 0.7 = 70% day, 30% night

    [Range(0f, 360f)]
    public float dayStartAngle = 0f;    // Angle where day starts (sunrise)
    [Range(0f, 360f)]
    public float dayEndAngle = 180f;    // Angle where day ends (sunset)

    [Header("Editor")]
    public bool simulateInEditor = true; // Toggle for ExecuteAlways

    private Light sunLight;
    private float rotationAngle;
    // Networked variable to sync the sun's time across all clients
    [Networked] public float NetworkedDayTime { get; set; }

    void Start()
    {
        sunLight = GetComponent<Light>();
    }

    // void Update()
    // {
    //     if (!Application.isPlaying && !simulateInEditor)
    //         return;


    //     float dayProgress = Mathf.Repeat(Time.time / dayDurationInSeconds, 1f);

    //     float sunAngle;
    //     if (dayProgress < dayPortion)
    //     {
    //         // Daytime: interpolate from dayStartAngle to dayEndAngle
    //         float t = dayProgress / dayPortion;
    //         sunAngle = Mathf.Lerp(dayStartAngle, dayEndAngle, t);
    //     }
    //     else
    //     {
    //         // Nighttime: interpolate from dayEndAngle to dayStartAngle + 360
    //         float t = (dayProgress - dayPortion) / (1f - dayPortion);
    //         sunAngle = Mathf.Lerp(dayEndAngle, dayStartAngle + 360f, t);
    //     }

    //     transform.rotation = Quaternion.Euler(sunAngle, 170, 0);

    //     // Adjust light properties
    //     sunLight.color = lightColor.Evaluate(dayProgress);
    //     sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
    //         Mathf.Sin(dayProgress * Mathf.PI));
    // }

    public override void Render()
    {
        if (!Application.isPlaying && !simulateInEditor)
            return;

        // Only StateAuthority updates the time, others just follow
        if (Object.HasStateAuthority)
        {
            float currentTime = Mathf.Repeat(Time.time, dayDurationInSeconds);
            NetworkedDayTime = currentTime;
        }

        float dayProgress = NetworkedDayTime / dayDurationInSeconds;

        float sunAngle;
        if (dayProgress < dayPortion)
        {
            // Daytime: interpolate from dayStartAngle to dayEndAngle
            float t = dayProgress / dayPortion;
            sunAngle = Mathf.Lerp(dayStartAngle, dayEndAngle, t);
        }
        else
        {
            // Nighttime: interpolate from dayEndAngle to dayStartAngle + 360
            float t = (dayProgress - dayPortion) / (1f - dayPortion);
            sunAngle = Mathf.Lerp(dayEndAngle, dayStartAngle + 360f, t);
        }

        transform.rotation = Quaternion.Euler(sunAngle, 170, 0);

        // Adjust light properties
        sunLight.color = lightColor.Evaluate(dayProgress);
        sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
            Mathf.Sin(dayProgress * Mathf.PI));
    }
}