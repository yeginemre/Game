using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject warriorPrefab;    // Reference to your unit prefab
    [SerializeField] private GameObject archerPrefab;    // Reference to your unit prefab
    [SerializeField] private HexGrid hexGrid;         // Reference to your HexGrid component
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIController uiController;
    
    private GameObject selectedUnit;
    private MeshRenderer selectedUnitRenderer;
    private Color originalColor; // Stores the original color of the selected unit


    public void SpawnUnitAtBottomRow(string unitType)
    {
        // Get the bottom row index (last row in the grid)
        int bottomRowIndex = hexGrid.GetHexArray().Length - 5;
        
        // Get the middle hex in the bottom row
        int middleColumnIndex = (hexGrid.GetHexArray()[bottomRowIndex].Length / 2);
        
        // Get the hex object
        GameObject targetHex = hexGrid.GetHexArray()[bottomRowIndex][middleColumnIndex];
        
        // Get the position of the chosen hex
        Vector3 spawnPosition = targetHex.transform.position;
        
        // Create rotation with X = -90 and Z = 180
        Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);
        
        GameObject spawnedUnit = null;

        switch (unitType){  
            case "Warrior":
                // Instantiate the unit at the hex position with specified rotation
                spawnedUnit = Instantiate(warriorPrefab, spawnPosition, spawnRotation);
                spawnedUnit.GetComponent<Warrior>()?.SetObjects(hexGrid, audioManager, uiController);  // Pass the reference

                break;
            case "Archer":
                // Instantiate the unit at the hex position with specified rotation
                spawnedUnit = Instantiate(archerPrefab, spawnPosition, spawnRotation);
                spawnedUnit.GetComponent<Archer>()?.SetObjects(hexGrid, audioManager, uiController);  // Pass the reference
                break;
        }
        
        if (spawnedUnit != null){
            uiController.mapUnitToText(spawnedUnit);
            hexGrid.AssignUnitToHex(spawnedUnit, targetHex);
            spawnedUnit.GetComponent<Unit>().SetIndexes(bottomRowIndex, middleColumnIndex);
        }
        
        // Register the unit with the hex grid
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            audioManager.PlaySFX(audioManager.spawnWarrior);
            SpawnUnitAtBottomRow("Warrior");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            audioManager.PlaySFX(audioManager.spawnArcher);
            SpawnUnitAtBottomRow("Archer");
        }

        if (Input.GetMouseButtonDown(0))  // Unit Selection Implementation
        {
            if (selectedUnit != null && (selectedUnit.GetComponent<Unit>().InMoveMode() || selectedUnit.GetComponent<Unit>().InAttackMode()))
            {
                return;
            }
            GameObject hoveredHex = hexGrid.GetLastHoveredHex();
            if (hoveredHex != null)
            {
                GameObject unitOnHex = hexGrid.GetUnitAtHex(hoveredHex);
                
                // If we click on a hex with no unit or a different hex
                if (unitOnHex != selectedUnit)
                {
                    if (selectedUnit != null)
                    {
                        selectedUnitRenderer.materials[0].color = originalColor;  // First material (commented)
                        //selectedUnitRenderer.materials[1].color = originalColor;    // Second material
                        selectedUnit.GetComponent<Unit>()?.Deselect();
                    }
                    
                    // If we clicked on a hex with a unit
                    if (unitOnHex != null)
                    {
                        selectedUnit = unitOnHex;
                        selectedUnitRenderer = selectedUnit.GetComponent<Unit>().GetMainRenderer();
                        if (selectedUnitRenderer != null)
                        {
                            originalColor = selectedUnitRenderer.materials[1].color;  // Store second material's color
                            selectedUnitRenderer.materials[0].color = Color.grey;   // First material (commented)
                            //selectedUnitRenderer.materials[1].color = Color.grey;     // Second material
                            selectedUnit.GetComponent<Unit>()?.Select();
                            audioManager.PlaySFX(audioManager.selectWarrior);
                            
                            // Get and set the unit's position indices
                            var (row, column) = hexGrid.GetHexIndices(hoveredHex);
                            selectedUnit.GetComponent<Unit>()?.SetIndexes(row, column);
                        }
                    }
                    else
                    {
                        // Clicked on empty hex, clear selection
                        selectedUnit = null;
                        selectedUnitRenderer = null;
                    }
                }
            }
        }
    }
} 