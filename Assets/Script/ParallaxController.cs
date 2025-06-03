using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private GameObject[] backgroundLayers; // Arkaplan katmanları
    [SerializeField] private float[] parallaxSpeeds; // Her katmanın hızı
    private PlayerController player;
    private Vector3[] startPositions;
    private float lastPlayerPosX;
    private bool isPlayerMovingRight;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        startPositions = new Vector3[backgroundLayers.Length];

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            startPositions[i] = backgroundLayers[i].transform.position;
        }

        lastPlayerPosX = player.transform.position.x;
        isPlayerMovingRight = true;

        PlayerController.OnDirectionChange += HandleDirectionChange;
    }

    void OnDestroy()
    {
        PlayerController.OnDirectionChange -= HandleDirectionChange;
    }

    void HandleDirectionChange(bool isMovingRight)
    {
        isPlayerMovingRight = isMovingRight;
    }

    void Update()
    {
        if (!player) return;

        float deltaX = player.transform.position.x - lastPlayerPosX;
        lastPlayerPosX = player.transform.position.x;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            float parallaxEffect = deltaX * parallaxSpeeds[i];
            float backgroundTargetPosX = backgroundLayers[i].transform.position.x + parallaxEffect;
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, startPositions[i].y, startPositions[i].z);
            backgroundLayers[i].transform.position = Vector3.Lerp(backgroundLayers[i].transform.position, backgroundTargetPos, Time.deltaTime * 5f);
        }
    }
}