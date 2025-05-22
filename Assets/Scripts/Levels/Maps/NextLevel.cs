using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string scene;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(CambiarEscenaDespuesDeCutscene());

        }
    }
    
    private IEnumerator CambiarEscenaDespuesDeCutscene()
    {
        yield return StartCoroutine(CutsceneManager.Instance.EndMapCutsceneSequence());
        SceneManager.LoadScene(scene);
    }

}