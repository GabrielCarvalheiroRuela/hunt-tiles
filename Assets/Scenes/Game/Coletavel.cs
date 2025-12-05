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
        int tamanho = 48;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        Vector2 centro = new Vector2(tamanho / 2f, tamanho / 2f);
        float raioExterno = tamanho / 2f - 3f;

        // Cores premium para moeda dourada
        Color ouroEscuro = new Color(0.72f, 0.53f, 0.04f, 1f);
        Color ouroBase = new Color(0.95f, 0.75f, 0.15f, 1f);
        Color ouroClaro = new Color(1f, 0.92f, 0.55f, 1f);
        Color brilho = new Color(1f, 1f, 0.9f, 1f);

        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro);
                Color corPixel = Color.clear;

                if (distancia <= raioExterno)
                {
                    // Gradiente radial para efeito 3D
                    float t = distancia / raioExterno;
                    
                    // Borda escura
                    if (t > 0.85f)
                    {
                        corPixel = Color.Lerp(ouroBase, ouroEscuro, (t - 0.85f) / 0.15f);
                    }
                    // Corpo principal com gradiente
                    else
                    {
                        corPixel = Color.Lerp(ouroClaro, ouroBase, t / 0.85f);
                    }

                    // Círculo interno (símbolo $)
                    float distanciaInterna = Vector2.Distance(new Vector2(x, y), centro);
                    if (distanciaInterna < raioExterno * 0.5f)
                    {
                        // Desenha símbolo de moeda simplificado
                        Vector2 pos = new Vector2(x, y) - centro;
                        if (Mathf.Abs(pos.x) < 3f && Mathf.Abs(pos.y) < 8f)
                        {
                            corPixel = Color.Lerp(corPixel, ouroEscuro, 0.4f);
                        }
                    }

                    // Brilho no canto superior esquerdo
                    Vector2 brilhoPos = new Vector2(x, y) - (centro + new Vector2(-6, 6));
                    float distBrilho = brilhoPos.magnitude;
                    if (distBrilho < 8f && distancia < raioExterno * 0.8f)
                    {
                        float intensidade = 1f - (distBrilho / 8f);
                        corPixel = Color.Lerp(corPixel, brilho, intensidade * 0.6f);
                    }

                    // Borda anti-aliasing
                    if (distancia > raioExterno - 1.5f)
                    {
                        float alpha = 1f - (distancia - (raioExterno - 1.5f)) / 1.5f;
                        corPixel.a = alpha;
                    }
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.filterMode = FilterMode.Bilinear;
        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f));
    }

    private Sprite CriarSpritePowerUp()
    {
        int tamanho = 48;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        Vector2 centro = new Vector2(tamanho / 2f, tamanho / 2f);
        float raio = tamanho / 2f - 4f;

        // Cores vibrantes para power-up
        Color corExterna, corMedia, corInterna, corBrilho;
        
        switch (tipoPowerUp)
        {
            case TipoPowerUp.Velocidade:
                corExterna = new Color(0.0f, 0.4f, 0.9f, 1f);   // Azul
                corMedia = new Color(0.2f, 0.6f, 1f, 1f);
                corInterna = new Color(0.5f, 0.85f, 1f, 1f);
                corBrilho = new Color(0.8f, 0.95f, 1f, 1f);
                break;
            case TipoPowerUp.PontuacaoDupla:
                corExterna = new Color(0.6f, 0.0f, 0.8f, 1f);   // Roxo
                corMedia = new Color(0.75f, 0.3f, 1f, 1f);
                corInterna = new Color(0.9f, 0.6f, 1f, 1f);
                corBrilho = new Color(1f, 0.85f, 1f, 1f);
                break;
            case TipoPowerUp.Invencibilidade:
            default:
                corExterna = new Color(0.9f, 0.6f, 0f, 1f);     // Dourado
                corMedia = new Color(1f, 0.8f, 0.2f, 1f);
                corInterna = new Color(1f, 0.95f, 0.5f, 1f);
                corBrilho = new Color(1f, 1f, 0.9f, 1f);
                break;
        }

        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                Vector2 pos = new Vector2(x, y) - centro;
                float distancia = pos.magnitude;
                Color corPixel = Color.clear;

                // Forma de estrela de 6 pontas
                float angulo = Mathf.Atan2(pos.y, pos.x);
                float raioEstrela = raio * (0.6f + 0.4f * Mathf.Abs(Mathf.Cos(angulo * 3f)));

                if (distancia <= raioEstrela)
                {
                    // Gradiente do centro para fora
                    float t = distancia / raioEstrela;

                    if (t < 0.3f)
                    {
                        corPixel = Color.Lerp(corBrilho, corInterna, t / 0.3f);
                    }
                    else if (t < 0.7f)
                    {
                        corPixel = Color.Lerp(corInterna, corMedia, (t - 0.3f) / 0.4f);
                    }
                    else
                    {
                        corPixel = Color.Lerp(corMedia, corExterna, (t - 0.7f) / 0.3f);
                    }

                    // Efeito de brilho central
                    if (distancia < 5f)
                    {
                        corPixel = Color.Lerp(corPixel, Color.white, 0.5f * (1f - distancia / 5f));
                    }

                    // Anti-aliasing na borda
                    if (distancia > raioEstrela - 1.5f)
                    {
                        float alpha = 1f - (distancia - (raioEstrela - 1.5f)) / 1.5f;
                        corPixel.a = alpha;
                    }
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.filterMode = FilterMode.Bilinear;
        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f));
    }
    #endregion
}

/// <summary>
/// Enum de compatibilidade com código legado
/// </summary>
public enum CollectibleType
{
    Coin,
    Gem,
    PowerUp,
    Key
}
