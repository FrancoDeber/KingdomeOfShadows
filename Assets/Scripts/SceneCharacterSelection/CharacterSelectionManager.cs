using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    [SerializeField] private GameObject[] personajes;
    [SerializeField] private Transform posIzquierda;
    [SerializeField] private Transform posCentro;
    [SerializeField] private Transform posDerecha;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterTitleText;
    [SerializeField] private TextMeshProUGUI characterSubTitleText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI dashText;
    [SerializeField] private Image backgroundBlack;


    private int indexCentro = 0;
    private int total;

    private GameObject objIzquierda;
    private GameObject objCentro;
    private GameObject objDerecha;

    private bool enTransicion = false;

    // Variables para el parpadeo del personaje
    [SerializeField] private Image[] borders = new Image[4]; // El borde que parpadea alrededor del personaje central
    private bool enConfirmacion = false;
    private float blinkTimer = 1f;
    private float intervaloBlink = 1f;
    private bool bordeVisible = true;

    public enum CharacterScale
    {
        Center,
        Lateral,
        Game
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        total = personajes.Length;
        CargarPersonajes();
        ActualizarPanelPersonaje(objCentro);
    }

    private void Update()
    {
        if (CutsceneManager.Instance.changingScene) return;
        if (enTransicion)
        {
            foreach (Image border in borders)
            {
                border.enabled = false;
            }
            return;
        }

        // En estado confirmacion, solo puede aceptar o cancelar
        if (enConfirmacion)
        {
            // Parpadeo del borde
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= intervaloBlink)
            {
                blinkTimer = 0f;
                bordeVisible = !bordeVisible;

                foreach (Image border in borders)
                {
                    border.enabled = bordeVisible;
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmarSeleccion();  // Segundo Enter: cargar escena
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SalirConfirmacion();  // Escape vuelve a estado navegando
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(Transicionar(1));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(Transicionar(-1));
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EntrarConfirmacion();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitButton();
        }
    }

    void EntrarConfirmacion()
    {
        enConfirmacion = true;
        intervaloBlink = 0.3f; // Parpadeo rápido
        bordeVisible = true;
        foreach (Image border in borders)
        {
            border.enabled = true;
            border.color = Color.yellow;
        }


    }

    void SalirConfirmacion()
    {
        enConfirmacion = false;
        intervaloBlink = 1f; // Parpadeo lento
        bordeVisible = true;
        foreach (Image border in borders)
        {
            border.enabled = true;
            border.color = Color.green;
        }
    }


    void CargarPersonajes()
    {
        objCentro = Instantiate(personajes[indexCentro], posCentro.position, Quaternion.identity, transform);
        SetVisual(objCentro, CharacterScale.Center, 1f);

        objIzquierda = Instantiate(personajes[GetIndexRelativo(-1)], posIzquierda.position, Quaternion.identity, transform);
        SetVisual(objIzquierda, CharacterScale.Lateral, 0.5f);

        objDerecha = Instantiate(personajes[GetIndexRelativo(1)], posDerecha.position, Quaternion.identity, transform);
        SetVisual(objDerecha, CharacterScale.Lateral, 0.5f);
    }

    int GetIndexRelativo(int offset)
    {
        int nuevo = (indexCentro + offset + total) % total;
        return nuevo;
    }

    void SetVisual(GameObject obj, CharacterScale characterScale, float alpha)
    {
        float scale;
        switch (characterScale)
        {
            case CharacterScale.Center:
                scale = obj.GetComponent<Player>().centerScale;
                break;

            case CharacterScale.Lateral:
                scale = obj.GetComponent<Player>().lateralScale;
                break;

            case CharacterScale.Game:
                scale = obj.GetComponent<Player>().gameScale;
                break;
            default:
                scale = 1f;
                break;
        }
        obj.transform.localScale = Vector3.one * scale;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    IEnumerator Transicionar(int direccion)
    {
        if (enTransicion) yield break;
        enTransicion = true;


        GameObject nuevo = Instantiate(
            personajes[GetIndexRelativo(direccion * 2)],
            direccion == 1 ? posDerecha.position + Vector3.right * 3 + Vector3.up * 1 : posIzquierda.position + Vector3.left * 3 + Vector3.up * 1,
            Quaternion.identity,
            transform
        );
        SetVisual(nuevo, CharacterScale.Game, 0.5f);

        // Guardamos referencias seguras
        Transform izqTransform = objIzquierda != null ? objIzquierda.transform : null;
        Transform cenTransform = objCentro != null ? objCentro.transform : null;
        Transform derTransform = objDerecha != null ? objDerecha.transform : null;
        Transform nuevoTransform = nuevo.transform;

        Vector3 izqInicio = izqTransform != null ? izqTransform.position : Vector3.zero;
        Vector3 cenInicio = cenTransform != null ? cenTransform.position : Vector3.zero;
        Vector3 derInicio = derTransform != null ? derTransform.position : Vector3.zero;
        Vector3 nuevoInicio = nuevoTransform.position;

        float duracion = 0.3f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;

            if (cenTransform != null)
            {
                cenTransform.position = Vector3.Lerp(cenInicio, direccion == 1 ? posIzquierda.position : posDerecha.position, t);
                cenTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.7f, t);
            }

            if (derTransform != null)
            {
                derTransform.position = Vector3.Lerp(derInicio, direccion == 1 ? posCentro.position : derInicio + Vector3.right * 3 + Vector3.up * 1, t);
                derTransform.localScale = Vector3.Lerp(Vector3.one * 0.7f, direccion == 1 ? Vector3.one : Vector3.one * 0.7f, t);
            }

            if (izqTransform != null)
            {
                izqTransform.position = Vector3.Lerp(izqInicio, direccion == 1 ? izqInicio + Vector3.left * 3 + Vector3.up * 1 : posCentro.position, t);
                izqTransform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.zero, t);
            }

            if (nuevoTransform != null)
            {
                nuevoTransform.position = Vector3.Lerp(nuevoInicio, direccion == 1 ? posDerecha.position : posIzquierda.position, t);
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        indexCentro = GetIndexRelativo(direccion);

        GameObject tempDelete;
        if (direccion == 1)
        {
            tempDelete = objIzquierda;
            objIzquierda = objCentro;
            objCentro = objDerecha;
            objDerecha = nuevo;

        }
        else
        {
            tempDelete = objDerecha;
            objDerecha = objCentro;
            objCentro = objIzquierda;
            objIzquierda = nuevo;

        }
        Destroy(tempDelete);


        SetVisual(objCentro, CharacterScale.Center, 1f);
        SetVisual(objIzquierda, CharacterScale.Lateral, 0.5f);
        SetVisual(objDerecha, CharacterScale.Lateral, 0.5f);

        ActualizarPanelPersonaje(objCentro);

        //Hacer visible el border
        foreach (Image border in borders)
        {
            border.enabled = true;
            border.color = Color.green;
        }

        enTransicion = false;
    }




    void ActualizarPanelPersonaje(GameObject personaje)
    {
        var player = personaje.GetComponent<Player>();
        if (player == null) return;

        characterNameText.text = player.characterName;
        characterTitleText.text = player.characterTitleName;
        characterSubTitleText.text = player.characterSecondTitleName;
        healthText.text = player.currentHealth.ToString();
        speedText.text = player.moveSpeed.ToString();
        damageText.text = player.damage.ToString();
        dashText.text = player.maxDashCount.ToString();
    }

    void ConfirmarSeleccion()
    {
        //SetVisual(objCentro, CharacterScale.Game, 1f);
        objCentro.GetComponent<Player>().isPreview = false;
        objCentro.transform.SetParent(null);
        DontDestroyOnLoad(objCentro);
        objCentro.SetActive(false);
        GameManager.Instance.player = objCentro.GetComponent<Player>();
        Destroy(AudioManagerMainMenu.Instance.gameObject);
        StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("LoreScene"));
    }

    void OnExitButton()
    {
        GameManager.Instance.player = objCentro.GetComponent<Player>();
        StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("MainMenu"));
    }

}
