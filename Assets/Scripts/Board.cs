using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

/// <summary>
/// 실행시 최초 블록을 만들어주는 스크립트
/// </summary>

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    private FindMatches findMatches;

    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        Setup();
    }

    private void Setup()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")"; // 오브젝트 명 (i,j)로 지정
                int dotToUse = Random.Range(0, dots.Length); // dots 오브젝트의 길이(갯수)만큼 랜덤 변수 생성
                int maxIterations = 0; // 최초 블록 생성 리셋 제한 갯수 초기화

                while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {//3개연속 동일한 블록이 나오지 않을때까지, 리셋 제한 갯수가 100까지 랜덤변수를 재생성한다.
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                    Debug.Log(maxIterations);
                }
                maxIterations = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity); //(i,j)에 정보 기입
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;

                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece) //3개연속 동일한 블록이 있는지 검사하는 함수
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }

            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            findMatches.currentMatches.Remove(allDots[column, row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowco());
    }

    private IEnumerator DecreaseRowco()
    {
        int nullCount = 0;
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;

                }
                else if(nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    piece.transform.parent = this.transform; //새로 생성된 블록을 보드로 상속시켜준다(클론이 생성되지 않음)
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;

                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
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
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
