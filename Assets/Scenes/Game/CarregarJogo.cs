using UnityEngine;

/// <summary>
/// Script de entrada que apenas cria o GerenciadorJogo.
/// O GerenciadorJogo contém toda a lógica do jogo unificada.
/// </summary>
public class CarregarJogo : MonoBehaviour
{
    public static CarregarJogo Instance { get; private set; }

    private GerenciadorJogo gerenciador;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        CriarGerenciador();
    }

    private void CriarGerenciador()
    {
        gerenciador = FindObjectOfType<GerenciadorJogo>();
        if (gerenciador == null)
        {
            GameObject go = new GameObject("GerenciadorJogo");
            gerenciador = go.AddComponent<GerenciadorJogo>();
            Debug.Log("✓ GerenciadorJogo criado por CarregarJogo");
        }
    }

    // Atalhos para acessar o gerenciador
    public GerenciadorJogo Gerenciador => gerenciador ?? GerenciadorJogo.Instancia;
    public Personagem GetPersonagem() => Gerenciador?.ObterPersonagem();
    public Tabuleiro GetTabuleiro() => Gerenciador?.ObterTabuleiro();
    public bool EstaPausado() => Gerenciador?.EstaPausado ?? false;
    public bool EstaInicializado() => Gerenciador?.EstaInicializado ?? false;
}
