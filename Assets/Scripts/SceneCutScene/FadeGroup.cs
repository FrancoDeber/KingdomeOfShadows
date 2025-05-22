using System.Collections;
using UnityEngine;
using TMPro;

public class FadeGroupCustom : MonoBehaviour
{
    public float fadeDuration = 1f;

    [Header("Asignar en inspector o buscar en Awake")]
    public TextMeshProUGUI textMesh;            // El texto del lore
    public Transform spritesParent;         // Panel que contiene sprites con SpriteRenderer

    private SpriteRenderer[] spriteRenderers;

    public void Init()
    {
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshProUGUI>();

        if (spritesParent == null)
            Debug.LogError("Asignar spritesParent en inspector o buscarlo manualmente.");

        if (spritesParent != null)
            spriteRenderers = spritesParent.GetComponentsInChildren<SpriteRenderer>();
    }

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(0f, 1f));
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);

            // Fade texto
            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = alpha;
                textMesh.color = c;
            }

            // Fade sprites
            if (spriteRenderers != null)
            {
                foreach (var sr in spriteRenderers)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Asegurar valor final
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = to;
            textMesh.color = c;
        }

        if (spriteRenderers != null)
        {
            foreach (var sr in spriteRenderers)
            {
                Color c = sr.color;
                c.a = to;
                sr.color = c;
            }
        }
    }
}
