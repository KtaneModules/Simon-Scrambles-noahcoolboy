﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Random = UnityEngine.Random;

public class simonScramblesScript : MonoBehaviour {
    public KMBombInfo info;
    public KMBombModule module;
    public KMAudio bombAudio;
    public Light[] buttonLights;
    public KMSelectable[] buttons;
    public KMRuleSeedable RuleSeedable;

    int[] sequence = new int[10];
    readonly string[] colorNames = { "Blue", "Yellow", "Red", "Green" };
    float beep;
    Coroutine lightShow = null;
    Coroutine nowLight = null;
    int currentInt;
    readonly int[,] colorTable = {
        { 1, 3, 0, 2 },
        { 3, 0, 1, 2 },
        { 2, 3, 1, 0 },
        { 2, 1, 3, 0 },
        { 2, 0, 3, 1 },
        { 0, 1, 2, 3 },
        { 1, 3, 0, 2 },
        { 1, 0, 3, 2 },
        { 2, 1, 0, 3 },
        { 3, 2, 1, 0 }
    };

    MonoRandom rnd;

    bool moduleSolved;
    static int moduleIdCounter = 1;
    int moduleId;

    private void Awake() {
        moduleId = moduleIdCounter++;

        for (var i = 0; i < buttonLights.Length; i++)
            buttonLights[i].enabled = false;

        rnd = RuleSeedable.GetRNG();
        Debug.LogFormat("[Simon Scrambles #{0}] Using rule seed: {1}", moduleId, rnd.Seed);

        if (rnd.Seed != 1) {
            var nowColors = rnd.ShuffleFisherYates(Enumerable.Range(0, 4).ToArray());

            for (var i = 0; i < colorTable.GetLength(0); i++) {
                for (var j = 0; j < colorTable.GetLength(1); j++)
                    colorTable[i, j] = nowColors[j];

                nowColors = rnd.ShuffleFisherYates(nowColors);
            }
        }

        HandleSequence();

        foreach (KMSelectable key in buttons) {
            key.OnInteract += delegate () {
                KeyPressed(key);

                return false;
            };
        }
    }

    void HandleSequence() {
        beep = 0f;

        for (var i = 0; i < 10; i++)
            sequence[i] = Random.Range(0, 4);

        Debug.LogFormat("[Simon Scrambles #{0}] Sequence is: {1}", moduleId, sequence.Select(x => colorNames[x]).Join(", "));
        Debug.LogFormat("[Simon Scrambles #{0}] Answer is: {1}", moduleId, sequence.Select((x, y) => colorNames[colorTable[y, x]]).Join(", "));
    }

    void Update() {
        if (lightShow == null) {
            beep += Time.deltaTime;

            if (!moduleSolved && beep >= 5f) {
                lightShow = StartCoroutine(LightSequence());
                beep = 0f;
            }
        }
    }

    IEnumerator LightSequence() {
        for (var i = 0; i < 10; i++) {
            StartCoroutine(FlashLight(sequence[i]));

            yield return new WaitForSeconds(1.1f);
        }

        lightShow = null;
    }

    IEnumerator FlashLight(int nowFlash) {
        buttonLights[nowFlash].enabled = true;

        yield return new WaitForSeconds(0.9f);

        buttonLights[nowFlash].enabled = false;
        nowLight = null;
    }

    void KeyPressed(KMSelectable key) {
        bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (moduleSolved) return;

        var nowButton = Array.IndexOf(buttons, key);
        Debug.LogFormat("[Simon Scrambles #{0}] You pressed {1}.", moduleId, colorNames[nowButton]);

        if (lightShow != null) {
            StopCoroutine(lightShow);
            lightShow = null;
        }

        beep = 0f;

        for (var i = 0; i < buttonLights.Length; i++)
            buttonLights[i].enabled = false;

        if (nowLight != null) {
            StopCoroutine(nowLight);
            nowLight = null;
        }

        nowLight = StartCoroutine(FlashLight(nowButton));

        if (colorTable[currentInt, sequence[currentInt]] == nowButton) {
            Debug.LogFormat("[Simon Scrambles #{0}] That was correct.", moduleId);

            if (++currentInt == 10) {
                module.HandlePass();
                moduleSolved = true;
                Debug.LogFormat("[Simon Scrambles #{0}] Module solved!", moduleId);
            }
        } else {
            module.HandleStrike();
            Debug.LogFormat("[Simon Scrambles #{0}] That was incorrect. The module has been resetted.", moduleId);
            currentInt = 0;
            HandleSequence();
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit by ""!{0} RGBY..."" (initial letter of colors to press)";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command) {
        command = command.ToLowerInvariant().Trim();

        if (Regex.IsMatch(command, @"^[(r|y|g|b)]+$")) {
            var pressList = new List<KMSelectable>();

            for (var i = 0; i < command.Length; i++) {
                if (Regex.IsMatch(command[i].ToString(), @"^(r|y|g|b)$")) {
                    pressList.Add(buttons[Array.IndexOf(colorNames, colorNames[Array.FindIndex(colorNames, x => x.ToLowerInvariant().StartsWith(command[i].ToString()))])]);
                }
            }

            return (pressList.Count > 0) ? pressList.ToArray() : null;
        }

        return null;
    }
}