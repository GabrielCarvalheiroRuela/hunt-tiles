using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ObstacleType
{
    Wall,
    Hole,
    Ice,
    Teleporter
}

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleType type;
    [SerializeField] private bool isActive = true;
    [SerializeField] private int tileX = 0;
    [SerializeField] private int tileY = 0;
    
    [Header("Visual Settings")]
    [SerializeField] private Color wallColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color holeColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color iceColor = new Color(0.7f, 0.9f, 1f, 0.8f);
    [SerializeField] private Color teleporterColor = new Color(1f, 0f, 1f, 0.8f);
    
    [Header("Teleporter Settings")]
    [SerializeField] private int teleportTargetX = -1;
    [SerializeField] private int teleportTargetY = -1;
    
    private RectTransform rectTransform;
    private Image obstacleImage;
    private int gridX, gridY;
    
    public ObstacleType Type => type;
    public bool IsActive => isActive;
    public int GridX => gridX;
    public int GridY => gridY;
    public int TileX => tileX;
    public int TileY => tileY;
    public Vector2Int TeleportTarget => new Vector2Int(teleportTargetX, teleportTargetY);
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();
            
        obstacleImage = GetComponent<Image>();
        if (obstacleImage == null)
            obstacleImage = gameObject.AddComponent<Image>();
    }
    
    void Start()
    {
        SetupObstacle();
    }
    
    public void Initialize(ObstacleType obstacleType, int x, int y)
    {
        type = obstacleType;
        gridX = x;
        gridY = y;
        tileX = x;
        tileY = y;
        SetupObstacle();
    }
    
    public void SetTeleportTarget(int x, int y)
    {
        teleportTargetX = x;
        teleportTargetY = y;
    }
    
    private void SetupObstacle()
    {
        switch (type)
        {
            case ObstacleType.Wall:
                obstacleImage.color = wallColor;
                obstacleImage.sprite = CreateWallSprite();
                rectTransform.sizeDelta = new Vector2(38f, 38f);
                break;
            case ObstacleType.Hole:
                obstacleImage.color = holeColor;
                obstacleImage.sprite = CreateHoleSprite();
                rectTransform.sizeDelta = new Vector2(35f, 35f);
                break;
            case ObstacleType.Ice:
                obstacleImage.color = iceColor;
                obstacleImage.sprite = CreateIceSprite();
                rectTransform.sizeDelta = new Vector2(38f, 38f);
                break;
            case ObstacleType.Teleporter:
                obstacleImage.color = teleporterColor;
                obstacleImage.sprite = CreateTeleporterSprite();
                rectTransform.sizeDelta = new Vector2(36f, 36f);
                break;
        }
        
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        AddVisualEffects();
        
        if (type == ObstacleType.Teleporter || type == ObstacleType.Ice)
        {
            StartCoroutine(AnimateObstacle());
        }
    }
    
    private void AddVisualEffects()
    {
        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        
        Shadow shadow = GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
        }
        
        switch (type)
        {
            case ObstacleType.Wall:
                outline.effectColor = new Color(0.2f, 0.1f, 0.05f, 1f);
                outline.effectDistance = new Vector2(2f, 2f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
                shadow.effectDistance = new Vector2(3f, -3f);
                break;
            case ObstacleType.Hole:
                outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                outline.effectDistance = new Vector2(1f, 1f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(2f, -2f);
                break;
            case ObstacleType.Ice:
                outline.effectColor = new Color(0.4f, 0.7f, 1f, 1f);
                outline.effectDistance = new Vector2(1f, 1f);
                shadow.effectColor = new Color(0.5f, 0.8f, 1f, 0.3f);
                shadow.effectDistance = new Vector2(2f, -2f);
                break;
            case ObstacleType.Teleporter:
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(2f, 2f);
                shadow.effectColor = new Color(1f, 0f, 1f, 0.4f);
                shadow.effectDistance = new Vector2(3f, -3f);
                break;
        }
    }
    
    public bool BlocksMovement()
    {
        return isActive && type == ObstacleType.Wall;
    }
    
    public bool CausesGameOver()
    {
        return isActive && type == ObstacleType.Hole;
    }
    
    public bool CausesSliding()
    {
        return isActive && type == ObstacleType.Ice;
    }
    
    public bool IsTeleporter()
    {
        return isActive && type == ObstacleType.Teleporter;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);
    }
    
    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }
    
    private IEnumerator AnimateObstacle()
    {
        Vector3 originalScale = rectTransform.localScale;
        float time = 0f;
        
        while (isActive)
        {
            time += Time.deltaTime;
            
            switch (type)
            {
                case ObstacleType.Teleporter:
                    float pulse = 1f + Mathf.Sin(time * 3f) * 0.2f;
                    rectTransform.localScale = originalScale * pulse;
                    rectTransform.Rotate(0f, 0f, 60f * Time.deltaTime);
                    
                    float colorLerp = (Mathf.Sin(time * 2f) + 1f) / 2f;
                    obstacleImage.color = Color.Lerp(teleporterColor, Color.magenta, colorLerp * 0.3f);
                    break;
                    
                case ObstacleType.Ice:
                    float shimmer = (Mathf.Sin(time * 1.5f) + 1f) / 2f;
                    obstacleImage.color = Color.Lerp(iceColor, Color.white, shimmer * 0.2f);
                    break;
            }
            
            yield return null;
        }
    }
    
    private Sprite CreateWallSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color stoneColor = new Color(0.4f, 0.35f, 0.3f, 1f);
        Color mortarColor = new Color(0.25f, 0.2f, 0.15f, 1f);
        Color lightColor = new Color(0.5f, 0.45f, 0.4f, 1f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Color pixelColor = stoneColor;
                
                bool isMortar = false;
                
                if (y % 8 == 0 || y % 8 == 7)
                    isMortar = true;
                
                int row = y / 8;
                int offset = (row % 2) * 4;
                if ((x + offset) % 16 == 0 || (x + offset) % 16 == 15)
                    isMortar = true;
                
                if (isMortar)
                {
                    pixelColor = mortarColor;
                }
                else
                {
                    if ((x + y) % 3 == 0)
                        pixelColor = lightColor;
                    else if ((x * 2 + y) % 5 == 0)
                        pixelColor = Color.Lerp(stoneColor, mortarColor, 0.3f);
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateHoleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxRadius = size / 2f - 1f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color pixelColor = Color.clear;
                
                if (distance <= maxRadius)
                {
                    float normalizedDistance = distance / maxRadius;
                    float alpha = 1f - normalizedDistance * 0.3f;
                    float brightness = 0.1f * (1f - normalizedDistance);
                    
                    pixelColor = new Color(brightness, brightness, brightness, alpha);
                    
                    if (distance > maxRadius * 0.8f)
                    {
                        pixelColor = new Color(0.05f, 0.05f, 0.05f, 1f);
                    }
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateIceSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color iceBlue = new Color(0.8f, 0.9f, 1f, 0.9f);
        Color iceDark = new Color(0.6f, 0.8f, 1f, 1f);
        Color iceLight = new Color(0.95f, 0.98f, 1f, 0.7f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Color pixelColor = iceBlue;
                
                if ((x + y) % 4 == 0)
                {
                    pixelColor = iceLight;
                }
                else if ((x - y) % 6 == 0)
                {
                    pixelColor = iceDark;
                }
                
                if (x == size / 4 || x == size * 3 / 4 || y == size / 3 || y == size * 2 / 3)
                {
                    pixelColor = Color.Lerp(pixelColor, iceLight, 0.5f);
                }
                
                float distanceFromEdge = Mathf.Min(x, y, size - 1 - x, size - 1 - y);
                if (distanceFromEdge < 2f)
                {
                    pixelColor.a *= distanceFromEdge / 2f;
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateTeleporterSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        Color magenta = new Color(1f, 0f, 1f, 0.8f);
        Color purple = new Color(0.6f, 0f, 0.8f, 1f);
        Color bright = new Color(1f, 0.5f, 1f, 0.9f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y) - center;
                float distance = pos.magnitude;
                float angle = Mathf.Atan2(pos.y, pos.x);
                
                Color pixelColor = Color.clear;
                
                float spiralDistance = distance - (angle * 2f);
                
                if (distance <= size / 2f - 1f && distance >= 3f)
                {
                    if (Mathf.Sin(spiralDistance * 0.8f) > 0.3f)
                    {
                        if (distance < size / 3f)
                            pixelColor = bright;
                        else
                            pixelColor = magenta;
                    }
                    else
                    {
                        pixelColor = purple;
                    }
                }
                
                if (distance <= 3f)
                {
                    pixelColor = Color.Lerp(bright, Color.white, 0.5f);
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
