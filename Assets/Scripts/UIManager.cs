using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject gameElements;
    public GameObject gameOver;
    public TMP_Text high_score;
    public int score;
    string sceneName;
    // Start is called before the first frame update
    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }
    public void GameOverScreen()
    {
        gameElements.SetActive(false);
        gameOver.SetActive(true);
        
        if (Highscore.highscore < score)
        {
            high_score.text = "New High Score: " + score + "m";
            Highscore.highscore = score;
            return;
        }
        high_score.text = "Score: " + score + "m";
    }
    public void Restart()
    {
        SceneManager.LoadScene(sceneName);
    }
}
