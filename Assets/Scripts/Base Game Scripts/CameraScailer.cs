using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScailer : MonoBehaviour
{
    private Board board;
    public float cameraOffset;
    public float aspectRatio = 0.625f;
    public float padding = 2;

    // Use this for initialization
    void Start()
    {
        board = FindObjectOfType<Board>();
        if (board != null)
        {
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new Vector3(x / 2, y / 2, cameraOffset);
        transform.position = tempPosition;

        /* 스마트 스케일러(오브젝트 크기가 맵 크기에 따라 달라져서 사용안함 일단)
        if (board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
        */

        Camera.main.orthographicSize = 6.5f + padding;


    }


}
