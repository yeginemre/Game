using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private AudioManager audioManager;
    private MousePosition mousePosition;
    private GameObject currentHoveredHex;
    private GameObject lastHoveredHex;
    private Color lastHoveredHexColor;

    [Header("Grid Parameters")]
    [SerializeField] private int centerRowCount; // Must be odd number
    private int centerRowIndex;
    private const float HEX_SIZE = 7.5f;      // Distance between hex centers horizontally
    private const float Z_OFFSET = 5.625f;    // Vertical offset between rows
    private const float X_STEP = 3.75f;      // X offset for each row from center
    
    [Header("Data Structures")]
    private GameObject[][] hexArray; // Jagged array to store hexes
    private Dictionary<GameObject, GameObject> hexToUnitMap; // Maps hex objects to their units
    
    [Header("Flags")]
    private bool hoverMode = false;



    // Start is called before the first frame update
    void Start()
    {
        if (centerRowCount % 2 == 0)
        {
            Debug.LogError("Center row count must be odd!");
            return;
        }
        
        hexToUnitMap = new Dictionary<GameObject, GameObject>();
        mousePosition = FindFirstObjectByType<MousePosition>();
        centerRowIndex = centerRowCount / 2;
        CreateHexGrid();
    }

    // Hex Grid Generator Functions
    void CreateHexGrid()
    {
        hexArray = new GameObject[centerRowCount][];
        // Create center row first
        float centerX = -HEX_SIZE * centerRowIndex;
        CreateRow(centerRowCount, centerX, 0, centerRowIndex);

        // Create rows above and below center
        for (int i = 1; i <= centerRowIndex; i++)
        {
            int tilesInRow = centerRowCount - i;
            float currentX = centerX + (i * X_STEP);
            float currentZ = i * Z_OFFSET;
            
            // Create upper row
            CreateRow(tilesInRow, currentX, currentZ, centerRowIndex - i);
            
            // Create lower row
            CreateRow(tilesInRow, currentX, -currentZ, centerRowIndex + i);
        }
    }

    void CreateRow(int tileCount, float startX, float z, int rowIndex)
    {
        hexArray[rowIndex] = new GameObject[tileCount];
        
        for (int x = 0; x < tileCount; x++)
        {
            Vector3 position = new Vector3(startX + x * HEX_SIZE, 0, z);
            Quaternion rotation = Quaternion.Euler(90, 0, 0);
            
            GameObject hex = Instantiate(hexPrefab, position, rotation, transform);
            hex.name = $"Hex_{rowIndex}_{x}";
            hexArray[rowIndex][x] = hex;
        }
    }

    // Get Hex object using hex array
    public GameObject GetHexAt(int row, int col)
    {
        if (row >= 0 && row < hexArray.Length && col >= 0 && col < hexArray[row].Length)
        {
            return hexArray[row][col];
        }
        return null;
    }

    // Get Array position using hex object
    public (int row, int col) GetHexIndices(GameObject hex)
    {
        for (int row = 0; row < hexArray.Length; row++)
        {
            for (int col = 0; col < hexArray[row].Length; col++)
            {
                if (hexArray[row][col] == hex)
                {
                    return (row, col);
                }
            }
        }
        return (-1, -1); // Return invalid indices if hex not found
    }
    
    // Unit Detection
    public GameObject GetUnitAtHex(GameObject hex)
    {
        return hexToUnitMap.ContainsKey(hex) ? hexToUnitMap[hex] : null;
    }

    
    // Link and unlink units to hexes
    public void AssignUnitToHex(GameObject unit, GameObject hex)
    {
        hexToUnitMap[hex] = unit;
    }

    public void RemoveUnitFromHex(GameObject hex)
    {
        if (hexToUnitMap.ContainsKey(hex))
        {
            hexToUnitMap.Remove(hex);
        }
    }

    public void HighlightTile(GameObject hex, Color color)
    {
        if (hex != null)
        {
            SpriteRenderer spriteRenderer = hex.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                lastHoveredHexColor = spriteRenderer.color; // Store the previous color
                spriteRenderer.color = color;
                
                // Set sorting order based on color
                if (color == Color.white)
                {
                    spriteRenderer.sortingOrder = 3;  // Topmost layer
                }
                else if (color == Color.red)
                {
                    spriteRenderer.sortingOrder = 2;  // Middle layer
                }
                else if (color == Color.blue)
                {
                    spriteRenderer.sortingOrder = 1;  // Middle layer
                }
                else if (color == Color.black)
                {
                    spriteRenderer.sortingOrder = 0;  // Bottom layer
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mousePosition == null) return;

        Vector3 mousePos = mousePosition.currentMousePosition;
        currentHoveredHex = null;
        
        // First, find the approximate row based on Z position
        float approxRow = mousePos.z / Z_OFFSET;
        int targetRow = centerRowIndex - Mathf.RoundToInt(approxRow);
        
        // Clamp row to valid range
        targetRow = Mathf.Clamp(targetRow, 0, hexArray.Length - 1);
        
        // Find approximate column based on X position and row offset
        float rowXOffset = (hexArray[targetRow].Length < centerRowCount) ? X_STEP : 0;
        float approxCol = (mousePos.x + rowXOffset) / HEX_SIZE;
        int targetCol = Mathf.RoundToInt(approxCol + (hexArray[targetRow].Length / 2));
        
        // Clamp column to valid range for this row
        targetCol = Mathf.Clamp(targetCol, 0, hexArray[targetRow].Length - 1);
        
        // Check the target hex and its immediate neighbors
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            int checkRow = targetRow + rowOffset;
            if (checkRow < 0 || checkRow >= hexArray.Length) continue;
            
            for (int colOffset = -1; colOffset <= 1; colOffset++)
            {
                int checkCol = targetCol + colOffset;
                if (checkCol < 0 || checkCol >= hexArray[checkRow].Length) continue;
                
                GameObject hex = hexArray[checkRow][checkCol];
                float dist = Vector3.Distance(mousePos, hex.transform.position);
                
                if (dist < HEX_SIZE/2)
                {
                    currentHoveredHex = hex;
                    break;
                }
            }
        }

        // Only trigger color changes when hovering over a new hex
        if (currentHoveredHex != lastHoveredHex)
        {
            // Reset previous hex to its original color, if it was hovered recently(meaning its white)
            if (lastHoveredHex != null && lastHoveredHex.GetComponent<SpriteRenderer>().color == Color.white)
            {
                HighlightTile(lastHoveredHex, lastHoveredHexColor);
            }

            // Highlight new hex if there is one
            if (currentHoveredHex != null)
            {
                if (hoverMode)
                {
                    audioManager.PlaySFX(audioManager.hover);
                    GameObject unitOnHex = GetUnitAtHex(currentHoveredHex);

                    HighlightTile(currentHoveredHex, Color.white);

                }
            }
            
            lastHoveredHex = currentHoveredHex;
        }
    }

    // Getters
    public GameObject[][] GetHexArray()
    {
        return hexArray;
    }

    public GameObject GetLastHoveredHex()
    {
        return lastHoveredHex;
    }

    public int GetCenterRowIndex()
    {
        return centerRowIndex;
    }

    public GameObject GetCurrentHoveredHex()
    {
        return currentHoveredHex;
    }


    public Color GetLastHoveredHexColor()
    {
        return lastHoveredHexColor;
    }

    // Setters
    public void SetHoverMode(bool mode)
    {
        hoverMode = mode;
    }
}
