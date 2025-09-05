using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSetup : MonoBehaviour
{
    [Header("Character Setup")]
    [SerializeField] private int startX = 0;
    [SerializeField] private int startY = 0;
    
    private BoardCharacter spawnedCharacter;
    private CharacterController characterController;
    private GridBoard gridBoard;
    
    void Start()
    {
        SetupScene();
    }
    
    private void SetupScene()
    {
        CreateEventSystemIfNeeded();
        
        CreateCanvasIfNeeded();
        
        CreateGridBoardIfNeeded();
        
        CreateGameManagerIfNeeded();
        
        CreateHUDIfNeeded();
        
        CreateTutorialIfNeeded();
        
        CreateVisualEffectsIfNeeded();
        
        Invoke(nameof(SetupCharacter), 0.1f);
    }
    
    private void CreateEventSystemIfNeeded()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem criado!");
        }
    }
    
    private void CreateCanvasIfNeeded()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvasComponent = canvasGO.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("Canvas criado com configurações responsivas!");
        }
        else
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }
        }
    }
    
    private void CreateGridBoardIfNeeded()
    {
        gridBoard = FindObjectOfType<GridBoard>();
        if (gridBoard == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            GameObject gridBoardGO = new GameObject("GridBoard");
            gridBoardGO.transform.SetParent(canvas.transform, false);
            
            RectTransform rectTransform = gridBoardGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, -40f);
            rectTransform.sizeDelta = new Vector2(600, 600);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            gridBoard = gridBoardGO.AddComponent<GridBoard>();
            
            Debug.Log("GridBoard criado!");
        }
    }
    
    private void SetupCharacter()
    {
        CreateCharacter();
        SetupCharacterController();
    }
    
    private void CreateCharacter()
    {
        if (gridBoard == null)
        {
            gridBoard = FindObjectOfType<GridBoard>();
        }
        
        if (gridBoard == null)
        {
            Debug.LogError("GridBoard não encontrado!");
            return;
        }
        
        GameObject characterGO = new GameObject("Character");
        characterGO.transform.SetParent(gridBoard.transform, false);
        
        RectTransform rectTransform = characterGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(40, 40);
        
        Image characterImage = characterGO.AddComponent<Image>();
        characterImage.color = Color.red;
        
        spawnedCharacter = characterGO.AddComponent<BoardCharacter>();
        
        spawnedCharacter.SetInitialPosition(startX, startY);
        
        Debug.Log($"Personagem criado na posição ({startX}, {startY})!");
    }
    
    private void SetupCharacterController()
    {
        characterController = FindObjectOfType<CharacterController>();
        
        if (characterController == null)
        {
            GameObject controllerGO = new GameObject("Character Controller");
            characterController = controllerGO.AddComponent<CharacterController>();
        }
        
        if (characterController != null && spawnedCharacter != null && gridBoard != null)
        {
            Debug.Log("CharacterController configurado!");
        }
    }
    
    private void CreateGameManagerIfNeeded()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            GameObject gameManagerGO = new GameObject("Game Manager");
            gameManagerGO.AddComponent<GameManager>();
            Debug.Log("GameManager criado!");
        }
    }
    
    private void CreateHUDIfNeeded()
    {
        GameHUD gameHUD = FindObjectOfType<GameHUD>();
        if (gameHUD == null)
        {
            GameObject hudGO = new GameObject("Game HUD Manager");
            hudGO.AddComponent<GameHUD>();
            Debug.Log("GameHUD criado!");
        }
    }
    
    private void CreateTutorialIfNeeded()
    {
        TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager == null)
        {
            GameObject tutorialGO = new GameObject("Tutorial Manager");
            tutorialGO.AddComponent<TutorialManager>();
            Debug.Log("TutorialManager criado!");
        }
    }
    
    private void CreateVisualEffectsIfNeeded()
    {
        VisualEffectsManager effectsManager = FindObjectOfType<VisualEffectsManager>();
        if (effectsManager == null)
        {
            GameObject effectsGO = new GameObject("Visual Effects Manager");
            effectsGO.AddComponent<VisualEffectsManager>();
            Debug.Log("VisualEffectsManager criado!");
        }
    }
    
    public BoardCharacter GetCharacter()
    {
        return spawnedCharacter;
    }
    
    public CharacterController GetCharacterController()
    {
        return characterController;
    }
}
