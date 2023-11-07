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
    private Button back;
    private VisualElement listLoadGame;
    private VisualElement item;
    private VisualElement Container_Bottom;
    private VisualElement Scrim;
    private VisualElement LoadScreen;
    public VisualTreeAsset itemTemplate;
    // Start is called before the first frame update

    private List<VisualElement> itemsList = new List<VisualElement>();
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        Scrim = root.Q<VisualElement>("Scrim");
        LoadScreen = root.Q<VisualElement>("LoadScreen");
        listLoadGame = root.Q<VisualElement>("listLoadGame");
        newGame = root.Q<Button>("NewGameBtn");
        continousGame = root.Q<Button>("ContinuesBtn");
        back = root.Q<Button>("BackBtn");
        quitGame = root.Q<Button>("QuitBtn");
        item = root.Q<VisualElement>("GameScene");
        Container_Bottom = root.Q< VisualElement > ("Container_Bottom");
        Container_Bottom.style.display = DisplayStyle.None;
        continousGame.RemoveFromClassList("list-up");
        newGame.RegisterCallback<ClickEvent>(OnNewGameClick);
        quitGame.RegisterCallback<ClickEvent>(OnQuitGameClick);
        continousGame.RegisterCallback<ClickEvent>(OnLoadGameClick);
        

        back.RegisterCallback<ClickEvent>(OnBackGameClick);
        ////List<VisualElement> stringg = new List<VisualElement>()
        ////{
        //List<VisualElement> listElement = new List<VisualElement>();
        ////};

        //itemsList = GenerateItems();

        //foreach (var item in itemsList)
        //{
        //    listLoadGame.Add(item);
        //}


    }
    private VisualElement MakeItem()
    {

        var newItem = itemTemplate.CloneTree();
        
        return newItem;
    }

    private void BindItem(VisualElement element, int index)
    {

    }

    private void OnQuitGameClick(ClickEvent evt)
    {
        EditorApplication.isPlaying = false;
    }

    private void OnNewGameClick(ClickEvent evt)
    {
        SceneManager.LoadScene("Test");
    }
    private void OnLoadGameClick(ClickEvent evt)
    {
        Container_Bottom.style.display = DisplayStyle.Flex;
        LoadScreen.AddToClassList("list-up");
        Scrim.AddToClassList("scrim-fadedin");
    }

    private void OnBackGameClick(ClickEvent evt)
    {
        Container_Bottom.style.display = DisplayStyle.None;
        LoadScreen.AddToClassList("list");
        Scrim.AddToClassList("scrim");
    }
    private List<VisualElement> GenerateItems()
    {
        // Generate dummy data for the ListView

        var items = new List<VisualElement>();
        for (int i = 0; i < 2; i++)
        {
            var newItem = itemTemplate.CloneTree();
            // Customize the new item if needed
            items.Add(newItem);
        }
        return items;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
