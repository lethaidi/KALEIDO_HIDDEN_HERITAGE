using System;
using UnityEngine;

public class Snap_point_trigger : MonoBehaviour
{
    [SerializeField] private Snap_system snapSystem;
    [SerializeField] private int snapPointIndex;
    [SerializeField] private Transform spawn_point;

    void Start()
    {
        if (spawn_point == null) spawn_point = transform.Find("spawn_point");
    }
    public void OnClicked()
    {
        snapSystem.ChooseSnapPointIndex(snapPointIndex, spawn_point);
    }
}