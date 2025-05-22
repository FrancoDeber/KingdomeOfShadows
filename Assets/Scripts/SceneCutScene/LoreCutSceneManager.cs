using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoreCutSceneManager : MonoBehaviour
{
    public List<GameObject> lorePartsPrefabs; // Prefabs de LorePart01, asignados en inspector
    public Transform canvasTransform;          // Donde se instancian los prefabs (el Canvas o panel padre)
    public TextMeshProUGUI startText;
    public float waitTime = 3f;                // Tiempo que se mantiene visible cada lore part

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("Level01"));
        }
    }

    private IEnumerator Start()
    {
        StartCoroutine(BlinkStartText());
        foreach (var prefab in lorePartsPrefabs)
        {
            // Instanciar prefab en el canvas
            GameObject go = Instantiate(prefab, canvasTransform);

            // Obtener script para controlar fade
            FadeGroupCustom fadeGroup = go.GetComponent<FadeGroupCustom>();

            if (fadeGroup != null)
            {
                fadeGroup.Init();
                // Fade in
                yield return StartCoroutine(fadeGroup.FadeIn());

                // Esperar un tiempo visible
                yield return new WaitForSeconds(waitTime);

                // Fade out
                yield return StartCoroutine(fadeGroup.FadeOut());
            }

            // Destruir para limpiar
            Destroy(go);
        }
        startText.transform.position = new Vector2(0, 0);
        
    }

    IEnumerator BlinkStartText()
    {
        while (true)
        {
            // Fade In
            yield return StartCoroutine(Fade(0f, 1f));
            // Fade Out
            yield return StartCoroutine(Fade(1f, 0f));
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / 1f);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(to);
    }
    private void SetAlpha(float alpha)
    {
        Color c = startText.color;
        c.a = alpha;
        startText.color = c;
    }
}
