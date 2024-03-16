using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class ForgetfulGridScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMBossModule Boss;
	public KMColorblindMode Colorblind;

	public KMSelectable[] gridButtons;
	public KMSelectable submit;

	public Material[] colors;
	public Material bgColor;
	public MeshRenderer screen;

	public TextMesh stageCounter;
	public TextMesh[] coordinateDisplays, rowText, columnText;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved, readyToSubmit, cbActive, isActivated;

	private static string[] ignoredModules;

	private string shuffledLetters, shuffledNumbers;

	private bool flip;

	private Color[] textColors;

	private static readonly int stageCap = 99;
	private int stage = 0, nonIgnoredModules, combineSet;

	private GridColor[] currentColors;
	private List<GridColor[]> combinedColorSet = new List<GridColor[]>(), generatedColors = new List<GridColor[]>();
	private GridGeneration generator = new GridGeneration();

	private Coroutine coordinateCycle;

	private static readonly string[] colorNames = { "Orange", "Lime", "Turquoise", "Magenta", "[Empty]" };

	private string Coordinate(int pos) => flip ? $"{shuffledLetters[pos / 5]}{shuffledNumbers[pos % 5]}" : $"{shuffledLetters[pos % 5]}{shuffledNumbers[pos / 5]}";

	void Awake()
    {

		moduleId = moduleIdCounter++;

		if (ignoredModules == null)
			ignoredModules = Boss.GetIgnoredModules("Forgetful Grid", new string[]
			{
				"14",
				"501",
				"Alarming",
				"A>N<D",
				"Bamboozling Time Keeper",
				"Black Arrows",
				"Blackout",
				"Brainf---",
				"The Board Walk",
				"Busy Beaver",
				"Don't Touch Anything",
				"Floor Lights",
				"Forget Any Color",
				"Forget Enigma",
				"Forget Everything",
                "Forgetful Grid",
				"Forget Infinity",
				"Forget It Not",
				"Forget Maze Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Colors",
				"Forget Them All",
				"Forget This",
				"Forget Us Not",
				"Iconic",
				"Keypad Directionality",
				"Kugelblitz",
				"Multitask",
				"OmegaDestroyer",
				"OmegaForget",
				"Organization",
				"Password Destroyer", 
				"Purgatory",
				"Reporting Anomalies",
				"RPS Judging",
				"Security Council",
				"Shoddy Chess",
				"Simon Forgets",
				"Simon's Stages",
				"Souvenir",
				"Tallordered Keys",
				"The Time Keeper",
				"Timing is Everything",
				"The Troll",
				"Turn The Key",
				"The Twin",
                "Übermodule",
				"Ultimate Custom Night",
				"The Very Annoying Button",
				"Whiteout"
            });

		foreach (KMSelectable button in gridButtons)
		{
			button.OnInteract += delegate () { GridButtonPress(button); return false; };
			button.GetComponentInChildren<TextMesh>().text = string.Empty;
		}

		submit.OnInteract += delegate () { SubmitPress(); return false; };

		cbActive = Colorblind.ColorblindModeActive;
		Module.OnActivate += delegate () { StartCoroutine(InitializeModule()); };

    }

	
	void Start()
    {
		flip = Range(0, 2) == 0;

		nonIgnoredModules = Bomb.GetSolvableModuleNames().Count(x => !ignoredModules.Contains(x));

		shuffledLetters = new string("ABCDE".ToCharArray().Shuffle());
		shuffledNumbers = new string("12345".ToCharArray().Shuffle());

		foreach (var text in rowText)
			text.text = string.Empty;

		foreach (var text in columnText)
			text.text = string.Empty;

		foreach (var text in coordinateDisplays)
			text.text = string.Empty;

		stageCounter.text = string.Empty;

		combineSet = Range(2, 5);

		foreach (KMSelectable button in gridButtons)
			button.gameObject.SetActive(false);

		currentColors = Enumerable.Repeat(new GridColor(colorNames[4]), 25).ToArray();

		textColors = Enumerable.Range(0, 4).Select(x => colors[x].color).ToArray();

		generatedColors = generator.GeneratedColors(nonIgnoredModules);
		combinedColorSet = generator.GetCombinedSets(generatedColors, combineSet, Bomb.GetSerialNumberNumbers().Last());

    }

	IEnumerator InitializeModule()
	{
		screen.material = bgColor;

		foreach (var button in gridButtons)
			button.gameObject.SetActive(true);

		for (int i = 0; i < 5; i++)
		{
			Audio.PlaySoundAtTransform("InitialClick", transform);
			rowText[i].text = flip ? shuffledNumbers[i].ToString() : shuffledLetters[i].ToString();
			yield return new WaitForSeconds(0.25f);
		}
		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < 5; i++)
		{
			Audio.PlaySoundAtTransform("InitialClick", transform);
			columnText[i].text = flip ? shuffledLetters[i].ToString() : shuffledNumbers[i].ToString();
			yield return new WaitForSeconds(0.25f);
		}

		if (nonIgnoredModules <= 1)
		{
			Log($"[Forgetful Grid #{moduleId}] There are no non-ignored modules available or not enough stages can be added, and therefore can't generate any stages. Solving...");
			moduleSolved = true;
			Module.HandlePass();
			yield break;
		}

		isActivated = true;



    }

	IEnumerator DisplayCoordinates()
	{
		while (true)
		{
			var coordinates = Enumerable.Range(0, 25).Where(x => generatedColors[stage][x] != null).Select(Coordinate).ToArray();
			var coordinateColors = generatedColors[stage].Where(x => x != null).Select(x => x.ColorIndex).ToArray();

			var ix = 0;


			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; i++)
					coordinateDisplays[j].text = coordinates[ix + j];



				if (!cbActive)
				{
                    ix += 3;
					continue;
                }

				for (int j = 0; j < 3; j++)
					coordinateDisplays[j].color = textColors[coordinateColors[ix + j]];

                ix += 3;

            }
		}
	}

	void GridButtonPress(KMSelectable button)
	{
		if (moduleSolved || !readyToSubmit || !isActivated)
			return;

		var ix = Array.IndexOf(gridButtons, button);
    }

	void SubmitPress()
	{
		if (moduleSolved || !isActivated)
			return;
	}
	
	
	void Update()
    {
		if (moduleSolved || !isActivated || readyToSubmit)
			return;

		stageCounter.text = stage == 0 ? combineSet.ToString() : stage.ToString();

		var solved = Bomb.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x));

		if (solved == nonIgnoredModules || solved == stageCap)
		{
			StopAllCoroutines();
			coordinateCycle = null;
			readyToSubmit = true;
			stageCounter.text = combinedColorSet.Count.ToString();

			foreach (var text in coordinateDisplays)
				text.text = string.Empty;

			return;
		}

		if (solved > stage)
		{
			stage++;

			if (coordinateCycle != null)
				StopCoroutine(coordinateCycle);

			coordinateCycle = StartCoroutine(DisplayCoordinates());
		}


    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





