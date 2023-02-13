using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class NameMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Button _button;
    public bool _nameEmpty = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _button.onClick.Invoke();
        }
    }

    public void CheckEmptyName()
    {
        _nameEmpty = _name.text == null ? true : false;
    }

    public void SaveName()
    {
        if (_nameEmpty) { return; }

        PlayerPrefs.SetString("Name", _name.text);
        SceneManager.LoadScene(1);
    }
}
