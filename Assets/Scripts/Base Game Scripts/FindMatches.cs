using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{

    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }

        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }

        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }
        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
            board.BombRow(dot1.row);
        }

        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
            board.BombRow(dot2.row);
        }

        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
            board.BombRow(dot3.row);
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
            board.BombColumn(dot1.column);
        }

        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
            board.BombColumn(dot2.column);
        }

        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
            board.BombColumn(dot3.column);
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        //yield return new WaitForSeconds(.2f);
        yield return null;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];

                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];

                        GameObject rightDot = board.allDots[i + 1, j];

                        if (leftDot != null && rightDot != null)
                        {
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }

                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];

                        GameObject downDot = board.allDots[i, j - 1];


                        if (upDot != null && downDot != null)
                        {
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }

        yield return null;


    }

    /*public void MatchPiecesOfColor(string pieceNum)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allDots[i, j].tag == pieceNum)
                    {
                        //Set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }*/

    public List<GameObject> GetColorPieces(int column, int row, string pieceNum)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allDots[i, j].tag == pieceNum)
                    {
                        Dot dot = board.allDots[i, j].GetComponent<Dot>();

                        if (dot.isRowBomb) //Row밤 연쇄
                        {
                            dots.Union(GetRowPieces(j)).ToList();
                        }
                        if (dot.isColumnBomb) //Column밤 연쇄
                        {
                            dots.Union(GetColumnPieces(i)).ToList();
                        }
                        if (dot.isAdjacentBomb) //33밤 연쇄
                        {
                            dots.Union(GetAdjacentPieces(i, j)).ToList();
                        }

                        //Set that dot to be matched
                        dots.Add(board.allDots[i, j]);                                               
                        dot.isMatched = true;
                        dot.deadbyBomb = true;
                    }
                }
            }
        }                
        return dots;
    }

    public List<GameObject> GetColorBombDestroy(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                //Check if the piece is inside the board
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allDots[i, j] != null)
                    {                               
                        dots.Add(board.allDots[i, j]);
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }

        if (column - 2 >= 0)
        {
            if (board.allDots[column - 2, row] != null)
            {
                board.allDots[column - 2, row].GetComponent<Dot>().isMatched = true;
            }
        }
        if (column + 2 < board.width)
        {
            if (board.allDots[column + 2, row] != null)
            {
                board.allDots[column + 2, row].GetComponent<Dot>().isMatched = true;
            }
        }
        if (row - 2 >= 0)
        {
            if (board.allDots[column, row - 2] != null)
            {
                board.allDots[column, row - 2].GetComponent<Dot>().isMatched = true;
            }
        }
        if (row + 2 < board.height)
        {
            if (board.allDots[column, row + 2] != null)
            {
                board.allDots[column, row + 2].GetComponent<Dot>().isMatched = true;
            }
        }

        return dots;
    }
    
    public List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                //Check if the piece is inside the board
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if(board.allDots[i,j] != null)
                    {
                        //폭탄 연쇄
                        Dot dot = board.allDots[i, j].GetComponent<Dot>();
                        if (dot.isRowBomb) //로우밤 연쇄
                        {
                            dots.Union(GetRowPieces(j)).ToList();
                        }
                        if (dot.isColumnBomb) //33밤 연쇄(내가 만든거 문제시 삭제)
                        {
                            dots.Union(GetColumnPieces(i)).ToList();
                        }
                        if (dot.isColorBomb)
                        {
                            dots.Union(GetColorBombDestroy(i, j)).ToList();
                        }

                        //폭탄 연쇄 end

                        dots.Add(board.allDots[i, j]);
                        dot.isMatched = true;
                    }                    
                }
            }
        }
        return dots;
    }

    public List<GameObject> GetColumnPieces(int column)
    {
        Dot dot;

        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                //폭탄 연쇄
                dot = board.allDots[column, i].GetComponent<Dot>();
                if (dot.isRowBomb) //로우밤 연쇄
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }
                if (dot.isAdjacentBomb) //33밤 연쇄(내가 만든거 문제시 삭제)
                {
                    dots.Union(GetAdjacentPieces(column, i)).ToList();
                }
                if (dot.isColorBomb)
                {
                    dots.Union(GetColorBombDestroy(column, i)).ToList();
                }

                //폭탄 연쇄 end

                dots.Add(board.allDots[column, i]);
                dot.isMatched = true;
                dot.deadbyBomb = true;
            }
        }
        return dots;
    }

    public List<GameObject> GetRowPieces(int row)
    {
        Dot dot;

        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                //폭탄 연쇄
                dot = board.allDots[i, row].GetComponent<Dot>();
                if (dot.isColumnBomb) //컬럼밤 연쇄
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                if (dot.isAdjacentBomb) //33밤 연쇄(내가 만든거 문제시 삭제)
                {
                    dots.Union(GetAdjacentPieces(i, row)).ToList();
                }
                if (dot.isColorBomb)
                {
                    dots.Union(GetColorBombDestroy(i, row)).ToList();
                }
                
                //폭탄 연쇄 end

                dots.Add(board.allDots[i, row]);
                dot.isMatched = true;
                dot.deadbyBomb = true;
            }
        }
        return dots;
    }

    public List<GameObject> GetColorXAdjacent(int column, int row, string pieceNum)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allDots[i, j].tag == pieceNum)
                    {
                        Dot dot = board.allDots[i, j].GetComponent<Dot>();



                        //Set that dot to be matched
                        dots.Add(board.allDots[i, j]);
                        dot.isAdjacentBomb = true;
                        dot.isMatched = true;
                        dot.deadbyBomb = true;
                    }
                }
            }
        }
        return dots;
    }


    public List<GameObject> GetColorXColumn(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int k = column -1; k < column + 2; k++)
        {
            for (int i = 0; i < board.height; i++)
            {
                if (k >= 0 && k < board.width)
                {
                    if (board.allDots[k, i] != null)
                    {
                        //폭탄 연쇄
                        Dot dot = board.allDots[k, i].GetComponent<Dot>();
                        if (dot.isRowBomb) //로우밤 연쇄
                        {
                            dots.Union(GetRowPieces(i)).ToList();
                        }
                        if (dot.isAdjacentBomb) //33밤 연쇄(내가 만든거 문제시 삭제)
                        {
                            dots.Union(GetAdjacentPieces(k, i)).ToList();
                        }
                        //폭탄 연쇄 end

                        dots.Add(board.allDots[k, i]);
                        dot.isMatched = true;
                        dot.deadbyBomb = true;
                    }
                }                    
            }
        }        
        return dots;
    }

    public List<GameObject> GetColorXRow(int row)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int k = row - 1; k < row + 2; k++)
        {
            for (int i = 0; i < board.width; i++)
            {
                if (k >= 0 && k < board.height)
                {
                    if (board.allDots[i, k] != null)
                    {
                        //폭탄 연쇄
                        Dot dot = board.allDots[i, k].GetComponent<Dot>();
                        if (dot.isColumnBomb) //컬럼밤 연쇄
                        {
                            dots.Union(GetColumnPieces(i)).ToList();
                        }
                        if (dot.isAdjacentBomb) //33밤 연쇄(내가 만든거 문제시 삭제)
                        {
                            dots.Union(GetAdjacentPieces(i, k)).ToList();
                        }
                        //폭탄 연쇄 end

                        dots.Add(board.allDots[i, k]);
                        dot.isMatched = true;
                        dot.deadbyBomb = true;
                    }
                }                
            }

        }
        return dots;
    }


    public void CheckBombs(MatchType matchType)
    {
        //Did the player move something?
        if (board.currentDot != null)
        {
            //Is the piece they moved matched?
            if (board.currentDot.isMatched && board.currentDot.tag == matchType.color)
            {
                //make it unmatched
                board.currentDot.isMatched = false;

                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                   || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                }
                else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            //Is the other piece matched?
            else if (board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                //Is the other Dot matched?
                if (otherDot.isMatched && otherDot.tag == matchType.color)
                {
                    //Make it unmatched
                    otherDot.isMatched = false;

                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                   || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }

        }
    }

}

