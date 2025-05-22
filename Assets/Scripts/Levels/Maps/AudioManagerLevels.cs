using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManagerLevels : MonoBehaviour
{
    public static AudioManagerLevels Instance;
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