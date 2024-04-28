using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class needyPianoS : MonoBehaviour {

    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable[] buttons;
    public SpriteRenderer[] noteRenderers;
    public Sprite[] noteSprites;
    string[] noteNames = new string[12] { "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B" };
    string[] noteAudioNames = new string[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    int[] noteAnswers = new int[3];
    private int whichNote = 0;

    private static int moduleIdCounter = 1;
    private int ModuleId;
    private bool moduleSolved;
    private bool active;
    private bool striked;
    int strikedNote;

    void Awake()
    {
        ModuleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        module.OnNeedyDeactivation += OnNeedyDeactivation;
        module.OnTimerExpired += OnTimerExpired;
        for (int i = 0; i < buttons.Length; i++)
        {
            int dummy = i;
            buttons[dummy].OnInteract += delegate () { buttonPressed(dummy); return false; };
        }
    }

    protected void OnNeedyActivation()
    {
        active = true;
        if(striked)
            noteRenderers[strikedNote].color = Color.black;

        string[] generatedNoteNames = new string[3];
        int[] generatedNotes = new int[3];
        int[] generatedAccidentals = new int[3];
        for(int i = 0; i < 3; i++)
        {
            int note = UnityEngine.Random.Range(0, 12);
            noteAnswers[i] = note;
            int accidental = 0;
            if (note == 1 || note == 3 || note == 6 || note == 8 || note == 10)
            {
                if (UnityEngine.Random.Range(0, 2) == 0) //its flat
                {
                    accidental = 1;
                    note++;
                    generatedNoteNames[i] = "b";
                }
                else //its sharp
                {
                    accidental = 2;
                    note--;
                    generatedNoteNames[i] = "#";
                }
            }
            generatedNoteNames[i] = noteNames[note] + generatedNoteNames[i];

            if (note <= 4) //makes note values actually usable
            {
                note /= 2;
            }
            else //if the notes are higher than E it divides it differently because music sucks
            {
                note = (note + 1) / 2;
            }
            if(note != 6 && UnityEngine.Random.Range(0, 2) == 0) //up an octave
                note += 7;
            generatedNotes[i] = note;
            generatedAccidentals[i] = accidental;

            if (generatedNotes.Contains(note)) //check for accidental funnies
            {
                for(int j = i - 1; j >= 0; j--)
                {
                    if(generatedNotes[j] == note && generatedAccidentals[j] != 0 && accidental == 0)
                        accidental = 3;
                }
            }
            noteRenderers[i].sprite = noteSprites[note * 4 + accidental];
        }
        DebugMsg("The notes are: " + generatedNoteNames[0] + ", " + generatedNoteNames[1] + ", "+ generatedNoteNames[2]);
    }

    protected void OnNeedyDeactivation()
    {
        module.OnPass();
        whichNote = 0;
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

    void buttonPressed(int pressedKey) //Treble starts at C, Bass starts at E
    {
        audio.PlaySoundAtTransform(noteAudioNames[pressedKey], transform);
        if (!active)
        {
            return;
        }

        if(pressedKey == noteAnswers[whichNote])
        {
            noteRenderers[whichNote].sprite = null;
            whichNote++;
            if(whichNote == 3)
            {
                module.OnNeedyDeactivation();
            }
        }
        else
        {
            striked = true;
            strikedNote = whichNote;
            noteRenderers[whichNote].color = Color.red;
            module.HandleStrike();
            module.OnNeedyDeactivation();
        }
    }

    public string TwitchHelpMessage = "Press a key using !{0} C C# D or !{0} press E Eb D";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (parts.Length > 4 - whichNote || (parts.Length > 3 - whichNote && !parts.Contains("press")))
        {
            yield return "sendtochaterror Too many notes!";
            yield break;
        }
        else
        {
            List<int> presses = new List<int>();
            bool invalidCommand = false;
            for (int i = 0; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "press":
                        break;
                    case "c":
                        presses.Add(0);
                        break;
                    case "c#":
                    case "db":
                        presses.Add(1);
                        break;
                    case "d":
                        presses.Add(2);
                        break;
                    case "d#":
                    case "eb":
                        presses.Add(3);
                        break;
                    case "e":
                        presses.Add(4);
                        break;
                    case "f":
                        presses.Add(5);
                        break;
                    case "f#":
                    case "gb":
                        presses.Add(6);
                        break;
                    case "g":
                        presses.Add(7);
                        break;
                    case "g#":
                    case "ab":
                        presses.Add(8);
                        break;
                    case "a":
                        presses.Add(9);
                        break;
                    case "a#":
                    case "bb":
                        presses.Add(10);
                        break;
                    case "b":
                        presses.Add(11);
                        break;
                    default: //
                        invalidCommand = true;
                        break;
                }
            }
            if(invalidCommand)
            {
                yield return "sendtochaterror Invalid note!";
                yield break;
            }
            yield return null;
            for(int i = 0; i < 3; i++)
            {
                yield return new KMSelectable[] { buttons[presses[i]] };
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Needy Piano #{0}] {1}", ModuleId, msg);
    }
}
