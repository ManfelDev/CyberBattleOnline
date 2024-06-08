using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 0.01f;

    private Transform localPlayer;

    void Start()
    {
        localPlayer = FindLocalPlayer();
    }

    void Update()
    {
        if (localPlayer == null)
        {
            localPlayer = FindLocalPlayer();
        }

        if (localPlayer != null)
        {
            Vector3 desiredPosition = localPlayer.position;
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
            if (player.IsLocalPlayer) 
            {
                return player.transform;
            }
        }
        return null;
    }
}