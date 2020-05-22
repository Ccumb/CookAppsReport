using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hexa
{
    [DisallowMultipleComponent]
    // 블럭 하나를 의미, 행, 열값이 바뀔때 블럭의 움직임을 담당.
    public class Dot : MonoBehaviour
    {
        public int column = 0;      // 열
        public int row = 0;         // 행
        public float targetX;       // 목표 좌표
        public float targetY;
        public bool isMatched = false;  // 매치 여부

        private Board mBoard;

        private FindMatches findMatches;
        private Vector2 mTmpPos;

        public bool isRowBomb;      // 특수블럭?
        public GameObject rowArrow;

        // Board와 FindMatches 저장, 시작시는 일반블럭이니까 false
        void Start()
        {
            mBoard = GameObject.FindObjectOfType<Board>();
            findMatches = FindObjectOfType<FindMatches>();

            isRowBomb = false;

        }

        // 좌표 업데이트
        private void Update()
        {
            UpdateDotPos();
        }



        void UpdateDotPos()
        {
            // column, row가 바뀔 때마다 목표 좌표 수정
            targetX = mBoard.tiles[column][row].x;
            targetY = mBoard.tiles[column][row].y;

            // 목표 좌표와 현재 위치가 같지 않으면 이동
            if (Mathf.Abs(targetX - transform.position.x) > 0.1f)
            {
                mTmpPos = new Vector2(targetX, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, mTmpPos, 0.5f);
                if (mBoard.dots[column][row] != this.gameObject)
                {
                    mBoard.dots[column][row] = this.gameObject;
                }
                // 이동 후 매치 된 것들 찾기
                findMatches.FindAllMatches();
            }
            // 아니면 가만히
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

        // 자기 자신이 장애물이 아니고, 특수블럭 상태면 특수블럭으로.
        public void MakeRowBomb()
        {
            if (this.tag != "Obstacle")
            {
                isRowBomb = true;
                GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
                arrow.transform.parent = this.transform;
            }
        }

    }
}
