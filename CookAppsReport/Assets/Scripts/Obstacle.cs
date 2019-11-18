using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Obstacle : MonoBehaviour
{
    public int count = 3;
    public Dot dot;
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        dot = this.gameObject.GetComponent<Dot>();
        board = FindObjectOfType<Board>();
        CheckNearNodes();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckNearNodes();
    }

    // 주변 노드들 탐지
    public void CheckNearNodes()
    {
        if(dot != null)
        {
            GameObject nodeObj = board.nodes[dot.column][dot.row];

            if (count <= 0)
            {
                board.obstacles.Dequeue();
                dot.isMatched = true;
            }

            if (nodeObj != null)
            {
                Node node = nodeObj.GetComponent<Node>();

                for (int i = 0; i < 6; i++)
                {
                    if (node.nearNodes[i] != null)
                    {
                        // 주변에서 블럭이 터지면
                        if (node.nearNodes[i].GetComponent<Node>().dot == null || node.nearNodes[i].GetComponent<Node>().dot.GetComponent<Dot>().isMatched == true)
                        {
                            this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                            count--;
                            break;
                        }
                    }
                }
            }
        }
    }
}
