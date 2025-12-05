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
        
        // Verifica se CarregarJogo existe na cena
        CarregarJogo carregarJogo = Object.FindObjectOfType<CarregarJogo>();
        if (carregarJogo == null)
        {
            GameObject jogoGO = new GameObject("Jogo");
            jogoGO.AddComponent<CarregarJogo>();
            
            Debug.Log("CarregarJogo adicionado automaticamente à cena Game!");
        }
    }
}
#endif
