using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interface;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    private Dictionary<string, Action> _onChangeActions;
    public GameObject DroppedItemTemplate;
    private IHudController _hudController;
    private PlayerController _player;
    private Tilemap _soilLayer;

    public IHudController HudController {
        get => _hudController;
        set {
            _hudController = value;
            OnPropertyChangeHandler(nameof(HudController));
        }
    }

    public PlayerController Player {
        get => _player;
        set {
            _player = value;
            OnPropertyChangeHandler(nameof(Player));
        }
    }

    public Tilemap SoilLayer {
        get => _soilLayer;
        set {
            _soilLayer = value;
            OnPropertyChangeHandler(nameof(SoilLayer));
        }
    }

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
        _onChangeActions = new Dictionary<string, Action>();
        SecondPassed = 0;
        OneSecondTick = delegate(GameManager manager) { };
        InvokeRepeating("InvokeOneSecondTick", 0f, 1f);
        _instance = this;
    }

    private void OnPropertyChangeHandler(string propertyName) {
        if (_onChangeActions.TryGetValue(propertyName.Trim(), out var action)) {
            action?.Invoke();
        }
    }

    public void OnPropertyChange(string propertyName, Action callback) {
        if (_onChangeActions.ContainsKey(propertyName.Trim())) {
            _onChangeActions[propertyName] += callback;
        } else {
            _onChangeActions[propertyName] = callback;
        }
    }

    public void CallDelay(Action callback, float delayTime) {
        StartCoroutine(WaitForFunction(delayTime, callback));
    }

    private IEnumerator WaitForFunction(float delayTime, Action callback) {
        yield return new WaitForSeconds(delayTime);
        callback.Invoke();
    }

    private void InvokeOneSecondTick() {
        OneSecondTick?.Invoke(_instance);
        SecondPassed++;
    }

    public static void Print(object obj) => print(obj);

    public static void DestroyGameObject(Object obj) => Destroy(obj);
}