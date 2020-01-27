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
    public Sprite[] noteSprites;
    public SpriteRenderer[] noteRenderers;
    private int whichNote = 0;

    private static int moduleIdCounter = 1;
    private int ModuleId;
    private bool moduleSolved;
    private bool active;

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
        DebugMsg("The notes in order are " + noteRenderers[0].name + " " + noteRenderers[1].name + " " + noteRenderers[2].name + ".");
    }

    protected void OnNeedyActivation()
    {
        active = true;
        foreach (SpriteRenderer note in noteRenderers)
        {
            note.sprite = noteSprites[UnityEngine.Random.Range(0,noteSprites.Length)]; //selects a random note for each display
        }
    }

    protected void OnNeedyDeactivation()
    {
        foreach(SpriteRenderer note in noteRenderers)
        {
            note.sprite = null; //all displays go blank
        }
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

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
        audio.PlaySoundAtTransform(pressedButton.name, transform);
        if (!active)
        {
            return;
        }
        else
        {
            DebugMsg("Pressed note: " + pressedButton.name + ", expecting " + noteRenderers[whichNote].sprite.name);
            if(pressedButton.name != noteRenderers[whichNote].sprite.name) //if pressed button's note isn't the same as the note on the displayed note
            {
                module.OnStrike();
                DebugMsg("Strike!");
            }
            else
            {
                noteRenderers[whichNote].sprite = null;
                whichNote++;
                if (whichNote == 3)
                {
                    module.OnPass();
                    OnNeedyDeactivation();
                }
            }
        }
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "c", "c#", "d", "d#", "e", "f", "f#", "g", "g#", "a", "a#", "b" };

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

    public string TwitchHelpMessage = "Press a button using !{0} C C# D";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (parts.Length > 3)
        {
            yield return "sendtochaterror Sorry, but that appears to be longer than the maximum sequence.";
            yield break;
        }
        else if (isCommandValid(cmd))
        {
            yield return null;
            for(int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == "c")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i].ToLower() == "c#")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i].ToLower() == "d")
                {
                    yield return new KMSelectable[] { buttons[2] };
                }
                else if (parts[i].ToLower() == "d#")
                {
                    yield return new KMSelectable[] { buttons[3] };
                }
                else if (parts[i].ToLower() == "e")
                {
                    yield return new KMSelectable[] { buttons[4] };
                }
                else if (parts[i].ToLower() == "f")
                {
                    yield return new KMSelectable[] { buttons[5] };
                }
                else if (parts[i].ToLower() == "f#")
                {
                    yield return new KMSelectable[] { buttons[6] };
                }
                else if (parts[i].ToLower() == "g")
                {
                    yield return new KMSelectable[] { buttons[7] };
                }
                else if (parts[i].ToLower() == "g#")
                {
                    yield return new KMSelectable[] { buttons[8] };
                }
                else if (parts[i].ToLower() == "a")
                {
                    yield return new KMSelectable[] { buttons[9] };
                }
                else if (parts[i].ToLower() == "a#")
                {
                    yield return new KMSelectable[] { buttons[10] };
                }
                else if (parts[i].ToLower() == "b")
                {
                    yield return new KMSelectable[] { buttons[11] };
                }
            }
        }
        else
        {
            yield return "sendtochaterror Sorry, there's an incorrect note. All valid note names are specified in the manual.";
            yield break;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Needy Piano #{0}] {1}", ModuleId, msg);
    }
}
