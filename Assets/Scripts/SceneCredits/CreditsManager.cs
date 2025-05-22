using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class CreditsManager : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(CutsceneManager.Instance.HideCinematicBars());
        Destroy(HUDManager.Instance.gameObject);
        Destroy(AudioManagerLevels.Instance.gameObject);
    }

    void Update()
    {
        if (CutsceneManager.Instance.changingScene) return;
        // Confirmar selección con Enter
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("MainMenu"));
        }
    }

    public void OnExitButton()
    {
        StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("MainMenu"));
    }

}
