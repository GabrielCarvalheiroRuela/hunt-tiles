using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int coinsCollected = 0;
    [SerializeField] private int gemsCollected = 0;
    [SerializeField] private int keysCollected = 0;
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private bool gameWon = false;
    
    [Header("Collectible Spawn Settings")]
    [SerializeField] private int minCoins = 8;
    [SerializeField] private int maxCoins = 15;
    [SerializeField] private int minGems = 3;
    [SerializeField] private int maxGems = 6;
    [SerializeField] private int minKeys = 1;
    [SerializeField] private int maxKeys = 3;
    [SerializeField] private int powerUpCount = 2;
    
    [Header("Obstacle Spawn Settings")]
    [SerializeField] private int wallCount = 4;
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text itemsText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text winScoreText;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip gemSound;
    [SerializeField] private AudioClip keySound;
    [SerializeField] private AudioClip powerUpSound;
    [SerializeField] private AudioClip winSound;
    
    private List<Collectible> allCollectibles = new List<Collectible>();
    private List<Obstacle> allObstacles = new List<Obstacle>();
    private GridBoard gridBoard;
    private BoardCharacter playerCharacter;
    private bool[] powerUpEffects = new bool[3];
    private float[] powerUpTimers = new float[3];
    
    public static GameManager Instance { get; private set; }
    
    public int TotalScore => totalScore;
    public int CoinsCollected => coinsCollected;
    public int GemsCollected => gemsCollected;
    public int KeysCollected => keysCollected;
    public float GameTime => gameTime;
    public bool GameWon => gameWon;
    
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
        StartCoroutine(InitializeGameManager());
    }
    
    void Update()
    {
        if (!gameWon)
        {
            gameTime += Time.deltaTime;
            UpdateUI();
            UpdatePowerUps();
            CheckWinCondition();
        }
    }
    
    private IEnumerator InitializeGameManager()
    {
        while (gridBoard == null || playerCharacter == null)
        {
            gridBoard = FindObjectOfType<GridBoard>();
            playerCharacter = FindObjectOfType<BoardCharacter>();
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        SpawnObstacles();
        SpawnCollectibles();
        SetupUI();
        
        Debug.Log("GameManager inicializado! Colete todos os itens para vencer!");
    }
    
    private void SpawnCollectibles()
    {
        if (gridBoard == null) return;
        
        List<Vector2Int> availablePositions = GetAvailablePositions();
        
        int coinCount = Random.Range(minCoins, maxCoins + 1);
        SpawnCollectiblesOfType(CollectibleType.Coin, coinCount, availablePositions);
        
        int gemCount = Random.Range(minGems, maxGems + 1);
        SpawnCollectiblesOfType(CollectibleType.Gem, gemCount, availablePositions);
        
        int keyCount = Random.Range(minKeys, maxKeys + 1);
        SpawnCollectiblesOfType(CollectibleType.Key, keyCount, availablePositions);
        
        SpawnCollectiblesOfType(CollectibleType.PowerUp, powerUpCount, availablePositions);
        
        ValidateCollectibleAccessibility();
        
        Debug.Log($"Coletáveis criados: {coinCount} moedas, {gemCount} gemas, {keyCount} chaves, {powerUpCount} power-ups");
    }
    
    private void ValidateCollectibleAccessibility()
    {
        List<Collectible> inaccessibleItems = new List<Collectible>();
        
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible != null && !IsPositionAccessible(collectible.TileX, collectible.TileY))
            {
                inaccessibleItems.Add(collectible);
            }
        }
        
        foreach (Collectible item in inaccessibleItems)
        {
            Vector2Int newPosition = FindAccessiblePosition();
            if (newPosition.x >= 0 && newPosition.y >= 0)
            {
                GridTileUI newTile = gridBoard.GetTile(newPosition.x, newPosition.y);
                if (newTile != null)
                {
                    RectTransform itemTransform = item.GetComponent<RectTransform>();
                    RectTransform tileTransform = newTile.GetComponent<RectTransform>();
                    itemTransform.anchoredPosition = tileTransform.anchoredPosition;
                    
                    item.Initialize(item.Type, newPosition.x, newPosition.y);
                    Debug.Log($"Item {item.Type} reposicionado de ({item.TileX}, {item.TileY}) para ({newPosition.x}, {newPosition.y})");
                }
            }
        }
    }
    
    private bool IsPositionAccessible(int targetX, int targetY)
    {
        bool[,] visited = new bool[gridBoard.Width, gridBoard.Height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(new Vector2Int(0, 0));
        visited[0, 0] = true;
        
        Vector2Int[] directions = { 
            new Vector2Int(0, 1), new Vector2Int(0, -1), 
            new Vector2Int(1, 0), new Vector2Int(-1, 0) 
        };
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current.x == targetX && current.y == targetY)
                return true;
            
            foreach (Vector2Int dir in directions)
            {
                int nextX = current.x + dir.x;
                int nextY = current.y + dir.y;
                
                if (nextX >= 0 && nextX < gridBoard.Width && 
                    nextY >= 0 && nextY < gridBoard.Height && 
                    !visited[nextX, nextY])
                {
                    // Verificar se não há obstáculo bloqueando
                    if (!IsObstacleAt(nextX, nextY, ObstacleType.Wall))
                    {
                        visited[nextX, nextY] = true;
                        queue.Enqueue(new Vector2Int(nextX, nextY));
                    }
                }
            }
        }
        
        return false;
    }
    
    private bool IsObstacleAt(int x, int y, ObstacleType type)
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle != null && obstacle.TileX == x && obstacle.TileY == y && obstacle.Type == type)
                return true;
        }
        return false;
    }
    
    private Vector2Int FindAccessiblePosition()
    {
        for (int x = 0; x < gridBoard.Width; x++)
        {
            for (int y = 0; y < gridBoard.Height; y++)
            {
                if (!IsPositionOccupied(x, y) && IsPositionAccessible(x, y))
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
    
    private void SpawnObstacles()
    {
        if (gridBoard == null) return;
        
        List<Vector2Int> availablePositions = GetAvailablePositions();
        
        List<Vector2Int> safePositions = EnsurePathAvailability(availablePositions);
        
        SpawnObstaclesOfType(ObstacleType.Wall, wallCount, safePositions);
        
        Debug.Log($"Obstáculos criados: {wallCount} paredes");
    }
    
    private void SpawnObstaclesOfType(ObstacleType type, int count, List<Vector2Int> availablePositions)
    {
        int actualCount = 0;
        int maxAttempts = count * 3;
        
        for (int attempts = 0; attempts < maxAttempts && actualCount < count && availablePositions.Count > 0; attempts++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int position = availablePositions[randomIndex];
            
            if (type == ObstacleType.Wall)
            {
                if (WouldBlockPath(position.x, position.y))
                {
                    continue;
                }
            }
            
            availablePositions.RemoveAt(randomIndex);
            CreateObstacle(type, position.x, position.y);
            actualCount++;
        }
        
        Debug.Log($"Criados {actualCount}/{count} obstáculos do tipo {type}");
    }
    
    private bool WouldBlockPath(int x, int y)
    {
        int adjacentWalls = 0;
        
        Vector2Int[] directions = { 
            new Vector2Int(0, 1), new Vector2Int(0, -1), 
            new Vector2Int(1, 0), new Vector2Int(-1, 0) 
        };
        
        foreach (Vector2Int dir in directions)
        {
            int checkX = x + dir.x;
            int checkY = y + dir.y;
            
            if (checkX < 0 || checkX >= gridBoard.Width || checkY < 0 || checkY >= gridBoard.Height)
            {
                adjacentWalls++;
                continue;
            }
            
            foreach (Obstacle obstacle in allObstacles)
            {
                if (obstacle != null && obstacle.Type == ObstacleType.Wall && 
                    obstacle.TileX == checkX && obstacle.TileY == checkY)
                {
                    adjacentWalls++;
                    break;
                }
            }
        }
        
        return adjacentWalls >= 2;
    }
    
    private void CreateObstacle(ObstacleType type, int x, int y)
    {
        GameObject obstacleGO = new GameObject($"{type}_{x}_{y}");
        
        obstacleGO.transform.SetParent(gridBoard.transform, false);
        
        Obstacle obstacle = obstacleGO.AddComponent<Obstacle>();
        obstacle.Initialize(type, x, y);
        
        GridTileUI tile = gridBoard.GetTile(x, y);
        if (tile != null)
        {
            RectTransform obstacleTransform = obstacle.GetComponent<RectTransform>();
            RectTransform tileTransform = tile.GetComponent<RectTransform>();
            
            obstacleTransform.anchoredPosition = tileTransform.anchoredPosition;
        }
        
        allObstacles.Add(obstacle);
    }
    
    private List<Vector2Int> EnsurePathAvailability(List<Vector2Int> availablePositions)
    {
        List<Vector2Int> reservedPositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridBoard.Width && x < 3; x++)
        {
            for (int y = 0; y < gridBoard.Height && y < 3; y++)
            {
                if (x == 0 && y == 0) continue;
                
                Vector2Int pos = new Vector2Int(x, y);
                reservedPositions.Add(pos);
            }
        }
        
        for (int x = 0; x < gridBoard.Width; x += 2)
        {
            reservedPositions.Add(new Vector2Int(x, gridBoard.Height - 1));
        }
        
        for (int y = 0; y < gridBoard.Height; y += 2)
        {
            reservedPositions.Add(new Vector2Int(gridBoard.Width - 1, y));
        }
        
        foreach (Vector2Int reserved in reservedPositions)
        {
            availablePositions.Remove(reserved);
        }
        
        Debug.Log($"Reservadas {reservedPositions.Count} posições para garantir caminho livre");
        return availablePositions;
    }
    
    private List<Vector2Int> GetAvailablePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int x = 0; x < gridBoard.Width; x++)
        {
            for (int y = 0; y < gridBoard.Height; y++)
            {
                if (x == 0 && y == 0) continue;
                
                if (!IsPositionOccupied(x, y))
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return positions;
    }
    
    private bool IsPositionOccupied(int x, int y)
    {
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible != null && collectible.TileX == x && collectible.TileY == y)
                return true;
        }
        
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle != null && obstacle.TileX == x && obstacle.TileY == y)
                return true;
        }
        
        return false;
    }
    
    private void SpawnCollectiblesOfType(CollectibleType type, int count, List<Vector2Int> availablePositions)
    {
        for (int i = 0; i < count && availablePositions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int position = availablePositions[randomIndex];
            availablePositions.RemoveAt(randomIndex);
            
            CreateCollectible(type, position.x, position.y);
        }
    }
    
    private void CreateCollectible(CollectibleType type, int x, int y)
    {
        GameObject collectibleGO = new GameObject($"{type}_{x}_{y}");
        
        collectibleGO.transform.SetParent(gridBoard.transform, false);
        
        Collectible collectible = collectibleGO.AddComponent<Collectible>();
        collectible.Initialize(type, x, y);
        
        GridTileUI tile = gridBoard.GetTile(x, y);
        if (tile != null)
        {
            RectTransform collectibleTransform = collectible.GetComponent<RectTransform>();
            RectTransform tileTransform = tile.GetComponent<RectTransform>();
            
            collectibleTransform.anchoredPosition = tileTransform.anchoredPosition;
        }
        
        allCollectibles.Add(collectible);
    }
    
    public void OnCollectibleCollected(Collectible collectible)
    {
        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            gameHUD.ShowCollectionFeedback(collectible.Type, collectible.Value);
        }
        
        switch (collectible.Type)
        {
            case CollectibleType.Coin:
                coinsCollected++;
                AddScore(collectible.Value);
                PlaySound(coinSound);
                break;
                
            case CollectibleType.Gem:
                gemsCollected++;
                AddScore(collectible.Value);
                PlaySound(gemSound);
                break;
                
            case CollectibleType.Key:
                keysCollected++;
                AddScore(collectible.Value);
                PlaySound(keySound);
                break;
                
            case CollectibleType.PowerUp:
                ActivateRandomPowerUp();
                PlaySound(powerUpSound);
                break;
        }
        
        allCollectibles.Remove(collectible);
        UpdateUI();
    }
    
    private void AddScore(int points)
    {
        int multiplier = powerUpEffects[1] ? 2 : 1;
        totalScore += points * multiplier;
    }
    
    private void ActivateRandomPowerUp()
    {
        int randomPowerUp = Random.Range(0, 3);
        powerUpEffects[randomPowerUp] = true;
        powerUpTimers[randomPowerUp] = 10f;
        
        string powerUpName = "";
        switch (randomPowerUp)
        {
            case 0: powerUpName = "Velocidade Aumentada"; break;
            case 1: powerUpName = "Pontuação Dupla"; break;
            case 2: powerUpName = "Invencibilidade"; break;
        }
        
        Debug.Log($"Power-up ativado: {powerUpName}!");
    }
    
    private void UpdatePowerUps()
    {
        for (int i = 0; i < powerUpEffects.Length; i++)
        {
            if (powerUpEffects[i])
            {
                powerUpTimers[i] -= Time.deltaTime;
                if (powerUpTimers[i] <= 0f)
                {
                    powerUpEffects[i] = false;
                }
            }
        }
    }
    
    private void CheckWinCondition()
    {
        int remainingImportantItems = 0;
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible.Type != CollectibleType.PowerUp)
            {
                remainingImportantItems++;
            }
        }
        
        if (remainingImportantItems == 0 && !gameWon)
        {
            WinGame();
        }
    }
    
    private void WinGame()
    {
        gameWon = true;
        
        int timeBonus = Mathf.Max(0, 300 - Mathf.RoundToInt(gameTime));
        totalScore += timeBonus;
        
        PlaySound(winSound);
        
        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            gameHUD.ShowWinPanel();
        }
        else if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winScoreText != null)
            {
                winScoreText.text = $"Pontuação Final: {totalScore}\nTempo: {FormatTime(gameTime)}\nBônus de Tempo: {timeBonus}";
            }
        }
        
        Debug.Log($"VITÓRIA! Pontuação final: {totalScore} pontos em {FormatTime(gameTime)}");
    }
    
    private void SetupUI()
    {
        if (scoreText == null || itemsText == null || timeText == null)
        {
            CreateGameUI();
        }
    }
    
    private void CreateGameUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject uiPanel = new GameObject("Game UI");
        uiPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelTransform = uiPanel.AddComponent<RectTransform>();
        panelTransform.anchorMin = new Vector2(0f, 1f);
        panelTransform.anchorMax = new Vector2(1f, 1f);
        panelTransform.pivot = new Vector2(0.5f, 1f);
        panelTransform.sizeDelta = new Vector2(0f, 80f);
        panelTransform.anchoredPosition = Vector2.zero;
        
        Image background = uiPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.5f);
        
        CreateUIText("Score Text", "Pontuação: 0", new Vector2(-300f, -40f), uiPanel.transform);
        CreateUIText("Items Text", "Itens: 0/0", new Vector2(0f, -40f), uiPanel.transform);
        CreateUIText("Time Text", "Tempo: 00:00", new Vector2(300f, -40f), uiPanel.transform);
    }
    
    private void CreateUIText(string name, string text, Vector2 position, Transform parent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        RectTransform textTransform = textGO.AddComponent<RectTransform>();
        textTransform.anchoredPosition = position;
        textTransform.sizeDelta = new Vector2(200f, 30f);
        
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        if (name.Contains("Score")) scoreText = textComponent;
        else if (name.Contains("Items")) itemsText = textComponent;
        else if (name.Contains("Time")) timeText = textComponent;
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Pontuação: {totalScore}";
            
        if (itemsText != null)
        {
            int totalItems = coinsCollected + gemsCollected + keysCollected;
            int totalSpawned = 0;
            foreach (Collectible c in allCollectibles)
            {
                if (c.Type != CollectibleType.PowerUp) totalSpawned++;
            }
            totalSpawned += totalItems;
            itemsText.text = $"Itens: {totalItems}/{totalSpawned}";
        }
            
        if (timeText != null)
            timeText.text = $"Tempo: {FormatTime(gameTime)}";
    }
    
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public bool HasSpeedBoost() => powerUpEffects[0];
    public bool HasScoreMultiplier() => powerUpEffects[1];
    public bool HasInvincibility() => powerUpEffects[2];
    
    public Obstacle GetObstacleAt(int x, int y)
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle.GridX == x && obstacle.GridY == y && obstacle.IsActive)
            {
                return obstacle;
            }
        }
        return null;
    }
    
    public bool CanMoveToPosition(int x, int y)
    {
        if (x < 0 || x >= gridBoard.Width || y < 0 || y >= gridBoard.Height)
            return false;
            
        Obstacle obstacle = GetObstacleAt(x, y);
        if (obstacle != null && obstacle.BlocksMovement())
            return false;
            
        return true;
    }
    
    public void HandleObstacleInteraction(int x, int y)
    {
        Obstacle obstacle = GetObstacleAt(x, y);
        if (obstacle == null) return;
        
        switch (obstacle.Type)
        {
            case ObstacleType.Wall:
                // Walls block movement but have no special interaction
                break;
        }
    }
    
    private void GameOver(string reason)
    {
        gameWon = false;
        Debug.Log($"Game Over! {reason}");
        
        Invoke(nameof(RestartGame), 2f);
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
