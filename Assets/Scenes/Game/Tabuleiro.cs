using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Tabuleiro - Script unificado que gerencia o grid e as células do jogo.
/// Substitui: GridBoard e GridTileUI
/// </summary>
public class Tabuleiro : MonoBehaviour
{
    #region Singleton
    public static Tabuleiro Instancia { get; private set; }
    #endregion

    #region Configurações do Grid
    [Header("Configurações do Grid")]
    [SerializeField] private int largura = 10;
    [SerializeField] private int altura = 10;
    [SerializeField] private float tamanhoCelula = 40f;
    [SerializeField] private float espacamento = 1f;
    #endregion

    #region Referências UI
    [Header("Referências UI")]
    [SerializeField] private Transform painelGrid;
    [SerializeField] private GameObject prefabCelula;
    #endregion

    #region Configurações Visuais
    [Header("Cores de Madeira")]
    [SerializeField] private Color corMadeiraClara = new Color(0.76f, 0.60f, 0.42f, 1f);   // Carvalho claro
    [SerializeField] private Color corMadeiraEscura = new Color(0.55f, 0.38f, 0.22f, 1f);  // Nogueira
    [SerializeField] private Color corMadeiraBorda = new Color(0.35f, 0.22f, 0.12f, 1f);   // Mogno escuro
    [SerializeField] private Color corDestaque = new Color(0.95f, 0.85f, 0.55f, 0.9f);     // Dourado suave
    [SerializeField] private Color corPersonagem = new Color(0f, 0f, 0f, 0f);              // Transparente (sem cor)
    
    [Header("Detalhes Visuais")]
    [SerializeField] private float intensidadeVeio = 0.08f;     // Intensidade do veio da madeira
    [SerializeField] private float arredondamentoBorda = 3f;    // Arredondamento visual
    #endregion

    #region Dados Internos
    private Celula[,] celulas;
    private RectTransform retanguloTransform;
    #endregion

    #region Propriedades Públicas
    public int Largura => largura;
    public int Altura => altura;
    public int Width => largura;
    public int Height => altura;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            retanguloTransform = GetComponent<RectTransform>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        CalcularTamanhoOtimo();
        CriarGrid();
    }

    void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }
    #endregion

    #region Criação do Grid
    private void CalcularTamanhoOtimo()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float larguraTela = canvasRect.rect.width;
        float alturaTela = canvasRect.rect.height;

        float larguraDisponivel = larguraTela * 0.8f;
        float alturaDisponivel = alturaTela * 0.8f;

        float maxLarguraCelula = (larguraDisponivel - (largura - 1) * espacamento) / largura;
        float maxAlturaCelula = (alturaDisponivel - (altura - 1) * espacamento) / altura;

        tamanhoCelula = Mathf.Min(maxLarguraCelula, maxAlturaCelula);
        tamanhoCelula = Mathf.Clamp(tamanhoCelula, 25f, 60f);

        Debug.Log($"Tela: {larguraTela}x{alturaTela}, Célula: {tamanhoCelula}px");
    }

    private void CriarGrid()
    {
        if (painelGrid == null)
            painelGrid = transform;

        celulas = new Celula[largura, altura];

        float larguraTotal = largura * tamanhoCelula + (largura - 1) * espacamento;
        float alturaTotal = altura * tamanhoCelula + (altura - 1) * espacamento;

        CriarFundo(larguraTotal, alturaTotal);

        float inicioX = -larguraTotal / 2f + tamanhoCelula / 2f;
        float inicioY = alturaTotal / 2f - tamanhoCelula / 2f;

        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                Vector2 posicao = new Vector2(
                    inicioX + x * (tamanhoCelula + espacamento),
                    inicioY - y * (tamanhoCelula + espacamento)
                );

                CriarCelula(x, y, posicao);
            }
        }

        if (retanguloTransform != null)
        {
            retanguloTransform.sizeDelta = new Vector2(larguraTotal + 40f, alturaTotal + 40f);
        }

        Debug.Log($"✓ Tabuleiro {largura}x{altura} criado");
    }

    private void CriarFundo(float larguraTotal, float alturaTotal)
    {
        // Moldura externa (mais escura)
        GameObject molduraObj = new GameObject("MolduraTabuleiro");
        molduraObj.transform.SetParent(painelGrid);
        
        Image imagemMoldura = molduraObj.AddComponent<Image>();
        imagemMoldura.color = new Color(0.25f, 0.15f, 0.08f, 1f); // Madeira escura
        
        RectTransform molduraRect = molduraObj.GetComponent<RectTransform>();
        molduraRect.anchorMin = Vector2.one * 0.5f;
        molduraRect.anchorMax = Vector2.one * 0.5f;
        molduraRect.sizeDelta = new Vector2(larguraTotal + 30f, alturaTotal + 30f);
        molduraRect.anchoredPosition = Vector2.zero;
        
        // Contorno da moldura
        Outline contornoMoldura = molduraObj.AddComponent<Outline>();
        contornoMoldura.effectColor = new Color(0.15f, 0.08f, 0.04f, 1f);
        contornoMoldura.effectDistance = new Vector2(4f, 4f);
        
        // Sombra profunda da moldura
        Shadow sombraMoldura = molduraObj.AddComponent<Shadow>();
        sombraMoldura.effectColor = new Color(0f, 0f, 0f, 0.6f);
        sombraMoldura.effectDistance = new Vector2(5f, -5f);
        
        molduraObj.transform.SetAsFirstSibling();
        
        // Fundo interno (tom médio)
        GameObject fundoObj = new GameObject("FundoTabuleiro");
        fundoObj.transform.SetParent(painelGrid);

        Image imagemFundo = fundoObj.AddComponent<Image>();
        imagemFundo.color = new Color(0.35f, 0.22f, 0.12f, 1f); // Madeira média

        RectTransform fundoRect = fundoObj.GetComponent<RectTransform>();
        fundoRect.anchorMin = Vector2.one * 0.5f;
        fundoRect.anchorMax = Vector2.one * 0.5f;
        fundoRect.sizeDelta = new Vector2(larguraTotal + 12f, alturaTotal + 12f);
        
        // Brilho interno sutil
        Outline brilhoInterno = fundoObj.AddComponent<Outline>();
        brilhoInterno.effectColor = new Color(0.5f, 0.35f, 0.2f, 0.5f);
        brilhoInterno.effectDistance = new Vector2(2f, 2f);
        
        fundoObj.transform.SetSiblingIndex(1);
    }

    private void CriarCelula(int x, int y, Vector2 posicao)
    {
        GameObject celulaObj;

        if (prefabCelula != null)
        {
            celulaObj = Instantiate(prefabCelula, painelGrid);
        }
        else
        {
            celulaObj = new GameObject($"Celula_{x}_{y}");
            celulaObj.transform.SetParent(painelGrid);

            // Cor base da célula com variação natural de madeira
            Image imagem = celulaObj.AddComponent<Image>();
            Color corBase = ((x + y) % 2 == 0) ? corMadeiraClara : corMadeiraEscura;
            
            // Adicionar variação sutil para simular veios da madeira
            float variacaoX = Mathf.PerlinNoise(x * 0.5f, y * 0.3f) * intensidadeVeio;
            float variacaoY = Mathf.PerlinNoise(x * 0.3f + 100f, y * 0.5f) * intensidadeVeio;
            corBase = new Color(
                Mathf.Clamp01(corBase.r + variacaoX - variacaoY * 0.5f),
                Mathf.Clamp01(corBase.g + variacaoX * 0.7f - variacaoY * 0.3f),
                Mathf.Clamp01(corBase.b + variacaoX * 0.3f),
                corBase.a
            );
            imagem.color = corBase;

            // Borda interna elegante (brilho)
            Outline brilho = celulaObj.AddComponent<Outline>();
            Color corBrilho = ((x + y) % 2 == 0) 
                ? new Color(0.9f, 0.75f, 0.55f, 0.4f)  // Brilho claro
                : new Color(0.7f, 0.5f, 0.3f, 0.3f);   // Brilho médio
            brilho.effectColor = corBrilho;
            brilho.effectDistance = new Vector2(-1f, 1f); // Brilho no topo-esquerda

            // Sombra sutil para profundidade
            Shadow sombra = celulaObj.AddComponent<Shadow>();
            sombra.effectColor = new Color(0.15f, 0.08f, 0.04f, 0.5f);
            sombra.effectDistance = new Vector2(1.5f, -1.5f);
            
            // Segunda sombra para borda mais definida
            GameObject bordaInternaObj = new GameObject("BordaInterna");
            bordaInternaObj.transform.SetParent(celulaObj.transform);
            Image bordaInterna = bordaInternaObj.AddComponent<Image>();
            bordaInterna.color = new Color(0f, 0f, 0f, 0f); // Transparente
            
            RectTransform bordaRect = bordaInternaObj.GetComponent<RectTransform>();
            bordaRect.anchorMin = Vector2.zero;
            bordaRect.anchorMax = Vector2.one;
            bordaRect.offsetMin = Vector2.one * 2f;
            bordaRect.offsetMax = Vector2.one * -2f;
            
            Outline contornoBorda = bordaInternaObj.AddComponent<Outline>();
            contornoBorda.effectColor = corMadeiraBorda;
            contornoBorda.effectDistance = new Vector2(1f, 1f);
        }

        RectTransform celulaRect = celulaObj.GetComponent<RectTransform>();
        celulaRect.anchorMin = Vector2.one * 0.5f;
        celulaRect.anchorMax = Vector2.one * 0.5f;
        celulaRect.sizeDelta = Vector2.one * tamanhoCelula;
        celulaRect.anchoredPosition = posicao;

        Celula componente = celulaObj.GetComponent<Celula>();
        if (componente == null)
        {
            componente = celulaObj.AddComponent<Celula>();
        }

        // Calcular cor base com variação de veio
        Color corNormal = ((x + y) % 2 == 0) ? corMadeiraClara : corMadeiraEscura;
        float varX = Mathf.PerlinNoise(x * 0.5f, y * 0.3f) * intensidadeVeio;
        float varY = Mathf.PerlinNoise(x * 0.3f + 100f, y * 0.5f) * intensidadeVeio;
        corNormal = new Color(
            Mathf.Clamp01(corNormal.r + varX - varY * 0.5f),
            Mathf.Clamp01(corNormal.g + varX * 0.7f - varY * 0.3f),
            Mathf.Clamp01(corNormal.b + varX * 0.3f),
            corNormal.a
        );
        
        componente.Inicializar(x, y, corNormal, corDestaque, corPersonagem);

        celulas[x, y] = componente;
    }
    #endregion

    #region Acesso às Células
    public Celula ObterCelula(int x, int y)
    {
        if (x >= 0 && x < largura && y >= 0 && y < altura)
        {
            return celulas[x, y];
        }
        return null;
    }

    public Celula ObterCelulaNaPosicaoTela(Vector2 posicaoTela)
    {
        Vector2 posicaoLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(retanguloTransform, posicaoTela, null, out posicaoLocal);

        float larguraTotal = largura * tamanhoCelula + (largura - 1) * espacamento;
        float alturaTotal = altura * tamanhoCelula + (altura - 1) * espacamento;

        float inicioX = -larguraTotal / 2f;
        float inicioY = alturaTotal / 2f;

        int x = Mathf.FloorToInt((posicaoLocal.x - inicioX) / (tamanhoCelula + espacamento));
        int y = Mathf.FloorToInt((inicioY - posicaoLocal.y) / (tamanhoCelula + espacamento));

        return ObterCelula(x, y);
    }

    public List<Celula> ObterVizinhos(int x, int y, bool incluirDiagonais = false)
    {
        List<Celula> vizinhos = new List<Celula>();

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        if (incluirDiagonais)
        {
            dx = new int[] { 0, 0, -1, 1, -1, -1, 1, 1 };
            dy = new int[] { -1, 1, 0, 0, -1, 1, -1, 1 };
        }

        for (int i = 0; i < dx.Length; i++)
        {
            Celula vizinho = ObterCelula(x + dx[i], y + dy[i]);
            if (vizinho != null)
            {
                vizinhos.Add(vizinho);
            }
        }

        return vizinhos;
    }
    #endregion

    #region Interação
    public void AoCelulaClicada(Celula celula)
    {
        Debug.Log($"Célula clicada: ({celula.X}, {celula.Y})");

        Personagem personagem = Personagem.Instancia;
        if (personagem != null)
        {
            personagem.AoCelulaClicada(celula);
        }
    }
    #endregion

    #region Destaques
    public void LimparDestaques()
    {
        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                celulas[x, y]?.DefinirDestaque(false);
            }
        }
    }

    public void DestacarCelulas(List<Celula> celulasParaDestacar)
    {
        LimparDestaques();
        foreach (Celula celula in celulasParaDestacar)
        {
            celula?.DefinirDestaque(true);
        }
    }
    #endregion

    #region Validação
    public bool PosicaoValida(int x, int y)
    {
        return x >= 0 && x < largura && y >= 0 && y < altura;
    }
    #endregion
}

/// <summary>
/// Celula - Representa uma célula individual do tabuleiro com visual de madeira.
/// </summary>
public class Celula : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Propriedades
    private int x;
    private int y;
    private bool transitavel = true;
    private bool ocupada = false;
    private bool destacada = false;
    private bool mouseEmCima = false;
    #endregion

    #region Cores
    private Color corNormal;
    private Color corDestaque;
    private Color corHover;
    private Color corBloqueada;
    #endregion

    #region Componentes
    private Image imagem;
    private Outline brilho;
    private Shadow sombra;
    #endregion

    #region Propriedades Públicas
    public int X => x;
    public int Y => y;
    public bool Transitavel => transitavel && !ocupada;
    public bool Ocupada => ocupada;
    // Compatibilidade
    public int TileX => x;
    public int TileY => y;
    public bool IsWalkable => Transitavel;
    public bool IsOccupied => ocupada;
    #endregion

    void Awake()
    {
        imagem = GetComponent<Image>();
        brilho = GetComponent<Outline>();
        sombra = GetComponent<Shadow>();
    }

    public void Inicializar(int posX, int posY, Color normal, Color destaque, Color ocupadaCor)
    {
        x = posX;
        y = posY;
        gameObject.name = $"Celula_{x}_{y}";

        corNormal = normal;
        corDestaque = destaque;
        
        // Cor de hover é um tom mais claro da cor normal
        corHover = new Color(
            Mathf.Min(1f, normal.r * 1.15f),
            Mathf.Min(1f, normal.g * 1.1f),
            Mathf.Min(1f, normal.b * 1.05f),
            normal.a
        );
        
        // Cor bloqueada é um cinza com tom de madeira
        corBloqueada = new Color(0.45f, 0.38f, 0.32f, 1f);

        if (imagem != null)
        {
            imagem.color = corNormal;
        }

        AtualizarVisual();
    }

    public void DefinirTransitavel(bool valor)
    {
        transitavel = valor;
        AtualizarVisual();
    }

    public void DefinirOcupada(bool valor)
    {
        ocupada = valor;
        AtualizarVisual();
    }

    public void DefinirDestaque(bool valor)
    {
        destacada = valor;
        AtualizarVisual();
    }

    // Compatibilidade com GridTileUI
    public void SetWalkable(bool walkable) => DefinirTransitavel(walkable);
    public void SetOccupied(bool occupied) => DefinirOcupada(occupied);
    public void SetHighlight(bool highlight) => DefinirDestaque(highlight);

    private void AtualizarVisual()
    {
        if (imagem == null) return;

        Color corAlvo;
        float alfaBrilho = 0.4f;

        if (destacada)
        {
            // Destaque dourado elegante
            corAlvo = corDestaque;
            alfaBrilho = 0.7f;
        }
        else if (ocupada)
        {
            // Personagem em cima: manter cor normal (sem alteração visual)
            corAlvo = corNormal;
            alfaBrilho = 0.3f;
        }
        else if (!transitavel)
        {
            // Célula bloqueada
            corAlvo = corBloqueada;
            alfaBrilho = 0.2f;
        }
        else if (mouseEmCima)
        {
            // Hover: levemente mais claro
            corAlvo = corHover;
            alfaBrilho = 0.5f;
        }
        else
        {
            corAlvo = corNormal;
        }

        imagem.color = corAlvo;
        
        // Atualizar brilho se existir
        if (brilho != null)
        {
            Color corBrilhoAtual = brilho.effectColor;
            brilho.effectColor = new Color(corBrilhoAtual.r, corBrilhoAtual.g, corBrilhoAtual.b, alfaBrilho);
        }
    }

    #region Eventos de Ponteiro
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEmCima = true;
        if (!ocupada && transitavel)
        {
            // Mostrar hover sutil em vez de destaque completo
            AtualizarVisual();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEmCima = false;
        destacada = false;
        AtualizarVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Transitavel)
        {
            // Flash de feedback ao clicar
            StartCoroutine(FlashClique());
            Tabuleiro.Instancia?.AoCelulaClicada(this);
        }
    }
    
    private System.Collections.IEnumerator FlashClique()
    {
        if (imagem == null) yield break;
        
        Color corOriginal = imagem.color;
        Color corFlash = new Color(
            Mathf.Min(1f, corOriginal.r * 1.3f),
            Mathf.Min(1f, corOriginal.g * 1.25f),
            Mathf.Min(1f, corOriginal.b * 1.1f),
            corOriginal.a
        );
        
        imagem.color = corFlash;
        yield return new WaitForSeconds(0.1f);
        
        // Voltar gradualmente
        float tempo = 0f;
        float duracao = 0.15f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            imagem.color = Color.Lerp(corFlash, corOriginal, tempo / duracao);
            yield return null;
        }
        
        AtualizarVisual();
    }
    #endregion
}
