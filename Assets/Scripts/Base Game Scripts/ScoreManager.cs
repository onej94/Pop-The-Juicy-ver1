using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ScoreManager : MonoBehaviour
{

    private Board board;
    public Text scoreText;
    public int score;
    public Image scoreBar;
    private GameData gameData;
    private int numberStars;
    public Image[] BalnkStars;
    public Image[] stars;


    // Use this for initialization
    void Start()
    {        
        board = FindObjectOfType<Board>();
        gameData = FindObjectOfType<GameData>();

        RectTransform rectTransform = GetComponent<RectTransform>();        

        SetStarPosition();
        for (int i = 0; i < 3; i++)
        {
            stars[i].enabled = false;
        }
        UpdateBar();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "" + score;
    }

    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease;
        for (int i = 0; i < board.scoreGoals.Length; i++)
        {
            if (score > board.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }

        if (gameData != null)
        {
            if (board.currentState == GameState.win)
            {
                int highScore = gameData.saveData.highScores[board.level];
                if (score > highScore)
                {
                    gameData.saveData.highScores[board.level] = score;
                }

                int currentStars = gameData.saveData.stars[board.level];
                if (numberStars > currentStars)
                {
                    gameData.saveData.stars[board.level] = numberStars;
                }

                gameData.saveData.isClear[board.level] = true;            
                gameData.Save();

            }
        }
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (board != null && scoreBar != null)
        {

            int length = board.scoreGoals.Length;

            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1];
        }
        UpdateStar();
    }   

    private void UpdateStar()
    {      
        if (numberStars == 1)
        {
            stars[0].enabled = true;            
        }
        else if (numberStars == 2)
        {
            stars[1].enabled = true;
        }
        else if (numberStars == 3)
        {
            stars[2].enabled = true;
        }
    }

    private void SetStarPosition()
    {
        RectTransform rt = (RectTransform)scoreBar.transform;
        RectTransform[] stars_rt = new RectTransform[3];
        
        float[] ratio = new float[3];

        for (int i = 0; i < 3; i++)
        {
            ratio[i] = (float)board.scoreGoals[i] / (float)board.scoreGoals[2];
            stars_rt[i] = (RectTransform)BalnkStars[i].transform;
            stars_rt[i].anchoredPosition = new Vector2(rt.rect.width * ratio[i] * 9/10, rt.rect.height + 8);
        }
    }
}
