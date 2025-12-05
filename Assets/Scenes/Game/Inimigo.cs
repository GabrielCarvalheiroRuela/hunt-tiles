using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Inimigo - Persegue o jogador no tabuleiro.
/// </summary>
public class Inimigo : MonoBehaviour
{
    #region Configurações
    [Header("Configurações")]
    [SerializeField] private int posicaoX = 0;
    [SerializeField] private int posicaoY = 0;
    [SerializeField] private float velocidadeMovimento = 0.35f;
    [SerializeField] private float intervaloMovimento = 0.8f;
    [SerializeField] private int dano = 1;
    [SerializeField] private float chanceMovimentoDuplo = 0.25f; // 25% chance de mover 2x
    [SerializeField] private bool usarPathfinding = true;
    #endregion

    #region Visual
    [Header("Visual")]
    [SerializeField] private Color corInimigo = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color corOlhos = Color.yellow;
    [SerializeField] private float tamanho = 35f;
    #endregion

    #region Componentes
    private RectTransform retanguloTransform;
    private Image imagemInimigo;
    private Tabuleiro tabuleiro;
    private Personagem alvo;
    #endregion

    #region Estado
    private bool estaMovendo = false;
    private bool ativo = true;
    private Coroutine rotinaPerseguicao;
    private float cooldownDano = 0f;
    private const float TEMPO_COOLDOWN_DANO = 1.5f;
    #endregion

    #region Propriedades
    public int PosicaoX => posicaoX;
    public int PosicaoY => posicaoY;
    public int Dano => dano;
    public bool Ativo => ativo;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        ConfigurarVisual();
    }

    void Start()
    {
        StartCoroutine(InicializarComDelay());
    }

    private IEnumerator InicializarComDelay()
    {
        yield return new WaitForSeconds(0.5f);

        tabuleiro = Tabuleiro.Instancia;
        alvo = Personagem.Instancia;

        if (tabuleiro == null)
            tabuleiro = FindObjectOfType<Tabuleiro>();
        if (alvo == null)
            alvo = FindObjectOfType<Personagem>();

        if (tabuleiro != null)
        {
            PosicionarEm(posicaoX, posicaoY);
        }

        Debug.Log($"Inimigo inicializado: Tabuleiro={tabuleiro != null}, Alvo={alvo != null}");

        IniciarPerseguicao();
    }

    void Update()
    {
        if (alvo == null)
        {
            alvo = Personagem.Instancia;
            if (alvo == null)
                alvo = FindObjectOfType<Personagem>();
        }

        if (tabuleiro == null)
        {
            tabuleiro = Tabuleiro.Instancia;
            if (tabuleiro == null)
                tabuleiro = FindObjectOfType<Tabuleiro>();
        }
        
        if (cooldownDano > 0f)
        {
            cooldownDano -= Time.deltaTime;
        }
        
        if (ativo && alvo != null)
        {
            if (posicaoX == alvo.PosicaoX && posicaoY == alvo.PosicaoY)
            {
                CausarDano();
            }
        }
    }

    void OnDestroy()
    {
        if (rotinaPerseguicao != null)
        {
            StopCoroutine(rotinaPerseguicao);
        }
    }
    #endregion

    #region Inicialização
    public void Inicializar(int x, int y, float velocidade = 1.2f)
    {
        posicaoX = x;
        posicaoY = y;
        intervaloMovimento = velocidade;
        gameObject.name = $"Inimigo_{x}_{y}";
    }

    private void ConfigurarVisual()
    {
        retanguloTransform = GetComponent<RectTransform>();
        if (retanguloTransform == null)
            retanguloTransform = gameObject.AddComponent<RectTransform>();

        retanguloTransform.anchorMin = Vector2.one * 0.5f;
        retanguloTransform.anchorMax = Vector2.one * 0.5f;
        retanguloTransform.sizeDelta = Vector2.one * tamanho;

        imagemInimigo = GetComponent<Image>();
        if (imagemInimigo == null)
            imagemInimigo = gameObject.AddComponent<Image>();

        imagemInimigo.color = Color.white;
        imagemInimigo.sprite = CriarSpriteInimigo();

        // Contorno vermelho brilhante (aura ameaçadora)
        Outline contorno = gameObject.AddComponent<Outline>();
        contorno.effectColor = new Color(1f, 0.3f, 0.1f, 0.8f);
        contorno.effectDistance = new Vector2(2f, 2f);

        // Sombra profunda
        Shadow sombra = gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.3f, 0f, 0f, 0.7f);
        sombra.effectDistance = new Vector2(4f, -4f);

        // Criar aura de perigo
        CriarAuraPerigo();

        // Criar olhos brilhantes
        CriarOlhos();

        // Animação de pulsação ameaçadora
        StartCoroutine(AnimarPulsacao());
    }

    private void CriarAuraPerigo()
    {
        GameObject auraObj = new GameObject("AuraPerigo");
        auraObj.transform.SetParent(transform, false);
        auraObj.transform.SetAsFirstSibling();

        RectTransform auraRect = auraObj.AddComponent<RectTransform>();
        auraRect.anchorMin = Vector2.zero;
        auraRect.anchorMax = Vector2.one;
        auraRect.sizeDelta = new Vector2(12f, 12f);
        auraRect.anchoredPosition = Vector2.zero;

        Image auraImg = auraObj.AddComponent<Image>();
        auraImg.color = new Color(1f, 0.2f, 0f, 0.25f);
        auraImg.sprite = CriarSpriteCircular(32, new Color(1f, 0.2f, 0f, 0.4f));

        // Animação da aura
        StartCoroutine(AnimarAura(auraObj));
    }

    private IEnumerator AnimarAura(GameObject aura)
    {
        if (aura == null) yield break;
        
        RectTransform rt = aura.GetComponent<RectTransform>();
        Image img = aura.GetComponent<Image>();
        
        while (aura != null && ativo)
        {
            float t = Time.time * 2f;
            float escala = 1f + Mathf.Sin(t) * 0.15f;
            float alpha = 0.2f + Mathf.Sin(t * 1.5f) * 0.1f;
            
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(12f, 12f) * escala;
            }
            if (img != null)
            {
                img.color = new Color(1f, 0.2f, 0f, alpha);
            }
            
            yield return null;
        }
    }

    private Sprite CriarSpriteCircular(int tamanhoTex, Color cor)
    {
        Texture2D tex = new Texture2D(tamanhoTex, tamanhoTex, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(tamanhoTex / 2f, tamanhoTex / 2f);
        float raio = tamanhoTex / 2f;

        for (int x = 0; x < tamanhoTex; x++)
        {
            for (int y = 0; y < tamanhoTex; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centro);
                if (dist <= raio)
                {
                    float alpha = cor.a * (1f - (dist / raio) * 0.5f);
                    tex.SetPixel(x, y, new Color(cor.r, cor.g, cor.b, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tamanhoTex, tamanhoTex), Vector2.one * 0.5f);
    }

    private Sprite CriarSpriteInimigo()
    {
        int texSize = 64;
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(texSize / 2f, texSize / 2f);
        float raio = texSize / 2f - 4f;

        // Cores do inimigo (gradiente vermelho/preto)
        Color vermelhoProfundo = new Color(0.6f, 0.05f, 0.05f, 1f);
        Color vermelhoMedio = new Color(0.85f, 0.15f, 0.1f, 1f);
        Color vermelhoBrilho = new Color(1f, 0.35f, 0.2f, 1f);

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centro);
                
                float angulo = Mathf.Atan2(y - centro.y, x - centro.x);
                float ondulacao = Mathf.Sin(angulo * 8f) * 3f + Mathf.Sin(angulo * 4f) * 2f;
                float raioModificado = raio + ondulacao;
                
                if (dist <= raioModificado)
                {
                    float t = dist / raioModificado;
                    Color cor;

                    if (t < 0.3f)
                    {
                        cor = Color.Lerp(vermelhoBrilho, vermelhoMedio, t / 0.3f);
                    }
                    else if (t < 0.7f)
                    {
                        cor = Color.Lerp(vermelhoMedio, vermelhoProfundo, (t - 0.3f) / 0.4f);
                    }
                    else
                    {
                        cor = Color.Lerp(vermelhoProfundo, new Color(0.3f, 0f, 0f, 1f), (t - 0.7f) / 0.3f);
                    }

                    Vector2 brilhoPos = new Vector2(x, y) - (centro + new Vector2(-8, 8));
                    if (brilhoPos.magnitude < 10f && t < 0.6f)
                    {
                        float intensidade = 1f - (brilhoPos.magnitude / 10f);
                        cor = Color.Lerp(cor, new Color(1f, 0.6f, 0.4f, 1f), intensidade * 0.4f);
                    }

                    if (dist > raioModificado - 1.5f)
                    {
                        cor.a = 1f - (dist - (raioModificado - 1.5f)) / 1.5f;
                    }

                    tex.SetPixel(x, y, cor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.one * 0.5f);
    }

    private void CriarOlhos()
    {
        GameObject olhoE = new GameObject("OlhoE");
        olhoE.transform.SetParent(transform, false);
        
        RectTransform rtE = olhoE.AddComponent<RectTransform>();
        rtE.anchorMin = new Vector2(0.22f, 0.52f);
        rtE.anchorMax = new Vector2(0.42f, 0.78f);
        rtE.offsetMin = Vector2.zero;
        rtE.offsetMax = Vector2.zero;
        
        Image imgE = olhoE.AddComponent<Image>();
        imgE.color = new Color(1f, 0.95f, 0.3f, 1f);
        imgE.sprite = CriarSpriteOlho();

        Outline brilhoE = olhoE.AddComponent<Outline>();
        brilhoE.effectColor = new Color(1f, 0.8f, 0f, 0.7f);
        brilhoE.effectDistance = new Vector2(1f, 1f);

        GameObject olhoD = new GameObject("OlhoD");
        olhoD.transform.SetParent(transform, false);
        
        RectTransform rtD = olhoD.AddComponent<RectTransform>();
        rtD.anchorMin = new Vector2(0.58f, 0.52f);
        rtD.anchorMax = new Vector2(0.78f, 0.78f);
        rtD.offsetMin = Vector2.zero;
        rtD.offsetMax = Vector2.zero;
        
        Image imgD = olhoD.AddComponent<Image>();
        imgD.color = new Color(1f, 0.95f, 0.3f, 1f);
        imgD.sprite = CriarSpriteOlho();

        Outline brilhoD = olhoD.AddComponent<Outline>();
        brilhoD.effectColor = new Color(1f, 0.8f, 0f, 0.7f);
        brilhoD.effectDistance = new Vector2(1f, 1f);

        StartCoroutine(AnimarOlhos(imgE, imgD));
    }

    private Sprite CriarSpriteOlho()
    {
        int tam = 16;
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(tam / 2f, tam / 2f);
        float raio = tam / 2f - 1f;

        for (int x = 0; x < tam; x++)
        {
            for (int y = 0; y < tam; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centro);
                if (dist <= raio)
                {
                    float t = dist / raio;
                    Color cor;
                    if (t < 0.3f)
                    {
                        cor = Color.white;
                    }
                    else if (t < 0.6f)
                    {
                        cor = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.2f, 1f), (t - 0.3f) / 0.3f);
                    }
                    else
                    {
                        cor = Color.Lerp(new Color(1f, 0.9f, 0.2f, 1f), new Color(0.9f, 0.5f, 0f, 1f), (t - 0.6f) / 0.4f);
                    }

                    if (dist < 2f)
                    {
                        cor = Color.black;
                    }

                    if (dist > raio - 1f)
                    {
                        cor.a = 1f - (dist - (raio - 1f));
                    }

                    tex.SetPixel(x, y, cor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), Vector2.one * 0.5f);
    }

    private IEnumerator AnimarOlhos(Image olhoE, Image olhoD)
    {
        while (ativo && olhoE != null && olhoD != null)
        {
            float t = Time.time * 3f;
            float intensidade = 0.85f + Mathf.Sin(t) * 0.15f;
            Color cor = new Color(intensidade, intensidade * 0.9f, 0.3f * intensidade, 1f);
            
            olhoE.color = cor;
            olhoD.color = cor;
            
            yield return null;
        }
    }
    #endregion

    #region Movimento e Perseguição
    public void IniciarPerseguicao()
    {
        if (rotinaPerseguicao != null)
            StopCoroutine(rotinaPerseguicao);

        rotinaPerseguicao = StartCoroutine(RotinaPerseguicao());
    }

    public void PararPerseguicao()
    {
        ativo = false;
        if (rotinaPerseguicao != null)
        {
            StopCoroutine(rotinaPerseguicao);
            rotinaPerseguicao = null;
        }
    }

    private IEnumerator RotinaPerseguicao()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"Inimigo {gameObject.name} iniciando perseguição agressiva");

        while (ativo)
        {
            if (alvo != null && posicaoX == alvo.PosicaoX && posicaoY == alvo.PosicaoY)
            {
                CausarDano();
            }
            
            float intervaloAtual = intervaloMovimento;
            if (alvo != null)
            {
                float distancia = Vector2.Distance(
                    new Vector2(posicaoX, posicaoY),
                    new Vector2(alvo.PosicaoX, alvo.PosicaoY)
                );

                if (distancia <= 4f)
                {
                    intervaloAtual *= 0.4f + (distancia / 10f);
                }
            }
            
            yield return new WaitForSeconds(intervaloAtual);

            GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
            if (gerenciador == null || gerenciador.JogoVencido || gerenciador.JogoPerdido)
            {
                continue;
            }

            if (alvo == null)
            {
                alvo = Personagem.Instancia ?? FindObjectOfType<Personagem>();
                if (alvo == null) continue;
            }

            if (!estaMovendo)
            {
                if (gerenciador.TemInvencibilidade())
                {
                    MoverParaLonge();
                }
                else
                {
                    if (Random.value < chanceMovimentoDuplo)
                    {
                        yield return StartCoroutine(MoverEmDirecaoAoAlvoComRetorno());
                        yield return new WaitForSeconds(0.15f);
                        if (!estaMovendo && ativo)
                        {
                            yield return StartCoroutine(MoverEmDirecaoAoAlvoComRetorno());
                        }
                    }
                    else
                    {
                        MoverEmDirecaoAoAlvo();
                    }
                }
            }
        }
    }

    private IEnumerator MoverEmDirecaoAoAlvoComRetorno()
    {
        MoverEmDirecaoAoAlvo();
        while (estaMovendo)
        {
            yield return null;
        }
    }

    private void MoverEmDirecaoAoAlvo()
    {
        if (alvo == null || tabuleiro == null)
        {
            if (alvo == null) alvo = Personagem.Instancia ?? FindObjectOfType<Personagem>();
            if (tabuleiro == null) tabuleiro = Tabuleiro.Instancia ?? FindObjectOfType<Tabuleiro>();
            
            if (alvo == null || tabuleiro == null)
            {
                Debug.LogWarning($"Inimigo {gameObject.name}: alvo={alvo != null}, tabuleiro={tabuleiro != null}");
                return;
            }
        }

        Vector2Int melhorMovimento = CalcularMelhorDirecao();
        
        if (melhorMovimento != Vector2Int.zero)
        {
            int novoX = posicaoX + melhorMovimento.x;
            int novoY = posicaoY + melhorMovimento.y;

            if (PodeMoverPara(novoX, novoY))
            {
                StartCoroutine(MoverPara(novoX, novoY));
            }
            else
            {
                TentarMovimentoAlternativo(melhorMovimento);
            }
        }
        else
        {
            TentarMovimentoAleatorio();
        }
    }

    private Vector2Int CalcularMelhorDirecao()
    {
        if (alvo == null) return Vector2Int.zero;
        
        int alvoX = alvo.PosicaoX;
        int alvoY = alvo.PosicaoY;
        
        int deltaX = alvoX - posicaoX;
        int deltaY = alvoY - posicaoY;
        
        if (deltaX == 0 && deltaY == 0) return Vector2Int.zero;
        
        List<Vector2Int> todasDirecoes = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        todasDirecoes.Sort((a, b) =>
        {
            float distA = Vector2.Distance(
                new Vector2(posicaoX + a.x, posicaoY + a.y),
                new Vector2(alvoX, alvoY)
            );
            float distB = Vector2.Distance(
                new Vector2(posicaoX + b.x, posicaoY + b.y),
                new Vector2(alvoX, alvoY)
            );
            return distA.CompareTo(distB);
        });
        
        foreach (var dir in todasDirecoes)
        {
            int novoX = posicaoX + dir.x;
            int novoY = posicaoY + dir.y;
            
            if (PodeMoverPara(novoX, novoY))
            {
                return dir;
            }
        }
        
        return Vector2Int.zero;
    }

    private void TentarMovimentoAlternativo(Vector2Int direcaoBloqueada)
    {
        Vector2Int[] alternativas;
        
        if (direcaoBloqueada.x != 0)
        {
            alternativas = new Vector2Int[] { Vector2Int.up, Vector2Int.down };
        }
        else
        {
            alternativas = new Vector2Int[] { Vector2Int.right, Vector2Int.left };
        }
        
        if (alvo != null)
        {
            System.Array.Sort(alternativas, (a, b) =>
            {
                float distA = Vector2.Distance(
                    new Vector2(posicaoX + a.x, posicaoY + a.y),
                    new Vector2(alvo.PosicaoX, alvo.PosicaoY)
                );
                float distB = Vector2.Distance(
                    new Vector2(posicaoX + b.x, posicaoY + b.y),
                    new Vector2(alvo.PosicaoX, alvo.PosicaoY)
                );
                return distA.CompareTo(distB);
            });
        }
        
        foreach (var dir in alternativas)
        {
            int novoX = posicaoX + dir.x;
            int novoY = posicaoY + dir.y;
            
            if (PodeMoverPara(novoX, novoY))
            {
                StartCoroutine(MoverPara(novoX, novoY));
                return;
            }
        }
    }

    private Vector2Int EncontrarMovimentoComPredicao()
    {
        return CalcularMelhorDirecao();
    }

    private void TentarMovimentoAleatorio()
    {
        Vector2Int[] direcoes = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = direcoes.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = direcoes[i];
            direcoes[i] = direcoes[j];
            direcoes[j] = temp;
        }

        foreach (var dir in direcoes)
        {
            int novoX = posicaoX + dir.x;
            int novoY = posicaoY + dir.y;

            if (PodeMoverPara(novoX, novoY))
            {
                StartCoroutine(MoverPara(novoX, novoY));
                return;
            }
        }
    }

    private void MoverParaLonge()
    {
        if (alvo == null || tabuleiro == null) return;

        Vector2Int melhorMovimento = EncontrarMelhorMovimento(alvo.PosicaoX, alvo.PosicaoY, false);
        
        if (melhorMovimento != Vector2Int.zero)
        {
            int novoX = posicaoX + melhorMovimento.x;
            int novoY = posicaoY + melhorMovimento.y;

            if (PodeMoverPara(novoX, novoY))
            {
                StartCoroutine(MoverPara(novoX, novoY));
            }
        }
    }

    private Vector2Int EncontrarMelhorMovimento(int alvoX, int alvoY, bool aproximar)
    {
        Vector2Int[] direcoes = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2Int melhor = Vector2Int.zero;
        float melhorDistancia = aproximar ? float.MaxValue : float.MinValue;

        foreach (var dir in direcoes)
        {
            int novoX = posicaoX + dir.x;
            int novoY = posicaoY + dir.y;

            if (!PodeMoverPara(novoX, novoY)) continue;

            float distancia = Vector2.Distance(
                new Vector2(novoX, novoY),
                new Vector2(alvoX, alvoY)
            );

            if (aproximar)
            {
                if (distancia < melhorDistancia)
                {
                    melhorDistancia = distancia;
                    melhor = dir;
                }
            }
            else
            {
                if (distancia > melhorDistancia)
                {
                    melhorDistancia = distancia;
                    melhor = dir;
                }
            }
        }

        return melhor;
    }

    private bool PodeMoverPara(int x, int y)
    {
        if (tabuleiro == null) return false;
        
        if (x < 0 || x >= tabuleiro.Largura || y < 0 || y >= tabuleiro.Altura)
            return false;

        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula == null)
            return false;

        if (alvo != null && x == alvo.PosicaoX && y == alvo.PosicaoY)
            return true;

        if (!celula.Transitavel)
            return false;

        Inimigo[] inimigos = FindObjectsOfType<Inimigo>();
        foreach (var inimigo in inimigos)
        {
            if (inimigo != this && inimigo.PosicaoX == x && inimigo.PosicaoY == y)
                return false;
        }

        return true;
    }

    private IEnumerator MoverPara(int novoX, int novoY)
    {
        estaMovendo = true;

        Celula celulaDestino = tabuleiro.ObterCelula(novoX, novoY);
        if (celulaDestino == null)
        {
            estaMovendo = false;
            yield break;
        }

        Vector2 posInicial = retanguloTransform.anchoredPosition;
        RectTransform celulaRt = celulaDestino.GetComponent<RectTransform>();
        Vector2 posFinal = celulaRt.anchoredPosition;

        float tempo = 0f;
        float duracao = velocidadeMovimento;

        Vector3 escalaOriginal = retanguloTransform.localScale;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            
            float tCurva = t < 0.5f 
                ? 2f * t * t 
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            
            retanguloTransform.anchoredPosition = Vector2.Lerp(posInicial, posFinal, tCurva);
            
            float escalaBonus = Mathf.Sin(t * Mathf.PI) * 0.15f;
            retanguloTransform.localScale = escalaOriginal * (1f + escalaBonus);
            
            yield return null;
        }

        retanguloTransform.anchoredPosition = posFinal;
        retanguloTransform.localScale = escalaOriginal;
        posicaoX = novoX;
        posicaoY = novoY;

        VerificarColisaoJogador();

        estaMovendo = false;
    }

    private void PosicionarEm(int x, int y)
    {
        Celula celula = tabuleiro.ObterCelula(x, y);
        if (celula != null)
        {
            RectTransform celulaRt = celula.GetComponent<RectTransform>();
            retanguloTransform.anchoredPosition = celulaRt.anchoredPosition;
        }
        posicaoX = x;
        posicaoY = y;
    }
    #endregion

    #region Colisão e Dano
    private void VerificarColisaoJogador()
    {
        if (alvo == null) return;

        if (posicaoX == alvo.PosicaoX && posicaoY == alvo.PosicaoY)
        {
            CausarDano();
        }
    }

    public void VerificarColisao(int jogadorX, int jogadorY)
    {
        if (posicaoX == jogadorX && posicaoY == jogadorY)
        {
            CausarDano();
        }
    }

    private void CausarDano()
    {
        if (cooldownDano > 0f) return;
        
        GerenciadorJogo gerenciador = GerenciadorJogo.Instancia;
        if (gerenciador != null)
        {
            if (gerenciador.TemInvencibilidade())
            {
                StartCoroutine(AnimacaoMorte());
                return;
            }

            gerenciador.JogadorReceberDano(dano);
            cooldownDano = TEMPO_COOLDOWN_DANO;
            StartCoroutine(AnimacaoAtaque());
        }
    }
    #endregion

    #region Animações
    private IEnumerator AnimarPulsacao()
    {
        while (ativo)
        {
            float tempo = Time.time * 3f;
            float escala = 1f + Mathf.Sin(tempo) * 0.08f;
            retanguloTransform.localScale = Vector3.one * escala;
            
            float intensidade = 0.9f + Mathf.Sin(tempo * 2f) * 0.1f;
            if (imagemInimigo != null)
            {
                imagemInimigo.color = new Color(
                    corInimigo.r * intensidade,
                    corInimigo.g,
                    corInimigo.b,
                    corInimigo.a
                );
            }
            
            yield return null;
        }
    }

    private IEnumerator AnimacaoAtaque()
    {
        Color corOriginal = imagemInimigo.color;
        imagemInimigo.color = Color.white;
        
        retanguloTransform.localScale = Vector3.one * 1.3f;
        
        yield return new WaitForSeconds(0.1f);
        
        imagemInimigo.color = corOriginal;
        retanguloTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimacaoMorte()
    {
        ativo = false;
        PararPerseguicao();

        float duracao = 0.5f;
        float tempo = 0f;
        Vector3 escalaInicial = retanguloTransform.localScale;
        Color corInicial = imagemInimigo.color;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;

            retanguloTransform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            imagemInimigo.color = new Color(corInicial.r, corInicial.g, corInicial.b, 1f - t);
            retanguloTransform.Rotate(0, 0, 360f * Time.deltaTime);

            yield return null;
        }

        Destroy(gameObject);
    }
    #endregion
}
