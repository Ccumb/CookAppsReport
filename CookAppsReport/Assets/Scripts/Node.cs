using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir
{
    UP,
    RIGHT_UP,
    RIGHT_DOWN,
    DOWN,
    LEFT_DOWN,
    LEFT_UP
}

public class Node : MonoBehaviour
{
    public GameObject dot;
    //public GameObject mOtherDot;
    private Dir currentDir;

    public int column;
    public int row;

    public GameObject[] nearNodes;

    public float swipAngle = 0;
    public float swipResist = 1.0f;

    private Vector2 mFirstTouchPos;
    private Vector2 mFinalTouchPos;
    private Board mBoard;

    private Touch touch;
    private bool touchOn;

    // Start is called before the first frame update
    void Awake()
    {
        mBoard = GameObject.FindObjectOfType<Board>();
        nearNodes = new GameObject[6];
        
    }
    private void Start()
    {
        SearchNearDots();
    }

    private void Update()
    {
        if(mBoard.currentState == GameState.move && mBoard.currentState != GameState.clear)
        {
            touchOn = false;

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    touch = Input.GetTouch(i);

                    if (touch.phase == TouchPhase.Began)
                    {
                        if (mBoard.currentState == GameState.move)
                        {
                            mFirstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        }
                    }
                    if (touch.phase == TouchPhase.Ended)
                    {
                        if (mBoard.currentState == GameState.move)
                        {
                            mFinalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            CalculateAngle();
                        }
                    }
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (mBoard.currentState == GameState.move && mBoard.currentState != GameState.clear)
        {
            mFirstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (mBoard.currentState == GameState.move && mBoard.currentState != GameState.clear)
        {
            mFinalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    public void SearchNearDots()
    {
        SearchUpDot();
        SearchDownDot();
        SearchRightUpDot();
        SearchLeftUpDot();
        SearchLeftDown();
        SearchRightDown();
    }

    void SearchUpDot()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.y + 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.up, this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.UP] = hit.transform.gameObject;
        }
    }

    void SearchDownDot()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.down, this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.DOWN] = hit.transform.gameObject;
        }
    }

    void SearchRightUpDot()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x + 0.5f, this.transform.position.y + 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, new Vector2(1, 1), this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.RIGHT_UP] = hit.transform.gameObject;
        }
    }

    void SearchLeftUpDot()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x - 0.5f, this.transform.position.y + 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, new Vector2(-1, 1), this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.LEFT_UP] = hit.transform.gameObject;
        }
    }

    void SearchRightDown()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x + 0.5f, this.transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, new Vector2(1, -1), this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.RIGHT_DOWN] = hit.transform.gameObject;
        }
    }

    void SearchLeftDown()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Node");

        Vector2 startPos = new Vector2(this.transform.position.x - 0.5f, this.transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(startPos, new Vector2(-1, -1), this.transform.localScale.x * 0.5f, layerMask);
        if (hit)
        {
            Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
            nearNodes[(int)Dir.LEFT_DOWN] = hit.transform.gameObject;
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(mFinalTouchPos.y - mFirstTouchPos.y) > swipResist
            || Mathf.Abs(mFinalTouchPos.x - mFirstTouchPos.x) > swipResist)
        {
            mBoard.currentState = GameState.wait;
            swipAngle = Mathf.Atan2(mFinalTouchPos.y - mFirstTouchPos.y, mFinalTouchPos.x - mFirstTouchPos.x) * Mathf.Rad2Deg;
            MovePieces();

            mBoard.currentDot = dot.GetComponent<Dot>();
        }
        else
        {
            mBoard.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if ((swipAngle > 0 && swipAngle < 55))                // 우상단
        {
            MoveActual(Dir.RIGHT_UP);
        }
        else if ((swipAngle >= 55 && swipAngle < 125))      // 위
        {
            MoveActual(Dir.UP);
        }
        else if ((swipAngle >= 125 && swipAngle < 180))     // 좌상단
        {
            MoveActual(Dir.LEFT_UP);
        }
        else if ((swipAngle < 0 && swipAngle >= -55))       // 우하단
        {
            MoveActual(Dir.RIGHT_DOWN);
        }
        else if ((swipAngle < -55 && swipAngle >= -125))    // 아래
        {
            MoveActual(Dir.DOWN);
        }
        else if ((swipAngle < -125 && swipAngle >= -180))   // 좌하단
        {
            MoveActual(Dir.LEFT_DOWN);
        }
        else
        {
            mBoard.currentState = GameState.move;
        }

    }

    void MoveActual(Dir dir)
    {
        if (nearNodes[(int)dir] != null && nearNodes[(int)dir].GetComponent<Node>().dot != null)
        {
            GameObject tmp = dot;
            dot = nearNodes[(int)dir].GetComponent<Node>().dot;
            nearNodes[(int)dir].GetComponent<Node>().dot = tmp;

            dot.GetComponent<Dot>().column = this.column;
            dot.GetComponent<Dot>().row = this.row;

            nearNodes[(int)dir].GetComponent<Node>().dot.GetComponent<Dot>().column = nearNodes[(int)dir].GetComponent<Node>().column;
            nearNodes[(int)dir].GetComponent<Node>().dot.GetComponent<Dot>().row = nearNodes[(int)dir].GetComponent<Node>().row;
    
            currentDir = dir;
            
            StartCoroutine("CheckMoveCo");
        }
        else
        {
            mBoard.currentState = GameState.move;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(0.5f);
        mBoard.DestroyMatches();
        GameObject other = nearNodes[(int)currentDir].GetComponent<Node>().dot;

        if (other != null)
        {
            GameObject tmp = dot;
            dot = nearNodes[(int)currentDir].GetComponent<Node>().dot;
            nearNodes[(int)currentDir].GetComponent<Node>().dot = tmp;

            dot.GetComponent<Dot>().column = this.column;
            dot.GetComponent<Dot>().row = this.row;

            if(nearNodes[(int)currentDir].GetComponent<Node>().dot != null)
            {
                nearNodes[(int)currentDir].GetComponent<Node>().dot.GetComponent<Dot>().column = nearNodes[(int)currentDir].GetComponent<Node>().column;
                nearNodes[(int)currentDir].GetComponent<Node>().dot.GetComponent<Dot>().row = nearNodes[(int)currentDir].GetComponent<Node>().row;
            }
        }
        else
        {
            mBoard.DestroyMatches();
        }
    }


}

