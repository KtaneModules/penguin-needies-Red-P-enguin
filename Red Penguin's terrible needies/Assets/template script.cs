using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class templatescript : MonoBehaviour
{

    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable[] buttons;

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
    }

    protected void OnNeedyActivation()
    {
        active = true;
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
            module.OnPass();
            OnNeedyDeactivation();
        }
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "a", "b", "c" };

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

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == "a")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i].ToLower() == "b")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i].ToLower() == "c")
                {
                    yield return new KMSelectable[] { buttons[2] };
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
        Debug.LogFormat("[Needy Piano #{0}] {1}", ModuleId, msg);
    }
}

