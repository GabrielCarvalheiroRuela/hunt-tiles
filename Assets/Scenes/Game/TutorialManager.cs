using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private bool showTutorial = true;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject arrowIndicator;
    
    private bool tutorialCompleted = false;
    private int currentStep = 0;
    private string[] tutorialMessages = {
        "🎮 Bem-vindo ao Parkour Island!",
        "⬅️➡️⬆️⬇️ Use as SETAS do teclado para mover",
        "🪙 Colete MOEDAS DOURADAS (10 pontos)",
        "💎 Colete GEMAS ROXAS (50 pontos)",
        "🗝️ Colete CHAVES PRATEADAS (100 pontos)",
        "⚡ POWER-UPS VERDES dão poderes especiais",
        "🧱 PAREDES CINZAS bloqueiam o caminho",
        "🕳️ Evite BURACOS PRETOS (Game Over!)",
        "🧊 GELO AZUL faz você deslizar",
        "🌀 PORTAIS ROXOS teletransportam você",
        "🏆 Colete TODOS os itens para vencer!"
    };
    
    public static TutorialManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (showTutorial)
        {
            StartCoroutine(InitializeTutorial());
        }
        else
        {
            tutorialCompleted = true;
        }
    }
    
    void Update()
    {
        if (!tutorialCompleted && (Keyboard.current.escapeKey.wasPressedThisFrame || 
                                  Keyboard.current.enterKey.wasPressedThisFrame || 
                                  Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            SkipTutorial();
        }
    }
    
    private IEnumerator InitializeTutorial()
    {
        Debug.Log("Iniciando inicialização do tutorial...");
        
        yield return new WaitForSeconds(0.5f);
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas não encontrado! Não é possível criar tutorial.");
            tutorialCompleted = true;
            yield break;
        }
        
        Debug.Log("Canvas encontrado, criando Tutorial UI...");
        CreateTutorialUI();
        
        if (tutorialPanel != null)
        {
            Debug.Log("Tutorial UI criado com sucesso, iniciando tutorial...");
            StartTutorial();
        }
        else
        {
            Debug.LogError("Falha ao criar Tutorial UI!");
            tutorialCompleted = true;
        }
    }
    
    private void CreateTutorialUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas não encontrado para criar Tutorial UI!");
            return;
        }
        
        GameObject panel = new GameObject("Tutorial Panel");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelTransform = panel.AddComponent<RectTransform>();

        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;
        panelTransform.anchoredPosition = Vector2.zero;
        
        Image panelBackground = panel.AddComponent<Image>();
        panelBackground.color = new Color(0f, 0f, 0f, 0.8f);
        panelBackground.raycastTarget = true;
        
        Button panelButton = panel.AddComponent<Button>();
        panelButton.onClick.AddListener(SkipTutorial);
        
        tutorialPanel = panel;
        
        GameObject textBox = new GameObject("Tutorial Text Box");
        textBox.transform.SetParent(panel.transform, false);
        
        RectTransform textBoxTransform = textBox.AddComponent<RectTransform>();
        textBoxTransform.anchorMin = new Vector2(0.1f, 0.3f);
        textBoxTransform.anchorMax = new Vector2(0.9f, 0.7f);
        textBoxTransform.offsetMin = Vector2.zero;
        textBoxTransform.offsetMax = Vector2.zero;
        
        Image textBoxBackground = textBox.AddComponent<Image>();
        textBoxBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        textBoxBackground.raycastTarget = false;
        
        Outline textBoxOutline = textBox.AddComponent<Outline>();
        textBoxOutline.effectColor = new Color(1f, 0.8f, 0f, 1f);
        textBoxOutline.effectDistance = new Vector2(3f, 3f);
        
        Shadow textBoxShadow = textBox.AddComponent<Shadow>();
        textBoxShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        textBoxShadow.effectDistance = new Vector2(4f, -4f);
        
        GameObject textGO = new GameObject("Tutorial Text");
        textGO.transform.SetParent(textBox.transform, false);
        
        RectTransform textTransform = textGO.AddComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.05f, 0.1f);
        textTransform.anchorMax = new Vector2(0.95f, 0.9f);
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;
        
        tutorialText = textGO.AddComponent<Text>();
        tutorialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tutorialText.fontSize = 28;
        tutorialText.color = Color.white;
        tutorialText.alignment = TextAnchor.MiddleCenter;
        tutorialText.fontStyle = FontStyle.Bold;
        tutorialText.raycastTarget = false;
        
        CreateCloseInstructions(textBox.transform);
        
        CreateSkipButton(panel.transform);
        
        Debug.Log("Tutorial UI criado com melhor visibilidade!");
    }
    
    private void CreateCloseInstructions(Transform parent)
    {
        GameObject instructionsGO = new GameObject("Close Instructions");
        instructionsGO.transform.SetParent(parent, false);
        
        RectTransform instructionsTransform = instructionsGO.AddComponent<RectTransform>();
        instructionsTransform.anchorMin = new Vector2(0.05f, 0.8f);
        instructionsTransform.anchorMax = new Vector2(0.95f, 0.95f);
        instructionsTransform.sizeDelta = Vector2.zero;
        instructionsTransform.anchoredPosition = Vector2.zero;
        
        Text instructionsText = instructionsGO.AddComponent<Text>();
        instructionsText.text = "📱 Clique em qualquer lugar, pressione ESC, ENTER ou ESPAÇO para pular";
        instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionsText.fontSize = 16;
        instructionsText.color = new Color(1f, 1f, 0.5f, 1f);
        instructionsText.alignment = TextAnchor.MiddleCenter;
        instructionsText.fontStyle = FontStyle.Italic;
        instructionsText.raycastTarget = false;
    }
    
    private void CreateSkipButton(Transform parent)
    {
        GameObject buttonGO = new GameObject("Skip Button");
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform buttonTransform = buttonGO.AddComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.75f, 0.05f);
        buttonTransform.anchorMax = new Vector2(0.95f, 0.15f);
        buttonTransform.sizeDelta = Vector2.zero;
        buttonTransform.anchoredPosition = Vector2.zero;
        
        Image buttonBackground = buttonGO.AddComponent<Image>();
        buttonBackground.color = new Color(1f, 0.3f, 0.3f, 0.9f);

        Outline buttonOutline = buttonGO.AddComponent<Outline>();
        buttonOutline.effectColor = Color.white;
        buttonOutline.effectDistance = new Vector2(2f, 2f);
        
        skipButton = buttonGO.AddComponent<Button>();
        skipButton.onClick.AddListener(SkipTutorial);
        
        ColorBlock colors = skipButton.colors;
        colors.normalColor = new Color(1f, 0.3f, 0.3f, 0.9f);
        colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        skipButton.colors = colors;
        
        GameObject buttonTextGO = new GameObject("Button Text");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform buttonTextTransform = buttonTextGO.AddComponent<RectTransform>();
        buttonTextTransform.anchorMin = Vector2.zero;
        buttonTextTransform.anchorMax = Vector2.one;
        buttonTextTransform.sizeDelta = Vector2.zero;
        buttonTextTransform.anchoredPosition = Vector2.zero;
        
        Text buttonText = buttonTextGO.AddComponent<Text>();
        buttonText.text = "❌ PULAR";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.raycastTarget = false;
        
        // Adicionar sombra ao texto
        Shadow buttonTextShadow = buttonTextGO.AddComponent<Shadow>();
        buttonTextShadow.effectColor = Color.black;
        buttonTextShadow.effectDistance = new Vector2(1f, -1f);
    }
    
    private void StartTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            StartCoroutine(ShowTutorialMessages());
        }
    }
    
    private IEnumerator ShowTutorialMessages()
    {
        for (int i = 0; i < tutorialMessages.Length; i++)
        {
            currentStep = i;
            
            if (tutorialText != null)
            {
                StartCoroutine(TypewriterEffect(tutorialMessages[i]));
            }
            
            HighlightTutorialElement(i);

            float displayTime = Mathf.Max(messageDisplayTime, tutorialMessages[i].Length * 0.1f);
            yield return new WaitForSeconds(displayTime);
            
            if (tutorialCompleted) break;
        }
        
        FinishTutorial();
    }
    
    private IEnumerator TypewriterEffect(string message)
    {
        if (tutorialText == null) yield break;
        
        tutorialText.text = "";
        
        for (int i = 0; i <= message.Length; i++)
        {
            if (tutorialCompleted) break;
            
            tutorialText.text = message.Substring(0, i);
            yield return new WaitForSeconds(0.03f);
        }
        
        tutorialText.text = message;
    }
    
    private void HighlightTutorialElement(int step)
    {
        switch (step)
        {
            case 1: 
                HighlightPlayerCharacter();
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                HighlightCollectibles();
                break;
            default:
                ClearHighlights();
                break;
        }
    }
    
    private void HighlightPlayerCharacter()
    {
        BoardCharacter player = FindObjectOfType<BoardCharacter>();
        if (player != null)
        {
            StartCoroutine(BlinkElement(player.gameObject));
        }
    }
    
    private void HighlightCollectibles()
    {
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible collectible in collectibles)
        {
            StartCoroutine(BlinkElement(collectible.gameObject));
        }
    }
    
    private void ClearHighlights()
    {
        StopAllCoroutines();
        StartCoroutine(ShowTutorialMessages());
    }
    
    private IEnumerator BlinkElement(GameObject element)
    {
        if (element == null) yield break;
        
        Image image = element.GetComponent<Image>();
        if (image == null) yield break;
        
        Color originalColor = image.color;
        float blinkDuration = 0.5f;
        
        for (int i = 0; i < 6; i++)
        {
            image.color = Color.yellow;
            yield return new WaitForSeconds(blinkDuration);
            image.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
    
    public void SkipTutorial()
    {
        tutorialCompleted = true;
        FinishTutorial();
    }
    
    private void FinishTutorial()
    {
        tutorialCompleted = true;
        StopAllCoroutines();
        
        if (tutorialPanel != null)
        {
            StartCoroutine(FadeOutTutorial());
        }
        else
        {
            CompleteTutorialCleanup();
        }
    }
    
    private IEnumerator FadeOutTutorial()
    {
        if (tutorialPanel == null) yield break;
        
        Image panelImage = tutorialPanel.GetComponent<Image>();
        CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
        }
        
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - (elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        CompleteTutorialCleanup();
    }
    
    private void CompleteTutorialCleanup()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            Destroy(tutorialPanel);
            tutorialPanel = null;
        }
        
        ShowGameStartMessage();
        
        Debug.Log("Tutorial concluído e limpo! Jogo iniciado!");
    }
    
    private void ShowGameStartMessage()
    {
        GameHUD gameHUD = FindObjectOfType<GameHUD>();
        if (gameHUD != null)
        {
            gameHUD.ShowTemporaryMessage("🎯 OBJETIVO: Colete todos os itens para vencer!", 3f);
        }
    }
    
    public bool IsTutorialCompleted()
    {
        return tutorialCompleted;
    }
    
    public void ShowHint(string message, float duration = 2f)
    {
        if (tutorialCompleted) return;
        
        GameHUD gameHUD = FindObjectOfType<GameHUD>();
        if (gameHUD != null)
        {
            gameHUD.ShowTemporaryMessage("💡 " + message, duration);
        }
    }
    
    public void ForceRestartTutorial()
    {
        Debug.Log("Forçando restart do tutorial...");
        
        if (tutorialPanel != null)
        {
            Destroy(tutorialPanel);
            tutorialPanel = null;
        }
        
        tutorialCompleted = false;
        currentStep = 0;
        
        StartCoroutine(InitializeTutorial());
    }
    
    public void DebugTutorialState()
    {
        Debug.Log($"Tutorial Status:");
        Debug.Log($"- Completed: {tutorialCompleted}");
        Debug.Log($"- Current Step: {currentStep}");
        Debug.Log($"- Panel exists: {tutorialPanel != null}");
        Debug.Log($"- Panel active: {(tutorialPanel != null ? tutorialPanel.activeInHierarchy : false)}");
        Debug.Log($"- Show Tutorial: {showTutorial}");
        
        Canvas canvas = FindObjectOfType<Canvas>();
        Debug.Log($"- Canvas exists: {canvas != null}");
    }
}
