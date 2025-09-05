using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CollectibleType
{
    Coin,
    Gem,
    PowerUp,
    Key
}

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private CollectibleType type;
    [SerializeField] private int value = 10;
    [SerializeField] private bool isCollected = false;
    [SerializeField] private int tileX = 0;
    [SerializeField] private int tileY = 0;
    
    [Header("Visual Settings")]
    [SerializeField] private Color coinColor = new Color(1f, 0.8f, 0f, 1f);      // Dourado
    [SerializeField] private Color gemColor = new Color(0.5f, 0f, 1f, 1f);       // Roxo
    [SerializeField] private Color powerUpColor = new Color(0f, 1f, 0.5f, 1f);   // Verde claro
    [SerializeField] private Color keyColor = new Color(0.8f, 0.8f, 0.8f, 1f);   // Prata
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    [SerializeField] private float rotationSpeed = 50f;
    
    private RectTransform rectTransform;
    private Image itemImage;
    private Vector3 originalScale;
    private int gridX, gridY;
    
    public CollectibleType Type => type;
    public int Value => value;
    public bool IsCollected => isCollected;
    public int GridX => gridX;
    public int GridY => gridY;
    public int TileX => tileX;
    public int TileY => tileY;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();
            
        itemImage = GetComponent<Image>();
        if (itemImage == null)
            itemImage = gameObject.AddComponent<Image>();
    }
    
    void Start()
    {
        SetupCollectible();
        originalScale = rectTransform.localScale;
        StartCoroutine(AnimateCollectible());
    }
    
    public void Initialize(CollectibleType collectibleType, int x, int y)
    {
        type = collectibleType;
        gridX = x;
        gridY = y;
        tileX = x;
        tileY = y;
        SetupCollectible();
    }
    
    private void SetupCollectible()
    {
        switch (type)
        {
            case CollectibleType.Coin:
                itemImage.color = coinColor;
                itemImage.sprite = CreateCoinSprite();
                value = 10;
                rectTransform.sizeDelta = new Vector2(22f, 22f);
                break;
            case CollectibleType.Gem:
                itemImage.color = gemColor;
                itemImage.sprite = CreateGemSprite();
                value = 50;
                rectTransform.sizeDelta = new Vector2(28f, 28f);
                break;
            case CollectibleType.PowerUp:
                itemImage.color = powerUpColor;
                itemImage.sprite = CreatePowerUpSprite();
                value = 0;
                rectTransform.sizeDelta = new Vector2(32f, 32f);
                break;
            case CollectibleType.Key:
                itemImage.color = keyColor;
                itemImage.sprite = CreateKeySprite();
                value = 100;
                rectTransform.sizeDelta = new Vector2(24f, 24f);
                break;
        }
        
        AddVisualEffects();
        
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }
    
    private IEnumerator AnimateCollectible()
    {
        float time = 0f;
        
        while (!isCollected)
        {
            time += Time.deltaTime;
            
            float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseIntensity;
            rectTransform.localScale = originalScale * pulse;
            
            if (type == CollectibleType.Gem || type == CollectibleType.PowerUp)
            {
                rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
            
            yield return null;
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        VisualEffectsManager effectsManager = VisualEffectsManager.Instance;
        if (effectsManager != null)
        {
            effectsManager.ShowCollectionEffect(transform.position, type);
            effectsManager.ShowScorePopup(transform.position, value, GetScoreColor());
        }
        
        StartCoroutine(CollectionAnimation());
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCollectibleCollected(this);
        }
    }
    
    private Color GetScoreColor()
    {
        switch (type)
        {
            case CollectibleType.Coin: return Color.yellow;
            case CollectibleType.Gem: return Color.magenta;
            case CollectibleType.Key: return Color.cyan;
            case CollectibleType.PowerUp: return Color.green;
            default: return Color.white;
        }
    }
    
    private IEnumerator CollectionAnimation()
    {
        float animationTime = 0.5f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = startScale * 1.5f;
        Color startColor = itemImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationTime)
        {
            float progress = elapsedTime / animationTime;
            
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            
            itemImage.color = Color.Lerp(startColor, targetColor, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
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
            case CollectibleType.Coin:
                outline.effectColor = new Color(0.8f, 0.5f, 0f, 1f);
                outline.effectDistance = new Vector2(1f, 1f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
                shadow.effectDistance = new Vector2(2f, -2f);
                break;
            case CollectibleType.Gem:
                outline.effectColor = new Color(0.3f, 0f, 0.6f, 1f);
                outline.effectDistance = new Vector2(1.5f, 1.5f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
                shadow.effectDistance = new Vector2(3f, -3f);
                break;
            case CollectibleType.PowerUp:
                outline.effectColor = new Color(0f, 0.8f, 0.3f, 1f);
                outline.effectDistance = new Vector2(2f, 2f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
                shadow.effectDistance = new Vector2(3f, -3f);
                break;
            case CollectibleType.Key:
                outline.effectColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                outline.effectDistance = new Vector2(1f, 1f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
                shadow.effectDistance = new Vector2(2f, -2f);
                break;
        }
    }
    
    private Sprite CreateCoinSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size / 2f - 2f;
        float innerRadius = outerRadius * 0.7f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color pixelColor = Color.clear;
                
                if (distance <= outerRadius)
                {
                    if (distance > innerRadius)
                    {
                        pixelColor = new Color(0.9f, 0.7f, 0.1f, 1f);
                    }
                    else
                    {
                        pixelColor = new Color(1f, 0.85f, 0.3f, 1f);
                    }
                    
                    float centerDist = Vector2.Distance(new Vector2(x, y), center + new Vector2(-3, 3));
                    if (centerDist < 4f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.4f);
                    }
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateGemSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y) - center;
                Color pixelColor = Color.clear;
                
                if (Mathf.Abs(pos.x) + Mathf.Abs(pos.y) <= 12f)
                {
                    float distance = Vector2.Distance(Vector2.zero, pos);
                    
                    if (distance < 8f)
                    {
                        pixelColor = new Color(0.7f, 0.2f, 1f, 1f);
                    }
                    else
                    {
                        pixelColor = new Color(0.4f, 0f, 0.8f, 1f);
                    }
                    
                    if (x > center.x - 3 && x < center.x + 1 && y > center.y && y < center.y + 4)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.6f);
                    }
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreatePowerUpSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y) - center;
                Color pixelColor = Color.clear;
                
                bool inStar = false;
                
                if (Mathf.Abs(pos.y) <= 3f && Mathf.Abs(pos.x) <= 11f)
                    inStar = true;
                
                if (Mathf.Abs(pos.x) <= 3f && Mathf.Abs(pos.y) <= 11f)
                    inStar = true;
                
                if (inStar)
                {
                    float distance = Vector2.Distance(Vector2.zero, pos);
                    
                    if (distance < 6f)
                    {
                        pixelColor = new Color(0.4f, 1f, 0.6f, 1f);
                    }
                    else
                    {
                        pixelColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                    }
                    
                    if (distance < 3f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.5f);
                    }
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateKeySprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color keyColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        Color darkColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Color pixelColor = Color.clear;
                
                Vector2 headCenter = new Vector2(size / 2f, size * 0.75f);
                float headDistance = Vector2.Distance(new Vector2(x, y), headCenter);
                
                if (headDistance <= 5f && headDistance >= 3f)
                {
                    pixelColor = keyColor;
                }
                
                if (x >= size / 2f - 1 && x <= size / 2f + 1 && y >= size * 0.25f && y <= size * 0.7f)
                {
                    pixelColor = keyColor;
                }
                
                if (y >= size * 0.25f && y <= size * 0.35f)
                {
                    if ((x >= size / 2f + 2 && x <= size / 2f + 4) || 
                        (x >= size / 2f + 2 && x <= size / 2f + 6 && y >= size * 0.3f))
                    {
                        pixelColor = keyColor;
                    }
                }
                
                if (pixelColor != Color.clear && (x == size / 2f + 1 || y == size * 0.25f))
                {
                    pixelColor = darkColor;
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
