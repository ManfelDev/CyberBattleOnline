using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 1.0f;

    public Transform  target;

    void FixedUpdate()
    {
        if (target == null)
        {
            var player = FindObjectOfType<Player>();
            target = (player) ? (player.transform) : (null);
        }

        if (target != null)
        {
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
}