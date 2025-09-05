using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool useKeyboardInput = true;
    [SerializeField] private bool useMouseInput = true;
    
    [Header("References")]
    [SerializeField] private BoardCharacter character;
    
    void Start()
    {
        if (character == null)
        {
            character = FindObjectOfType<BoardCharacter>();
        }
        
        if (character == null)
        {
            Debug.LogError("CharacterController não encontrou nenhum BoardCharacter na cena!");
        }
    }
    
    void Update()
    {
        HandleKeyboardInput();
    }
    
    private void HandleKeyboardInput()
    {
        if (!useKeyboardInput || character == null || character.IsMoving) return;
        
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            character.MoveUp();
        }
        else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
        {
            character.MoveDown();
        }
        else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            character.MoveLeft();
        }
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            character.MoveRight();
        }
    }
    
    public void OnTileClicked(GridTileUI clickedTile)
    {
        if (!useMouseInput || character == null || character.IsMoving) return;
        
        int deltaX = Mathf.Abs(clickedTile.TileX - character.CurrentX);
        int deltaY = Mathf.Abs(clickedTile.TileY - character.CurrentY);
        
        if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
        {
            character.MoveToPosition(clickedTile.TileX, clickedTile.TileY);
        }
        else if (deltaX == 0 && deltaY == 0)
        {
            Debug.Log("Personagem já está nesta posição!");
        }
        else
        {
            Debug.Log($"Movimento muito distante! Distância: ({deltaX}, {deltaY})");
        }
    }
}
