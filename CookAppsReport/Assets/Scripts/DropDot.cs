using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDot : MonoBehaviour
{
    public Dot dot;
    private Board board;
    public Node node;

    // Start is called before the first frame update
    void Start()
    {
        dot = this.GetComponent<Dot>();
        board = FindObjectOfType<Board>();
       
        node = board.nodes[dot.column][dot.row].GetComponent<Node>();

        //StartCoroutine("DropDot_DownCo");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDropDot()
    {
        
    }

    IEnumerator DropDot_DownCo()
    {
        while(true)
        {
            if(node.nearNodes[(int)Dir.DOWN] != null)
            {
                if(node.nearNodes[(int)Dir.DOWN].GetComponent<Node>().dot == null)
                {
                    node.dot = null;
                    board.dots[dot.column][dot.row] = null;
                    board.nodes[dot.column][dot.row].GetComponent<Node>().dot = null;

                    if(dot.row > 0)
                    {
                        dot.row--;
                    }
                    node = board.nodes[dot.column][dot.row].GetComponent<Node>();
                    board.dots[dot.column][dot.row] = this.gameObject;
                    node.dot = this.gameObject;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DropDot_SideCo()
    {
        while(true)
        {

        }
    }


}
