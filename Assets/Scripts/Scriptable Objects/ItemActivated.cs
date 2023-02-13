using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemActivated : ScriptableObject
{
    [SerializeField] private bool _value;

    public bool Value { get => _value; set => _value = value; }
}
