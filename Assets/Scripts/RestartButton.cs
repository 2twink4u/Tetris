using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    void Start()
    {
        if (GetComponent<Image>().sprite == null)
        {
            GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }

        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Используем современный метод поиска
            var board = FindAnyObjectByType<Board>();

            if (board != null)
            {
                board.RestartGame();
                Debug.Log("Restart button clicked!");
            }
            else
            {
                Debug.LogError("Board not found in the scene!");
            }
        });
    }
}