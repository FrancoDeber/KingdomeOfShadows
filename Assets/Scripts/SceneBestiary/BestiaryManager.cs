using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BestiaryManager : MonoBehaviour
{
    public enum Categoria
    {
        Characters,
        Enemies,
        Bosses,
        Items,
        Powerups,
        Maps
    }
    private enum EstadoMenu
    {
        Categorias,
        SeleccionPrefabs,
        DetallePrefab
    }

    private EstadoMenu estadoActual = EstadoMenu.Categorias;
    [SerializeField] private List<Button> botonesCategorias;

    private bool activo = false; // Controla si el sistema está activo

    [Header("Contenedor Grid")]
    [SerializeField] private Transform gridContainer; // Panel con Grid Layout Group

    [Header("Prefab del botón")]
    [SerializeField] private GameObject botonPrefab;

    [Header("Listas de prefabs por categoría")]
    public List<GameObject> personajesPrefabs;
    public List<GameObject> enemigosPrefabs;
    public List<GameObject> itemsPrefabs;
    public List<GameObject> powerupsPrefabs;
    public List<GameObject> bossPrefabs;
    public List<GameObject> mapssPrefabs;

    [Header("Panel de Detalles")]
    [SerializeField] private GameObject detallePanel;
    [SerializeField] private Transform prefabContainer;
    [SerializeField] private TMPro.TextMeshProUGUI textoLore;

    private int botonSeleccionadoIndex = 0;
    private List<GameObject> botonesInstanciados = new List<GameObject>();

    private int categoriaIndex = 0;
    private Categoria[] categorias = (Categoria[])System.Enum.GetValues(typeof(Categoria));


    void Start()
    {
        CargarCategoria(Categoria.Characters);
        ActivarSelector();
    }


    void Update()
    {
        if (CutsceneManager.Instance.changingScene) return;

        if (!activo) return;

        switch (estadoActual)
        {
            case EstadoMenu.Categorias:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    categoriaIndex = Mathf.Min(categoriaIndex + 1, categorias.Length - 1);
                    
                    CargarCategoria(categorias[categoriaIndex]);
                    ActualizarVisualCategorias();
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    categoriaIndex = Mathf.Max(categoriaIndex - 1, 0);
                    CargarCategoria(categorias[categoriaIndex]);
                    ActualizarVisualCategorias();
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    estadoActual = EstadoMenu.SeleccionPrefabs;
                    SeleccionarBoton(0);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("MainMenu"));
                }
                else
                {
                    return;
                }
                break;

            case EstadoMenu.SeleccionPrefabs:
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    SeleccionarSiguiente();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    SeleccionarAnterior();
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    estadoActual = EstadoMenu.DetallePrefab;
                    ConfirmarSeleccion(); // Invoca el botón y muestra el detalle
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    estadoActual = EstadoMenu.Categorias;
                    LimpiarPanel();
                    CargarCategoria(categorias[categoriaIndex]);
                }
                else
                {
                    return;
                }
                break;

            case EstadoMenu.DetallePrefab:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
                {
                    detallePanel.SetActive(false);
                    if (instanciaActual != null) Destroy(instanciaActual);
                    estadoActual = EstadoMenu.SeleccionPrefabs;
                }
                else
                {
                    return;
                }
                break;
        }
    }

    void ActualizarVisualCategorias()
    {
        for (int i = 0; i < botonesCategorias.Count; i++)
        {
            var img = botonesCategorias[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == categoriaIndex) ? Color.yellow : Color.white;
        }
    }

    void LimpiarPanel()
    {
        foreach (GameObject btn in botonesInstanciados)
        {
            Destroy(btn);
        }
        botonesInstanciados.Clear();
        botonSeleccionadoIndex = 0;
    }

    public void CargarCategoria(Categoria categoria)
    {
        LimpiarPanel();

        List<GameObject> listaACargar = null;

        switch (categoria)
        {
            case Categoria.Characters:
                listaACargar = personajesPrefabs;
                break;
            case Categoria.Enemies:
                listaACargar = enemigosPrefabs;
                break;
            case Categoria.Bosses:
                listaACargar = bossPrefabs;
                break;
            case Categoria.Items:
                listaACargar = itemsPrefabs;
                break;
            case Categoria.Powerups:
                listaACargar = powerupsPrefabs;
                break;
            case Categoria.Maps:
                listaACargar = mapssPrefabs;
                break;
        }

        if (listaACargar == null || listaACargar.Count == 0) return;

        foreach (GameObject prefab in listaACargar)
        {
            GameObject boton = Instantiate(botonPrefab, gridContainer);
            // Aquí asigna el texto, imagen, etc. del botón basado en el prefab

            // Guardar una copia del prefab para usarlo en el botón
            GameObject prefabCopy = prefab;

            boton.GetComponent<Button>().onClick.AddListener(() =>
            {
                MostrarDetalle(prefabCopy);
            });

            // Obtener el SpriteRenderer del prefab
            SpriteRenderer spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Buscar el hijo llamado "Icon" dentro del botón
                Transform iconTransform = boton.transform.Find("Icon");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = spriteRenderer.sprite;
                    }
                }
            }

            botonesInstanciados.Add(boton);
        }

        SeleccionarBoton(0);
    }

    void SeleccionarBoton(int index)
    {
        if (botonesInstanciados.Count == 0) return;

        botonSeleccionadoIndex = Mathf.Clamp(index, 0, botonesInstanciados.Count - 1);

        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            var img = botonesInstanciados[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == botonSeleccionadoIndex) ? Color.yellow : Color.white;
        }
    }

    public void SeleccionarSiguiente()
    {
        if (botonSeleccionadoIndex < botonesInstanciados.Count - 1)
            SeleccionarBoton(botonSeleccionadoIndex + 1);
    }

    public void SeleccionarAnterior()
    {
        if (botonSeleccionadoIndex > 0)
            SeleccionarBoton(botonSeleccionadoIndex - 1);
    }

    public GameObject GetBotonSeleccionado()
    {
        if (botonesInstanciados.Count == 0) return null;
        return botonesInstanciados[botonSeleccionadoIndex];
    }

    private void ConfirmarSeleccion()
    {
        GameObject seleccionado = GetBotonSeleccionado();
        if (seleccionado != null)
        {
            seleccionado.GetComponent<Button>().onClick.Invoke();
            Debug.Log("Seleccionado: " + seleccionado.name);
        }
    }


    public void ActivarSelector()
    {
        activo = true;
    }
    
    private GameObject instanciaActual;

    private void MostrarDetalle(GameObject prefab)
    {
        if (instanciaActual != null)
            Destroy(instanciaActual);

        // Instanciar el prefab en el contenedor
        instanciaActual = Instantiate(prefab, prefabContainer);
        
        // Asegurarse de que quede en posición y escala correctas
        instanciaActual.transform.localPosition = Vector3.zero;
        instanciaActual.transform.localRotation = Quaternion.identity;
        instanciaActual.transform.localScale = new Vector3(500f, 500f, 1f); 

        ILoreProvider loreProvider = instanciaActual.GetComponentInChildren<ILoreProvider>();
        textoLore.text = loreProvider != null ? loreProvider.GetLore() : "Sin descripción.";

        // Activar el panel
        detallePanel.SetActive(true);
    }
}
