using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Script principal que gerencia todo o carregamento e controle do jogo.
/// Este é o ÚNICO script que precisa ser referenciado na cena Game.
/// </summary>
public class CarregarJogo : MonoBehaviour
{
    #region Singleton
    public static CarregarJogo Instance { get; private set; }
    #endregion

    #region Componentes Internos
    private GridBoard gridBoard;
    private GameSetup gameSetup;
    private BoardCharacter playerCharacter;
    private CharacterController characterController;
    #endregion

    #region UI References
    [Header("UI Elements (Opcional - serão criados se não existirem)")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pauseMenu;
    #endregion

    #region Estado do Jogo
    private bool isPaused = false;
    private bool jogoInicializado = false;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        ConfigurarSingleton();
    }

    void Start()
    {
        InicializarJogo();
    }

    void Update()
    {
        if (jogoInicializado)
        {
            VerificarInputs();
        }
    }
    #endregion

    #region Inicialização
    private void ConfigurarSingleton()
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

    private void InicializarJogo()
    {
        Debug.Log("=== CARREGANDO JOGO ===");
        
        // Resetar estado
        Time.timeScale = 1f;
        isPaused = false;

        // Criar componentes na ordem correta
        CriarEventSystem();
        CriarCanvas();
        CriarGridBoard();
        CriarGameSetup();
        
        // Configurar UI
        ConfigurarUI();

        // Aguardar inicialização completa
        StartCoroutine(AguardarInicializacaoCompleta());
    }

    private void CriarEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("✓ EventSystem criado");
        }
    }

    private void CriarCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvasComponent = canvasGO.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Canvas criado");
        }
    }

    private void CriarGridBoard()
    {
        gridBoard = FindObjectOfType<GridBoard>();
        if (gridBoard == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject gridBoardGO = new GameObject("GridBoard");
                gridBoardGO.transform.SetParent(canvas.transform, false);

                RectTransform rectTransform = gridBoardGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;

                gridBoard = gridBoardGO.AddComponent<GridBoard>();
                Debug.Log("✓ GridBoard criado");
            }
        }
    }

    private void CriarGameSetup()
    {
        gameSetup = FindObjectOfType<GameSetup>();
        if (gameSetup == null)
        {
            GameObject setupGO = new GameObject("GameSetup");
            gameSetup = setupGO.AddComponent<GameSetup>();
            Debug.Log("✓ GameSetup criado");
        }
    }

    private void ConfigurarUI()
    {
        if (gameUI != null)
            gameUI.SetActive(true);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    private IEnumerator AguardarInicializacaoCompleta()
    {
        Debug.Log("Aguardando inicialização completa...");
        
        // Aguardar até que o personagem esteja pronto
        float timeout = 10f;
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            if (gameSetup != null && gameSetup.GetCharacter() != null)
            {
                playerCharacter = gameSetup.GetCharacter();
                characterController = gameSetup.GetCharacterController();
                break;
            }
            
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        jogoInicializado = true;
        Debug.Log("=== JOGO CARREGADO COM SUCESSO ===");
        Debug.Log("Use as setas do teclado para mover o personagem!");
    }
    #endregion

    #region Controles do Jogo
    private void VerificarInputs()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenu != null)
                pauseMenu.SetActive(true);
            Debug.Log("Jogo pausado");
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
            Debug.Log("Jogo retomado");
        }
    }

    public void ContinuarJogo()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void ReiniciarJogo()
    {
        Debug.Log("Reiniciando jogo...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        Debug.Log("Voltando ao menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Getters Públicos
    public BoardCharacter GetPersonagem()
    {
        return playerCharacter;
    }

    public GridBoard GetTabuleiro()
    {
        return gridBoard;
    }

    public GameSetup GetGameSetup()
    {
        return gameSetup;
    }

    public bool EstaPausado()
    {
        return isPaused;
    }

    public bool EstaInicializado()
    {
        return jogoInicializado;
    }
    #endregion
}
