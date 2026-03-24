using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class EndPanel : MonoBehaviour
{
    public GameObject endPanel;
    public TextMeshProUGUI text;
    public Image thisImage;

    public static EndPanel instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void OnEnable()
    {
        GameManager.onWinGame += WinGame;
        GameManager.onLoseGame += LoseGame;
    }
    
    void OnDisable()
    {
        GameManager.onWinGame -= WinGame;
        GameManager.onLoseGame -= LoseGame;
    }

    void WinGame()
    {
        endPanel.SetActive(true);
        thisImage.color = Color.green;
        text.text = "You win!";
        Time.timeScale = 0;
    }
    
    public void LoseGame()
    {
        endPanel.SetActive(true);
        thisImage.color = Color.red;
        text.text = "You Lose!";
        Time.timeScale = 0;
    }
}
