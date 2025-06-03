using UnityEngine;
using System.Collections.Generic;

public class PowerupManager : MonoBehaviour
{
    [System.Serializable]
    public class PowerupInfo
    {
        public string name;
        public GameObject prefab;
        public float spawnChance; // 0-1 arası
        public Sprite icon;
    }

    [SerializeField] private PowerupInfo[] availablePowerups;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float minScore = 5f;
    [SerializeField] private float minSpawnY = -3f;
    [SerializeField] private float maxSpawnY = 3f;

    private PlayerController player;
    private GameManager gameManager;
    private float nextSpawnTime;
    private List<GameObject> activePowerups = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        ResetTimer();
    }

    void Update()
    {
        if (player != null && player.IsAlive() && player.GetScore() >= minScore && Time.time >= nextSpawnTime && !isSpawning)
        {
            isSpawning = true;
            TrySpawnPowerup();
            ResetTimer();
            isSpawning = false;
        }
    }

    void TrySpawnPowerup()
    {
        float totalChance = 0;
        foreach (PowerupInfo info in availablePowerups)
        {
            totalChance += info.spawnChance;
        }

        float randomValue = Random.Range(0, totalChance);
        float cumulativeChance = 0;
        PowerupInfo selectedPowerup = null;

        foreach (PowerupInfo info in availablePowerups)
        {
            cumulativeChance += info.spawnChance;
            if (randomValue <= cumulativeChance)
            {
                selectedPowerup = info;
                break;
            }
        }

        if (selectedPowerup != null)
        {
            float spawnX = player.transform.rotation.eulerAngles.y < 90 ? gameManager.pipeXPositionRight - 3f : gameManager.pipeXPositionLeft + 3f;
            float randomY = Random.Range(minSpawnY, maxSpawnY);
            Vector3 spawnPos = new Vector3(spawnX, randomY, 0);

            while (Physics2D.OverlapCircle(spawnPos, 0.5f, LayerMask.GetMask("Obstacle")))
            {
                randomY = Random.Range(minSpawnY, maxSpawnY);
                spawnPos = new Vector3(spawnX, randomY, 0);
            }

            GameObject powerup = Instantiate(selectedPowerup.prefab, spawnPos, Quaternion.identity);
            Powerup powerupScript = powerup.GetComponent<Powerup>();
            powerupScript.Initialize(selectedPowerup.name, 0, player); // Hala 3 argüman, ama duration kullanılmıyor
            activePowerups.Add(powerup);
        }
    }

    void ResetTimer()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void ClearPowerups()
    {
        foreach (GameObject powerup in activePowerups)
        {
            if (powerup != null) Destroy(powerup);
        }
        activePowerups.Clear();
    }
}