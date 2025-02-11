using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;
using UnityEditor.SceneManagement;
public class Snake : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private GameObject _tailPrefab;

    [SerializeField] private GameObject _applePrefab;

    [SerializeField] private GameObject _purplePrefab;



    private GameObject player;
    private GameObject tail;
    private GameObject apple;
    private GameObject purple;
    private int frame = 0;
    private Vector3 Direction = new Vector3(0, 1, 0);
    private string oppDirection = "";
    private bool VertDirect = true;
    private int length = 0;
    private int highscore = 0;
    public Text lengthLabel;

    public int board = 10;

    public Text highscorelabel;

    private int[] ai = {4, 1,1,1,1,1,2,3,3,3, 3, 2, 1, 1, 1, 1,  2, 3, 3, 3,  3, 2, 1, 1, 1, 1, 2, 3, 3, 3, 3,3, 4,4,4,4 };
    private int tracker = 0;

    private bool[,] grid = new bool[5, 5];
    private Vector3[] available_moves = new Vector3[100];

    private Vector3 lastTail;

    private int movement = 0;

    private bool won = false;

    //Vector3[] positionHistory= new Vector3[];



    private int computer(Vector3 current_position, Vector3 current_direction)
    {

       if (current_position == new Vector3(board-1, 1, 0))
        {
            return 3;
        }

        if (current_position == new Vector3(board-1, 0, 0))
        {
            return 4;
        }

        if (current_position == new Vector3(0, 0, 0))
        {
            return 1;
        }

       if (current_position.y == (board-1) && current_direction == new Vector3(0,1,0))
       {
            return 2;
       }

       if (current_position.y == (board-1) && current_direction == new Vector3(1,0,0))
       {
            return 3;
       }

       if (current_position.y == 1 && current_direction == new Vector3(0,-1,0))
        {
            return 2;
        }

       if (current_position.y == 1 && current_direction == new Vector3(1,0,0))
        {
            return 1;
        }

       

        return 0;
    }
    void Start()
    {
        player = Instantiate(_playerPrefab, new Vector3(0, 1), Quaternion.identity);
        spawnTail(new Vector3(0, 0));
        Debug.Log("first");
        
            spawnApple(new Vector3(0, 0, 0));
        
        highscorelabel.text = "High Score"; 
    }

    private void spawn_purple(Vector3 spawn_pos)
    {
        purple = Instantiate(_purplePrefab, spawn_pos, Quaternion.identity);
    }

    private void kill_purple()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "purple" || obj.name == "purple(Clone)")
            {
                Destroy(obj);
            }
        }
    }


    private void spawnApple(Vector3 repeat)
    {
        Vector3 position;
        bool isValidPosition;
        kill_purple();
        do
        {
            // Generate a random position within the board limits
            position = new Vector3(Random.Range(0, board), Random.Range(0, board), 0);
            spawn_purple(position);
            // Assume position is valid, and verify if it's free from collisions
            isValidPosition = true;

            // Check if position collides with player
            if (position == player.transform.position)
            {
                isValidPosition = false;
            }

            // Check if position is same as previous apple
            if (position == repeat)
            {
                isValidPosition = false;
            }



            // Check if position collides with any part of the snake's tail
            for (int i = 0; i < length && isValidPosition; i++)
            {
                GameObject tailSegment = GameObject.Find("Tail " + i);
                //Debug.Log("tail " + i+ " " + tailSegment.transform.position);
                Debug.Log(i);
                if (position == tailSegment.transform.position)
                {
                    isValidPosition = false;
                    break;
                }
            }
            

        } while (!isValidPosition); // Repeat until a valid position is found
        //kill_purple();
        //Instantiate apple at the valid position
        apple = Instantiate(_applePrefab, position, Quaternion.identity);
        apple.name = "Apple"; 
        
    }




    private void Update()
    {
        bool pressed = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || movement == 4)
        {
            if (VertDirect == true)
            {
                Debug.Log("error");
                Direction = new Vector3(-1, 0, 0);
                oppDirection = "left";
                VertDirect = false;
                pressed = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || movement == 1)
        {
            if (VertDirect == false)
            {
                Direction = new Vector3(0, 1, 0);
                oppDirection = "up";
                VertDirect = true;
                pressed = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || movement == 3)
        {
            if (VertDirect == false)
            {
                Direction = new Vector3(0, -1, 0);
                oppDirection = "down";
                VertDirect = true;
                pressed = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || movement == 2)
        {
            if (VertDirect == true)
            {
                Direction = new Vector3(1, 0, 0);
                oppDirection = "right";
                VertDirect=false;
                pressed = true;
            }
        }

        frame++;
        if (frame == 1 || pressed)
        {
            Vector3 oldPos = player.transform.position;
            findApple();
            //findApple();
            player.transform.Translate(Direction);
            //findApple();
            moveTail(oldPos, 0);
            //spawnTail(length, oldPos);
            frame = 0;

            if (tracker < 5)
            {
                tracker++;
            }
            else
            {
                tracker = 0;
            }
            movement = computer(player.transform.position, Direction);

        }
        checkDeath();

    }

    private Vector3 spawnTail(Vector3 oldPos)
    {
        tail = Instantiate(_tailPrefab, (oldPos), Quaternion.identity);
        tail.name = $"Tail {length}";
        float scaleShrink = (float)(1 - (length * 0.01));
        //tail.transform.localScale = new Vector3(scaleShrink, scaleShrink, scaleShrink);
        length++;
        lengthLabel.text = "Score: " + (length -1);
        return tail.transform.position;
    }

    private void findApple()
    {

        Vector3 futureMove = player.transform.position + Direction;

        if (futureMove == apple.transform.position)
        {
            

            tail = GameObject.Find("Tail " + (length));

            if (tail != null)
            {
                lastTail = spawnTail(tail.transform.position);
            }
            else
            {
                lastTail = spawnTail(player.transform.position);
            }
            eatApple(lastTail);
        }
    }

    private void moveTail(Vector3 position, int size)
    {
        if (size < length)
        {
            tail = GameObject.Find("Tail " + size);
            Vector3 previousPosition = tail.transform.position;
            tail.transform.position = position;
            moveTail(previousPosition, size+1);
        } 
        
    }

    private void eatApple(Vector3 EndTail)
    {
        Destroy(apple);
        Debug.Log("eatern" + length);

        if (length-1 < (board * board) -2)
        {
            spawnApple(apple.transform.position);
        }
        else
        {
            lengthLabel.text = "You won!!!!";
            highscorelabel.text = "High Score: " + (length - 1);
        }
        
    }

    private void checkDeath()
    {
        //player = GameObject.Find("Snake2(Clone)");
        

        if (player.transform.position.x > (board-1) || player.transform.position.x < 0)
        {
            Death();
        }

        if (player.transform.position.y > (board-1) || player.transform.position.y < 0)
        {
            Death();
        }

        for (int i = 0; i < length; i++)
        {
            tail = GameObject.Find("Tail " + (i));
            //Debug.Log(player.transform.position);
            //Debug.Log(tail.transform.position);
            if (player.transform.position == tail.transform.position)
            {
                //Debug.Log("mate");
                Death();
            }
        }

        


    }

    private void Death()
    {
        Debug.Log("died lol");
        for (int i = 0; i < length; i++) 
        {
            tail = GameObject.Find("Tail " + (i));
            Destroy(tail);
        }

        if (highscore < length +1)
        {
            highscore = length -1;
        }
        highscorelabel.text = "High Score: " + highscore;
        length = 0;
        player.transform.position = new Vector3(0, 1, 0);
        spawnTail(new Vector3(0, 0));
        eatApple(new Vector3(0, 0));
        Direction = new Vector3(0, 1, 0);
        VertDirect = true;

    }
}
