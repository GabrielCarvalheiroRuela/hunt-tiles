using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoardCharacter : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private int currentX = 0;
    [SerializeField] private int currentY = 0;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Visual Settings")]
    [SerializeField] private Color characterColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    [SerializeField] private float characterSize = 30f;
    
    private RectTransform characterTransform;
    private Image characterImage;
    private bool isMoving = false;
    private GridBoard gridBoard;
    
    public int CurrentX => currentX;
    public int CurrentY => currentY;
    public bool IsMoving => isMoving;
    
    public void SetInitialPosition(int x, int y)
    {
        currentX = x;
        currentY = y;
    }
    
    void Awake()
    {
        SetupCharacterVisual();
    }
    
    void Start()
    {
        gridBoard = GridBoard.Instance;
        if (gridBoard != null)
        {
            PlaceAtPosition(currentX, currentY);
            MarkCurrentTileAsOccupied(true);
        }
    }
    
    private void SetupCharacterVisual()
    {
        // Configurar RectTransform
        characterTransform = GetComponent<RectTransform>();
        if (characterTransform == null)
        {
            characterTransform = gameObject.AddComponent<RectTransform>();
        }
        
        characterTransform.anchorMin = Vector2.one * 0.5f;
        characterTransform.anchorMax = Vector2.one * 0.5f;
        characterTransform.sizeDelta = Vector2.one * characterSize;
        
        characterImage = GetComponent<Image>();
        if (characterImage == null)
        {
            characterImage = gameObject.AddComponent<Image>();
        }
        
        characterImage.color = characterColor;
        characterImage.sprite = CreateCircleSprite();
        
        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2f, 2f);
        
        Shadow shadow = GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    public bool CanMoveTo(int x, int y)
    {
        if (gridBoard == null) return false;
        
        GridTileUI targetTile = gridBoard.GetTile(x, y);
        return targetTile != null && targetTile.IsWalkable;
    }
    
    public void MoveToPosition(int x, int y)
    {
        if (isMoving || !CanMoveTo(x, y)) return;
        
        StartCoroutine(MoveToPositionCoroutine(x, y));
    }
    
    private IEnumerator MoveToPositionCoroutine(int targetX, int targetY)
    {
        isMoving = true;
        
        MarkCurrentTileAsOccupied(false);
        
        Vector2 startPos = characterTransform.anchoredPosition;
        Vector2 targetPos = GetTilePosition(targetX, targetY);
        
        float elapsed = 0f;
        float duration = 1f / moveSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = moveCurve.Evaluate(t);
            
            characterTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            yield return null;
        }
        
        characterTransform.anchoredPosition = targetPos;
        
        currentX = targetX;
        currentY = targetY;
        
        MarkCurrentTileAsOccupied(true);
        
        isMoving = false;
    }
    
    private void PlaceAtPosition(int x, int y)
    {
        currentX = x;
        currentY = y;
        characterTransform.anchoredPosition = GetTilePosition(x, y);
    }
    
    private Vector2 GetTilePosition(int x, int y)
    {
        if (gridBoard == null) return Vector2.zero;
        
        GridTileUI tile = gridBoard.GetTile(x, y);
        if (tile != null)
        {
            RectTransform tileRect = tile.GetComponent<RectTransform>();
            return tileRect.anchoredPosition;
        }
        
        return Vector2.zero;
    }
    
    private void MarkCurrentTileAsOccupied(bool occupied)
    {
        if (gridBoard == null) return;
        
        GridTileUI currentTile = gridBoard.GetTile(currentX, currentY);
        if (currentTile != null)
        {
            currentTile.SetOccupied(occupied);
        }
    }
    
    public void MoveUp()
    {
        MoveToPosition(currentX, currentY - 1);
    }
    
    public void MoveDown()
    {
        MoveToPosition(currentX, currentY + 1);
    }
    
    public void MoveLeft()
    {
        MoveToPosition(currentX - 1, currentY);
    }
    
    public void MoveRight()
    {
        MoveToPosition(currentX + 1, currentY);
    }
}
