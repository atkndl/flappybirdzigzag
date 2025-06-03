using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private GameObject scoreFeedbackPrefab; // +1 efekti prefab'ı
    [SerializeField] private ParticleSystem flapParticles; // Zıplama parçacıkları
    [SerializeField] private ParticleSystem directionChangeParticles; // Yön değiştirme parçacıkları
    [SerializeField] private ParticleSystem deathParticles; // Ölüm parçacıkları
    private PlayerController player;
    private int lastScore = 0;
    private Camera mainCamera;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;

        PlayerController.OnDirectionChange += HandleDirectionChange;
    }

    void OnDestroy()
    {
        PlayerController.OnDirectionChange -= HandleDirectionChange;
    }

    void Update()
    {
        transform.position = player.transform.position;

        if (player.GetScore() > lastScore)
        {
            ShowScoreFeedback();
            lastScore = player.GetScore();
        }
    }

    public void PlayFlapParticles()
    {
        if (flapParticles != null)
            flapParticles.Play();
    }

    void HandleDirectionChange(bool isMovingRight)
    {
        if (directionChangeParticles != null)
        {
            directionChangeParticles.transform.rotation = Quaternion.Euler(0, isMovingRight ? 0 : 180, 0);
            directionChangeParticles.Play();
        }
    }

    public void PlayDeathParticles()
    {
        if (deathParticles != null)
            deathParticles.Play();
    }

    void ShowScoreFeedback()
    {
        if (scoreFeedbackPrefab != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(player.transform.position + Vector3.up);
            GameObject feedback = Instantiate(scoreFeedbackPrefab, screenPos, Quaternion.identity, GameObject.Find("Canvas").transform);
            Destroy(feedback, 1f);
        }
    }
}