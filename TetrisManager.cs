using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;

/*
구현한 내용
- 블럭 이동, 회전, 그리드 저장, 충돌 감지
- 바깥에 놓아지는 블럭 삭제하기. <<<현재 제대로 작동 안됨
- 완성된 라인 확인 및 삭제, 재배열
- 방향키 조작, 터치 조작
- 다음 블럭 미리보기, 사이즈 조정
- 게임 오버 조건 추가
- 점수 시스템 구현 (현재 점수 표시, 최고기록 저장)
- 일시정지 기능 추가
- UI 요소 추가 (시작 화면, 점수 표시 등)
- 사운드 추가

구현해야할 내용
- 난이도 조절 (레벨에 따른 속도 증가)
::1만점마다 내려오는 속도를 0.2f씩 증가하도록 하려고 했으나
::블럭을 배치하다보면 다음 블럭을 이동+회전시킬 수 있는 공간이 줄어서 자동으로 난이도가 올라간다...
::지금도 충분히 어려워서 난이도를 더 높이면 게임을 충분히 즐기지 못할 것 같다는 의견 있었음.
*/

public class TetrisManager : MonoBehaviour
{
    private AudioManager sound;
    
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Transform gameBoard;
    [SerializeField] private GameObject nextBoard;
    [SerializeField] private GameObject scoreBoard;
    [SerializeField] private float fallTime = 1.8f;
    [SerializeField] private GameObject GameOver;

    private int boardWidth = 13;
    private int boardHeight = 13;
    private int randomIndex;
    private Transform[,] grid;
    private Transform currentBlockTransform;
    private GameObject currentBlock;
    private GameObject nextBlock;
    private float previousTime;
    private bool isGameOver = false;
    private int currentScore = 0;
    private int bestScore = 0;
    private TextMeshProUGUI[] scoreText;
    private string filePath;
    private int move2step = 0;

    private void Awake()
    {
        //점수를 저장할 파일경로 설정
        filePath = Application.persistentDataPath + "/scoreRecords.txt";
        
        //파일이 존재하는 경우 기록된 최고점수 가져오기
        if (File.Exists(filePath))
        {
            using (StreamReader sr = new StreamReader(filePath)) {
                string line = sr.ReadLine();
                bestScore = int.Parse(line);
            }
        }
        else
        {
            Debug.Log("Record Not Exist");
        }

        //싱글톤 오디오매니저 인스턴스 가져오기
        sound = AudioManager.Instance;    
    }

    void Start()
    {
        //퍼블릭으로 바인딩 된 스코어보드의 텍스트들 가져오기
        scoreText = scoreBoard.GetComponentsInChildren<TextMeshProUGUI>();
        //현재 점수+최고점수 표시
        scoreText[1].text = $"{currentScore}";
        scoreText[3].text = $"{bestScore}";
        //새 그리드 만들기
        grid = new Transform[boardWidth, boardHeight];
        
        SetNextBlockIndex();
        SpawnBlock();

        currentBlockTransform = currentBlock.GetComponentInChildren<Transform>();
    }

    void Update()
    {
        if (currentBlock == null)
        {
            return;
        }

        //점수판 갱신
        scoreText[1].text = $"{currentScore}";
        
        //키보드 조작
        HandleInput();
    }

    //랜덤으로 다음블럭 인덱스 정하기
    void SetNextBlockIndex()
    {
        randomIndex = Random.Range(0, blockPrefabs.Length);
    }
    
    //다음 블럭 미리보기
    void ShowNextBlock()
    {
        if(nextBlock != null) Destroy(nextBlock.gameObject);

        //블럭 타입에 따라 알맞는 위치에 나타내기
        Transform nextBlockTransform;
        switch (randomIndex)
        {
            case 0:
                nextBlockTransform = nextBoard.transform.GetChild(0).transform;
                break;
            case 1: case 3: case 4: case 5: case 6:
                nextBlockTransform = nextBoard.transform.GetChild(1).transform;
                break;
            case 2:
                nextBlockTransform = nextBoard.transform.GetChild(2).transform;
                break;
            default:
                nextBlockTransform = nextBoard.transform.GetChild(0).transform;
                break;
        }

        nextBlockTransform.localScale = new Vector3(12, 12, 1);
        nextBlock = Instantiate(blockPrefabs[randomIndex], nextBlockTransform);
    }
    
    //블럭 생성하기
    void SpawnBlock()
    {
        int index = randomIndex;
        SetNextBlockIndex();
        ShowNextBlock();
        move2step = 0;
        
        Transform currentBlockPosition;
        //블럭 타입에 따라 알맞는 위치에 나타내기
        switch (index)
        {
            case 0:case 1: case 4: case 5:case 6:
                currentBlockPosition = transform.GetChild(0).transform;
                break;
            case 2:
                currentBlockPosition = transform.GetChild(1).transform;
                break;
            case 3:
                currentBlockPosition = transform.GetChild(2).transform;
                break;
            default:
                currentBlockPosition = transform.GetChild(0).transform;
                break;
        }

        currentBlockPosition.localScale = new Vector3(1, 1, 1);
        currentBlock = Instantiate(blockPrefabs[index], currentBlockPosition.position, Quaternion.identity);
        currentBlockTransform = currentBlock.GetComponentInChildren<Transform>(); //캐싱
        currentBlock.transform.SetParent(gameBoard);
    }

    //다른 블럭이 이미 놓아져 있으면 true반환
    bool CheckPlacedBlock(int roundedX, int roundedY)
    {
        if (grid[roundedX, roundedY] != null)
        {
            // 게임오버조건:: (10,10)에 placedBlock이 있는 경우.
            if (roundedX == 10 && roundedY == 10)
            {
                isGameOver = true;
                //최고 점수 갱신
                if (bestScore < currentScore)
                {
                    bestScore = currentScore;
                    string[] record = {bestScore.ToString()};
                    File.WriteAllLines(filePath,record);
                    Debug.Log("Score Updated");
                }
                //게임오버화면 활성화
                GameOver.SetActive(true);
                //최종 점수 표시
                GameOver.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Score: {currentScore}";
            }
            return true;
        }
        return false;
    }
    
    //블럭이 보드를 벗어나면 true 반환
    bool CheckOutOfBound(int roundedX, int roundedY)
    {
        if (roundedX < 0 || roundedX >= boardWidth || roundedY < 0 || roundedY >= boardHeight)
        {
            return true;
        }
        return false;
    }

    // 유효한 이동일때 true 반환,
    // 이미 블럭이 있거나 보드를 벗어날때 false 반환
    bool ValidMove()
    {
        foreach (Transform children in currentBlockTransform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            
            //블럭이 범위밖일때
            if (CheckOutOfBound(roundedX,roundedY)) return false;
            
            //블럭이 이미 자리에 있을때
            if(CheckPlacedBlock(roundedX, roundedY)) return false;
        }
        return true;
    }

    //블럭 이동하기
    void MoveBlock(Vector3 moveDirection, bool bForce = true)
    {
        currentBlock.transform.position += moveDirection;
        move2step++;    //처음 두 스텝만 자유롭게 움직이기 위해 추가한 부분
        
        if (bForce&&move2step>3)
        {
            bool falseMove = false;
            
            foreach (Transform children in currentBlockTransform)
            {   
                if (children.transform.position.x < -1.0f || children.transform.position.y > 10.0f || children.transform.position.x > 10.0f)
                {   //보드 밖으로 벗어나면 falseMove=true
                    falseMove = true;
                    currentBlock.transform.position -= moveDirection;
                    move2step--;
                    break;
                }
            }
            if (falseMove)
                return;
        }
        
        if (!ValidMove()) //블럭을 더 이상 움직일 수 없는 경우
        {   
            currentBlock.transform.position -= moveDirection;
            
            if (moveDirection == Vector3.left)
            {
                PlaceBlock(0,-1);
            }
            
            if (moveDirection == Vector3.down)
            {
                PlaceBlock(-1,0);
            }
        }
    }

    void PlaceBlock(int moveleft, int moveDown)
    {
        bool cantMove = false;
        int i = 0;
                
        //아래 대각선에 빈자리가 있는지 확인하고, 빈자리있으면 Place하지않음.
        foreach (Transform children in currentBlockTransform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            
            if(CheckOutOfBound(roundedX+moveleft, roundedY +moveDown))
                cantMove = true;
            else if (CheckPlacedBlock(roundedX+moveleft, roundedY+moveDown))
                cantMove = true;
            i++;
        }
                    
        //이동가능한 빈자리가 없으면 블럭을 그자리에 Place함.
        if (cantMove)
        {
            AddToGrid();
            RemoveOutOfBound();
            CheckForLines();
            if (!isGameOver)
            {
                sound.PlayBlockSound();
                SpawnBlock();
            }
        }
    }
    
    void AddToGrid()
    {
        Debug.Log("AddToGrid");
        foreach (Transform children in currentBlockTransform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            grid[roundedX, roundedY] = children;
        }
    }

    //ㅡㅡㅡㅡㅡ보드 밖의 블럭 삭제하기ㅡㅡㅡㅡㅡ
    void RemoveOutOfBound()
    {
        for (int y = boardHeight-2; y<boardHeight ; y++)
        {
            for(int x=0; x< boardWidth;x++)
            {
                if (grid[x, y])
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                    Debug.Log($"Remove x,y: {x},{y}");
                }
            }
        }
        
        for (int x = boardWidth-2; x < boardWidth; x++)
        {
            for (int y=0; y< boardHeight; y++)
            {
                if (grid[x, y])
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                    Debug.Log($"Remove x,y: {x},{y}");
                }
            }
        }

    }
    //ㅡㅡㅡㅡㅡㅡ각 행,열이 완성되었는지 체크하기ㅡㅡㅡㅡㅡㅡ
    void CheckForLines()
    {
        for (int y = boardHeight-2; y >= 0; y--)
        {
            if (HasRow(y))
            {
                DeleteRow(y);
                RearrangeRow(y);
            }
        }
        Debug.Log("Row Check Completed");

        for (int x = boardWidth-2; x >= 0; x--)
        {
            if (HasColumn(x))
            {
                DeleteColumn(x);
                RearrangeColumn(x);
            }
        }
        Debug.Log("Column Check Completed");
    }
    //완성된 행이 있는지 확인하기
    bool HasRow(int y)
    {
        for (int x = 0; x < boardWidth-2; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }
    //완성된 행 삭제
    void DeleteRow(int y)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            if (grid[x, y])
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
        currentScore += 1000;
        sound.PlayClearSound();
    }
    //블럭 재배치(행)
    void RearrangeRow(int i)
    {
        for (int y = i; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }
    //완성된 열이 있는지 확인하기
    bool HasColumn(int x)
    {
        for (int y = 0; y < boardHeight-2; y++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }
    //완성된 열 삭제
    void DeleteColumn(int x)
    {
        for (int y = 0; y < boardHeight; y++)
        {
            if (grid[x, y])
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
        currentScore += 1000;
        sound.PlayClearSound();
    }
    //블럭 재배치(열)
    void RearrangeColumn(int i)
    {
        for (int x = i; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (grid[x, y] != null && x>0)
                {
                    grid[x-1, y] = grid[x, y];
                    grid[x, y] = null;
                    grid[x-1, y].transform.position -= new Vector3(1, 0, 0);
                }
            }
        }    
    }
    
    //ㅡㅡㅡㅡㅡㅡㅡ키보드 입력 처리ㅡㅡㅡㅡㅡㅡㅡ
    void HandleInput()
    {
        // Move Left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        // Move Right
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }
        // Rotate
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateBlock();
        }
        // Move Down
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown(true);
        }
        else if (Time.time - previousTime >= fallTime)
        {
            MoveDown(false);
        }
    }
    //ㅡㅡㅡㅡㅡㅡㅡ블럭 좌우이동, 하강, 회전 처리ㅡㅡㅡㅡㅡㅡㅡ
    public void MoveLeft()
    {
        MoveBlock(Vector3.left);
        MoveBlock(Vector3.up);
    }

    public void MoveRight()
    {
        MoveBlock(Vector3.down);
        MoveBlock(Vector3.right);
    }
    public void MoveDown(bool bForce)
    {
        MoveBlock(Vector3.left, bForce);
        MoveBlock(Vector3.down, bForce);
        previousTime = Time.time;
    }
    public void RotateBlock() //회전 : 시계방향으로
    {
        currentBlock.transform.Rotate(0, 0, -90);

        if (!ValidMove())
        {
            currentBlock.transform.Rotate(0, 0, 90);
        }
        sound.PlayRotationSound();
    }
}