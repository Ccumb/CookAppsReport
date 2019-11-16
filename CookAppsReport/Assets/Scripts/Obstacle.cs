using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
        //CheckNearNodes();
    }

    public void CheckNearNodes()
    {
        GameObject nodeObj = board.nodes[dot.column][dot.row];
         
        if (count <= 0)
        {
            dot.isMatched = true;
        }

        if (nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();

            for (int i = 0; i < 6; i++)
            {
                if(node.nearNodes[i] != null)
                {
                    if(node.nearNodes[i].GetComponent<Node>().dot == null
                        /*&& node.nearNodes[i].GetComponent<Node>().dot.tag != this.tag*/)
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
