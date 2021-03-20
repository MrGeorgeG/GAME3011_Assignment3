using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Level level;
    public GameOver gameOver;

    public Text remainingText;
    public Text remainingSubtext;
    public Text targetText;
    public Text targetSubtext;
    public Text scoreText;
    public Image[] stars;

    private int starIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < stars.Length; i++)
        {

            stars[i].enabled = false;
            
        }
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();

        int visibleStar = 0;

        if (score >= level.Score1Star && score < level.Score2Star)
        {
            stars[0].enabled = true;
            visibleStar = 0;

        }
        else if (score >= level.Score2Star && score < level.Score3Star)
        {
            stars[1].enabled = true;
            visibleStar = 1;
        }
        else if (score >= level.Score3Star)
        {
            stars[2].enabled = true;
            visibleStar = 2;
        }
        starIdx = visibleStar;
    }

    public void SetTarget(int target)
    {
        targetText.text = target.ToString();
    }

    public void SetRemaining(int remaining)
    {
        remainingText.text = remaining.ToString();
    }

    public void SetRemaining(string remaining)
    {
        remainingText.text = remaining;
    }

    public void SetLevelType(Level.LevelType type)
    {
        if(type == Level.LevelType.MOVES)
        {
            remainingSubtext.text = "Move remaining";
            targetSubtext.text = "Target score";
        }
        else if (type == Level.LevelType.TIMER)
        {
            remainingSubtext.text = "Time remaining";
            targetSubtext.text = "Target score";
        }
        else if (type == Level.LevelType.OBSTACLE)
        {
            remainingSubtext.text = "Time remaining";
            targetSubtext.text = "Bubble remaining";
        }
    }

    public void OnGameWin(int score)
    {
        gameOver.ShowWin(score,starIdx);

    }
    public void OnGameLose()
    {
        gameOver.ShowLose();
    }
}
