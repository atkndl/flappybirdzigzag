using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject firstPipeTemplate; // Skor 0 için kullanılacak sabit boru (geniş boşluklu)
    [SerializeField] private GameObject[] easyPipeTemplates; // Skor 2-10 için kolay borular
    [SerializeField] private GameObject[] allPipeTemplates; // Skor 11+ için tüm borular (kolay ve zor)
    public float pipeXPositionRight = 8f; // Sağ kenardaki boruların X pozisyonu
    public float pipeXPositionLeft = -8f; // Sol kenardaki boruların X pozisyonu
    public float spawnOffset = 2f; // Ekran dışı başlangıç offset'i
    public float slideDuration = 0.5f; // Kayma animasyonunun süresi (saniye cinsinden)
    private GameObject currentPipe; // Şu anki aktif boru
    private PlayerController player;
    private bool spawnOnRight = true; // Başlangıçta sağdan spawn
    private bool isDespawning = false; // Boru yok edilirken çakışmayı önlemek için
    private int pipeSpawnCount = 0; // Spawn edilen pipe sayısını takip etmek için

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController not found!");
            return;
        }
        if (firstPipeTemplate == null || easyPipeTemplates == null || allPipeTemplates == null)
        {
            Debug.LogWarning("Pipe templates are not assigned in the Inspector!");
        }

        SpawnPipe(spawnOnRight); // İlk boruyu sağda spawnla

        // Yön değişim olayına abone ol
        PlayerController.OnDirectionChange += HandleDirectionChange;

        Debug.Log("GameManager Start: Initial pipe spawned");
    }

    void OnDestroy()
    {
        // Aboneliği kaldır
        PlayerController.OnDirectionChange -= HandleDirectionChange;
    }

    void HandleDirectionChange(bool isMovingRight)
    {
        Debug.Log("HandleDirectionChange called with isMovingRight: " + isMovingRight);
        // Yön değiştiğinde mevcut boruyu animasyonla yok et ve yeni boruyu spawnla
        if (currentPipe != null)
        {
            StartCoroutine(DespawnPipe(currentPipe, spawnOnRight));
        }
        spawnOnRight = isMovingRight; // Kuşun hareket yönüne göre spawn yönünü ayarla
        if (player.IsAlive())
        {
            SpawnPipe(spawnOnRight);
        }
    }

    public void SpawnPipe(bool onRight)
    {
        if (isDespawning)
        {
            Debug.LogWarning("SpawnPipe: Waiting for despawn to complete");
            return; // Eğer boru yok ediliyorsa, yeni boru spawnlama
        }

        pipeSpawnCount++; // Her spawn'da sayacı artır

        // Skoru al
        int currentScore = player.GetScore();

        // Skora göre boru template'ini seç
        GameObject selectedTemplate;
        if (pipeSpawnCount == 1)
        {
            // İlk pipe her zaman firstPipeTemplate olacak
            selectedTemplate = firstPipeTemplate;
            Debug.Log("1. pipe seçildi: firstPipeTemplate");
        }
        else if (currentScore >= 2 && currentScore <= 10)
        {
            // Skor 2-10 arasında ise kolay borular
            int templateIndex = Random.Range(0, easyPipeTemplates.Length);
            selectedTemplate = easyPipeTemplates[templateIndex];
            Debug.Log($"Skor {currentScore}: Kolay pipe seçildi: easyPipeTemplates[{templateIndex}]");
        }
        else
        {
            // Skor 11 ve sonrası için tüm borular
            int templateIndex = Random.Range(0, allPipeTemplates.Length);
            selectedTemplate = allPipeTemplates[templateIndex];
            Debug.Log($"Skor {currentScore}: Tüm pipe'lar arasından seçildi: allPipeTemplates[{templateIndex}]");
        }

        if (selectedTemplate == null)
        {
            Debug.LogError("Selected pipe template is null! Check your template assignments.");
            return;
        }

        // Başlangıç pozisyonunu belirle (ekranın dışı)
        float startX = onRight ? pipeXPositionRight + spawnOffset : pipeXPositionLeft - spawnOffset;
        float targetX = onRight ? pipeXPositionRight : pipeXPositionLeft;

        // Template'i başlangıç pozisyonunda spawnla
        currentPipe = Instantiate(selectedTemplate, new Vector3(startX, 0, 0), Quaternion.identity);

        // Pipe'ın ölçeğini kontrol et ve logla
        Vector3 originalScale = selectedTemplate.transform.localScale;
        currentPipe.transform.localScale = originalScale;
        Debug.Log($"Pipe spawned with scale: {currentPipe.transform.localScale}, Original scale: {originalScale}");

        // Kayma animasyonunu başlat
        StartCoroutine(SlidePipe(currentPipe, startX, targetX));

        Debug.Log($"Pipe spawned at X: {targetX}, Template: {selectedTemplate.name}, Score: {currentScore}, PipeSpawnCount: {pipeSpawnCount}");
    }

    public void ResetPipes()
    {
        StartCoroutine(ResetPipesCoroutine());
    }

    private IEnumerator ResetPipesCoroutine()
    {
        // Mevcut boruyu yok et
        if (currentPipe != null)
        {
            isDespawning = true;
            yield return StartCoroutine(DespawnPipe(currentPipe, spawnOnRight));
            currentPipe = null; // Referansı sıfırla
            isDespawning = false;
        }

        // Pipe sayacını sıfırla
        pipeSpawnCount = 0;

        // Yeni boruyu sağda spawnla
        spawnOnRight = true;
        SpawnPipe(spawnOnRight);

        Debug.Log("ResetPipes: Pipes reset, new pipe spawned on the right, pipeSpawnCount sıfırlandı.");
    }

    // Boruyu kaydırarak spawnlama animasyonu
    private IEnumerator SlidePipe(GameObject pipe, float startX, float targetX)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = new Vector3(startX, pipe.transform.position.y, 0);
        Vector3 targetPosition = new Vector3(targetX, pipe.transform.position.y, 0);

        // Animasyon sırasında ölçeği koru
        Vector3 originalScale = pipe.transform.localScale;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            pipe.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            pipe.transform.localScale = originalScale; // Ölçeği koru
            yield return null;
        }

        // Animasyon tamamlandığında boruyu tam hedef pozisyona sabitle
        pipe.transform.position = targetPosition;
        pipe.transform.localScale = originalScale; // Ölçeği tekrar koru
    }

    // Boruyu kaydırarak yok etme animasyonu
    private IEnumerator DespawnPipe(GameObject pipe, bool wasOnRight)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = pipe.transform.position;
        float targetX = wasOnRight ? pipeXPositionRight + spawnOffset : pipeXPositionLeft - spawnOffset;
        Vector3 targetPosition = new Vector3(targetX, pipe.transform.position.y, 0);

        // Animasyon sırasında ölçeği koru
        Vector3 originalScale = pipe.transform.localScale;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            pipe.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            pipe.transform.localScale = originalScale; // Ölçeği koru
            yield return null;
        }

        // Animasyon tamamlandığında boruyu yok et
        Destroy(pipe);
        Debug.Log("DespawnPipe: Pipe destroyed");
    }
}