using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System;

public class simonScramblesScript : MonoBehaviour
{
    public KMBombInfo info;
    public KMBombModule module;
    public KMAudio audio;
    public Light[] lights3 = new Light[4];
    public KMSelectable[] buttons;
    private int[] sequence;
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private int beep = 500;
    private int currentInt;
    private string answer = "";
    int[,] array2Da = new int[,] { { 1,3,0,2 }, {3,0,1,2 }, {2,3,1,0 }, {2,1,3,0}, {2,0,3,1 }, {0,1,2,3 }, { 1,3,0,2}, {1,0,3,2 }, { 2, 1, 0, 3 }, { 3,2,1,0} };
    void Awake()
    {
        lights3[0].enabled = false;
        lights3[1].enabled = false;
        lights3[2].enabled = false;
        lights3[3].enabled = false;
        sequence = new int[10];
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 10; i++)
        {
            sequence[i] = UnityEngine.Random.Range(0, 4);
            
            
        }
        
        Debug.LogFormat("[simonScrambles #{0}] 0 = blue, 1 = yellow, 2 = red, 3 = green", moduleId);
        Debug.LogFormat("[simonScrambles #{0}] sequence is " + sequence[0]+ sequence[1] + sequence[2] + sequence[3] + sequence[4] + sequence[5] + sequence[6] + sequence[7] + sequence[8] + sequence[9], moduleId);
        for (int i = 0; i < 10; i++)
        {
            answer = answer + array2Da[i, sequence[i]];
        }
        Debug.LogFormat("[simonScrambles #{0}] answer is " + answer, moduleId);
        foreach (KMSelectable key in buttons)
        {
            key.OnInteract += delegate () { keyPressed(key); return false; };
        }
    }

    void Start()
    {

    }
    void Update()
    {
        beep++;
        if (beep == 1000 && !moduleSolved)
        {
            StartCoroutine(lights());
            beep = 0;
        }
        

    }
    IEnumerator lights()
    {
        for (int i = 0; i < 10; i++)
        {
           
            lights3[sequence[i]].enabled = true;
            yield return new WaitForSeconds(1);
            lights3[sequence[i]].enabled = false;
            yield return new WaitForSeconds(.1f);
        }
    }
    void keyPressed(KMSelectable key)
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (!moduleSolved)
        {
            Debug.LogFormat("[simonScrambles #{0}] {1} has been put in", moduleId, Array.IndexOf(buttons, key));

            if (array2Da[currentInt, sequence[currentInt]] == Array.IndexOf(buttons, key))
            {

                Debug.LogFormat("[simonScrambles #{0}] correct", moduleId);



            } else
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
                module.HandleStrike();
                currentInt = 0;
                Debug.LogFormat("[simonScrambles #{0}] wrong, reset", moduleId);
            }


        }

        currentInt++;
        if (currentInt == 10)
        {
            module.HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[simonScrambles #{0}] solved", moduleId);
        }
    }
}

