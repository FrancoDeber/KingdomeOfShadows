using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    public Button[] botones; // Asigna en el Inspector
    private int indexActual = 0;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(botones[0].gameObject);
        SeleccionarBoton(indexActual);
    }

    void Update()
    {
        if (CutsceneManager.Instance.changingScene) return;
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            indexActual = (indexActual + 1) % botones.Length;
            SeleccionarBoton(indexActual);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            indexActual = (indexActual - 1 + botones.Length) % botones.Length;
            SeleccionarBoton(indexActual);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            botones[indexActual].onClick.Invoke();
        }
    }

    void SeleccionarBoton(int index)
    {
        EventSystem.current.SetSelectedGameObject(null); // limpia selección anterior
        EventSystem.current.SetSelectedGameObject(botones[index].gameObject);
    }

    public void OnStartButton()
    {
        StartButtonRoutine("CharacterSelection");
    }

    public void OnBestiaryButton()
    {
        StartButtonRoutine("Bestiary");
    }

    public void OnTutorialButton()
    {
        StartButtonRoutine("Tutorial");
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    void StartButtonRoutine(string scene)
    {
        StartCoroutine(CutsceneManager.Instance.ExitSceneSequence(scene));
    }
}
