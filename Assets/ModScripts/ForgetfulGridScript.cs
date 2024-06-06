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

	public TextMesh stageCounter, submitButtonText;
	public TextMesh[] coordinateDisplays, rowText, columnText;

	static int moduleIdCounter = 1, forgetfulGridIdCounter = 1;
	int moduleId, forgetfulGridId;
	private bool moduleSolved, readyToSubmit, cbActive, isActivated;

	private static string[] ignoredModules;

	private string shuffledLetters, shuffledNumbers;

	private bool flip;

	private Color[] textColors;

	private static readonly int stageCap = 99;
	private int stage = 0, nonIgnoredModules;

	private Coroutine coordinateCycle, stageRecovery;
	private bool firstTime = true;
	private bool isSolving;

	private string Coordinate(int pos) => $"{shuffledLetters[flip ? pos / 5 : pos % 5]}{shuffledNumbers[flip ? pos % 5 : pos / 5]}";

	private static readonly string[] colorNames = { "Orange", "Lime", "Turquoise", "Magenta", "Empty" },
		colorNamesAbbrev = { "O", "L", "T", "M", "-" };


    private List<ColorGrid> generatedStages;
	private List<GridColorOption[]> combinedSets;
	private PriorityNumbers priorityList;
	private List<List<int>> individualStageIxes;

	private GridColorOption[] currentGrid;
	private int[] currentGridIx;

	private StageGenerator generateStages;

	private List<string> LogCoordinatesAsGrid(List<ColorGrid> grids)
	{
		var log = new List<string>();

		var allGrids = grids.Select(x => x.Grid).ToList();
		foreach (var aGrid in allGrids)
        {
			var gridToLog = Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => colorNamesAbbrev[colorNames.ToList().IndexOf(aGrid[5 * r + c].ColorName)]).Join("")).Join(";");
			/* Create an enum from 0-4 (r)
			 * Inside that, for each r, create another enum from 0-4 (c)
			 * From each in c, from there, obtain the color name of the grid in 5*r+c, and find its index in colorNames.
			 * Use said index to find element in abbreviated form.
			 * Join each in c with "", then finally, join each in r with ";"
			 */
			log.Add(gridToLog);
		}

		return log;
	}
	private List<string> LogCombinedCoordinatesAsGrid(List<GridColorOption[]> grids)
	{
		var log = new List<string>();

		foreach (var aGrid in grids.ToList())
		{
			var gridToLog = Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => colorNamesAbbrev[colorNames.ToList().IndexOf(aGrid[5 * r + c].ColorName)]).Join("")).Join(";");
			/* Create an enum from 0-4 (r)
			 * Inside that, for each r, create another enum from 0-4 (c)
			 * From each in c, from there, obtain the color name of the grid in 5*r+c, and find its index in colorNames.
			 * Use said index to find element in abbreviated form.
			 * Join each in c with "", then finally, join each in r with ";"
			 */
			log.Add(gridToLog);
		}

		return log;
	}
	private List<string> LogPriority(IEnumerable<int> priority)
    {
		var priorityRefs = new Dictionary<int, string[]> {
			{ 2, new[] { "Lowest", "Highest", } },
			{ 3, new[] { "Lowest", "Medium", "Highest", } },
			{ 4, new[] { "Lowest", "2nd Lowest", "2nd Highest", "Highest", } },
		};
		return priorityRefs.ContainsKey(priority.Count()) ? priority.Select(a => priorityRefs[priority.Count()][a]).ToList() : new List<string>() ;
    }
	/*
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
	*/
	void Awake()
    {

		moduleId = moduleIdCounter++;
		forgetfulGridId = forgetfulGridIdCounter++;

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

		submitButtonText.text = string.Empty;

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

		if (nonIgnoredModules <= 1)
		{
            Log($"[Forgetful Grid #{moduleId}] There are no non-ignored modules available or not enough stages can be added, and therefore can't generate any stages. Solving...");
			return;
        }

		textColors = Enumerable.Range(0, 4).Select(x => colors[x].color).ToArray();

		priorityList = new PriorityNumbers(Range(0, 3), Bomb.GetSerialNumberNumbers().Last());
		generateStages = new StageGenerator(colors, nonIgnoredModules, priorityList);
		generatedStages = generateStages.StagesGenerated;
		combinedSets = generateStages.CombinedSets.ToList();
		individualStageIxes = generateStages.IndividualIxes;

		currentGrid = Enumerable.Range(0, 25).Select(_ => new GridColorOption(colorNames.Last(), colors.Last())).ToArray();
		currentGridIx = Enumerable.Range(0, 25).Select(_ => 4).ToArray();

		if (nonIgnoredModules > stageCap)
		{
			generatedStages.RemoveRange(stageCap, nonIgnoredModules);
			combinedSets.RemoveRange(stageCap, nonIgnoredModules);
		}

        Log($"[Forgetful Grid #{moduleId}] Sets of {priorityList.Sets.Length} stages will be combined.");

		Log($"[Forgetful Grid #{moduleId}] The row is displayed as {(flip ? shuffledLetters.Join("") : shuffledNumbers.Join(""))}");
		Log($"[Forgetful Grid #{moduleId}] The column is displayed as {(flip ? shuffledNumbers.Join("") : shuffledLetters.Join(""))}");

		Log($"[Forgetful Grid #{moduleId} The priority set is as follows for the grouped stages: {LogPriority(priorityList.Sets).Join(", ")}");

		Log($"[Forgetful Grid #{moduleId}] Stages generated: {nonIgnoredModules}");

		var generatedCoords = LogCoordinatesAsGrid(generatedStages);
		var generatedCombinedCoords = LogCombinedCoordinatesAsGrid(combinedSets);

		for (int i = 0; i < generatedCoords.Count; i++)
			Log($"[Forgetful Grid #{moduleId}] Stage {i + 1}: {generatedCoords[i]}");

		Log($"[Forgetful Grid #{moduleId}] *==========================================================*");
		Log($"[Forgetful Grid #{moduleId}] There are a total of {combinedSets.Count} combined sets. They are as follows:");

		foreach (var combinedCoord in generatedCombinedCoords)
            Log($"[Forgetful Grid #{moduleId}] {combinedCoord}");
			
    }

    void OnDestroy() => forgetfulGridIdCounter = 1;

    void OnActivate()
	{
        screen.material = bgColor;

		submitButtonText.text = "SUBMIT";

        foreach (var button in gridButtons)
            button.gameObject.SetActive(true);

        if (nonIgnoredModules <= 1)
        {
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

			if (forgetfulGridId == 1)
                Audio.PlaySoundAtTransform("InitialClick", transform);

            rowText[i].text = flip ? shuffledLetters[i].ToString() : shuffledNumbers[i].ToString();
			yield return new WaitForSeconds(0.085f);
		}

		for (int i = 0; i < 5; i++)
		{
			if (forgetfulGridId == 1)
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
		var selectedColors = selectedIxes.Select(x => textColors[Array.IndexOf(colorNames, displayedStage[x].ColorName)]).ToArray();
		var colorblind = selectedIxes.Select(x => Array.IndexOf(colorNames, displayedStage[x].ColorName)).ToArray();

		string[][] splitCoords = { new[] { selectedCoords[0], selectedCoords[1], selectedCoords[2] }, new[] { selectedCoords[3], selectedCoords[4], selectedCoords[5] } };
		Color[][] splitColors = { new[] { selectedColors[0], selectedColors[1], selectedColors[2] }, new[] { selectedColors[3], selectedColors[4], selectedColors[5] } };
		int[][] splitCb = { new[] { colorblind[0], colorblind[1], colorblind[2] }, new[] { colorblind[3], colorblind[4], colorblind[5] } };

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
		stageCounter.text = string.Empty;

		var combinedSetIxes = Enumerable.Range(0, 25).Where(x => combinedSets.First()[x].ColorName != "Empty").ToArray();
		var combinedCoords = combinedSetIxes.Select(Coordinate).ToList();
		var combinedColors = combinedSetIxes.Select(x => combinedSets.First()[x].Color.color).ToList();
		var combinedCb = combinedSetIxes.Select(x => Array.IndexOf(colorNames, combinedSets.First()[x].ColorName)).ToList();

		var separateCoordSet = combinedCoords.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 3).Select(x => x.Select(v => v.Value).ToList()).ToList();
		var separateColorSet = combinedColors.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 3).Select(x => x.Select(v => v.Value).ToList()).ToList();
		var separateCb = combinedCb.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 3).Select(x => x.Select(v => v.Value).ToList()).ToList();

		for (int i = 0; i < priorityList.Sets.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				coordinateDisplays[j].text = separateCoordSet[i][j];
				coordinateDisplays[j].color = separateColorSet[i][j];
			}
			yield return new WaitForSeconds(2);

			if (!cbActive)
				continue;

			for (int j = 0; j < 3; j++)
			{
				coordinateDisplays[j].text = colorNames[separateCb[i][j]][0].ToString();
				coordinateDisplays[j].color = Color.white;
			}

			yield return new WaitForSeconds(2);

		}

		stageRecovery = StartCoroutine(ShowRecoveredStage(individualStageIxes[0]));
	}

	IEnumerator ShowRecoveredStage(List<int> stages)
	{
		stageCounter.color = new Color(1, 1, 0);

		while (true)
            foreach (var stage in stages)
            {
                var stageCoordinates = Enumerable.Range(0, 25).Where(x => generatedStages[stage].Grid[x].ColorName != "Empty").ToArray();
                var stageCoordinateConverted = stageCoordinates.Select(Coordinate).ToArray();
                var stageColors = stageCoordinates.Select(x => generatedStages[stage].Grid[x].Color.color).ToArray();
                var colorblind = stageCoordinates.Select(x => Array.IndexOf(colorNames, generatedStages[stage].Grid[x].ColorName)).ToArray();

                string[][] splitCoords = { new[] { stageCoordinateConverted[0], stageCoordinateConverted[1], stageCoordinateConverted[2] }, new[] { stageCoordinateConverted[3], stageCoordinateConverted[4], stageCoordinateConverted[5] } };
                Color[][] splitColors = { new[] { stageColors[0], stageColors[1], stageColors[2] }, new[] { stageColors[3], stageColors[4], stageColors[5] } };
                int[][] splitCb = { new[] { colorblind[0], colorblind[1], colorblind[2] }, new[] { colorblind[3], colorblind[4], colorblind[5] } };

				stageCounter.text = (stage + 1).ToString();

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

	void GridButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);

		if (moduleSolved || !readyToSubmit || !isActivated || isSolving)
		{
			Audio.PlaySoundAtTransform("Inactive", transform);
			return;
		}

		if (stageRecovery != null)
		{
			StopAllCoroutines();
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
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

		if (moduleSolved || !isActivated || !readyToSubmit || stageRecovery != null || isSolving)
		{
			Audio.PlaySoundAtTransform("Inactive", transform);
			return;
		}

		var currentGridColorNames = currentGrid.Select(x => x.ColorName).ToArray();
		var solutionGridColorNames = combinedSets.First().Select(x => x.ColorName).ToArray();

		if (solutionGridColorNames.SequenceEqual(currentGridColorNames))
		{
			combinedSets.RemoveAt(0);
			individualStageIxes.RemoveAt(0);
			
			if (combinedSets.Count == 0)
			{
				Log($"[Forgetful Grid #{moduleId}] All combined sets have been submitted correctly. Solved!");
				isSolving = true;
				stageCounter.text = string.Empty;
				StartCoroutine(SolveAnimation());
				return;
			}

			Audio.PlaySoundAtTransform("Correct", transform);

            stageCounter.text = combinedSets.Count.ToString(); stageCounter.text = combinedSets.Count.ToString();
        }
		else
		{
			Audio.PlaySoundAtTransform("Incorrect", transform);
			//Log($"[Forgetful Grid #{moduleId}] The current grid ({Enumerable.Range(0, 25).Select(x => $"{Coordinate(x)} in {currentGrid[x].ColorName}").Join(", ")}) is not valid. Strike!");
			Log($"[Forgetful Grid #{moduleId}] The current grid ({Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => colorNamesAbbrev[colorNames.ToList().IndexOf(currentGrid[5 * r + c].ColorName)]).Join("")).Join(";")}) is not valid. Strike!");
			Module.HandleStrike();
			stageRecovery = StartCoroutine(StageRecovery());
		}

		currentGrid = Enumerable.Range(0, 25).Select(_ => new GridColorOption(colorNames.Last(), colors.Last())).ToArray();
		currentGridIx = Enumerable.Repeat(4, 25).ToArray();

		for (int i = 0; i < currentGrid.Length; i++)
			gridButtons[i].GetComponentInChildren<MeshRenderer>().material = colors[currentGridIx[i]];

		foreach (var text in gridButtons.Select(x => x.GetComponentInChildren<TextMesh>()).ToArray())
			text.text = string.Empty;
    }

	IEnumerator SolveAnimation()
	{
		Audio.PlaySoundAtTransform("Solve", transform);
		StartCoroutine(ClearCoordinateDisplays());

		var allMeshes = Enumerable.Range(0, 25).Select(x => gridButtons[x].GetComponentInChildren<MeshRenderer>()).ToArray();

		foreach (var text in gridButtons)
			text.GetComponentInChildren<TextMesh>().text = string.Empty;

		var solveGridText = new[]
		{
			"x...x.x.x...x....x....x..",
			".xxx.x...xx...xx...x.xxx.",
			"x...xx...xx...xx...x.xxx.",
			"x...xx...xx.x.xxx.xxx...x",
			"xxxxx..x....x....x..xxxxx",
			"x...xxx..xx.x.xx..xxx...x"
		}.Select(letter => letter.Select(cell => cell == 'x').ToArray()).ToArray();

		var goFast = false;

		for (int i = 0; i < solveGridText.Length; i++)
		{
			var randomColor = colors[Range(0, 4)];

			for (int j = 0; j < 25; j++)
				allMeshes[j].material = solveGridText[i][j] ? randomColor : colors.Last();

			yield return new WaitForSeconds(goFast ? 0.125f : 0.250f);

			if (i == 1)
				goFast = true;
		}

		foreach (var mat in allMeshes)
			mat.material = colors.Last();

		yield return new WaitForSeconds(1);

		isSolving = false;

		moduleSolved = true;
		Module.HandlePass();

		stageCounter.text = "GG";
		stageCounter.color = Color.green;

		while (true)
		{
			foreach (var mat in allMeshes)
				mat.material = colors.PickRandom();

			yield return new WaitForSeconds(0.5f);
		}
	}

	IEnumerator ClearCoordinateDisplays()
	{
		foreach (var text in rowText)
		{
			text.text = string.Empty;
			yield return new WaitForSeconds(0.085f);
		}

		foreach (var text in columnText)
		{
			text.text = string.Empty;
			yield return new WaitForSeconds(0.085f);
		}
	}
	
	
	void Update()
    {
		if (moduleSolved || !isActivated || readyToSubmit)
			return;

		stageCounter.text = stage == 0 ? priorityList.Sets.Length.ToString() : stage.ToString();

		if (stage > 0 && stageCounter.color != Color.green)
			stageCounter.color = Color.green;

		var solved = Bomb.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x));

		if (solved == nonIgnoredModules + 1 || solved == stageCap)
		{
			StopAllCoroutines();
			coordinateCycle = null;
			readyToSubmit = true;

			if (firstTime)
			{
				firstTime = false;
				StartCoroutine(InitializeCoordDisplay());
			}

			foreach (var disp in coordinateDisplays)
				disp.text = string.Empty;

			stageCounter.text = combinedSets.Count.ToString();
			stageCounter.color = Color.white;

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
	private readonly string TwitchHelpMessage = @"!{0} submit presses the submit button || !{0} cb toggles colorblind || ABCDE12345OLTMK places the color on that coordinate (e.g. A4M would place magenta at A4 in regular coordinates)";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;

        if ("CB".ContainsIgnoreCase(split[0]))
        {
            if (split.Length > 1)
                yield break;

            cbActive = !cbActive;

            for (int i = 0; i < 25; i++)
                gridButtons[i].GetComponentInChildren<TextMesh>().text = cbActive && currentGridIx[i] != 4 ? colorNames[currentGridIx[i]][0].ToString() : string.Empty;

			if (coordinateCycle != null)
			{
				StopCoroutine(coordinateCycle);
				coordinateCycle = StartCoroutine(DisplayCoordinates(stage - 1));
			}

			if (stageRecovery != null)
			{
				StopCoroutine(stageRecovery);
				stageRecovery = StartCoroutine(StageRecovery());
			}

            yield break;
        }


        if (!readyToSubmit)
		{
			yield return "sendtochaterror The module isn't ready to be submitted yet!";
			yield break;
		}

		if ("SUBMIT".ContainsIgnoreCase(split[0]))
		{
			if (split.Length > 1)
				yield break;

			submit.OnInteract();
			yield return new WaitForSeconds(0.1f);
			yield return "solve";

			yield break;
		}


		foreach (var coord in split)
		{
			if (coord.Length != 3)
				yield break;

			if (!"ABCDE".Contains(coord[0]))
			{
				yield return $"sendtochaterror {coord[0]} is not a valid letter!";
				yield break;
			}

			if (!"12345".Contains(coord[1]))
			{
				yield return $"sendtochaterror {coord[1]} is not a valid number!";
				yield break;
			}

			if (!"OLTMK".Contains(coord[2]))
			{
				yield return $"sendtochaterror {coord[2]} is not a valid color!";
				yield break;
			}

			var getButtonIx = coord[0] - 'A' + 5 * (coord[1] - '1');

			while (currentGrid[getButtonIx].ColorName != colorNames["OLTMK".IndexOf(coord[2])])
			{
				gridButtons[getButtonIx].OnInteract();
				yield return new WaitForSeconds(0.04f);
			}
		}
		
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!readyToSubmit)
		{
			if (stageRecovery != null)
				goto skipwait;

            yield return true;
        }

	skipwait:;

		while (true)
		{
			if (isSolving)
				break;

			for (int i = 0; i < 25; i++)
				while (currentGrid[i].ColorName != combinedSets.First()[i].ColorName)
				{
					gridButtons[i].OnInteract();
					yield return new WaitForSeconds(0.04f);
				}

			submit.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

		while (!moduleSolved)
			yield return true;
    }


}





