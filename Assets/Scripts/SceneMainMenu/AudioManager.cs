using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManagerMainMenu : MonoBehaviour
{
    public static AudioManagerMainMenu Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}