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

[DisallowMultipleComponent]
public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;

    public GameObject tilePrefab;
    public GameObject[] dotPrefabs;
    public GameObject nodePrefab;
    public GameObject obstaclePrefab;
    public GameObject endPage;
    public GameObject goalPage;

    public float offset;

    // 제일 적은 열의 갯수
    public int minHeight;
    // 제일 많은 열의 갯수
    public int maxHeight;
    // 총 행의 갯수
    public int totalWidth;
    
    public List<List<GameObject>> dots = new List<List<GameObject>>();
    public List<List<GameObject>> nodes = new List<List<GameObject>>();
    public List<List<Vector2>> tiles = new List<List<Vector2>>();

    // 장애물 위치, 실제 좌표 값이 아닌 인덱스 번호로.
    public List<Vector2> obstaclePos = new List<Vector2>();

    public Dot currentDot;
    private FindMatches mFindMatches;

    private ScoreManager scoreManager;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private Queue<GameObject> refillDots;

    public Queue<GameObject> obstacles = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("DisplayGoalPage");

        mFindMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        refillDots = new Queue<GameObject>();
        endPage.active = false;

        SetUp();
    }

    IEnumerator DisplayGoalPage()
    {
        goalPage.active = true;
        currentState = GameState.wait;

        yield return new WaitForSeconds(3.0f);

        goalPage.active = false;
        currentState = GameState.move;

        mFindMatches.FindAllMatches();
        DestroyMatches();
    }

    // Dot 세팅
    void SetUp()
    {
        Vector2 tilePos = new Vector2(0, 0);
        int currentHeight = minHeight;

        SetUp_TileAndNode();

        for (int i = 0; i < totalWidth; i++)
        {
            dots.Add(new List<GameObject>());

            if (currentHeight <= maxHeight)
            {
                tilePos = new Vector2(i, -(float)0.5 * i);

                for (int j = 0; j < currentHeight; j++)
                {
                    tilePos.y += 1;

                    // Dot 겹치지 않게 하는 부분
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

                for (int j = 0; j < maxHeight - (currentHeight - maxHeight); j++)
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

    // Tile 및 Node 세팅
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

    // 장애물 세팅
    void SetUp_Obstacle()
    {
        for (int i = 0; i < obstaclePos.Count; i++)
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

        scoreManager.obstacleCount = obstacles.Count;
    }

    // 현재 터져야 할 블럭이 있는가?
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

    bool IsNullNode()
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            for(int j = 0; j < nodes[i].Count; j++)
            {
                if(nodes[i][j] != null)
                {
                    if(nodes[i][j].GetComponent<Node>().dot == null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // 해당 인덱스의 블럭 실질적으로 제거
    void DestroyMatchesAt(int column, int row)
    {
        if (dots[column][row].GetComponent<Dot>().isMatched)
        {
            if (dots[column][row].tag != "Obstacle")
            {
                scoreManager.IncreaseScore(basePieceValue * streakValue);

                if (mFindMatches.currentMatches.Count >= 4)
                {
                    CheckToMakeBombs();
                }

            }
            nodes[column][row].GetComponent<Node>().dot = null;
            Destroy(dots[column][row]);
            dots[column][row] = null;
        }
    }

    // 각 행의 제일 위칸 블럭 검색 -> 양 옆으로 내려주기
    void CheckTopDots()
    {
        currentState = GameState.fill;
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = nodes[i].Count - 1; j >= 0; j--)
            {
                if (nodes[i][j].GetComponent<Node>().dot != null)
                {
                    if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN] != null
                        && nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN] != null)
                    {
                        int rand = Random.Range(0, 1);

                        if (rand == 1)
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

                    if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN] != null)
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

    // 맨 위칸이 비면 블럭 생성
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

    // 블럭 리필 후 처리들
    IEnumerator FillBoardCo()
    {
        StartCoroutine("DropDot_reFill");
        yield return new WaitForSeconds(0.5f);

        while (MatchesOnBoard())
        {
            streakValue++;
            yield return new WaitForSeconds(0.5f);

            DestroyMatches();

        }
        mFindMatches.currentMatches.Clear();
        currentDot = null;

        CheckTopDots();

        // 데드락 수정중. 이후 블럭 섞기 구현해야.
        if (IsDeadLocked())
        {
            Debug.Log("DeadLocked!");
            //StartCoroutine("ShakeBoard");
        }

        yield return new WaitForSeconds(0.5f);
        currentState = GameState.move;
        streakValue = 1;
    }

    // 블럭을 아래칸까지 내려주기
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

        CheckClear();

        yield return new WaitForSeconds(0.3f);
        currentState = GameState.move;
        StartCoroutine("FillBoardCo");
    }

    // 터져야할 블럭 터트리기
    public void DestroyMatches()
    {
        Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();

        if (!IsNullNode())
        {
            for (int i = 0; i < dots.Count; i++)
            {
                for (int j = 0; j < dots[i].Count; j++)
                {
                    if (dots[i][j] != null)
                    {
                        if (dots[i][j].tag == "Obstacle")
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
            scoreManager.UpdateObstacleCount(obstacles.Count);
            mFindMatches.currentMatches.Clear();
        }

        StartCoroutine("DropDotsCo");
        CheckClear();
    }

    // 해당 블럭이 터질 수 있는 환경인가?
    public bool CheckNearArea(int column, int row)
    {
        GameObject nodeObj = nodes[column][row];

        List<string> otherDotTags = new List<string>();

        Queue<GameObject> check_nodes = new Queue<GameObject>();

        if (nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();
            GameObject dot = node.dot;

            if (dot != null)
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

                            if (node.nearNodes[i].GetComponent<Node>().dot.tag == dot.tag)
                            {
                                check_nodes.Enqueue(node.nearNodes[i]);

                            }
                        }
                    }
                }

                // 주변에 나와 다르지만, 색이 같은 블럭들이 있나 체크
                if (CheckNearTag(node.column, node.row))
                {
                    return true;
                }

                // 나와 색이 같은 블럭의 주변에 나 이외에 색이 같은 블럭들이 있나 체크
                for (int i = 0; i < check_nodes.Count; i++)
                {
                    GameObject checkNode = check_nodes.Dequeue();
                    Node checkNode_node = checkNode.GetComponent<Node>();

                    for (int j = 0; j < 6; j++)
                    {
                        if (checkNode_node.nearNodes[i] != null)
                        {
                            if (checkNode_node.nearNodes[i].GetComponent<Node>().dot != null)
                            {
                                if (node.column != checkNode_node.nearNodes[i].GetComponent<Node>().column
                                    && node.row != checkNode_node.nearNodes[i].GetComponent<Node>().row)
                                {
                                    for (int k = 0; k < 6; k++)
                                    {
                                        if (node.GetComponent<Node>().nearNodes[k] != null)
                                        {
                                            if (node.GetComponent<Node>().nearNodes[k] != checkNode_node.nearNodes[i])
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
                if (CheckNearNode_NotSameTag(node.column, node.row))
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
                        if (dot.tag == otherDot.tag && dot.tag != "Obstaacle")
                        {
                            int nCount = 0;

                            for (int j = 0; j < 6; j++)
                            {
                                GameObject findSame = node.nearNodes[j];

                                if (findSame != null)
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

        if (nodeObj != null)
        {
            Node node = nodeObj.GetComponent<Node>();

            for (int i = 0; i < 6; i++)
            {
                GameObject nearNodeObj = node.nearNodes[i];

                if (nearNodeObj != null)
                {
                    Node nearNode = nearNodeObj.GetComponent<Node>();

                    if (node.dot != null && nearNode.dot != null)
                    {
                        if (node.dot.tag != nearNode.tag)
                        {
                            if (nearNode.nearNodes[i] != null)
                            {
                                if (nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i] != null)
                                {
                                    if (nearNode.nearNodes[i].GetComponent<Node>().dot != null
                                        && nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i].GetComponent<Node>().dot != null)
                                    {
                                        if (nearNode.nearNodes[i].GetComponent<Node>().nearNodes[i].GetComponent<Node>().dot.tag == nearNode.nearNodes[i].GetComponent<Node>().dot.tag
                                            && nearNode.nearNodes[i].GetComponent<Node>().dot.tag != "Obstacle")
                                        {
                                            int nCount = 0;

                                            for (int j = 0; j < 6; j++)
                                            {
                                                GameObject tmp = node.nearNodes[j];

                                                if (tmp != null && tmp.GetComponent<Node>().dot != null)
                                                {
                                                    if(tmp.GetComponent<Node>().dot.tag == nearNode.nearNodes[i].GetComponent<Node>().dot.tag)
                                                    {
                                                        nCount++;
                                                    }
                                                }
                                            }
                                            
                                            if(nCount > 1)
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
        }

        return false;
    }


    // 데드락?
    bool IsDeadLocked()
    {
        Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();
        int nCount = 0;

        if (topNode.dot != null && dots[topNode.column][topNode.row] != null)
        {
            for (int i = 0; i < dots.Count; i++)
            {
                for (int j = 0; j < dots[i].Count; j++)
                {
                    if (dots[i][j] != null)
                    {
                        if (CheckNearArea(i, j))
                        {
                            nCount++;

                            if(nCount > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // 터질 수 있는 것들이 하나도 없으면 true
            if(nCount <= 0)
            {
                return true;
            }
        }
        return false;
    }

    // 블럭 초기 생성시 바로 터지지 않게 미리 검사 ( 아직 같은 열만 검사 )
    bool MatchesAt(int column, int row, GameObject piece)
    {
        if (row > 1)
        {
            if (dots[column][row - 1] != null && dots[column][row - 2] != null)
            {
                if (dots[column][row - 1].tag == piece.tag && dots[column][row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 클리어
    void  CheckClear()
    {
        // 장애물이 다 사라지면 클리어
        if (obstacles.Count <= 0)
        {
            endPage.active = true;
            StopAllCoroutines();
            currentState = GameState.clear;
        }
    }

    // 폭탄을 만들 수 있으면 만들기
    void CheckToMakeBombs()
    {
        // 터진 블럭의 갯수가 4개면
        if (mFindMatches.currentMatches.Count == 4)
        {
            mFindMatches.CheckBombs();
        }
    }

    IEnumerator ShakeBoard()
    {
        yield return new WaitForSeconds(0.5f);
        List<GameObject> newDots = new List<GameObject>();

        Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();
        int nCount = 0;

        if (topNode.dot != null && dots[topNode.column][topNode.row] != null)
        {

            for (int i = 0; i < dots.Count; i++)
            {
                for (int j = 0; j < dots[i].Count; j++)
                {
                    if (dots[i][j] != null && dots[i][j].tag != "Obstacle")
                    {
                        newDots.Add(dots[i][j]);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < dots.Count; i++)
            {
                for (int j = 0; j < dots[i].Count; j++)
                {
                    int dotToUse = Random.Range(0, newDots.Count);

                    int maxIteration = 0;

                    while (MatchesAt(i, j, newDots[dotToUse]) && maxIteration < 100)
                    {
                        dotToUse = Random.Range(0, newDots.Count);
                        maxIteration++;
                    }
                    maxIteration = 0;

                    Dot dot = newDots[dotToUse].GetComponent<Dot>();

                    if (dots[i][j].tag != "Obstacle")
                    {
                        dot.column = i;
                        dot.row = j;
                        dots[i][j] = newDots[dotToUse];
                        nodes[i][j].GetComponent<Node>().dot = newDots[dotToUse];
                        newDots.Remove(newDots[dotToUse]);
                    }
                }
            }
        }

        if(IsDeadLocked())
        {
            ShakeBoard();
        }
    }
                

    // Update is called once per frame
    void Update()
    {

    }
}

