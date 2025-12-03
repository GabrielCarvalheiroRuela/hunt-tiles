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
    
    [Header("Level System")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private float levelTransitionDelay = 3f;
    
    [Header("Collectible Spawn Settings")]
    [SerializeField] private int baseMinCoins = 6;
    [SerializeField] private int baseMaxCoins = 10;
    [SerializeField] private int baseMinGems = 2;
    [SerializeField] private int baseMaxGems = 4;
    [SerializeField] private int baseMinKeys = 1;
    [SerializeField] private int baseMaxKeys = 2;
    [SerializeField] private int basePowerUpCount = 1;
    
    [Header("Obstacle Spawn Settings")]
    [SerializeField] private int baseWallCount = 3;
    
    [Header("UI References")]
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
    public int CurrentLevel => currentLevel;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("GameManager duplicado detectado! Destruindo instância duplicada.");
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
        if (!gameWon && IsGameInitialized())
        {
            gameTime += Time.deltaTime;
            UpdatePowerUps();
            CheckWinCondition();
        }
        else if (!IsGameInitialized() && Time.frameCount % 120 == 0) // Log every 2 seconds if not initialized
        {
            Debug.Log($"⚠️ Jogo não inicializado ainda: gridBoard={gridBoard != null}, playerCharacter={playerCharacter != null}, collectibles={allCollectibles.Count}");
        }
    }
    
    private bool IsGameInitialized()
    {
        return gridBoard != null && playerCharacter != null && allCollectibles.Count > 0;
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
        
        Debug.Log($"GameManager inicializado! NÍVEL {currentLevel} - Colete todos os itens para avançar!");

        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            string startMessage = $"🚀 NÍVEL {currentLevel} INICIANDO!\nColete todos os itens para avançar!";
            gameHUD.ShowTemporaryMessage(startMessage, 3f);
        }

    }
    
    private void SpawnCollectibles()
    {
        if (gridBoard == null) return;
        
        List<Vector2Int> availablePositions = GetAvailablePositions();
        
        // Calcular quantidades baseadas no nível
        int coinCount = Random.Range(GetLevelValue(baseMinCoins), GetLevelValue(baseMaxCoins) + 1);
        SpawnCollectiblesOfType(CollectibleType.Coin, coinCount, availablePositions);
        
        int gemCount = Random.Range(GetLevelValue(baseMinGems), GetLevelValue(baseMaxGems) + 1);
        SpawnCollectiblesOfType(CollectibleType.Gem, gemCount, availablePositions);
        
        int keyCount = Random.Range(GetLevelValue(baseMinKeys), GetLevelValue(baseMaxKeys) + 1);
        SpawnCollectiblesOfType(CollectibleType.Key, keyCount, availablePositions);
        
        int powerUpCount = GetLevelValue(basePowerUpCount);
        SpawnCollectiblesOfType(CollectibleType.PowerUp, powerUpCount, availablePositions);
        
        ValidateCollectibleAccessibility();
        
        Debug.Log($"NÍVEL {currentLevel} - Coletáveis criados: {coinCount} moedas, {gemCount} gemas, {keyCount} chaves, {powerUpCount} power-ups");
        Debug.Log($"📋 Lista final de coletáveis: {allCollectibles.Count} itens total");
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
        
        int wallCount = GetLevelValue(baseWallCount);
        SpawnObstaclesOfType(ObstacleType.Wall, wallCount, safePositions);
        
        Debug.Log($"NÍVEL {currentLevel} - Obstáculos criados: {wallCount} paredes");
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
        Debug.Log($"🎯 Coletável {collectible.Type} coletado! Valor: {collectible.Value}");
        
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
                Debug.Log($"💰 Moedas coletadas: {coinsCollected}");
                break;
                
            case CollectibleType.Gem:
                gemsCollected++;
                AddScore(collectible.Value);
                PlaySound(gemSound);
                Debug.Log($"💎 Gemas coletadas: {gemsCollected}");
                break;
                
            case CollectibleType.Key:
                keysCollected++;
                AddScore(collectible.Value);
                PlaySound(keySound);
                Debug.Log($"🗝️ Chaves coletadas: {keysCollected}");
                break;
                
            case CollectibleType.PowerUp:
                ActivateRandomPowerUp();
                PlaySound(powerUpSound);
                Debug.Log($"⚡ Power-up ativado!");
                break;
        }
        
        allCollectibles.Remove(collectible);
        Debug.Log($"📋 Lista atualizada: {allCollectibles.Count} coletáveis restantes");
        
        // Forçar verificação imediata após coletar item
        Debug.Log($"🔍 VERIFICAÇÃO IMEDIATA: Chamando CheckWinCondition() após coletar {collectible.Type}");
        CheckWinCondition();
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
        int totalCollectibles = allCollectibles.Count;
        
        List<string> remainingTypes = new List<string>();
        
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible.Type == CollectibleType.Coin || 
                collectible.Type == CollectibleType.Gem || 
                collectible.Type == CollectibleType.Key)
            {
                remainingImportantItems++;
                remainingTypes.Add(collectible.Type.ToString());
            }
        }
        
        Debug.Log($"🔍 VERIFICANDO NÍVEL {currentLevel}:");
        Debug.Log($"   📊 {remainingImportantItems} itens importantes restantes de {totalCollectibles} total");
        Debug.Log($"   � Coletados: {coinsCollected} moedas, {gemsCollected} gemas, {keysCollected} chaves");
        Debug.Log($"   📋 Tipos restantes: [{string.Join(", ", remainingTypes)}]");
        Debug.Log($"   🎮 GameWon: {gameWon}, IsGameInitialized: {IsGameInitialized()}");
        
        if (remainingImportantItems == 0 && !gameWon)
        {
            Debug.Log($"🎉 CONDIÇÃO DE VITÓRIA ATINGIDA! Todos os itens importantes coletados!");
            
            if (currentLevel >= maxLevel)
            {
                Debug.Log("🏆 JOGO COMPLETADO! Todos os níveis concluídos! 🏆");
                CompleteGame();
            }
            else
            {
                Debug.Log($"✅ NÍVEL {currentLevel} CONCLUÍDO! Preparando próximo nível...");
                AdvanceToNextLevel();
            }
        }
        else if (remainingImportantItems > 0)
        {
            Debug.Log($"⏳ Ainda faltam {remainingImportantItems} itens para completar o nível.");
        }
        else if (gameWon)
        {
            Debug.Log($"⏸️ Jogo já foi vencido, não verificando mais condições.");
        }
    }
    
    private int GetLevelValue(int baseValue)
    {
        // Aumenta a dificuldade gradualmente a cada nível
        float multiplier = 1f + (currentLevel - 1) * 0.3f;
        return Mathf.RoundToInt(baseValue * multiplier);
    }
    
    private void AdvanceToNextLevel()
    {
        gameWon = true; // Impede novas verificações
        
        // Calcular bônus do nível
        int levelBonus = CalculateLevelBonus();
        totalScore += levelBonus;
        
        // Mostrar mensagem de nível completo
        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            string message = $"🎉 NÍVEL {currentLevel} CONCLUÍDO! 🎉\n" +
                           $"💰 Bônus: +{levelBonus} pontos\n" +
                           $"🔥 Próximo: Nível {currentLevel + 1}";
            gameHUD.ShowTemporaryMessage(message, levelTransitionDelay);
        }
        
        // Avançar para o próximo nível após delay
        StartCoroutine(TransitionToNextLevel());
    }
    
    private int CalculateLevelBonus()
    {
        // Bônus baseado no tempo e nível atual
        int timeBonus = Mathf.Max(0, 180 - Mathf.RoundToInt(gameTime));
        int levelMultiplier = currentLevel * 100;
        return timeBonus + levelMultiplier;
    }
    
    private IEnumerator TransitionToNextLevel()
    {
        yield return new WaitForSeconds(levelTransitionDelay);
        
        // Preparar próximo nível
        currentLevel++;
        
        // Limpar objetos do nível anterior
        ClearLevel();
        
        // Reinicializar variáveis do nível
        gameWon = false;
        coinsCollected = 0;
        gemsCollected = 0;
        keysCollected = 0;
        
        // Gerar novo nível
        SpawnObstacles();
        SpawnCollectibles();
        
        // Mostrar mensagem do novo nível
        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            string message = $"🚀 NÍVEL {currentLevel} INICIADO! 🚀\n" +
                           $"🎯 Colete todos os itens para avançar!";
            gameHUD.ShowTemporaryMessage(message, 2f);
        }
        
        Debug.Log($"🆙 NÍVEL {currentLevel} INICIADO! Dificuldade aumentada!");
    }
    
    private void ClearLevel()
    {
        // Remover todos os coletáveis existentes
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible != null)
            {
                Destroy(collectible.gameObject);
            }
        }
        allCollectibles.Clear();
        
        // Remover todos os obstáculos existentes
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle.gameObject);
            }
        }
        allObstacles.Clear();
        
        Debug.Log("Nível anterior limpo!");
    }
    
    private void CompleteGame()
    {
        Debug.Log("🏆 JOGO COMPLETADO! Todos os níveis concluídos! 🏆");
        gameWon = true;
        
        // Bônus de conclusão do jogo
        int completionBonus = 1000 + (currentLevel * 200);
        totalScore += completionBonus;
        
        PlaySound(winSound);
        
        GameHUD gameHUD = GameHUD.Instance;
        if (gameHUD != null)
        {
            gameHUD.ShowGameCompletionPanel();
        }
        else if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winScoreText != null)
            {
                winScoreText.text = $"🏆 JOGO COMPLETADO! 🏆\n\n" +
                                   $"💰 Pontuação Final: {totalScore}\n" +
                                   $"⏱️ Tempo Total: {FormatTime(gameTime)}\n" +
                                   $"🏅 Níveis Concluídos: {currentLevel}/{maxLevel}\n" +
                                   $"🎁 Bônus de Conclusão: {completionBonus}";
            }
        }
        
        Debug.Log($"JOGO COMPLETO! Pontuação final: {totalScore} pontos, {currentLevel} níveis concluídos!");
    }
    
    private void WinGame()
    {
        // Este método agora é usado apenas para casos especiais
        // A progressão normal usa AdvanceToNextLevel() e CompleteGame()
        Debug.Log("🏆 VITÓRIA ALCANÇADA! 🏆");
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
        else
        {
            Debug.LogWarning("Nem GameHUD nem winPanel foram encontrados!");
        }
        
        Debug.Log($"VITÓRIA! Pontuação final: {totalScore} pontos em {FormatTime(gameTime)}");
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
