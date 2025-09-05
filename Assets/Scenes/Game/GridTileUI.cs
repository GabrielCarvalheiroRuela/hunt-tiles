using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridTileUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Tile Properties")]
    [SerializeField] private int tileX;
    [SerializeField] private int tileY;
    [SerializeField] private bool isWalkable = true;
    [SerializeField] private bool isOccupied = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color occupiedColor = Color.red;
    
    private Image tileImage;
    private Color originalColor;
    private bool isHighlighted = false;
    
    public int TileX => tileX;
    public int TileY => tileY;
    public bool IsWalkable => isWalkable && !isOccupied;
    public bool IsOccupied => isOccupied;
    
    public void SetCustomColors(Color normal, Color highlight, Color occupied)
    {
        normalColor = normal;
        highlightColor = highlight;
        occupiedColor = occupied;
        originalColor = normal;
    }
    
    void Awake()
    {
        tileImage = GetComponent<Image>();
        if (tileImage == null)
        {
            Debug.LogError($"GridTileUI at ({tileX}, {tileY}) não possui Image component!");
        }
    }
    
    void Start()
    {
        if (tileImage != null)
        {
            originalColor = tileImage.color;
            normalColor = originalColor;
        }
        UpdateVisual();
    }
    
    public void Initialize(int x, int y)
    {
        tileX = x;
        tileY = y;
        gameObject.name = $"Tile_{x}_{y}";
        
        if (tileImage != null)
        {
            originalColor = tileImage.color;
            normalColor = originalColor;
        }
        
        UpdateVisual();
    }
    
    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        UpdateVisual();
    }
    
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        UpdateVisual();
    }
    
    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        if (tileImage == null) return;
        
        Color targetColor;
        
        if (isHighlighted)
        {
            targetColor = highlightColor;
        }
        else if (isOccupied)
        {
            targetColor = occupiedColor;
        }
        else if (!isWalkable)
        {
            targetColor = Color.gray;
        }
        else
        {
            targetColor = normalColor;
        }
        
        tileImage.color = targetColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOccupied)
        {
            SetHighlight(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsWalkable)
        {
            GridBoard.Instance?.OnTileClicked(this);
        }
    }
}
