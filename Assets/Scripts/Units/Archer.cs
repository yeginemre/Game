using UnityEngine;
using System.Collections;

public class Archer : NormalUnit
{
    protected override IEnumerator ExecuteAttack(GameObject targetUnit)
    {
        audioManager.PlaySFX(audioManager.arrowHitBlood);
        uiController.setTargetUnit(targetUnit);
        ResetTilesToBlack();
        yield return new WaitForSeconds(0.5f);
        uiController.dealDamage(attackDamage);
    }
}
