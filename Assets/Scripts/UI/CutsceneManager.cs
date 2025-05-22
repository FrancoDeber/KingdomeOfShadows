using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;
    public bool changingScene = true;
    // Canvas CinematicsBars
    [SerializeField] private Canvas canvasCinematicBars;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float barHeight = 60f;
    [SerializeField] private float animationDuration = 0.2f;
    // Canvas BlackPanel
    [SerializeField] private Canvas canvasBlackPanel;
    [SerializeField] private Image panelNegro;
    [SerializeField] private TextMeshProUGUI textoNivel;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI pressEnterText;
    // Levels name
    private String[] levelsName = { "Dark Forest", "Burning Desert", "Dark Wastelands", "Frozen Tundra", "Catacombs", "Castle" };
    private string levelName = "LevelName";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Init()
    {
        canvasBlackPanel.worldCamera = Camera.main;
        canvasCinematicBars.worldCamera = Camera.main;
    }

    public void InitLevel()
    {
        levelName = MapGenerator.Instance.levelName;
        StartCoroutine(IntoCutsceneSequence());
    }

    public IEnumerator IntoCutsceneSequence()
    {
        // Asegurarse de que todo esté visible al inicio
        canvasBlackPanel.gameObject.SetActive(true);
        panelNegro.gameObject.SetActive(true);
        textoNivel.gameObject.SetActive(true);
        SetAlpha(panelNegro, 1f);
        GameManager.Instance.cutscene = true;
        CameraFollowVertical.Instance.enableCameraFollow = false;
        StartCoroutine(ShowCinematicBars());

        textoNivel.text = "Level " + GameManager.Instance.levelActual.ToString() + "\n" + levelsName[GameManager.Instance.levelActual - 1];

        // Aparece el texto
        StartCoroutine(FadeText(textoNivel, 0f, 1f, 2f));

        // Espera inicial
        yield return new WaitForSeconds(1f);

        // Desvanecer fondo negro
        yield return FadeImage(panelNegro, 1f, 0f, 2f);

        // Espera un poco antes de quitar el texto
        //yield return new WaitForSeconds(waitAfterFade);

        // Desvanecer texto
        yield return FadeText(textoNivel, 1f, 0f, 1f);

        yield return StartCoroutine(HideCinematicBars());
        textoNivel.gameObject.SetActive(false);
        panelNegro.gameObject.SetActive(false);
        canvasBlackPanel.gameObject.SetActive(false);
        GameManager.Instance.cutscene = false;
        CameraFollowVertical.Instance.enableCameraFollow = true;
    }


    public IEnumerator EndMapCutsceneSequence()
    {
        StartCoroutine(ShowCinematicBars());
        GameManager.Instance.cutscene = true;
        CameraFollowVertical.Instance.enableCameraFollow = false;
        canvasBlackPanel.gameObject.SetActive(true);
        panelNegro.gameObject.SetActive(true);

        Color color = panelNegro.color;
        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / 1f);
            panelNegro.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        panelNegro.color = new Color(color.r, color.g, color.b, 1f); // Asegura opacidad completa

    }

    public IEnumerator ShowCinematicBars()
    {
        canvasCinematicBars.gameObject.SetActive(true);
        HUDManager.Instance.HideHUD();
        float elapsed = 0f;
        Vector2 topTarget = new Vector2(0, 0);
        Vector2 bottomTarget = new Vector2(0, 0);

        Vector2 topStart = new Vector2(0, barHeight);
        Vector2 bottomStart = new Vector2(0, -barHeight);

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            topBar.anchoredPosition = Vector2.Lerp(topStart, topTarget, t);
            bottomBar.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, t);
            yield return null;
        }
        topBar.anchoredPosition = topTarget;
        bottomBar.anchoredPosition = bottomTarget;
    }
    

    public IEnumerator HideCinematicBars()
    {
        float elapsed = 0f;
        Vector2 topStart = topBar.anchoredPosition;
        Vector2 bottomStart = bottomBar.anchoredPosition;
        Vector2 topTarget = new Vector2(0, barHeight);
        Vector2 bottomTarget = new Vector2(0, -barHeight);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            topBar.anchoredPosition = Vector2.Lerp(topStart, topTarget, t);
            bottomBar.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, t);
            yield return null;
        }

        topBar.anchoredPosition = topTarget;
        bottomBar.anchoredPosition = bottomTarget;
        HUDManager.Instance.ShowHUD();
        canvasCinematicBars.gameObject.SetActive(false);
    }

    public IEnumerator BossEnterCutscene()
    {
        GameManager.Instance.cutscene = true;
        CameraFollowVertical.Instance.enableCameraFollow = false;

        // Hago Invencible e intangible al Player 
        Player.Instance.SetInvulnerable(true);
        
        StartCoroutine(ShowCinematicBars());
        // Iniciar movimiento automático hacia target
        StartCoroutine(Player.Instance.MovePlayerToTarget(BossZoneTrigger.Instance.targetPlayerPos.transform.position, 1.5f));
        
        yield return new WaitForSeconds(5f);

        StartCoroutine(HideCinematicBars());

        // Saco Invencible e intangible al Player 
        Player.Instance.SetInvulnerable(false);

        HUDManager.Instance.ShowBossHUD();
        GameManager.Instance.cutscene = false;
        
        yield return null;
    }

    public IEnumerator BossDeathCutscene(){
        GameManager.Instance.cutscene = true;
        yield return StartCoroutine(ShowCinematicBars());
        HUDManager.Instance.HideBossHUD();
        yield return new WaitForSeconds(5f);
        BossZoneTrigger.Instance.activateLimits(false);
        yield return StartCoroutine(CutsceneManager.Instance.HideCinematicBars());
        BossZoneTrigger.Instance.bossIsAlive = false;
        CameraFollowVertical.Instance.enableCameraFollow = true;
        GameManager.Instance.cutscene = false;
    }

    public IEnumerator DeathCutSceneSequence(string deathTextContent, string pressEnterTextContent)
    {
        canvasBlackPanel.gameObject.SetActive(true);
        panelNegro.gameObject.SetActive(true);
        GameManager.Instance.cutscene = true;
        CameraFollowVertical.Instance.enableCameraFollow = false;
        deathText.text = deathTextContent;
        pressEnterText.text = pressEnterTextContent;
        deathText.gameObject.SetActive(true);


        Color panelColor = panelNegro.color;
        Color textColor = deathText.color;
        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / 1f);
            panelNegro.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            deathText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }

        panelNegro.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1f); // Asegura opacidad completa
        deathText.color = new Color(textColor.r, textColor.g, textColor.b, 1f); // Asegura opacidad completa

        yield return new WaitForSecondsRealtime(2f);
        pressEnterText.gameObject.SetActive(true);
        // Inicia la corutina de parpadeo
        Coroutine parpadeo = StartCoroutine(FadeInOutTexto());
        // Espera hasta que se presione Enter (Return)
        while (!Input.GetKeyDown(KeyCode.Return))
        {

            yield return null; // Espera un frame
        }
        // Cuando se presione Enter, detiene el parpadeo y asegura que el texto se oculte
        StopCoroutine(parpadeo);
        deathText.gameObject.SetActive(false);
        pressEnterText.gameObject.SetActive(false);
        //Dejo pressEnterText de nuevo visible
        Color finalColor = pressEnterText.color;
        finalColor.a = 0f;
        pressEnterText.color = finalColor;

        GameManager.Instance.cutscene = false;
    }

    public IEnumerator ExitSceneSequence(String scene)
    {
        changingScene = true;
        canvasBlackPanel.gameObject.SetActive(true);
        panelNegro.gameObject.SetActive(true);
        SetAlpha(panelNegro, 0f);
        // Aparecer fondo negro
        yield return FadeImage(panelNegro, 0f, 1f, 1f);
        SceneManager.LoadScene(scene);
    }

    public IEnumerator EnterSceneSequence()
    {
        canvasBlackPanel.gameObject.SetActive(true);
        SetAlpha(panelNegro, 1f);
        panelNegro.gameObject.SetActive(true);

        // Desvanecer fondo negro
        yield return FadeImage(panelNegro, 1f, 0f, 1.5f);
        panelNegro.gameObject.SetActive(false);
        canvasBlackPanel.gameObject.SetActive(false);
        changingScene = false;
    }

    private IEnumerator FadeInOutTexto()
    {
        Color color = pressEnterText.color;
        float duracion = 1f;

        while (true)
        {
            float t = Mathf.PingPong(Time.unscaledTime, duracion) / duracion;
            pressEnterText.color = new Color(color.r, color.g, color.b, t);
            yield return null;
        }
    }

    void SetAlpha(Graphic graphic, float alpha)
    {
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetAlpha(image, alpha);
            yield return null;
        }
        SetAlpha(image, to);
    }

    IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetAlpha(text, alpha);
            yield return null;
        }
        SetAlpha(text, to);
    }
}