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
        
        // Verifica se GerenciadorJogo existe na cena
        GerenciadorJogo gerenciador = Object.FindObjectOfType<GerenciadorJogo>();
        if (gerenciador == null)
        {
            GameObject go = new GameObject("GerenciadorJogo");
            go.AddComponent<GerenciadorJogo>();
            
            Debug.Log("GerenciadorJogo adicionado automaticamente à cena Game!");
        }
    }
}
#endif
