using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Dot : MonoBehaviour
{
    public int column = 0;
    public int row = 0;
    public float targetX;
    public float targetY;
    public bool isMatched = false;
    
    private Board mBoard;

    private FindMatches findMatches;
    private Vector2 mTmpPos;

    public bool isRowBomb;
    public GameObject rowArrow;

    // Start is called before the first frame update
    void Start()
    {
        mBoard = GameObject.FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();

        isRowBomb = false;

    }

    private void Update()
    {
        UpdateDotPos();
    }
    
    

    void UpdateDotPos()
    {
        targetX = mBoard.tiles[column][row].x;
        targetY = mBoard.tiles[column][row].y;

        if (Mathf.Abs(targetX - transform.position.x) > 0.1f) 
        {
            mTmpPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, mTmpPos, 0.5f);
            if (mBoard.dots[column][row] != this.gameObject)
            {
                mBoard.dots[column][row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            mTmpPos = new Vector2(targetX, transform.position.y);
            transform.position = mTmpPos;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1f) 
        {
            mTmpPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, mTmpPos, 0.5f);
            if (mBoard.dots[column][row] != this.gameObject)
            {
                mBoard.dots[column][row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            mTmpPos = new Vector2(transform.position.x, targetY);
            transform.position = mTmpPos;
        }
    }

    public void MakeRowBomb()
    {
        if(this.tag != "Obstacle")
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

}
