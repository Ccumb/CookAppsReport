using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move,
    fill,
    clear
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;

    public GameObject tilePrefab;
    public float offset;

    public int minHeight;      
    public int maxHeight;       
    public int totalWidth;
    public float refillDelay = 0.3f;

    public GameObject[] dotPrefabs;
    public GameObject nodePrefab;
    public GameObject obstaclePrefab;
    public GameObject endPage;

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

    public Queue<GameObject> obstacles = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mFindMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        nullNodes = new Queue<Node>();
        refillDots = new Queue<GameObject>();
        endPage.active = false;

        SetUp();

        mFindMatches.FindAllMatches();

        DestroyMatches();

    }
    

    void SetUp()
    {
        Vector2 tilePos = new Vector2(0, 0);
        int currentHeight = minHeight;

        SetUp_TileAndNode();

        for (int i = 0; i < totalWidth; i++)
        {
            dots.Add(new List<GameObject>());

            if(currentHeight <= maxHeight)
            {
                tilePos = new Vector2(i, -(float)0.5 * i);

                for (int j = 0; j < currentHeight; j++)
                {
                    tilePos.y += 1;
                    
                    int maxIteration = 0;
                    int dotToUse = Random.Range(0, dotPrefabs.Length);
                    while (MatchesAt(i, j, dotPrefabs[dotToUse]) && maxIteration < 100)
                    {
                        dotToUse = Random.Range(0, dotPrefabs.Length);

                        maxIteration++;
                    }
                    maxIteration = 0;

                    GameObject dot = Instantiate(dotPrefabs[dotToUse], tilePos, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name = "dot (" + i + ", " + j + ")";

                    nodes[i][j].GetComponent<Node>().dot = dot;
                    dots[i].Add(dot);
                }
            }
            else
            {
                tilePos = new Vector2(i, -(float)0.5 * ((totalWidth - 1) - i));

                for (int j = 0; j < maxHeight - (currentHeight - maxHeight) ; j++)
                {
                    tilePos.y += 1;

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

                    nodes[i][j].GetComponent<Node>().dot = dot;
                    dots[i].Add(dot);
                }
            }
            currentHeight++;
        }

        SetUp_Obstacle();
    }

    void SetUp_TileAndNode()
    {
        Vector2 tilePos = new Vector2(0, 0);
        int currentHeight = minHeight;

        for (int i = 0; i < totalWidth; i++)
        {
            tiles.Add(new List<Vector2>());
            nodes.Add(new List<GameObject>());

            if (currentHeight <= maxHeight)
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
                    
                    node.GetComponent<Node>().row = j;
                    node.GetComponent<Node>().column = i;
                }
            }
            else
            {
                tilePos = new Vector2(i, -(float)0.5 * ((totalWidth - 1) - i));

                for (int j = 0; j < maxHeight - (currentHeight - maxHeight); j++)
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
                    
                    node.GetComponent<Node>().row = j;
                    node.GetComponent<Node>().column = i;
                    
                }
            }
            currentHeight++;
        }
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

            obstacles.Enqueue(obstacle);
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

                if (mFindMatches.currentMatches.Count >= 4)
                {
                    CheckToMakeBombs();
                }

            }
            nodes[column][row].GetComponent<Node>().dot = null;
            Destroy(dots[column][row]);
            
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

    IEnumerator DropDot_reFill()
    {
        Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();
        
        if (topNode.dot == null && dots[topNode.column][topNode.row] == null)
        {
            int dotToUse = Random.Range(0, dotPrefabs.Length);

            GameObject dot = Instantiate(dotPrefabs[dotToUse], topNode.gameObject.transform.position, Quaternion.identity);

            dot.transform.parent = this.transform;
            topNode.dot = dot;
            topNode.dot.GetComponent<Dot>().column = topNode.column;
            topNode.dot.GetComponent<Dot>().row = topNode.row;
            dots[topNode.column][topNode.row] = topNode.dot;
            topNode.dot.active = true;
        }
        
        yield return new WaitForSeconds(0.5f);
        StartCoroutine("DropDotsCo");
        CheckTopDots();
    }

    IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.5f);

        while(MatchesOnBoard())
        {
            streakValue++;
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();

        }
        mFindMatches.currentMatches.Clear();
        currentDot = null;

        CheckTopDots();
        
        if(IsDeadLocked())
        {
        }

        yield return new WaitForSeconds(0.5f);
        currentState = GameState.move;
        streakValue = 1;
    }

    IEnumerator DropDotsCo()
    {
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

        yield return new WaitForSeconds(0.5f);
        currentState = GameState.move;
        StartCoroutine("FillBoardCo");
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
        StartCoroutine("CheckClear");
    }
    
    public bool CheckNearArea(int column, int row)
    {
        GameObject nodeObj = nodes[column][row];

        List<string> otherDotTags = new List<string>();

        Queue<GameObject> check_nodes = new Queue<GameObject>();

        if(nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();
            GameObject dot = node.dot;

            if(dot != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (node.nearNodes[i] != null)
                    {
                        if (node.nearNodes[i].GetComponent<Node>().dot != null)
                        {
                            if (node.nearNodes[i].GetComponent<Node>().dot.tag != "Obstacle")
                            {
                                otherDotTags.Add(node.nearNodes[i].GetComponent<Node>().dot.tag);
                            }

                            if (node.nearNodes[i].GetComponent<Node>().dot.tag == dot.tag && dot.tag != "Obstacle")
                            {
                                check_nodes.Enqueue(node.nearNodes[i]);
                                
                            }
                        }
                    }
                }

                // 주변에 나와 다르지만, 색이 같은 블럭들이 있나 체크
                if(CheckNearTag(node.column, node.row))
                {
                    return true;
                }

                // 나와 색이 같은 블럭의 주변에 나 이외에 색이 같은 블럭들이 있나 체크
                for(int i = 0; i < check_nodes.Count; i++)
                {
                    GameObject checkNode = check_nodes.Dequeue();
                    Node checkNode_node = checkNode.GetComponent<Node>();

                    for(int j = 0; j < 6; j++)
                    {
                        if(checkNode_node.nearNodes[i] != null)
                        {
                            if(checkNode_node.nearNodes[i].GetComponent<Node>().dot != null)
                            {
                                if(node.column != checkNode_node.nearNodes[i].GetComponent<Node>().column 
                                    && node.row != checkNode_node.nearNodes[i].GetComponent<Node>().row)
                                {
                                    for(int k = 0; k < 6; k++)
                                    {
                                        if (node.GetComponent<Node>().nearNodes[k] != null)
                                        {
                                            if(node.GetComponent<Node>().nearNodes[k] != checkNode_node.nearNodes[i])
                                            {
                                                if (checkNode_node.dot.tag == checkNode_node.nearNodes[i].GetComponent<Node>().dot.tag)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                // 나와 색이 다른 블럭의 주변에 나와 색이 같은 블럭들이 있나 체크
                if(CheckNearNode_NotSameTag(node.column, node.row))
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    bool CheckNearTag(int column, int row)
    {
        GameObject nodeObj = nodes[column][row];

        if (nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();

            for (int i = 0; i < 3; i++)
            {
                GameObject nearNodeObj = node.nearNodes[i];
                GameObject nearNodeObj_other = node.nearNodes[i + 3];

                if (nearNodeObj != null && nearNodeObj_other != null)
                {
                    GameObject dot = nearNodeObj.GetComponent<Node>().dot;
                    GameObject otherDot = nearNodeObj_other.GetComponent<Node>().dot;

                    if (dot != null && otherDot != null)
                    {
                        if(dot.tag == otherDot.tag && dot.tag != "Obstaacle")
                        {
                            int nCount = 0;

                            for(int j = 0; j < 6; j++)
                            {
                                GameObject findSame = node.nearNodes[j];

                                if(findSame != null)
                                {
                                    if (findSame.GetComponent<Node>().dot != null)
                                    {
                                        if (findSame.GetComponent<Node>().dot.tag == dot.tag)
                                        {
                                            nCount++;
                                        }
                                    }
                                    if (nCount > 2)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    bool CheckNearNode_NotSameTag(int column, int row)
    {
        GameObject nodeObj = nodes[column][row];

        if(nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();

            for (int i = 0; i < 6; i++)
            {
                GameObject nearNodeObj = node.nearNodes[i];

                if(nearNodeObj != null)
                {
                    Node nearNode = nearNodeObj.GetComponent<Node>();

                    if(node.dot != null && nearNode.dot != null)
                    {
                        if (node.dot.tag != nearNode.tag && node.dot.tag != "Obstacle")
                        {
                            if (nearNode.nearNodes[i] != null)
                            {
                                if (nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i] != null)
                                {
                                    if (nearNode.nearNodes[i].GetComponent<Node>().dot != null
                                        && nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i].GetComponent<Node>().dot != null)
                                    {
                                        if (nearNode.nearNodes[i].GetComponent<Node>().dot.tag == node.tag
                                        && nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i].GetComponent<Node>().dot.tag == nearNode.nearNodes[i].GetComponent<Node>().dot.tag)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    bool IsDeadLocked()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            for (int j = 0; j < dots[i].Count; j++)
            {
                if (dots[i][j] != null)
                {
                    if (CheckNearArea(i, j))
                    {
                        return false;
                    }
                    //CheckNearArea(i, j);
                }
            }
        }
        return true;
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

        return false;
    }

    IEnumerator CheckClear()
    {
        yield return new WaitForSeconds(0.3f);

        if(obstacles.Count <= 0)
        {
            endPage.active = true;
            //currentState = GameState.clear;

            yield return new WaitForSeconds(0.5f);
            StopAllCoroutines();
            currentState = GameState.clear;
        }
    }

    void CheckToMakeBombs()
    {
        if (mFindMatches.currentMatches.Count == 4)
        {
            Debug.Log("Match 4");
            mFindMatches.CheckBombs();
        }
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}
