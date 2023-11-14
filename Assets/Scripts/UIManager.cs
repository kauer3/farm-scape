using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject gameElements;
    public GameObject gameOver;
    public GameObject pause;
    public TMP_Text high_score;
    public int score;
    public TMP_Text playerText;
    string sceneName;
    string savePath;
    XmlDocument xmlDocument;
    // Start is called before the first frame update
    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        savePath = Application.dataPath + "/SaveData.save";
        xmlDocument = new XmlDocument();
        if (!File.Exists(savePath))
        {
            XmlElement root = xmlDocument.CreateElement("SaveData");
            xmlDocument.AppendChild(root);
            xmlDocument.Save(savePath);
        }
        else
        {
            xmlDocument.Load(savePath);
        }
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(pause.activeSelf)
            {
                pause.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else if (!gameOver.activeSelf)
            {
                pause.SetActive(true);
                Time.timeScale = 0;
            }
        }
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
    public void Exit()
    {
        Application.Quit();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void SaveGame()
    {
        if(playerText.text.Length > 0) 
        {
            string playerName = playerText.text;
            try
            {
                xmlDocument.SelectSingleNode("/SaveData/Player[@name='" + playerName + "']/Highscore").InnerText = Highscore.highscore.ToString();
            }
            catch (NullReferenceException)
            {
                XmlElement playerElement = xmlDocument.CreateElement("Player");
                playerElement.SetAttribute("name", playerName);
                XmlElement highscoreElement = xmlDocument.CreateElement("Highscore");
                highscoreElement.InnerText = Highscore.highscore.ToString();
                playerElement.AppendChild(highscoreElement);
                xmlDocument.FirstChild.AppendChild(playerElement);
            }
            xmlDocument.Save(savePath);
        }
    }
}
