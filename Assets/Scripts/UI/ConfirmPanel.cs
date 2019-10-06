using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    [Header ("Level Information")]
    public string levelToLoad;
    public int level;
    private GameData gameData;
    private int starsActive;
    private int highScore;

    [Header ("UI stuff")]
    public Image[] stars;
    public Text highScoreText;
    public Text stageText;
    public GameObject confirmfadePanel;




    // Start is called before the first frame update
    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();
        LoadData();
        SetText();
        ActivateStars();
    }

    void LoadData()
    {
        if (gameData != null)
        {
            starsActive = gameData.saveData.stars[level - 1];
            highScore = gameData.saveData.highScores[level - 1];
        }
    }

    void SetText()
    {
        stageText.text = "" + level;
        highScoreText.text = "" + highScore;
    }

    void ActivateStars()
    {
        //Come back to this when the binary file is done!!!
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void Cancel()
    {
        confirmfadePanel.SetActive(false);
        this.gameObject.SetActive(false);
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = false;
        }
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

}
