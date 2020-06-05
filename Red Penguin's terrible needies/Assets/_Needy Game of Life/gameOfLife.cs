using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class gameOfLife : MonoBehaviour
{

    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable[] buttons;
    public Renderer[] lights;
    public Material[] lightMats;

    private bool incorrect = false;

    private static int moduleIdCounter = 1;
    private int ModuleId;
    private bool moduleSolved;
    private bool active;

    private bool[] board = { false, false, false, false, false, false,
                             false, false, false, false, false, false,
                             false, false, false, false, false, false,
                             false, false, false, false, false, false,
                             false, false, false, false, false, false,
                             false, false, false, false, false, false,}; //extra border is used so the squares around it is pretty much ignored
    private int[] boardNumbers = { 7,8,9,10,13,14,15,16,19,20,21,22,25,26,27,28 }; //used to read the board (the edges are there not to make logs look fancy but to create an edge of empty blocks so that the blocks on the 4x4 grid will pretty much ignore them)
    private int boardDecider = 0;
    private string debugBoard = "";
    private bool[] solvedBoard = { false, false, false, false,
                                   false, false, false, false,
                                   false, false, false, false,
                                   false, false, false, false };
    private string debugSolvedBoard = "";
    private bool[] inputtedBoard = { false, false, false, false,
                                   false, false, false, false,
                                   false, false, false, false,
                                   false, false, false, false };
    private string debugInputtedBoard = "";
    private int[] lightCheckingDirections = { -7,-6,-5,-1,1,5,6,7 };
    private int lightWhiteNeighbors = 0;

    private string[] debugStrikeAreas = { "A1", "B1", "C1", "D1", "A2", "B2", "C2", "D2", "A3", "B3", "C3", "D3", "A4", "B4", "C4", "D4" };
    private List<int> debugStrikeNumbers = new List<int>();

    private string twitchPlaysCommandTextHolder = "";
    private int twitchPlaysButtonNumber = 0;

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
        debugBoard = "";
        debugSolvedBoard = "";
        active = true;
        for(int i = 0; i < 16; i++)
        {
            boardDecider = UnityEngine.Random.Range(0,2);
            if (boardDecider == 1)
            {
                board[boardNumbers[i]] = true;
                debugBoard = debugBoard + "▇";
            }
            else
            {
                board[boardNumbers[i]] = false;
                debugBoard = debugBoard + "░";
            }
        }
        DebugMsg("The Board is:");
        DebugMsg("╔════╗");
        DebugMsg("║" + debugBoard[0] + debugBoard[1] + debugBoard[2] + debugBoard[3] + "║");
        DebugMsg("║" + debugBoard[4] + debugBoard[5] + debugBoard[6] + debugBoard[7] + "║");
        DebugMsg("║" + debugBoard[8] + debugBoard[9] + debugBoard[10] + debugBoard[11] + "║");
        DebugMsg("║" + debugBoard[12] + debugBoard[13] + debugBoard[14] + debugBoard[15] + "║");
        DebugMsg("╚════╝");
        for(int i = 0; i < 16; i++)
        {
            lightWhiteNeighbors = 0;
            if (board[boardNumbers[i]] == true)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[boardNumbers[i] + lightCheckingDirections[j]])
                    {
                        lightWhiteNeighbors++;
                    }
                }
                if (lightWhiteNeighbors != 2 && lightWhiteNeighbors != 3)
                {
                    solvedBoard[i] = false;
                }
                else
                {
                    solvedBoard[i] = true;
                }
            }
            else
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[boardNumbers[i] + lightCheckingDirections[j]] == true)
                    {
                        lightWhiteNeighbors++;
                    }
                    if (lightWhiteNeighbors == 3)
                    {
                        solvedBoard[i] = true;
                    }
                    else
                    {
                        solvedBoard[i] = false;
                    }
                }
            }
        }
        for(int i = 0; i < 16; i++)
        {
            if(solvedBoard[i] == true)
            {
                debugSolvedBoard = debugSolvedBoard + "▇";
            }
            else
            {
                debugSolvedBoard = debugSolvedBoard + "░";
            }
        }
        DebugMsg("The solved board is:");
        DebugMsg("╔════╗");
        DebugMsg("║" + debugSolvedBoard[0] + debugSolvedBoard[1] + debugSolvedBoard[2] + debugSolvedBoard[3] + "║");
        DebugMsg("║" + debugSolvedBoard[4] + debugSolvedBoard[5] + debugSolvedBoard[6] + debugSolvedBoard[7] + "║");
        DebugMsg("║" + debugSolvedBoard[8] + debugSolvedBoard[9] + debugSolvedBoard[10] + debugSolvedBoard[11] + "║");
        DebugMsg("║" + debugSolvedBoard[12] + debugSolvedBoard[13] + debugSolvedBoard[14] + debugSolvedBoard[15] + "║");
        DebugMsg("╚════╝");
        setBoard();
    }

    void setBoard()
    {
        for(int i = 0; i < 16; i++)
        {
            if(board[boardNumbers[i]] == true)
            {
                lights[i].material = lightMats[0];
            }
            else
            {
                lights[i].material = lightMats[1];
            }
            inputtedBoard[i] = board[boardNumbers[i]];
        }
    }

    protected void OnNeedyDeactivation()
    {
        active = false;
    }

    protected void OnTimerExpired()
    {
        if (active)
        {
            module.OnStrike();
            DebugMsg("Strike! Ran out of time.");
            OnNeedyDeactivation();
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        if (!active)
        {
            return;
        }
        else
        {
            if(buttons[16] == pressedButton)
            {
                setBoard();
            }
            else if(buttons[17] == pressedButton)
            {
                incorrect = false;
                debugStrikeNumbers.Clear();
                for(int i = 0; i < 16; i++)
                {
                    if (inputtedBoard[i] != solvedBoard[i])
                    {
                        incorrect = true;
                        debugStrikeNumbers.Add(i);
                    }
                }
                if(!incorrect)
                {
                    module.OnPass();
                    DebugMsg("Correct input.");
                    OnNeedyDeactivation();
                }
                else
                {
                    module.OnStrike();
                    debugInputtedBoard = "";
                    for (int i = 0; i < 16; i++)
                    {
                        if (inputtedBoard[i] == true)
                        {
                            debugInputtedBoard = debugInputtedBoard + "▇";
                        }
                        else
                        {
                            debugInputtedBoard = debugInputtedBoard + "░";
                        }
                    }
                    DebugMsg("Strike! Inputted:" + debugInputtedBoard.Length);
                    DebugMsg("╔════╗");
                    DebugMsg("║" + debugInputtedBoard[0] + debugInputtedBoard[1] + debugInputtedBoard[2] + debugInputtedBoard[3] + "║");
                    DebugMsg("║" + debugInputtedBoard[4] + debugInputtedBoard[5] + debugInputtedBoard[6] + debugInputtedBoard[7] + "║");
                    DebugMsg("║" + debugInputtedBoard[8] + debugInputtedBoard[9] + debugInputtedBoard[10] + debugInputtedBoard[11] + "║");
                    DebugMsg("║" + debugInputtedBoard[12] + debugInputtedBoard[13] + debugInputtedBoard[14] + debugInputtedBoard[15] + "║");
                    DebugMsg("╚════╝");
                    DebugMsg("Expected:");
                    DebugMsg("╔════╗");
                    DebugMsg("║" + debugSolvedBoard[0] + debugSolvedBoard[1] + debugSolvedBoard[2] + debugSolvedBoard[3] + "║");
                    DebugMsg("║" + debugSolvedBoard[4] + debugSolvedBoard[5] + debugSolvedBoard[6] + debugSolvedBoard[7] + "║");
                    DebugMsg("║" + debugSolvedBoard[8] + debugSolvedBoard[9] + debugSolvedBoard[10] + debugSolvedBoard[11] + "║");
                    DebugMsg("║" + debugSolvedBoard[12] + debugSolvedBoard[13] + debugSolvedBoard[14] + debugSolvedBoard[15] + "║");
                    DebugMsg("╚════╝");
                    module.OnPass();
                    OnNeedyDeactivation();
                }
            }
            else
            {
                for(int i = 0; i < 16; i++)
                {
                    if(pressedButton == buttons[i])
                    {
                        if(inputtedBoard[i])
                        {
                            inputtedBoard[i] = false;
                            lights[i].material = lightMats[1];
                        }
                        else
                        {
                            inputtedBoard[i] = true;
                            lights[i].material = lightMats[0];
                        }
                        i = 16;
                    }
                }
            }
        }
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "a1", "b1", "c1", "d1", "a2", "b2", "c2", "d2", "a3", "b3", "c3", "d3", "a4", "b4", "c4", "d4", "submit", "reset" };

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

    public string TwitchHelpMessage = "Press a button using !{0} A1/B2/C3/D4, etc, press the reset button with !{0} reset, press the submit button with !{0} submit. Commands can be chained (example: !{0} a1 reset b2 b3 submit) ";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == "reset")
                {
                    yield return new KMSelectable[] { buttons[16] };
                }
                else if (parts[i].ToLower() == "submit")
                {
                    yield return new KMSelectable[] { buttons[17] };
                }
                else
                {
                    twitchPlaysButtonNumber = 0;
                    twitchPlaysCommandTextHolder = parts[i];
                    if(twitchPlaysCommandTextHolder[0] == 'b')
                    {
                        twitchPlaysButtonNumber = 1;
                    }
                    else if(twitchPlaysCommandTextHolder[0] == 'c')
                    {
                        twitchPlaysButtonNumber = 2;
                    }
                    else if (twitchPlaysCommandTextHolder[0] == 'd')
                    {
                        twitchPlaysButtonNumber = 3;
                    }

                    if (twitchPlaysCommandTextHolder[1] == '2')
                    {
                        twitchPlaysButtonNumber = twitchPlaysButtonNumber + 4;
                    }
                    else if (twitchPlaysCommandTextHolder[1] == '3')
                    {
                        twitchPlaysButtonNumber = twitchPlaysButtonNumber + 8;
                    }
                    else if (twitchPlaysCommandTextHolder[1] == '4')
                    {
                        twitchPlaysButtonNumber = twitchPlaysButtonNumber + 12;
                    }
                    yield return new KMSelectable[] { buttons[twitchPlaysButtonNumber] };
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
        Debug.LogFormat("[Needy Game of Life #{0}] {1}", ModuleId, msg);
    }
}

