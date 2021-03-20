using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObstacles : Level
{
    public int timeInSeconds;
    public GameManager.SweetsType[] obstacleTypes;

    private int targetScore;

    private float timer;
    private bool timeOut = false;

    private int numObstaclesLeft;

    // Start is called before the first frame update
    void Start()
    {
        type = LevelType.OBSTACLE;

        for(int i = 0; i < obstacleTypes.Length; i++)
        {
            numObstaclesLeft += gameManager.GetSweetsOfType(obstacleTypes[i]).Count;
        }

        hud.SetLevelType(type);
        hud.SetScore(currentScore);
        hud.SetTarget(numObstaclesLeft);
        hud.SetRemaining(string.Format("{0}:{1:00}", timeInSeconds / 60, timeInSeconds % 60));
    }

    // Update is called once per frame
    void Update()
    {
        if (!timeOut)
        {
            timer += Time.deltaTime;
            hud.SetRemaining(string.Format("{0}:{1:00}", (int)Mathf.Max((timeInSeconds - timer) / 60, 0), (int)Mathf.Max((timeInSeconds - timer) % 60, 0)));

            if (timeInSeconds - timer <= 0)
            {
                if (numObstaclesLeft == 0)
                {
                    GameWin();
                }
                else
                {
                    GameLose();
                }

                timeOut = true;
            }
        }
    }

    public override void OnSweetCleared(GameSweet Sweet)
    {
        base.OnSweetCleared(Sweet);

        for(int i = 0; i < obstacleTypes.Length; i++)
        {
            if(obstacleTypes[i] == Sweet.Type)
            {
                numObstaclesLeft--;
                hud.SetTarget(numObstaclesLeft);

                if(numObstaclesLeft == 0)
                {
                    currentScore += 100;
                    hud.SetScore(currentScore);
                }

            }    
        }
    }
}
