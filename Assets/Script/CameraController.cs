using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float lookAheadFactor = 1.5f;

    private PlayerController player;
    private Vector3 currentVelocity;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (target == null) target = player.transform;
        PlayerController.OnDirectionChange += HandleDirectionChange;
    }

    void OnDestroy()
    {
        PlayerController.OnDirectionChange -= HandleDirectionChange;
    }

    void HandleDirectionChange(bool isMovingRight)
    {
        // Yön değişiminde kamera hafif öne kayar
    }

    void LateUpdate()
    {
        if (target == null) return;

        float directionX = player.transform.rotation.eulerAngles.y > 90 ? -1 : 1;
        Vector3 targetPos = target.position + offset + new Vector3(directionX * lookAheadFactor, 0, 0);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }
}