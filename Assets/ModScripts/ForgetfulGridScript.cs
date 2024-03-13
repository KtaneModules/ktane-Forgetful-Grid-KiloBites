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

	public TextMesh stageCounter;
	public TextMesh[] coordinateDisplays, rowText, columnText, cbTexts;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved, readyToSubmit, cbActive, isActivated;

	private static string[] ignoredModules;

	private string shuffledLetters, shuffledNumbers;

	private bool flip;

	private static readonly int stageCap = 99;
	private int stage = 0, nonIgnoredModules;

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
			button.OnInteract += delegate () { GridButtonPress(button); return false; };

		submit.OnInteract += delegate () { SubmitPress(); return false; };

		cbActive = Colorblind.ColorblindModeActive;

    }

	
	void Start()
    {
		flip = Range(0, 2) == 0;

		nonIgnoredModules = Bomb.GetSolvableModuleNames().Count(x => !ignoredModules.Contains(x));

		shuffledLetters = new string("ABCDE".ToCharArray().Shuffle());
		shuffledNumbers = new string("12345".ToCharArray().Shuffle());
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





