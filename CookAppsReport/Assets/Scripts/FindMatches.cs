using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board mBorad;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mBorad = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine("FindAllMatchesCo");
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
        yield return new WaitForSeconds(0.2f);

        for(int i = 0; i < mBorad.nodes.Count; i++)
        {
            for(int j = 0; j < mBorad.nodes[i].Count; j++)
            {
                Node currentDot_node = mBorad.nodes[i][j].GetComponent<Node>();
                GameObject currentDot = currentDot_node.GetComponent<Node>().dot;

                if (currentDot != null && currentDot.tag != "Obstacle")
                {
                    GameObject upDot_node = currentDot_node.nearNodes[(int)Dir.UP];
                    GameObject downDot_node = currentDot_node.nearNodes[(int)Dir.DOWN];

                    if(upDot_node != null && downDot_node != null)
                    {
                        // 상 하
                        GameObject upDot = upDot_node.GetComponent<Node>().dot;
                        GameObject downDot = downDot_node.GetComponent<Node>().dot;

                        if (upDot != null && downDot != null)
                        {
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsRowBomb(upDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), downDot.GetComponent<Dot>()));

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

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isRowBomb)
        {
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

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();

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

    public void CheckBombs()
    {
        if (mBorad.currentDot != null)
        {
            int column = mBorad.currentDot.column;
            int row = mBorad.currentDot.row;

            Debug.Log(column + ", " + row);

            if (mBorad.currentDot.isMatched)
            {
                mBorad.currentDot.isMatched = false;

                Debug.Log("Create Bomb");
                mBorad.currentDot.MakeRowBomb();
            }
        }
        else
        {
            if(currentMatches[0] != null)
            {
                int column = currentMatches[0].GetComponent<Dot>().column;
                int row = currentMatches[0].GetComponent<Dot>().row;

                if(currentMatches[0].GetComponent<Dot>().isMatched)
                {
                    currentMatches[0].GetComponent<Dot>().isMatched = false;
                    currentMatches[0].GetComponent<Dot>().MakeRowBomb();
                }
            }
        }
    }
}
