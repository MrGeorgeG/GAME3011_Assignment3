using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public enum LevelType
    {
        TIMER,
        OBSTACLE,
        MOVES
    };

    public GameManager gameManager;
    public HUD hud;

    public int Score1Star;
    public int Score2Star;
    public int Score3Star;

    protected LevelType type;

    public LevelType Type
    {
        get { return type; }
    }

    protected int currentScore;

    protected bool didWin;

    // Start is called before the first frame update
    void Start()
    {
        hud.SetScore(currentScore);
    }

    public virtual void GameWin()
    {
        gameManager.GameOver();
        didWin = true;
        StartCoroutine(WaitForSweetFill());
    }

    public virtual void GameLose()
    {
        gameManager.GameOver();
        didWin = false;
        StartCoroutine(WaitForSweetFill());
    }

    public virtual void OnMove()
    {
        
    }

    public virtual void OnSweetCleared(GameSweet Sweet)
    {
        currentScore += Sweet.score;
        hud.SetScore(currentScore);
    }

    protected virtual IEnumerator WaitForSweetFill()
    {
        while(gameManager.IsFilling)
        {
            yield return 0;
        }

        if(didWin)
        {
            hud.OnGameWin(currentScore);
        }
        else
        {
            hud.OnGameLose();
        }
    }
}
