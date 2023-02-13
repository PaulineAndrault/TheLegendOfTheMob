using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liftable : MonoBehaviour
{
    [SerializeField] private float _xRotationWhenLifted;
    [SerializeField] private float _weight;

    public float XRotationWhenLifted { get => _xRotationWhenLifted; set => _xRotationWhenLifted = value; }
    public float Weight { get => _weight; set => _weight = value; }
}
