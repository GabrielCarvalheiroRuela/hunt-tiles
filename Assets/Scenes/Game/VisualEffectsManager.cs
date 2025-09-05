using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualEffectsManager : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private GameObject popupTextPrefab;
    [SerializeField] private float popupDuration = 1.5f;
    [SerializeField] private float popupMoveDistance = 50f;
    
    public static VisualEffectsManager Instance { get; private set; }
    
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
        SetupEffectSystem();
    }
    
    private void SetupEffectSystem()
    {
        if (popupTextPrefab == null)
        {
            CreatePopupTextPrefab();
        }
    }
    
    private void CreatePopupTextPrefab()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject prefab = new GameObject("Popup Text Prefab");
        prefab.transform.SetParent(canvas.transform, false);
        prefab.SetActive(false);
        
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200f, 50f);
        
        Text text = prefab.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        
        Outline outline = prefab.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, 2f);
        
        popupTextPrefab = prefab;
    }
    
    public void ShowScorePopup(Vector3 worldPosition, int score, Color color)
    {
        if (popupTextPrefab == null) return;
        
        GameObject popup = Instantiate(popupTextPrefab);
        popup.SetActive(true);
        popup.name = "Score Popup";
        
        Text popupText = popup.GetComponent<Text>();
        popupText.text = $"+{score}";
        popupText.color = color;
        
        RectTransform popupTransform = popup.GetComponent<RectTransform>();
        
        Canvas canvas = FindObjectOfType<Canvas>();
        Camera camera = Camera.main;
        
        if (canvas != null && camera != null)
        {
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                screenPosition, 
                canvas.worldCamera, 
                out canvasPosition
            );
            
            popupTransform.anchoredPosition = canvasPosition;
        }
        
        StartCoroutine(AnimateScorePopup(popup));
    }
    
    private IEnumerator AnimateScorePopup(GameObject popup)
    {
        if (popup == null) yield break;
        
        RectTransform transform = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        
        Vector2 startPos = transform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * popupMoveDistance;
        
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < popupDuration)
        {
            float progress = elapsedTime / popupDuration;
            
            transform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            
            text.color = Color.Lerp(startColor, endColor, progress);
            
            float scale = 1f + (progress * 0.5f);
            transform.localScale = Vector3.one * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(popup);
    }
    
    public void ShowPowerUpEffect(Vector3 worldPosition, string powerUpName)
    {
        if (popupTextPrefab == null) return;
        
        GameObject popup = Instantiate(popupTextPrefab);
        popup.SetActive(true);
        popup.name = "PowerUp Popup";
        
        Text popupText = popup.GetComponent<Text>();
        popupText.text = powerUpName;
        popupText.color = Color.yellow;
        popupText.fontSize = 24;
        
        RectTransform popupTransform = popup.GetComponent<RectTransform>();
        popupTransform.sizeDelta = new Vector2(300f, 60f);
        
        Canvas canvas = FindObjectOfType<Canvas>();
        Camera camera = Camera.main;
        
        if (canvas != null && camera != null)
        {
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                screenPosition, 
                canvas.worldCamera, 
                out canvasPosition
            );
            
            popupTransform.anchoredPosition = canvasPosition;
        }
        
        StartCoroutine(AnimatePowerUpPopup(popup));
    }
    
    private IEnumerator AnimatePowerUpPopup(GameObject popup)
    {
        if (popup == null) yield break;
        
        RectTransform transform = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        
        Vector2 startPos = transform.anchoredPosition;
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float duration = 2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            
            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 6f) * 0.2f;
            transform.localScale = Vector3.one * pulse;
            
            if (progress > 0.7f)
            {
                float fadeProgress = (progress - 0.7f) / 0.3f;
                text.color = Color.Lerp(startColor, endColor, fadeProgress);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(popup);
    }
    
    public void ShowDamageEffect(Vector3 worldPosition)
    {
        if (popupTextPrefab == null) return;
        
        GameObject popup = Instantiate(popupTextPrefab);
        popup.SetActive(true);
        popup.name = "Damage Popup";
        
        Text popupText = popup.GetComponent<Text>();
        popupText.text = "GAME OVER!";
        popupText.color = Color.red;
        popupText.fontSize = 28;
        
        RectTransform popupTransform = popup.GetComponent<RectTransform>();
        popupTransform.sizeDelta = new Vector2(250f, 70f);
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            popupTransform.anchoredPosition = Vector2.zero;
        }
        
        StartCoroutine(AnimateDamagePopup(popup));
    }
    
    private IEnumerator AnimateDamagePopup(GameObject popup)
    {
        if (popup == null) yield break;
        
        RectTransform transform = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        
        Vector2 originalPos = transform.anchoredPosition;
        float shakeDuration = 1f;
        float shakeIntensity = 10f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < shakeDuration)
        {
            float progress = elapsedTime / shakeDuration;
            
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity * (1f - progress);
            transform.anchoredPosition = originalPos + randomOffset;
            
            float scale = 1f + progress * 0.5f;
            transform.localScale = Vector3.one * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float fadeTime = 0.5f;
        elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            float progress = elapsedTime / fadeTime;
            text.color = Color.Lerp(startColor, endColor, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(popup);
    }
    
    public void ShowCollectionEffect(Vector3 worldPosition, CollectibleType type)
    {
        string effectText = "";
        Color effectColor = Color.white;
        
        switch (type)
        {
            case CollectibleType.Coin:
                effectText = "🪙";
                effectColor = Color.yellow;
                break;
            case CollectibleType.Gem:
                effectText = "💎";
                effectColor = Color.magenta;
                break;
            case CollectibleType.Key:
                effectText = "🗝️";
                effectColor = Color.cyan;
                break;
            case CollectibleType.PowerUp:
                effectText = "⚡";
                effectColor = Color.green;
                break;
        }
        
        if (popupTextPrefab == null) return;
        
        GameObject popup = Instantiate(popupTextPrefab);
        popup.SetActive(true);
        popup.name = "Collection Effect";
        
        Text popupText = popup.GetComponent<Text>();
        popupText.text = effectText;
        popupText.color = effectColor;
        popupText.fontSize = 32;
        
        RectTransform popupTransform = popup.GetComponent<RectTransform>();
        popupTransform.sizeDelta = new Vector2(80f, 80f);
        
        Canvas canvas = FindObjectOfType<Canvas>();
        Camera camera = Camera.main;
        
        if (canvas != null && camera != null)
        {
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                screenPosition, 
                canvas.worldCamera, 
                out canvasPosition
            );
            
            popupTransform.anchoredPosition = canvasPosition;
        }
        
        StartCoroutine(AnimateCollectionEffect(popup));
    }
    
    private IEnumerator AnimateCollectionEffect(GameObject popup)
    {
        if (popup == null) yield break;
        
        RectTransform transform = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        
        Vector2 startPos = transform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * 30f;
        
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            
            transform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.5f;
            transform.localScale = Vector3.one * scale;
            
            if (progress > 0.6f)
            {
                float fadeProgress = (progress - 0.6f) / 0.4f;
                text.color = Color.Lerp(startColor, endColor, fadeProgress);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(popup);
    }
}
