using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hexa
{

    // 게임의 상태
    public enum GameState
    {
        wait,
        move,
        fill,
        clear
    }

    [DisallowMultipleComponent]
    // 보드, 게임의 상태와 노드, 블럭, 타일들을 갖고 있고 전반적인 흐름, 움직임을 제어
    public class Board : MonoBehaviour
    {
        // 현재 게임상태
        public GameState currentState = GameState.move;

        // 각각에 쓰일 프리팹들
        public GameObject tilePrefab;
        public GameObject[] dotPrefabs;
        public GameObject nodePrefab;
        public GameObject obstaclePrefab;
        public GameObject endPage;
        public GameObject goalPage;

        // 제일 적은 열의 갯수
        public int minHeight;
        // 제일 많은 열의 갯수
        public int maxHeight;
        // 총 행의 갯수
        public int totalWidth;

        // 블럭, 노드, 타일들
        public List<List<GameObject>> dots = new List<List<GameObject>>();
        public List<List<GameObject>> nodes = new List<List<GameObject>>();
        public List<List<Vector2>> tiles = new List<List<Vector2>>();

        // 장애물 위치, 실제 좌표 값이 아닌 인덱스 번호로.
        public List<Vector2> obstaclePos = new List<Vector2>();

        // 현재 선택된 블럭
        public Dot currentDot;
        private FindMatches mFindMatches;

        private ScoreManager scoreManager;

        // 한 조각당 점수
        public int basePieceValue = 20;
        // 터진 횟수
        private int streakValue = 1;
        private Queue<GameObject> refillDots;

        // 장애물들을 담은 큐, 갯수만 아는 용도로
        public Queue<GameObject> obstacles = new Queue<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            //StartCoroutine("DisplayGoalPage");

            mFindMatches = FindObjectOfType<FindMatches>();
            scoreManager = FindObjectOfType<ScoreManager>();
            refillDots = new Queue<GameObject>();
            //endPage.active = false;

            SetUp();
        }

        // 목표 화면 띄우기
        IEnumerator DisplayGoalPage()
        {
            // 화면을 띄우고, 정지상태로
            goalPage.active = true;
            currentState = GameState.wait;

            // 3초후
            yield return new WaitForSeconds(3.0f);

            // 화면을 내리고, 움직이는 상태로
            goalPage.active = false;
            currentState = GameState.move;

            // 매치된 것이 있다면 없애기, 블럭생성시의 중복 문제 해결하면 없애기
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

        // 비어있는 노드가 있는가?
        bool IsNullNode()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes[i].Count; j++)
                {
                    if (nodes[i][j] != null)
                    {
                        if (nodes[i][j].GetComponent<Node>().dot == null)
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
                // 장애물이 아니면
                if (dots[column][row].tag != "Obstacle")
                {
                    // 점수를 증가시키고
                    scoreManager.IncreaseScore(basePieceValue * streakValue);

                    // 만약 현재 매치된 블럭의 수가 4개 이상이라면
                    if (mFindMatches.currentMatches.Count >= 4)
                    {
                        // 폭탄으로
                        CheckToMakeBombs();
                    }

                }

                // 해당 위치의 노드의 블럭 정보를 null로
                nodes[column][row].GetComponent<Node>().dot = null;
                // 블럭 리스트에서 삭제
                Destroy(dots[column][row]);
                // null로
                dots[column][row] = null;
            }
        }

        // 각 행의 제일 위칸 블럭 검색 -> 양 옆으로 내려주기
        void CheckTopDots()
        {
            currentState = GameState.fill;
            for (int i = 0; i < nodes.Count; i++)
            {
                // 위에서 부터 검사
                for (int j = nodes[i].Count - 1; j >= 0; j--)
                {
                    // 해당 노드에 블럭이 있다면
                    if (nodes[i][j].GetComponent<Node>().dot != null)
                    {
                        // 해당 노드의 좌하단, 우하단이 모두 존재하면
                        if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN] != null
                            && nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.LEFT_DOWN] != null)
                        {
                            // 랜덤
                            int rand = Random.Range(0, 1);

                            // 랜덤값에 따라 좌, 우하단으로 블럭 이동
                            if (rand == 1)
                            {
                                Node sideNode = nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.RIGHT_DOWN].GetComponent<Node>();

                                // 우하단 노드가 비어있고, 해당 노드의 dot이 있다면
                                if (sideNode.dot == null && nodes[i][j].GetComponent<Node>().dot != null)
                                {
                                    // 블럭들의 좌표를 바꾸고
                                    nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().column = sideNode.column;
                                    nodes[i][j].GetComponent<Node>().dot.GetComponent<Dot>().row = sideNode.row;

                                    // 우하단 노드에 현재 노드의 블럭 전달, dot 리스트에서 해당 인덱스 값 변경
                                    sideNode.dot = nodes[i][j].GetComponent<Node>().dot;
                                    dots[sideNode.column][sideNode.row] = nodes[i][j].GetComponent<Node>().dot;

                                    // 현재 노드의 블럭 비우고 dot 리스트에서 해당 인덱스 값도 비우기
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

                        // 좌하단만 존재하면, else if 로 할껄
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

                        // 우하단만 존재하면
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
            // 게임 상태를 움직이기 가능 상태로
            currentState = GameState.move;
        }

        // 맨 위칸이 비면 블럭 생성
        IEnumerator DropDot_reFill()
        {
            Debug.Log("ReFillDots _1");
            Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();

            // 제일 꼭대기 노드가 비어있고, 리스트의 해당 인덱스도 비워져있다면
            if (topNode.dot == null && dots[topNode.column][topNode.row] == null)
            {
                // 랜덤으로 생성
                int dotToUse = Random.Range(0, dotPrefabs.Length);

                GameObject dot = Instantiate(dotPrefabs[dotToUse], topNode.gameObject.transform.position, Quaternion.identity);

                dot.transform.parent = this.transform;
                topNode.dot = dot;
                topNode.dot.GetComponent<Dot>().column = topNode.column;
                topNode.dot.GetComponent<Dot>().row = topNode.row;
                dots[topNode.column][topNode.row] = topNode.dot;

                // 원래는 비어있는 만큼 생성하고 위가 비면 불러오는 방식으로 할 때 쓰기 위해 active를 썼었다.
                // 이 방식으로 할 때 active를 안쓰면 원래 초기값인 0, 0 좌표에서 꼭대기까지 움직이는게 보여 부자연스러움
                // 코드를 간소화하면서 여기에 true를 미처 삭제하지 못함
                topNode.dot.active = true;
            }

            // 위에 채우고 0.5초 후
            yield return new WaitForSeconds(0.5f);
            Debug.Log("ReFillDots _2");

            // 아래로 내리고
            StartCoroutine("DropDotsCo");

            // 각 열의 꼭대기 블럭들을 확인, 옆으로 내려줘
            CheckTopDots();
        }

        // 블럭 리필 후 처리들
        IEnumerator FillBoardCo()
        {
            // 블럭들을 생성, 다 내리고
            StartCoroutine("DropDot_reFill");

            // 0.5초 후
            yield return new WaitForSeconds(0.5f);
            Debug.Log("FillBoards Update _1");

            // 매치된 것들이 있다면
            while (MatchesOnBoard())
            {
                // 몇개가 터졌는지 ++
                streakValue++;

                // 0.5초 후
                yield return new WaitForSeconds(0.5f);
                Debug.Log("FillBoards Update _2");

                // 없애주기
                DestroyMatches();

            }
            // 현재 매치 리스트 싹 비우고
            mFindMatches.currentMatches.Clear();
            // 현재 선택된 블럭 null
            currentDot = null;

            // 옆으로 내릴 수 있다면 내리고
            // 이 부분에서 이 함수를 쓸 필요 없음. 지워도 되는 부분
            //CheckTopDots();

            // 데드락이라면, 블럭 섞어주고
            // 데드락 수정중. 이후 블럭 섞기 구현해야.
            if (IsDeadLocked())
            {
                Debug.Log("DeadLocked!");
                //StartCoroutine("ShakeBoard");
            }

            // 0.5초 후
            yield return new WaitForSeconds(0.5f);
            Debug.Log("FillBoards Update _3");
            // 상태를 move로
            currentState = GameState.move;
            // 터진 값 다시 1로
            streakValue = 1;
        }

        // 블럭을 아래칸까지 내려주기
        IEnumerator DropDotsCo()
        {
            Debug.Log("DropDots _1");
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes[i].Count; j++)
                {
                    if (nodes[i][j].GetComponent<Node>().dot != null)
                    {
                        if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.DOWN] != null)
                        {
                            // 아래칸의 블럭이 비어잇다면
                            if (nodes[i][j].GetComponent<Node>().nearNodes[(int)Dir.DOWN].GetComponent<Node>().dot == null)
                            {
                                GameObject dot = nodes[i][j].GetComponent<Node>().dot;

                                // 아래칸으로 내려주고
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

            // 장애물이 사라졌는지 검사
            CheckClear();

            // 0.3초 후
            yield return new WaitForSeconds(0.3f);
            Debug.Log("DropDots _2");

            // 게임 상태를 움직임 가능 상태로
            currentState = GameState.move;

            // 블럭을 다시 채우고 터질 것이 있다면 터트리기
            StartCoroutine("FillBoardCo");
        }

        // 터져야할 블럭 터트리기
        public void DestroyMatches()
        {
            Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();

            // 빈 노드가 없다면, 즉 노드가 다 차있다면, 채워졌다면
            if (!IsNullNode())
            {
                for (int i = 0; i < dots.Count; i++)
                {
                    for (int j = 0; j < dots[i].Count; j++)
                    {
                        if (dots[i][j] != null)
                        {
                            // 장애물이면
                            if (dots[i][j].tag == "Obstacle")
                            {
                                // 주변을 체크하여 횟수를 차감하고
                                dots[i][j].GetComponent<Obstacle>().CheckNearNodes();
                                // 다 차감 되었다면 사라지게
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

            // 블럭을 터트린 후 아래로 내리기
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
                    // 근처에 나와 같은 색들이 있는 경우 해당 노드를 check_node에 미리 넣어놔
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
                        // 체크 노드에서 꺼내서
                        GameObject checkNode = check_nodes.Dequeue();
                        Node checkNode_node = checkNode.GetComponent<Node>();

                        // 체크 노드의 주변을 검색
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
                                                // 체크 노드의 주변이지만, 검사중인 노드가 아니고
                                                if (node.GetComponent<Node>().nearNodes[k] != checkNode_node.nearNodes[i])
                                                {
                                                    // 색이 같은 경우 true
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

        // 근처에 같은 색을 가진 것들을 체크
        bool CheckNearTag(int column, int row)
        {
            GameObject nodeObj = nodes[column][row];

            if (nodeObj != null)
            {
                Node node = nodeObj.GetComponent<Node>();

                for (int i = 0; i < 3; i++)
                {
                    // 근처 노드 하나와 그 맞은편 노드를 검사
                    // 번갈아 있을 경우, 터질 수 없어.
                    // 하지만 이 경우 아예 연달아 붙어 있는 경우를 체크할 수 없어. 다른 부분에서 체크해줄거야
                    GameObject nearNodeObj = node.nearNodes[i];
                    GameObject nearNodeObj_other = node.nearNodes[i + 3];

                    if (nearNodeObj != null && nearNodeObj_other != null)
                    {
                        GameObject dot = nearNodeObj.GetComponent<Node>().dot;
                        GameObject otherDot = nearNodeObj_other.GetComponent<Node>().dot;

                        if (dot != null && otherDot != null)
                        {
                            // 근처 노드와 그 맞은편 노드의 태그가 같고
                            if (dot.tag == otherDot.tag && dot.tag != "Obstaacle")
                            {
                                int nCount = 0;

                                for (int j = 0; j < 6; j++)
                                {
                                    GameObject findSame = node.nearNodes[j];

                                    // 이들을 포함하여 색이 같은 블럭이 몇개인지 검사
                                    if (findSame != null)
                                    {
                                        if (findSame.GetComponent<Node>().dot != null)
                                        {
                                            if (findSame.GetComponent<Node>().dot.tag == dot.tag)
                                            {
                                                nCount++;
                                            }
                                        }
                                        // 3개 이상이면 true
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

        // 근처에 나와는 색이 다르지만, 그 근처에 같은 색을 가진 것들을 체크
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
                                                        if (tmp.GetComponent<Node>().dot.tag == nearNode.nearNodes[i].GetComponent<Node>().dot.tag)
                                                        {
                                                            nCount++;
                                                        }
                                                    }
                                                }

                                                if (nCount > 1)
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

                                if (nCount > 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                // 터질 수 있는 것들이 하나도 없으면 true
                if (nCount <= 0)
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
        void CheckClear()
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

        // 보드 섞기
        IEnumerator ShakeBoard()
        {
            yield return new WaitForSeconds(0.5f);
            List<GameObject> newDots = new List<GameObject>();

            Node topNode = nodes[(int)totalWidth / 2][(int)maxHeight - 1].GetComponent<Node>();
            int nCount = 0;

            // 맨 위가 비어있지 않을 때, 즉 모두 다 찼을때
            if (topNode.dot != null && dots[topNode.column][topNode.row] != null)
            {
                for (int i = 0; i < dots.Count; i++)
                {
                    for (int j = 0; j < dots[i].Count; j++)
                    {
                        // 비어있지 않고 장애물이 아니면
                        if (dots[i][j] != null && dots[i][j].tag != "Obstacle")
                        {
                            // 새 dot 목록에 넣어
                            newDots.Add(dots[i][j]);
                        }
                    }
                }

                // 0.5초 기다리고
                yield return new WaitForSeconds(0.5f);

                for (int i = 0; i < dots.Count; i++)
                {
                    for (int j = 0; j < dots[i].Count; j++)
                    {
                        // 새 dot 목록 중 랜덤으로 하나 선택
                        int dotToUse = Random.Range(0, newDots.Count);

                        int maxIteration = 0;

                        // 생성시 바로 터지게 겹치지 않는지 검사
                        while (MatchesAt(i, j, newDots[dotToUse]) && maxIteration < 100)
                        {
                            dotToUse = Random.Range(0, newDots.Count);
                            maxIteration++;
                        }
                        maxIteration = 0;

                        Dot dot = newDots[dotToUse].GetComponent<Dot>();

                        // 해당 위치에 장애물이 없으면 새로 배치
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

            if (IsDeadLocked())
            {
                StartCoroutine("ShakeBoard");
            }
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}

