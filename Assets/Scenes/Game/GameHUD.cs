using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text itemsText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text powerUpText;
    [SerializeField] private Text messageText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text winScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color hudBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 1f);
    
    [Header("Progress Bar")]
    [SerializeField] private GameObject progressBarContainer;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Text progressText;
    
    private GameManager gameManager;
    private Coroutine messageCoroutine;
    
    public static GameHUD Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("GameHUD duplicado detectado! Destruindo instância duplicada.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        StartCoroutine(InitializeHUD());
    }
    
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        float topMargin = (screenSize.y - safeArea.yMax) / screenSize.y;
        float bottomMargin = safeArea.yMin / screenSize.y;
        float leftMargin = safeArea.xMin / screenSize.x;
        float rightMargin = (screenSize.x - safeArea.xMax) / screenSize.x;
        
        if (hudPanel != null)
        {
            RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
            if (hudRect != null)
            {
                hudRect.anchoredPosition = new Vector2(0f, -(30f + topMargin * Screen.height));
            }
        }
        
        Debug.Log($"Safe area aplicada: Top={topMargin}, Bottom={bottomMargin}, Left={leftMargin}, Right={rightMargin}");
    }
    
    private IEnumerator InitializeHUD()
    {
        while (gameManager == null)
        {
            gameManager = GameManager.Instance;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Verificar se já foi inicializado para evitar duplicação
        if (hudPanel != null)
        {
            Debug.LogWarning("HUD já foi inicializado, evitando duplicação.");
            yield break;
        }
        
        CreateHUDElements();
        
        ApplySafeArea();
        
        Debug.Log("GameHUD inicializado!");
    }
    
    private void CreateHUDElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Verificar se já existem elementos HUD duplicados antes de criar novos
        GameObject[] existingHUDs = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in existingHUDs)
        {
            if (obj.name == "Game HUD" && obj != hudPanel)
            {
                Debug.LogWarning($"Removendo HUD duplicado: {obj.name}");
                Destroy(obj);
            }
        }
        
        CreateMainHUDPanel(canvas.transform);
        
        CreateProgressBar();
        
        CreateMessageArea();
        
        CreateWinPanel(canvas.transform);
    }
    
    private void CreateMainHUDPanel(Transform parent)
    {
        GameObject panel = new GameObject("Game HUD");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelTransform = panel.AddComponent<RectTransform>();

        float hudHeight = Mathf.Max(40f, Screen.height * 0.08f);
        panelTransform.anchorMin = new Vector2(0f, 1f);
        panelTransform.anchorMax = new Vector2(1f, 1f);
        panelTransform.sizeDelta = new Vector2(0f, hudHeight);
        panelTransform.anchoredPosition = new Vector2(0f, -hudHeight/2f);
        
        Image panelBackground = panel.AddComponent<Image>();
        panelBackground.color = hudBackgroundColor;
        
        hudPanel = panel;
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 10;
        
        CreateHUDText("Score", "💰0", panel.transform, ref scoreText);
        CreateHUDText("Level", "🚀Nível 1", panel.transform, ref itemsText);
        CreateHUDText("Time", "⏱️0:00", panel.transform, ref timeText);
        CreateHUDText("PowerUps", "", panel.transform, ref powerUpText);
    }
    
    private void CreateHUDText(string name, string text, Transform parent, ref Text textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 14;
        textComponent.color = textColor;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontStyle = FontStyle.Bold;
        
        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, 1f);
    }
    
    private void CreateProgressBar()
    {
        GameObject container = new GameObject("Progress Bar Container");
        container.transform.SetParent(hudPanel.transform.parent, false);
        
        RectTransform containerTransform = container.AddComponent<RectTransform>();

        containerTransform.anchorMin = new Vector2(0f, 1f);
        containerTransform.anchorMax = new Vector2(1f, 1f);
        containerTransform.sizeDelta = new Vector2(-20f, 20f);
        containerTransform.anchoredPosition = new Vector2(0f, -80f);
        
        progressBarContainer = container;
        
        Image backgroundBar = container.AddComponent<Image>();
        backgroundBar.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        GameObject fillGO = new GameObject("Progress Fill");
        fillGO.transform.SetParent(container.transform, false);
        
        RectTransform fillTransform = fillGO.AddComponent<RectTransform>();
        fillTransform.anchorMin = Vector2.zero;
        fillTransform.anchorMax = Vector2.one;
        fillTransform.sizeDelta = Vector2.zero;
        fillTransform.anchoredPosition = Vector2.zero;
        
        progressBarFill = fillGO.AddComponent<Image>();
        progressBarFill.color = highlightColor;
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        
        GameObject progressTextGO = new GameObject("Progress Text");
        progressTextGO.transform.SetParent(container.transform, false);
        
        RectTransform progressTextTransform = progressTextGO.AddComponent<RectTransform>();
        progressTextTransform.anchorMin = Vector2.zero;
        progressTextTransform.anchorMax = Vector2.one;
        progressTextTransform.sizeDelta = Vector2.zero;
        progressTextTransform.anchoredPosition = Vector2.zero;
        
        progressText = progressTextGO.AddComponent<Text>();
        progressText.text = "Progresso: 0%";
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = 16;
        progressText.color = Color.white;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.fontStyle = FontStyle.Bold;
        
        Outline progressOutline = progressTextGO.AddComponent<Outline>();
        progressOutline.effectColor = Color.black;
        progressOutline.effectDistance = new Vector2(1f, 1f);
    }
    
    private void CreateMessageArea()
    {
        GameObject messageGO = new GameObject("Message Text");
        messageGO.transform.SetParent(hudPanel.transform.parent, false);
        
        RectTransform messageTransform = messageGO.AddComponent<RectTransform>();

        messageTransform.anchorMin = new Vector2(0.5f, 0.5f);
        messageTransform.anchorMax = new Vector2(0.5f, 0.5f);
        messageTransform.sizeDelta = new Vector2(400f, 100f);
        messageTransform.anchoredPosition = Vector2.zero;
        
        messageText = messageGO.AddComponent<Text>();
        messageText.text = "";
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 24;
        messageText.color = highlightColor;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontStyle = FontStyle.Bold;
        
        Outline messageOutline = messageGO.AddComponent<Outline>();
        messageOutline.effectColor = Color.black;
        messageOutline.effectDistance = new Vector2(2f, 2f);
        
        messageGO.SetActive(false);
    }
    
    private void CreateWinPanel(Transform parent)
    {
        GameObject panel = new GameObject("Win Panel");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelTransform = panel.AddComponent<RectTransform>();

        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;
        
        Image panelBackground = panel.AddComponent<Image>();
        panelBackground.color = new Color(0f, 0f, 0f, 0.8f);
        
        winPanel = panel;
        
        GameObject centerBox = new GameObject("Win Box");
        centerBox.transform.SetParent(panel.transform, false);
        
        RectTransform centerTransform = centerBox.AddComponent<RectTransform>();
        centerTransform.anchorMin = new Vector2(0.15f, 0.2f);
        centerTransform.anchorMax = new Vector2(0.85f, 0.8f);
        centerTransform.offsetMin = Vector2.zero;
        centerTransform.offsetMax = Vector2.zero;
        
        Image centerBackground = centerBox.AddComponent<Image>();
        centerBackground.color = new Color(0.1f, 0.5f, 0.1f, 0.9f);
        
        CreateWinText(centerBox.transform);
        CreateWinButtons(centerBox.transform);
        
        panel.SetActive(false);
    }
    
    private void CreateWinText(Transform parent)
    {
        GameObject textGO = new GameObject("Win Text");
        textGO.transform.SetParent(parent, false);
        
        RectTransform textTransform = textGO.AddComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.1f, 0.5f);
        textTransform.anchorMax = new Vector2(0.9f, 0.9f);
        textTransform.sizeDelta = Vector2.zero;
        textTransform.anchoredPosition = Vector2.zero;
        
        winScoreText = textGO.AddComponent<Text>();
        winScoreText.text = "🏆 VITÓRIA! 🏆";
        winScoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winScoreText.fontSize = 28;
        winScoreText.color = Color.yellow;
        winScoreText.alignment = TextAnchor.MiddleCenter;
        winScoreText.fontStyle = FontStyle.Bold;
    }
    
    private void CreateWinButtons(Transform parent)
    {
        CreateButton("Restart Button", "🔄 JOGAR NOVAMENTE", parent, new Vector2(0.1f, 0.1f), new Vector2(0.45f, 0.4f), () => {
            gameManager?.RestartGame();
        });
        
        CreateButton("Menu Button", "🏠 MENU PRINCIPAL", parent, new Vector2(0.55f, 0.1f), new Vector2(0.9f, 0.4f), () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        });
    }
    
    private void CreateButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform buttonTransform = buttonGO.AddComponent<RectTransform>();
        buttonTransform.anchorMin = anchorMin;
        buttonTransform.anchorMax = anchorMax;
        buttonTransform.sizeDelta = Vector2.zero;
        buttonTransform.anchoredPosition = Vector2.zero;
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.8f, 0.8f);
        
        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke());
        
        GameObject buttonTextGO = new GameObject("Button Text");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform buttonTextTransform = buttonTextGO.AddComponent<RectTransform>();
        buttonTextTransform.anchorMin = Vector2.zero;
        buttonTextTransform.anchorMax = Vector2.one;
        buttonTextTransform.sizeDelta = Vector2.zero;
        buttonTextTransform.anchoredPosition = Vector2.zero;
        
        Text buttonText = buttonTextGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
    }
    
    void Update()
    {
        if (gameManager != null)
        {
            UpdateHUDElements();
        }
    }
    
    private void UpdateHUDElements()
    {
        if (scoreText != null)
        {
            scoreText.text = $"💰{gameManager.TotalScore}";
        }
        
        if (itemsText != null)
        {
            itemsText.text = $"🚀Nível {gameManager.CurrentLevel}";
        }
        
        if (timeText != null)
        {
            timeText.text = $"⏱️{FormatTime(gameManager.GameTime)}";
        }
        
        UpdatePowerUpDisplay();
        
        UpdateProgressBar();
    }
    
    private void UpdatePowerUpDisplay()
    {
        if (powerUpText != null && gameManager != null)
        {
            List<string> activePowerUps = new List<string>();
            
            if (gameManager.HasSpeedBoost()) activePowerUps.Add("⚡");
            if (gameManager.HasScoreMultiplier()) activePowerUps.Add("✨");
            if (gameManager.HasInvincibility()) activePowerUps.Add("🛡️");
            
            powerUpText.text = activePowerUps.Count > 0 ? string.Join(" ", activePowerUps) : "";
        }
    }
    
    private void UpdateProgressBar()
    {
        if (progressBarFill != null && progressText != null && gameManager != null)
        {
            Collectible[] allCollectibles = FindObjectsOfType<Collectible>();
            int totalImportantItems = 0;
            int collectedItems = gameManager.CoinsCollected + gameManager.GemsCollected + gameManager.KeysCollected;
            
            foreach (Collectible c in allCollectibles)
            {
                if (c.Type != CollectibleType.PowerUp) totalImportantItems++;
            }
            totalImportantItems += collectedItems;
            
            float progress = totalImportantItems > 0 ? (float)collectedItems / totalImportantItems : 0f;
            progressBarFill.fillAmount = progress;
            progressText.text = $"Nível {gameManager.CurrentLevel}: {Mathf.RoundToInt(progress * 100)}%";
        }
    }
    
    public void ShowTemporaryMessage(string message, float duration = 3f)
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        
        messageCoroutine = StartCoroutine(DisplayMessage(message, duration));
    }
    
    private IEnumerator DisplayMessage(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = message;
            
            yield return new WaitForSeconds(duration);
            
            messageText.gameObject.SetActive(false);
        }
    }
    
    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winScoreText != null && gameManager != null)
            {
                int timeBonus = Mathf.Max(0, 300 - Mathf.RoundToInt(gameManager.GameTime));
                winScoreText.text = $"🏆 VITÓRIA! 🏆\n\n" +
                                   $"💰 Pontuação: {gameManager.TotalScore}\n" +
                                   $"⏱️ Tempo: {FormatTime(gameManager.GameTime)}\n" +
                                   $"🎯 Bônus: {timeBonus} pontos\n\n" +
                                   $"🪙 Moedas: {gameManager.CoinsCollected}\n" +
                                   $"💎 Gemas: {gameManager.GemsCollected}\n" +
                                   $"🗝️ Chaves: {gameManager.KeysCollected}";
            }
        }
    }
    
    public void ShowGameCompletionPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winScoreText != null && gameManager != null)
            {
                winScoreText.text = $"🏆 JOGO COMPLETADO! 🏆\n\n" +
                                   $"🎊 Parabéns! Você completou todos os níveis!\n\n" +
                                   $"💰 Pontuação Final: {gameManager.TotalScore}\n" +
                                   $"⏱️ Tempo Total: {FormatTime(gameManager.GameTime)}\n" +
                                   $"🚀 Nível Final: {gameManager.CurrentLevel}\n\n" +
                                   $"🌟 Você é um mestre! 🌟";
            }
        }
    }
    
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    public void ShowCollectionFeedback(CollectibleType type, int value)
    {
        string message = "";
        switch (type)
        {
            case CollectibleType.Coin:
                message = $"🪙 +{value} pontos!";
                break;
            case CollectibleType.Gem:
                message = $"💎 +{value} pontos!";
                break;
            case CollectibleType.Key:
                message = $"🗝️ +{value} pontos!";
                break;
            case CollectibleType.PowerUp:
                message = "⚡ Power-up ativado!";
                break;
        }
        
        ShowTemporaryMessage(message, 1.5f);
    }
}
