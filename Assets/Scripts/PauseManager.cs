using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject uiDocument;

    private bool isUIVisible = false;

    void Start()
    {
        HideUI(); // Ẩn UI document khi vào PauseScene
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUIVisible)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }
    }

    void ShowUI()
    {
        uiDocument.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian trong trò chơi khi hiển thị UI
        isUIVisible = true;
    }

    void HideUI()
    {
        uiDocument.SetActive(false);
        Time.timeScale = 1f; // Khôi phục thời gian trong trò chơi khi ẩn UI
        isUIVisible = false;
    }
}