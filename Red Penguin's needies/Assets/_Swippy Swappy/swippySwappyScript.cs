using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class swippySwappyScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable theButton;
    float rotationThreshold = 91f;
    bool facingBack;

    private static int moduleIdCounter = 1;
    private int ModuleId;
    private bool moduleSolved;
    bool active;

    int numberOfBlocks;
    List<int> availableBlocks = new List<int>();
    int[] blocks;
    bool sorted;

    public GameObject blockParent;
    public GameObject blockPrefab;
    GameObject[] blockObjects;
    Color[] blockColors;
    public Color firstColor;
    public Color lastColor;

    public AudioSource audioSource;
    public GameObject digitsObject;
    public GameObject[] leftDigitSegments;
    public GameObject[] rightDigitSegments;

    private SwippySwappySettings Settings;
    sealed class SwippySwappySettings
    {
        public int NumberOfBlocks = 5;
        public bool AnimateTimer = true;
    }

    void Awake()
    {
        ModuleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        theButton.OnInteract += delegate () { buttonPressed(); return false; };
    }

    protected void OnNeedyActivation()
    {
        var modConfig = new ModConfig<SwippySwappySettings>("SwippySwappySettings");
        Settings = modConfig.Read();
        modConfig.Write(Settings);

        numberOfBlocks = Settings.NumberOfBlocks;
        blocks = new int[numberOfBlocks];
        blockObjects = new GameObject[numberOfBlocks];
        blockColors = new Color[numberOfBlocks];
        for (int i = 0; i < numberOfBlocks; i++)
        {
            availableBlocks.Add(i);
        }

        float spaceBetweenBlocks = Mathf.Min(.005f, .05f / numberOfBlocks);
        float blockWidth = (.13f - spaceBetweenBlocks * (numberOfBlocks - 1)) / numberOfBlocks;
        float blockOffset = .13f / numberOfBlocks;
        float startingBlockX = blockOffset * (numberOfBlocks - 1) / -2f;

        for (int i = 0; i < numberOfBlocks; i++)
        {
            int index = Random.Range(0, availableBlocks.Count);
            blocks[i] = availableBlocks[index];

            float lerpAmount = blocks[i] / (numberOfBlocks - 1f);
            blockObjects[blocks[i]] = Instantiate(blockPrefab, new Vector3(0, 0, 0), Quaternion.identity, blockParent.transform);
            blockObjects[blocks[i]].transform.localPosition = new Vector3(startingBlockX + blockOffset * i, 0.014f, Mathf.Lerp(-0.0225f, 0f, lerpAmount));
            blockObjects[blocks[i]].transform.localScale = new Vector3(blockWidth, 0.015f, Mathf.Lerp(0.015f, 0.06f, lerpAmount));
            blockObjects[blocks[i]].transform.localEulerAngles = new Vector3(0,0,0);
            blockColors[blocks[i]] = Color.Lerp(firstColor, lastColor, lerpAmount);
            blockObjects[blocks[i]].GetComponent<Renderer>().material.color = blockColors[blocks[i]];

            availableBlocks.RemoveAt(index);
        }
        DebugMsg("The blocks' initial configuration is" + logConfiguration() + ".");

        facingBack = Mathf.Abs(currentRotation()) > rotationThreshold;
        if (!facingBack)
            StartCoroutine(AnimateCurtainsOpening());
        active = true;

        if (Settings.AnimateTimer)
        {
            if (!Application.isEditor)
            {
                GameObject originalTimerText = gameObject.GetComponentInChildren<RectTransform>().gameObject;
                if (originalTimerText != null)
                    originalTimerText.SetActive(false);
            }

            digitsObject.SetActive(true);
            StartCoroutine(AnimateDigits());
        }
    }

    void Update()
    {
        if (!active)
            return;

        bool currentlyFacingBack = Mathf.Abs(currentRotation()) > rotationThreshold;

        if (facingBack && !currentlyFacingBack)
        {
            //DebugMsg("OPEN | Euler angles: " + gameObject.transform.eulerAngles + " | Current rotation: " + currentRotation());
            StartCoroutine(AnimateCurtainsOpening());
            swapBlocks();
        }
        if(!facingBack && currentlyFacingBack)
        {
            //DebugMsg("CLOSE | Euler angles: " + gameObject.transform.eulerAngles + " | Current rotation: " + currentRotation());
            StartCoroutine(AnimateCurtainsClosing());
            if (sorted)
            {
                module.HandleStrike();
                sorted = false;
                DebugMsg("Strike! Did not recieve a button press when the list was sorted.");
            }
        }

        facingBack = currentlyFacingBack;
        module.SetNeedyTimeRemaining(0.5f); //LAUGH HA HA HA FUNNY NUMBER
    }

    void buttonPressed()
    {
        theButton.AddInteractionPunch(.4f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        if (!active)
            return;

        if (!sorted)
        {
            module.HandleStrike();
            DebugMsg("Strike! Pressed button when list wasn't sorted.");
        }
        else
        {
            DebugMsg("Thank you for the yummy input :)");
            sorted = false;
            StartCoroutine(AnimateCorrectPress());
        }
    }

    void swapBlocks()
    {
        int firstIndex = Random.Range(0, numberOfBlocks);
        int secondIndex = Random.Range(0, numberOfBlocks);
        while (secondIndex == firstIndex)
        {
            secondIndex = Random.Range(0, numberOfBlocks);
        }

        //Vector3 tempPos = blockObjects[blocks[firstIndex]].transform.localPosition;
        Vector3 tempPos = new Vector3(blockObjects[blocks[firstIndex]].transform.localPosition.x, .014f, blockObjects[blocks[secondIndex]].transform.localPosition.z);
        blockObjects[blocks[firstIndex]].transform.localPosition = new Vector3(blockObjects[blocks[secondIndex]].transform.localPosition.x, .014f, blockObjects[blocks[firstIndex]].transform.localPosition.z);
        blockObjects[blocks[secondIndex]].transform.localPosition = tempPos;

        int temp = blocks[firstIndex];
        blocks[firstIndex] = blocks[secondIndex];
        blocks[secondIndex] = temp;

        DebugMsg("Swapped block in position " + (firstIndex + 1) + " with block in position " + (secondIndex + 1) + ". New configuration:" + logConfiguration() + ".");

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != i)
                break;
            else if (i == blocks.Length - 1)
            {
                DebugMsg("The list is now sorted! Expecting a button press.");
                sorted = true;
            }
        }
    }

    float currentRotation()
    {
        Vector3 eulerAngles = gameObject.transform.eulerAngles;
        float rotation = eulerAngles.z;

        if (eulerAngles.y >= 179 && eulerAngles.y < 360)
            rotation += eulerAngles.y;
        while (rotation > 359.9f)
            rotation -= 360;

        if (rotation > 180)
            rotation -= 360; //equals the same but rotates the other way, makes comparison easier

        return rotation;
    }

    public GameObject leftCurtain;
    public GameObject rightCurtain;

    Vector3 startingScale = new Vector3(.5f, 1.6f, .5f);
    Vector3 endingScale = new Vector3(.075f, 1.6f, .5f);

    float startingX = .0314f;
    float endingX = .0314f*1.7f;

    IEnumerator AnimateCurtainsOpening()
    {
        float t = 0;
        while(t < 1.1)
        {
            leftCurtain.transform.localScale = Vector3.Lerp(startingScale, endingScale, t);
            leftCurtain.transform.localPosition = new Vector3(Mathf.Lerp(-startingX, -endingX, t), .0069f, 0f);
            rightCurtain.transform.localScale = Vector3.Lerp(startingScale, endingScale, t);
            rightCurtain.transform.localPosition = new Vector3(Mathf.Lerp(startingX, endingX, t), .0069f, 0f);
            t += .1f;
            yield return new WaitForSeconds(.01f);
        }
    }

    IEnumerator AnimateCurtainsClosing()
    {
        float t = 1;
        while (t > -.1)
        {
            leftCurtain.transform.localScale = Vector3.Lerp(startingScale, endingScale, t);
            leftCurtain.transform.localPosition = new Vector3(Mathf.Lerp(-startingX, -endingX, t), .0069f, 0f);
            rightCurtain.transform.localScale = Vector3.Lerp(startingScale, endingScale, t);
            rightCurtain.transform.localPosition = new Vector3(Mathf.Lerp(startingX, endingX, t), .0069f, 0f);
            t -= .1f;
            yield return new WaitForSeconds(.01f);
        }
    }

    IEnumerator AnimateCorrectPress()
    {
        float timeBetweenColors = 1f / numberOfBlocks;
        print(timeBetweenColors);
        for(int i = 0; i < numberOfBlocks; i++)
        {
            audioSource.volume = Wawa.DDL.Preferences.Sound;
            audioSource.pitch = Mathf.Lerp(1, 2f, (float)i / (numberOfBlocks - 1));
            audioSource.Play();
            StartCoroutine(FlashBlock(i));
            yield return new WaitForSeconds(timeBetweenColors);
        }
    }
    IEnumerator FlashBlock(int blockIndex)
    {
        float t = 0;
        while (t <= 1)
        {
            blockObjects[blockIndex].GetComponent<Renderer>().material.color = Color.Lerp(Color.green, blockColors[blockIndex], t);
            t += .1f;
            yield return new WaitForSeconds(.01f);
        }
    }

    IEnumerator AnimateDigits()
    {
        int index = 5;
        while (true)
        {
            leftDigitSegments[index].SetActive(false);
            rightDigitSegments[index].SetActive(false);

            index++;
            if (index >= 6)
                index = 0;

            leftDigitSegments[index].SetActive(true);
            rightDigitSegments[index].SetActive(true);

            yield return new WaitForSeconds(.1f);
        }
    }

    public string TwitchHelpMessage = "Press the button using !{0} press/sorted.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        string[] acceptableCommands = new string[] { "sorted","press" };

        if (acceptableCommands.Contains(cmd.ToLower()))
        {
            buttonPressed();
            yield return null;
        }
        else
        {
            yield break;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Swippy Swappy #{0}] {1}", ModuleId, msg);
    }

    string logConfiguration()
    {
        string logThis = "";
        for(int i = 0; i < blocks.Length; i++)
        {
            logThis += " " + (blocks[i] + 1);
        }
        return logThis;
    }
}