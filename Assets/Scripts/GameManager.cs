using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT
    }

    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    };

    [System.Serializable]
    public struct SweetPosition
    {
        public SweetsType type;
        public int x;
        public int y;
    }

    public int xColumn;
    public int yRow;
    public float fillTime;

    public Level level;

    public SweetPrefab[] sweetPrefabs;
    public GameObject gridPrefab;

    public SweetPosition[] initialSweets;

    private Dictionary<SweetsType, GameObject> sweetPrefabDict;

    private GameSweet[,] sweets;

    private bool inverse = false;

    private GameSweet pressedSweet;
    private GameSweet enteredSweet;
    
    private bool gameOver = false;

    private bool isFilling = false;

    public bool IsFilling
    {
        get { return isFilling; }
    }


    void Awake()
    {
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CorrectPositon(x, y), Quaternion.identity);
                chocolate.transform.parent = transform;
            }
        }

        sweets = new GameSweet[xColumn, yRow];

        for (int i = 0; i < initialSweets.Length; i++)
        {
            if (initialSweets[i].x >= 0 && initialSweets[i].x < xColumn && initialSweets[i].y >= 0 && initialSweets[i].y < yRow)
            {
                CreateNewSweet(initialSweets[i].x, initialSweets[i].y, initialSweets[i].type);
            }
        }

        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if(sweets[x, y] == null)
                {
                    CreateNewSweet(x, y, SweetsType.EMPTY);
                }
            }
        }

        StartCoroutine(AllFill());
    }

    public IEnumerator AllFill()
    {
        bool needRefill = true;
        isFilling = true;

        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);

            while (Fill())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needRefill = ClearAllMatchedSweet();
        }

        isFilling = false;
    }

    public bool Fill()
    {
        bool filledNotFinished = false;

        for (int y = yRow - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xColumn; loopX++)
            {
                int x = loopX;
                if(inverse)
                {
                    x = xColumn - 1 - loopX;
                }

                GameSweet sweet = sweets[x, y];

                if (sweet.CanMove())
                {
                    GameSweet sweetBelow = sweets[x, y + 1];

                    if (sweetBelow.Type == SweetsType.EMPTY)
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponent.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        filledNotFinished = true;
                    }
                    else
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;

                                if(inverse)
                                {
                                    downX = x - down;
                                }

                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];

                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboveY];
                                            
                                            if (sweetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            filledNotFinished = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];

            if (sweet.Type == SweetsType.EMPTY)
            {
                GameObject newSweet = Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPositon(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0, fillTime);
                sweets[x, 0].ColoredComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColoredComponent.NumColors));
                filledNotFinished = true;
            }
        }

        return filledNotFinished;
    }

    public Vector2 CorrectPositon(int x, int y)
    {
        return new Vector2(transform.position.x - xColumn / 2.0f + x, transform.position.y + yRow / 2.0f - y);

    }

    public GameSweet CreateNewSweet(int x, int y, SweetsType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], CorrectPositon(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);

        return sweets[x, y];
    }

    private bool IsFriend(GameSweet sweet1, GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }

    private void ExchangeSweets(GameSweet sweet1, GameSweet sweet2)
    {
        if(gameOver)
        {
            return;
        }

        if (sweet1.CanMove() && sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null || MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || sweet1.Type == SweetsType.RAINBOWCANDY || sweet2.Type == SweetsType.RAINBOWCANDY)
            {
                int tempX = sweet1.X;
                int tempY = sweet1.Y;

                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponent.Move(tempX, tempY, fillTime);

                if (sweet1.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();

                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColoredComponent.Color;
                    }

                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet2.CanClear() && sweet1.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();

                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet1.ColoredComponent.Color;
                    }

                    ClearSweet(sweet2.X, sweet2.Y);
                }

                if(sweet1.Type == SweetsType.ROW_CLEAR || sweet1.Type == SweetsType.COLUMN_CLEAR)
                {
                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetsType.ROW_CLEAR || sweet2.Type == SweetsType.COLUMN_CLEAR)
                {
                    ClearSweet(sweet2.X, sweet2.Y);
                }

                pressedSweet = null;
                enteredSweet = null;

                ClearAllMatchedSweet();
                StartCoroutine(AllFill());

                level.OnMove();
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }

    public void PressSweet(GameSweet sweet)
    {
        pressedSweet = sweet;
    }

    public void EnterSweet(GameSweet sweet)
    {
        enteredSweet = sweet;
    }

    public void ReleaseSweet()
    {
        if (IsFriend(pressedSweet, enteredSweet))
        {
            ExchangeSweets(pressedSweet, enteredSweet);
        }
    }

    public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            ColorSweet.ColorType color = sweet.ColoredComponent.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            matchRowSweets.Add(sweet);

            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;

                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xColumn)
                    {
                        break;
                    }

                    if (sweets[x, newY].CanColor() && sweets[x, newY].ColoredComponent.Color == color)
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }

            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;

                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColoredComponent.Color == color)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchLineSweets.Count < 2)
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }

            matchRowSweets.Clear();
            matchLineSweets.Clear();
            matchLineSweets.Add(sweet);

            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow)
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColoredComponent.Color == color)
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;

                            if (j == 0)
                            {
                                x = newY - xDistance;
                            }
                            else
                            {
                                x = newY + xDistance;
                            }
                            if (x < 0 || x >= xColumn)
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColoredComponent.Color == color)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }
        }

        return null;
    }

    private bool ClearAllMatchedSweet()
    {
        bool needRefill = false;

        for (int y = 0; y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);

                    if (matchList != null)
                    {
                        SweetsType specialSweetsType = SweetsType.COUNT;
                        GameSweet randomSweet = matchList[Random.Range(0, matchList.Count)];
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;

                        if (matchList.Count == 4)
                        {
                            if(pressedSweet == null || enteredSweet == null)
                            {
                                specialSweetsType = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR);
                            }
                            else if(pressedSweet.Y == enteredSweet.Y)
                            {
                                specialSweetsType = SweetsType.ROW_CLEAR;
                            }
                            else
                            {
                                specialSweetsType = SweetsType.COLUMN_CLEAR;
                            }
                        }
                        else if (matchList.Count >= 5)
                        {
                            specialSweetsType = SweetsType.RAINBOWCANDY;
                        }

                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (ClearSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;

                                if(matchList[i] == pressedSweet || matchList[i] == enteredSweet)
                                {
                                    specialSweetX = matchList[i].X;
                                    specialSweetY = matchList[i].Y;
                                }

                            }
                        }

                        if (specialSweetsType != SweetsType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialSweetsType);
                            
                            if (specialSweetsType == SweetsType.ROW_CLEAR || specialSweetsType == SweetsType.COLUMN_CLEAR && newSweet.CanColor() && matchList[0].CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(matchList[0].ColoredComponent.Color);
                            }
                            else if (specialSweetsType == SweetsType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needRefill;
    }

    public bool ClearSweet(int x, int y)
    {
        if (sweets[x, y].CanClear() && !sweets[x, y].ClearedComponent.IsClearing)
        {
            sweets[x, y].ClearedComponent.Clear();
            CreateNewSweet(x, y, SweetsType.EMPTY);

            ClearBarrier(x, y);
            return true;
        }

        return false;
    }

    private void ClearBarrier(int x, int y)
    {
        for (int friendX = x - 1; friendX <= x + 1; friendX++)
        {
            if (friendX != x && friendX >= 0 && friendX < xColumn)
            {
                if (sweets[friendX, y].Type == SweetsType.BARRIER && sweets[friendX, y].CanClear())
                {
                    sweets[friendX, y].ClearedComponent.Clear();
                    CreateNewSweet(friendX, y, SweetsType.EMPTY);
                }
            }
        }

        for (int friendY = y - 1; friendY <= y + 1; friendY++)
        {
            if (friendY != y && friendY >= 0 && friendY < yRow)
            {
                if (sweets[x, friendY].Type == SweetsType.BARRIER && sweets[x, friendY].CanClear())
                {
                    sweets[x, friendY].ClearedComponent.Clear();
                    CreateNewSweet(x, friendY, SweetsType.EMPTY);
                }
            }
        }
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearSweet(column, y);
        }
    }

    public void ClearColor(ColorSweet.ColorType color)
    {
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x, y].CanColor() && (sweets[x, y].ColoredComponent.Color == color || color == ColorSweet.ColorType.ANY))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }

    public void GameOver()
    {
        gameOver = true;
    }

    public List<GameSweet> GetSweetsOfType(SweetsType type)
    {
        List<GameSweet> sweetsOfType = new List<GameSweet>();

        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x, y].Type == type)
                {
                    sweetsOfType.Add(sweets[x, y]);
                }
            }
        }

        return sweetsOfType;
    }
}
