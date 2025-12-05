using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Tipos de itens coletáveis
/// </summary>
public enum TipoColetavel
{
    Moeda,
    PowerUp
}

/// <summary>
/// Tipos de power-ups disponíveis
/// </summary>
public enum TipoPowerUp
{
    Velocidade,
    PontuacaoDupla,
    Invencibilidade
}

/// <summary>
/// Coletavel - Item que pode ser coletado pelo jogador.
/// </summary>
public class Coletavel : MonoBehaviour
{
    #region Configurações
    [Header("Configurações")]
    [SerializeField] private TipoColetavel tipo;
    [SerializeField] private TipoPowerUp tipoPowerUp;
    [SerializeField] private int pontos = 10;
    [SerializeField] private bool coletado = false;
    [SerializeField] private int posicaoX = 0;
    [SerializeField] private int posicaoY = 0;
    #endregion

    #region Cores
    [Header("Cores")]
    [SerializeField] private Color corMoeda = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color corPowerUp = new Color(0f, 1f, 0.5f, 1f);
    #endregion

    #region Animação
    [Header("Animação")]
    [SerializeField] private float velocidadePulso = 2f;
    [SerializeField] private float intensidadePulso = 0.3f;
    [SerializeField] private float velocidadeRotacao = 50f;
    #endregion

    #region Componentes
    private RectTransform retanguloTransform;
    private Image imagem;
    private Vector3 escalaOriginal;
    #endregion

    #region Propriedades Públicas
    public TipoColetavel Tipo => tipo;
    public TipoPowerUp TipoPower => tipoPowerUp;
    public int Pontos => pontos;
    public int Valor => pontos;
    public bool Coletado => coletado;
    public bool FoiColetado => coletado;
    public int PosicaoX => posicaoX;
    public int PosicaoY => posicaoY;
    public int PosX => posicaoX;
    public int PosY => posicaoY;
    // Compatibilidade
    public CollectibleType Type => tipo == TipoColetavel.Moeda ? CollectibleType.Coin : CollectibleType.PowerUp;
    public int Value => pontos;
    public bool IsCollected => coletado;
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
        escalaOriginal = retanguloTransform.localScale;
        StartCoroutine(Animar());
    }
    #endregion

    #region Inicialização
    public void Inicializar(TipoColetavel tipoColetavel, int x, int y)
    {
        tipo = tipoColetavel;
        posicaoX = x;
        posicaoY = y;
        Configurar();
    }

    // Compatibilidade com código antigo
    public void Initialize(CollectibleType collectibleType, int x, int y)
    {
        tipo = (collectibleType == CollectibleType.PowerUp) ? TipoColetavel.PowerUp : TipoColetavel.Moeda;
        posicaoX = x;
        posicaoY = y;
        Configurar();
    }

    private void Configurar()
    {
        switch (tipo)
        {
            case TipoColetavel.Moeda:
                imagem.color = corMoeda;
                imagem.sprite = CriarSpriteMoeda();
                pontos = 10;
                retanguloTransform.sizeDelta = new Vector2(22f, 22f);
                break;

            case TipoColetavel.PowerUp:
                imagem.color = corPowerUp;
                imagem.sprite = CriarSpritePowerUp();
                pontos = 0;
                retanguloTransform.sizeDelta = new Vector2(32f, 32f);
                // Definir tipo de power-up aleatório
                tipoPowerUp = (TipoPowerUp)Random.Range(0, 3);
                break;
        }

        AdicionarEfeitosVisuais();

        if (retanguloTransform != null)
        {
            retanguloTransform.anchorMin = new Vector2(0.5f, 0.5f);
            retanguloTransform.anchorMax = new Vector2(0.5f, 0.5f);
            retanguloTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }
    #endregion

    #region Animação
    private IEnumerator Animar()
    {
        float tempo = 0f;

        while (!coletado)
        {
            tempo += Time.deltaTime;

            // Pulsar
            float pulso = 1f + Mathf.Sin(tempo * velocidadePulso) * intensidadePulso;
            retanguloTransform.localScale = escalaOriginal * pulso;

            // Rotacionar power-ups
            if (tipo == TipoColetavel.PowerUp)
            {
                retanguloTransform.Rotate(0f, 0f, velocidadeRotacao * Time.deltaTime);
            }

            yield return null;
        }
    }
    #endregion

    #region Coleta
    public void Coletar()
    {
        if (coletado) return;

        Debug.Log($"Coletando {tipo} na posição ({posicaoX}, {posicaoY})");
        coletado = true;

        // Efeitos visuais (apenas efeito de partículas, sem popup de texto para não poluir)
        VisualEffectsManager efeitosManager = VisualEffectsManager.Instance;
        if (efeitosManager != null)
        {
            efeitosManager.ShowCollectionEffect(transform.position, Type);
            // Popup de texto apenas para power-ups (moedas usam flash no HUD)
            if (tipo == TipoColetavel.PowerUp)
            {
                efeitosManager.ShowScorePopup(transform.position, pontos, ObterCorPontuacao());
            }
        }

        StartCoroutine(AnimacaoColeta());

        // Notificar gerenciador
        GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
        if (gerenciador != null)
        {
            gerenciador.AoColetarItem(this);
        }
        else
        {
            Debug.LogError("GerenciadorJogo.Instancia é null!");
        }
    }

    // Compatibilidade
    public void Collect() => Coletar();

    private Color ObterCorPontuacao()
    {
        return tipo == TipoColetavel.Moeda ? Color.yellow : Color.green;
    }

    private IEnumerator AnimacaoColeta()
    {
        float duracao = 0.5f;
        Vector3 escalaInicial = retanguloTransform.localScale;
        Vector3 escalaFinal = escalaInicial * 1.5f;
        Color corInicial = imagem.color;
        Color corFinal = new Color(corInicial.r, corInicial.g, corInicial.b, 0f);

        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracao)
        {
            float progresso = tempoDecorrido / duracao;

            retanguloTransform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progresso);
            imagem.color = Color.Lerp(corInicial, corFinal, progresso);

            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
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

        switch (tipo)
        {
            case TipoColetavel.Moeda:
                contorno.effectColor = new Color(0.8f, 0.5f, 0f, 1f);
                contorno.effectDistance = new Vector2(1f, 1f);
                sombra.effectColor = new Color(0f, 0f, 0f, 0.3f);
                sombra.effectDistance = new Vector2(2f, -2f);
                break;

            case TipoColetavel.PowerUp:
                contorno.effectColor = new Color(0f, 0.8f, 0.3f, 1f);
                contorno.effectDistance = new Vector2(2f, 2f);
                sombra.effectColor = new Color(0f, 0f, 0f, 0.5f);
                sombra.effectDistance = new Vector2(3f, -3f);
                break;
        }
    }
    #endregion

    #region Criação de Sprites
    private Sprite CriarSpriteMoeda()
    {
        int tamanho = 32;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        Vector2 centro = new Vector2(tamanho / 2f, tamanho / 2f);
        float raioExterno = tamanho / 2f - 2f;
        float raioInterno = raioExterno * 0.7f;

        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro);
                Color corPixel = Color.clear;

                if (distancia <= raioExterno)
                {
                    if (distancia > raioInterno)
                    {
                        corPixel = new Color(0.9f, 0.7f, 0.1f, 1f);
                    }
                    else
                    {
                        corPixel = new Color(1f, 0.85f, 0.3f, 1f);
                    }

                    // Brilho
                    float distanciaCentro = Vector2.Distance(new Vector2(x, y), centro + new Vector2(-3, 3));
                    if (distanciaCentro < 4f)
                    {
                        corPixel = Color.Lerp(corPixel, Color.white, 0.4f);
                    }
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f));
    }

    private Sprite CriarSpritePowerUp()
    {
        int tamanho = 32;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        Vector2 centro = new Vector2(tamanho / 2f, tamanho / 2f);

        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                Vector2 pos = new Vector2(x, y) - centro;
                Color corPixel = Color.clear;

                bool naEstrela = false;

                // Forma de cruz/estrela
                if (Mathf.Abs(pos.y) <= 3f && Mathf.Abs(pos.x) <= 11f)
                    naEstrela = true;

                if (Mathf.Abs(pos.x) <= 3f && Mathf.Abs(pos.y) <= 11f)
                    naEstrela = true;

                if (naEstrela)
                {
                    float distancia = Vector2.Distance(Vector2.zero, pos);

                    if (distancia < 6f)
                    {
                        corPixel = new Color(0.4f, 1f, 0.6f, 1f);
                    }
                    else
                    {
                        corPixel = new Color(0.2f, 0.8f, 0.4f, 1f);
                    }

                    // Centro brilhante
                    if (distancia < 3f)
                    {
                        corPixel = Color.Lerp(corPixel, Color.white, 0.5f);
                    }
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f));
    }
    #endregion
}

public enum CollectibleType
{
    Coin,
    Gem,
    PowerUp,
    Key
}
public class Collectible : Coletavel { }
