using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class permutationsScript : MonoBehaviour {
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    List<KMSelectable> squaresSelectables = new List<KMSelectable>();
    int squaresAmount = 0;

    int[][] piecesPerRow = new int[20][] {
        new int[]{ 1 },
        new int[]{ 2 },
        new int[]{ 3 },
        new int[]{ 4 },
        new int[]{ 3, 2 },
        new int[]{ 3, 3 },
        new int[]{ 2, 3, 2 },
        new int[]{ 4, 4 },
        new int[]{ 3, 3, 3 },
        new int[]{ 3, 4, 3 },
        new int[]{ 4, 3, 4 },
        new int[]{ 4, 4, 4 },
        new int[]{ 4, 5, 4 },
        new int[]{ 5, 4, 5 },
        new int[]{ 5, 5, 5 },
        new int[]{ 4, 4, 4, 4 },
        new int[]{ 5, 5, 4, 3 },
        new int[]{ 4, 5, 5, 4 },
        new int[]{ 5, 5, 5, 4 },
        new int[]{ 5, 5, 5, 5 } };

    public Mesh[] patternMeshes;
    public Material[] patternMaterials;
    public GameObject squareSpawner;
    public GameObject squarePrefab;
    List<int[]> squarePatterns = new List<int[]>();

    static int moduleIdCounter = 1;
    int ModuleId;
    bool moduleSolved;
    bool active;

    List<int[]> usedPermutations = new List<int[]>() /*{ new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 } }*/;
    int[] currentPermutation = new int[1] /*{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }*/;
    int maxUsedPermutations;
    bool squareSelected;
    int selectedSquare;
    bool inAnimation;

    void Awake()
    {
        ModuleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        module.OnNeedyDeactivation += OnNeedyDeactivation;
        module.OnTimerExpired += OnTimerExpired;
    }

    void Start()
    {
        generateNewSquare();
        generateNewSquare();
        generateNewSquare();
        positionSquares();
    }

    protected void OnNeedyActivation()
    {
        active = true;
        print(squaresSelectables.Count);
        module.CountdownTime = squaresSelectables.Count * 5;
        module.SetNeedyTimeRemaining(module.CountdownTime);
    }

    protected void OnNeedyDeactivation()
    {
        module.HandlePass();
        if (usedPermutations.Count >= maxUsedPermutations)
        {
            StartCoroutine(AddNewSquare());
        }
        active = false;
    }

    IEnumerator AddNewSquare()
    {
        yield return new WaitUntil(() => !inAnimation);
        generateNewSquare();
        updatePermutations();
        positionSquares();
    }

    protected void OnTimerExpired()
    {
        module.HandleStrike();
        module.OnNeedyDeactivation();
    }

    void buttonPressed(int pressedButton)
    {
        if (!active || inAnimation)
            return;

        squaresSelectables[pressedButton].AddInteractionPunch(.3f);
        //print("Button " + pressedButton + " pressed.");
        if (!squareSelected)
        {
            int pressedButtonIndex = 0;
            for (int i = 0; i < squaresAmount; i++)
            {
                if (currentPermutation[i] == pressedButton)
                {
                    pressedButtonIndex = i;
                    break;
                }
            }

            squareSelected = true;
            selectedSquare = pressedButton;
            StartCoroutine(SelectedIdleAnimation(squaresSelectables[currentPermutation[pressedButtonIndex]]));
            audio.PlaySoundAtTransform("medium", transform);
            return;
        }

        squareSelected = false;
        if (pressedButton != selectedSquare)
        {
            //print("Swapped " + pressedButton + " with " + selectedSquare + ".");

            int selectedSquareIndex = 0;
            int pressedButtonIndex = 0;
            for(int i = 0; i < squaresAmount; i++)
            {
                if(currentPermutation[i] == selectedSquare)
                {
                    selectedSquareIndex = i;
                }
                if (currentPermutation[i] == pressedButton)
                {
                    pressedButtonIndex = i;
                }
            }

            int temp = currentPermutation[selectedSquareIndex];
            currentPermutation[selectedSquareIndex] = currentPermutation[pressedButtonIndex];
            currentPermutation[pressedButtonIndex] = temp;

            audio.PlaySoundAtTransform("high", transform);
            StartCoroutine(SwapTwoSquaresAnimation(squaresSelectables[currentPermutation[selectedSquareIndex]], squaresSelectables[currentPermutation[pressedButtonIndex]]));
        }
        else
        {
            audio.PlaySoundAtTransform("low", transform);
        }
        //print(currentPermutation[0] + " " + currentPermutation[1] + " " + currentPermutation[2] + " " + currentPermutation[3]);
        //logListIntArray();

        if (!permutationUsed(currentPermutation))
        {
            usedPermutations.Add(copyPermutation(currentPermutation));
            module.OnNeedyDeactivation();
        }
    }

    void generateNewSquare()
    {
        squaresAmount++;

        GameObject newSquare = Instantiate(squarePrefab, squareSpawner.transform);
        GameObject squarePattern = newSquare.transform.GetChild(0).gameObject;

        SelectPattern:
        int randomBackingMaterial = Random.Range(0, patternMaterials.Length);
        int randomPatternMaterial = Random.Range(0, patternMaterials.Length);
        int randomPatternMesh = Random.Range(0, patternMeshes.Length);
        if(randomBackingMaterial == randomPatternMaterial || squarePatterns.Contains(new int[3] { randomBackingMaterial, randomPatternMaterial, randomPatternMesh }))
        {
            goto SelectPattern;
        }
        squarePatterns.Add(new int[3] { randomBackingMaterial, randomPatternMaterial, randomPatternMesh });

        newSquare.GetComponent<Renderer>().material = patternMaterials[randomBackingMaterial];
        squarePattern.GetComponent<MeshFilter>().mesh = patternMeshes[randomPatternMesh];
        squarePattern.GetComponent<Renderer>().material = patternMaterials[randomPatternMaterial];

        newSquare.GetComponent<KMSelectable>().Parent = module.GetComponent<KMSelectable>();
        squaresSelectables.Add(newSquare.GetComponent<KMSelectable>());
        module.GetComponent<KMSelectable>().Children = squaresSelectables.ToArray();
        module.GetComponent<KMSelectable>().UpdateChildrenProperly();
        int dummy = squaresAmount - 1;
        newSquare.GetComponent<KMSelectable>().OnInteract += delegate () { buttonPressed(dummy); return false; };
        StartCoroutine(SquareSpawnAnimation(newSquare));

        updatePermutations();
    }

    void positionSquares()
    {
        int[] currentPositions = squarePatterns[squarePatterns.Count - 1];
        int currentSquare = 0;
        float amountBetween = .035f;

        int numberOfRows = piecesPerRow[squaresAmount - 1].Length;
        float shiftDown = amountBetween * ((numberOfRows - 1f) / 2f);
        for (int i = 0; i < numberOfRows; i++)
        {
            int piecesInThisRow = piecesPerRow[squaresAmount - 1][i];
            //print(squaresAmount + " " + piecesInThisRow);
            float newZ = (-amountBetween * i) + shiftDown;

            float shiftToLeft = amountBetween * ((piecesInThisRow - 1f) / 2f);
            //print(amountBetween + " " + shiftToLeft);
            for (int j = 0; j < piecesInThisRow; j++)
            {
                //print((amountBetween * j) + " " + ((amountBetween * j) - shiftToLeft));
                float newX = (amountBetween * j) - shiftToLeft;
                //print("new X: " + newX);
                //squaresSelectables[usedPermutations[usedPermutations.Count - 1][currentSquare]].transform.localPosition = new Vector3(newX, 0, newZ);
                KMSelectable square = squaresSelectables[usedPermutations[usedPermutations.Count - 1][currentSquare]];
                bool wantBounce = false;
                if(currentSquare == squaresAmount - 1)
                {
                    square.transform.localPosition = new Vector3(newX, 0, newZ);
                }
                else
                {
                    wantBounce = square.transform.localPosition.z != newZ;
                    StartCoroutine(MoveSquareAnimation(square, square.transform.localPosition, new Vector3(newX, 0, newZ), wantBounce));
                }
                //print("position: " + squaresSelectables[usedPermutations[usedPermutations.Count - 1][currentSquare]].transform.localPosition.x);
                currentSquare++;
            }
        }
    }

    void updatePermutations()
    {
        //print("Length: " + squaresAmount);

        int[] lastPermutation = new int[squaresAmount];
        //string debug = "";
        for(int i = 0; i < squaresAmount - 1; i++)
        {
            lastPermutation[i] = usedPermutations[usedPermutations.Count - 1][i];
            //debug += " " + usedPermutations[usedPermutations.Count - 1][i];
        }
        lastPermutation[squaresAmount - 1] = squaresAmount - 1;
        //print("New permutation: " + debug + " " + lastPermutation[squaresAmount - 1]);
        usedPermutations.Clear();
        usedPermutations.Add(copyPermutation(lastPermutation));
        currentPermutation = lastPermutation;

        maxUsedPermutations = 1;
        for(int i = 1; i < squaresAmount + 1; i++)
        {
            maxUsedPermutations *= i;
        }
        //print("Factorial: " + maxUsedPermutations);
    }

    bool permutationUsed(int[] permutation)
    {
        for(int i = 0; i < usedPermutations.Count; i++)
        {
            for(int j = 0; j < usedPermutations[i].Length; j++)
            {
                if(usedPermutations[i][j] != permutation[j])
                {
                    j = usedPermutations[i].Length;
                }
                else if(j == usedPermutations[i].Length - 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void logListIntArray()
    {
        for (int i = 0; i < usedPermutations.Count; i++)
        {
            string debug = "";
            for (int j = 0; j < usedPermutations[i].Length; j++)
            {
                debug += " " + usedPermutations[i][j];
            }
            Log(debug);
        }
    }

    IEnumerator SquareSpawnAnimation(GameObject square)
    {
        Vector3 zero = new Vector3(0, 0, 0);
        Vector3 startingRotation = new Vector3(90, -360, 0);
        square.transform.localScale = zero;
        float t = 0;
        while(t < 1)
        {
            square.transform.localScale = Vector3.Lerp(zero, new Vector3(0.3333333f, 0.3333333f, 1), t);
            square.transform.localRotation = Quaternion.Euler(Vector3.Lerp(startingRotation, new Vector3(90, 0, 0), t));
            t += .03f;
            yield return new WaitForSeconds(.01f);
        }
        square.transform.localScale = new Vector3(0.3333333f, 0.3333333f, 1);
        square.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    IEnumerator MoveSquareAnimation(KMSelectable squareObject, Vector3 startingPosition, Vector3 endingPosition, bool bounce)
    {
        float t = 0f;
        while(t < 1)
        {
            t += .05f;
            squareObject.transform.localPosition = Vector3.Lerp(startingPosition, endingPosition, t);
            if(bounce)
            {
                float bounceAmount = .16f * -Mathf.Pow(t - .5f, 2) + .04f;
                print("t: " + t + " bounce: " +  bounceAmount);
                squareObject.transform.localPosition = new Vector3(squareObject.transform.localPosition.x, squareObject.transform.localPosition.y + bounceAmount, squareObject.transform.localPosition.z);
            }
            yield return new WaitForSeconds(.01f);
        }
        squareObject.transform.localPosition = endingPosition;
    }

    IEnumerator SelectedIdleAnimation(KMSelectable squareObject)
    {
        float t = 0f;

        while (squareSelected)
        {
            t += .02f;
            float scaleAmount = Mathf.Sin(t) * .0333333f;
            squareObject.transform.localScale = new Vector3(0.3333333f + scaleAmount, 0.3333333f + scaleAmount, 1f);
            yield return new WaitForSeconds(.01f);
        }
        t = 0;
        Vector3 currentScale = squareObject.transform.localScale;
        Vector3 wantedScale = new Vector3(0.3333333f, 0.3333333f, 1f);
        while (t < 1)
        {
            t += .05f;
            squareObject.transform.localScale = Vector3.Lerp(currentScale, wantedScale, t);
            yield return new WaitForSeconds(.01f);
        }
        squareObject.transform.localScale = wantedScale;
    }

    IEnumerator SwapTwoSquaresAnimation(KMSelectable squareOne, KMSelectable squareTwo)
    {
        inAnimation = true;
        Vector3 squareOneStartingPosition = squareOne.transform.localPosition;
        Vector3 squareTwoStartingPosition = squareTwo.transform.localPosition;

        Vector3 midpoint = Vector3.Lerp(squareOne.transform.localPosition, squareTwo.transform.localPosition, 0.5f);
        float adjacent = squareOne.transform.localPosition.x - midpoint.x;
        float opposite = squareOne.transform.localPosition.z - midpoint.z;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(adjacent, 2) + Mathf.Pow(opposite, 2));
        float t = 0; //i would probably do Mathf.Atan but that can run into divide by 0 :zany:
        float bouncet = 0;

        if (adjacent < 0)
        {
            if (opposite < 0)
            {
                t = Mathf.Abs(Mathf.Asin(opposite / hypotenuse)) + Mathf.PI;
            }
            else
            {
                t = Mathf.Acos(adjacent / hypotenuse);
            }
        }
        else
        {
            t = Mathf.Asin(opposite / hypotenuse);
        }

        float maximumt = t + Mathf.PI - .09f;

        //print("Midpoint: " + midpoint.x + ", " + midpoint.y + " Adjacent: " + adjacent + " Opposite: " + opposite + " Hypotenuse: " + hypotenuse + " Starting angle (radians):" + t + " Ending angle: " + maximumt);

        while (t < maximumt)
        {
            t += .1f;
            bouncet += .1f / Mathf.PI;
            float bounceAmount = .08f * -Mathf.Pow(bouncet - .5f, 2) + .02f;

            squareOne.transform.localPosition = new Vector3(midpoint.x + Mathf.Cos(t) * hypotenuse, squareOneStartingPosition.y + bounceAmount, midpoint.z + Mathf.Sin(t) * hypotenuse);
            squareTwo.transform.localPosition = new Vector3(midpoint.x + Mathf.Cos(t + Mathf.PI) * hypotenuse, squareTwoStartingPosition.y + bounceAmount, midpoint.z + Mathf.Sin(t + Mathf.PI) * hypotenuse);
            yield return new WaitForSeconds(.01f);
        }
        inAnimation = false;

        squareOne.transform.localPosition = squareTwoStartingPosition;
        squareTwo.transform.localPosition = squareOneStartingPosition;
        
        squareOne.AddInteractionPunch(0.125f);
    }

    int[] copyPermutation(int[] copyThis) //i have to use this because List<int[]> sux
    {
        int[] copied = new int[copyThis.Length];
        for(int i = 0; i < copyThis.Length; i++)
        {
            copied[i] = copyThis[i];
        }
        return copied;
    }

    public string TwitchHelpMessage = "Press a square with !{0} press 1, with the position of the buttons you want to press in reading order. Commands can stack (!{0} press 1 2 3 4). \"press\" is optional.";
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        string[] parts = command.Split(' ');

        List<int> buttonPresses = new List<int>();
        for (int i = 0; i < parts.Length; i++)
        {
            if (onlyNumbers(parts[i]) || (i == 0 && parts[i] == "press"))
            {
                int buttonNumber = int.Parse(parts[i]);
                if(buttonNumber > squaresAmount)
                {
                    yield return "sendtochaterror There is not a " + buttonNumber + "th button on the module!";
                    yield break;
                }
                buttonPresses.Add(currentPermutation[buttonNumber - 1]);
            }
            else
            {
                yield return "sendtochaterror There's a letter somewhere in your command!";
                yield break;
            }
        }

        for(int i = 0; i < buttonPresses.Count; i++)
        {
            yield return new WaitUntil(() => !inAnimation);
            buttonPressed(buttonPresses[i]);
            if (i < buttonPresses.Count - 1)
            {
                yield return new WaitForSeconds(.05f);
            }
        }
    }

    bool onlyNumbers(string testThis)
    {
        foreach(char character in testThis)
        {
            if (char.IsLetter(character))
                return false;
        }
        return true;
    }

    void Log(string msg)
    {
        Debug.LogFormat("[Permutations #{0}] {1}", ModuleId, msg);
    }
}
