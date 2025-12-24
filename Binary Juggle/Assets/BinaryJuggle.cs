using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using static Enums;

public class BinaryJuggle : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;
    public KMSelectable[] Buttons;
    public TextMesh[] CBTexts;
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private bool cbActive;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        GetComponent<KMBombModule>().OnActivate += delegate { Activate(); };
        moduleId = moduleIdCounter++;

        cbActive = Colorblind.ColorblindModeActive;

        foreach (KMSelectable button in Buttons) {
            button.OnInteract += delegate () { InputHandler(button); return false; };
        }
    }

    void Activate()
    {
        StartCoroutine(AnimateBlendShape(SkinnedMeshRendererCurtain, 0, 100, 4.5f));
        ManageLoopingCycle();
    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module
        GenerateCycle();
        AssignToBalls();
    }

    #region Getting Randoms

    List<Ball> firstBallList;
    List<Ball> secondBallList;

    void GenerateCycle()
    {
        // Complete Setup for Balls Stage 1
        Debug.LogFormat("[Binary Juggle #{0}] Stage 1:", moduleId);

        firstBallList = CreateBallList(6);

        while (firstBallList.Where(x => x.isOne).Count() == 0 || firstBallList.Where(x => !x.isOne).Count() == 0)
            firstBallList = CreateBallList(6);

        int index = 0;

        foreach (Ball ball in firstBallList)
        {
            Debug.LogFormat("[Binary Juggle #{0}] Ball {1} || Color: {2}, Symbol: {3}, Rotation: {4}, Direction: {5}",
                moduleId, index + 1, firstBallList[index].color.ToString(), firstBallList[index].symbol.ToString(), firstBallList[index].rotation.ToString(), firstBallList[index].direction.ToString());
            index++;
        }

        GetBallsToClick(firstBallList);

        // Complete Setup for Balls Stage 2
        Debug.LogFormat("[Binary Juggle #{0}] Stage 2:", moduleId);

        secondBallList = CreateBallList(8);

        while (secondBallList.Where(x => x.isOne).Count() == 0 || secondBallList.Where(x => !x.isOne).Count() == 0)
            secondBallList = CreateBallList(8);

        index = 0;

        foreach (Ball ball in secondBallList)
        {
            Debug.LogFormat("[Binary Juggle #{0}] Ball {1} || Color: {2}, Symbol: {3}, Rotation: {4}, Direction: {5}",
                moduleId, index + 1, secondBallList[index].color.ToString(), secondBallList[index].symbol.ToString(), secondBallList[index].rotation.ToString(), secondBallList[index].direction.ToString());
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
            // Getting a Random Enum through indexing
            var color = GetRandomEnum<BallColor>();
            var symbol = GetRandomEnum<Symbol>();
            var rotation = GetRandomEnum<Rotation>();
            var direction = GetRandomEnum<BallDirection>();

            list.Add(new Ball(color, symbol, rotation, direction));
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
        Debug.LogFormat("[Binary Juggle #{0}] Binary: {1}", moduleId, Binary);

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
        Debug.LogFormat("[Binary Juggle #{0}] Binary after juggling: {1}", moduleId, Binary);
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
        Debug.LogFormat("[Binary Juggle #{0}] The amount of times to juggle: {1}", moduleId, juggleAmount);
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

        AssignAllColorblind();
    }

    void AssignAllColorblind()
    {
        AssignColorblindToBalls(true, firstBallList.Select(x => x.color).ToArray());
        AssignColorblindToBalls(false, secondBallList.Select(x => x.color).ToArray());
    }

    void AssignColorblindToBalls(bool stage1, BallColor[] colors)
    {
        for (int i = stage1 ? 0 : 6; i < (stage1 ? 6 : 14); i++)
        {
            CBTexts[i].text = cbActive ? colors[stage1 ? i : i - 6].ToString()[0].ToString() : string.Empty;
            CBTexts[i].color = colors[stage1 ? i : i - 6] == BallColor.Yellow ? Color.black : Color.white;
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
    public Transform peakHeight;

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

        StartCoroutine(MoveParabola(ball.transform, transform, localStart, localEnd, peakHeight, CWRotation));
    }

    public void ManageLoopingCycle()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
            
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

    public IEnumerator MoveParabola(Transform target, Transform reference, Vector3 localStart, Vector3 localEnd, Transform peakHeight, bool CWRotation)
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

            // convert start, end, peak into reference local space
            Vector3 start = localStart;
            Vector3 end = localEnd;
            Vector3 peak = reference.InverseTransformPoint(peakHeight.position);

            // linear movement (local)
            Vector3 pos = Vector3.Lerp(start, end, t);

            // baseline Z
            float baseZ = Mathf.Lerp(start.z, end.z, t);

            // arc curve
            float arc = 4f * t * (1f - t);

            // ensure peak height at t = 0.5
            float midBaseZ = (start.z + end.z) * 0.5f;
            float requiredOffset = peak.z - midBaseZ;

            // final Z
            pos.z = baseZ + arc * requiredOffset;

            // convert back to world
            target.position = reference.TransformPoint(pos);

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
        int index = Array.IndexOf(Buttons, button);

        if (Stage == 1)
        {
            if (firstBallList[index].needsClick)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);

                firstBallList[index].needsClick = false;
                BallsStage1[index].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);

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
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);

                secondBallList[index - 6].needsClick = false;
                BallsStage2[index - 6].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);

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
        moduleSolved = true;
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
        GetComponent<KMBombModule>().HandleStrike();
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} colorblind/cb [Toggles colorblind mode] || !{0} submit 2 3 5 7 [presses the number of balls you want to submit]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        switch (split[0])
        {
            case "COLORBLIND":
            case "CB":
                if (split.Length > 1)
                {
                    yield return "sendtochaterror Too many parameters!";
                    yield break;
                }
                yield return null;
                cbActive = !cbActive;
                AssignAllColorblind();
                yield break;
            case "SUBMIT":
                if (split.Length == 1)
                {
                    yield return "sendtochaterror Please specify which balls to press!";
                    yield break;
                }
                var expectedStageLength = Stage == 1 ? 6 : 8;

                if (split.Skip(1).Count() > expectedStageLength)
                {
                    yield return $"sendtochaterror Too many parameters! Please make sure your numbers to input is exactly {expectedStageLength}!";
                    yield break;
                }

                var digitsToCheck = split.Skip(1).ToArray();

                var numbersToPress = new int[digitsToCheck.Length];

                for (int i = 0; i < numbersToPress.Length; i++)
                {
                    if (!int.TryParse(digitsToCheck[i], out numbersToPress[i]))
                    {
                        yield return $"sendtochaterror {digitsToCheck[i]} is not a valid number.";
                        yield break;
                    }
                    else if (numbersToPress[i] < 1)
                    {
                        yield return $"sendtochaterror One or more numbers cannot go less than 1!";
                        yield break;
                    }

                    numbersToPress[i]--;

                    if (!Enumerable.Range(0, expectedStageLength).Contains(numbersToPress[i]))
                    {
                        yield return $"sendtochaterror Make sure the numbers are in the range of 1-{expectedStageLength} inclusive!";
                        yield break;
                    }
                }

                yield return null;

                foreach (var number in numbersToPress)
                {
                    Buttons[Stage == 1 ? number : number + 6].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            default:
                yield return "sendtochaterror This is not a valid command!";
                yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while (loopRoutine == null)
                yield return true;

            var ballList = (Stage == 1 ? firstBallList : secondBallList).ToList();

            for (int i = 0; i < ballList.Count; i++)
            {
                if (!ballList[i].needsClick)
                    continue;

                Buttons[Stage == 1 ? i : i + 6].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
