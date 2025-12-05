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
    [Header("Cores Premium")]
    [SerializeField] private Color corMadeiraClara = new Color(0.82f, 0.68f, 0.52f, 1f);   // Maple claro
    [SerializeField] private Color corMadeiraEscura = new Color(0.58f, 0.42f, 0.28f, 1f);  // Nogueira rica
    [SerializeField] private Color corMadeiraBorda = new Color(0.28f, 0.18f, 0.10f, 1f);   // Ébano
    [SerializeField] private Color corDestaque = new Color(1f, 0.92f, 0.65f, 0.95f);       // Ouro suave
    [SerializeField] private Color corPersonagem = new Color(0f, 0f, 0f, 0f);              // Transparente
    
    [Header("Detalhes Premium")]
    [SerializeField] private float intensidadeVeio = 0.06f;     // Veio sutil da madeira
    [SerializeField] private float arredondamentoBorda = 4f;    // Arredondamento elegante
    [SerializeField] private bool usarSombrasCelulas = true;    // Sombras nas células
    [SerializeField] private bool usarBrilhoRealista = true;    // Brilho 3D nas células
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
        // === SOMBRA EXTERNA PROFUNDA ===
        GameObject sombraExternaObj = new GameObject("SombraExterna");
        sombraExternaObj.transform.SetParent(painelGrid);
        
        Image imagemSombraExterna = sombraExternaObj.AddComponent<Image>();
        imagemSombraExterna.color = new Color(0f, 0f, 0f, 0.4f);
        
        RectTransform sombraExternaRect = sombraExternaObj.GetComponent<RectTransform>();
        sombraExternaRect.anchorMin = Vector2.one * 0.5f;
        sombraExternaRect.anchorMax = Vector2.one * 0.5f;
        sombraExternaRect.sizeDelta = new Vector2(larguraTotal + 50f, alturaTotal + 50f);
        sombraExternaRect.anchoredPosition = new Vector2(8f, -8f);
        
        sombraExternaObj.transform.SetAsFirstSibling();

        // === MOLDURA EXTERNA PREMIUM ===
        GameObject molduraObj = new GameObject("MolduraTabuleiro");
        molduraObj.transform.SetParent(painelGrid);
        
        Image imagemMoldura = molduraObj.AddComponent<Image>();
        imagemMoldura.color = new Color(0.18f, 0.12f, 0.07f, 1f); // Ébano profundo
        
        RectTransform molduraRect = molduraObj.GetComponent<RectTransform>();
        molduraRect.anchorMin = Vector2.one * 0.5f;
        molduraRect.anchorMax = Vector2.one * 0.5f;
        molduraRect.sizeDelta = new Vector2(larguraTotal + 36f, alturaTotal + 36f);
        molduraRect.anchoredPosition = Vector2.zero;
        
        // Contorno dourado da moldura
        Outline contornoMoldura = molduraObj.AddComponent<Outline>();
        contornoMoldura.effectColor = new Color(0.75f, 0.6f, 0.3f, 0.8f);
        contornoMoldura.effectDistance = new Vector2(2f, 2f);
        
        // Sombra interna da moldura
        Shadow sombraMoldura = molduraObj.AddComponent<Shadow>();
        sombraMoldura.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sombraMoldura.effectDistance = new Vector2(6f, -6f);
        
        molduraObj.transform.SetSiblingIndex(1);
        
        // === DETALHE INTERNO DA MOLDURA ===
        GameObject detalheObj = new GameObject("DetalheMoldura");
        detalheObj.transform.SetParent(painelGrid);
        
        Image imagemDetalhe = detalheObj.AddComponent<Image>();
        imagemDetalhe.color = new Color(0.35f, 0.25f, 0.15f, 1f);
        
        RectTransform detalheRect = detalheObj.GetComponent<RectTransform>();
        detalheRect.anchorMin = Vector2.one * 0.5f;
        detalheRect.anchorMax = Vector2.one * 0.5f;
        detalheRect.sizeDelta = new Vector2(larguraTotal + 24f, alturaTotal + 24f);
        detalheRect.anchoredPosition = Vector2.zero;
        
        Outline brilhoDetalhe = detalheObj.AddComponent<Outline>();
        brilhoDetalhe.effectColor = new Color(0.5f, 0.4f, 0.25f, 0.6f);
        brilhoDetalhe.effectDistance = new Vector2(-1f, 1f);
        
        detalheObj.transform.SetSiblingIndex(2);
        
        // === FUNDO INTERNO (FELTRO) ===
        GameObject fundoObj = new GameObject("FundoTabuleiro");
        fundoObj.transform.SetParent(painelGrid);

        Image imagemFundo = fundoObj.AddComponent<Image>();
        imagemFundo.color = new Color(0.22f, 0.16f, 0.10f, 1f); // Tom escuro elegante

        RectTransform fundoRect = fundoObj.GetComponent<RectTransform>();
        fundoRect.anchorMin = Vector2.one * 0.5f;
        fundoRect.anchorMax = Vector2.one * 0.5f;
        fundoRect.sizeDelta = new Vector2(larguraTotal + 8f, alturaTotal + 8f);
        fundoRect.anchoredPosition = Vector2.zero;
        
        // Brilho interno sutil
        Outline brilhoInterno = fundoObj.AddComponent<Outline>();
        brilhoInterno.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.4f);
        brilhoInterno.effectDistance = new Vector2(1f, 1f);
        
        // Sombra interna para profundidade
        Shadow sombraInterna = fundoObj.AddComponent<Shadow>();
        sombraInterna.effectColor = new Color(0f, 0f, 0f, 0.3f);
        sombraInterna.effectDistance = new Vector2(2f, -2f);
        
        fundoObj.transform.SetSiblingIndex(3);
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

            // === COR BASE COM VARIAÇÃO PREMIUM DE MADEIRA ===
            Image imagem = celulaObj.AddComponent<Image>();
            Color corBase = ((x + y) % 2 == 0) ? corMadeiraClara : corMadeiraEscura;
            
            // Variação de veio mais natural usando múltiplas camadas de Perlin noise
            float veio1 = Mathf.PerlinNoise(x * 0.4f, y * 0.25f) * intensidadeVeio;
            float veio2 = Mathf.PerlinNoise(x * 0.2f + 50f, y * 0.4f) * intensidadeVeio * 0.5f;
            float veio3 = Mathf.PerlinNoise(x * 0.7f + 100f, y * 0.15f) * intensidadeVeio * 0.3f;
            float variacaoTotal = veio1 + veio2 - veio3;
            
            corBase = new Color(
                Mathf.Clamp01(corBase.r + variacaoTotal),
                Mathf.Clamp01(corBase.g + variacaoTotal * 0.85f),
                Mathf.Clamp01(corBase.b + variacaoTotal * 0.5f),
                corBase.a
            );
            imagem.color = corBase;

            if (usarBrilhoRealista)
            {
                // === BRILHO 3D NO TOPO-ESQUERDA ===
                Outline brilho = celulaObj.AddComponent<Outline>();
                Color corBrilho = ((x + y) % 2 == 0) 
                    ? new Color(1f, 0.92f, 0.75f, 0.35f)  // Brilho dourado suave
                    : new Color(0.8f, 0.65f, 0.45f, 0.28f);   // Brilho cobre
                brilho.effectColor = corBrilho;
                brilho.effectDistance = new Vector2(-1.5f, 1.5f);
            }

            if (usarSombrasCelulas)
            {
                // === SOMBRA 3D NO CANTO INFERIOR-DIREITA ===
                Shadow sombra = celulaObj.AddComponent<Shadow>();
                sombra.effectColor = new Color(0.1f, 0.06f, 0.03f, 0.45f);
                sombra.effectDistance = new Vector2(2f, -2f);
            }
            
            // === BORDA INTERNA ELEGANTE ===
            GameObject bordaInternaObj = new GameObject("BordaInterna");
            bordaInternaObj.transform.SetParent(celulaObj.transform);
            Image bordaInterna = bordaInternaObj.AddComponent<Image>();
            bordaInterna.color = new Color(0f, 0f, 0f, 0f); // Transparente
            
            RectTransform bordaRect = bordaInternaObj.GetComponent<RectTransform>();
            bordaRect.anchorMin = Vector2.zero;
            bordaRect.anchorMax = Vector2.one;
            bordaRect.offsetMin = Vector2.one * 1.5f;
            bordaRect.offsetMax = Vector2.one * -1.5f;
            
            // Contorno fino e elegante
            Outline contornoBorda = bordaInternaObj.AddComponent<Outline>();
            contornoBorda.effectColor = new Color(corMadeiraBorda.r, corMadeiraBorda.g, corMadeiraBorda.b, 0.6f);
            contornoBorda.effectDistance = new Vector2(0.8f, 0.8f);
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
