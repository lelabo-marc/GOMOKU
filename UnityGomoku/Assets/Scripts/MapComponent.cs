﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;
using Gomoku;

public class MapComponent : MonoBehaviour
{
		public GameObject TilePrefab;
		private List <List <Tile>> graphicMap;
		public const int SIZE_MAP = 19;
		private GameObject arbiter;
		private GameManager gameManager;
		private Rules rules;
		private Gomoku.Map map;
		private int currentX;
		private int currentY;
		public static Dictionary<Gomoku.Orientation , int[]> ORIENTATION ;
	
		void Start ()
		{
				ORIENTATION = new Dictionary<Gomoku.Orientation, int[]> ()
		{
			{ Orientation.EAST , new int[] { 1 , 0 } },
			{ Orientation.WEST , new int[] { -1 , 0 } },
			{ Orientation.SOUTH , new int[] { 0 , -1 } },
			{ Orientation.NORTH , new int[] { 0 , 1 } },
			{ Orientation.SOUTHEAST, new int[] { 1 , -1 } },
			{ Orientation.NORTHEAST , new int[] { 1 , 1 } },
			{ Orientation.SOUTHWEST , new int[] { -1 , -1 } },
			{ Orientation.NORTHWEST , new int[] { -1 , 1 } }
		};
				this.map = new Gomoku.Map (SIZE_MAP);
				arbiter = GameObject.Find ("Arbiter");
				rules = arbiter.GetComponent<Rules> ();
				gameManager = arbiter.GetComponent<GameManager> ();

				generateGraphicMap ();
		}
	
		void Update ()
		{
	
		}

		static public Gomoku.Orientation inverseOrientation (Gomoku.Orientation orientation)
		{	
				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						if (entry.Value [0] == -ORIENTATION [orientation] [0] && entry.Value [1] == -ORIENTATION [orientation] [1])
								return entry.Key;
				}
				return orientation;
		}

		private void generateGraphicMap ()
		{
				graphicMap = new List<List<Tile>> ();
				for (int i = 0; i < SIZE_MAP; ++i) {
						List <Tile> row = new List<Tile> ();
						for (int a = 0; a < SIZE_MAP; ++a) {
								Tile tile = ((GameObject)Instantiate (TilePrefab, 
				                                      new Vector3 (i - Mathf.Floor (SIZE_MAP / 2), 0, -a + Mathf.Floor (SIZE_MAP / 2)),
				                                      Quaternion.Euler (new Vector3 ()))).GetComponent<Tile> ();
								tile.name = "Tile_" + (i * SIZE_MAP + a).ToString ();
								tile.setGridPosition (new Vector2 (i, a));
								row.Add (tile);
						}
						graphicMap.Add (row);
				}
		}

		public bool PlayOnTile (int x, int y)
		{
				return this.graphicMap [x] [y].PutPawn ();
		}
		
		public bool PutPawn (int x, int y, Gomoku.Color color)
		{
				if (!rules.IsFree (map, x, y) || (rules.DoubleThree && rules.IsDoubleThree (map, x, y, color)))
						return false;
				map.PutPawn (x, y, color);

				rules.UpdateMap (map, x, y);

				map.GeneratePossibleCells (x, y, 2);
				
				gameManager.SetLastPawn (x, y);
				return true;
		}
		
		public bool removePawn (int x, int y)
		{
				if (map.GetColor (x, y) != Gomoku.Color.Empty) {
						map.RemovePawn (x, y);
				}
				Destroy (GameObject.Find ("Pawn_" + (x * SIZE_MAP + y).ToString ()));
				return true;
		}
	
		public Gomoku.Map GetMap ()
		{
				return this.map;
		}

		public static Gomoku.Orientation? FindOrientation (int wayX, int wayY)
		{
				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						if (entry.Value [0] == wayX && entry.Value [1] == wayY)
								return entry.Key;
				}
				return null;
		}
}
