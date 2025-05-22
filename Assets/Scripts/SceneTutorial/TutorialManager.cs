using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{

    void Start()
    {
    }

    void Update()
    {
        if (CutsceneManager.Instance.changingScene) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(CutsceneManager.Instance.ExitSceneSequence("MainMenu"));
        }
    }
}
