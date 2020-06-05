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

    private static int moduleIdCounter = 1;
    private int ModuleId;
    private bool moduleSolved;
    private bool active;

    private string[] mazes = { "▇▇▇▇▇▇▇" + //all mazes from top view
                               "▇░░░░░▇" +
                               "▇░▇░▇░▇" +
                               "▇░▇░▇░▇" +
                               "▇░▇▇▇░▇" +
                               "▇░░░░░▇" +
                               "▇▇▇▇▇▇▇",
                               "▇▇▇▇▇▇▇" +
                               "▇░░░░░▇" +
                               "▇░▇▇▇░▇" +
                               "▇░▇░░░▇" +
                               "▇░▇░▇░▇" +
                               "▇░░░░░▇" +
                               "▇▇▇▇▇▇▇",
                               "▇▇▇▇▇▇▇" +
                               "▇░░░░░▇" +
                               "▇░░░░░▇" +
                               "▇░░░░░▇" +
                               "▇░░░░░▇" +
                               "▇░░░░░▇" +
                               "▇▇▇▇▇▇▇",
                               "▇▇▇▇▇▇▇" +
                               "▇░░░░░▇" +
                               "▇░▇░▇░▇" +
                               "▇░▇░▇░▇" +
                               "▇░▇░▇░▇" +
                               "▇░▇░▇░▇" +
                               "▇▇▇▇▇▇▇",
                               "▇▇▇▇▇▇▇" +
                               "▇▇░░░▇▇" +
                               "▇░░░░░▇" +
                               "▇░░▇░░▇" +
                               "▇░░░░░▇" +
                               "▇▇░░░▇▇" +
                               "▇▇▇▇▇▇▇"};
    private string selectedMaze;
    private int playerPosition;
    private int newPlayerPosition;
    private int goalPosition;

    public SpriteRenderer[] wallRenderers;
    public Sprite[] walls;
    private List<int> displayWalls = new List<int>();

    //made for convience for display
    private int left = -1;
    private int up = -7;
    private int right = 1;
    private int down = 7;
    private int direction = 0; //0 = up, 1= right, 2=down, 3=left

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

    protected void OnNeedyActivation()
    {
        active = true;
        direction = 0;
        selectedMaze = mazes[UnityEngine.Random.Range(0,mazes.Count())]; //selects maze
        playerPosition = 0;
        while(selectedMaze[playerPosition] == '▇')
        {
            playerPosition = UnityEngine.Random.Range(0,selectedMaze.Length);
        }
        DebugMsg("" + selectedMaze[playerPosition]);
        goalPosition = 0;
        while (selectedMaze[goalPosition] == '▇' || goalPosition == playerPosition)
        {
            goalPosition = UnityEngine.Random.Range(0, selectedMaze.Length);
        }
        DebugMsg("Maze:");
        DebugMsg("" + selectedMaze[0] + selectedMaze[1] + selectedMaze[2] + selectedMaze[3] + selectedMaze[4] + selectedMaze[5] + selectedMaze[6] );
        DebugMsg("" + selectedMaze[7] + selectedMaze[8] + selectedMaze[9] + selectedMaze[10] + selectedMaze[11] + selectedMaze[12] + selectedMaze[13] );
        DebugMsg("" + selectedMaze[14] + selectedMaze[15] + selectedMaze[16] + selectedMaze[17] + selectedMaze[18] + selectedMaze[19] + selectedMaze[20] );
        DebugMsg("" + selectedMaze[21] + selectedMaze[22] + selectedMaze[23] + selectedMaze[24] + selectedMaze[25] + selectedMaze[26] + selectedMaze[27] );
        DebugMsg("" + selectedMaze[28] + selectedMaze[29] + selectedMaze[30] + selectedMaze[31] + selectedMaze[32] + selectedMaze[33] + selectedMaze[34] );
        DebugMsg("" + selectedMaze[35] + selectedMaze[36] + selectedMaze[37] + selectedMaze[38] + selectedMaze[39] + selectedMaze[40] + selectedMaze[41] );
        DebugMsg("" + selectedMaze[42] + selectedMaze[43] + selectedMaze[44] + selectedMaze[45] + selectedMaze[46] + selectedMaze[47] + selectedMaze[48] );
        DebugMsg("Player position: " + (playerPosition + 1));
        DebugMsg("Goal position: " + (goalPosition + 1));
        displayUpdate();
    }

    protected void OnNeedyDeactivation()
    {
        for (int i = 0; i < 8; i++)
        {
            wallRenderers[i].sprite = null;
        }
        active = false;
    }

    protected void OnTimerExpired()
    {
        if (active)
        {
            if(playerPosition == goalPosition)
            {
                module.OnPass();
                DebugMsg("Was on goal position when timer ran out.");
                OnNeedyDeactivation();
            }
            else
            {
                module.OnStrike();
                DebugMsg("Strike! Was not on right tile when timer ran out. Player position: " + playerPosition + ", Goal position: " + goalPosition);
                OnNeedyDeactivation();
            }
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
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
                if (direction == 0)
                {
                    newPlayerPosition = playerPosition + up;
                }
                else if(direction == 1)
                {
                    newPlayerPosition = playerPosition + right;
                }
                else if (direction == 2)
                {
                    newPlayerPosition = playerPosition + down;
                }
                else
                {
                    newPlayerPosition = playerPosition + left;
                }
                if(selectedMaze[newPlayerPosition] != '▇')
                {
                    playerPosition = newPlayerPosition;
                }
            }
            displayUpdate();
        }
    }

    void displayUpdate()
    {
        displayWalls.Clear();
        if(direction == 0) //direction is up
        {
            if(selectedMaze[playerPosition + left] == '▇')
            {
                displayWalls.Add(0);
            }
            if (selectedMaze[playerPosition + up] == '▇')
            {
                displayWalls.Add(1);
            }
            if (selectedMaze[playerPosition + right] == '▇')
            {
                displayWalls.Add(2);
            }

            if (selectedMaze[playerPosition + left + up] == '▇')
            {
                displayWalls.Add(4);
            }
            if(playerPosition + up + up > 0)
            {
                if (selectedMaze[playerPosition + up + up] == '▇')
                {
                    displayWalls.Add(5);
                }
            }
            if (selectedMaze[playerPosition + right + up] == '▇')
            {
                displayWalls.Add(6);
            }
            if (goalPosition == playerPosition + up)
            {
                displayWalls.Add(7);
            }
        }
        if (direction == 1) //direction is right
        {
            if (selectedMaze[playerPosition + up] == '▇')
            {
                displayWalls.Add(0);
            }
            if (selectedMaze[playerPosition + right] == '▇')
            {
                displayWalls.Add(1);
            }
            if (selectedMaze[playerPosition + down] == '▇')
            {
                displayWalls.Add(2);
            }
            if (selectedMaze[playerPosition + up + right] == '▇')
            {
                displayWalls.Add(4);
            }
            if (selectedMaze[playerPosition + right + right] == '▇')
            {
                displayWalls.Add(5);
            }
            if (selectedMaze[playerPosition + down + right] == '▇')
            {
                displayWalls.Add(6);
            }
            if (goalPosition == playerPosition + right)
            {
                displayWalls.Add(7);
            }
        }
        if (direction == 2) //direction is down
        {
            if (selectedMaze[playerPosition + right] == '▇')
            {
                displayWalls.Add(0);
            }
            if (selectedMaze[playerPosition + down] == '▇')
            {
                displayWalls.Add(1);
            }
            if (selectedMaze[playerPosition + left] == '▇')
            {
                displayWalls.Add(2);
            }
            if (selectedMaze[playerPosition + right + down] == '▇')
            {
                displayWalls.Add(4);
            }
            if (playerPosition + down + down < 49)
            {
                if (selectedMaze[playerPosition + down + down] == '▇')
                {
                    displayWalls.Add(5);
                }
            }
            if (selectedMaze[playerPosition + left + down] == '▇')
            {
                displayWalls.Add(6);
            }
            if (goalPosition == playerPosition + down)
            {
                displayWalls.Add(7);
            }
        }
        if (direction == 3) //direction is left
        {
            if (selectedMaze[playerPosition + down] == '▇')
            {
                displayWalls.Add(0);
            }
            if (selectedMaze[playerPosition + left] == '▇')
            {
                displayWalls.Add(1);
            }
            if (selectedMaze[playerPosition + up] == '▇')
            {
                displayWalls.Add(2);
            }

            if (selectedMaze[playerPosition + down + left] == '▇')
            {
                displayWalls.Add(4);
            }
            if (selectedMaze[playerPosition + left + left] == '▇')
            {
                displayWalls.Add(5);
            }
            if (selectedMaze[playerPosition + up + left] == '▇')
            {
                displayWalls.Add(6);
            }
            if (goalPosition == playerPosition + left)
            {
                displayWalls.Add(7);
            }
        }
        for (int i = 0; i < 8; i++)
        {
            if (!displayWalls.Contains(i))
            {
                wallRenderers[i].sprite = null;
            }
            else
            {
                wallRenderers[i].sprite = walls[i];
            }
        }
    }

    private bool isCommandValid(string cmd)
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
