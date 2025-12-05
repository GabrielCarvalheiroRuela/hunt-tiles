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
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject painelMensagem;

    [Header("Textos do HUD")]
    [SerializeField] private Text textoPontuacao;
    [SerializeField] private Text textoNivel;
    [SerializeField] private Text textoTempo;
    [SerializeField] private Text textoMoedas;
    [SerializeField] private Text textoPowerUps;

    [Header("Barra de Progresso")]
    [SerializeField] private GameObject containerProgresso;
    [SerializeField] private Image barraProgresso;
    [SerializeField] private Text textoProgresso;

    [Header("Mensagens")]
    [SerializeField] private Text textoMensagem;

    [Header("Painel de Vitória")]
    [SerializeField] private Text textoVitoria;
    #endregion

    #region Configurações Visuais
    [Header("Cores")]
    [SerializeField] private Color corFundo = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    [SerializeField] private Color corTexto = Color.white;
    [SerializeField] private Color corDestaque = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color corProgresso = new Color(0.2f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color corBorda = new Color(0.4f, 0.35f, 0.3f, 1f);
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
        CriarAreaMensagens(canvas.transform);
        CriarPainelVitoria(canvas.transform);
        
        AjustarLayoutResponsivo();
    }

    private void LimparHUDsAntigos()
    {
        string[] nomesAntigos = { "Game HUD", "HUD", "Progress Bar Container", "Painel HUD" };
        foreach (string nome in nomesAntigos)
        {
            GameObject antigo = GameObject.Find(nome);
            if (antigo != null && antigo != painelHUD)
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

        // Obter dimensões do canvas
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

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

        // Calcular espaço disponível à direita do tabuleiro
        float espacoDireita = (canvasWidth / 2f) - tabuleiroMax.x;
        
        // Calcular espaço disponível abaixo do tabuleiro
        float espacoAbaixo = tabuleiroMin.y + (canvasHeight / 2f);

        RectTransform hudRect = painelHUD.GetComponent<RectTransform>();

        // Margem de segurança
        float margem = 15f;

        // Decidir posicionamento: lateral ou inferior
        if (espacoDireita >= 180f) // Espaço suficiente à direita
        {
            PosicionarLateral(hudRect, tabuleiroMax.x, canvasWidth, canvasHeight, margem);
        }
        else // Posicionar abaixo do tabuleiro
        {
            PosicionarInferior(hudRect, tabuleiroMin.y, canvasWidth, canvasHeight, margem);
        }
    }

    private void PosicionarLateral(RectTransform hudRect, float tabuleiroX, float canvasWidth, float canvasHeight, float margem)
    {
        // Largura fixa minimalista
        float larguraHUD = 130f;

        hudRect.anchorMin = new Vector2(1f, 0.15f);
        hudRect.anchorMax = new Vector2(1f, 0.85f);
        hudRect.pivot = new Vector2(1f, 0.5f);
        hudRect.sizeDelta = new Vector2(larguraHUD, 0f);
        hudRect.anchoredPosition = new Vector2(-8f, 0f);
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
        GameObject painel = new GameObject("Painel HUD");
        painel.transform.SetParent(parent, false);

        RectTransform rt = painel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.15f);
        rt.anchorMax = new Vector2(1f, 0.85f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(140f, 0f);
        rt.anchoredPosition = new Vector2(-10f, 0f);

        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        Outline borda = painel.AddComponent<Outline>();
        borda.effectColor = new Color(0.3f, 0.25f, 0.2f, 0.8f);
        borda.effectDistance = new Vector2(1f, 1f);

        painelHUD = painel;

        VerticalLayoutGroup layout = painel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 12, 12);
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        // Criar elementos minimalistas
        CriarItemHUD(painel.transform, "Pontos", ref textoPontuacao, corDestaque, 18);
        CriarLinhaFina(painel.transform);
        CriarItemHUD(painel.transform, "Moedas", ref textoMoedas, Color.yellow, 16);
        CriarItemHUD(painel.transform, "Nível", ref textoNivel, Color.cyan, 16);
        CriarItemHUD(painel.transform, "Tempo", ref textoTempo, corTexto, 14);
        CriarLinhaFina(painel.transform);
        CriarBarraProgressoSimples(painel.transform);
        CriarLinhaFina(painel.transform);
        CriarItemHUD(painel.transform, "Power", ref textoPowerUps, new Color(0.5f, 0.5f, 0.5f), 11);
    }

    private void CriarLinhaFina(Transform parent)
    {
        GameObject sep = new GameObject("Linha");
        sep.transform.SetParent(parent, false);

        Image linha = sep.AddComponent<Image>();
        linha.color = new Color(1f, 1f, 1f, 0.15f);

        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 1f;
        le.flexibleWidth = 1f;
    }

    private void CriarItemHUD(Transform parent, string label, ref Text textoRef, Color cor, int tamanhoFonte)
    {
        GameObject container = new GameObject($"Item_{label}");
        container.transform.SetParent(parent, false);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = tamanhoFonte + 16f;
        le.flexibleWidth = 1f;

        VerticalLayoutGroup vl = container.AddComponent<VerticalLayoutGroup>();
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.spacing = 1f;

        // Label pequeno
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        Text textoLabel = labelObj.AddComponent<Text>();
        textoLabel.text = label.ToUpper();
        textoLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoLabel.fontSize = 9;
        textoLabel.color = new Color(0.5f, 0.5f, 0.5f);
        textoLabel.alignment = TextAnchor.MiddleCenter;

        LayoutElement leLabel = labelObj.AddComponent<LayoutElement>();
        leLabel.preferredHeight = 10f;

        // Valor
        GameObject valorObj = new GameObject("Valor");
        valorObj.transform.SetParent(container.transform, false);

        textoRef = valorObj.AddComponent<Text>();
        textoRef.text = "0";
        textoRef.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoRef.fontSize = tamanhoFonte;
        textoRef.fontStyle = FontStyle.Bold;
        textoRef.color = cor;
        textoRef.alignment = TextAnchor.MiddleCenter;
        textoRef.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoRef.verticalOverflow = VerticalWrapMode.Overflow;

        // Contorno
        Outline outline = valorObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, 1f);
    }

    private void CriarBarraProgressoSimples(Transform parent)
    {
        GameObject container = new GameObject("Progresso");
        container.transform.SetParent(parent, false);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 24f;
        le.flexibleWidth = 1f;

        // Label
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
        textoLabel.fontSize = 8;
        textoLabel.color = new Color(0.5f, 0.5f, 0.5f);
        textoLabel.alignment = TextAnchor.MiddleCenter;

        // Barra
        GameObject barraContainer = new GameObject("Barra");
        barraContainer.transform.SetParent(container.transform, false);

        RectTransform rtBarra = barraContainer.AddComponent<RectTransform>();
        rtBarra.anchorMin = new Vector2(0.05f, 0f);
        rtBarra.anchorMax = new Vector2(0.95f, 0.6f);
        rtBarra.offsetMin = Vector2.zero;
        rtBarra.offsetMax = Vector2.zero;

        Image fundoBarra = barraContainer.AddComponent<Image>();
        fundoBarra.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        containerProgresso = barraContainer;

        // Preenchimento
        GameObject preencher = new GameObject("Fill");
        preencher.transform.SetParent(barraContainer.transform, false);

        RectTransform rtFill = preencher.AddComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = new Vector2(1f, 1f);
        rtFill.offsetMax = new Vector2(-1f, -1f);

        barraProgresso = preencher.AddComponent<Image>();
        barraProgresso.color = corProgresso;
        barraProgresso.type = Image.Type.Filled;
        barraProgresso.fillMethod = Image.FillMethod.Horizontal;
        barraProgresso.fillAmount = 0f;

        // Texto %
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
        textoProgresso.fontSize = 10;
        textoProgresso.fontStyle = FontStyle.Bold;
        textoProgresso.color = Color.white;
        textoProgresso.alignment = TextAnchor.MiddleCenter;

        Outline outline = textoObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, 1f);
    }

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
        // Posicionar no TOPO da tela, nunca em cima do tabuleiro
        rt.anchorMin = new Vector2(0.1f, 0.92f);
        rt.anchorMax = new Vector2(0.7f, 0.99f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

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
        fundo.color = new Color(0f, 0f, 0f, 0.9f);

        painelVitoria = painel;

        // Caixa central responsiva
        GameObject caixa = new GameObject("Caixa");
        caixa.transform.SetParent(painel.transform, false);

        RectTransform rtCaixa = caixa.AddComponent<RectTransform>();
        rtCaixa.anchorMin = new Vector2(0.15f, 0.2f);
        rtCaixa.anchorMax = new Vector2(0.85f, 0.8f);
        rtCaixa.offsetMin = Vector2.zero;
        rtCaixa.offsetMax = Vector2.zero;

        Image fundoCaixa = caixa.AddComponent<Image>();
        fundoCaixa.color = new Color(0.08f, 0.35f, 0.08f, 0.98f);

        Outline bordaCaixa = caixa.AddComponent<Outline>();
        bordaCaixa.effectColor = corDestaque;
        bordaCaixa.effectDistance = new Vector2(3f, 3f);

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(caixa.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.05f, 0.4f);
        rtTexto.anchorMax = new Vector2(0.95f, 0.95f);
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        textoVitoria = textoObj.AddComponent<Text>();
        textoVitoria.text = "🏆 VITÓRIA! 🏆";
        textoVitoria.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoVitoria.fontSize = 24;
        textoVitoria.fontStyle = FontStyle.Bold;
        textoVitoria.color = Color.yellow;
        textoVitoria.alignment = TextAnchor.MiddleCenter;

        AdicionarContorno(textoObj);

        // Botões
        CriarBotao(caixa.transform, "Reiniciar", "🔄 JOGAR NOVAMENTE",
            new Vector2(0.08f, 0.08f), new Vector2(0.48f, 0.32f),
            () => gerenciador?.ReiniciarJogo());

        CriarBotao(caixa.transform, "Menu", "🏠 MENU",
            new Vector2(0.52f, 0.08f), new Vector2(0.92f, 0.32f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"));

        painel.SetActive(false);
    }

    private void CriarBotao(Transform parent, string nome, string texto, Vector2 anchorMin, Vector2 anchorMax, System.Action aoClicar)
    {
        GameObject btn = new GameObject($"Botão {nome}");
        btn.transform.SetParent(parent, false);

        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.15f, 0.25f, 0.7f, 1f);

        Button botao = btn.AddComponent<Button>();
        botao.onClick.AddListener(() => aoClicar?.Invoke());

        ColorBlock cores = botao.colors;
        cores.highlightedColor = new Color(0.25f, 0.4f, 0.9f, 1f);
        cores.pressedColor = new Color(0.1f, 0.15f, 0.5f, 1f);
        botao.colors = cores;

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(btn.transform, false);

        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        Text textoBotao = textoObj.AddComponent<Text>();
        textoBotao.text = texto;
        textoBotao.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoBotao.fontSize = 16;
        textoBotao.fontStyle = FontStyle.Bold;
        textoBotao.color = Color.white;
        textoBotao.alignment = TextAnchor.MiddleCenter;

        AdicionarContorno(textoObj);
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

        List<string> ativos = new List<string>();

        if (gerenciador.TemVelocidadeExtra()) ativos.Add("⚡");
        if (gerenciador.TemPontuacaoDupla()) ativos.Add("x2");
        if (gerenciador.TemInvencibilidade()) ativos.Add("🛡️");

        if (ativos.Count > 0)
        {
            textoPowerUps.text = string.Join(" ", ativos);
            textoPowerUps.color = corDestaque;
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
}

public class GameHUD : InterfaceJogo { }
