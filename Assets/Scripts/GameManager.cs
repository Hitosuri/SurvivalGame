using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interface;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldTime;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour {
    public GameObject DroppedItemTemplate;
    public int DayLengthInSeconds = 30;
    private static GameManager _instance;
    private Dictionary<string, Action> _onChangeActions;
    private IHudController _hudController;
    private PlayerController _player;
    private Tilemap _soilLayer;
    private float _fixedTimeDelta;
    public TimeSpan CurrentRealTime { get; set; }
    public TimeSpan CurrentGameTime { get; set; }

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
    public event Action<TimeSpan> FixedUpdateTick;

    private void Awake() {
        _onChangeActions = new Dictionary<string, Action>();
        CurrentGameTime = TimeSpan.Zero;
        CurrentRealTime = TimeSpan.Zero;
        OneSecondTick = delegate(GameManager manager) {
            HudController?.SetTime((int)Math.Round((CurrentGameTime + TimeSpan.FromHours(12)).TotalMinutes));
        };
        FixedUpdateTick = delegate(TimeSpan timeSpan) { };
        InvokeRepeating("InvokeOneSecondTick", 0f, 1f);
        _instance = this;
        _fixedTimeDelta = WorldTimeContants.MinutesDay * 60f / DayLengthInSeconds * Time.fixedDeltaTime;
    }

    private void FixedUpdate() {
        CurrentGameTime += TimeSpan.FromSeconds(_fixedTimeDelta);
        FixedUpdateTick?.Invoke(CurrentGameTime + TimeSpan.FromHours(12));
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
        CurrentRealTime += TimeSpan.FromSeconds(1);
    }

    public static void Print(object obj) => print(obj);

    public static void DestroyGameObject(Object obj) => Destroy(obj);
}