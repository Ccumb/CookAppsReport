using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move,
    fill
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;

    public GameObject tilePrefab;
    public float offset;

    public int minHeight;      
    public int maxHeight;       
    public int totalWidth;     

    public GameObject[] dotPrefabs;
    public GameObject nodePrefab;
    public GameObject obstaclePrefab;

    public List<List<GameObject>> dots = new List<List<GameObject>>();
    public List<List<GameObject>> nodes = new List<List<GameObject>>();
    public List<List<Vector2>> tiles = new List<List<Vector2>>();

    public List<Vector2> obstaclePos = new List<Vector2>();

    public Dot currentDot;
    private FindMatches mFindMatches;

    private ScoreManager scoreManager;
    public int basePieceValue = 20;
    private int streakValue = 1;

    private Queue<Node> nullNodes;
    private Queue<GameObject> refillDots;

    // Start is called before the first frame update
    void Start()
    {
        mFindMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        nullNodes = new Queue<Node>();
        refillDots = new Queue<GameObject>(); 
        SetUp();

        //StartCoroutine("CheckNullCo");
    }
    

    void SetUp()
    {
        Vector2 tilePos = new Vector2(0, 0);
        int currentHeight = minHeight;

        for(int i = 0; i < totalWidth; i++)
        {
            dots.Add(new List<GameObject>());
            tiles.Add(new List<Vector2>());
            nodes.Add(new List<GameObject>());

            if(currentHeight <= maxHeight)
            {
                tilePos = new Vector2(i, -(float)0.5 * i);

                for (int j = 0; j < currentHeight; j++)
                {
                    tilePos.y += 1;
                    GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                    tile.name = "(" + i + ", " + j + ")";
                    tile.transform.parent = this.transform;

                    GameObject node = Instantiate(nodePrefab, tilePos, Quaternion.identity) as GameObject;
                    node.name = "node (" + i + ", " + j + ")";
                    node.transform.parent = this.transform;

                    tiles[i].Add(tilePos);
                    nodes[i].Add(node);

                    int maxIteration = 0;
                    int dotToUse = Random.Range(0, dotPrefabs.Length);
                    while (MatchesAt(i, j, dotPrefabs[dotToUse]) && maxIteration < 100)
                    {
                        dotToUse = Random.Range(0, dotPrefabs.Length);

                        maxIteration++;
                        Debug.Log(maxIteration);
                    }
                    maxIteration = 0;

                    GameObject dot = Instantiate(dotPrefabs[dotToUse], tilePos, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name = "dot (" + i + ", " + j + ")";

                    node.GetComponent<Node>().dot = dot;
                    node.GetComponent<Node>().row = j;
                    node.GetComponent<Node>().column = i;
                    
                    dots[i].Add(dot);
                }
            }
            else
            {
                tilePos = new Vector2(i, -(float)0.5 * ((totalWidth - 1) - i));

                for (int j = 0; j < maxHeight - (currentHeight - maxHeight) ; j++)
                {
                    tilePos.y += 1;
                    GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                    tile.name = "(" + i + ", " + j + ")";
                    tile.transform.parent = this.transform;

                    GameObject node = Instantiate(nodePrefab, tilePos, Quaternion.identity) as GameObject;
                    node.name = "node (" + i + ", " + j + ")";
                    node.transform.parent = this.transform;

                    tiles[i].Add(tilePos);
                    nodes[i].Add(node);

                    int dotToUse = Random.Range(0, dotPrefabs.Length);
                    int maxIteration = 0;
                    while (MatchesAt(i, j, dotPrefabs[dotToUse]) && maxIteration < 100)
                    {
                        dotToUse = Random.Range(0, dotPrefabs.Length);

                        maxIteration++;
                        Debug.Log(maxIteration);
                    }
                    maxIteration = 0;

                    GameObject dot = Instantiate(dotPrefabs[dotToUse], tilePos, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name = "dot (" + i + ", " + j + ")";

                    node.GetComponent<Node>().dot = dot;
                    node.GetComponent<Node>().row = j;
                    node.GetComponent<Node>().column = i;

                    dots[i].Add(dot);
                    
                }
            }
            currentHeight++;
        }
        SetUp_Obstacle();
    }

    void SetUp_Obstacle()
    {
        for(int i = 0; i < obstaclePos.Count; i++)
        {
            Vector2 ObstaclePos = obstaclePos[i];

            GameObject obstacle = Instantiate(obstaclePrefab, tiles[(int)ObstaclePos.x][(int)ObstaclePos.y], Quaternion.identity);
            GameObject dot = nodes[(int)ObstaclePos.x][(int)ObstaclePos.y].GetComponent<Node>().dot;
            Destroy(dot);

            nodes[(int)ObstaclePos.x][(int)ObstaclePos.y].GetComponent<Node>().dot = obstacle;
            obstacle.GetComponent<Dot>().column = (int)ObstaclePos.x;
            obstacle.GetComponent<Dot>().row = (int)ObstaclePos.y;
            dots[(int)ObstaclePos.x][(int)ObstaclePos.y] = obstacle;
        }
    }
    

    bool MatchesOnBoard()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            for (int j = 0; j < dots[i].Count; j++)
            {
                if (dots[i][j] != null)
                {
                    if (dots[i][j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void DestroyMatchesAt(int column, int row)
    {
        if (dots[column][row].GetComponent<Dot>().isMatched)
        {
            if(dots[column][row].tag != "Obstacle")
            {
                scoreManager.IncreaseScore(basePieceValue * streakValue);
            }
            Destroy(dots[column][row]);
            nodes[column][row].GetComponent<Node>().dot = null;
            nullNodes.Enqueue(nodes[column][row].GetComponent<Node>());
            dots[column][row] = null;
        }
    }

    void RefillBoard()
    {
        StartCoroutine("DropDot_reFill");
    }

    void CheckTopDots()
    {
        currentState = GameState.fill;
        for (int i = 0; i < nodes.Count; i++)
        {
            for(int j = nodes[i].Count - 1; j >= 0; j--)
            {
                if(nodes[i][j].GetComponent<Node>().dot != null)
                {
                    if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN] != null
                        && nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN] != null)
                    {
                        int rand = Random.Range(0, 1);

                        if(rand == 1)
                        {
                            Node sideNode = nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN].GetComponent<Node>();

                            if (sideNode.dot == null && nodes[i][j].GetComponent<Node>().dot != null)
                            {
                                nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().column = sideNode.column;
                                nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().row = sideNode.row;

                                sideNode.dot = nodes[i][j].GetComponent<Node>().dot;
                                dots[sideNode.column][sideNode.row] = nodes[i][j].GetComponent<Node>().dot;

                                nodes[i][j].GetComponent<Node>().dot = null;
                                dots[i][j] = null;
                            }
                        }
                        else
                        {
                            Node sideNode = nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN].GetComponent<Node>();

                            if (sideNode.dot == null)
                            {
                                nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().column = sideNode.column;
                                nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().row = sideNode.row;

                                sideNode.dot = nodes[i][j].GetComponent<Node>().dot;
                                dots[sideNode.column][sideNode.row] = nodes[i][j].GetComponent<Node>().dot;

                                nodes[i][j].GetComponent<Node>().dot = null;
                                dots[i][j] = null;
                            }
                        }
                    }

                    if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN] != null)
                    {
                        Node sideNode = nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN].GetComponent<Node>();

                        if (sideNode.dot == null && nodes[i][j].GetComponent<Node>().dot != null)
                        {
                            nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().column = sideNode.column;
                            nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().row = sideNode.row;

                            sideNode.dot = nodes[i][j].GetComponent<Node>().dot;
                            dots[sideNode.column][sideNode.row] = nodes[i][j].GetComponent<Node>().dot;

                            nodes[i][j].GetComponent<Node>().dot = null;
                            dots[i][j] = null;
                        }
                    }

                    if(nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN] != null)
                    {
                        Node sideNode = nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN].GetComponent<Node>();

                        if (sideNode.dot == null && nodes[i][j].GetComponent<Node>().dot != null)
                        {
                            nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().column = sideNode.column;
                            nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().row = sideNode.row;

                            sideNode.dot = nodes[i][j].GetComponent<Node>().dot;
                            dots[sideNode.column][sideNode.row] = nodes[i][j].GetComponent<Node>().dot;

                            nodes[i][j].GetComponent<Node>().dot = null;
                            dots[i][j] = null;
                        }
                    }
                }
            }
        }
        currentState = GameState.move;
    }

    IEnumerator CheckNullCo()
    {
        while(true)
        {
            CheckNullNode();

            int nullCount = nullNodes.Count;
            Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();

            if (nullCount > 0)
            {
                if (topNode.dot == null && dots[topNode.column][topNode.row] == null)
                {
                    int dotToUse = Random.Range(0, dotPrefabs.Length);

                    GameObject dot = Instantiate(dotPrefabs[dotToUse], topNode.gameObject.transform.position, Quaternion.identity);

                    topNode.dot = dot;
                    topNode.dot.GetComponent<Dot>().column = topNode.column;
                    topNode.dot.GetComponent<Dot>().row = topNode.row;
                    dots[topNode.column][topNode.row] = topNode.dot;
                    topNode.dot.active = true;
                }
                yield return new WaitForSeconds(0.2f);

                while (MatchesOnBoard())
                {
                    //streakValue++;
                    yield return new WaitForSeconds(0.2f);
                    DestroyMatches();

                }
                mFindMatches.currentMatches.Clear();
                currentDot = null;

                yield return new WaitForSeconds(0.3f);
                currentState = GameState.move;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator DropDot_reFill()
    {
        Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();
        
        if (topNode.dot == null && dots[topNode.column][topNode.row] == null)
        {
            int dotToUse = Random.Range(0, dotPrefabs.Length);

            GameObject dot = Instantiate(dotPrefabs[dotToUse], topNode.gameObject.transform.position, Quaternion.identity);

            topNode.dot = dot;
            topNode.dot.GetComponent<Dot>().column = topNode.column;
            topNode.dot.GetComponent<Dot>().row = topNode.row;
            dots[topNode.column][topNode.row] = topNode.dot;
            topNode.dot.active = true;
        }
        
        yield return new WaitForSeconds(0.3f);
        StartCoroutine("DropDotsCo");
        CheckTopDots();
    }

    IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.3f);

        while(MatchesOnBoard())
        {
            streakValue++;
            yield return new WaitForSeconds(0.3f);
            DestroyMatches();

        }
        mFindMatches.currentMatches.Clear();
        currentDot = null;

        CheckTopDots();
        
        yield return new WaitForSeconds(0.3f);
        currentState = GameState.move;
        streakValue = 1;
    }
    
    IEnumerator DropDotsCo()
    {
        yield return new WaitForSeconds(0.2f);
        
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes[i].Count; j++)
            {
                if (nodes[i][j].GetComponent<Node>().dot != null)
                {
                    if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.DOWN] != null)
                    {
                        if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.DOWN].GetComponent<Node>().dot == null)
                        {
                            GameObject dot = nodes[i][j].GetComponent<Node>().dot;

                            if (dot.GetComponent<Dot>().row > 0)
                            {
                                dot.GetComponent<Dot>().row--;
                            }

                            nodes[i][dot.GetComponent<Dot>().row].GetComponent<Node>().dot = dot;
                            nodes[i][j].GetComponent<Node>().dot = null;
                            
                            dots[i][j] = null;
                        }
                    }
                }
            }
        }

        nullNodes.Clear();


        yield return new WaitForSeconds(0.15f);
        currentState = GameState.move;
        StartCoroutine("FillBoardCo");
    }

   
    void CheckNullNode()
    {
        nullNodes.Clear();

        for(int i = 0; i < nodes.Count; i++)
        {
            for(int j = 0; j < nodes[i].Count; j++)
            {
                if(nodes[i][j].GetComponent<Node>().dot == null && !(nullNodes.Contains(nodes[i][j].GetComponent<Node>())))
                {
                    nullNodes.Enqueue(nodes[i][j].GetComponent<Node>());
                }
            }
        }
        Debug.Log(nullNodes.Count);
        
    }
    

    public void DestroyMatches()
    {
        if(currentState != GameState.fill)
        {
            for (int i = 0; i < dots.Count; i++)
            {
                for (int j = 0; j < dots[i].Count; j++)
                {
                    if (dots[i][j] != null)
                    {

                        if(dots[i][j].tag == "Obstacle")
                        {
                            dots[i][j].GetComponent<Obstacle>().CheckNearNodes();
                            DestroyMatchesAt(i, j);
                        }
                        else
                        {
                            DestroyMatchesAt(i, j);
                        }
                    }
                }
            }
            mFindMatches.currentMatches.Clear();
        }

        StartCoroutine("DropDotsCo");
    }

    bool MatchesAt(int column, int row, GameObject piece)
    {
        if(row > 1)
        {
            if(dots[column][row - 1] != null && dots[column][row - 2] != null)
            {
                if(dots[column][row - 1].tag == piece.tag && dots[column][row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        /*if(column > 1 && row > 0 )
        {
            Node node = nodes[column][row].GetComponent<Node>();

            GameObject leftDown = node.nearNodes[(int)Dir.LEFT_DOWN];
            GameObject leftDownDown = leftDown.GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN];

            if (leftDown != null && leftDownDown != null)
            {

            }
        }*/


        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
