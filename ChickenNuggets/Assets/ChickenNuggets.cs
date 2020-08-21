﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class ChickenNuggets : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Nuggies;
    public KMRuleSeedable Ruleseed;
    public GameObject Commie;
    public Material[] McDonalds;
    public TextMesh McBlowjob;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    int COMMIE = 0;
    int Boobies = 0;
    bool Simp = false;
    bool truefalse = false;
    int CondonGenerator = 0;
    float elapsed = 0f;
    float duration = 1f;
    int[] kum;

    //This exists so that we can tell the other instances of this module to not regenerate the answer on the same rule seed.
    sealed class ChickenNuggetsInfos
    {
        public bool First = true;
        public List<int> Distance = new List<int>();
        public List<string> Solutions = new List<string>();
    };

    //Just so that we can distinguish between different rule seeds, not certain if it is possible to have two bombs with different rule seeds, but it would prevent such issue.
    private static readonly Dictionary<int, ChickenNuggetsInfos> _chickenNuggetsInfos = new Dictionary<int, ChickenNuggetsInfos>();

    private ChickenNuggetsInfos _chickenNuggetsInfo;

    void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable Nuggie in Nuggies) {
        Nuggie.OnInteract += delegate () { NuggiesPress(Nuggie); return false; };
      }
      foreach (KMSelectable Nuggie in Nuggies) {
        Nuggie.OnInteractEnded += delegate () { NuggiesEnd(Nuggie);};
      }
      _chickenNuggetsInfos.Clear();
    }
    void Start () {
      var rnd = Ruleseed.GetRNG();
      int seed = rnd.Seed;
      kum = seed == 1 ? new int[3] { 6, 9, 20 } : new int[3] { rnd.Next(4, 8), rnd.Next(8, 15), rnd.Next(14, 24) };
      Debug.LogFormat("[Chicken Nuggets #{0}] Ruleseed {1}: The nuggets counts are {2}, {3}, and {4}.", moduleId, seed, kum[0], kum[1], kum[2]);
      if (!_chickenNuggetsInfos.ContainsKey(seed))
        _chickenNuggetsInfos[seed] = new ChickenNuggetsInfos();
      _chickenNuggetsInfo = _chickenNuggetsInfos[seed];
      if (_chickenNuggetsInfo.First) {
        for (int i = 0; i < 201; i++)
          SearchAllSolutions(i);
      }
      _chickenNuggetsInfo.First = false;
      StartCoroutine(GenerateMcNumeros());
      COMMIE = UnityEngine.Random.Range(0,101);
      if (COMMIE == 69) {
        Commie.GetComponent<MeshRenderer>().material = McDonalds[1];
      }
	}
  void NuggiesPress(KMSelectable Nuggie){
    Nuggie.AddInteractionPunch();
    GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Nuggie.transform);
    if (!moduleSolved)
    {
      if (Nuggie == Nuggies[0])
      {
        StartCoroutine(FuckMcShit());
      }
      else if (Nuggie == Nuggies[1])
      {
        CondonGenerator += kum[0];
        Debug.LogFormat("[Chicken Nuggets #{0}] Your current number is now {1}.", moduleId, CondonGenerator);
      }
      else if (Nuggie == Nuggies[2])
      {
        CondonGenerator += kum[1];
        Debug.LogFormat("[Chicken Nuggets #{0}] Your current number is now {1}.", moduleId, CondonGenerator);
      }
      else if (Nuggie == Nuggies[3])
      {
        CondonGenerator += kum[2];
        Debug.LogFormat("[Chicken Nuggets #{0}] Your current number is now {1}.", moduleId, CondonGenerator);
      }
    }
  }
  void NuggiesEnd(KMSelectable Nuggie){
    GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Nuggie.transform);
    if (!moduleSolved)
    {
      if (Nuggie == Nuggies[0])
      {
        StopAllCoroutines();
        if (truefalse == true && Simp == true)
        {
          moduleSolved = true;
          GetComponent<KMBombModule>().HandlePass();
          Debug.LogFormat("[Chicken Nuggets #{0}] The number is impossible to make, and you determined that. Module disarmed.", moduleId, CondonGenerator);
          Audio.PlaySoundAtTransform("AwesomeSex", transform);
        }
        else if (truefalse == false && CondonGenerator == Boobies)
        {
          moduleSolved = true;
          GetComponent<KMBombModule>().HandlePass();
          Debug.LogFormat("[Chicken Nuggets #{0}] You inputted the right number. Module disarmed.", moduleId, CondonGenerator);
          Audio.PlaySoundAtTransform("AwesomeSex", transform);
        }
        else if (truefalse == true && Simp == false)
        {
          Debug.LogFormat("[Chicken Nuggets #{0}] You said the number was impossible to make, but I say it's not. Strike, burger consumer.", moduleId, CondonGenerator);
          Audio.PlaySoundAtTransform("wokgowekgoiwekgwegwegw", transform);
          GetComponent<KMBombModule>().HandleStrike();
          elapsed = 0f;
          truefalse = false;
          CondonGenerator = 0;
          StartCoroutine(GenerateMcNumeros());
        }
        else
        {
          Debug.LogFormat("[Chicken Nuggets #{0}] You inputted the wrong number. Strike, fries slurper.", moduleId, CondonGenerator);
          Audio.PlaySoundAtTransform("wokgowekgoiwekgwegwegw", transform);
          GetComponent<KMBombModule>().HandleStrike();
          elapsed = 0f;
          truefalse = false;
          CondonGenerator = 0;
          StartCoroutine(GenerateMcNumeros());
        }
      }
    }
  }

  //Graph structure for the algorithm that will generate solutions

  struct Graph
  {
    public int NuggCount { get { return _nuggCount; } }
    public int[] ButtCount { get { return _buttCount; } }
    public int Time { get { return _time; } }
    public Graph(int nuggCount, int[] buttCount, int time) {
      _nuggCount = nuggCount;
      _buttCount = buttCount.ToArray();
      _time = time;
    }
    private int _nuggCount;
    private int[] _buttCount;
    private int _time;
  }

  //Algorithm that searches for the most efficient solution to given number.

  void SearchAllSolutions(int i)
  {
    string command = "";
    int minimum = int.MaxValue;
    Queue<Graph> graphQueue = new Queue<Graph>();
    graphQueue.Enqueue(new Graph(i, new int[3] { 0, 0, 0 }, 0));
    bool found = false;
    while (graphQueue.Count != 0)
    {
      Graph node = graphQueue.Dequeue();
      if (node.NuggCount < 0 || (_chickenNuggetsInfo.Distance.Count > node.NuggCount && _chickenNuggetsInfo.Distance[node.NuggCount] == int.MaxValue)) continue;
      if (node.NuggCount < _chickenNuggetsInfo.Distance.Count)
      {
        int compDist = node.Time + _chickenNuggetsInfo.Distance[node.NuggCount];
        if (minimum < compDist)
          continue;
        else
          minimum = compDist;
      }
      if (node.NuggCount == 0)
      {
        found = true;
        command = String.Join(" ", node.ButtCount.Select(x => x.ToString()).ToArray());
        if (minimum > node.Time)
            minimum = node.Time;
        graphQueue.Clear();
        continue;
      }
      int[] buttonCount = node.ButtCount.ToArray();
      graphQueue.Enqueue(new Graph(node.NuggCount - kum[2], new int[3] { buttonCount[0], buttonCount[1], buttonCount[2] + 1 }, node.Time + 1));
      graphQueue.Enqueue(new Graph(node.NuggCount - kum[1], new int[3] { buttonCount[0], buttonCount[1] + 1, buttonCount[2] }, node.Time + 1));
      graphQueue.Enqueue(new Graph(node.NuggCount - kum[0], new int[3] { buttonCount[0] + 1, buttonCount[1], buttonCount[2] }, node.Time + 1));
    }
    _chickenNuggetsInfo.Distance.Add(minimum);
    if (!found)
      command = "none";
    _chickenNuggetsInfo.Solutions.Add(command);
  }
   IEnumerator FuckMcShit(){
   while (elapsed < duration){
     yield return null;
     elapsed += Time.deltaTime;
   }
   truefalse = true;
  }
  IEnumerator GenerateMcNumeros(){
    Boobies = UnityEngine.Random.Range(0,201);
    if (_chickenNuggetsInfo.Distance[Boobies] == int.MaxValue) {
       Simp = true;
    }
    Debug.LogFormat("[Chicken Nuggets #{0}] The target number is {1}.", moduleId, Boobies);
    McBlowjob.text = Boobies.ToString();
    yield return null;
  }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press 1 2 3 <s/sub/submit> [Presses the buttons on the bottom in order from left to right and then presses the top one] | !{0} drown [Holds down the top button and releases it after a second]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*drown\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Nuggies[0].OnInteract();
            yield return new WaitForSeconds(1f);
            Nuggies[0].OnInteractEnded();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*sub\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*s\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Nuggies[0].OnInteract();
            yield return new WaitForSeconds(0.1f);
            Nuggies[0].OnInteractEnded();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify which buttons to press!";
                yield break;
            }
            string[] valids = { "s", "sub", "submit", "1", "2", "3" };
            for (int i = 1; i < parameters.Length; i++)
            {
                if (!valids.Contains(parameters[i].ToLower()))
                {
                    yield return "sendtochaterror The specified button to press '" + parameters[i] + "' is invalid!";
		    yield break;
                }
            }
            for (int i = 1; i < parameters.Length; i++)
            {
                if (parameters[i].EqualsIgnoreCase("s") || parameters[i].EqualsIgnoreCase("sub") || parameters[i].EqualsIgnoreCase("submit"))
                {
                    Nuggies[0].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    Nuggies[0].OnInteractEnded();
                }
                else if (parameters[i].EqualsIgnoreCase("1"))
                {
                    Nuggies[1].OnInteract();
                }
                else if (parameters[i].EqualsIgnoreCase("2"))
                {
                    Nuggies[2].OnInteract();
                }
                else if (parameters[i].EqualsIgnoreCase("3"))
                {
                    Nuggies[3].OnInteract();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        CondonGenerator = 0;
        if (Simp)
        {
            Nuggies[0].OnInteract();
            while (elapsed < duration) { yield return true; yield return new WaitForSeconds(0.1f); }
            Nuggies[0].OnInteractEnded();
        }
        else
        {
            string[] vals;
            string temp = _chickenNuggetsInfo.Solutions[Boobies];
            vals = temp.Split(' ');
            for (int j = 1; j < 4; j++)
            {
                for (int i = 0; i < int.Parse(vals[j - 1]); i++)
                {
                    Nuggies[j].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            Nuggies[0].OnInteract();
            yield return new WaitForSeconds(0.1f);
            Nuggies[0].OnInteractEnded();
        }
    }
}
