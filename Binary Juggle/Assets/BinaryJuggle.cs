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
using HarmonyLib;
using System.Reflection;

public partial class BinaryJuggle : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;

        foreach (KMSelectable button in Buttons) {
            button.OnInteract += delegate () { InputHandler(button); return false; };
        }

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
        GenerateCycle();
        AssignToBalls();
        StartCoroutine(AnimateBlendShape(SkinnedMeshRendererCurtain, 0, 100, 4.5f));
        ManageLoopingCycle();
    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    #region Getting Randoms

    List<Ball> firstBallList;
    List<Ball> secondBallList;

    void GenerateCycle()
    {
        // Complete Setup for Balls Stage 1
        Debug.LogFormat("[Binary Juggle #{0}] Stage 1:", ModuleId);

        firstBallList = CreateBallList(6);

        while (firstBallList.Where(x => x.isOne).Count() == 0 || firstBallList.Where(x => !x.isOne).Count() == 0)
            firstBallList = CreateBallList(6);

        int index = 0;

        foreach (Ball ball in firstBallList)
        {
            Debug.LogFormat("[Binary Juggle #{0}] Ball {1} || Color: {2}, Symbol: {3}, Rotation: {4}, Direction: {5}",
                ModuleId, index + 1, firstBallList[index].color.ToString(), firstBallList[index].symbol.ToString(), firstBallList[index].rotation.ToString(), firstBallList[index].direction.ToString());
            index++;
        }

        GetBallsToClick(firstBallList);

        // Complete Setup for Balls Stage 2
        Debug.LogFormat("[Binary Juggle #{0}] Stage 2:", ModuleId);

        secondBallList = CreateBallList(8);

        while (secondBallList.Where(x => x.isOne).Count() == 0 || secondBallList.Where(x => !x.isOne).Count() == 0)
            secondBallList = CreateBallList(8);

        index = 0;

        foreach (Ball ball in secondBallList)
        {
            Debug.LogFormat("[Binary Juggle #{0}] Ball {1} || Color: {2}, Symbol: {3}, Rotation: {4}, Direction: {5}",
                ModuleId, index + 1, secondBallList[index].color.ToString(), secondBallList[index].symbol.ToString(), secondBallList[index].rotation.ToString(), secondBallList[index].direction.ToString());
            index++;
        }

        GetBallsToClick(secondBallList);
    }

    // List Builder
    List<Ball> CreateBallList(int count)
    {
        List<Ball> list = new List<Ball>();

        for (int i = 0; i < count; i++)
        {
            int position = i + 1;

            // Getting a Random Enum through indexing
            var color = GetRandomEnum<BallColor>();
            var symbol = GetRandomEnum<Symbol>();
            var rotation = GetRandomEnum<Rotation>();
            var direction = GetRandomEnum<BallDirection>();

            list.Add(new Ball(color, symbol, rotation, direction, position));
        }
        return list;
    }

    void GetBallsToClick(List<Ball> balls)
    {
        string Binary = "";
        foreach ( var ball in balls)
        {
            Binary += ball.isOne ? "1" : "0";
        }
        Debug.LogFormat("[Binary Juggle #{0}] Binary: {1}", ModuleId, Binary);

        // Split in 2 Halfes
        string leftBinary = Binary.Substring(0, Binary.Length/2);
        string rightBinary = Binary.Substring(Binary.Length/2);
        Binary = JuggleBinary(leftBinary, rightBinary, GetJuggleAmount(Binary, balls));
        for (int i = 0;i < balls.Count; i++)
        {
            if (Binary[i] == '1')
                balls[i].needsClick = true;
        }
    }
    string JuggleBinary(string leftBinary, string rightBinary, int JuggleAmount)
    {
        for (int i = 0; i < JuggleAmount; i++)
        {
            char fromRight = rightBinary[0];
            rightBinary = rightBinary.Remove(0, 1);
            leftBinary = fromRight + leftBinary;

            char fromLeft = leftBinary.Last();
            leftBinary = leftBinary.Remove(leftBinary.Length - 1, 1);
            rightBinary = rightBinary + fromLeft;
        }
        string Binary = leftBinary + rightBinary;
        Debug.LogFormat("[Binary Juggle #{0}] Binary after juggling: {1}", ModuleId, Binary);
        return Binary;
    }

    int GetJuggleAmount(string binary, List<Ball> balls)
    {
        int juggleAmount = 0;

        if (balls.Where(x => x.direction == BallDirection.Right).Count() > balls.Where(x => x.rotation == Rotation.Counterclockwise).Count())
        {
            foreach (var ones in binary)
            {
                if (ones == '1')
                    juggleAmount++;
            }
        }
        else if (balls.Where(x => x.direction == BallDirection.Left).Count() > balls.Where(x => x.rotation == Rotation.Clockwise).Count())
        {
            foreach (var ones in binary)
            {
                if (ones == '0')
                    juggleAmount++;
            }
        }
        else
        {
            int one = 0;
            int zero = 0;

            foreach (var ones in binary)
            {
                if (ones == '1')
                    one++;
                else
                    zero++;
            }
            juggleAmount = Mathf.Abs(one - zero);
        }
        Debug.LogFormat("[Binary Juggle #{0}] The amount of times to juggle: {1}", ModuleId, juggleAmount);
        return juggleAmount;
    }

    #endregion

    #region Assign Randoms to Balls

    public GameObject[] BallsStage1 = new GameObject[6];
    public GameObject[] BallsStage2 = new GameObject[8];

    public Material[] SymbolMaterials = new Material[4];

    void AssignToBalls()
    {
        for (int i = 0;i < BallsStage1.Length; i++)
        {
            GetSymbolForBall(BallsStage1[i].GetComponent<MeshRenderer>(), firstBallList[i].symbol);
            GetColorForBall(BallsStage1[i].GetComponent<MeshRenderer>(), firstBallList[i].color);
        }

        for (int i = 0; i < BallsStage2.Length; i++)
        {
            GetSymbolForBall(BallsStage2[i].GetComponent<MeshRenderer>(), secondBallList[i].symbol);
            GetColorForBall(BallsStage2[i].GetComponent<MeshRenderer>(), secondBallList[i].color);
        }
    }

    void GetColorForBall(MeshRenderer BallMeshRen, BallColor col)
    {
        switch (col)
        {
            case BallColor.Red:
                BallMeshRen.material.color = new Color(1, 0, 0);
                break;
            case BallColor.Green:
                BallMeshRen.material.color = new Color(0, 1, 0);
                break;
            case BallColor.Yellow:
                BallMeshRen.material.color = new Color(1, 1, 0);
                break;
            case BallColor.Magenta:
                BallMeshRen.material.color = new Color(1, 0, 1);
                break;
            default:
                Debug.Log("No Color Found!");
                break;
        }
    }

    void GetSymbolForBall(MeshRenderer BallMeshRen, Symbol sym)
    {
        switch (sym)
        {
            case Symbol.MagiciansHat:
                BallMeshRen.material = SymbolMaterials[0];
                break;
            case Symbol.JestersHat:
                BallMeshRen.material = SymbolMaterials[1];
                break;
            case Symbol.Pin:
                BallMeshRen.material = SymbolMaterials[2];
                break;
            case Symbol.Weight:
                BallMeshRen.material = SymbolMaterials[3];
                break;
            default:
                Debug.Log("No Symbol Found!");
                break;
        }
    }

    #endregion

    #region Play Sequence

    public Coroutine loopRoutine;

    int Stage = 1;

    public Transform startRight;    // 0.0575f, 0.021f, -0.065f
    public Transform startLeft;     // -0.0575f, 0.021f, -0.065f
    public Transform endRight;      // 0.04f, 0.021f, -0.065f
    public Transform endLeft;       // -0.04f, 0.021f, -0.065f

    bool CWRotation;

    void MoveBall(GameObject ball, BallDirection dir, Rotation rot)
    {
        Transform start = dir == BallDirection.Left ? startRight : startLeft;
        Transform end = dir == BallDirection.Left ? endLeft : endRight;

        CWRotation = rot == Rotation.Clockwise ? true : false;

        // parent to the module so local positions are relative to the module transform
        ball.transform.SetParent(transform, false);

        // convert the start/end Transforms into local positions relative to the module
        Vector3 localStart = transform.InverseTransformPoint(start.position);
        Vector3 localEnd = transform.InverseTransformPoint(end.position);

        // place ball at local start
        ball.transform.localPosition = localStart;

        StartCoroutine(MoveParabola(ball.transform, transform, localStart, localEnd, 0.08f, CWRotation));
    }

    public void ManageLoopingCycle()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);
        if (Stage == 1)
            loopRoutine = StartCoroutine(LoopStage(firstBallList.Count));
        else if (Stage == 2)
            loopRoutine = StartCoroutine(LoopStage(secondBallList.Count));
    }

    IEnumerator LoopStage(int maxIndex)
    {
        yield return new WaitForSeconds(5);
        float timeBetweenBalls = 0.85f;
        float timeBetweenLoops = 2.35f;
        int index = 0;
        while (true)
        {
            if (Stage == 1)
                MoveBall(BallsStage1[index], firstBallList[index].direction, firstBallList[index].rotation);
            else
                MoveBall(BallsStage2[index], secondBallList[index].direction, secondBallList[index].rotation);

            yield return new WaitForSeconds(timeBetweenBalls);

            index++;

            if (index == maxIndex) // Loop Reset
            {
                yield return new WaitForSeconds(timeBetweenLoops);
                index = 0;
            }
        }
    }

    public IEnumerator MoveParabola(Transform target, Transform reference, Vector3 localStart, Vector3 localEnd, float height, bool CWRotation)
    {
        float duration = 3f;
        float elapsed = 0f;

        float initialY = target.localEulerAngles.y;
        float totalYRotation;

        if (CWRotation)
            totalYRotation = Rnd.Range(270f, 360f);
        else
            totalYRotation = Rnd.Range(-270f, -360f);

        while (elapsed < duration)
            {
                float t = elapsed / duration;

                // get start/end in world space relative to reference
                Vector3 worldStart = reference.TransformPoint(localStart);
                Vector3 worldEnd = reference.TransformPoint(localEnd);

                // linear movement
                Vector3 pos = Vector3.Lerp(worldStart, worldEnd, t);

                // height offset while moving across
                pos += reference.forward * (height * 4f * t * (1f - t));

                target.position = pos;

                // rotate along Y axis
                float currentY = Mathf.Lerp(0f, totalYRotation, t);
                target.localRotation = Quaternion.Euler(target.localEulerAngles.x, initialY + currentY, target.localEulerAngles.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

        // snap to final position
        target.position = reference.TransformPoint(localEnd + new Vector3(0, -0.05f, 0));
    }

    #endregion

    #region Input Handling

    void InputHandler(KMSelectable button)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);

        int index = Array.IndexOf(Buttons, button);

        if (Stage == 1)
        {
            if (firstBallList[index].needsClick)
            {
                firstBallList[index].needsClick = false;

                if (firstBallList.Where(x => x.needsClick).Count() == 0)
                {
                    Stage++;
                    StartCoroutine(AnimateBlendShape(SkinnedMeshRendererCurtain, 0, 85, 5));
                    ManageLoopingCycle();
                }
                return;
            }
            Strike();
        }
        else if (Stage == 2)
        {
            if (index < 6) return;
            if (secondBallList[index - 6].needsClick)
            {
                secondBallList[index - 6].needsClick = false;
                if (secondBallList.Where(x => x.needsClick).Count() == 0)
                {
                    Stage++;
                    StartCoroutine(AnimateBlendShape(SkinnedMeshRendererCurtain, 0, 0, 5));
                    ManageLoopingCycle();
                    Solve();
                }
                return;
            }
            Strike();
        }

    }

    #endregion

    #region Helpers

    // Helper to get random valid index from Enum
    T GetRandomEnum<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        int index = Rnd.Range(0, values.Length);
        return (T)values.GetValue(index);
    }

    public SkinnedMeshRenderer SkinnedMeshRendererCurtain;

    public IEnumerator AnimateBlendShape(SkinnedMeshRenderer skinnedMesh, int blendShapeIndex, float targetValue, float duration)
    {
        float startValue = skinnedMesh.GetBlendShapeWeight(blendShapeIndex);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentValue = Mathf.Lerp(startValue, targetValue, t);
            skinnedMesh.SetBlendShapeWeight(blendShapeIndex, currentValue);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure it reaches the target exactly
        skinnedMesh.SetBlendShapeWeight(blendShapeIndex, targetValue);
    }


    #endregion

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
