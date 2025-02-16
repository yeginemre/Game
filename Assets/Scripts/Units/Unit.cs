using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.VFX;

public abstract class Unit : MonoBehaviour
{
    [Header("Unit Stats")]
    [SerializeField] protected int speed; // Number of tiles the unit can move
    [SerializeField] protected int attackRange;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected float maxHP;
    [SerializeField] protected float currentHP;
    [SerializeField] protected float moveSpeed;  // Move transition speed

    // =============References=============
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] protected AudioManager audioManager;
    [SerializeField] protected UIController uiController;

    // =============Flags=============
    private bool isSelected = false;
    private bool moveMode = false;
    private bool attackMode = false;
    private bool moveTransitioning = false;  // Different from moveMode (movement mode)

    // Properties
    private int remainingSpeed;
    private int rowIndex; // Indexes of the unit in the hex array
    private int columnIndex;
    private Color currentHighlightColor;

    // =============References=============
    [Header("References")]
    [SerializeField] protected MeshRenderer mainRenderer;
    [SerializeField] protected MeshRenderer uiRenderer;

    [SerializeField] protected GameObject hitEffectPrefab;

    private void Awake()
    {
        // Find and cache the renderers from children
        // Assuming the first child is the main part and second is the UI part
        mainRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        uiRenderer = transform.GetChild(1).GetComponent<MeshRenderer>();
        
        if (mainRenderer == null || uiRenderer == null)
        {
            Debug.LogError($"Missing renderer references on {gameObject.name}");
        }
    }

    // =============Initialize=============
    public void SetObjects(HexGrid grid, AudioManager manager, UIController ui)
    {
        hexGrid = grid;
        audioManager = manager;
        uiController = ui;
    }


    private void Update()
    {
        if (isSelected && !moveTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.M)){
                if (attackMode){
                    ResetTilesToBlack();
                }
                
                if (!moveMode)
                {
                    if (remainingSpeed == 0){
                        return;
                    }
                    currentHighlightColor = Color.blue;
                    moveMode = true;
                    hexGrid.SetHoverMode(true);
                    FindAdjacentHexes(currentHighlightColor, remainingSpeed, true);
                }
                else
                {
                    ResetTilesToBlack();
                }
            }
            if (Input.GetKeyDown(KeyCode.R)){
                if (moveMode){
                    ResetTilesToBlack();
                }
                if (!attackMode)
                {
                    currentHighlightColor = Color.red;
                    attackMode = true;
                    hexGrid.SetHoverMode(true);
                    FindAdjacentHexes(currentHighlightColor, attackRange, false);
                }
                else
                {

                    ResetTilesToBlack();
                }
            }            
  
        }
        if (attackMode && Input.GetMouseButtonDown(0)){
            GameObject targetHex = hexGrid.GetCurrentHoveredHex();
            if (targetHex != null)
            {
                if (hexGrid.GetLastHoveredHexColor() == currentHighlightColor || targetHex.GetComponent<SpriteRenderer>().color == currentHighlightColor)
                {
                    GameObject unitAtTile = hexGrid.GetUnitAtHex(targetHex);
                    if (unitAtTile != null)
                    {
                        StartCoroutine(ExecuteAttack(unitAtTile));
                    }
                }
            }
        }

        // Add new movement click handling
        if (moveMode && Input.GetMouseButtonDown(0))  // Left click during move mode
        {
            GameObject targetHex = hexGrid.GetCurrentHoveredHex();
            if (targetHex != null)
            {
                if (hexGrid.GetLastHoveredHexColor() == currentHighlightColor || targetHex.GetComponent<SpriteRenderer>().color == currentHighlightColor)
                {
                    
                    // Get the current hex this unit is on
                    GameObject currentHex = hexGrid.GetHexArray()[rowIndex][columnIndex];

                    // Update hex-to-unit mapping
                    hexGrid.RemoveUnitFromHex(currentHex);
                    hexGrid.AssignUnitToHex(gameObject, targetHex);

                    // Update unit's indices
                    var (newRow, newCol) = hexGrid.GetHexIndices(targetHex);
                    SetIndexes(newRow, newCol);
                    if (moveTransitioning) return;  // Prevent starting new movement while already moving
                    ResetTilesToBlack();

                    StartCoroutine(MoveSequence(targetHex));
                }
                else{
                    Debug.Log("invalid tile!");
                }
            }
        }
    }

    // =============Selection Functions=============
    public void Select()
    {
        remainingSpeed = speed;
        isSelected = true;
        uiController.setCurrentUnit(gameObject);
    }

    public void Deselect()
    {
        isSelected = false;
        ResetTilesToBlack();
    }



    // =============Hex Grid Functions=============
    private void FindAdjacentHexes(Color highlightColor, int range, bool ignoreUnits)
    {
        // First wave - find immediate adjacent hexes

        List<GameObject> markedHexes = FindAndHighlightAdjacent(rowIndex, columnIndex, null, highlightColor, ignoreUnits);
        
        // Continue for remaining movement range
        for (int i = 1; i < range; i++)
        {
            List<GameObject> newMarkedHexes = new List<GameObject>();
            
            // Check adjacents of each previously highlighted hex
            foreach (GameObject markedHex in markedHexes)
            {
                var (hexRow, hexCol) = hexGrid.GetHexIndices(markedHex);
                newMarkedHexes.AddRange(FindAndHighlightAdjacent(hexRow, hexCol, markedHex, highlightColor, ignoreUnits));
            }
            
            markedHexes = newMarkedHexes;
        }
    }

    private List<GameObject> FindAndHighlightAdjacent(int centerRow, int centerCol, GameObject prev, Color highlightColor, bool ignoreUnits)
    {
        List<GameObject> highlightedHexes = new List<GameObject>();
        int[,] upperOffsets = new int[,] {
            {-1, -1},  // Upper Left
            {-1, 0},   // Upper Right
            {1, 0},   // Lower Left
            {1, 1},  // lower Right
            {0, -1}, // Left
            {0, 1}   // Right
        };
        int[,] lowerOffsets = new int[,] {
            {-1, 0},  // Upper Left
            {-1, 1},   // Upper Right
            {1, -1}, // Lower Left
            {1, 0},   // Lower Right
            {0, -1}, // Left
            {0, 1}   // Right
        };
        
        // Check all six adjacent positions
        for (int i = 0; i < 6; i++)
        {
            int newRow = 0;
            int newCol = 0;
            if (centerRow < hexGrid.GetCenterRowIndex())
            {
                newRow = centerRow + upperOffsets[i, 0];
                newCol = centerCol + upperOffsets[i, 1];
            }
            else if (centerRow > hexGrid.GetCenterRowIndex())
            {
                newRow = centerRow + lowerOffsets[i, 0];
                newCol = centerCol + lowerOffsets[i, 1];
            } 
            else
            {
                if (i < 2)
                {
                    newRow = centerRow + upperOffsets[i, 0];
                    newCol = centerCol + upperOffsets[i, 1];
                }
                else
                {
                    newRow = centerRow + lowerOffsets[i, 0];
                    newCol = centerCol + lowerOffsets[i, 1];
                }
            }

            // Check if the new position is within bounds
            if (newRow >= 0 && newRow < hexGrid.GetHexArray().Length &&
                newCol >= 0 && newCol < hexGrid.GetHexArray()[newRow].Length)
            {
                GameObject targetHex = hexGrid.GetHexArray()[newRow][newCol];
                
                // Only highlight if hex is not occupied and not already highlighted
                if (targetHex.GetComponent<SpriteRenderer>().color != highlightColor)
                {
                    GameObject unitAtTile = hexGrid.GetUnitAtHex(targetHex);
                    if (unitAtTile == null){
                        hexGrid.HighlightTile(targetHex, highlightColor);
                        highlightedHexes.Add(targetHex);
                        targetHex.GetComponent<Hex>().SetPrev(prev);
                    }
                    else if (!ignoreUnits&& unitAtTile != gameObject){
                        hexGrid.HighlightTile(targetHex, highlightColor);
                        highlightedHexes.Add(targetHex);
                        targetHex.GetComponent<Hex>().SetPrev(prev);
                    }
                }
            }
        }
        
        return highlightedHexes;
    }

    protected void ResetTilesToBlack()
    {
        hexGrid.SetHoverMode(false);
        moveMode = false;
        attackMode = false;
        for (int row = 0; row < hexGrid.GetHexArray().Length; row++)
        {
            for (int col = 0; col < hexGrid.GetHexArray()[row].Length; col++)
            {
                hexGrid.HighlightTile(hexGrid.GetHexArray()[row][col], Color.black);
            }
        }
    }
    public void SetIndexes(int row, int column)
    {
        rowIndex = row;
        columnIndex = column;
    }
    
    
    
    // =============Movement Functions=============
    private IEnumerator MoveSequence(GameObject targetHex)
    {
        // First move to previous hex if it exists
        if (targetHex.GetComponent<Hex>().GetPrev() != null)
        {
            yield return StartCoroutine(MoveSequence(targetHex.GetComponent<Hex>().GetPrev()));
        }

        audioManager.PlaySFX(audioManager.slide);
        
        // Calculate target position
        Vector3 targetPosition = targetHex.transform.position;
        targetPosition.y = 0.4f; // Maintain the unit's height above the hex
        
        // Start smooth movement and wait for it to complete
        yield return StartCoroutine(SmoothMovement(targetPosition));
        
        // Reset movement mode and highlighting after the last movement
        moveTransitioning = false;
        remainingSpeed--;
        targetHex.GetComponent<Hex>().SetPrev(null);
    }

    private IEnumerator SmoothMovement(Vector3 targetPosition)
    {
        moveTransitioning = true;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            // Update the UI text position in sync with the unit movement
            uiController.UpdateTextPosition(transform.position);
            yield return null;
        }
        
        // Ensure we end up exactly at the target position
        transform.position = targetPosition;
        uiController.UpdateTextPosition(targetPosition);
    }
    
    
    
    // =============Getters and Setters=============
    public float getMaxHP(){
        return maxHP;
    }

    public float getCurrentHP(){
        return currentHP;
    }

    public void setCurrentHP(float hp){
        currentHP = hp;
    }

    public bool InMoveMode()
    {
        return moveMode;
    }

    public bool InAttackMode()
    {
        return attackMode;
    }

    public bool InMoveTransitioning(){
        return moveTransitioning;
    }

    public MeshRenderer GetMainRenderer(){
        return mainRenderer;
    }

    public (int, int) getArrayLocation(){
        return (rowIndex, columnIndex);
    }

    protected abstract IEnumerator ExecuteAttack(GameObject targetUnit);

    protected void SpawnHitEffect(GameObject targetUnit)
    {
        if (hitEffectPrefab != null)
        {
            // Calculate spawn position with height offset
            Vector3 spawnPosition = targetUnit.transform.position;
            spawnPosition.y += 1f; // Adjust this value to change the height offset
            
            // Spawn VFX at target position with offset
            GameObject hitEffect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
            
            // Get the Visual Effect component and set the direction
            VisualEffect vfx = hitEffect.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                // Calculate direction from this unit to the target
                Vector3 direction = (targetUnit.transform.position - transform.position).normalized;
                vfx.SetVector3("direction", direction);
            }
            
            Destroy(hitEffect, 2f); // Cleanup VFX after 2 seconds
        }
    }
} 