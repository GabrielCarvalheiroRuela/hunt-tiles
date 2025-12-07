using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Personagem - Script unificado que gerencia o personagem do jogador.
/// Substitui: BoardCharacter e CharacterController
/// </summary>
public class Personagem : MonoBehaviour
{
    #region Singleton
    public static Personagem Instancia { get; private set; }
    #endregion

    #region Configurações de Posição
    [Header("Posição")]
    [SerializeField] private int posicaoX = 0;
    [SerializeField] private int posicaoY = 0;
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private AnimationCurve curvaMovimento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    #endregion

    #region Configurações Visuais
    [Header("Visual")]
    [SerializeField] private Color corPersonagem = new Color(0.2f, 0.7f, 1f, 1f);  // Azul vibrante
    [SerializeField] private Color corContorno = new Color(0.1f, 0.3f, 0.6f, 1f);  // Azul escuro
    [SerializeField] private float tamanho = 32f;
    [SerializeField] private bool animarIdle = true;
    #endregion

    #region Configurações de Input
    [Header("Controles")]
    [SerializeField] private bool usarTeclado = true;
    [SerializeField] private bool usarMouse = true;
    #endregion

    #region Componentes
    private RectTransform retanguloTransform;
    private Image imagemPersonagem;
    private Tabuleiro tabuleiro;
    #endregion

    #region Estado
    private bool estaMovendo = false;
    private Coroutine animacaoIdle;
    #endregion

    #region Propriedades Públicas
    public int PosicaoX => posicaoX;
    public int PosicaoY => posicaoY;
    public bool EstaMovendo => estaMovendo;
    // Compatibilidade
    public int CurrentX => posicaoX;
    public int CurrentY => posicaoY;
    public bool IsMoving => estaMovendo;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ConfigurarVisual();
    }

    void Start()
    {
        tabuleiro = Tabuleiro.Instancia;
        if (tabuleiro != null)
        {
            PosicionarEm(posicaoX, posicaoY);
            MarcarCelulaOcupada(true);
        }
    }

    void Update()
    {
        ProcessarEntradaTeclado();
        ProcessarEntradaTecladoContinuo();
    }

    void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }
    #endregion

    #region Configuração Inicial
    public void DefinirPosicaoInicial(int x, int y)
    {
        posicaoX = x;
        posicaoY = y;
    }

    // Compatibilidade
    public void SetInitialPosition(int x, int y) => DefinirPosicaoInicial(x, y);

    private void ConfigurarVisual()
    {
        retanguloTransform = GetComponent<RectTransform>();
        if (retanguloTransform == null)
        {
            retanguloTransform = gameObject.AddComponent<RectTransform>();
        }

        retanguloTransform.anchorMin = Vector2.one * 0.5f;
        retanguloTransform.anchorMax = Vector2.one * 0.5f;
        retanguloTransform.sizeDelta = Vector2.one * tamanho;

        imagemPersonagem = GetComponent<Image>();
        if (imagemPersonagem == null)
        {
            imagemPersonagem = gameObject.AddComponent<Image>();
        }

        imagemPersonagem.color = corPersonagem;
        imagemPersonagem.sprite = CriarSpritePersonagem();

        // Contorno elegante
        Outline contorno = GetComponent<Outline>();
        if (contorno == null)
        {
            contorno = gameObject.AddComponent<Outline>();
        }
        contorno.effectColor = corContorno;
        contorno.effectDistance = new Vector2(2f, 2f);

        // Sombra profunda
        Shadow sombra = GetComponent<Shadow>();
        if (sombra == null)
        {
            sombra = gameObject.AddComponent<Shadow>();
        }
        sombra.effectColor = new Color(0f, 0f, 0f, 0.6f);
        sombra.effectDistance = new Vector2(3f, -3f);

        // Iniciar animação idle
        if (animarIdle)
        {
            animacaoIdle = StartCoroutine(AnimarIdle());
        }
    }
    #endregion

    #region Animação Idle
    private IEnumerator AnimarIdle()
    {
        Vector3 escalaBase = Vector3.one;
        float tempo = 0f;

        while (true)
        {
            if (!estaMovendo)
            {
                tempo += Time.deltaTime * 2f;
                float escala = 1f + Mathf.Sin(tempo) * 0.05f;
                retanguloTransform.localScale = escalaBase * escala;
            }
            else
            {
                retanguloTransform.localScale = escalaBase;
            }
            yield return null;
        }
    }
    #endregion

    #region Criação de Sprite
    private Sprite CriarSpritePersonagem()
    {
        int tamanhoTextura = 64;
        Texture2D textura = new Texture2D(tamanhoTextura, tamanhoTextura, TextureFormat.RGBA32, false);

        Vector2 centro = new Vector2(tamanhoTextura / 2f, tamanhoTextura / 2f);
        float raioExterno = tamanhoTextura / 2f - 2f;
        float raioInterno = raioExterno * 0.7f;

        Color corClara = new Color(
            Mathf.Min(1f, corPersonagem.r * 1.3f),
            Mathf.Min(1f, corPersonagem.g * 1.3f),
            Mathf.Min(1f, corPersonagem.b * 1.3f),
            1f
        );
        Color corEscura = new Color(
            corPersonagem.r * 0.6f,
            corPersonagem.g * 0.6f,
            corPersonagem.b * 0.6f,
            1f
        );

        for (int x = 0; x < tamanhoTextura; x++)
        {
            for (int y = 0; y < tamanhoTextura; y++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro);
                Color corPixel = Color.clear;

                if (distancia <= raioExterno)
                {
                    float distanciaNormalizada = distancia / raioExterno;

                    if (distancia <= raioInterno)
                    {
                        corPixel = Color.Lerp(corClara, corPersonagem, distanciaNormalizada);
                    }
                    else
                    {
                        corPixel = Color.Lerp(corPersonagem, corEscura, (distanciaNormalizada - 0.7f) / 0.3f);
                    }

                    // Brilho
                    Vector2 posLuz = centro + new Vector2(-8f, 8f);
                    float distanciaLuz = Vector2.Distance(new Vector2(x, y), posLuz);
                    if (distanciaLuz < 12f && distancia <= raioExterno * 0.8f)
                    {
                        float intensidadeLuz = (12f - distanciaLuz) / 12f * 0.4f;
                        corPixel = Color.Lerp(corPixel, Color.white, intensidadeLuz);
                    }

                    // Olhos
                    Vector2 olhoEsquerdo = centro + new Vector2(-6f, 4f);
                    Vector2 olhoDireito = centro + new Vector2(6f, 4f);
                    if (Vector2.Distance(new Vector2(x, y), olhoEsquerdo) < 3f ||
                        Vector2.Distance(new Vector2(x, y), olhoDireito) < 3f)
                    {
                        corPixel = Color.Lerp(corPixel, Color.black, 0.8f);
                    }

                    // Boca
                    Vector2 boca = centro + new Vector2(0f, -4f);
                    if (Vector2.Distance(new Vector2(x, y), boca) < 2f && y < centro.y - 2f)
                    {
                        corPixel = Color.Lerp(corPixel, Color.black, 0.6f);
                    }
                }

                textura.SetPixel(x, y, corPixel);
            }
        }

        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanhoTextura, tamanhoTextura), new Vector2(0.5f, 0.5f));
    }
    #endregion

    #region Entrada do Jogador

    // Movimento contínuo
    private float tempoSegurando = 0f;
    private float delayInicial = 0.1f;   // tempo antes de começar a repetir
    private float intervaloRepeticao = 0.08f; // intervalo entre movimentos
    private bool teclaSegurada = false;
    private Vector2Int direcaoSegurada;

    private void ProcessarEntradaTeclado()
    {
        if (!usarTeclado || estaMovendo) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame)
        {
            MoverCima();
        }
        else if (teclado.sKey.wasPressedThisFrame || teclado.downArrowKey.wasPressedThisFrame)
        {
            MoverBaixo();
        }
        else if (teclado.aKey.wasPressedThisFrame || teclado.leftArrowKey.wasPressedThisFrame)
        {
            MoverEsquerda();
        }
        else if (teclado.dKey.wasPressedThisFrame || teclado.rightArrowKey.wasPressedThisFrame)
        {
            MoverDireita();
        }
    }

    public void AoCelulaClicada(Celula celula)
    {
        if (!usarMouse || estaMovendo) return;

        int deltaX = Mathf.Abs(celula.X - posicaoX);
        int deltaY = Mathf.Abs(celula.Y - posicaoY);

        if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
        {
            MoverPara(celula.X, celula.Y);
        }
        else if (deltaX == 0 && deltaY == 0)
        {
            Debug.Log("Personagem já está nesta posição!");
        }
        else
        {
            Debug.Log($"Movimento muito distante! Distância: ({deltaX}, {deltaY})");
        }
    }

    private void ProcessarEntradaTecladoContinuo()
    {
        if (!usarTeclado) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        // Detectar direção atual considerando teclas pressionadas
        Vector2Int novaDirecao = Vector2Int.zero;

        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed)
            novaDirecao = new Vector2Int(0, -1);
        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed)
            novaDirecao = new Vector2Int(0, 1);
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed)
            novaDirecao = new Vector2Int(-1, 0);
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed)
            novaDirecao = new Vector2Int(1, 0);

        // Se nenhuma tecla está pressionada
        if (novaDirecao == Vector2Int.zero)
        {
            teclaSegurada = false;
            tempoSegurando = 0f;
            return;
        }

        // Se mudou de direção -> reinicia o timer e anda imediatamente
        if (novaDirecao != direcaoSegurada)
        {
            direcaoSegurada = novaDirecao;
            tempoSegurando = 0f;
            teclaSegurada = true;

            if (!estaMovendo)
            {
                int alvoX = posicaoX + direcaoSegurada.x;
                int alvoY = posicaoY + direcaoSegurada.y;
                MoverPara(alvoX, alvoY);
            }

            return;
        }

        // Mantendo pressionada a mesma direção
        teclaSegurada = true;
        tempoSegurando += Time.deltaTime;

        // Após delay inicial, repetir movimento
        if (tempoSegurando >= delayInicial)
        {
            if (tempoSegurando >= delayInicial + intervaloRepeticao)
            {
                tempoSegurando = delayInicial;

                if (!estaMovendo)
                {
                    int alvoX = posicaoX + direcaoSegurada.x;
                    int alvoY = posicaoY + direcaoSegurada.y;
                    MoverPara(alvoX, alvoY);
                }
            }
        }
    }

    private void IniciarSegurarTecla(Vector2Int direcao)
    {
        if (!teclaSegurada)
        {
            teclaSegurada = true;
            tempoSegurando = 0f;
            direcaoSegurada = direcao;

            // Move uma vez imediatamente
            int alvoX = posicaoX + direcao.x;
            int alvoY = posicaoY + direcao.y;
            MoverPara(alvoX, alvoY);
        }
    }

    #endregion

    #region Movimento
    public bool PodeMoverPara(int x, int y)
    {
        if (tabuleiro == null) return false;

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula == null || !celula.Transitavel) return false;

        GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
        if (gerenciador != null)
        {
            return gerenciador.PodeMoverPara(x, y);
        }

        return true;
    }

    // Compatibilidade
    public bool CanMoveTo(int x, int y) => PodeMoverPara(x, y);

    public void MoverPara(int x, int y)
    {
        if (estaMovendo || !PodeMoverPara(x, y)) return;

        StartCoroutine(CoroutineMovimento(x, y));
    }

    // Compatibilidade
    public void MoveToPosition(int x, int y) => MoverPara(x, y);

    private IEnumerator CoroutineMovimento(int alvoX, int alvoY)
    {
        estaMovendo = true;

        MarcarCelulaOcupada(false);

        Vector2 posicaoInicial = retanguloTransform.anchoredPosition;
        Vector2 posicaoAlvo = ObterPosicaoCelula(alvoX, alvoY);

        float tempoDecorrido = 0f;
        float velocidadeAtual = velocidade;

        GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
        if (gerenciador != null && gerenciador.TemVelocidadeExtra())
        {
            velocidadeAtual *= 2f;
        }

        float duracao = 1f / velocidadeAtual;

        while (tempoDecorrido < duracao)
        {
            tempoDecorrido += Time.deltaTime;
            float t = tempoDecorrido / duracao;
            float valorCurva = curvaMovimento.Evaluate(t);

            retanguloTransform.anchoredPosition = Vector2.Lerp(posicaoInicial, posicaoAlvo, valorCurva);
            yield return null;
        }

        retanguloTransform.anchoredPosition = posicaoAlvo;

        posicaoX = alvoX;
        posicaoY = alvoY;

        MarcarCelulaOcupada(true);

        VerificarColetaveis(alvoX, alvoY);

        estaMovendo = false;
    }

    public void MoverCima() => MoverPara(posicaoX, posicaoY - 1);
    public void MoverBaixo() => MoverPara(posicaoX, posicaoY + 1);
    public void MoverEsquerda() => MoverPara(posicaoX - 1, posicaoY);
    public void MoverDireita() => MoverPara(posicaoX + 1, posicaoY);

    // Compatibilidade
    public void MoveUp() => MoverCima();
    public void MoveDown() => MoverBaixo();
    public void MoveLeft() => MoverEsquerda();
    public void MoveRight() => MoverDireita();
    #endregion

    #region Utilitários
    private void PosicionarEm(int x, int y)
    {
        posicaoX = x;
        posicaoY = y;
        retanguloTransform.anchoredPosition = ObterPosicaoCelula(x, y);
    }

    private Vector2 ObterPosicaoCelula(int x, int y)
    {
        if (tabuleiro == null) return Vector2.zero;

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula != null)
        {
            RectTransform celulaRect = celula.GetComponent<RectTransform>();
            return celulaRect.anchoredPosition;
        }

        return Vector2.zero;
    }

    private void MarcarCelulaOcupada(bool ocupada)
    {
        if (tabuleiro == null) return;

        Celula celula = tabuleiro.ObterCelula(posicaoX, posicaoY);
        if (celula != null)
        {
            celula.DefinirOcupada(ocupada);
        }
    }

    private void VerificarColetaveis(int x, int y)
    {
        Coletavel[] coletaveis = FindObjectsOfType<Coletavel>();

        foreach (Coletavel coletavel in coletaveis)
        {
            if (coletavel.PosX == x && coletavel.PosY == y && !coletavel.FoiColetado)
            {
                Debug.Log($"Coletando {coletavel.Tipo} na posição ({x}, {y})!");
                coletavel.Coletar();
            }
        }

        // Verificar colisão com inimigos
        VerificarColisaoInimigos(x, y);
    }

    private void VerificarColisaoInimigos(int x, int y)
    {
        GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
        if (gerenciador != null)
        {
            gerenciador.VerificarColisaoInimigos(x, y);
        }
    }
    #endregion
}

public class BoardCharacter : Personagem { }
