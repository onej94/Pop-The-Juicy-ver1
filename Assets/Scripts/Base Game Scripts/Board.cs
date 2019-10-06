using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Lock,
    Concrete,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public int columnorrow;
    public string color;
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    [Header("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;
    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject ConcreteTilePrefab;
    public GameObject SlimePiecePrefab;
    public GameObject[] dots;
    public GameObject destroyParticle;

    [Header("Layout")]
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public GameObject[,] allDots;

    [Header("Match Stuff")]
    public MatchType matchType;
    public Dot currentDot;
    private FindMatches findMatches;
    public int basePieceValue = 20; //기본 점수
    private int streakValue = 1; //연속 점수
    private ScoreManager scoreManager; //점수 관리 스크립트 권한 부여
    private SoundManager soundManager;
    private GoalManager goalManager;
    private EndGameManager endgameManager;

    public float refillDealy = 0.5f;
    public int[] scoreGoals;
    private bool makeSlime = true;
    private int cnt = 0;


    private void Awake() //실행전 스크립터블 스크립트에서 정보 가져오기
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        goalManager = FindObjectOfType<GoalManager>();
        soundManager = FindObjectOfType<SoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        endgameManager = FindObjectOfType<EndGameManager>();
        breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        slimeTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "jelly" tile
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                //create a "jelly" tile at that position.
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + boardLayout[i].x + ", " + boardLayout[i].y + " )";

                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }

        }
    }

    private void GenerateLockTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //create a "Lock" tile at that position.
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + boardLayout[i].x + ", " + boardLayout[i].y + " )";

                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }

        }
    }

    private void GenerateConcreteTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //create a "Lock" tile at that position.
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + boardLayout[i].x + ", " + boardLayout[i].y + " )";

                GameObject tile = Instantiate(ConcreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }

        }
    }

    private void GenerateSlimeTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Slime)
            {

                //Create a "Lock" tile at that position;
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);


                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + boardLayout[i].x + ", " + boardLayout[i].y + " )";


                GameObject tile = Instantiate(SlimePiecePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void MakeUnmovetype()
    {
        int key = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (key == 0 && (slimeTiles[i, j] || concreteTiles[i, j]))
                {
                    key = j;
                }
            }

            for (int k = 0; k < key; k++)
            {
                Vector2 tempPosition = new Vector2(i, k);

                blankSpaces[i, k] = true;

                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ", " + k + " )";
            }

            key = 0;

        }


    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();

        /*for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                if (slimeTiles[i, j] || concreteTiles[i, j])
                {
                    for (int k = 0; k < j; k++)
                    {
                        Vector2 tempPosition = new Vector2(i, k);

                        blankSpaces[i, k] = true;

                        GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                        backgroundTile.transform.parent = this.transform;
                        backgroundTile.name = "( " + i + ", " + k + " )";
                    }
                }
            }
        }*/

        MakeUnmovetype();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                if (!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {

                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + ", " + j + " )";

                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log("max" + maxIterations);
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";
                    allDots[i, j] = dot;
                }
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private MatchType ColumnOrRow()
    {
        //Make a copy of the current matches
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = 0;
        matchType.color = "";

        //Cycle through all of match Copy and decide if a bomb needs to be made
        for (int i = 0; i < matchCopy.Count; i++)
        {
            //Store this dot
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;

            //Cycle through the rest of the pieces and compare
            for (int j = 0; j < matchCopy.Count; j++)
            {
                //Store the next dot
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            //Return 3 if column or row match
            //Return 2 if adjacent
            //Return 1 if it's a color bomb
            if (columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                if (columnMatch == 3)
                    matchType.columnorrow = 1;
                else if (rowMatch == 3)
                    matchType.columnorrow = 2;

                return matchType;
            }
        }
        matchType.type = 0;
        matchType.color = "";
        return matchType;
    }

    private void CheckToMakeBombs()
    {
        //How many objects are in findMatches currentMatches?
        if (findMatches.currentMatches.Count > 3)
        {
            //What type of match?
            MatchType typeOfMatch = ColumnOrRow();
            if (typeOfMatch.type == 1)
            {
                //Make a color bomb
                //is the current dot matched?
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeColorBomb();
                }
                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                        if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                        {
                            otherDot.isMatched = false;
                            otherDot.MakeColorBomb();
                        }
                    }
                }
            }
            else if (typeOfMatch.type == 2)
            {
                //Make a adjacent bomb
                //is the current dot matched?
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjacentBomb();
                }
                else if (currentDot.otherDot != null)
                {
                    Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                    if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                    {
                        otherDot.isMatched = false;
                        otherDot.MakeAdjacentBomb();
                    }
                }
            }
            else if (typeOfMatch.type == 3)
            {
                findMatches.CheckBombs(typeOfMatch);
            }
        }
    }


    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {

            if (concreteTiles[i, row])
            {
                concreteTiles[i, row].TakeDamage(1);
                if (concreteTiles[i, row].hitPoints <= 0)
                {
                    concreteTiles[i, row] = null;
                }
                makeSlime = false;
                MakeNormaltype(i, row);
            }

            if (slimeTiles[i, row])
            {
                slimeTiles[i, row].TakeDamage(1);
                if (slimeTiles[i, row].hitPoints <= 0)
                {
                    slimeTiles[i, row] = null;
                }
                makeSlime = false;
                MakeNormaltype(i, row);
            }
        }
    }

    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {

            if (concreteTiles[column, i])
            {
                concreteTiles[column, i].TakeDamage(1);
                if (concreteTiles[column, i].hitPoints <= 0)
                {
                    concreteTiles[column, i] = null;
                }
                makeSlime = false;
                MakeNormaltype(column, i);
            }

            if (slimeTiles[column, i])
            {
                slimeTiles[column, i].TakeDamage(1);
                if (slimeTiles[column, i].hitPoints <= 0)
                {
                    slimeTiles[column, i] = null;
                }
                makeSlime = false;
                MakeNormaltype(column, i);
            }

        }
    }

    public void WinBomb()
    {

        int x = 0;
        int y = 0;
        int fx = 0;
        int fy = 0;

        int counter = endgameManager.currentCounterValue;
        endgameManager.currentCounterValue = 0;
        
        for (int i = 0; i < counter; i++)
        {
            x = Random.Range(0, width);
            y = Random.Range(0, height);
            int kindofbomb = Random.Range(0, 2);


            if (allDots[x, y].GetComponent<Dot>() != null && x == fx && y == fy)
            {
                while (!(x == fx) || !(y == fy))
                {
                    x = Random.Range(0, width);
                    y = Random.Range(0, height);
                    Debug.Log("rerandom ");

                }
            }

            Debug.Log("x : " + x + ", y : " + y);


            switch (kindofbomb)
            {

                case 0:
                    allDots[x, y].GetComponent<Dot>().isMatched = false;
                    allDots[x, y].GetComponent<Dot>().MakeColumnBomb();
                    findMatches.currentMatches.Union(findMatches.GetRowPieces(allDots[x, y].GetComponent<Dot>().row));
                    break;
                case 1:
                    allDots[x, y].GetComponent<Dot>().isMatched = false;
                    allDots[x, y].GetComponent<Dot>().MakeRowBomb();
                    findMatches.currentMatches.Union(findMatches.GetColumnPieces(allDots[x, y].GetComponent<Dot>().column));
                    break;
            }

            findMatches.currentMatches.Clear();


            fx = x;
            fy = y;

        }

        Debug.Log("winbomb");


        DestroyAllBomb();
    }
    
    private void DestroyAllBomb()
    {
        //findMatches.currentMatches.Clear();

        StartCoroutine(DestroyAllBombCo());

        StartCoroutine(DecreaseRowCo2());
    }

    private IEnumerator DestroyAllBombCo()
    {
        yield return new WaitForSeconds(refillDealy);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                //Debug.Log(allDots[i, j].GetComponent<Dot>().column + " " + allDots[i, j].GetComponent<Dot>().row);
                DestroyMatchesAt(allDots[i, j].GetComponent<Dot>().column, allDots[i, j].GetComponent<Dot>().row);

                Debug.Log("winbomb Destroy");


            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            //Does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                //if it does, give one damage.
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            //lockTile
            if (lockTiles[column, row] != null)
            {
                //if it does, give one damage.
                lockTiles[column, row].TakeDamage(1);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }

            if (!allDots[column, row].GetComponent<Dot>().deadbyBomb)
            {
                //ConcreteTile
                DamageConcrete(column, row);
                //SlimeTile
                DamageSlime(column, row);
            }


            //점수 카운팅
            if (goalManager != null && currentState != GameState.win)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }
            //end

            //Does that sound manager exist?
            if (soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }
            //end

            GameObject particle = Instantiate(destroyParticle, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            allDots[column, row].GetComponent<Dot>().PopAnimation(); //터지는 애니메이션 부여            
            Destroy(allDots[column, row], .4f);
            scoreManager.IncreaseScore(basePieceValue * streakValue); // 점수 = 기본점수 * 연속점수
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        //How many elements are in the matched pieces list from findmatches?

        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        StartCoroutine(DecreaseRowCo2());
    }

    private void MakeNormaltype(int column, int row)
    {
        int key = 0;

        for (int i = 0; i < height; i++)
        {
            if (slimeTiles[column, i] || concreteTiles[column, i])
            {
                key = i;
            }
        }

        if (key == 0)
        {
            for (int k = 0; k < row; k++)
            {
                blankSpaces[column, k] = false;
            }
        }
        else
        {
            for (int k = 0; k < key; k++)
            {
                blankSpaces[column, k] = true;
            }
        }
    }

    private void DamageConcrete(int column, int row)
    {
        if (column > 0)
        {
            if (concreteTiles[column - 1, row])
            {
                concreteTiles[column - 1, row].TakeDamage(1);
                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
                MakeNormaltype(column - 1, row);
            }
        }
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);
                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
                MakeNormaltype(column + 1, row);
            }
        }
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);
                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
                MakeNormaltype(column, row - 1);

            }
        }
        if (row < height - 1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);
                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column, row + 1] = null;
                }
                MakeNormaltype(column, row + 1);
            }
        }



    }

    private void DamageSlime(int column, int row)
    {
        if (column > 0)
        {
            if (slimeTiles[column - 1, row])
            {
                slimeTiles[column - 1, row].TakeDamage(1);
                if (slimeTiles[column - 1, row].hitPoints <= 0)
                {
                    slimeTiles[column - 1, row] = null;
                }
                makeSlime = false;
                MakeNormaltype(column - 1, row);

            }
        }
        if (column < width - 1)
        {
            if (slimeTiles[column + 1, row])
            {
                slimeTiles[column + 1, row].TakeDamage(1);
                if (slimeTiles[column + 1, row].hitPoints <= 0)
                {
                    slimeTiles[column + 1, row] = null;
                }
                makeSlime = false;
                MakeNormaltype(column + 1, row);
            }
        }
        if (row > 0)
        {
            if (slimeTiles[column, row - 1])
            {
                slimeTiles[column, row - 1].TakeDamage(1);
                if (slimeTiles[column, row - 1].hitPoints <= 0)
                {
                    slimeTiles[column, row - 1] = null;
                }
                makeSlime = false;
                MakeNormaltype(column, row - 1);
            }
        }
        if (row < height - 1)
        {
            if (slimeTiles[column, row + 1])
            {
                slimeTiles[column, row + 1].TakeDamage(1);
                if (slimeTiles[column, row + 1].hitPoints <= 0)
                {
                    slimeTiles[column, row + 1] = null;
                }
                makeSlime = false;
                MakeNormaltype(column, row + 1);
            }
        }
    }

    private IEnumerator DecreaseRowCo2()
    {
        yield return new WaitForSeconds(refillDealy);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                //if the current spot isn't blank and is empty. . . 
                if (allDots[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    //loop from the space above to the top of the column
                    for (int k = j + 1; k < height; k++)
                    {
                        //if a dot is found. . .
                        if (allDots[i, k] != null)
                        {
                            //move that dot to this empty space
                            allDots[i, k].GetComponent<Dot>().row = j;
                            //set that spot to be null
                            allDots[i, k] = null;
                            //break out of the loop;
                            break;
                        }
                    }
                }
            }
        }
        //yield return new WaitForSeconds(refillDealy * 0.5f);

        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                    allDots[i, j].name = "( " + i + ", " + j + " )";
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        findMatches.FindAllMatches();
        Debug.Log("count : " + findMatches.currentMatches.Count);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(refillDealy);
        RefillBoard();
        yield return new WaitForSeconds(refillDealy);
        while (MatchesOnBoard())
        {
            streakValue++; //연속 점수 증가
            DestroyMatches();
            yield break;
        }

        currentDot = null;
        CheckToMakeSlime();

        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
        }

        yield return new WaitForSeconds(refillDealy);
        System.GC.Collect();
        if (currentState != GameState.pause && currentState != GameState.win)
            currentState = GameState.move;
        makeSlime = true;
        streakValue = 1; //연속 점수 초기화

        if (currentState == GameState.win && cnt < 2)
        {
            if (cnt == 0)
            {
                yield return new WaitForSeconds(1f);
                WinBomb();
            }


            if (cnt == 1)
            {
                yield return new WaitForSeconds(0.5f);

                endgameManager.RealWinGame();
            }

            cnt++;
        }


    }

    private void CheckToMakeSlime()
    {
        //Check the slime tiles array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (slimeTiles[i, j] != null && makeSlime)
                {
                    //Call another method to make a new slime
                    MakeNewSlime();
                    return;
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int column, int row)
    {
        if (column < width - 1 && allDots[column + 1, row])
        {
            return Vector2.right;
        }
        else if (column > 0 && allDots[column - 1, row])
        {
            return Vector2.left;
        }
        else if (row < height - 1 && allDots[column, row + 1])
        {
            return Vector2.up;
        }
        else if (row > 0 && allDots[column, row - 1])
        {
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewSlime()
    {
        bool slime = false;
        int loops = 0;
        while (!slime && loops < 100000)
        {
            int newX = Random.Range(0, width);
            int newY = Random.Range(0, height);
            if (slimeTiles[newX, newY] != null)
            {
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                if (adjacent != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(SlimePiecePrefab, tempPosition, Quaternion.identity);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    slime = true;
                }
            }
            loops++;
        }
        Debug.Log("slime loop" + loops);
    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        if (allDots[column + (int)direction.x, row + (int)direction.y] != null)
        {
            //take the first piece and save it in a holder
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            //switching the first dot to be second position
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            //set the first dot to be the second dot
            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //make sure that one and two to the right are in the board
                    if (i < width - 2)
                    {
                        //check if the dots to the right and two to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                    if (j < height - 2)
                    {
                        //check if the dots above exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(0.5f);
        //Create a list of game objects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to this list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        //for every spot on the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if (!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    //pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    //assign the column to the piece
                    int maxIterations = 0;

                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    //Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;

                    piece.column = i;
                    //assign the row to the piece
                    piece.row = j;
                    //fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        //check if it's still deadlocked
        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
        }

    }

}

