using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject painelMenu;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelOpcoes;

    // Botão para iniciar o jogo
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Botão menu opções
    public void AbrirOptions()
    {
        painelMenu.SetActive(false);
        painelOpcoes.SetActive(true);
    }
    public void FecharOptions()
    {
        painelOpcoes.SetActive(false);
        painelMenu.SetActive(true);
    }

    // Botão de creditos
    public void AbrirCreditos(){
        painelMenu.SetActive(false);
        painelCreditos.SetActive(true);
    }
    public void FecharCreditos()
    {
        painelCreditos.SetActive(false);
        painelMenu.SetActive(true);
    }

    // Botão "Sair"
    public void QuitGame(){
        Debug.Log("Sair do jogo...");
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
