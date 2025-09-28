using UnityEngine;

namespace DefaultNamespace
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("������")]
        public Light sun;

        [Header("�������� ��� � �������� � �������")]
        public float rotationSpeed = 10f;

        [Header("������������� �����")]
        public float maxIntensity = 1f;
        public float minIntensity = 0.1f;

        private void Update()
        {
            if (sun == null) return;

            // ������� ������ ������ X ���
            sun.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

            // ������ ������������� ��� ��������/������
            float angle = sun.transform.eulerAngles.x;
            float intensity = Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) * (maxIntensity - minIntensity) + minIntensity;
            sun.intensity = intensity;

            // ������������� ����� ������ ���� ��� ������/��������
            sun.color = Color.Lerp(Color.red, Color.white, intensity);
        }
    }
}
