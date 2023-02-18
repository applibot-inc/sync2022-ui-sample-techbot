// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuruBuru : MonoBehaviour
{
    private Vector3 _originalPos;
    [SerializeField] private float _haba = 0;
    void Start()
    {
        Application.targetFrameRate = 60;
        _originalPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float haba = _haba * Random.Range(0.2f, 1f);
        
        float random = Random.Range(0, 360f);
        double dx = haba * Math.Cos(Mathf.Deg2Rad * random);
        double dy = haba * Math.Sin(Mathf.Deg2Rad * random);
        transform.localPosition = (_originalPos + new Vector3((float)dx, (float)dy, 0));
    }
}
