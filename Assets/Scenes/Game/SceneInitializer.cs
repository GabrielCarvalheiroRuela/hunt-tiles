using UnityEngine;

[System.Serializable]
public class SceneInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game")
        {
            GameSetup existingSetup = FindObjectOfType<GameSetup>();
            if (existingSetup == null)
            {
                GameObject setupGO = new GameObject("Game Setup");
                setupGO.AddComponent<GameSetup>();
                Debug.Log("GameSetup criado automaticamente!");
            }
        }
    }
}
