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
            
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("Canvas criado!");
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
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(800, 800);
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
    
    public BoardCharacter GetCharacter()
    {
        return spawnedCharacter;
    }
    
    public CharacterController GetCharacterController()
    {
        return characterController;
    }
}
