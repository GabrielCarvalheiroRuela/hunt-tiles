using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameSetup gameSetup = FindObjectOfType<GameSetup>();
        GameController gameController = FindObjectOfType<GameController>();
        
        if (gameSetup == null)
        {
            GameObject setupGO = new GameObject("Game Setup");
            setupGO.AddComponent<GameSetup>();
            Debug.Log("GameSetup criado pelo Bootstrap!");
        }
        
        if (gameController == null)
        {
            GameObject controllerGO = new GameObject("Game Controller");
            controllerGO.AddComponent<GameController>();
            Debug.Log("GameController criado pelo Bootstrap!");
        }
    }
}
