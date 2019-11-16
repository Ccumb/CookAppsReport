using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                                GetNearbyPieces(rightUpDot, currentDot, leftDownDot);
                            }
                        }
                    }
                }
            }
        }
    }

    
}
