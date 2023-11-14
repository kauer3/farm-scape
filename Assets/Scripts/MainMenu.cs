using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Animation cameraAnimation;
    public GameObject mainMenu;
    public TMP_Text scoreText;
    public TMP_Dropdown playerList;
    bool started = false;
    string savePath;

    private void Start()
    {
        savePath = Application.dataPath + "/SaveData.save";
        LoadScore();
        LoadList();
    }
    public void PlayGame()
    {
        mainMenu.SetActive(false);
        cameraAnimation.Play();
        started = true;
    }
    public void LoadScore()
    {
        scoreText.text = "Highscore: " + Highscore.highscore + "m";
    }
    public void Load()
    {
        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(savePath);
            string loadPlayer = playerList.options[playerList.value].text;
            Highscore.highscore = int.Parse(xmlDocument.SelectSingleNode("/SaveData/Player[@name='" + loadPlayer + "']/Highscore").InnerText);
            LoadScore();
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.LogError("Load List Empty!!!");
        }
    }
    public void LoadList()
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(savePath);
        List<string> options = new List<string>();
        XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Player");
        foreach (XmlNode node in nodeList)
        {
            options.Add(node.Attributes["name"].Value);
        }
        playerList.AddOptions(options);
    }
    public void Exit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if(started && !cameraAnimation.isPlaying)
        {
            SceneManager.LoadSceneAsync("Game");
        }
    }
}
