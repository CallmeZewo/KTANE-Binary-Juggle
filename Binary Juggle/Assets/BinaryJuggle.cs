using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using ZT = ZToolsKtane;
using Rnd = UnityEngine.Random;
using Math = ExMath;
using static Enums;
using NUnit.Framework;

public partial class BinaryJuggle : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable Select;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void OnDestroy()
    { //Shit you need to do when the bomb ends

    }

    void Activate()
    { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module
        
    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    void GenerateCycle()
    {
        List<Ball> firstBallList = CreateBallList(4);
        List<Ball> secondBallList = CreateBallList(6);
    }

    List<Ball> CreateBallList(int count)
    {
        List<Ball> list = new List<Ball>();

        int colorLength = Enum.GetValues(typeof(Enums.Color)).Length;
        int symbolLength = Enum.GetValues(typeof(Symbol)).Length;
        int rotationLength = Enum.GetValues(typeof(Rotation)).Length;
        int directionLength = Enum.GetValues(typeof(Direction)).Length;

        for (int i = 0; i < count; i++)
        {
            var color = (Enums.Color)Rnd.Range(0, colorLength);
            var symbol = (Symbol)Rnd.Range(0, symbolLength);
            var rotation = (Rotation)Rnd.Range(0, rotationLength);
            var direction = (Direction)Rnd.Range(0, directionLength);

            list.Add(new Ball(color, symbol, rotation, direction));
        }
        return list;
    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }
    /* Delete this if you dont want TP integration
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }*/
}
