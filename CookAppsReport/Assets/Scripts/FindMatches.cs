using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hexa
{
    [DisallowMultipleComponent]
    // 현재 블럭들이 매치되었는가를 판단하고 이를 저장, 특수블럭 처리 담당
    public class FindMatches : MonoBehaviour
    {
        private Board mBorad;
        public List<GameObject> currentMatches = new List<GameObject>();    // 현재 매치된 것들을 담는 List

        // Board 저장
        void Start()
        {
            mBorad = FindObjectOfType<Board>();
        }

        // 매치를 찾는 코루틴 실행
        public void FindAllMatches()
        {
            StartCoroutine("FindAllMatchesCo");
        }

        // 매치를 리스트에 저장
        private void AddToListAndMatch(GameObject dot)
        {
            // 리스트에 들어있지 않으면
            if (!currentMatches.Contains(dot))
            {
                // 해당 블럭 추가
                currentMatches.Add(dot);
            }
            // 블럭의 매치 여부 true
            dot.GetComponent<Dot>().isMatched = true;
        }

        // 3개의 블럭들을 함께 담기 위해 만든 함수
        private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
        {
            AddToListAndMatch(dot1);
            AddToListAndMatch(dot2);
            AddToListAndMatch(dot3);
        }

        // 모든 매치된 것들 찾기
        private IEnumerator FindAllMatchesCo()
        {
            // 0.2초 후
            yield return new WaitForSeconds(0.2f);

            for (int i = 0; i < mBorad.nodes.Count; i++)
            {
                for (int j = 0; j < mBorad.nodes[i].Count; j++)
                {
                    Node currentDot_node = mBorad.nodes[i][j].GetComponent<Node>();
                    GameObject currentDot = currentDot_node.GetComponent<Node>().dot;

                    // 해당 노드의 블럭이 비어있지 않고, 장애물이 아니면
                    if (currentDot != null && currentDot.tag != "Obstacle")
                    {
                        // 해당 노드와 연결된 위, 아래 노드를 검사
                        GameObject upDot_node = currentDot_node.nearNodes[(int)Dir.UP];
                        GameObject downDot_node = currentDot_node.nearNodes[(int)Dir.DOWN];

                        // 위, 아래 노드가 존재하면
                        if (upDot_node != null && downDot_node != null)
                        {
                            // 상 하
                            GameObject upDot = upDot_node.GetComponent<Node>().dot;
                            GameObject downDot = downDot_node.GetComponent<Node>().dot;

                            // 위 아래 노드의 블럭이 존재하면
                            if (upDot != null && downDot != null)
                            {
                                // 위 아래 노드의 블럭의 태그가 현재 검사중인 블럭의 태그와 같다면
                                if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                                {
                                    // 해당 블럭들 중에 특수블럭이 있다면, 이 특수블럭의 범위 내의 블럭들까지 리스트에 합쳐 (Union - 합집합)
                                    currentMatches.Union(IsRowBomb(upDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), downDot.GetComponent<Dot>()));
                                    // 검사된 블럭들을 리스트에 집어넣어
                                    GetNearbyPieces(upDot, currentDot, downDot);
                                }
                            }
                        }

                        GameObject leftUpDot_node = currentDot_node.nearNodes[(int)Dir.LEFT_UP];
                        GameObject rightDownDot_node = currentDot_node.nearNodes[(int)Dir.RIGHT_DOWN];

                        if (leftUpDot_node != null && rightDownDot_node != null)
                        {
                            // 좌상 우하
                            GameObject leftUpDot = leftUpDot_node.GetComponent<Node>().dot;
                            GameObject rightDownDot = rightDownDot_node.GetComponent<Node>().dot;

                            if (leftUpDot != null && rightDownDot != null)
                            {
                                if (leftUpDot.tag == currentDot.tag && rightDownDot.tag == currentDot.tag)
                                {
                                    currentMatches.Union(IsRowBomb(leftUpDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), rightDownDot.GetComponent<Dot>()));

                                    GetNearbyPieces(leftUpDot, currentDot, rightDownDot);
                                }
                            }
                        }

                        GameObject rightUpDot_node = currentDot_node.nearNodes[(int)Dir.RIGHT_UP];
                        GameObject leftDownDot_node = currentDot_node.nearNodes[(int)Dir.LEFT_DOWN];

                        if (rightUpDot_node != null && leftDownDot_node != null)
                        {
                            // 우상 좌하
                            GameObject rightUpDot = rightUpDot_node.GetComponent<Node>().dot;
                            GameObject leftDownDot = leftDownDot_node.GetComponent<Node>().dot;

                            if (rightUpDot != null && leftDownDot != null)
                            {
                                if (rightUpDot.tag == currentDot.tag && leftDownDot.tag == currentDot.tag)
                                {
                                    currentMatches.Union(IsRowBomb(rightUpDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), leftDownDot.GetComponent<Dot>()));
                                    GetNearbyPieces(rightUpDot, currentDot, leftDownDot);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 폭탄인지 확인 후 터트리기
        private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
        {
            List<GameObject> currentDots = new List<GameObject>();

            // dot1이 특수블럭이면
            if (dot1.isRowBomb)
            {
                // 현재 매치에 해당 범위의 블럭들을 합쳐넣어
                currentMatches.Union(GetRowPieces(dot1.column));
            }

            if (dot2.isRowBomb)
            {
                currentMatches.Union(GetRowPieces(dot2.column));
            }

            if (dot3.isRowBomb)
            {
                currentMatches.Union(GetRowPieces(dot3.column));
            }

            return currentDots;
        }

        // 상하 방향으로 터트리기
        // union - 합집합, 상하 방향의 것들을 matched시키고 currentMatches에 있는 것들과 합쳐.
        List<GameObject> GetRowPieces(int row)
        {
            List<GameObject> dots = new List<GameObject>();

            // 세로 방향의 것들의 매치 상태를 전부 true로
            for (int i = 0; i < mBorad.dots[row].Count; i++)
            {
                if (mBorad.dots[row][i] != null)
                {
                    Dot dot = mBorad.dots[row][i].GetComponent<Dot>();

                    dots.Add(mBorad.dots[row][i]);
                    dot.isMatched = true;
                }
            }

            return dots;
        }

        // 폭탄 만들기
        public void CheckBombs()
        {
            // 현재 블럭이 존재하면 == 블럭을 내가 움직였을 때 
            if (mBorad.currentDot != null)
            {
                int column = mBorad.currentDot.column;
                int row = mBorad.currentDot.row;

                Debug.Log(column + ", " + row);

                // 매치상태라면
                if (mBorad.currentDot.isMatched)
                {
                    // 매치상태를 false로
                    mBorad.currentDot.isMatched = false;

                    // 세로 폭탄을 만들어줘
                    mBorad.currentDot.MakeRowBomb();
                }
            }
            // 현재 블럭이 없으면 == 블럭을 내가 움직이지 않고 우연히 겹칠 때
            else
            {
                // 해당 목록의 첫번째 것으로 만들어
                if (currentMatches[0] != null)
                {
                    int column = currentMatches[0].GetComponent<Dot>().column;
                    int row = currentMatches[0].GetComponent<Dot>().row;

                    if (currentMatches[0].GetComponent<Dot>().isMatched)
                    {
                        currentMatches[0].GetComponent<Dot>().isMatched = false;
                        currentMatches[0].GetComponent<Dot>().MakeRowBomb();
                    }
                }
            }
        }
    }
}