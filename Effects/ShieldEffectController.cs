using UnityEngine;

public class ShieldEffectController : MonoBehaviour
{
    private Transform firstChild;
    private Transform secondChild;
    public float rotationSpeed = 5f;

    private void Start()
    {
        firstChild = transform.GetChild(0);
        secondChild = transform.GetChild(1);
    }
    
    private void Update()
    {
        UpdateShieldRotation();
    }

    private void UpdateShieldRotation()
    {
        if (firstChild != null && secondChild != null)
        {
            // 타원의 반지름 설정
            float radiusX = 0.7f; // X축 반지름
            float radiusY = 0.2f; // Y축 반지름

            // 시간에 따라 회전 각도 계산
            float angle = Time.time * rotationSpeed;

            // 부모의 위치
            Vector3 parentPosition = transform.position;

            // 첫 번째 자식 위치 업데이트 (타원 방정식 사용)
            float x1 = parentPosition.x + Mathf.Cos(angle) * radiusX;
            float y1 = parentPosition.y + Mathf.Sin(angle) * radiusY;
            firstChild.position = new Vector3(x1, y1, 0);

            // 두 번째 자식 위치 업데이트 (대칭 위치)
            float x2 = parentPosition.x + Mathf.Cos(angle + Mathf.PI) * radiusX;
            float y2 = parentPosition.y + Mathf.Sin(angle + Mathf.PI) * radiusY;
            secondChild.position = new Vector3(x2, y2, 0);
        }
        else
        {
            Debug.LogWarning("ShieldEffectController | firstChild or secondChild is null");
        }
    }
}
