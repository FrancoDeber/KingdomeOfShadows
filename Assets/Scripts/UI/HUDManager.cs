using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;


public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    // HUD
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image[] dashImages;
    [SerializeField] private Image[] healthImages;
    [SerializeField] private Image dashCooldownBar; // ← la barra que se llena cuando se recarga
    [SerializeField] private Image experienceBar; // ← la barra que se llena cuando se recarga
    [SerializeField] private GameObject bossHUD;
    [SerializeField] private Image bossHealthBar; // ← la barra que se llena cuando se recarga
    [SerializeField] private TextMeshProUGUI bossNameText;

    // Opacity
    Color fullOpacity = new Color(1f, 1f, 1f, 1f);
    Color lowOpacity = new Color(1f, 1f, 1f, 0.2f);
    Color hiddenOpacity = new Color(1f, 1f, 1f, 0f);


    // PowerUp Menu
    [SerializeField] private GameObject powerUpPanel;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI[] optionTexts;


    // Stats Menu
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI[] statsTexts;
    private Dictionary<GameObject, Vector3> panelOriginalScales = new Dictionary<GameObject, Vector3>();

    // Exit Menu
    [SerializeField] private GameObject exitPanel;
    public Button[] exitBotones; // Asigna en el Inspector
    private int exitPanelIndexActual = 0;


    private PowerUpType[] currentOptions;
    private int currentSelection = 0;
    public Player currentPlayer;

    private bool isPanelOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        powerUpPanel.SetActive(false);
        statsPanel.SetActive(false);
    }

    void Start()
    {
        currentPlayer = GameObject.FindWithTag("Player").GetComponent<Player>();
        UpdateHealthUI();
        UpdateDashUI();
    }

    public void Init()
    {
        hudCanvas.worldCamera = Camera.main;
        currentPlayer = GameObject.FindWithTag("Player").GetComponent<Player>();
        HideBossHUD();
        UpdateHealthUI();
        UpdateDashUI();
    }


    void Update()
    {

        // EXIT MENU 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventSystem.current.SetSelectedGameObject(exitBotones[0].gameObject);
            TogglePanel(exitPanel);
            return;
        }
        if (exitPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                exitPanelIndexActual = (exitPanelIndexActual + 1) % exitBotones.Length;
                SeleccionarBotonExit(exitPanelIndexActual);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                exitPanelIndexActual = (exitPanelIndexActual - 1 + exitBotones.Length) % exitBotones.Length;
                SeleccionarBotonExit(exitPanelIndexActual);
            }
            return;
        }
        
        // POWERUP MENU
        if (powerUpPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                currentSelection = (currentSelection + 2) % 3;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                currentSelection = (currentSelection + 1) % 3;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                ApplySelected();

            UpdatePowerUpVisuals();
            return; // No permitir otras entradas si el menú está abierto
        }

        // STATS MENU TOGGLE
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdateStats(currentPlayer);
            TogglePanel(statsPanel);
        }


        

        
    }

    void SeleccionarBotonExit(int index)
    {
        EventSystem.current.SetSelectedGameObject(null); // limpia selección anterior
        EventSystem.current.SetSelectedGameObject(exitBotones[index].gameObject);
    }

    void TogglePanel(GameObject panel)
    {
        bool isOpen = panel.activeSelf;

        if (isOpen)
        {
            StartCoroutine(AnimatePanelClose(panel));
        }
        else if(!isPanelOpen)
        {

            StartCoroutine(AnimatePanelOpen(panel));
        }
    }


    public void ShowPowerUpMenu(PowerUpType[] options, Player player, Boolean levelUp = false)
    {
        currentOptions = options;
        currentPlayer = player;
        currentSelection = 0;
        if (levelUp)
        {
            levelNumberText.gameObject.SetActive(true);
            levelNumberText.text = "Level " + currentPlayer.level.ToString();
        }

        for (int i = 0; i < 3; i++)

            optionTexts[i].text = PowerUpInfo.Data[options[i]].diplayName;

        UpdatePowerUpVisuals();
        StartCoroutine(AnimatePanelOpen(powerUpPanel));
    }

    void UpdatePowerUpVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            optionTexts[i].color = (i == currentSelection) ? Color.yellow : Color.white;
        }
    }

    void ApplySelected()
    {
        currentPlayer.ApplyPowerUp(currentOptions[currentSelection]);
        currentPlayer.IncrementarStat(StatType.PowerUps, 1);
        levelNumberText.gameObject.SetActive(false);
        StartCoroutine(AnimatePanelClose(powerUpPanel));
    }

    public void UpdateStats(Player player)
    {
        statsTexts[0].text = $"{player.lifes}";
        statsTexts[1].text = $"{player.maxHealth}";
        statsTexts[2].text = $"{player.invulnerabilityTime}";
        statsTexts[3].text = $"{player.knockbackResistance}";
        statsTexts[4].text = $"{player.moveSpeed}";
        statsTexts[5].text = $"{player.maxDashCount}";
        statsTexts[6].text = $"{player.dashSpeed}";
        statsTexts[7].text = $"{player.dashDistance}";
        statsTexts[8].text = $"{player.dashRechargeTime}";
        statsTexts[9].text = $"{player.damage}";
        statsTexts[10].text = $"{player.shotRange}";
        statsTexts[11].text = $"{player.shotSpeed}";
        statsTexts[12].text = $"{player.shotSize}";
        statsTexts[13].text = $"{player.shotRate}";
        statsTexts[14].text = $"{player.shotKnockback}";
        statsTexts[15].text = $"{player.bossKills}";
        statsTexts[16].text = $"{player.kills}";
        statsTexts[17].text = $"{player.damageDone}";
        statsTexts[18].text = $"{player.shotsFired}";
        statsTexts[19].text = $"{player.shotsHit}";
        statsTexts[20].text = $"{player.dashesDone}";
        statsTexts[21].text = $"{player.damageReceived}";
        statsTexts[22].text = $"{player.healReceived}";
        statsTexts[23].text = $"{player.powerUpsTaken}";
        statsTexts[24].text = $"{player.deaths}";
        statsTexts[25].text = $"{GameManager.Instance.GetPlayedTime()}";
    }

    public void RegisterPlayer(Player player)
    {
        currentPlayer = player;
    }

    public bool IsAnyMenuOpen()
    {
        return isPanelOpen;
    }

    public IEnumerator AnimatePanelOpen(GameObject panel)
    {
        while (GameManager.Instance.cutscene)
        {
            yield return null; // Espera un frame y vuelve a comprobar
        }
        Time.timeScale = 0f;
        if (!panelOriginalScales.ContainsKey(panel))
            panelOriginalScales[panel] = panel.transform.localScale; // Guarda la escala original

        Vector3 targetScale = panelOriginalScales[panel];
        panel.SetActive(true);
        panel.transform.localScale = Vector3.zero;

        float duration = 0.2f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            panel.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
        panel.transform.localScale = targetScale;
    }

    public IEnumerator AnimatePanelClose(GameObject panel)
    {
        float duration = 0.2f;
        float time = 0f;
        Vector3 initialScale = panel.transform.localScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            panel.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }

        panel.SetActive(false);
        panel.transform.localScale = Vector3.one;
        Time.timeScale = 1f;

    }


    public void UpdateHealthUI()
    {
        int maxHealth = currentPlayer.maxHealth;
        int currentHealth = currentPlayer.currentHealth;
        for (int i = 0; i < healthImages.Length; i++)
        {
            if (i < maxHealth) // Dentro del maxHealth
            {
                if (i < currentHealth)
                {
                    healthImages[i].color = fullOpacity;
                }
                else
                {
                    healthImages[i].color = lowOpacity;
                }
            }
            else
            {
                healthImages[i].color = hiddenOpacity;
            }
        }
    }

    public void UpdateDashUI()
    {
        int currentDashCount = currentPlayer.currentDashCount;
        int maxDashCount = currentPlayer.maxDashCount;
        for (int i = 0; i < dashImages.Length; i++)
        {
            if (i < maxDashCount)
            {
                if (i < currentDashCount)
                {
                    dashImages[i].color = fullOpacity;
                }
                else
                {
                    dashImages[i].color = lowOpacity;
                }
            }
            else
            {
                dashImages[i].color = hiddenOpacity;
            }
        }
    }

    public IEnumerator AnimateDashCooldownBar(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            dashCooldownBar.fillAmount = fill;
            yield return null;
        }

        dashCooldownBar.fillAmount = 1f; // Asegurar que quede llena

        // Esperar un frame para asegurar que se vea llena, luego vaciar
        yield return new WaitForEndOfFrame();
        dashCooldownBar.fillAmount = 0f;
    }

    public void AnimateExpBar(float targetFill)
    {
        experienceBar.fillAmount = targetFill;
    }

    public void HideHUD()
    {
        if (hudCanvas != null)
            hudCanvas.enabled = false;
    }

    public void ShowHUD()
    {
        if (hudCanvas != null)
            hudCanvas.enabled = true;
    }

    public void HideBossHUD()
    {
        if (bossHUD != null)
            bossHUD.SetActive(false);
    }

    public void ShowBossHUD()
    {
        if (bossHUD != null)
            bossHUD.SetActive(true);
    }

    public void SetBossName(string bossName)
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }


    public void AnimateBossHealthBar(float targetFill)
    {
        bossHealthBar.fillAmount = targetFill;
    }

    public void OnReanudar()
    {
        StartCoroutine(AnimatePanelClose(exitPanel));
    }

    public void OnSalirButton()
    {
        Destroy(AudioManagerLevels.Instance.gameObject);
        GameManager.Instance.BackToMainMenu();
    }
}
