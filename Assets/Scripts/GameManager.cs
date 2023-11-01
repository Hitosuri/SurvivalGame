using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;

    public static GameManager Instance {
        get {
            if (_instance is null) {
                Debug.LogError("Game Manager is null");
            }

            return _instance;
        }
    }

    public event Action<GameManager> OneSecondTick;
    public int SecondPassed { get; private set; }

    private void Awake() {
        SecondPassed = 0;
        OneSecondTick = delegate(GameManager manager) { };
        InvokeRepeating("InvokeOneSecondTick", 0f, 1f);
        _instance = this;
    }

    private void InvokeOneSecondTick() {
        OneSecondTick?.Invoke(_instance);
        SecondPassed++;
    }
}