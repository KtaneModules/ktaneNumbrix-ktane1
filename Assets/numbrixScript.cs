using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class numbrixScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public GameObject[] squares;
    public TextMesh bigText;

    private KeyCode[] typableKeys =
    {
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Backspace, KeyCode.R
    };
    private bool focused;

    private int[] chosenPath = new int[81];
    private int[] solutionState = new int[81];
    private int selectedSquare = -1;

    private int[][] puzzles = new int[][]
    {
        new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 17, 18, 26, 27, 35, 36, 44, 45, 53, 54, 62, 63, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80},
        new int[]{0, 8, 10, 16, 20, 24, 30, 32, 40, 48, 50, 56, 60, 64, 70, 72, 80},
        new int[]{0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 26, 28, 34, 36, 44, 46, 52, 54, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80},
        new int[]{10, 12, 14, 16, 20, 22, 24, 28, 34, 38, 42, 46, 52, 56, 58, 60, 64, 66, 68, 70},
        new int[]{0, 2, 4, 6, 8, 13, 18, 26, 36, 37, 43, 44, 54, 62, 67, 72, 74, 76, 78, 80},
    };
    private int rnd;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleActivated, inputMode, moduleSolved; // Some helpful booleans

    void Awake()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < squares.Length; i++)
        {
            int j = i;
            squares[i].GetComponent<KMSelectable>().OnInteract += () => { squareInput(j); return false; };
        }

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };
        if (Application.isEditor)
            focused = true;
    }

    void Start()
    {
        bigText.text = "";
        chosenPath = Enumerable.Repeat(-1, 81).ToArray();
        var sb = new StringBuilder();
        foreach (GameObject k in squares)
        {
            k.transform.GetChild(0).GetComponent<TextMesh>().text = "";
        }
        generatePath();
        rnd = UnityEngine.Random.Range(0, puzzles.Length);
        for (int i = 0; i < chosenPath.Length; i++)
        {
            solutionState[chosenPath[i]] = i + 1;
            if (puzzles[rnd].ToList().Contains(chosenPath[i]))
            {
                squares[chosenPath[i]].transform.GetChild(0).GetComponent<TextMesh>().text = (i + 1).ToString();
                squares[chosenPath[i]].GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.8f, 1f, 1f);
            }
        }
        foreach (int i in solutionState)
        {
            sb.Append(i.ToString() + ", ");
        }
        sb.Remove(sb.Length - 2, 2);
        Debug.LogFormat("[Numbrix #{0}] The solution grid, in reading order, is {1}.", moduleId, sb.ToString());
    }

    void squareInput(int k)
    {
        if (moduleSolved) return;
        selectedSquare = k;
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        for (int i = 0; i < squares.Length; i++)
            if (i == k)
                squares[i].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0.5f, 1f);
            else if (puzzles[rnd].ToList().Contains(i))
                squares[i].GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.8f, 1f, 1f);
            else
                squares[i].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 1f);

    }

    void generatePath()
    {
        generateZigZag();
        for (int i = 0; i < 1000; i++)
        {
            backbite(true);
        }
        for (int i = 0; i < 1000; i++)
        {
            backbite(false);
        }
    }

    void generateZigZag()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                if (i % 2 == 0)
                    chosenPath[i * 9 + j] = i * 9 + j;
                else
                    chosenPath[i * 9 + j] = i * 9 + 9 - j - 1;
    }

    void backbite(bool isStart)
    {
        int[] tempPath = new int[81];
        int start;
        var neighbours = new List<int>();
        Array.Copy(chosenPath, tempPath, chosenPath.Length);
        if (isStart)
            start = chosenPath[0];
        else
            start = chosenPath[80];

        if (start % 9 > 0) { neighbours.Add(start - 1); }//Left
        if (start % 9 < 8) { neighbours.Add(start + 1); }//Right
        if (start > 8) { neighbours.Add(start - 9); }//Up
        if (start < 72) { neighbours.Add(start + 9); }//Down

        if (isStart)
            neighbours.Remove(chosenPath[1]);
        else
            neighbours.Remove(chosenPath[79]);


        int chosenNeighbour = neighbours[UnityEngine.Random.Range(0, neighbours.Count())];
        int k = 0;
        if (isStart)
        {
            while (chosenPath[k] != chosenNeighbour)
                k++;
            for (int i = 0; i < k; i++)
                tempPath[i] = chosenPath[k - i - 1];
        }
        else
        {
            while (chosenPath[chosenPath.Length - 1 - k] != chosenNeighbour)
                k++;
            for (int i = 0; i < k; i++)
                tempPath[chosenPath.Length - 1 - i] = chosenPath[chosenPath.Length - (k - i)];
        }
        Array.Copy(tempPath, chosenPath, tempPath.Length);
    }

    IEnumerator solveAnim()
    {
        yield return null;
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        squares[selectedSquare].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 1f);
        for (int i = 0; i < chosenPath.Length; i++)
        {
            squares[chosenPath[i]].GetComponent<MeshRenderer>().material.color = new Color(0.7f, 1f, 0.7f, 1f);
            yield return new WaitForSeconds(0.03f); 
        }
        for (int i = 0; i < chosenPath.Length; i++)
        {
            squares[chosenPath[i]].transform.GetChild(0).GetComponent<TextMesh>().text = "";
            yield return new WaitForSeconds(0.03f);
        }
        for (int i = 0; i < chosenPath.Length; i++)
        {
            squares[chosenPath[i]].SetActive(false);
            yield return new WaitForSeconds(0.03f);
        }
        bigText.text = "MODULE SOLVED!";
    }

    void Update() //Runs every frame.
    {
        if (moduleSolved) return;   
        for (int i = 0; i < typableKeys.Count(); i++)
        {
            if (Input.GetKeyDown(typableKeys[i]) && focused)
            {
                if (i < 20) { handleKey(i); }
                else if (i < 21) { reset(false); }
                else { reset(true); }
            }
        }

        bool check = true;
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].transform.GetChild(0).GetComponent<TextMesh>().text != solutionState[i].ToString())
            {
                check = false;
            }
        }
        if (check)
        {
            module.HandlePass();
            moduleSolved = true;
            StartCoroutine(solveAnim());
        }
    }

    void handleKey(int k)
    {
        if (selectedSquare < 0) return;
        if (moduleSolved || puzzles[rnd].ToList().Contains(selectedSquare)) return;
        squares[selectedSquare].transform.GetChild(0).GetComponent<TextMesh>().text = (Convert.ToInt32(squares[selectedSquare].transform.GetChild(0).GetComponent<TextMesh>().text + k % 10) % 100).ToString();
    }


    void reset(bool everything)
    {
        if (everything)
        {
            for (int i = 0; i < squares.Length; i++)
                if (!puzzles[rnd].ToList().Contains(i))
                    squares[i].transform.GetChild(0).GetComponent<TextMesh>().text = "";
        }
        else
        {
            if (!puzzles[rnd].ToList().Contains(selectedSquare))
                squares[selectedSquare].transform.GetChild(0).GetComponent<TextMesh>().text = "";
        }
    }

    /*Twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^()$");
        yield return null;
    }
    */

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!moduleSolved)
        {
            yield return null;
            for (int i = 0; i < chosenPath.Length; i++)
            {
                squares[i].GetComponent<KMSelectable>().OnInteract();
                yield return null;
                for (int j = 0; j < solutionState[i].ToString("00").Length; j++)
                {
                    handleKey(solutionState[i].ToString("00")[j] - '0');
                    yield return null;
                }
            }
        }
    }
}
