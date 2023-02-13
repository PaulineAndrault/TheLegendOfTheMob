using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInventory : MonoBehaviour
{
    // Rubis (à passer en scriptable objects
    [Header("Paramètres des Rubis")]
    [SerializeField] private IntVariable _totalRubies;

    // Inventaire HUD
    [Header("Slots de l'inventaire")]
    [SerializeField] private List<Transform> _slots = new List<Transform>();
    [SerializeField] private Transform _nonUsedItemsParent;

    // Références
    private UIManager _UI;

    public IntVariable TotalRubies { get => _totalRubies; set => _totalRubies = value; }

    private void Awake()
    {
        _UI = FindObjectOfType<UIManager>();
    }

    public void EarnMoney(int amount)
    {
        _totalRubies.Value += amount;
        _UI.RefreshHUD();
    }

    public void EarnItem(GameObject itemInUI)
    {
        itemInUI.SetActive(true);        
    }

    public bool HasAvailableSlot()
    {
        bool noSlotAvailable = true;
        foreach (Transform slot in _slots)
        {
            // S'il n'y a pas d'enfant dans le Slot, alors il est available
            if (slot.childCount == 0)
            {
                noSlotAvailable = false;
                break;
            }
        }
        return !noSlotAvailable;
    }

    public void PutItemInAvailableSlot(GameObject item)
    {
        foreach(Transform slot in _slots)
        {
            // S'il n'y a pas d'enfant dans le Slot, alors il est available
            if(slot.childCount == 0)
            {
                // On place l'item dans le slot
                item.transform.SetParent(slot);
                break;
            }
        }
    }

    public void ClearSlot(Transform slot)
    {
        // On re-parente l'item child du slot dans le parent Non Used Items
        slot.GetChild(0).SetParent(_nonUsedItemsParent);
    }
}
