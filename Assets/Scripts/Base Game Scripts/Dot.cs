using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour //Block Action Class
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    public bool deadbyBomb = false;
    public GameObject otherDot;

    private Animator anim; 
    private EndGameManager endGameManager;
    private HintManager hintManager;
    private FindMatches findMatches;
    private Board board;
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;

    // Use this for initialization
    void Start()
    {
        //Bomb Action init
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        //Get Other Class and Setting
        anim = GetComponent<Animator>();
        endGameManager = FindObjectOfType<EndGameManager>();
        hintManager = FindObjectOfType<HintManager>();
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        findMatches = FindObjectOfType<FindMatches>();
    }

    //테스터용 입력
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }

        if (Input.GetKeyDown("1"))
        {
            isColumnBomb = true;
            GameObject marker = Instantiate(columnArrow, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }

        if (Input.GetKeyDown("2"))
        {
            isRowBomb = true;
            GameObject marker = Instantiate(rowArrow, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }

        if (Input.GetKeyDown("3"))
        {
            isColorBomb = true;
            GameObject marker = Instantiate(colorBomb, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    void Update()
    {      
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public void PopAnimation()
    {
        anim.SetBool("Popped", true);
    }

    public void StarAnimation()
    {
        anim.SetBool("StarBomb", true);
    }

    //Check this match is colrbomb or not
    public void CheckColorBomb()
    {
        Dot tempDot = otherDot.GetComponent<Dot>();

        if (isColorBomb)
        {
            if (tempDot.isAdjacentBomb)
            {
                findMatches.GetColorXAdjacent(tempDot.column, tempDot.row, tempDot.tag);
            }
            if (tempDot.isColumnBomb)
            {
                findMatches.GetColorXColumn(tempDot.column);
            }
            if (tempDot.isRowBomb)
            {
                findMatches.GetColorXRow(tempDot.row);
            }
            findMatches.GetColorPieces(tempDot.column, tempDot.row, tempDot.tag);
            isMatched = true;
        }
        else if (tempDot.isColorBomb)
        {
            if (this.isAdjacentBomb)
            {
                findMatches.GetColorXAdjacent(this.column, this.row, this.gameObject.tag);
            }
            if (this.isColumnBomb)
            {
                findMatches.GetColorXColumn(this.column);
            }
            if (this.isRowBomb)
            {
                findMatches.GetColorXRow(this.row);
            }
            findMatches.GetColorPieces(this.column, this.row, this.gameObject.tag);
            tempDot.isMatched = true;
            tempDot.deadbyBomb = true;
        }
    }

    //Check this block is moveable or not
    public IEnumerator CheckMoveCo()
    {
        CheckColorBomb();
        yield return new WaitForSeconds(.5f);

        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();
            }
        }
    }

    private void OnMouseDown()
    {
        //Destroy hint
        if(hintManager != null)
        {
            hintManager.DestroyHint();
        }

        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (board.lockTiles[column, row] == null && board.lockTiles[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if (otherDot != null)
            {
                otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
                otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
                column += (int)direction.x;
                row += (int)direction.y;
                StartCoroutine(CheckMoveCo());
            }
            else
            {
                board.currentState = GameState.move;
            }
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //Right
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Up
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    public void MakeRowBomb()
    {
        if(!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }        
    }

    public void MakeColumnBomb()
    {
        if(!isRowBomb && !isColorBomb && !isAdjacentBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

    public void MakeColorBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isAdjacentBomb)
        {
            isColorBomb = true;            
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);            
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
            this.StarAnimation();
        }
    }

    public void MakeAdjacentBomb()
    {
        if(!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }
}