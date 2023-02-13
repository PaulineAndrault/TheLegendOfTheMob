using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int _value;

    public int Value { get => _value; set => _value = value; }
}
