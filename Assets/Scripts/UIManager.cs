using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Rubis
    [SerializeField] TMP_Text _rubyText;
    [SerializeField] private IntVariable _totalRubies;

    // Timer
    [Header("Timer")]
    [SerializeField] TMP_Text _timeText;
    
    // Références
    private HeroInventory _heroInv;
    private LevelManager _levelManager;

    private void Awake()
    {
        _heroInv = FindObjectOfType<HeroInventory>();
        _levelManager = FindObjectOfType<LevelManager>();

        // On initialise les valeurs
        _totalRubies.Value = 0;
    }

    private void Start()
    {
        RefreshHUD();
    }

    public void RefreshTimer()
    {
        _timeText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(_levelManager.CurrentTime / 60), Mathf.FloorToInt(_levelManager.CurrentTime % 60));
    }

    public void RefreshHUD()
    {
        _rubyText.text = string.Format("{0:000}", _totalRubies.Value);
    }

}
