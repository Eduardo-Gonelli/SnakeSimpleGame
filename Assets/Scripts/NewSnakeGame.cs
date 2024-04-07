//You are free to use this script in Free or Commercial projects
//sharpcoderblog.com @2019

using System.Collections.Generic;
using UnityEngine;

public class NewSnakeGame : MonoBehaviour
{
    //Resolucao da area do game, aumente o numero para aumentar os blocos
    public int areaLines = 20;
    public int areaColumns = 40;
    //Velocidade do movimento da cobra
    public float snakeSpeed = 10f;
    //Camera
    public Camera mainCamera;
    //Materais
    public Material groundMaterial;
    public Material snakeMaterial;
    public Material headMaterial;
    public Material fruitMaterial;

    //Sistema de grid
    Renderer[] gameBlocks;
    //Coordenadas da cobra
    List<int> snakeCoordinates = new List<int>();
    //Estado da direcao
    enum Direction { Up, Down, Left, Right };
    Direction snakeDirection = Direction.Right;
    float timeTmp = 0;
    //O bloco onde a fruta e colocada
    int fruitBlockIndex = -1;
    //Pontuacao
    int totalPoints = 0;
    //Estado do jogo
    bool gameStarted = false;
    bool gameOver = false;
    //Escalonamento da camera
    Bounds targetBounds;
    //Estilo do texto
    GUIStyle mainStyle = new GUIStyle();

    void Start()
    {
        //Gera a area de gameplay. Aqui podemos configurar varias coisas interessntes.
        gameBlocks = new Renderer[areaLines * areaColumns];
        for (int x = 0; x < areaColumns; x++)
        {
            for (int y = 0; y < areaLines; y++)
            {
                GameObject quadPrimitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadPrimitive.transform.position = new Vector3(x, 0, y);
                Destroy(quadPrimitive.GetComponent<Collider>());
                quadPrimitive.transform.localEulerAngles = new Vector3(90, 0, 0);
                quadPrimitive.transform.SetParent(transform);
                gameBlocks[(x * areaLines) + y] = quadPrimitive.GetComponent<Renderer>();
                targetBounds.Encapsulate(gameBlocks[(x * areaLines) + y].bounds);
            }
        }

        //Escala a camera para se encaixar nos blocos do game
        mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        mainCamera.orthographic = true;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = targetBounds.size.x / targetBounds.size.y;

        if (screenRatio >= targetRatio)
        {
            mainCamera.orthographicSize = targetBounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            mainCamera.orthographicSize = targetBounds.size.y / 2 * differenceInSize;
        }
        mainCamera.transform.position = new Vector3(targetBounds.center.x, targetBounds.center.y + 1, targetBounds.center.z);

        //Gera a cobra com 3 blocos
        InitializeSnake();
        ApplyMaterials();

        mainStyle.fontSize = 24;
        mainStyle.alignment = TextAnchor.MiddleCenter;
        mainStyle.normal.textColor = Color.white;
    }

    void InitializeSnake()
    {
        snakeCoordinates.Clear();
        int firstlock = Random.Range(0, areaLines - 1) + (areaLines * 3);
        snakeCoordinates.Add(firstlock);
        snakeCoordinates.Add(firstlock - areaLines);
        snakeCoordinates.Add(firstlock - (areaLines * 2));

        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, 90, 0);
        fruitBlockIndex = -1;
        timeTmp = 1;
        snakeDirection = Direction.Right;
        totalPoints = 0;
    }


    void Update()
    {
        if (!gameStarted)
        {
            if (Input.anyKeyDown)
            {
                gameStarted = true;
            }
            return;
        }
        if (gameOver)
        {
            //Cintila os blocos da cobra
            if (timeTmp < 0.44f)
            {
                timeTmp += Time.deltaTime;
            }
            else
            {
                timeTmp = 0;
                for (int i = 0; i < snakeCoordinates.Count; i++)
                {
                    if (gameBlocks[snakeCoordinates[i]].sharedMaterial == groundMaterial)
                    {
                        gameBlocks[snakeCoordinates[i]].sharedMaterial = (i == 0 ? headMaterial : snakeMaterial);
                    }
                    else
                    {
                        gameBlocks[snakeCoordinates[i]].sharedMaterial = groundMaterial;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitializeSnake();
                ApplyMaterials();
                gameOver = false;
                gameStarted = false;
            }
        }
        else
        {
            if (timeTmp < 1)
            {
                timeTmp += Time.deltaTime * snakeSpeed;
            }
            else
            {
                timeTmp = 0;
                if (snakeDirection == Direction.Right || snakeDirection == Direction.Left)
                {
                    //Detecta se a cobra acertou os lados
                    if (snakeDirection == Direction.Left && snakeCoordinates[0] < areaLines)
                    {
                        gameOver = true;
                        return;
                    }
                    else if (snakeDirection == Direction.Right && snakeCoordinates[0] >= (gameBlocks.Length - areaLines))
                    {
                        gameOver = true;
                        return;
                    }

                    int newCoordinate = snakeCoordinates[0] + (snakeDirection == Direction.Left ? -areaLines : areaLines);
                    //Detecta se a cobra acertou a si propria
                    if (snakeCoordinates.Contains(newCoordinate))
                    {
                        gameOver = true;
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        for (int i = snakeCoordinates.Count - 1; i > 0; i--)
                        {
                            snakeCoordinates[i] = snakeCoordinates[i - 1];
                        }
                        snakeCoordinates[0] = newCoordinate;
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Left ? -90 : 90), 0);
                    }
                }
                else if (snakeDirection == Direction.Up || snakeDirection == Direction.Down)
                {
                    //Detecta se a cobra acertou o topo ou a base
                    if (snakeDirection == Direction.Up && (snakeCoordinates[0] + 1) % areaLines == 0)
                    {
                        gameOver = true;
                        return;
                    }
                    else if (snakeDirection == Direction.Down && (snakeCoordinates[0] + 1) % areaLines == 1)
                    {
                        gameOver = true;
                        return;
                    }

                    int newCoordinate = snakeCoordinates[0] + (snakeDirection == Direction.Down ? -1 : 1);
                    //Se a cobra se acertar, e game over
                    if (snakeCoordinates.Contains(newCoordinate))
                    {
                        gameOver = true;
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        for (int i = snakeCoordinates.Count - 1; i > 0; i--)
                        {
                            snakeCoordinates[i] = snakeCoordinates[i - 1];
                        }
                        snakeCoordinates[0] = newCoordinate;
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Down ? 180 : 0), 0);
                    }
                }

                ApplyMaterials();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLeft();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveUP();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveDown();
            }
        }

        if (fruitBlockIndex < 0)
        {
            //Coloca o bloco da fruta
            int indexTmp = Random.Range(0, gameBlocks.Length - 1);

            //Chega se o bloco da fruta nao esta ocupado com um bloco da cobra
            for (int i = 0; i < snakeCoordinates.Count; i++)
            {
                if (snakeCoordinates[i] == indexTmp)
                {
                    indexTmp = -1;
                    break;
                }
            }

            fruitBlockIndex = indexTmp;
        }
    }

    public void MoveRight()
    {
        int newCoordinate = snakeCoordinates[0] + areaLines;
        if (!ContainsCoordinate(newCoordinate))
        {
            snakeDirection = Direction.Right;
        }
    }
    public void MoveLeft()
    {
        int newCoordinate = snakeCoordinates[0] - areaLines;
        if (!ContainsCoordinate(newCoordinate))
        {
            snakeDirection = Direction.Left;
        }
    }
    public void MoveUP()
    {
        int newCoordinate = snakeCoordinates[0] + 1;
        if (!ContainsCoordinate(newCoordinate))
        {
            snakeDirection = Direction.Up;
        }
    }
    public void MoveDown()
    {
        int newCoordinate = snakeCoordinates[0] - 1;
        if (!ContainsCoordinate(newCoordinate))
        {
            snakeDirection = Direction.Down;
        }
    }

    void ApplyMaterials()
    {
        //Aplica o material da cobra
        for (int i = 0; i < gameBlocks.Length; i++)
        {
            gameBlocks[i].sharedMaterial = groundMaterial;
            bool fruitPicked = false;
            for (int a = 0; a < snakeCoordinates.Count; a++)
            {
                if (snakeCoordinates[a] == i)
                {
                    gameBlocks[i].sharedMaterial = (a == 0 ? headMaterial : snakeMaterial);
                }
                if (snakeCoordinates[a] == fruitBlockIndex)
                {
                    //Pega a fruta
                    fruitPicked = true;
                }
            }
            if (fruitPicked)
            {
                fruitBlockIndex = -1;
                //Adiciona um novo bloco à cobra
                int snakeBlockRotationY = (int)gameBlocks[snakeCoordinates[snakeCoordinates.Count - 1]].transform.localEulerAngles.y;
                //print(snakeBlockRotationY);
                if (snakeBlockRotationY == 270)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] + areaLines);
                }
                else if (snakeBlockRotationY == 90)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] - areaLines);
                }
                else if (snakeBlockRotationY == 0)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] + 1);
                }
                else if (snakeBlockRotationY == 180)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] - 1);
                }
                totalPoints++;
            }
            if (i == fruitBlockIndex)
            {
                gameBlocks[i].sharedMaterial = fruitMaterial;
                gameBlocks[i].transform.localEulerAngles = new Vector3(90, 0, 0);
            }
        }
    }

    bool ContainsCoordinate(int coordinate)
    {
        for (int i = 0; i < snakeCoordinates.Count; i++)
        {
            if (snakeCoordinates[i] == coordinate)
            {
                return true;
            }
        }

        return false;
    }

    void OnGUI()
    {
        //Exibe a pontuacao e outras informacoes
        if (gameStarted)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 5, 200, 20), totalPoints.ToString(), mainStyle);
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 10, 200, 20), "Press Any Key to Play\n(Use Arrows to Change Direction)", mainStyle);
        }
        if (gameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 40), "Game Over\n(Press 'Space' to Restart)", mainStyle);
        }
    }
}
