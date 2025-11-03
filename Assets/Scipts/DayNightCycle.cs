using UnityEngine;

namespace DefaultNamespace
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Sun")]
        public Light sun;

        [Header("Day speed in degrees per second")]
        public float rotationSpeed = 10f;

        [Header("Light intensity")]
        public float maxIntensity = 1f;
        public float minIntensity = 0.1f;

        private void Update()
        {
            if (sun == null) return;

            // Rotate the sun around the X axis
            sun.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

            // Change the intensity for sunrise/sunset
            float angle = sun.transform.eulerAngles.x;
            float intensity = Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) * (maxIntensity - minIntensity) + minIntensity;
            sun.intensity = intensity;

            // Change the sunset/sunrise color
            sun.color = Color.Lerp(Color.red, Color.white, intensity);
        }
    }
}
