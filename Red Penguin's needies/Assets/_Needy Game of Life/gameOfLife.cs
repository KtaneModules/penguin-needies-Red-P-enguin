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
    public KMSelectable submitButton;
    public KMSelectable resetButton;
    public Renderer[] lights;
    public Material[] lightMats;
    public Material[] strikeMats;

    static int moduleIdCounter = 1;
    int ModuleId;
    bool moduleSolved;
    bool active;

    public int shortTimer;
    public int longTimer;

    bool[,] board = new bool[4, 4];
    bool[,] solvedBoard = new bool[4, 4];
    bool[,] inputtedBoard = new bool[4, 4];
    List<bool[,]> previousBoards = new List<bool[,]>();
    bool repeatedGrid = true;

    private NeedyGameOfLifeSettings Settings;
    sealed class NeedyGameOfLifeSettings
    {
        public bool ShortenTimerAfterFirstIteration = true;
    }

    void Awake()
    {
        var modConfig = new ModConfig<NeedyGameOfLifeSettings>("NeedyGameOfLifeSettings");
        Settings = modConfig.Read();
        modConfig.Write(Settings);

        ModuleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        module.OnNeedyDeactivation += OnNeedyDeactivation;
        module.OnTimerExpired += OnTimerExpired;

        submitButton.OnInteract += delegate () { submitPressed(); return false; };
        resetButton.OnInteract += delegate () { setBoard(); return false; };
        for(int i = 0; i < 16; i++)
        {
            int dummy = i;
            buttons[dummy].OnInteract += delegate () { buttonPressed(dummy); return false; };
        }
    }

    protected void OnNeedyActivation()
    {
        active = true;
        if(Settings.ShortenTimerAfterFirstIteration)
            module.CountdownTime = shortTimer;
        if (repeatedGrid)
        {
            repeatedGrid = false;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int dummy = UnityEngine.Random.Range(0, 2);
                    if (dummy == 1) //▇░
                    {
                        board[i, j] = true;
                        solvedBoard[i, j] = true;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++) //you'll see me making each individual bool to the same instead of bool[,] = bool[,] because of sillies :PPPPPPPPPPP
            {
                for (int j = 0; j < 4; j++)
                {
                    board[i, j] = solvedBoard[i, j];
                }
            }
        }
        DebugMsg("Board:\n" + logGrid(board));

        bool allBlack = true;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int neighbors = 0;

                for(int vertOffset = -1; vertOffset <= 1; vertOffset++)
                {
                    for (int horizOffset = -1; horizOffset <= 1; horizOffset++)
                    {
                        if(vertOffset != 0 || horizOffset != 0)
                        {
                            int searchY = i + vertOffset;
                            int searchX = j + horizOffset;
                            if (searchY >= 0 && searchX >= 0 && searchY < 4 && searchX < 4)
                            {
                                if (board[searchY, searchX])
                                    neighbors++;
                            }
                        }
                    }
                }

                if(board[i, j] == true)
                {
                    if ((neighbors < 2 || neighbors > 3))
                    {
                        solvedBoard[i, j] = false;
                    }
                    else
                        allBlack = false;
                }
                else if(neighbors == 3)
                {
                    solvedBoard[i, j] = true;
                    allBlack = false;
                }
            }
        }
        DebugMsg("Solution:\n" + logGrid(solvedBoard));

        if (checkForRepeatedBoards() || allBlack)
        {
            repeatedBoardSetup();
        }
        else
        {
            bool[,] dummy = new bool[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    dummy[i, j] = solvedBoard[i, j];
                }
            }
            previousBoards.Add(dummy);
        }

        setBoard();
    }

    void setBoard()
    {
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                inputtedBoard[i, j] = board[i, j];
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                if (board[i, j] == true)
                {
                    lights[i * 4 + j].material = lightMats[0];
                }
                else
                {
                    lights[i * 4 + j].material = lightMats[1];
                }
            }
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
            module.CountdownTime = longTimer;
            OnNeedyDeactivation();
        }
    }

    void buttonPressed(int pressedButton)
    {
        if (!active)
        {
            return;
        }

        int column = pressedButton % 4;
        int row = (pressedButton - column) / 4;

        if (inputtedBoard[row, column])
        {
            inputtedBoard[row, column] = false;
            lights[pressedButton].material = lightMats[1];
        }
        else
        {
            inputtedBoard[row, column] = true;
            lights[pressedButton].material = lightMats[0];
        }
    }

    void submitPressed()
    {
        if (!active)
        {
            return;
        }

        List<int> incorrectSquares = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                if (inputtedBoard[i, j] != solvedBoard[i, j])
                {
                    incorrectSquares.Add(i * 4 + j);
                }
            }
        }
        if(incorrectSquares.Count > 0)
        {
            module.OnStrike();
            DebugMsg("Strike! Inputted:\n" + logGrid(inputtedBoard));
            DebugMsg("Expected:\n" + logGrid(solvedBoard));
            module.OnPass();
            for(int i = 0; i < incorrectSquares.Count; i++)
            {
                int squareNumber = incorrectSquares[i];
                if (inputtedBoard[(squareNumber - (squareNumber % 4)) / 4, squareNumber % 4])
                {
                    lights[incorrectSquares[i]].material = strikeMats[0];
                }
                else
                {
                    lights[incorrectSquares[i]].material = strikeMats[1];
                }
            }

            module.CountdownTime = longTimer;
            OnNeedyDeactivation();
            return;
        }

        module.OnPass();
        DebugMsg("Correct input.");
        OnNeedyDeactivation();
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "submit", "reset" };
        char[] gridLetters = { 'a','b','c','d' };
        char[] gridNumbers = { '1','2','3','4' };

        string[] btnSequence = cmd.ToLowerInvariant().Split(new[] { ' ' });

        foreach (var btn in btnSequence)
        {
            if (validbtns.Contains(btn.ToLower()))
            {
                return true;
            }
            else if(btn.Length >= 2)
            {
                if (gridLetters.Contains(btn[0]) && gridNumbers.Contains(btn[1]))
                {
                    return true;
                }
            }
        }
        return false;
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
                    setBoard();
                }
                else if (parts[i].ToLower() == "submit")
                {
                    submitPressed();
                }
                else
                {
                    int buttonInputted = 0;
                    switch (parts[i][0])
                    {
                        case 'b':
                            buttonInputted = 1;
                            break;
                        case 'c':
                            buttonInputted = 2;
                            break;
                        case 'd':
                            buttonInputted = 3;
                            break;
                        default: //a
                            break;
                    }
                    switch (parts[i][1])
                    {
                        case '2':
                            buttonInputted += 4;
                            break;
                        case '3':
                            buttonInputted += 8;
                            break;
                        case '4':
                            buttonInputted += 12;
                            break;
                        default: //1
                            break;
                    }
                    buttonPressed(buttonInputted);
                }
                yield return new WaitForSeconds(.05f);
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

    string logGrid(bool[,] grid)
    {
        string log = "";
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                if(grid[i,j] == true)
                {
                    log += '▇';
                }
                else
                {
                    log += '░';
                }
            }
            log += '\n';
        }
        return log;
    }

    bool checkForRepeatedBoards()
    {
        for(int k = 0; k < previousBoards.Count; k++)
        {
            for(int i = 0; i < 4; i++)
            {
                bool doesntRepeat = false;
                for (int j = 0; j < 4; j++)
                {
                    if (previousBoards[k][i, j] != solvedBoard[i, j])
                    {
                        doesntRepeat = true;
                        break;
                    }
                }

                if(doesntRepeat)
                {
                    break;
                }
                else if(i == 3)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void repeatedBoardSetup()
    {
        repeatedGrid = true;
        module.CountdownTime = longTimer;
        previousBoards.Clear();
    }
}