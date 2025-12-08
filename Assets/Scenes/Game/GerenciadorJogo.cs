using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GerenciadorJogo - Script unificado que gerencia todo o jogo.
/// Substitui: GameController, GameManager e GameSetup
/// </summary>
public class GerenciadorJogo : MonoBehaviour
{
    #region Singleton
    public static GerenciadorJogo Instancia { get; private set; }
    #endregion

    #region Configurações do Personagem
    [Header("Configurações do Personagem")]
    [SerializeField] private int posicaoInicialX = 0;
    [SerializeField] private int posicaoInicialY = 0;
    #endregion

    #region Configurações de Pontuação
    [Header("Pontuação")]
    [SerializeField] private int pontuacaoTotal = 0;
    [SerializeField] private int moedasColetadas = 0;
    [SerializeField] private float tempoDeJogo = 0f;
    #endregion

    #region Sistema de Níveis
    [Header("Sistema de Níveis")]
    [SerializeField] private int nivelAtual = 1;
    [SerializeField] private int nivelMaximo = 5;
    [SerializeField] private float tempoTransicaoNivel = 3f;
    #endregion

    #region Configurações de Coletáveis
    [Header("Spawn de Coletáveis")]
    [SerializeField] private int moedasMinBase = 8;
    [SerializeField] private int moedasMaxBase = 15;
    [SerializeField] private int powerUpsBase = 2;
    #endregion

    #region Configurações de Obstáculos
    [Header("Spawn de Obstáculos")]
    [SerializeField] private int paredesBase = 3;
    #endregion

    #region Sistema de Vidas
    [Header("Sistema de Vidas")]
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private int vidasAtuais = 3;
    [SerializeField] private float tempoInvencibilidadeDano = 2f;
    [SerializeField] private bool invencivelPorDano = false;
    #endregion

    #region Configurações de Inimigos
    [Header("Inimigos")]
    [SerializeField] private int inimigosBase = 1;
    [SerializeField] private float velocidadeInimigoBase = 1.5f;
    #endregion

    #region Referências de UI
    [Header("Interface")]
    [SerializeField] private GameObject painelJogo;
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private Text textoPontuacaoVitoria;
    #endregion

    #region Áudio
    [Header("Áudio - Volume")]
    [SerializeField] [Range(0f, 1f)] private float volumeMaster = 0.15f;
    [SerializeField] [Range(0f, 1f)] private float volumeEfeitos = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float volumeMusica = 0.1f;
    
    [Header("Áudio - Efeitos Sonoros")]
    [SerializeField] private AudioSource fonteAudio;
    [SerializeField] private AudioSource fonteMusica;
    [SerializeField] private AudioClip somMoeda;
    [SerializeField] private AudioClip somPowerUp;
    [SerializeField] private AudioClip somVitoria;
    [SerializeField] private AudioClip somDano;
    [SerializeField] private AudioClip somMovimento;
    [SerializeField] private AudioClip somNivelCompleto;
    [SerializeField] private AudioClip somDerrota;
    [SerializeField] private AudioClip somPausa;
    #endregion

    #region Componentes Internos
    private Tabuleiro tabuleiro;
    private Personagem personagem;
    private List<Coletavel> todosColetaveis = new List<Coletavel>();
    private List<Obstacle> todosObstaculos = new List<Obstacle>();
    private List<Inimigo> todosInimigos = new List<Inimigo>();
    #endregion

    #region Estado do Jogo
    private bool estaPausado = false;
    private bool jogoVencido = false;
    private bool jogoPerdido = false;
    private bool jogoInicializado = false;
    private bool[] efeitosPowerUp = new bool[3];
    private float[] temporizadoresPowerUp = new float[3];
    #endregion

    #region Propriedades Públicas
    public int PontuacaoTotal => pontuacaoTotal;
    public int MoedasColetadas => moedasColetadas;
    public float TempoDeJogo => tempoDeJogo;
    public bool JogoVencido => jogoVencido;
    public bool JogoPerdido => jogoPerdido;
    public int NivelAtual => nivelAtual;
    public bool EstaPausado => estaPausado;
    public bool EstaInicializado => jogoInicializado;
    public int VidasAtuais => vidasAtuais;
    public int VidasMaximas => vidasMaximas;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        ConfigurarSingleton();
    }

    void Start()
    {
        IniciarJogo();
    }

    void Update()
    {
        if (!jogoInicializado) return;

        VerificarInputs();

        if (!jogoVencido && !jogoPerdido)
        {
            tempoDeJogo += Time.deltaTime;
            AtualizarPowerUps();
            AtualizarInvencibilidadeDano();
        }
    }
    #endregion

    #region Invencibilidade por Dano
    private void AtualizarInvencibilidadeDano()
    {
        // Gerenciado por coroutine
    }
    #endregion

    #region Inicialização
    private void ConfigurarSingleton()
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

    private void IniciarJogo()
    {
        Debug.Log("=== INICIANDO JOGO ===");

        Time.timeScale = 1f;
        estaPausado = false;
        jogoVencido = false;

        CriarEventSystem();
        CriarCanvas();
        CriarTabuleiro();
        CriarHUD();
        CriarEfeitosVisuais();

        ConfigurarUI();

        StartCoroutine(InicializacaoCompleta());
    }

    private void CriarEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Debug.Log("✓ EventSystem criado");
        }
    }

    private void CriarCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("Canvas");
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 1;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Canvas criado");
        }
    }

    private void CriarTabuleiro()
    {
        tabuleiro = FindObjectOfType<Tabuleiro>();
        if (tabuleiro == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject go = new GameObject("Tabuleiro");
                go.transform.SetParent(canvas.transform, false);

                RectTransform rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                tabuleiro = go.AddComponent<Tabuleiro>();
                Debug.Log("✓ Tabuleiro criado");
            }
        }
    }

    private void CriarHUD()
    {
        if (FindObjectOfType<InterfaceJogo>() == null)
        {
            GameObject go = new GameObject("InterfaceJogo");
            go.AddComponent<InterfaceJogo>();
            Debug.Log("✓ Interface do jogo criada");
        }
    }

    private void CriarEfeitosVisuais()
    {
        if (FindObjectOfType<VisualEffectsManager>() == null)
        {
            GameObject go = new GameObject("EfeitosVisuais");
            go.AddComponent<VisualEffectsManager>();
            Debug.Log("✓ Efeitos visuais criados");
        }
    }

    private void ConfigurarUI()
    {
        if (painelJogo != null) painelJogo.SetActive(true);
        if (painelPausa != null) painelPausa.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(false);
    }

    private IEnumerator InicializacaoCompleta()
    {
        // Aguardar tabuleiro
        while (tabuleiro == null)
        {
            tabuleiro = FindObjectOfType<Tabuleiro>();
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.2f);

        // Criar personagem
        CriarPersonagem();

        yield return new WaitForSeconds(0.3f);

        // Criar obstáculos e coletáveis
        CriarObstaculos();
        CriarColetaveis();
        CriarInimigos();

        jogoInicializado = true;

        Debug.Log("=== JOGO INICIADO ===");
        Debug.Log($"🎮 NÍVEL {nivelAtual} - Use as setas para mover!");

        MostrarMensagem($"🚀 NÍVEL {nivelAtual}\nColete todos os itens!", 3f);
    }
    #endregion

    #region Criação do Personagem
    private void CriarPersonagem()
    {
        if (tabuleiro == null) return;

        GameObject go = new GameObject("Personagem");
        go.transform.SetParent(tabuleiro.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        Image img = go.AddComponent<Image>();
        img.color = Color.red;

        personagem = go.AddComponent<Personagem>();
        personagem.DefinirPosicaoInicial(posicaoInicialX, posicaoInicialY);

        Debug.Log($"✓ Personagem criado em ({posicaoInicialX}, {posicaoInicialY})");
    }
    #endregion

    #region Criação de Obstáculos
    private void CriarObstaculos()
    {
        if (tabuleiro == null) return;

        List<Vector2Int> posicoes = ObterPosicoesDisponiveis();
        posicoes = ReservarCaminho(posicoes);

        int quantidade = ObterValorNivel(paredesBase);
        CriarObstaculosPorTipo(ObstacleType.Wall, quantidade, posicoes);

        Debug.Log($"✓ {quantidade} obstáculos criados");
    }

    private void CriarObstaculosPorTipo(ObstacleType tipo, int quantidade, List<Vector2Int> posicoes)
    {
        int criados = 0;
        int tentativas = quantidade * 3;

        for (int i = 0; i < tentativas && criados < quantidade && posicoes.Count > 0; i++)
        {
            int idx = Random.Range(0, posicoes.Count);
            Vector2Int pos = posicoes[idx];

            if (tipo == ObstacleType.Wall && BloqueariaCaminho(pos.x, pos.y))
                continue;

            posicoes.RemoveAt(idx);
            CriarObstaculo(tipo, pos.x, pos.y);
            criados++;
        }
    }

    private void CriarObstaculo(ObstacleType tipo, int x, int y)
    {
        GameObject go = new GameObject($"Obstaculo_{x}_{y}");
        go.transform.SetParent(tabuleiro.transform, false);

        Obstacle obstaculo = go.AddComponent<Obstacle>();
        obstaculo.Initialize(tipo, x, y);

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula != null)
        {
            RectTransform rt = obstaculo.GetComponent<RectTransform>();
            RectTransform celulaRt = celula.GetComponent<RectTransform>();
            rt.anchoredPosition = celulaRt.anchoredPosition;
        }

        todosObstaculos.Add(obstaculo);
    }

    private bool BloqueariaCaminho(int x, int y)
    {
        int paredesAdjacentes = 0;
        Vector2Int[] direcoes = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in direcoes)
        {
            int checkX = x + dir.x;
            int checkY = y + dir.y;

            if (checkX < 0 || checkX >= tabuleiro.Width || checkY < 0 || checkY >= tabuleiro.Height)
            {
                paredesAdjacentes++;
                continue;
            }

            foreach (var obs in todosObstaculos)
            {
                if (obs != null && obs.Type == ObstacleType.Wall && obs.TileX == checkX && obs.TileY == checkY)
                {
                    paredesAdjacentes++;
                    break;
                }
            }
        }

        return paredesAdjacentes >= 2;
    }
    #endregion

    #region Criação de Coletáveis
    private void CriarColetaveis()
    {
        if (tabuleiro == null) return;

        List<Vector2Int> posicoes = ObterPosicoesDisponiveis();

        int moedas = Random.Range(ObterValorNivel(moedasMinBase), ObterValorNivel(moedasMaxBase) + 1);
        CriarColetaveisPorTipo(TipoColetavel.Moeda, moedas, posicoes);

        int powerUps = ObterValorNivel(powerUpsBase);
        CriarColetaveisPorTipo(TipoColetavel.PowerUp, powerUps, posicoes);

        ValidarAcessibilidade();

        Debug.Log($"✓ Coletáveis: {moedas} moedas, {powerUps} power-ups");
    }

    private void CriarColetaveisPorTipo(TipoColetavel tipo, int quantidade, List<Vector2Int> posicoes)
    {
        for (int i = 0; i < quantidade && posicoes.Count > 0; i++)
        {
            int idx = Random.Range(0, posicoes.Count);
            Vector2Int pos = posicoes[idx];
            posicoes.RemoveAt(idx);

            CriarColetavel(tipo, pos.x, pos.y);
        }
    }

    private void CriarColetavel(TipoColetavel tipo, int x, int y)
    {
        GameObject go = new GameObject($"Coletavel_{tipo}_{x}_{y}");
        go.transform.SetParent(tabuleiro.transform, false);

        Coletavel coletavel = go.AddComponent<Coletavel>();
        coletavel.Inicializar(tipo, x, y);

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula != null)
        {
            RectTransform rt = coletavel.GetComponent<RectTransform>();
            RectTransform celulaRt = celula.GetComponent<RectTransform>();
            rt.anchoredPosition = celulaRt.anchoredPosition;
        }

        todosColetaveis.Add(coletavel);
    }
    #endregion

    #region Criação de Inimigos
    private void CriarInimigos()
    {
        if (tabuleiro == null) return;

        int quantidade = inimigosBase + (nivelAtual - 1); // Mais inimigos por nível
        quantidade = Mathf.Min(quantidade, 5); // Máximo 5 inimigos

        float velocidade = velocidadeInimigoBase - (nivelAtual - 1) * 0.1f; // Mais rápidos por nível
        velocidade = Mathf.Max(velocidade, 0.6f); // Velocidade mínima

        List<Vector2Int> posicoesValidas = ObterPosicoesParaInimigos();

        for (int i = 0; i < quantidade && posicoesValidas.Count > 0; i++)
        {
            int idx = Random.Range(0, posicoesValidas.Count);
            Vector2Int pos = posicoesValidas[idx];
            posicoesValidas.RemoveAt(idx);

            CriarInimigo(pos.x, pos.y, velocidade);
        }

        Debug.Log($"✓ {quantidade} inimigos criados (velocidade: {velocidade:F1}s)");
    }

    private List<Vector2Int> ObterPosicoesParaInimigos()
    {
        List<Vector2Int> posicoes = new List<Vector2Int>();
        int metadeX = tabuleiro.Width / 2;
        int metadeY = tabuleiro.Height / 2;

        // Inimigos começam longe do jogador
        for (int x = metadeX; x < tabuleiro.Width; x++)
        {
            for (int y = metadeY; y < tabuleiro.Height; y++)
            {
                if (!PosicaoOcupada(x, y) && !TemObstaculoEm(x, y, ObstacleType.Wall))
                {
                    posicoes.Add(new Vector2Int(x, y));
                }
            }
        }

        // Se não houver posições no canto, tentar outras áreas
        if (posicoes.Count < 3)
        {
            for (int x = 0; x < tabuleiro.Width; x++)
            {
                for (int y = 0; y < tabuleiro.Height; y++)
                {
                    // Distância mínima do jogador
                    if (x + y > 4 && !PosicaoOcupada(x, y) && !TemObstaculoEm(x, y, ObstacleType.Wall))
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (!posicoes.Contains(pos))
                            posicoes.Add(pos);
                    }
                }
            }
        }

        return posicoes;
    }

    private void CriarInimigo(int x, int y, float velocidade)
    {
        GameObject go = new GameObject($"Inimigo_{x}_{y}");
        go.transform.SetParent(tabuleiro.transform, false);

        Inimigo inimigo = go.AddComponent<Inimigo>();
        inimigo.Inicializar(x, y, velocidade);

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula != null)
        {
            RectTransform rt = inimigo.GetComponent<RectTransform>();
            RectTransform celulaRt = celula.GetComponent<RectTransform>();
            rt.anchoredPosition = celulaRt.anchoredPosition;
        }

        todosInimigos.Add(inimigo);
    }
    #endregion

    #region Utilitários de Posição
    private List<Vector2Int> ObterPosicoesDisponiveis()
    {
        List<Vector2Int> posicoes = new List<Vector2Int>();

        for (int x = 0; x < tabuleiro.Width; x++)
        {
            for (int y = 0; y < tabuleiro.Height; y++)
            {
                if (x == 0 && y == 0) continue;
                if (!PosicaoOcupada(x, y))
                    posicoes.Add(new Vector2Int(x, y));
            }
        }

        return posicoes;
    }

    private bool PosicaoOcupada(int x, int y)
    {
        foreach (var c in todosColetaveis)
            if (c != null && c.TileX == x && c.TileY == y) return true;

        foreach (var o in todosObstaculos)
            if (o != null && o.TileX == x && o.TileY == y) return true;

        return false;
    }

    private List<Vector2Int> ReservarCaminho(List<Vector2Int> posicoes)
    {
        List<Vector2Int> reservadas = new List<Vector2Int>();

        // Área inicial
        for (int x = 0; x < 3 && x < tabuleiro.Width; x++)
            for (int y = 0; y < 3 && y < tabuleiro.Height; y++)
                if (!(x == 0 && y == 0))
                    reservadas.Add(new Vector2Int(x, y));

        // Corredores
        for (int x = 0; x < tabuleiro.Width; x += 2)
            reservadas.Add(new Vector2Int(x, tabuleiro.Height - 1));

        for (int y = 0; y < tabuleiro.Height; y += 2)
            reservadas.Add(new Vector2Int(tabuleiro.Width - 1, y));

        foreach (var r in reservadas)
            posicoes.Remove(r);

        return posicoes;
    }

    private void ValidarAcessibilidade()
    {
        List<Coletavel> inacessiveis = new List<Coletavel>();

        foreach (var c in todosColetaveis)
            if (c != null && !PosicaoAcessivel(c.PosX, c.PosY))
                inacessiveis.Add(c);

        foreach (var item in inacessiveis)
        {
            Vector2Int nova = EncontrarPosicaoAcessivel();
            if (nova.x >= 0)
            {
                Celula celula = tabuleiro.ObterCelula(nova.x, nova.y);
                if (celula != null)
                {
                    RectTransform rt = item.GetComponent<RectTransform>();
                    RectTransform celulaRt = celula.GetComponent<RectTransform>();
                    rt.anchoredPosition = celulaRt.anchoredPosition;
                    item.Inicializar(item.Tipo, nova.x, nova.y);
                }
            }
        }
    }

    private bool PosicaoAcessivel(int alvoX, int alvoY)
    {
        bool[,] visitado = new bool[tabuleiro.Width, tabuleiro.Height];
        Queue<Vector2Int> fila = new Queue<Vector2Int>();

        fila.Enqueue(Vector2Int.zero);
        visitado[0, 0] = true;

        Vector2Int[] direcoes = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (fila.Count > 0)
        {
            Vector2Int atual = fila.Dequeue();

            if (atual.x == alvoX && atual.y == alvoY)
                return true;

            foreach (var dir in direcoes)
            {
                int nx = atual.x + dir.x;
                int ny = atual.y + dir.y;

                if (nx >= 0 && nx < tabuleiro.Width && ny >= 0 && ny < tabuleiro.Height && !visitado[nx, ny])
                {
                    if (!TemObstaculoEm(nx, ny, ObstacleType.Wall))
                    {
                        visitado[nx, ny] = true;
                        fila.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return false;
    }

    private bool TemObstaculoEm(int x, int y, ObstacleType tipo)
    {
        foreach (var o in todosObstaculos)
            if (o != null && o.TileX == x && o.TileY == y && o.Type == tipo)
                return true;
        return false;
    }

    private Vector2Int EncontrarPosicaoAcessivel()
    {
        for (int x = 0; x < tabuleiro.Width; x++)
            for (int y = 0; y < tabuleiro.Height; y++)
                if (!PosicaoOcupada(x, y) && PosicaoAcessivel(x, y))
                    return new Vector2Int(x, y);

        return new Vector2Int(-1, -1);
    }

    private int ObterValorNivel(int valorBase)
    {
        float multiplicador = 1f + (nivelAtual - 1) * 0.3f;
        return Mathf.RoundToInt(valorBase * multiplicador);
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
        if (painelPausa == null)
        {
            InterfaceJogo ui = InterfaceJogo.Instancia;
            if (ui != null && ui.PainelPausa != null)
            {
                painelPausa = ui.PainelPausa;
            }
        }
        
        estaPausado = !estaPausado;

        if (estaPausado)
        {
            TocarSomPausa();
            Time.timeScale = 0f;
            if (painelPausa != null) painelPausa.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (painelPausa != null) painelPausa.SetActive(false);
        }
    }

    public void ContinuarJogo()
    {
        estaPausado = false;
        Time.timeScale = 1f;
        
        if (painelPausa == null)
        {
            InterfaceJogo ui = InterfaceJogo.Instancia;
            if (ui != null && ui.PainelPausa != null)
            {
                painelPausa = ui.PainelPausa;
            }
        }
        
        if (painelPausa != null) painelPausa.SetActive(false);
    }

    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SairDoJogo()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Sistema de Coleta
    public void AoColetarItem(Coletavel coletavel)
    {
        Debug.Log($"🎯 {coletavel.Tipo} coletado! Valor: {coletavel.Valor}");

        InterfaceJogo ui = InterfaceJogo.Instancia;
        if (ui != null)
            ui.MostrarFeedbackColeta(coletavel.Tipo, coletavel.Valor);

        switch (coletavel.Tipo)
        {
            case TipoColetavel.Moeda:
                moedasColetadas++;
                AdicionarPontos(coletavel.Valor);
                TocarSomMoeda();
                break;

            case TipoColetavel.PowerUp:
                AtivarPowerUp(coletavel.TipoPower);
                TocarSomPowerUp();
                break;
        }

        todosColetaveis.Remove(coletavel);
        VerificarVitoria();
    }

    private void AdicionarPontos(int pontos)
    {
        int multiplicador = efeitosPowerUp[1] ? 2 : 1;
        pontuacaoTotal += pontos * multiplicador;
    }

    private void AtivarPowerUp(TipoPowerUp tipo)
    {
        int indice = (int)tipo;
        efeitosPowerUp[indice] = true;
        temporizadoresPowerUp[indice] = 10f;

        string mensagem = "";
        switch (tipo)
        {
            case TipoPowerUp.Velocidade:
                mensagem = "⚡ VELOCIDADE!\nMovimento 2x mais rápido por 10s";
                break;
            case TipoPowerUp.PontuacaoDupla:
                mensagem = "✨ PONTUAÇÃO DUPLA!\nMoedas valem 2x por 10s";
                break;
            case TipoPowerUp.Invencibilidade:
                mensagem = "🛡️ INVENCIBILIDADE!\nImune a inimigos por 10s";
                break;
        }
        
        MostrarMensagem(mensagem, 2.5f);
        Debug.Log($"⚡ Power-up: {tipo}!");
    }

    private void AtualizarPowerUps()
    {
        for (int i = 0; i < efeitosPowerUp.Length; i++)
        {
            if (efeitosPowerUp[i])
            {
                temporizadoresPowerUp[i] -= Time.deltaTime;
                if (temporizadoresPowerUp[i] <= 0f)
                    efeitosPowerUp[i] = false;
            }
        }
    }
    #endregion

    #region Condição de Vitória
    private void VerificarVitoria()
    {
        int moedasRestantes = 0;

        foreach (var c in todosColetaveis)
        {
            if (c.Tipo == TipoColetavel.Moeda)
            {
                moedasRestantes++;
            }
        }

        if (moedasRestantes == 0 && !jogoVencido)
        {
            if (nivelAtual >= nivelMaximo)
                CompletarJogo();
            else
                AvancarNivel();
        }
    }

    private void AvancarNivel()
    {
        jogoVencido = true;

        int bonus = CalcularBonus();
        pontuacaoTotal += bonus;

        TocarSomNivelCompleto();
        MostrarMensagem($"🎉 NÍVEL {nivelAtual} CONCLUÍDO!\n+{bonus} pontos\nPróximo: Nível {nivelAtual + 1}", tempoTransicaoNivel);

        StartCoroutine(TransicaoDeNivel());
    }

    private IEnumerator TransicaoDeNivel()
    {
        yield return new WaitForSeconds(tempoTransicaoNivel);

        nivelAtual++;
        LimparNivel();

        jogoVencido = false;
        moedasColetadas = 0;

        CriarObstaculos();
        CriarColetaveis();
        CriarInimigos();

        MostrarMensagem($"🚀 NÍVEL {nivelAtual}!\nColete todos os itens!", 2f);
    }

    private void LimparNivel()
    {
        foreach (var c in todosColetaveis)
            if (c != null) Destroy(c.gameObject);
        todosColetaveis.Clear();

        foreach (var o in todosObstaculos)
            if (o != null) Destroy(o.gameObject);
        todosObstaculos.Clear();

        foreach (var i in todosInimigos)
            if (i != null) Destroy(i.gameObject);
        todosInimigos.Clear();
    }

    private void CompletarJogo()
    {
        jogoVencido = true;

        int bonusFinal = 1000 + (nivelAtual * 200);
        pontuacaoTotal += bonusFinal;

        TocarSomVitoria();

        InterfaceJogo ui = InterfaceJogo.Instancia;
        if (ui != null)
        {
            ui.MostrarPainelConclusao();
        }
        else if (painelVitoria != null)
        {
            painelVitoria.SetActive(true);
            if (textoPontuacaoVitoria != null)
            {
                textoPontuacaoVitoria.text = $"🏆 JOGO COMPLETO! 🏆\n\n" +
                    $"Pontuação: {pontuacaoTotal}\n" +
                    $"Tempo: {FormatarTempo(tempoDeJogo)}\n" +
                    $"Níveis: {nivelAtual}/{nivelMaximo}";
            }
        }

        Debug.Log($"🏆 JOGO COMPLETO! Pontuação: {pontuacaoTotal}");
    }

    private int CalcularBonus()
    {
        int bonusTempo = Mathf.Max(0, 180 - Mathf.RoundToInt(tempoDeJogo));
        int bonusNivel = nivelAtual * 100;
        return bonusTempo + bonusNivel;
    }

    private string FormatarTempo(float tempo)
    {
        int min = Mathf.FloorToInt(tempo / 60f);
        int seg = Mathf.FloorToInt(tempo % 60f);
        return $"{min:00}:{seg:00}";
    }
    #endregion

    #region Verificação de Movimento
    public Obstacle ObterObstaculoEm(int x, int y)
    {
        foreach (var o in todosObstaculos)
            if (o.GridX == x && o.GridY == y && o.IsActive)
                return o;
        return null;
    }

    public bool PodeMoverPara(int x, int y)
    {
        if (x < 0 || x >= tabuleiro.Width || y < 0 || y >= tabuleiro.Height)
            return false;

        Obstacle obs = ObterObstaculoEm(x, y);
        if (obs != null && obs.BlocksMovement())
            return false;

        return true;
    }
    #endregion

    #region Utilitários
    private void TocarSom(AudioClip clip, float volumeExtra = 1f)
    {
        if (fonteAudio != null && clip != null)
        {
            float volumeFinal = volumeMaster * volumeEfeitos * volumeExtra;
            fonteAudio.PlayOneShot(clip, volumeFinal);
        }
    }
    
    private void TocarSomGerado(TipoSom tipo)
    {
        if (fonteAudio == null) return;
        
        AudioClip clip = GerarSom(tipo);
        if (clip != null)
        {
            float volumeFinal = volumeMaster * volumeEfeitos;
            fonteAudio.PlayOneShot(clip, volumeFinal);
        }
    }
    
    private enum TipoSom { Moeda, PowerUp, Dano, Vitoria, Derrota, Movimento, NivelCompleto, Pausa }
    
    private AudioClip GerarSom(TipoSom tipo)
    {
        int sampleRate = 44100;
        int samples;
        float[] data;
        
        switch (tipo)
        {
            case TipoSom.Moeda:
                samples = sampleRate / 6; // ~0.16s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - (float)i / samples;
                    float freq1 = 880f + (i < samples/2 ? 0 : 220f); // A5 -> B5
                    data[i] = Mathf.Sin(2f * Mathf.PI * freq1 * t) * envelope * 0.4f;
                }
                break;
                
            case TipoSom.PowerUp:
                samples = sampleRate / 3; // ~0.33s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float progress = (float)i / samples;
                    float envelope = (1f - progress) * Mathf.Sin(progress * Mathf.PI);
                    float freq = 440f + progress * 880f; // Sweep de A4 a A6
                    data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + 
                              Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.3f) * envelope * 0.35f;
                }
                break;
                
            case TipoSom.Dano:
                samples = sampleRate / 4; // ~0.25s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - (float)i / samples;
                    float freq = 150f - ((float)i / samples) * 50f;
                    float noise = (Random.value - 0.5f) * 0.3f;
                    data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + noise) * envelope * 0.5f;
                }
                break;
                
            case TipoSom.Vitoria:
                samples = sampleRate; // 1s
                data = new float[samples];
                float[] notasVitoria = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    int notaIdx = Mathf.Min((int)(t * 4f), 3);
                    float envelope = Mathf.Max(0, 1f - (t * 4f - notaIdx) * 0.5f);
                    float freq = notasVitoria[notaIdx];
                    data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
                }
                break;
                
            case TipoSom.Derrota:
                samples = sampleRate; // 1s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Max(0, 1f - t);
                    float freq = 440f - t * 200f; // Descendo
                    data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
                }
                break;
                
            case TipoSom.Movimento:
                samples = sampleRate / 20; // ~0.05s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float envelope = 1f - (float)i / samples;
                    data[i] = (Random.value - 0.5f) * envelope * 0.15f;
                }
                break;
                
            case TipoSom.NivelCompleto:
                samples = sampleRate / 2; // 0.5s
                data = new float[samples];
                float[] notasNivel = { 392f, 494f, 587f, 784f }; // G4, B4, D5, G5
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    int notaIdx = Mathf.Min((int)(t * 8f), 3);
                    float envelope = Mathf.Max(0, 1f - (t * 8f - notaIdx) * 0.7f);
                    float freq = notasNivel[notaIdx];
                    data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.35f;
                }
                break;
                
            case TipoSom.Pausa:
                samples = sampleRate / 10; // 0.1s
                data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Sin((float)i / samples * Mathf.PI);
                    data[i] = Mathf.Sin(2f * Mathf.PI * 600f * t) * envelope * 0.25f;
                }
                break;
                
            default:
                return null;
        }
        
        AudioClip clip = AudioClip.Create($"Som_{tipo}", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
    
    public void TocarSomMoeda() => TocarSomGerado(TipoSom.Moeda);
    public void TocarSomPowerUp() => TocarSomGerado(TipoSom.PowerUp);
    public void TocarSomDano() => TocarSomGerado(TipoSom.Dano);
    public void TocarSomVitoria() => TocarSomGerado(TipoSom.Vitoria);
    public void TocarSomDerrota() => TocarSomGerado(TipoSom.Derrota);
    public void TocarSomMovimento() => TocarSomGerado(TipoSom.Movimento);
    public void TocarSomNivelCompleto() => TocarSomGerado(TipoSom.NivelCompleto);
    public void TocarSomPausa() => TocarSomGerado(TipoSom.Pausa);

    private void MostrarMensagem(string mensagem, float duracao)
    {
        InterfaceJogo ui = InterfaceJogo.Instancia;
        if (ui != null)
            ui.MostrarMensagem(mensagem, duracao);
    }

    public bool TemVelocidadeExtra() => efeitosPowerUp[0];
    public bool TemPontuacaoDupla() => efeitosPowerUp[1];
    public bool TemInvencibilidade() => efeitosPowerUp[2] || invencivelPorDano;

    public float TempoRestanteVelocidadeExtra()
    => Mathf.Max(0f, temporizadoresPowerUp[(int)TipoPowerUp.Velocidade]);

    public float TempoRestantePontuacaoDupla()
        => Mathf.Max(0f, temporizadoresPowerUp[(int)TipoPowerUp.PontuacaoDupla]);

    public float TempoRestanteInvencibilidade()
        => Mathf.Max(0f, temporizadoresPowerUp[(int)TipoPowerUp.Invencibilidade]);

    public Personagem ObterPersonagem() => personagem;
    public Tabuleiro ObterTabuleiro() => tabuleiro;
    #endregion

    #region Sistema de Dano e Vidas
    public void JogadorReceberDano(int dano)
    {
        // Verificar se está invencível
        if (TemInvencibilidade()) return;
        if (jogoPerdido || jogoVencido) return;

        vidasAtuais -= dano;
        vidasAtuais = Mathf.Max(0, vidasAtuais);

        TocarSomDano();
        Debug.Log($"💥 Dano recebido! Vidas: {vidasAtuais}/{vidasMaximas}");

        // Feedback visual
        if (personagem != null)
        {
            StartCoroutine(AnimacaoDano());
        }

        // Ativar invencibilidade temporária
        StartCoroutine(InvencibilidadeTemporaria());

        // Verificar derrota
        if (vidasAtuais <= 0)
        {
            PerderJogo();
        }
        else
        {
            MostrarMensagem($"💥 -{dano} vida! ({vidasAtuais}/{vidasMaximas})", 1.5f);
        }

        // Atualizar HUD
        AtualizarHUDVidas();
    }

    private IEnumerator InvencibilidadeTemporaria()
    {
        invencivelPorDano = true;
        
        // Piscar personagem
        if (personagem != null)
        {
            Image img = personagem.GetComponent<Image>();
            if (img != null)
            {
                float tempo = 0f;
                while (tempo < tempoInvencibilidadeDano)
                {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 
                        Mathf.PingPong(tempo * 8f, 1f) > 0.5f ? 0.3f : 1f);
                    tempo += Time.deltaTime;
                    yield return null;
                }
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            }
        }

        invencivelPorDano = false;
    }

    private IEnumerator AnimacaoDano()
    {
        Image img = personagem.GetComponent<Image>();
        if (img == null) yield break;

        Color corOriginal = img.color;
        img.color = Color.red;

        RectTransform rt = personagem.GetComponent<RectTransform>();
        Vector2 posOriginal = rt.anchoredPosition;

        // Shake
        float tempo = 0f;
        float duracao = 0.3f;
        while (tempo < duracao)
        {
            float offset = Mathf.Sin(tempo * 50f) * 5f * (1f - tempo / duracao);
            rt.anchoredPosition = posOriginal + new Vector2(offset, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = posOriginal;
        img.color = corOriginal;
    }

    private void AtualizarHUDVidas()
    {
        InterfaceJogo ui = InterfaceJogo.Instancia;
        if (ui != null)
        {
            ui.AtualizarVidas(vidasAtuais, vidasMaximas);
        }
    }

    public void RecuperarVida(int quantidade = 1)
    {
        vidasAtuais = Mathf.Min(vidasAtuais + quantidade, vidasMaximas);
        AtualizarHUDVidas();
        MostrarMensagem($"❤️ +{quantidade} vida!", 1f);
    }
    #endregion

    #region Condição de Derrota
    private void PerderJogo()
    {
        jogoPerdido = true;
        jogoVencido = false;

        TocarSomDerrota();
        Debug.Log("💩 GAME OVER!");

        // Parar inimigos
        foreach (var inimigo in todosInimigos)
        {
            if (inimigo != null)
                inimigo.PararPerseguicao();
        }

        // Mostrar tela de derrota
        InterfaceJogo ui = InterfaceJogo.Instancia;
        if (ui != null)
        {
            ui.MostrarPainelDerrota(pontuacaoTotal, tempoDeJogo, nivelAtual, moedasColetadas);
        }
    }

    public void VerificarColisaoInimigos(int jogadorX, int jogadorY)
    {
        foreach (var inimigo in todosInimigos)
        {
            if (inimigo != null && inimigo.Ativo)
            {
                inimigo.VerificarColisao(jogadorX, jogadorY);
            }
        }
    }
    #endregion
}
