using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button newGame;
    private Button continousGame;
    private Button quitGame;

    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        newGame = root.Q<Button>("NewGameBtn");
        continousGame = root.Q<Button>("ContinuesBtn");
        quitGame = root.Q<Button>("QuitBtn");

        newGame.RegisterCallback<ClickEvent>(OnNewGameClick);
        quitGame.RegisterCallback<ClickEvent>(OnQuitGameClick);
    }

    private void OnQuitGameClick(ClickEvent evt)
    {
        EditorApplication.isPlaying = false;
    }

    private void OnNewGameClick(ClickEvent evt)
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
