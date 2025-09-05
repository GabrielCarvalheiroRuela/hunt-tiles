using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[InitializeOnLoad]
public class AutoGameSetup
{
    static AutoGameSetup()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }
    
    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (Application.isPlaying) return;
        
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Game") return;
        
        GameSceneBootstrap bootstrap = Object.FindObjectOfType<GameSceneBootstrap>();
        if (bootstrap == null)
        {
            GameObject bootstrapGO = new GameObject("Game Scene Bootstrap");
            bootstrapGO.AddComponent<GameSceneBootstrap>();
            
            Debug.Log("Bootstrap adicionado automaticamente à cena Game!");
        }
    }
}
#endif
