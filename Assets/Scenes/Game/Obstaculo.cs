using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tipos de obstáculos
/// </summary>
public enum TipoObstaculo
{
    Parede
}

/// <summary>
/// Obstaculo - Representa um obstáculo no tabuleiro (apenas paredes).
/// </summary>
public class Obstaculo : MonoBehaviour
{
    #region Configurações
    [Header("Configurações")]
    [SerializeField] private TipoObstaculo tipo = TipoObstaculo.Parede;
    [SerializeField] private bool ativo = true;
    [SerializeField] private int posicaoX = 0;
    [SerializeField] private int posicaoY = 0;
    #endregion

    #region Visual
    [Header("Visual")]
    [SerializeField] private Color corParede = new Color(0.3f, 0.3f, 0.3f, 1f);
    #endregion

    #region Componentes
    private RectTransform retanguloTransform;
    private Image imagem;
    #endregion

    #region Propriedades Públicas
    public TipoObstaculo Tipo => tipo;
    public bool Ativo => ativo;
    public int PosX => posicaoX;
    public int PosY => posicaoY;
    // Compatibilidade
    public ObstacleType Type => ObstacleType.Wall;
    public bool IsActive => ativo;
    public int GridX => posicaoX;
    public int GridY => posicaoY;
    public int TileX => posicaoX;
    public int TileY => posicaoY;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        retanguloTransform = GetComponent<RectTransform>();
        if (retanguloTransform == null)
            retanguloTransform = gameObject.AddComponent<RectTransform>();

        imagem = GetComponent<Image>();
        if (imagem == null)
            imagem = gameObject.AddComponent<Image>();
    }

    void Start()
    {
        Configurar();
    }
    #endregion

    #region Inicialização
    public void Inicializar(int x, int y)
    {
        tipo = TipoObstaculo.Parede;
        posicaoX = x;
        posicaoY = y;
        Configurar();
    }

    // Compatibilidade com código antigo
    public void Initialize(ObstacleType obstacleType, int x, int y)
    {
        tipo = TipoObstaculo.Parede;
        posicaoX = x;
        posicaoY = y;
        Configurar();
    }

    private void Configurar()
    {
        imagem.color = corParede;
        imagem.sprite = CriarSpriteParede();
        retanguloTransform.sizeDelta = new Vector2(38f, 38f);

        if (retanguloTransform != null)
        {
            retanguloTransform.anchorMin = new Vector2(0.5f, 0.5f);
            retanguloTransform.anchorMax = new Vector2(0.5f, 0.5f);
            retanguloTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        AdicionarEfeitosVisuais();
    }
    #endregion

    #region Efeitos Visuais
    private void AdicionarEfeitosVisuais()
    {
        Outline contorno = GetComponent<Outline>();
        if (contorno == null)
            contorno = gameObject.AddComponent<Outline>();

        Shadow sombra = GetComponent<Shadow>();
        if (sombra == null)
            sombra = gameObject.AddComponent<Shadow>();

        contorno.effectColor = new Color(0.2f, 0.1f, 0.05f, 1f);
        contorno.effectDistance = new Vector2(2f, 2f);
        sombra.effectColor = new Color(0f, 0f, 0f, 0.6f);
        sombra.effectDistance = new Vector2(3f, -3f);
    }
    #endregion

    #region Comportamento
    public bool BloqueiaMovimento()
    {
        return ativo;
    }

    // Compatibilidade
    public bool BlocksMovement() => BloqueiaMovimento();

    public void DefinirAtivo(bool valor)
    {
        ativo = valor;
        gameObject.SetActive(valor);
    }

    // Compatibilidade
    public void SetActive(bool active) => DefinirAtivo(active);

    public void DefinirPosicao(int x, int y)
    {
        posicaoX = x;
        posicaoY = y;
    }

    // Compatibilidade
    public void SetGridPosition(int x, int y) => DefinirPosicao(x, y);
    #endregion

    #region Criação de Sprite
    private Sprite CriarSpriteParede()
    {
        int tamanho = 32;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        Color corPedra = new Color(0.4f, 0.35f, 0.3f, 1f);
        Color corArgamassa = new Color(0.25f, 0.2f, 0.15f, 1f);
        Color corClara = new Color(0.5f, 0.45f, 0.4f, 1f);

        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                Color corPixel = corPedra;

                bool ehArgamassa = false;

                // Linhas horizontais
                if (y % 8 == 0 || y % 8 == 7)
                    ehArgamassa = true;

                // Linhas verticais alternadas
                int fileira = y / 8;
                int offset = (fileira % 2) * 4;
                if ((x + offset) % 16 == 0 || (x + offset) % 16 == 15)
                    ehArgamassa = true;

                if (ehArgamassa)
                {
                    corPixel = corArgamassa;
                }
                else
                {
                    // Textura de pedra
                    if ((x + y) % 3 == 0)
                        corPixel = corClara;
                    else if ((x * 2 + y) % 5 == 0)
                        corPixel = Color.Lerp(corPedra, corArgamassa, 0.3f);
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f));
    }
    #endregion
}

public enum ObstacleType
{
    Wall,
    Hole,
    Ice,
    Teleporter
}

public class Obstacle : Obstaculo { }
