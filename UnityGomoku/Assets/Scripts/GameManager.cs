﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gomoku;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
		public GameObject player1;
		public GameObject player2;
		private int lastX;
		private int lastY;
		private PlayerComponent playerComponent1;
		private PlayerComponent playerComponent2;
		public Rules rules;
		public MapComponent map;
		private Gomoku.Color winner;
		private GameObject end;
		private GameObject menu;
		private bool lastPlayer;

		void Start ()
		{
				Text c = GameObject.Find ("score whiteE").GetComponent<Text> ();
				c.enabled = false;
				Text d = GameObject.Find ("score blackE").GetComponent<Text> ();
				d.enabled = false;
				menu = GameObject.Find ("Menu");
				end = GameObject.Find ("End");
				NGUITools.SetActive (end, false);
				NGUITools.SetActive (menu, false);
				
				rules = GameObject.Find ("Arbiter").GetComponent<Rules> ();
				map = GameObject.Find ("Map").GetComponent<MapComponent> ();
				Image imgW = GameObject.Find ("imgW").GetComponent<Image> ();
				Image imgB = GameObject.Find ("imgB").GetComponent<Image> ();
				
				
				map.GetMap ().id = 1;
				winner = Gomoku.Color.Empty;

				playerComponent1 = player1.GetComponent<PlayerComponent> ();
				playerComponent1.color = Gomoku.Color.White;
				playerComponent1.playing = true;
				imgW.enabled = true;

				playerComponent2 = player2.GetComponent<PlayerComponent> ();
				if (PlayerPrefs.GetInt ("IA") > 0) {
						int ialevel = PlayerPrefs.GetInt ("IaLevel");
						if (ialevel < 1) 
								ialevel = 1;
								playerComponent2.Ia = new MCTS_IA (SystemInfo.processorCount + 1, 100 + ialevel * 300);
						
				}
				playerComponent2.color = Gomoku.Color.Black;
				playerComponent2.playing = false;
				imgB.enabled = false;
		}

		public void closeWin ()
		{
				NGUITools.SetActive (menu, false);
				if (lastPlayer)
						this.playerComponent1.playing = true;
				else
						this.playerComponent2.playing = true;
		}

		public void openWin ()
		{
				NGUITools.SetActive (menu, true);
				if (this.playerComponent1.playing)
						lastPlayer = true;
				if (this.playerComponent2.playing)
						lastPlayer = false;
				this.playerComponent1.playing = false;
				this.playerComponent2.playing = false;
		}

		void Update ()
		{
				if (currentPlayer ().playing && currentPlayer ().played) {
						if ((winner = CheckMap (this.lastX, this.lastY, map.GetMap ())) != Gomoku.Color.Empty)
								GameDone ();
						else if (!isEnoughSpace ())
								GameDone ();
						else 
								changeTurn ();
				}
				if (Input.GetKeyDown (KeyCode.Escape)) {
					
						if (NGUITools.GetActive (menu)) {
								NGUITools.SetActive (menu, false);
								if (lastPlayer)
										this.playerComponent1.playing = true;
								else
										this.playerComponent2.playing = true;
						} else {
								NGUITools.SetActive (menu, true);
								if (this.playerComponent1.playing)
										lastPlayer = true;
								if (this.playerComponent2.playing)
										lastPlayer = false;
								this.playerComponent1.playing = false;
								this.playerComponent2.playing = false;
						}
				}
		}

		public Gomoku.Color CheckMap (int x, int y, Gomoku.Map map)
		{
				Gomoku.Color win = Gomoku.Color.Empty;
				CheckTakePawns (x, y, map);

				if ((win = isScoringWinner (map)) != Gomoku.Color.Empty) {
						return win;
				}
			
				if ((win = isWinner (map)) != Gomoku.Color.Empty) {
						return win;
				}
			
				return win;
		}

		public PlayerComponent currentPlayer ()
		{
				if (playerComponent1.playing)
						return playerComponent1;
				return playerComponent2;
		}

		private void CheckTakePawns (int x, int y, Gomoku.Map m)
		{
				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						if (rules.CanTakePawns (m, x, y, entry.Key)) {
								rules.TakePawns (m, x, y, entry.Key);
								if (m.id == 1) {
										map.removePawn (x + MapComponent.ORIENTATION [entry.Key] [0], y + MapComponent.ORIENTATION [entry.Key] [1]);
										map.removePawn (x + 2 * MapComponent.ORIENTATION [entry.Key] [0], y + 2 * MapComponent.ORIENTATION [entry.Key] [1]);
								}
						}
					
				}
		}

		public void SetLastPawn (int x, int y)
		{
				lastX = x;
				lastY = y;
		}

		public PlayerComponent getActivePlayer ()
		{
				if (playerComponent1.playing)
						return playerComponent1;
				return playerComponent2;
		}
		
		private bool isEnoughSpace ()
		{
				Gomoku.Map tab = map.GetMap ();

				for (int x = 0; x < MapComponent.SIZE_MAP; ++x) {
						for (int y = 0; y < MapComponent.SIZE_MAP; ++y) {
								if (tab.GetColor (x, y) == Gomoku.Color.Empty)
										return true;
						}
				}
				return false;
		}

		private void changeTurn ()
		{
				Image imgW = GameObject.Find ("imgW").GetComponent<Image> ();
				Image imgB = GameObject.Find ("imgB").GetComponent<Image> ();
				playerComponent1.playing = !playerComponent1.playing;
				playerComponent2.playing = !playerComponent2.playing;

				playerComponent1.played = false;
				playerComponent2.played = false;

				if (playerComponent1.playing) {
					
						imgW.enabled = true;
						imgB.enabled = false;
						GameObject.Find ("textW").GetComponent<FadeMaterialsTxt> ().ShowImage (); 
						;
				} else {
					
						imgW.enabled = false;
						imgB.enabled = true;
						if (playerComponent2.Ia != null)
								GameObject.Find ("textB").GetComponent<FadeMaterials> ().ShowImage (); 
						;
				}
		}

		private Gomoku.Color isScoringWinner (Gomoku.Map map)
		{
				return rules.IsScoringWinner (map);
		}

		private Gomoku.Color isWinner (Gomoku.Map map)
		{
				return rules.IsWinner (map, currentPlayer ().color);
		}

		private void GameDone ()
		{
				this.playerComponent1.playing = false;
				this.playerComponent2.playing = false;
				Image imgW = GameObject.Find ("imgW").GetComponent<Image> ();
				Image imgB = GameObject.Find ("imgB").GetComponent<Image> ();
				Image bB = GameObject.Find ("backG").GetComponent<Image> ();
				imgW.enabled = false;
				imgB.enabled = false;
				bB.enabled = false;
				Text c = GameObject.Find ("score white").GetComponent<Text> ();
				c.enabled = false;
				Text d = GameObject.Find ("score black").GetComponent<Text> ();
				d.enabled = false;
				Text indic = GameObject.Find ("Indic").GetComponent<Text> ();
				indic.enabled = false;
				Text cf = GameObject.Find ("score whiteE").GetComponent<Text> ();
				cf.enabled = true;
				Text df = GameObject.Find ("score blackE").GetComponent<Text> ();
				df.enabled = true;
				NGUITools.SetActive (end, true);
				if (winner == Gomoku.Color.White) {
					
						PlayerPrefs.SetInt ("Winner", 0);
				} else if (winner == Gomoku.Color.Black) {
						
						PlayerPrefs.SetInt ("Winner", 1);
				} else {
						
						PlayerPrefs.SetInt ("Winner", 2);
				}
		}
}
