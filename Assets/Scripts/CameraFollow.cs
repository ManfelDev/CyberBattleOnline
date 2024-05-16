using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 1.0f;

    private Transform  target;

    void FixedUpdate()
    {
        if (target == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;

        Vector3 positionDifference = targetPosition - transform.position;
        
        if (Time.deltaTime > 0)
        {
            float factor = Mathf.Clamp01(Time.fixedDeltaTime / smoothSpeed);
            transform.position = transform.position + positionDifference * factor;
        }
    }
}