using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hexa
{
    // 위치를 헷갈리지 않기 위해
    public enum Dir
    {
        UP,
        RIGHT_UP,
        RIGHT_DOWN,
        DOWN,
        LEFT_DOWN,
        LEFT_UP
    }

    [DisallowMultipleComponent]
    // 노드, 주변 노드 정보를 담고 터치도 담당
    public class Node : MonoBehaviour
    {
        // 내 위치에 있는 dot
        public GameObject dot;

        // 현재 이동 방향
        private Dir currentDir;

        public int column;
        public int row;

        // 주위 6방향 노드들
        public GameObject[] nearNodes;

        public float swipAngle = 0;
        public float swipResist = 1.0f;

        private Vector2 mFirstTouchPos;
        private Vector2 mFinalTouchPos;
        private Board mBoard;
        private GameObject mOtherDot;

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

        // 터치가 이루어져
        private void Update()
        {
            if (mBoard.currentState == GameState.move && mBoard.currentState != GameState.clear)
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

        // 6방향 주변 노드들 검색 및 저장
        public void SearchNearDots()
        {
            SearchUpDot();
            SearchDownDot();
            SearchRightUpDot();
            SearchLeftUpDot();
            SearchLeftDown();
            SearchRightDown();
        }

        // 위
        void SearchUpDot()
        {
            // rayMask로 node끼리만 검사
            int layerMask = 1 << LayerMask.NameToLayer("Node");

            Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.y + 0.5f);
            RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.up, this.transform.localScale.x * 0.5f, layerMask);
            if (hit)
            {
                Dot nearDot = hit.transform.gameObject.GetComponent<Dot>();
                nearNodes[(int)Dir.UP] = hit.transform.gameObject;
            }
        }


        // 아래
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

        // 우상
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

        // 좌상
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

        // 우하
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

        // 좌하
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

        // 각도 계산    
        void CalculateAngle()
        {
            // 현재 터치 지점과 마지막 터치 지점이 다르면
            if (Mathf.Abs(mFinalTouchPos.y - mFirstTouchPos.y) > swipResist
                || Mathf.Abs(mFinalTouchPos.x - mFirstTouchPos.x) > swipResist)
            {
                // 상태를 정지로 == 다른 블럭들 못움직이게
                mBoard.currentState = GameState.wait;
                // 각도를 계산하고
                swipAngle = Mathf.Atan2(mFinalTouchPos.y - mFirstTouchPos.y, mFinalTouchPos.x - mFirstTouchPos.x) * Mathf.Rad2Deg;
                // 움직이고
                MovePieces();

                // 현재 블럭을 이 블럭으로
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

        // 실질적으로 움직이는 함수
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
                // 다시 원위치 시킬때를 대비해서 저장해두기
                mOtherDot = nearNodes[(int)dir].GetComponent<Node>().dot;

                StartCoroutine("CheckMoveCo");
            }
            else
            {
                mBoard.currentState = GameState.move;
            }
        }

        // 옮긴 후 매치되지 않으면 다시 제자리로
        public IEnumerator CheckMoveCo()
        {
            yield return new WaitForSeconds(0.5f);

            // 매치된 것들을 전부 없애고
            mBoard.DestroyMatches();

            // 해당 방향의 노드에 있는 dot 불러오기
            GameObject other = nearNodes[(int)currentDir].GetComponent<Node>().dot;

            // 만약 dot이 존재하고, 바꾸려는 블럭이 원래의 내가 갖고있던 블럭이 맞으면 원상복구
            if (other != null && mOtherDot == other)
            {
                GameObject tmp = dot;
                dot = nearNodes[(int)currentDir].GetComponent<Node>().dot;
                nearNodes[(int)currentDir].GetComponent<Node>().dot = tmp;

                dot.GetComponent<Dot>().column = this.column;
                dot.GetComponent<Dot>().row = this.row;

                if (nearNodes[(int)currentDir].GetComponent<Node>().dot != null)
                {
                    nearNodes[(int)currentDir].GetComponent<Node>().dot.GetComponent<Dot>().column = nearNodes[(int)currentDir].GetComponent<Node>().column;
                    nearNodes[(int)currentDir].GetComponent<Node>().dot.GetComponent<Dot>().row = nearNodes[(int)currentDir].GetComponent<Node>().row;
                }
            }
            else
            {
                //mBoard.DestroyMatches();
            }
        }
    }
}

