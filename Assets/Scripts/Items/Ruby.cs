using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ruby : Pickable
{
    public override void TakenByHero()
    {
        _heroInventory.EarnMoney(_value);

        base.TakenByHero();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hero"))
        {
            TakenByHero();
        }
    }
}
