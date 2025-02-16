using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{

    [Header("HP Parameters")]
    private float maxHP;
    private float currentHP;
    private float targetMaxHP;
    private float targetCurrentHP;

    [Header("UI Parameters")]
    [SerializeField] private float smoothDecreaseDuration = 0.5f;
    [SerializeField] private GameObject hpTextPrefab;
    [SerializeField] private Canvas targetCanvas; // Reference to existing canvas in scene
    [SerializeField] private Vector3 offset;
    private Dictionary<GameObject, TMP_Text> unitTextMap = new Dictionary<GameObject, TMP_Text>();
    private GameObject currentUnit;
    private GameObject targetUnit;
    private TMP_Text currentText;
    private TMP_Text targetText;

    [Header("Colors")]
    [SerializeField] private Color HighHPColor;
    [SerializeField] private Color MidHPColor;
    [SerializeField] private Color LowHPColor;

    public void dealDamage(float damageAmount){
        StartCoroutine(SmoothDecreaseHP(damageAmount));
    }
    

    public void setTargetUnit(GameObject unit){
        targetUnit = unit;
        targetText = unitTextMap[unit];
        targetMaxHP = unit.GetComponent<Unit>().getMaxHP();
        targetCurrentHP = unit.GetComponent<Unit>().getCurrentHP();
    }

    // Instantiation of UI, this will be called when a unit is created
    public void mapUnitToText(GameObject unit){
        currentText = Instantiate(hpTextPrefab, targetCanvas.transform).GetComponent<TMP_Text>();
        currentHP = unit.GetComponent<Unit>().getMaxHP();
        currentText.color = HighHPColor;
        currentText.transform.position = unit.transform.position + offset;
        unitTextMap[unit] = currentText;
        currentText.text = currentHP.ToString("0");

    }

    // Set the current unit and update current variables
    public void setCurrentUnit(GameObject unit){
        currentUnit = unit;
        currentText = unitTextMap[unit];
        maxHP = unit.GetComponent<Unit>().getMaxHP();
        currentHP = unit.GetComponent<Unit>().getCurrentHP();
    }

    // Attach text to the unit
    public void UpdateTextPosition(Vector3 position) {
        currentText.transform.position = position + offset;
    }

    private IEnumerator SmoothDecreaseHP(float damageAmount){
        float damagePerTick = damageAmount / smoothDecreaseDuration;
        float elapsedTime = 0f;

        while (elapsedTime < smoothDecreaseDuration){
            float currentDamage = damagePerTick * Time.deltaTime;
            targetCurrentHP -= currentDamage;
            elapsedTime += Time.deltaTime;
            targetText.text = targetCurrentHP.ToString("0");


            if (targetCurrentHP <= 0){
                targetCurrentHP = 0;
                break;
            }

            yield return null;
            
        }

        if (targetCurrentHP < targetMaxHP/4){
                targetText.color = LowHPColor;
            }
            else if (targetCurrentHP < targetMaxHP/2){
                targetText.color = MidHPColor;
            }
            else {
                targetText.color = HighHPColor;
        }
        targetCurrentHP = Mathf.Round(targetCurrentHP);
        targetUnit.GetComponent<Unit>().setCurrentHP(targetCurrentHP);
        targetUnit =null;



    }

}
