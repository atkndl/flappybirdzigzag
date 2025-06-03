using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject audioSettingsPanel; // Ses ayar paneli
    [SerializeField] private Slider themesongSlider; // Themesong ses seviyesi slider'ı
    [SerializeField] private Slider effectsSlider; // Efekt ses seviyesi slider'ı
    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null) Debug.LogError("PlayerController bulunamadı!");

        // Varsayılan ses seviyelerini yükle
        themesongSlider.value = PlayerPrefs.GetFloat("ThemesongVolume", 0.5f);
        effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 1f);

        // Slider'lara dinleyici ekle
        themesongSlider.onValueChanged.AddListener(SetThemesongVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectsVolume);

        // Paneli başlangıçta kapat
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.SetActive(true);
            Debug.Log("AudioSettingsPanel açıldı.");
        }
    }

    public void ClosePanel() // Metodu public yapıyoruz
    {
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.SetActive(false);
            Debug.Log("AudioSettingsPanel kapatıldı.");
        }
    }

    private void SetThemesongVolume(float volume)
    {
        PlayerPrefs.SetFloat("ThemesongVolume", volume);
        PlayerPrefs.Save();
        player?.SetThemesongVolume(volume);
    }

    private void SetEffectsVolume(float volume)
    {
        PlayerPrefs.SetFloat("EffectsVolume", volume);
        PlayerPrefs.Save();
        player?.SetEffectsVolume(volume);
    }
}