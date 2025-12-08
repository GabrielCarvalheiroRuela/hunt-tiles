using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// InterfaceJogo - HUD responsivo que se posiciona dinamicamente
/// para nunca sobrepor o tabuleiro.
/// </summary>
public class InterfaceJogo : MonoBehaviour
{
    #region Elementos de UI
    [Header("Painéis")]
    [SerializeField] private GameObject painelHUD;
    [SerializeField] private GameObject painelTutorial;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject painelDerrota;
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private GameObject painelMensagem;

    [Header("Textos do HUD")]
    [SerializeField] private Text textoPontuacao;
    [SerializeField] private Text textoNivel;
    [SerializeField] private Text textoTempo;
    [SerializeField] private Text textoMoedas;
    [SerializeField] private Text textoPowerUps;
    [SerializeField] private Text textoVidas;

    [Header("Barra de Progresso")]
    [SerializeField] private GameObject containerProgresso;
    [SerializeField] private Image barraProgresso;
    [SerializeField] private Text textoProgresso;

    [Header("Mensagens")]
    [SerializeField] private Text textoMensagem;

    [Header("Painel de Vitória")]
    [SerializeField] private Text textoVitoria;

    [Header("Painel de Derrota")]
    [SerializeField] private Text textoDerrota;
    #endregion

    #region Configurações Visuais
    [Header("Cores de Madeira (igual ao tabuleiro)")]
    [SerializeField] private Color corFundoPrincipal = new Color(0.28f, 0.18f, 0.10f, 0.98f);   // Ébano (igual moldura)
    [SerializeField] private Color corFundoSecundario = new Color(0.35f, 0.25f, 0.15f, 0.95f); // Madeira média
    [SerializeField] private Color corTexto = new Color(1f, 0.95f, 0.85f, 1f);                  // Creme claro
    [SerializeField] private Color corDestaque = new Color(1f, 0.88f, 0.35f, 1f);              // Dourado
    [SerializeField] private Color corProgresso = new Color(0.3f, 0.75f, 0.4f, 1f);            // Verde esmeralda
    [SerializeField] private Color corBordaDourada = new Color(0.75f, 0.6f, 0.3f, 1f);         // Dourado moldura
    [SerializeField] private Color corBordaInterna = new Color(0.5f, 0.4f, 0.25f, 0.7f);       // Madeira clara
    [SerializeField] private Color corMadeiraClara = new Color(0.82f, 0.68f, 0.52f, 1f);       // Maple claro
    [SerializeField] private Color corMadeiraEscura = new Color(0.58f, 0.42f, 0.28f, 1f);      // Nogueira

    // Compatibilidade
    private Color corFundo => corFundoPrincipal;
    private Color corBorda => corBordaDourada;
    #endregion

    #region Referências
    private GerenciadorJogo gerenciador;
    private Tabuleiro tabuleiro;
    private Canvas canvas;
    private RectTransform canvasRect;
    private Coroutine coroutineMensagem;
    private Coroutine coroutineFlashMoeda;
    private bool layoutCalculado = false;
    private int ultimasMoedas = 0;
    #endregion

    #region Singleton
    public static InterfaceJogo Instancia { get; private set; }
    public static InterfaceJogo Instance => Instancia;
    public GameObject PainelPausa => painelPausa;
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
    }

    void Start()
    {
        StartCoroutine(Inicializar());
    }

    void Update()
    {
        if (gerenciador != null)
        {
            AtualizarInterface();
        }

        // Recalcular layout se a tela mudar de tamanho
        if (layoutCalculado && canvas != null)
        {
            AjustarLayoutResponsivo();
        }
    }
    #endregion

    #region Inicialização
    private IEnumerator Inicializar()
    {
        // Aguardar gerenciador
        while (gerenciador == null)
        {
            gerenciador = GerenciadorJogo.Instancia;
            yield return new WaitForSeconds(0.1f);
        }

        // Aguardar tabuleiro
        while (tabuleiro == null)
        {
            tabuleiro = FindObjectOfType<Tabuleiro>();
            yield return new WaitForSeconds(0.1f);
        }

        // Aguardar canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        yield return new WaitForSeconds(0.2f);

        if (painelHUD != null)
        {
            yield break;
        }

        CriarElementosUI();
        layoutCalculado = true;

        Debug.Log("✓ InterfaceJogo responsiva inicializada!");
    }

    private void CriarElementosUI()
    {
        if (canvas == null) return;

        LimparHUDsAntigos();
        CriarPainelPrincipal(canvas.transform);
        CriarPainelTutorial(canvas.transform);
        CriarAreaMensagens(canvas.transform);
        CriarPainelVitoria(canvas.transform);
        CriarPainelPausa(canvas.transform);

        AjustarLayoutResponsivo();
    }

    private void LimparHUDsAntigos()
    {
        string[] nomesAntigos = { "Game HUD", "HUD", "Progress Bar Container", "Painel HUD", "Painel Tutorial" };
        foreach (string nome in nomesAntigos)
        {
            GameObject antigo = GameObject.Find(nome);
            if (antigo != null && antigo != painelHUD && antigo != painelTutorial)
            {
                Destroy(antigo);
            }
        }
    }
    #endregion

    #region Layout Responsivo
    private void AjustarLayoutResponsivo()
    {
        if (painelHUD == null || tabuleiro == null || canvasRect == null) return;

        RectTransform tabuleiroRect = tabuleiro.GetComponent<RectTransform>();
        if (tabuleiroRect == null) return;

        // Obter bounds do tabuleiro em coordenadas do canvas
        Vector3[] tabuleiroCorners = new Vector3[4];
        tabuleiroRect.GetWorldCorners(tabuleiroCorners);

        // Converter para coordenadas locais do canvas
        Vector2 tabuleiroMin = Vector2.zero;
        Vector2 tabuleiroMax = Vector2.zero;

        for (int i = 0; i < 4; i++)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(null, tabuleiroCorners[i]),
                null,
                out localPoint
            );

            if (i == 0)
            {
                tabuleiroMin = localPoint;
                tabuleiroMax = localPoint;
            }
            else
            {
                tabuleiroMin = Vector2.Min(tabuleiroMin, localPoint);
                tabuleiroMax = Vector2.Max(tabuleiroMax, localPoint);
            }
        }

        // Calcular dimensões e posição do tabuleiro
        float tabuleiroAltura = tabuleiroMax.y - tabuleiroMin.y;
        float tabuleiroCentroY = (tabuleiroMax.y + tabuleiroMin.y) / 2f;
        float tabuleiroDireita = tabuleiroMax.x;
        float tabuleiroEsquerda = tabuleiroMin.x;

        // Posicionar HUD à direita do tabuleiro
        RectTransform hudRect = painelHUD.GetComponent<RectTransform>();
        PosicionarAoLadoDoTabuleiro(hudRect, tabuleiroDireita, tabuleiroCentroY, tabuleiroAltura, true);

        // Posicionar Tutorial à esquerda do tabuleiro
        if (painelTutorial != null)
        {
            RectTransform tutorialRect = painelTutorial.GetComponent<RectTransform>();
            PosicionarAoLadoDoTabuleiro(tutorialRect, tabuleiroEsquerda, tabuleiroCentroY, tabuleiroAltura, false);
        }
    }

    private void PosicionarAoLadoDoTabuleiro(RectTransform rect, float tabuleiroX, float tabuleiroCentroY, float tabuleiroAltura, bool ladoDireito)
    {
        float largura = 150f;
        float margem = 12f;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        if (ladoDireito)
        {
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(tabuleiroX + margem, tabuleiroCentroY);
        }
        else
        {
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(tabuleiroX - margem, tabuleiroCentroY);
        }

        rect.sizeDelta = new Vector2(largura, tabuleiroAltura);
    }

    private void PosicionarLateral(RectTransform hudRect, float tabuleiroX, float canvasWidth, float canvasHeight, float margem)
    {
        // Mantido para compatibilidade - não será usado
        float larguraHUD = 150f;
        hudRect.anchorMin = new Vector2(0.5f, 0.5f);
        hudRect.anchorMax = new Vector2(0.5f, 0.5f);
        hudRect.pivot = new Vector2(0f, 0.5f);
        hudRect.sizeDelta = new Vector2(larguraHUD, 400f);
        hudRect.anchoredPosition = new Vector2(tabuleiroX + margem, 0f);
    }

    private void PosicionarInferior(RectTransform hudRect, float tabuleiroY, float canvasWidth, float canvasHeight, float margem)
    {
        // Altura fixa minimalista
        float alturaHUD = 70f;

        hudRect.anchorMin = new Vector2(0.15f, 0f);
        hudRect.anchorMax = new Vector2(0.85f, 0f);
        hudRect.pivot = new Vector2(0.5f, 0f);
        hudRect.sizeDelta = new Vector2(0f, alturaHUD);
        hudRect.anchoredPosition = new Vector2(0f, 8f);
    }

    private void AjustarLayoutVertical()
    {
        // Layout já configurado na criação
    }

    private void AjustarLayoutHorizontal()
    {
        // Layout horizontal não necessário para design minimalista
    }

    private void AjustarFontes(bool compacto)
    {
        // Fontes fixas no design minimalista
    }
    #endregion

    #region Criação do Painel Principal
    private void CriarPainelPrincipal(Transform parent)
    {
        // === PAINEL PRINCIPAL (posição será ajustada pelo AjustarLayoutResponsivo) ===
        GameObject painel = new GameObject("Painel HUD");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        // Posição inicial temporária - será recalculada
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(150f, 400f);
        rt.anchoredPosition = new Vector2(200f, 0f);

        // Fundo de madeira escura (igual moldura do tabuleiro)
        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.18f, 0.12f, 0.07f, 1f); // Ébano profundo

        // Borda dourada externa (igual tabuleiro)
        Outline bordaExterna = painel.AddComponent<Outline>();
        bordaExterna.effectColor = new Color(0.75f, 0.6f, 0.3f, 0.8f);
        bordaExterna.effectDistance = new Vector2(2f, 2f);

        // Sombra profunda
        Shadow sombraPainel = painel.AddComponent<Shadow>();
        sombraPainel.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sombraPainel.effectDistance = new Vector2(6f, -6f);

        painelHUD = painel;

        // === BORDA INTERNA DECORATIVA ===
        GameObject bordaInterna = new GameObject("BordaInterna");
        bordaInterna.transform.SetParent(painel.transform, false);

        RectTransform rtBorda = bordaInterna.AddComponent<RectTransform>();
        rtBorda.anchorMin = Vector2.zero;
        rtBorda.anchorMax = Vector2.one;
        rtBorda.offsetMin = new Vector2(2f, 2f);
        rtBorda.offsetMax = new Vector2(-2f, -2f);

        Image imgBorda = bordaInterna.AddComponent<Image>();
        imgBorda.color = new Color(0.35f, 0.25f, 0.15f, 1f); // Madeira média

        Outline contornoBorda = bordaInterna.AddComponent<Outline>();
        contornoBorda.effectColor = new Color(0.5f, 0.4f, 0.25f, 0.6f);
        contornoBorda.effectDistance = new Vector2(-1f, 1f); // Brilho no topo

        // === CONTAINER DE CONTEÚDO ===
        GameObject conteudo = new GameObject("Conteudo");
        conteudo.transform.SetParent(painel.transform, false);

        RectTransform rtConteudo = conteudo.AddComponent<RectTransform>();
        rtConteudo.anchorMin = Vector2.zero;
        rtConteudo.anchorMax = Vector2.one;
        rtConteudo.offsetMin = new Vector2(3f, 3f);
        rtConteudo.offsetMax = new Vector2(-3f, -3f);

        VerticalLayoutGroup layout = conteudo.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(2, 2, 2, 2);
        layout.spacing = 2f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childAlignment = TextAnchor.UpperCenter;

        // === CRIAR ELEMENTOS COM VISUAL DE MADEIRA ===
        CriarTituloHUD(conteudo.transform);
        CriarItemVidasPremium(conteudo.transform);
        CriarItemHUDCompacto(conteudo.transform, "PONTOS", ref textoPontuacao, corDestaque, 22, true);
        CriarItemHUDCompacto(conteudo.transform, "POWER-UP", ref textoPowerUps, new Color(0.85f, 0.65f, 1f), 10, false);
        CriarItemHUDCompacto(conteudo.transform, "NÍVEL", ref textoNivel, new Color(0.6f, 0.9f, 1f), 17, false);
        CriarItemHUDCompacto(conteudo.transform, "TEMPO", ref textoTempo, corTexto, 16, false);
        CriarBarraProgressoCompacta(conteudo.transform);
    }

    #region Painel Tutorial
    private void CriarPainelTutorial(Transform parent)
    {
        GameObject painel = new GameObject("Painel Tutorial");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(150f, 400f);
        rt.anchoredPosition = new Vector2(-200f, 0f);

        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.18f, 0.12f, 0.07f, 1f);

        Outline bordaExterna = painel.AddComponent<Outline>();
        bordaExterna.effectColor = new Color(0.75f, 0.6f, 0.3f, 0.8f);
        bordaExterna.effectDistance = new Vector2(2f, 2f);

        Shadow sombraPainel = painel.AddComponent<Shadow>();
        sombraPainel.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sombraPainel.effectDistance = new Vector2(-6f, -6f);

        painelTutorial = painel;

        GameObject bordaInterna = new GameObject("BordaInterna");
        bordaInterna.transform.SetParent(painel.transform, false);

        RectTransform rtBorda = bordaInterna.AddComponent<RectTransform>();
        rtBorda.anchorMin = Vector2.zero;
        rtBorda.anchorMax = Vector2.one;
        rtBorda.offsetMin = new Vector2(2f, 2f);
        rtBorda.offsetMax = new Vector2(-2f, -2f);

        Image imgBorda = bordaInterna.AddComponent<Image>();
        imgBorda.color = new Color(0.35f, 0.25f, 0.15f, 1f);

        Outline contornoBorda = bordaInterna.AddComponent<Outline>();
        contornoBorda.effectColor = new Color(0.5f, 0.4f, 0.25f, 0.6f);
        contornoBorda.effectDistance = new Vector2(-1f, 1f);

        GameObject conteudo = new GameObject("Conteudo");
        conteudo.transform.SetParent(painel.transform, false);

        RectTransform rtConteudo = conteudo.AddComponent<RectTransform>();
        rtConteudo.anchorMin = Vector2.zero;
        rtConteudo.anchorMax = Vector2.one;
        rtConteudo.offsetMin = new Vector2(3f, 3f);
        rtConteudo.offsetMax = new Vector2(-3f, -3f);

        VerticalLayoutGroup layout = conteudo.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 3f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childAlignment = TextAnchor.UpperCenter;

        CriarTituloTutorial(conteudo.transform);
        CriarSecaoObjetivo(conteudo.transform);
        CriarSecaoControles(conteudo.transform);
        CriarSecaoItens(conteudo.transform);
        CriarSecaoDicas(conteudo.transform);
    }

    private void CriarTituloTutorial(Transform parent)
    {
        GameObject titulo = new GameObject("TituloTutorial");
        titulo.transform.SetParent(parent, false);

        LayoutElement le = titulo.AddComponent<LayoutElement>();
        le.preferredHeight = 24f;
        le.flexibleWidth = 1f;

        Image fundoTitulo = titulo.AddComponent<Image>();
        fundoTitulo.color = new Color(0.45f, 0.32f, 0.2f, 0.6f);

        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(titulo.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        Text textoTitulo = textoObj.AddComponent<Text>();
        textoTitulo.text = "📖 TUTORIAL";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 22;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(1f, 0.9f, 0.65f);
        textoTitulo.alignment = TextAnchor.MiddleCenter;

        Outline brilho = textoObj.AddComponent<Outline>();
        brilho.effectColor = new Color(0.15f, 0.1f, 0.05f, 0.9f);
        brilho.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarSecaoObjetivo(Transform parent)
    {
        GameObject secao = new GameObject("SecaoObjetivo");
        secao.transform.SetParent(parent, false);

        LayoutElement le = secao.AddComponent<LayoutElement>();
        le.preferredHeight = 70f;
        le.flexibleWidth = 1f;

        Image fundo = secao.AddComponent<Image>();
        fundo.color = new Color(0.38f, 0.26f, 0.16f, 0.5f);

        // 🔹 Layout vertical interno
        VerticalLayoutGroup layout = secao.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 2f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 🔹 Título da seção
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(secao.transform, false);

        Text textoTitulo = tituloObj.AddComponent<Text>();
        textoTitulo.text = "🎯 OBJETIVO";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 16;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(1f, 0.85f, 0.4f);
        textoTitulo.alignment = TextAnchor.MiddleLeft;

        // 🔹 Conteúdo
        GameObject conteudoObj = new GameObject("Conteudo");
        conteudoObj.transform.SetParent(secao.transform, false);

        Text textoConteudo = conteudoObj.AddComponent<Text>();
        textoConteudo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoConteudo.fontSize = 13;
        textoConteudo.color = new Color(0.9f, 0.85f, 0.75f);
        textoConteudo.alignment = TextAnchor.UpperLeft;
        textoConteudo.horizontalOverflow = HorizontalWrapMode.Wrap;
        textoConteudo.verticalOverflow = VerticalWrapMode.Overflow;

        int nivelMaximo = 0;

        if (gerenciador != null)
        {
            var campoNivelMaximo = gerenciador.GetType().GetField(
                "nivelMaximo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            if (campoNivelMaximo != null)
            {
                object valor = campoNivelMaximo.GetValue(gerenciador);
                if (valor is int)
                    nivelMaximo = (int)valor;
            }
        }

        textoConteudo.text =
            $"Colete todas as moedas para\n" +
            $"passar de fase e alcance\n" +
            $"o nível {nivelMaximo} para vencer!";
    }


    private void CriarSecaoControles(Transform parent)
    {
        GameObject secao = new GameObject("SecaoControles");
        secao.transform.SetParent(parent, false);

        LayoutElement le = secao.AddComponent<LayoutElement>();
        le.preferredHeight = 60f;
        le.flexibleWidth = 1f;

        Image fundo = secao.AddComponent<Image>();
        fundo.color = new Color(0.38f, 0.26f, 0.16f, 0.5f);

        // Título
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(secao.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 0.75f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.offsetMin = new Vector2(4f, 0f);
        rtTitulo.offsetMax = new Vector2(-4f, 0f);

        Text textoTitulo = tituloObj.AddComponent<Text>();
        textoTitulo.text = "🎮 CONTROLES";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 16;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(0.6f, 0.9f, 1f);
        textoTitulo.alignment = TextAnchor.MiddleLeft;

        // Conteúdo
        GameObject conteudoObj = new GameObject("Conteudo");
        conteudoObj.transform.SetParent(secao.transform, false);

        RectTransform rtConteudo = conteudoObj.AddComponent<RectTransform>();
        rtConteudo.anchorMin = new Vector2(0f, 0f);
        rtConteudo.anchorMax = new Vector2(1f, 0.75f);
        rtConteudo.offsetMin = new Vector2(4f, 2f);
        rtConteudo.offsetMax = new Vector2(-4f, 0f);

        Text textoConteudo = conteudoObj.AddComponent<Text>();
        textoConteudo.text = "⬆️ W ou ↑\n⬇️ S ou ↓\n⬅️ A ou ←\n➡️ D ou →";
        textoConteudo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoConteudo.fontSize = 13;
        textoConteudo.color = new Color(0.9f, 0.85f, 0.75f);
        textoConteudo.alignment = TextAnchor.UpperLeft;
    }

    private void CriarSecaoItens(Transform parent)
    {
        GameObject secao = new GameObject("SecaoItens");
        secao.transform.SetParent(parent, false);

        LayoutElement le = secao.AddComponent<LayoutElement>();
        le.preferredHeight = 70f;
        le.flexibleWidth = 1f;

        Image fundo = secao.AddComponent<Image>();
        fundo.color = new Color(0.38f, 0.26f, 0.16f, 0.5f);

        // Título
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(secao.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 0.82f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.offsetMin = new Vector2(4f, 0f);
        rtTitulo.offsetMax = new Vector2(-4f, 0f);

        Text textoTitulo = tituloObj.AddComponent<Text>();
        textoTitulo.text = "✨ ITENS";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 16;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(0.85f, 0.65f, 1f);
        textoTitulo.alignment = TextAnchor.MiddleLeft;

        // Conteúdo
        GameObject conteudoObj = new GameObject("Conteudo");
        conteudoObj.transform.SetParent(secao.transform, false);

        RectTransform rtConteudo = conteudoObj.AddComponent<RectTransform>();
        rtConteudo.anchorMin = new Vector2(0f, 0f);
        rtConteudo.anchorMax = new Vector2(1f, 0.82f);
        rtConteudo.offsetMin = new Vector2(4f, 2f);
        rtConteudo.offsetMax = new Vector2(-4f, 0f);

        Text textoConteudo = conteudoObj.AddComponent<Text>();
        textoConteudo.text = "🪙 Moeda = +10 pts\n⚡ Velocidade\n🛡️ Invencibilidade\n❌ Evite inimigos!";
        textoConteudo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoConteudo.fontSize = 13;
        textoConteudo.color = new Color(0.9f, 0.85f, 0.75f);
        textoConteudo.alignment = TextAnchor.UpperLeft;
    }

    private void CriarSecaoDicas(Transform parent)
    {
        GameObject secao = new GameObject("SecaoDicas");
        secao.transform.SetParent(parent, false);

        LayoutElement le = secao.AddComponent<LayoutElement>();
        le.preferredHeight = 45f;
        le.flexibleWidth = 1f;

        Image fundo = secao.AddComponent<Image>();
        fundo.color = new Color(0.4f, 0.28f, 0.18f, 0.6f);

        Outline borda = secao.AddComponent<Outline>();
        borda.effectColor = new Color(0.6f, 0.45f, 0.25f, 0.4f);
        borda.effectDistance = new Vector2(1f, 1f);

        // Título
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(secao.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 0.65f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.offsetMin = new Vector2(4f, 0f);
        rtTitulo.offsetMax = new Vector2(-4f, 0f);

        Text textoTitulo = tituloObj.AddComponent<Text>();
        textoTitulo.text = "💡 DICA";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 16;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(0.3f, 0.85f, 0.4f);
        textoTitulo.alignment = TextAnchor.MiddleLeft;

        // Conteúdo
        GameObject conteudoObj = new GameObject("Conteudo");
        conteudoObj.transform.SetParent(secao.transform, false);

        RectTransform rtConteudo = conteudoObj.AddComponent<RectTransform>();
        rtConteudo.anchorMin = new Vector2(0f, 0f);
        rtConteudo.anchorMax = new Vector2(1f, 0.65f);
        rtConteudo.offsetMin = new Vector2(4f, 2f);
        rtConteudo.offsetMax = new Vector2(-4f, 0f);

        Text textoConteudo = conteudoObj.AddComponent<Text>();
        textoConteudo.text = "Inimigos ficam mais\nlentos quando longe!";
        textoConteudo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoConteudo.fontSize = 13;
        textoConteudo.color = new Color(0.9f, 0.95f, 0.85f);
        textoConteudo.alignment = TextAnchor.UpperLeft;
    }
    #endregion

    private void CriarTituloHUD(Transform parent)
    {
        GameObject titulo = new GameObject("TituloHUD");
        titulo.transform.SetParent(parent, false);

        LayoutElement le = titulo.AddComponent<LayoutElement>();
        le.preferredHeight = 22f;
        le.flexibleWidth = 1f;

        // Fundo sutil de madeira clara
        Image fundoTitulo = titulo.AddComponent<Image>();
        fundoTitulo.color = new Color(0.45f, 0.32f, 0.2f, 0.6f);

        // Texto do título
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(titulo.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        Text textoTitulo = textoObj.AddComponent<Text>();
        textoTitulo.text = "◆ STATUS ◆";
        textoTitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoTitulo.fontSize = 16;
        textoTitulo.fontStyle = FontStyle.Bold;
        textoTitulo.color = new Color(1f, 0.9f, 0.65f); // Dourado claro
        textoTitulo.alignment = TextAnchor.MiddleCenter;

        Outline brilho = textoObj.AddComponent<Outline>();
        brilho.effectColor = new Color(0.15f, 0.1f, 0.05f, 0.9f);
        brilho.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarItemVidasPremium(Transform parent)
    {
        GameObject container = new GameObject("Item_Vidas");
        container.transform.SetParent(parent, false);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 26f;
        le.flexibleWidth = 1f;

        // Fundo de madeira com vermelho sutil
        Image fundoVidas = container.AddComponent<Image>();
        fundoVidas.color = new Color(0.4f, 0.22f, 0.15f, 0.7f);

        Outline bordaVidas = container.AddComponent<Outline>();
        bordaVidas.effectColor = new Color(0.7f, 0.35f, 0.25f, 0.5f);
        bordaVidas.effectDistance = new Vector2(1f, 1f);

        // Label "VIDAS"
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0f, 0.5f);
        rtLabel.anchorMax = new Vector2(0.35f, 1f);
        rtLabel.offsetMin = new Vector2(6f, 0f);
        rtLabel.offsetMax = Vector2.zero;

        Text labelTexto = labelObj.AddComponent<Text>();
        labelTexto.text = "VIDAS";
        labelTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelTexto.fontSize = 11;
        labelTexto.color = new Color(0.75f, 0.6f, 0.5f);
        labelTexto.alignment = TextAnchor.MiddleLeft;

        // Valor das vidas (corações)
        GameObject valorObj = new GameObject("Valor");
        valorObj.transform.SetParent(container.transform, false);

        RectTransform rtValor = valorObj.AddComponent<RectTransform>();
        rtValor.anchorMin = new Vector2(0f, 0f);
        rtValor.anchorMax = new Vector2(1f, 0.65f);
        rtValor.offsetMin = new Vector2(4f, 2f);
        rtValor.offsetMax = new Vector2(-4f, 0f);

        textoVidas = valorObj.AddComponent<Text>();
        textoVidas.text = "❤️❤️❤️";
        textoVidas.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoVidas.fontSize = 22;
        textoVidas.color = new Color(1f, 0.35f, 0.35f);
        textoVidas.alignment = TextAnchor.MiddleCenter;

        Outline outline = valorObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.05f, 0.02f, 0.9f);
        outline.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarSeparadorElegante(Transform parent)
    {
        GameObject sep = new GameObject("SeparadorElegante");
        sep.transform.SetParent(parent, false);

        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 4f;
        le.flexibleWidth = 1f;

        // Linha central
        GameObject linhaCentral = new GameObject("LinhaCentral");
        linhaCentral.transform.SetParent(sep.transform, false);

        RectTransform rtLinha = linhaCentral.AddComponent<RectTransform>();
        rtLinha.anchorMin = new Vector2(0.15f, 0.4f);
        rtLinha.anchorMax = new Vector2(0.85f, 0.6f);
        rtLinha.offsetMin = Vector2.zero;
        rtLinha.offsetMax = Vector2.zero;

        Image linha = linhaCentral.AddComponent<Image>();
        linha.color = corBordaDourada * 0.5f;
    }

    private void CriarSeparadorFino(Transform parent)
    {
        GameObject sep = new GameObject("LinhaFina");
        sep.transform.SetParent(parent, false);

        Image linha = sep.AddComponent<Image>();
        linha.color = new Color(0.5f, 0.4f, 0.25f, 0.5f); // Tom de madeira

        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 1f;
        le.flexibleWidth = 1f;
    }

    private void CriarSeparadorMadeira(Transform parent)
    {
        GameObject sep = new GameObject("SeparadorMadeira");
        sep.transform.SetParent(parent, false);

        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 2f;
        le.flexibleWidth = 1f;

        // Linha central com gradiente de madeira
        GameObject linhaCentral = new GameObject("LinhaCentral");
        linhaCentral.transform.SetParent(sep.transform, false);

        RectTransform rtLinha = linhaCentral.AddComponent<RectTransform>();
        rtLinha.anchorMin = new Vector2(0.1f, 0.3f);
        rtLinha.anchorMax = new Vector2(0.9f, 0.7f);
        rtLinha.offsetMin = Vector2.zero;
        rtLinha.offsetMax = Vector2.zero;

        Image linha = linhaCentral.AddComponent<Image>();
        linha.color = new Color(0.55f, 0.42f, 0.28f, 0.8f); // Nogueira

        // Brilho superior (simula textura de madeira)
        Outline brilho = linhaCentral.AddComponent<Outline>();
        brilho.effectColor = new Color(0.75f, 0.6f, 0.35f, 0.4f);
        brilho.effectDistance = new Vector2(0f, 1f);
    }

    private void CriarItemHUDCompacto(Transform parent, string label, ref Text textoRef, Color cor, int tamanhoFonte, bool destacar)
    {
        GameObject container = new GameObject($"Item_{label}");
        container.transform.SetParent(parent, false);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = tamanhoFonte + 4f; // Altura mínima
        le.flexibleWidth = 1f;

        // Fundo de madeira clara/média
        Image fundoItem = container.AddComponent<Image>();
        if (destacar)
        {
            fundoItem.color = new Color(0.45f, 0.32f, 0.2f, 0.7f);
        }
        else
        {
            fundoItem.color = new Color(0.38f, 0.26f, 0.16f, 0.5f);
        }

        Outline bordaItem = container.AddComponent<Outline>();
        bordaItem.effectColor = new Color(0.55f, 0.42f, 0.28f, destacar ? 0.6f : 0.3f);
        bordaItem.effectDistance = new Vector2(1f, 1f);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0f, 0f);
        rtLabel.anchorMax = new Vector2(0.45f, 1f);
        rtLabel.offsetMin = new Vector2(4f, 0f);
        rtLabel.offsetMax = new Vector2(0f, 0f);

        Text textoLabel = labelObj.AddComponent<Text>();
        textoLabel.text = label;
        textoLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoLabel.fontSize = 12;
        textoLabel.fontStyle = FontStyle.Normal;
        textoLabel.color = new Color(0.85f, 0.75f, 0.6f);
        textoLabel.alignment = TextAnchor.MiddleLeft;

        GameObject valorObj = new GameObject("Valor");
        valorObj.transform.SetParent(container.transform, false);

        RectTransform rtValor = valorObj.AddComponent<RectTransform>();
        rtValor.anchorMin = new Vector2(0.45f, 0f);
        rtValor.anchorMax = new Vector2(1f, 1f);
        rtValor.offsetMin = new Vector2(0f, 0f);
        rtValor.offsetMax = new Vector2(-4f, 0f);

        textoRef = valorObj.AddComponent<Text>();
        textoRef.text = "0";
        textoRef.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoRef.fontSize = tamanhoFonte;
        textoRef.fontStyle = FontStyle.Bold;
        textoRef.color = cor;
        textoRef.alignment = TextAnchor.MiddleRight;

        Outline outline = valorObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.06f, 0.02f, 0.95f);
        outline.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarBarraProgressoCompacta(Transform parent)
    {
        GameObject container = new GameObject("Progresso");
        container.transform.SetParent(parent, false);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 16f;
        le.flexibleWidth = 1f;

        // Fundo de madeira
        Image fundoContainer = container.AddComponent<Image>();
        fundoContainer.color = new Color(0.38f, 0.26f, 0.16f, 0.6f);

        // Label no topo - fonte maior
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0f, 0.65f);
        rtLabel.anchorMax = new Vector2(1f, 1f);
        rtLabel.offsetMin = Vector2.zero;
        rtLabel.offsetMax = Vector2.zero;

        Text textoLabel = labelObj.AddComponent<Text>();
        textoLabel.text = "PROGRESSO";
        textoLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoLabel.fontSize = 11;
        textoLabel.color = new Color(0.85f, 0.75f, 0.6f);
        textoLabel.alignment = TextAnchor.MiddleCenter;

        // Container da barra
        GameObject barraContainer = new GameObject("BarraContainer");
        barraContainer.transform.SetParent(container.transform, false);

        RectTransform rtBarra = barraContainer.AddComponent<RectTransform>();
        rtBarra.anchorMin = new Vector2(0.04f, 0.1f);
        rtBarra.anchorMax = new Vector2(0.96f, 0.55f);
        rtBarra.offsetMin = Vector2.zero;
        rtBarra.offsetMax = Vector2.zero;

        Image fundoBarra = barraContainer.AddComponent<Image>();
        fundoBarra.color = new Color(0.2f, 0.14f, 0.08f, 1f); // Madeira escura

        Outline bordaBarra = barraContainer.AddComponent<Outline>();
        bordaBarra.effectColor = new Color(0.55f, 0.42f, 0.28f, 0.6f);
        bordaBarra.effectDistance = new Vector2(1f, 1f);

        containerProgresso = barraContainer;

        // Preenchimento
        GameObject preencher = new GameObject("Fill");
        preencher.transform.SetParent(barraContainer.transform, false);

        RectTransform rtFill = preencher.AddComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = new Vector2(2f, 2f);
        rtFill.offsetMax = new Vector2(-2f, -2f);

        barraProgresso = preencher.AddComponent<Image>();
        barraProgresso.color = new Color(0.35f, 0.75f, 0.45f, 1f); // Verde esmeralda
        barraProgresso.type = Image.Type.Filled;
        barraProgresso.fillMethod = Image.FillMethod.Horizontal;
        barraProgresso.fillAmount = 0f;

        // Texto percentual - fonte maior
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(barraContainer.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        textoProgresso = textoObj.AddComponent<Text>();
        textoProgresso.text = "0%";
        textoProgresso.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoProgresso.fontSize = 13;
        textoProgresso.fontStyle = FontStyle.Bold;
        textoProgresso.color = new Color(1f, 0.98f, 0.9f);
        textoProgresso.alignment = TextAnchor.MiddleCenter;

        Outline outlineTexto = textoObj.AddComponent<Outline>();
        outlineTexto.effectColor = new Color(0.1f, 0.06f, 0.02f, 0.95f);
        outlineTexto.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarItemHUDPremium(Transform parent, string label, ref Text textoRef, Color cor, int tamanhoFonte, bool destacar)
    {
        CriarItemHUDCompacto(parent, label, ref textoRef, cor, tamanhoFonte, destacar);
    }

    private void CriarBarraProgressoPremium(Transform parent)
    {
        CriarBarraProgressoCompacta(parent);
    }

    // Manter compatibilidade com métodos antigos
    private void CriarItemVidas(Transform parent) => CriarItemVidasPremium(parent);
    private void CriarLinhaFina(Transform parent) => CriarSeparadorFino(parent);
    private void CriarItemHUD(Transform parent, string label, ref Text textoRef, Color cor, int tamanhoFonte)
        => CriarItemHUDPremium(parent, label, ref textoRef, cor, tamanhoFonte, false);
    private void CriarBarraProgressoSimples(Transform parent) => CriarBarraProgressoPremium(parent);

    private void AdicionarContorno(GameObject obj)
    {
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, 1f);
    }
    #endregion

    #region Área de Mensagens
    private void CriarAreaMensagens(Transform parent)
    {
        GameObject msg = new GameObject("Painel Mensagem");
        msg.transform.SetParent(parent, false);

        RectTransform rt = msg.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.92f);
        rt.anchorMax = new Vector2(0.5f, 0.99f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500f, 50f);
        rt.anchoredPosition = new Vector2(0f, 0f);

        Image fundo = msg.AddComponent<Image>();
        fundo.color = new Color(0f, 0f, 0f, 0.85f);

        Outline borda = msg.AddComponent<Outline>();
        borda.effectColor = corDestaque;
        borda.effectDistance = new Vector2(2f, 2f);

        painelMensagem = msg;

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(msg.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = new Vector2(10f, 5f);
        rtTexto.offsetMax = new Vector2(-10f, -5f);

        textoMensagem = textoObj.AddComponent<Text>();
        textoMensagem.text = "";
        textoMensagem.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoMensagem.fontSize = 22;
        textoMensagem.fontStyle = FontStyle.Bold;
        textoMensagem.color = corDestaque;
        textoMensagem.alignment = TextAnchor.MiddleCenter;

        AdicionarContorno(textoObj);

        msg.SetActive(false);
    }
    #endregion

    #region Painel de Vitória
    private void CriarPainelVitoria(Transform parent)
    {
        GameObject painel = new GameObject("Painel Vitória");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0f, 0f, 0f, 0.85f);

        painelVitoria = painel;

        // Caixa central com gradiente verde
        GameObject caixa = new GameObject("Caixa");
        caixa.transform.SetParent(painel.transform, false);

        RectTransform rtCaixa = caixa.AddComponent<RectTransform>();
        rtCaixa.anchorMin = new Vector2(0.1f, 0.15f);
        rtCaixa.anchorMax = new Vector2(0.9f, 0.85f);
        rtCaixa.offsetMin = Vector2.zero;
        rtCaixa.offsetMax = Vector2.zero;

        Image fundoCaixa = caixa.AddComponent<Image>();
        fundoCaixa.color = new Color(0.05f, 0.25f, 0.1f, 0.98f);

        // Borda dourada dupla
        Outline bordaCaixa = caixa.AddComponent<Outline>();
        bordaCaixa.effectColor = new Color(1f, 0.85f, 0.2f, 1f);
        bordaCaixa.effectDistance = new Vector2(4f, 4f);

        Shadow sombraCaixa = caixa.AddComponent<Shadow>();
        sombraCaixa.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sombraCaixa.effectDistance = new Vector2(8f, -8f);

        // Título "VITÓRIA"
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0.05f, 0.7f);
        rtTitulo.anchorMax = new Vector2(0.95f, 0.95f);
        rtTitulo.offsetMin = Vector2.zero;
        rtTitulo.offsetMax = Vector2.zero;

        Text titulo = tituloObj.AddComponent<Text>();
        titulo.text = "VITÓRIA!";
        titulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titulo.fontSize = 32;
        titulo.fontStyle = FontStyle.Bold;
        titulo.color = new Color(1f, 0.9f, 0.2f, 1f);
        titulo.alignment = TextAnchor.MiddleCenter;

        Outline brilhoTitulo = tituloObj.AddComponent<Outline>();
        brilhoTitulo.effectColor = new Color(1f, 0.7f, 0f, 0.8f);
        brilhoTitulo.effectDistance = new Vector2(2f, 2f);

        // Subtítulo com pontuação
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.1f, 0.4f);
        rtTexto.anchorMax = new Vector2(0.9f, 0.68f);
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        textoVitoria = textoObj.AddComponent<Text>();
        textoVitoria.text = "Parabéns! Você coletou todas as moedas!";
        textoVitoria.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoVitoria.fontSize = 18;
        textoVitoria.fontStyle = FontStyle.Normal;
        textoVitoria.color = new Color(0.9f, 1f, 0.9f, 1f);
        textoVitoria.alignment = TextAnchor.MiddleCenter;

        AdicionarContorno(textoObj);

        // Botões estilizados
        CriarBotaoEstilizado(caixa.transform, "Reiniciar", "JOGAR NOVAMENTE",
            new Vector2(0.1f, 0.08f), new Vector2(0.48f, 0.28f),
            new Color(0.1f, 0.5f, 0.2f, 1f),
            () => gerenciador?.ReiniciarJogo());

        CriarBotaoEstilizado(caixa.transform, "Menu", "MENU PRINCIPAL",
            new Vector2(0.52f, 0.08f), new Vector2(0.9f, 0.28f),
            new Color(0.4f, 0.35f, 0.2f, 1f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"));

        painel.SetActive(false);
    }

    private void CriarPainelPausa(Transform parent)
    {
        GameObject painel = new GameObject("Painel Pausa");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0f, 0f, 0f, 0.75f);

        painelPausa = painel;

        GameObject caixa = new GameObject("Caixa");
        caixa.transform.SetParent(painel.transform, false);

        RectTransform rtCaixa = caixa.AddComponent<RectTransform>();
        rtCaixa.anchorMin = new Vector2(0.2f, 0.2f);
        rtCaixa.anchorMax = new Vector2(0.8f, 0.8f);
        rtCaixa.offsetMin = Vector2.zero;
        rtCaixa.offsetMax = Vector2.zero;

        Image fundoCaixa = caixa.AddComponent<Image>();
        fundoCaixa.color = new Color(0.25f, 0.18f, 0.1f, 0.98f);

        Outline bordaCaixa = caixa.AddComponent<Outline>();
        bordaCaixa.effectColor = new Color(0.75f, 0.6f, 0.3f, 1f);
        bordaCaixa.effectDistance = new Vector2(4f, 4f);

        Shadow sombraCaixa = caixa.AddComponent<Shadow>();
        sombraCaixa.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sombraCaixa.effectDistance = new Vector2(8f, -8f);

        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0.05f, 0.75f);
        rtTitulo.anchorMax = new Vector2(0.95f, 0.95f);
        rtTitulo.offsetMin = Vector2.zero;
        rtTitulo.offsetMax = Vector2.zero;

        Text titulo = tituloObj.AddComponent<Text>();
        titulo.text = "⏸️ PAUSADO";
        titulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titulo.fontSize = 36;
        titulo.fontStyle = FontStyle.Bold;
        titulo.color = new Color(1f, 0.9f, 0.65f, 1f);
        titulo.alignment = TextAnchor.MiddleCenter;

        Outline brilhoTitulo = tituloObj.AddComponent<Outline>();
        brilhoTitulo.effectColor = new Color(0.5f, 0.35f, 0.15f, 0.9f);
        brilhoTitulo.effectDistance = new Vector2(2f, 2f);

        GameObject subObj = new GameObject("Subtitulo");
        subObj.transform.SetParent(caixa.transform, false);

        RectTransform rtSub = subObj.AddComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.1f, 0.6f);
        rtSub.anchorMax = new Vector2(0.9f, 0.75f);
        rtSub.offsetMin = Vector2.zero;
        rtSub.offsetMax = Vector2.zero;

        Text subtitulo = subObj.AddComponent<Text>();
        subtitulo.text = "Pressione ESC para continuar";
        subtitulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitulo.fontSize = 16;
        subtitulo.fontStyle = FontStyle.Italic;
        subtitulo.color = new Color(0.85f, 0.8f, 0.7f, 0.9f);
        subtitulo.alignment = TextAnchor.MiddleCenter;

        CriarBotaoEstilizado(caixa.transform, "Continuar", "▶️ CONTINUAR",
            new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.55f),
            new Color(0.2f, 0.5f, 0.3f, 1f),
            () => gerenciador?.ContinuarJogo());

        CriarBotaoEstilizado(caixa.transform, "Reiniciar", "🔄 REINICIAR",
            new Vector2(0.15f, 0.22f), new Vector2(0.85f, 0.37f),
            new Color(0.5f, 0.4f, 0.2f, 1f),
            () => gerenciador?.ReiniciarJogo());

        CriarBotaoEstilizado(caixa.transform, "Menu", "🏠 MENU PRINCIPAL",
            new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.19f),
            new Color(0.5f, 0.25f, 0.2f, 1f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"));

        painel.SetActive(false);

        if (gerenciador != null)
        {
            var campo = typeof(GerenciadorJogo).GetField("painelPausa",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campo != null)
            {
                campo.SetValue(gerenciador, painelPausa);
            }
        }
    }

    private void CriarBotaoEstilizado(Transform parent, string nome, string texto, Vector2 anchorMin, Vector2 anchorMax, Color corBase, System.Action aoClicar)
    {
        GameObject btn = new GameObject($"Botão {nome}");
        btn.transform.SetParent(parent, false);

        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btn.AddComponent<Image>();
        img.color = corBase;

        // Borda do botão
        Outline bordaBotao = btn.AddComponent<Outline>();
        bordaBotao.effectColor = new Color(corBase.r + 0.3f, corBase.g + 0.3f, corBase.b + 0.2f, 1f);
        bordaBotao.effectDistance = new Vector2(2f, 2f);

        Shadow sombraBotao = btn.AddComponent<Shadow>();
        sombraBotao.effectColor = new Color(0f, 0f, 0f, 0.5f);
        sombraBotao.effectDistance = new Vector2(3f, -3f);

        Button botao = btn.AddComponent<Button>();
        botao.onClick.AddListener(() => aoClicar?.Invoke());

        ColorBlock cores = botao.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cores.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        botao.colors = cores;

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(btn.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = new Vector2(5f, 5f);
        rtTexto.offsetMax = new Vector2(-5f, -5f);

        Text textoBotao = textoObj.AddComponent<Text>();
        textoBotao.text = texto;
        textoBotao.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoBotao.fontSize = 14;
        textoBotao.fontStyle = FontStyle.Bold;
        textoBotao.color = Color.white;
        textoBotao.alignment = TextAnchor.MiddleCenter;

        Outline contornoTexto = textoObj.AddComponent<Outline>();
        contornoTexto.effectColor = new Color(0f, 0f, 0f, 0.8f);
        contornoTexto.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarBotao(Transform parent, string nome, string texto, Vector2 anchorMin, Vector2 anchorMax, System.Action aoClicar)
    {
        CriarBotaoEstilizado(parent, nome, texto, anchorMin, anchorMax, new Color(0.15f, 0.25f, 0.7f, 1f), aoClicar);
    }
    #endregion

    #region Atualização da Interface
    private void AtualizarInterface()
    {
        if (textoPontuacao != null)
            textoPontuacao.text = gerenciador.PontuacaoTotal.ToString("N0");

        if (textoMoedas != null)
        {
            int moedasAtuais = gerenciador.MoedasColetadas;
            textoMoedas.text = moedasAtuais.ToString();

            // Flash visual quando coletou moeda
            if (moedasAtuais > ultimasMoedas)
            {
                FlashMoeda();
                ultimasMoedas = moedasAtuais;
            }
        }

        if (textoNivel != null)
            textoNivel.text = gerenciador.NivelAtual.ToString();

        if (textoTempo != null)
            textoTempo.text = FormatarTempo(gerenciador.TempoDeJogo);

        AtualizarPowerUps();
        AtualizarProgresso();
    }

    private void AtualizarPowerUps()
    {
        if (textoPowerUps == null || gerenciador == null) return;

        List<string> ativosFormatados = new List<string>();

        float f = Mathf.PingPong(Time.time * 6f, 1f); // fator para piscar
        Color corCheia = corDestaque;
        Color corFraca = new Color(corDestaque.r, corDestaque.g, corDestaque.b, 0.25f);

        string CorParaHEX(Color c)
        {
            return ColorUtility.ToHtmlStringRGBA(c);
        }

        // 1️⃣ Velocidade
        if (gerenciador.TemVelocidadeExtra())
        {
            float t = gerenciador.TempoRestanteVelocidadeExtra();
            bool quaseAcabando = t <= 3f;

            Color cor = quaseAcabando ? Color.Lerp(corCheia, corFraca, f) : corCheia;
            ativosFormatados.Add($"<color=#{CorParaHEX(cor)}>Velocidade</color>");
        }

        // 2️⃣ Pontuação Dupla
        if (gerenciador.TemPontuacaoDupla())
        {
            float t = gerenciador.TempoRestantePontuacaoDupla();
            bool quaseAcabando = t <= 3f;

            Color cor = quaseAcabando ? Color.Lerp(corCheia, corFraca, f) : corCheia;
            ativosFormatados.Add($"<color=#{CorParaHEX(cor)}>Pontos x2</color>");
        }

        // 3️⃣ Invencibilidade
        if (gerenciador.TemInvencibilidade())
        {
            float t = gerenciador.TempoRestanteInvencibilidade();
            bool quaseAcabando = t <= 3f;

            Color cor = quaseAcabando ? Color.Lerp(corCheia, corFraca, f) : corCheia;
            ativosFormatados.Add($"<color=#{CorParaHEX(cor)}>Invencivel</color>");
        }

        if (ativosFormatados.Count > 0)
        {
            textoPowerUps.supportRichText = true;
            textoPowerUps.text = string.Join(" ", ativosFormatados);
        }
        else
        {
            textoPowerUps.text = "-";
            textoPowerUps.color = new Color(0.4f, 0.4f, 0.4f);
        }
    }



    private void AtualizarProgresso()
    {
        if (barraProgresso == null || textoProgresso == null || gerenciador == null) return;

        Coletavel[] coletaveis = FindObjectsOfType<Coletavel>();
        int moedasRestantes = 0;
        int moedasColetadas = gerenciador.MoedasColetadas;

        foreach (Coletavel c in coletaveis)
        {
            if (c.Tipo == TipoColetavel.Moeda) moedasRestantes++;
        }

        int total = moedasRestantes + moedasColetadas;
        float progresso = total > 0 ? (float)moedasColetadas / total : 0f;

        barraProgresso.fillAmount = progresso;
        textoProgresso.text = $"{Mathf.RoundToInt(progresso * 100)}%";

        barraProgresso.color = progresso >= 1f ? Color.green :
                              progresso >= 0.5f ? corProgresso :
                              new Color(0.8f, 0.5f, 0.1f);
    }

    private string FormatarTempo(float tempo)
    {
        int min = Mathf.FloorToInt(tempo / 60f);
        int seg = Mathf.FloorToInt(tempo % 60f);
        return $"{min:00}:{seg:00}";
    }
    #endregion

    #region Mensagens Públicas
    public void MostrarMensagem(string mensagem, float duracao = 3f)
    {
        if (coroutineMensagem != null)
            StopCoroutine(coroutineMensagem);
        coroutineMensagem = StartCoroutine(ExibirMensagem(mensagem, duracao));
    }

    public void ShowTemporaryMessage(string message, float duration = 3f) => MostrarMensagem(message, duration);

    private IEnumerator ExibirMensagem(string mensagem, float duracao)
    {
        if (painelMensagem != null && textoMensagem != null)
        {
            painelMensagem.SetActive(true);
            textoMensagem.text = mensagem;
            yield return new WaitForSeconds(duracao);
            painelMensagem.SetActive(false);
        }
    }

    public void MostrarFeedbackColeta(TipoColetavel tipo, int valor)
    {
        // Feedback visual no HUD - não bloqueia visão do tabuleiro
        if (tipo == TipoColetavel.Moeda)
        {
            // Flash já acontece automaticamente na atualização
            FlashPontuacao();
        }
        else if (tipo == TipoColetavel.PowerUp)
        {
            // Power-ups mostram mensagem rápida no topo
            MostrarMensagemRapida("⚡ Power-up ativado!");
        }
    }

    private void FlashMoeda()
    {
        if (coroutineFlashMoeda != null)
            StopCoroutine(coroutineFlashMoeda);
        coroutineFlashMoeda = StartCoroutine(AnimarFlash(textoMoedas, Color.green));
    }

    private void FlashPontuacao()
    {
        StartCoroutine(AnimarFlash(textoPontuacao, corDestaque));
    }

    private IEnumerator AnimarFlash(Text texto, Color corFlash)
    {
        if (texto == null) yield break;

        Color corOriginal = texto.color;
        float duracao = 0.3f;
        float tempo = 0f;

        // Aumentar escala e mudar cor
        texto.transform.localScale = Vector3.one * 1.3f;
        texto.color = corFlash;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            texto.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t);
            texto.color = Color.Lerp(corFlash, corOriginal, t);
            yield return null;
        }

        texto.transform.localScale = Vector3.one;
        texto.color = corOriginal;
    }

    private void MostrarMensagemRapida(string mensagem)
    {
        if (coroutineMensagem != null)
            StopCoroutine(coroutineMensagem);
        coroutineMensagem = StartCoroutine(ExibirMensagemRapida(mensagem));
    }

    private IEnumerator ExibirMensagemRapida(string mensagem)
    {
        if (painelMensagem != null && textoMensagem != null)
        {
            painelMensagem.SetActive(true);
            textoMensagem.text = mensagem;
            yield return new WaitForSeconds(0.8f);
            painelMensagem.SetActive(false);
        }
    }
    #endregion

    #region Painéis de Vitória
    public void MostrarPainelVitoria()
    {
        if (painelVitoria == null || textoVitoria == null || gerenciador == null) return;

        painelVitoria.SetActive(true);
        int bonus = Mathf.Max(0, 300 - Mathf.RoundToInt(gerenciador.TempoDeJogo));

        textoVitoria.text = $"🏆 VITÓRIA! 🏆\n\n" +
                           $"💰 {gerenciador.PontuacaoTotal:N0} pontos\n" +
                           $"⏱️ {FormatarTempo(gerenciador.TempoDeJogo)}\n" +
                           $"🎯 Bônus: +{bonus}\n" +
                           $"🪙 {gerenciador.MoedasColetadas} moedas";
    }

    public void ShowWinPanel() => MostrarPainelVitoria();

    public void MostrarPainelConclusao()
    {
        if (painelVitoria == null || textoVitoria == null || gerenciador == null) return;

        painelVitoria.SetActive(true);

        textoVitoria.text = $"🏆 COMPLETO! 🏆\n\n" +
                           $"🎊 Parabéns!\n\n" +
                           $"💰 {gerenciador.PontuacaoTotal:N0}\n" +
                           $"⏱️ {FormatarTempo(gerenciador.TempoDeJogo)}\n" +
                           $"🚀 Nível {gerenciador.NivelAtual}";
    }

    public void ShowGameCompletionPanel() => MostrarPainelConclusao();
    #endregion

    #region Painel de Derrota
    public void MostrarPainelDerrota(int pontuacao, float tempo, int nivel, int moedas)
    {
        // Criar painel de derrota se não existir
        if (painelDerrota == null)
        {
            CriarPainelDerrota(canvas.transform);
        }

        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);

            if (textoDerrota != null)
            {
                textoDerrota.text = $"💀 GAME OVER 💀\n\n" +
                                   $"💰 {pontuacao:N0} pontos\n" +
                                   $"⏱️ {FormatarTempo(tempo)}\n" +
                                   $"🚀 Nível {nivel}\n" +
                                   $"🪙 {moedas} moedas";
            }
        }
    }

    private void CriarPainelDerrota(Transform parent)
    {
        GameObject painel = new GameObject("Painel Derrota");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.15f, 0f, 0f, 0.85f);

        painelDerrota = painel;

        GameObject caixa = new GameObject("Caixa");
        caixa.transform.SetParent(painel.transform, false);

        RectTransform rtCaixa = caixa.AddComponent<RectTransform>();
        rtCaixa.anchorMin = new Vector2(0.1f, 0.15f);
        rtCaixa.anchorMax = new Vector2(0.9f, 0.85f);
        rtCaixa.offsetMin = Vector2.zero;
        rtCaixa.offsetMax = Vector2.zero;

        Image fundoCaixa = caixa.AddComponent<Image>();
        fundoCaixa.color = new Color(0.2f, 0.03f, 0.03f, 0.98f);

        Outline bordaCaixa = caixa.AddComponent<Outline>();
        bordaCaixa.effectColor = new Color(0.8f, 0.2f, 0.1f, 1f);
        bordaCaixa.effectDistance = new Vector2(4f, 4f);

        Shadow sombraCaixa = caixa.AddComponent<Shadow>();
        sombraCaixa.effectColor = new Color(0f, 0f, 0f, 0.8f);
        sombraCaixa.effectDistance = new Vector2(8f, -8f);

        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0.05f, 0.7f);
        rtTitulo.anchorMax = new Vector2(0.95f, 0.95f);
        rtTitulo.offsetMin = Vector2.zero;
        rtTitulo.offsetMax = Vector2.zero;

        Text titulo = tituloObj.AddComponent<Text>();
        titulo.text = "GAME OVER";
        titulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titulo.fontSize = 32;
        titulo.fontStyle = FontStyle.Bold;
        titulo.color = new Color(1f, 0.3f, 0.2f, 1f);
        titulo.alignment = TextAnchor.MiddleCenter;

        Outline brilhoTitulo = tituloObj.AddComponent<Outline>();
        brilhoTitulo.effectColor = new Color(0.5f, 0f, 0f, 0.8f);
        brilhoTitulo.effectDistance = new Vector2(2f, 2f);

        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.1f, 0.4f);
        rtTexto.anchorMax = new Vector2(0.9f, 0.68f);
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        textoDerrota = textoObj.AddComponent<Text>();
        textoDerrota.text = "Você perdeu todas as vidas!";
        textoDerrota.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoDerrota.fontSize = 18;
        textoDerrota.fontStyle = FontStyle.Normal;
        textoDerrota.color = new Color(1f, 0.85f, 0.85f, 1f);
        textoDerrota.alignment = TextAnchor.MiddleCenter;

        AdicionarContorno(textoObj);

        CriarBotaoEstilizado(caixa.transform, "Reiniciar", "TENTAR NOVAMENTE",
            new Vector2(0.1f, 0.08f), new Vector2(0.48f, 0.28f),
            new Color(0.6f, 0.15f, 0.1f, 1f),
            () => gerenciador?.ReiniciarJogo());

        CriarBotaoEstilizado(caixa.transform, "Menu", "MENU PRINCIPAL",
            new Vector2(0.52f, 0.08f), new Vector2(0.9f, 0.28f),
            new Color(0.35f, 0.25f, 0.2f, 1f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"));

        painel.SetActive(false);
    }
    #endregion

    #region Atualização de Vidas
    public void AtualizarVidas(int vidasAtuais, int vidasMaximas)
    {
        if (textoVidas == null) return;

        string coracoes = "";
        for (int i = 0; i < vidasMaximas; i++)
        {
            if (i < vidasAtuais)
                coracoes += "❤️";
            else
                coracoes += "🖤";
        }
        textoVidas.text = coracoes;

        if (vidasAtuais < vidasMaximas)
        {
            StartCoroutine(FlashVidas());
        }
    }

    private IEnumerator FlashVidas()
    {
        if (textoVidas == null) yield break;

        Color corOriginal = textoVidas.color;
        textoVidas.color = Color.white;
        textoVidas.transform.localScale = Vector3.one * 1.4f;

        float tempo = 0f;
        float duracao = 0.3f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            textoVidas.transform.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, t);
            textoVidas.color = Color.Lerp(Color.white, corOriginal, t);
            yield return null;
        }

        textoVidas.transform.localScale = Vector3.one;
        textoVidas.color = corOriginal;
    }
    #endregion
}

public class GameHUD : InterfaceJogo { }
