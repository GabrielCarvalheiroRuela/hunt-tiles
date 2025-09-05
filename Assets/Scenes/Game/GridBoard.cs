using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridBoard : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 40f;
    [SerializeField] private float tileSpacing = 1f;
    
    [Header("UI References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject tilePrefab;
    
    [Header("Visual Settings")]
    [SerializeField] private Color primaryColor = new Color(0.8f, 0.6f, 0.4f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.6f, 0.4f, 0.2f, 1f);
    [SerializeField] private Color borderColor = new Color(0.4f, 0.2f, 0.1f, 1f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0.4f, 1f);
    
    [Header("Layout Settings")]
    [SerializeField] private float topMargin = 80f;
    [SerializeField] private float bottomMargin = 80f;
    
    private GridTileUI[,] tiles;
    private RectTransform rectTransform;
    
    public static GridBoard Instance { get; private set; }
    
    public int Width => gridWidth;
    public int Height => gridHeight;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            rectTransform = GetComponent<RectTransform>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        CalculateOptimalSize();
        CreateUIGrid();
    }
    
    private void CalculateOptimalSize()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        float screenWidth = canvasRect.rect.width;
        float screenHeight = canvasRect.rect.height;
        
        float availableWidth = screenWidth * 0.8f;
        float availableHeight = screenHeight * 0.8f;
        
        float maxTileWidth = (availableWidth - (gridWidth - 1) * tileSpacing) / gridWidth;
        
        float maxTileHeight = (availableHeight - (gridHeight - 1) * tileSpacing) / gridHeight;
        
        tileSize = Mathf.Min(maxTileWidth, maxTileHeight);
        
        tileSize = Mathf.Clamp(tileSize, 25f, 60f);
        
        Debug.Log($"Tamanho da tela: {screenWidth}x{screenHeight}, Tile size calculado: {tileSize}");
    }
    
    private void CreateUIGrid()
    {
        if (gridParent == null)
            gridParent = transform;
        
        tiles = new GridTileUI[gridWidth, gridHeight];
        
        float totalWidth = gridWidth * tileSize + (gridWidth - 1) * tileSpacing;
        float totalHeight = gridHeight * tileSize + (gridHeight - 1) * tileSpacing;
        
        CreateBoardBackground(totalWidth, totalHeight);
        
        float startX = -totalWidth / 2f + tileSize / 2f;
        float startY = totalHeight / 2f - tileSize / 2f;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 position = new Vector2(
                    startX + x * (tileSize + tileSpacing),
                    startY - y * (tileSize + tileSpacing)
                );
                
                CreateUITile(x, y, position);
            }
        }
        
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(totalWidth + 40f, totalHeight + 40f);
        }
    }
    
    private void CreateBoardBackground(float totalWidth, float totalHeight)
    {
        GameObject backgroundObj = new GameObject("BoardBackground");
        backgroundObj.transform.SetParent(gridParent);
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.3f, 0.2f, 0.1f, 0.8f);
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.one * 0.5f;
        bgRect.anchorMax = Vector2.one * 0.5f;
        
        float paddingWidth = 20f;
        float paddingHeight = 20f;
        
        bgRect.sizeDelta = new Vector2(totalWidth + paddingWidth, totalHeight + paddingHeight);
        bgRect.anchoredPosition = Vector2.zero;
        
        Outline bgOutline = backgroundObj.AddComponent<Outline>();
        bgOutline.effectColor = borderColor;
        bgOutline.effectDistance = new Vector2(3f, 3f);
        
        backgroundObj.transform.SetAsFirstSibling();
    }
    
    private void CreateUITile(int x, int y, Vector2 position)
    {
        GameObject tileObject;
        
        if (tilePrefab != null)
        {
            tileObject = Instantiate(tilePrefab, gridParent);
        }
        else
        {
            tileObject = new GameObject($"Tile_{x}_{y}");
            tileObject.transform.SetParent(gridParent);
            
            Image image = tileObject.AddComponent<Image>();
            Color tileColor = ((x + y) % 2 == 0) ? primaryColor : secondaryColor;
            image.color = tileColor;
            
            Outline outline = tileObject.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1f, 1f);
            
            Shadow shadow = tileObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
        
        RectTransform tileRect = tileObject.GetComponent<RectTransform>();
        tileRect.anchorMin = Vector2.one * 0.5f;
        tileRect.anchorMax = Vector2.one * 0.5f;
        tileRect.sizeDelta = Vector2.one * tileSize;
        tileRect.anchoredPosition = position;
        
        GridTileUI tileComponent = tileObject.GetComponent<GridTileUI>();
        if (tileComponent == null)
        {
            tileComponent = tileObject.AddComponent<GridTileUI>();
        }
        
        tileComponent.SetCustomColors(
            ((x + y) % 2 == 0) ? primaryColor : secondaryColor,
            highlightColor,
            new Color(0.8f, 0.2f, 0.2f, 1f)
        );
        
        tileComponent.Initialize(x, y);
        
        if (tilePrefab == null)
        {
            Image image = tileObject.GetComponent<Image>();
            Color tileColor = ((x + y) % 2 == 0) ? primaryColor : secondaryColor;
            image.color = tileColor;
        }
        
        tiles[x, y] = tileComponent;
    }
    
    public GridTileUI GetTile(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return tiles[x, y];
        }
        return null;
    }
    
    public GridTileUI GetTileAtScreenPosition(Vector2 screenPosition)
    {
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null, out localPosition);
        
        float totalWidth = gridWidth * tileSize + (gridWidth - 1) * tileSpacing;
        float totalHeight = gridHeight * tileSize + (gridHeight - 1) * tileSpacing;
        
        float startX = -totalWidth / 2f;
        float startY = totalHeight / 2f;
        
        int x = Mathf.FloorToInt((localPosition.x - startX) / (tileSize + tileSpacing));
        int y = Mathf.FloorToInt((startY - localPosition.y) / (tileSize + tileSpacing));
        
        return GetTile(x, y);
    }
    
    public List<GridTileUI> GetNeighbors(int x, int y, bool includeDiagonals = false)
    {
        List<GridTileUI> neighbors = new List<GridTileUI>();
        
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };
        
        if (includeDiagonals)
        {
            dx = new int[] { 0, 0, -1, 1, -1, -1, 1, 1 };
            dy = new int[] { -1, 1, 0, 0, -1, 1, -1, 1 };
        }
        
        for (int i = 0; i < dx.Length; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            
            GridTileUI neighbor = GetTile(newX, newY);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    public void OnTileClicked(GridTileUI tile)
    {
        Debug.Log($"Tile clicado: ({tile.TileX}, {tile.TileY})");
        
        CharacterController characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
        {
            characterController.OnTileClicked(tile);
        }
    }
    
    public void ClearHighlights()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].SetHighlight(false);
                }
            }
        }
    }
    
    public void HighlightTiles(List<GridTileUI> tilesToHighlight)
    {
        ClearHighlights();
        
        foreach (GridTileUI tile in tilesToHighlight)
        {
            if (tile != null)
            {
                tile.SetHighlight(true);
            }
        }
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
