using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Pickable
{
    // R�f�rence � l'item dans le HUD
    [SerializeField] private GameObject _itemInUI;

    public override void TakenByHero()
    {
        // On rend actif l'item dans le HUD ----> FAIT DANS LE PUT ITEM IN SLOT du WaypointEvent
        // _heroInventory.EarnItem(_itemInUI);

        // On lance l'anim de disparition de l'objet de la sc�ne + son + destroy
        base.TakenByHero();
    }
}
