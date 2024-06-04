using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBonusScore : BonusScore
{
    public override int Pick()
    {
        if (!IsServer)
        {
            Display(false);
            return 0;
        }

        if (isCollected)
        {
            return 0;
        }

        isCollected = true;

        return bonusScore;
    }
}
