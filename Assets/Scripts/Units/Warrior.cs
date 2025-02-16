using UnityEngine;
using System.Collections;

public class Warrior : NormalUnit
{
    protected override IEnumerator ExecuteAttack(GameObject targetUnit)
    {
        audioManager.PlaySFX(audioManager.swordHitBlood);
        uiController.setTargetUnit(targetUnit);
        ResetTilesToBlack();
        yield return new WaitForSeconds(0.2f);
        uiController.dealDamage(attackDamage);
    }
}
