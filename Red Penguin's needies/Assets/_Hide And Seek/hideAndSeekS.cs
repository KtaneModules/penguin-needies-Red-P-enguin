using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class hideAndSeekS : MonoBehaviour {

    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable[] buttons;

    static int moduleIdCounter = 1;
    int ModuleId;
    bool moduleSolved;
    bool active;

    bool[,] maze = new bool[7, 7]
      { { true , true , true , true , true , true , true },
        { true , false, false, false, false, false, true },
        { true , false, false, false, false, false, true },
        { true , false, false, false, false, false, true },
        { true , false, false, false, false, false, true },
        { true , false, false, false, false, false, true },
        { true , true , true , true , true , true , true } };
    int playerPositionX = 3;
    int playerPositionY = 3;
    int goalPositionX = 3;
    int goalPositionY = 3;

    public GameObject[] farWalls;
    public GameObject[] mediumWalls;
    public GameObject[] closeWalls;
    public GameObject[] farGoals;
    public GameObject[] closeGoals;

    int direction = 0; //0 = up, 1= right, 2=down, 3=left
    int density = 7;

    bool onGoal;

    void Awake()
    {
        ModuleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        module.OnNeedyDeactivation += OnNeedyDeactivation;
        module.OnTimerExpired += OnTimerExpired;
        foreach (KMSelectable button in buttons)
        {
            button.OnInteract += delegate () { buttonPressed(button); return false; };
        }
    }

    void OnNeedyActivation()
    {
        active = true;

        for(int i = 1; i < 6; i++)
        {
            for (int j = 1; j < 6; j++)
            {
                maze[i, j] = false;
            }
        }
        generateMaze();
        string logMaze = "";
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (maze[i, j])
                {
                    logMaze += '#';
                }
                else
                {
                    logMaze += '.';
                }
            }
            logMaze += '\n';
        }
        print(logMaze);

        direction = Random.Range(0, 3);

        playerPositionX = Random.Range(1,6);
        playerPositionY = Random.Range(1,6);
        while(maze[playerPositionX, playerPositionY])
        {
            playerPositionX = Random.Range(1, 6);
            playerPositionY = Random.Range(1, 6);
        }
        goalPositionX = Random.Range(1, 6);
        goalPositionY = Random.Range(1, 6);
        while (maze[goalPositionX, goalPositionY] || (playerPositionX == goalPositionX && playerPositionY == goalPositionY))
        {
            goalPositionX = Random.Range(1, 6);
            goalPositionY = Random.Range(1, 6);
        }

        displayUpdate();
    }

    void OnNeedyDeactivation()
    {
        active = false;
    }

    void OnTimerExpired()
    {
        if (active)
        {
            if(playerPositionX == goalPositionX && playerPositionY == goalPositionY)
            {
                module.OnPass();
            }
            else
            {
                module.OnStrike();
                DebugMsg("Strike! Was not on the goal tile when timer ran out. Player position: " + playerPositionX + ", " + playerPositionY + " Goal position: " + goalPositionX + ", " + goalPositionY);
            }
        }
        OnNeedyDeactivation();
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch(.5f);
        if (!active)
        {
            return;
        }
        else
        {
            if(pressedButton == buttons[2])
            {
                direction++;
                direction = direction % 4;
            }
            else if(pressedButton == buttons[1])
            {
                direction--;
                if(direction == -1)
                {
                    direction = 3;
                }
            }
            else
            {
                int testPositionX = playerPositionX;
                int testPositionY = playerPositionY;
                switch(direction)
                {
                    case 0:
                        testPositionY--;
                        break;
                    case 1:
                        testPositionX++;
                        break;
                    case 2:
                        testPositionY++;
                        break;
                    case 3:
                        testPositionX--;
                        break;
                }
                if (!maze[testPositionX, testPositionY])
                {
                    playerPositionX = testPositionX;
                    playerPositionY = testPositionY;
                    module.WarnAtFiveSeconds = true;

                    if (playerPositionX == goalPositionX && playerPositionY == goalPositionY)
                    {
                        module.WarnAtFiveSeconds = false;
                    }
                }
            }
            displayUpdate();
        }
    }

    void displayUpdate() //lol
    {
        closeWalls[0].SetActive(isWall(-1, -1, direction));
        closeWalls[1].SetActive(isWall(0, -1, direction));
        closeWalls[2].SetActive(isWall(1, -1, direction));

        mediumWalls[0].SetActive(isWall(-2, -2, direction));
        mediumWalls[1].SetActive(isWall(-1, -2, direction));
        mediumWalls[2].SetActive(isWall(0, -2, direction));
        mediumWalls[3].SetActive(isWall(1, -2, direction));
        mediumWalls[4].SetActive(isWall(2, -2, direction));

        farWalls[0].SetActive(isWall(-2, -3, direction));
        farWalls[1].SetActive(isWall(-1, -3, direction));
        farWalls[2].SetActive(isWall(0, -3, direction));
        farWalls[3].SetActive(isWall(1, -3, direction));
        farWalls[4].SetActive(isWall(2, -3, direction));

        closeGoals[0].SetActive(isGoal(-1, -1, direction));
        closeGoals[1].SetActive(isGoal(0, -1, direction));
        closeGoals[2].SetActive(isGoal(1, -1, direction));

        farGoals[0].SetActive(isGoal(-2, -2, direction));
        farGoals[1].SetActive(isGoal(-1, -2, direction));
        farGoals[2].SetActive(isGoal(0, -2, direction));
        farGoals[3].SetActive(isGoal(1, -2, direction));
        farGoals[4].SetActive(isGoal(2, -2, direction));
    }

    bool isWall(int shiftRight, int shiftDown, int dir)
    { //ok so i looked at the 3d maze code and had absolutely no idea what was going on. now that i've written this though i think this does approximately the same thing as it
        int x = playerPositionX; //so this code is inadvertently stolen from timwi i guess idk
        int y = playerPositionY;
        switch(dir)
        {
            case 0:
                x += shiftRight;
                y += shiftDown;
                break;
            case 1:
                x -= shiftDown;
                y += shiftRight;
                break;
            case 2:
                x -= shiftRight;
                y -= shiftDown;
                break;
            case 3:
                x += shiftDown;
                y -= shiftRight;
                break;
        }

        if (x < 0 || x >= 7 || y < 0 || y >= 7)
        {
            return false;
        }

        if(maze[x, y])
        {
            return true;
        }
        return false;
    }

    bool isGoal(int shiftRight, int shiftDown, int dir) //i could probably just stuff this into the isWall code but thog dont caare
    {
        int x = playerPositionX;
        int y = playerPositionY;
        switch (dir)
        {
            case 0:
                x += shiftRight;
                y += shiftDown;
                break;
            case 1:
                x -= shiftDown;
                y += shiftRight;
                break;
            case 2:
                x -= shiftRight;
                y -= shiftDown;
                break;
            case 3:
                x += shiftDown;
                y -= shiftRight;
                break;
        }
        if (goalPositionX == x && goalPositionY == y)
        {
            return true;
        }
        return false;
    }

    void generateMaze()
    {
        print("Density: " + density);
        bool[,] potentialMaze = new bool[5, 5];
        List<int> unusedPositions = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

        int placedWalls = 0;
        while (placedWalls < density)
        {
            int toggleNumberIndex = Random.Range(0, unusedPositions.Count);
            int toggleNumber = unusedPositions[toggleNumberIndex];
            int toggleY = toggleNumber % 5;
            int toggleX = (toggleNumber - toggleY) / 5;
            unusedPositions.RemoveAt(toggleNumberIndex);

            //print("So far there are " + placedWalls + " walls placed in the maze.");

            //print("Toggling " + toggleX + ", " + toggleY);
            //print("==TESTING==");
            potentialMaze[toggleX, toggleY] = true;

            bool[,] passThroughMaze = new bool[5,5]; //i need to do this because im pretty sure if i pass potentialMaze through isValidMaze more than once it'll return true even when it changed so :(

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    passThroughMaze[i, j] = potentialMaze[i, j];
                }
            }

            if (isValidMaze(passThroughMaze)) //i dont need to keep track of whether it can't be a valid maze or not because
            { //im pretty sure there will always be a square it can put something unless the maze is filled up and im obviously not going to do that right haha
                maze[toggleX + 1, toggleY + 1] = true;
                placedWalls++;
            }
            else
            {
                potentialMaze[toggleX, toggleY] = false;
            }
        }
        //print("Done.");
    }

    bool isValidMaze(bool[,] testMaze)
    {
        int attempts = 0;
        List<int> queuedPositions = new List<int>(0);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if(!testMaze[i, j])
                {
                    //print(i + ", " + j + " is the first open square.");
                    queuedPositions.Add(i * 5 + j);
                    i = 5;
                    j = 5;
                }
            }
        }

        while (queuedPositions.Count > 0)
        {
            attempts++;
            int testMazeY = queuedPositions[0] % 5;
            int testMazeX = (queuedPositions[0] - testMazeY) / 5;
            testMaze[testMazeX, testMazeY] = true;

            //print("Now searching " + testMazeX + ", " + testMazeY + "\'s neighbors.");
            if (testMazeX - 1 >= 0)
            {
                if(!testMaze[testMazeX - 1, testMazeY]) //the fact that if i do this condition with the other one it'll throw an out of index errors makes me go insano style
                {
                    //print("Added " + (testMazeX - 1) + ", " + testMazeY + " to the queue.");
                    testMaze[testMazeX - 1, testMazeY] = true;
                    queuedPositions.Add((testMazeX - 1) * 5 + testMazeY);
                }
            }
            if (testMazeX + 1 < 5)
            {
                if (!testMaze[testMazeX + 1, testMazeY])
                {
                    //print("Added " + (testMazeX + 1) + ", " + testMazeY + " to the queue.");
                    testMaze[testMazeX + 1, testMazeY] = true;
                    queuedPositions.Add((testMazeX + 1) * 5 + testMazeY);
                }
            }
            if (testMazeY - 1 >= 0)
            {
                if (!testMaze[testMazeX, testMazeY - 1])
                {
                    //print("Added " + testMazeX + ", " + (testMazeY - 1) + " to the queue.");
                    testMaze[testMazeX, testMazeY - 1] = true;
                    queuedPositions.Add(testMazeX * 5 + testMazeY - 1);
                }
            }
            if (testMazeY + 1 < 5)
            {
                if (!testMaze[testMazeX, testMazeY + 1])
                {
                    //print("Added " + testMazeX + ", " + (testMazeY + 1) + " to the queue.");
                    testMaze[testMazeX, testMazeY + 1] = true;
                    queuedPositions.Add(testMazeX * 5 + testMazeY + 1);
                }
            }

            /*if (attempts >= 11)
            {
                print("BREAK AT 11: Attempts: " + attempts + " Count:" + queuedPositions.Count);
                break;
            }*/
            queuedPositions.RemoveAt(0);
        }

        //print("==CONCLUDE TESTING==");

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (!testMaze[i, j])
                {
                    //print(i + ", " + j + " was not searched. Test failed.");
                    return false;
                }
            }
        }
        //print("Test passed!");
        return true;
    }

    bool isCommandValid(string cmd)
    {
        string[] validbtns = { "f","u","forward","up", "l", "left", "r", "right" };

        string[] btnSequence = cmd.ToLowerInvariant().Split(new[] { ' ' });

        foreach (var btn in btnSequence)
        {
            if (!validbtns.Contains(btn.ToLower()))
            {
                return false;
            }
        }
        return true;
    }

    public string TwitchHelpMessage = "Go forward using !{0} f/u/forward/up . Turn left with !{0} l/left . Turn right with !{0} r/right. Commands are chainable, but please use spaces inbetween commands.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == "f" || parts[i].ToLower() == "u" || parts[i].ToLower() == "forward" || parts[i].ToLower() == "up")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i].ToLower() == "l" || parts[i].ToLower() == "left")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i].ToLower() == "r" || parts[i].ToLower() == "right")
                {
                    yield return new KMSelectable[] { buttons[2] };
                }
                if (parts.Length > 1)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        else
        {
            yield return "sendtochaterror There's an invalid input.";
            yield break;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Hide and Seek #{0}] {1}", ModuleId, msg);
    }
}
