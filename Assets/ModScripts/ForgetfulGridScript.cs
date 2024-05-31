using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;
using System.Reflection.Emit;

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
	private int stage = 0, nonIgnoredModules, combinedSetIx = 0;

	private Coroutine coordinateCycle, stageRecovery;
	private bool firstTime = true;

	private string Coordinate(int pos) => $"{shuffledLetters[flip ? pos / 5 : pos % 5]}{shuffledNumbers[flip ? pos % 5 : pos / 5]}";

	private static readonly string[] colorNames = { "Orange", "Lime", "Turquoise", "Magenta", "Empty" };


    private List<ColorGrid> generatedStages;
	private List<GridColorOption[]> combinedSets;
	private PriorityNumbers priorityList;

	private GridColorOption[] currentGrid;
	private int[] currentGridIx;

	private StageGenerator generateStages;

	private List<string> LoggedCoordinates(List<ColorGrid> grids)
	{
		var log = new List<string>();

		var allGrids = grids.Select(x => x.Grid).ToList();

		var coordinates = Enumerable.Range(0, allGrids.Count)
			.Select(x => Enumerable.Range(0, 25).Where(y => allGrids[x][y].ColorName != "Empty").Select(y => $"{Coordinate(y)} in {allGrids[x][y].ColorName}").ToArray()).ToList();

		foreach (var coordinate in coordinates)
			log.Add(coordinate.Join(", "));

		return log;
	}

	private List<string> LoggedCombinedCoordinates(List<GridColorOption[]> grids)
	{
		var log = new List<string>();

		var coordinates = Enumerable.Range(0, grids.Count)
			.Select(x => Enumerable.Range(0, 25).Where(y => grids[x][y].ColorName != "Empty").Select(y => $"{Coordinate(y)} in {grids[x][y].ColorName}").ToArray()).ToList();

		foreach (var coordinate in coordinates)
			log.Add(coordinate.Join(", "));

		return log;
	}

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
		Module.OnActivate += OnActivate;

    }

	
	void Start()
    {
		flip = Range(0, 2) == 0;

		nonIgnoredModules = Bomb.GetSolvableModuleNames().Count(x => !ignoredModules.Contains(x)) - 1;

		shuffledLetters = new string("ABCDE".ToCharArray().Shuffle());
		shuffledNumbers = new string("12345".ToCharArray().Shuffle());

		foreach (var text in rowText)
			text.text = string.Empty;

		foreach (var text in columnText)
			text.text = string.Empty;

		foreach (var text in coordinateDisplays)
			text.text = string.Empty;

		stageCounter.text = string.Empty;

		foreach (KMSelectable button in gridButtons)
			button.gameObject.SetActive(false);

		textColors = Enumerable.Range(0, 4).Select(x => colors[x].color).ToArray();

		priorityList = new PriorityNumbers(Range(0, 3), Bomb.GetSerialNumberNumbers().Last());
		generateStages = new StageGenerator(colors, nonIgnoredModules, priorityList);
		generatedStages = generateStages.StagesGenerated;
		combinedSets = generateStages.CombinedSet(colors.Last());

		currentGrid = Enumerable.Repeat(new GridColorOption("Empty", colors.Last()), 25).ToArray();
		currentGridIx = Enumerable.Repeat(4, 25).ToArray();

		if (nonIgnoredModules > stageCap)
		{
			generatedStages.RemoveRange(stageCap, nonIgnoredModules);
			combinedSets.RemoveRange(stageCap, nonIgnoredModules);
		}

        Log($"[Forgetful Grid #{moduleId}] {priorityList.Sets.Length} sets will be combined.");

		Log($"[Forgetful Grid #{moduleId}] The row is displayed as {(flip ? shuffledLetters.Join("") : shuffledNumbers.Join(""))}");
		Log($"[Forgetful Grid #{moduleId}] The column is displayed as {(flip ? shuffledNumbers.Join("") : shuffledLetters.Join(""))}");

		var generatedCoords = LoggedCoordinates(generatedStages);
		var generatedCombinedCoords = LoggedCombinedCoordinates(combinedSets);

		for (int i = 0; i < generatedCoords.Count; i++)
			Log($"[Forgetful Grid #{moduleId}] Stage {i + 1}: {generatedCoords[i]}");

		Log($"[Forgetful Grid #{moduleId}] *==========================================================*");
		Log($"[Forgetful Grid #{moduleId}] There are a total of {combinedSets.Count} combined sets. They are as follows:");

		for (int i = 0; i < generatedCombinedCoords.Count; i++)
			Log($"[Forgetful Grid #{moduleId}] {generatedCombinedCoords[i]}");
    }

	void OnActivate()
	{
        screen.material = bgColor;

        foreach (var button in gridButtons)
            button.gameObject.SetActive(true);

        if (nonIgnoredModules <= 1)
        {
            Log($"[Forgetful Grid #{moduleId}] There are no non-ignored modules available or not enough stages can be added, and therefore can't generate any stages. Solving...");
            moduleSolved = true;
            Module.HandlePass();
            return;
        }

		isActivated = true;
    }

	IEnumerator InitializeCoordDisplay()
	{
		for (int i = 0; i < 5; i++)
		{
			Audio.PlaySoundAtTransform("InitialClick", transform);
			rowText[i].text = flip ? shuffledLetters[i].ToString() : shuffledNumbers[i].ToString();
			yield return new WaitForSeconds(0.085f);
		}

		for (int i = 0; i < 5; i++)
		{
			Audio.PlaySoundAtTransform("InitialClick", transform);
			columnText[i].text = flip ? shuffledNumbers[i].ToString() : shuffledLetters[i].ToString();
			yield return new WaitForSeconds(0.085f);
		}
    }

	IEnumerator DisplayCoordinates(int stageIx)
	{

		var displayedStage = generatedStages[stageIx].Grid;


		var selectedIxes = Enumerable.Range(0, 25).Where(x => displayedStage[x].ColorName != "Empty").ToArray();
		var selectedCoords = selectedIxes.Select(Coordinate).ToArray();
		var selectedColors = Enumerable.Range(0, 25).Where(x => displayedStage[x].ColorName != "Empty").Select(x => textColors[Array.IndexOf(colorNames, displayedStage[x].ColorName)]).ToArray();
		var colorblind = selectedIxes.Select(x => Array.IndexOf(colorNames, displayedStage[x].ColorName)).ToArray();

		string[][] splitCoords = { new[] { selectedCoords[0], selectedCoords[1], selectedCoords[2] }, new[] { selectedCoords[3], selectedCoords[4], selectedCoords[5] } };
		Color[][] splitColors = { new[] { selectedColors[0], selectedColors[1], selectedColors[2] }, new[] { selectedColors[3], selectedColors[4], selectedColors[5] } };
		int[][] splitCb = {new[] { colorblind[0], colorblind[1], colorblind[2] }, new[] { colorblind[3], colorblind[4], colorblind[5] } };

		while (true)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					coordinateDisplays[j].text = splitCoords[i][j];
					coordinateDisplays[j].color = splitColors[i][j];
				}

				yield return new WaitForSeconds(1);

				if (!cbActive)
					continue;

				for (int j = 0; j < 3; j++)
				{
					coordinateDisplays[j].text = colorNames[splitCb[i][j]][0].ToString();
					coordinateDisplays[j].color = Color.white;
				}

				yield return new WaitForSeconds(1);
			}
		}

	}

	IEnumerator StageRecovery()
	{
		while (true)
		{
			for (int i = 0; i < generatedStages.Count; i++)
			{
				coordinateCycle = StartCoroutine(DisplayCoordinates(i));
				stageCounter.text = (i + 1).ToString();
				yield return new WaitForSeconds(cbActive ? 4 : 2);
			}
		}
	}

	void GridButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);

		if (moduleSolved || !readyToSubmit || !isActivated)
		{
			Audio.PlaySoundAtTransform("Inactive", transform);
			return;
		}

		if (stageRecovery != null)
		{
			StopCoroutine(stageRecovery);
			stageRecovery = null;

			foreach (var text in coordinateDisplays)
				text.text = string.Empty;

			stageCounter.text = string.Empty;
		}

		Audio.PlaySoundAtTransform("Button", transform);

		var ix = Array.IndexOf(gridButtons, button);

		currentGridIx[ix]++;
		currentGridIx[ix] %= 5;

		currentGrid[ix].ColorName = colorNames[currentGridIx[ix]];
		currentGrid[ix].Color = colors[currentGridIx[ix]];
		button.GetComponentInChildren<TextMesh>().text = cbActive && currentGridIx[ix] != 4 ? colorNames[currentGridIx[ix]][0].ToString() : string.Empty;

		button.GetComponentInChildren<MeshRenderer>().material = currentGrid[ix].Color;
    }

	void SubmitPress()
	{
		submit.AddInteractionPunch(0.4f);

		if (moduleSolved || !isActivated || !readyToSubmit || stageRecovery != null)
		{
			Audio.PlaySoundAtTransform("Inactive", transform);
			return;
		}

		var currentGridColorNames = currentGrid.Select(x => x.ColorName).ToArray();
		var solutionGridColorNames = combinedSets[0].Select(x => x.ColorName).ToArray();

		if (solutionGridColorNames.SequenceEqual(currentGridColorNames))
		{
			combinedSets.RemoveAt(0);
			stageCounter.text = combinedSets.Count.ToString();
		}
		else
		{
			Module.HandleStrike();

		}


	}
	
	
	void Update()
    {
		if (moduleSolved || !isActivated || readyToSubmit)
			return;

		stageCounter.text = stage == 0 ? priorityList.Sets.Length.ToString() : stage.ToString();

		var solved = Bomb.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x));

		if (solved == nonIgnoredModules || solved == stageCap)
		{
			StopAllCoroutines();
			coordinateCycle = null;
			readyToSubmit = true;

			foreach (var disp in coordinateDisplays)
				disp.text = string.Empty;

			stageCounter.text = combinedSets.Count.ToString();

			return;
		}

		if (solved > stage)
		{
			stage++;

			if (coordinateCycle != null)
				StopCoroutine(coordinateCycle);

			coordinateCycle = StartCoroutine(DisplayCoordinates(stage - 1));

			if (firstTime)
			{
				StartCoroutine(InitializeCoordDisplay());
				firstTime = false;
			}
		}

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





