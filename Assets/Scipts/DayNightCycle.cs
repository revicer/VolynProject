using UnityEngine;

namespace DefaultNamespace
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Солнце")]
        public Light sun;

        [Header("Скорость дня в градусах в секунду")]
        public float rotationSpeed = 10f;

        [Header("Интенсивность света")]
        public float maxIntensity = 1f;
        public float minIntensity = 0.1f;

        private void Update()
        {
            if (sun == null) return;

            // Вращаем солнце вокруг X оси
            sun.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

            // Меняем интенсивность для рассвета/заката
            float angle = sun.transform.eulerAngles.x;
            float intensity = Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) * (maxIntensity - minIntensity) + minIntensity;
            sun.intensity = intensity;

            // Дополнительно можно менять цвет для заката/рассвета
            sun.color = Color.Lerp(Color.red, Color.white, intensity);
        }
    }
}
