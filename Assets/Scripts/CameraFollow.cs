using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 0.01f;

    private Transform target;

    void Start()
    {
        target = FindLocalPlayer();
    }

    void Update()
    {
        if (target == null)
        {
            target = FindLocalPlayer();
        }

        if (target != null)
        {
            Vector3 desiredPosition = target.position;
            desiredPosition.z = transform.position.z;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    private Transform FindLocalPlayer()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.IsOwner) 
            {
                return player.transform;
            }
        }
        return null;
    }
}