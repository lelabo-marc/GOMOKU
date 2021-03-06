﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gomoku;

public class Rules : MonoBehaviour
{

		public bool FiveBreakable = false;
		public bool DoubleThree = false;
		private List<int[]> masks;
	
		void Start ()
		{
				if (PlayerPrefs.GetInt ("5 cassables") > 1) {
						FiveBreakable = true;
				}
				if (PlayerPrefs.GetInt ("double 3") > 1) {
						DoubleThree = true;
				}
				masks = new List<int[]> ();
				masks.Add (new int[] { -1, 0, 2, 2, 1, 0, -1, -1, -1});
				masks.Add (new int[] { 0, 2, 2, 0, 1, 0, -1, -1, -1});
				masks.Add (new int[] { 0, 2, 0, 2, 1, 0, -1, -1, -1});
				masks.Add (new int[] { -1, -1, 0, 2, 1, 2, 0, -1, -1});
				masks.Add (new int[] { -1, 0, 2, 0, 1, 2, 0, -1, -1});
				masks.Add (new int[] { -1, -1, 0, 2, 1, 0, 2, 0, -1});
				masks.Add (new int[] { -1, -1, -1, 0, 1, 2, 2, 0, -1});
				masks.Add (new int[] { -1, -1, -1, 0, 1, 2, 0, 2, 0});
				masks.Add (new int[] { -1, -1, -1, 0, 1, 0, 2, 2, 0});

		}

		void Update ()
		{
	
		}

		public bool IsFree (Gomoku.Map map, int x, int y)
		{
				if (map.GetColor (x, y) != Gomoku.Color.Empty)
						return false;
				return true;
		}
	
		public bool PutPawn (Gomoku.Map map, int x, int y, Gomoku.Color color, bool simulation = false)
		{
				if (!this.IsFree (map, x, y) || (simulation && this.DoubleThree && this.IsDoubleThree (map, x, y, color)))
						return false;

				map.PutPawn (x, y, color);
				this.UpdateMap (map, x, y);
				map.GeneratePossibleCells (x, y, 2);
				return true;
		}

		public bool UpdateMap (Gomoku.Map map, int x, int y)
		{
				Gomoku.Coord currentCell = new Gomoku.Coord ();
				currentCell.x = x;
				currentCell.y = y;
				Gomoku.Color color = map.GetColor (x, y);

				UpdateCellData (Orientation.EAST, currentCell, map);
				UpdateCellData (Orientation.NORTH, currentCell, map);
				UpdateCellData (Orientation.NORTHEAST, currentCell, map);
				UpdateCellData (Orientation.NORTHWEST, currentCell, map);

				SetIsTaking (map, currentCell, color);
			
				UpdateAround (map, currentCell);

				if (map.GetWeight (x, y, color) >= 5)
						map.SaveWeight (x, y, color);
				return true;
		}

		private void NoBreakeable (Gomoku.Map map, int x, int y, Gomoku.Orientation orientation)
		{
				Gomoku.Color color = map.GetColor (x, y);
				int originX = x;
				int originY = y;
				int it = 0;
				x += MapComponent.ORIENTATION [orientation] [0];
				y += MapComponent.ORIENTATION [orientation] [1];

				while (x >= 0 && x < Gomoku.Map.GetSizeMap () && 
			       y >= 0 && y < Gomoku.Map.GetSizeMap ()) {
						if (it == 0 && map.GetColor (x, y) != color)
								break;
						if (map.GetColor (x, y) == color)
								map.SetIsTakeable (x, y, false);
						else if (map.GetColor (x, y) != color) {
								map.SetIsTaking (x, y, MapComponent.inverseOrientation (orientation), false);
								map.SetIsTaking (originX, originY, orientation, false);
								break;
						}
						x += MapComponent.ORIENTATION [orientation] [0];
						y += MapComponent.ORIENTATION [orientation] [1];
				}
		}

		private void UpdateAround (Gomoku.Map map, Gomoku.Coord baseCell)
		{
				Gomoku.Color color = map.GetColor (baseCell.x, baseCell.y);
				Gomoku.Color enemy = (color == Gomoku.Color.White) ? Gomoku.Color.Black : Gomoku.Color.White;
				Gomoku.Coord currentCell = new Gomoku.Coord ();

				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						currentCell.x = baseCell.x + entry.Value [0];
						currentCell.y = baseCell.y + entry.Value [1];

						if (currentCell.x >= 0 && currentCell.x < Gomoku.Map.GetSizeMap () && 
								currentCell.y >= 0 && currentCell.y < Gomoku.Map.GetSizeMap () && 
								map.GetColor (currentCell.x, currentCell.y) == enemy) {
								SetIsTaking (map, currentCell, enemy);
						}
			
						currentCell.x = baseCell.x + 2 * entry.Value [0];
						currentCell.y = baseCell.y + 2 * entry.Value [1];
						
						if (currentCell.x >= 0 && currentCell.x < Gomoku.Map.GetSizeMap () && 
								currentCell.y >= 0 && currentCell.y < Gomoku.Map.GetSizeMap () && 
								map.GetColor (currentCell.x, currentCell.y) == enemy) {
								SetIsTaking (map, currentCell, enemy);
						}
			
						currentCell.x = baseCell.x + 3 * entry.Value [0];
						currentCell.y = baseCell.y + 3 * entry.Value [1];
						
						if (currentCell.x >= 0 && currentCell.x < Gomoku.Map.GetSizeMap () && 
								currentCell.y >= 0 && currentCell.y < Gomoku.Map.GetSizeMap () && 
								map.GetColor (currentCell.x, currentCell.y) == enemy) {
								SetIsTaking (map, currentCell, enemy);
						}

						
			
				}
		}

		private void SetIsTaking (Gomoku.Map map, Gomoku.Coord currentCell, Gomoku.Color color)
		{	
				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						IsTaking (map, entry.Key, currentCell, color);
				}
		}

		private bool IsTaking (Gomoku.Map map, Orientation orientation, Gomoku.Coord currentCell, Gomoku.Color color)
		{
				Gomoku.Color ennemy = (color == Gomoku.Color.White) ? Gomoku.Color.Black : Gomoku.Color.White;
				int wayX = MapComponent.ORIENTATION [orientation] [0];
				int wayY = MapComponent.ORIENTATION [orientation] [1];
				int x = currentCell.x + wayX;
				int y = currentCell.y + wayY;
				int nbEnnemy = 0;
		
				while (x >= 0 && y >= 0 &&
		       x < Gomoku.Map.GetSizeMap() && y < Gomoku.Map.GetSizeMap()) {
						if (map.GetColor (currentCell.x, currentCell.y) == color && map.GetColor (x, y) != ennemy && nbEnnemy == 2) {
								map.SetIsTaking (currentCell.x, currentCell.y, orientation, true);
								map.SetIsTakeable (x - wayX, y - wayY, true);
								map.SetIsTakeable (x - 2 * wayX, y - 2 * wayY, true);
								map.SetIsTaking (x, y, MapComponent.inverseOrientation (orientation), true);
								return true;
						}
						if (map.GetColor (x, y) != ennemy) {
								map.SetIsTaking (currentCell.x, currentCell.y, orientation, false);
								return false;
						}
						
						nbEnnemy++;
						x += wayX;
						y += wayY;
				}
				map.SetIsTaking (currentCell.x, currentCell.y, orientation, false);
				return false;
		}
	
		private void UpdateCellData (Gomoku.Orientation orientation, Gomoku.Coord currentCell, Gomoku.Map map)
		{
				Gomoku.Color color = map.GetColor (currentCell.x, currentCell.y);
				int x;
				int y;
				int weight = 1;
		
				x = currentCell.x + MapComponent.ORIENTATION [orientation] [0];
				y = currentCell.y + MapComponent.ORIENTATION [orientation] [1];
				while (x >= 0 && y >= 0 &&
		       x < Gomoku.Map.GetSizeMap() && y < Gomoku.Map.GetSizeMap() && map.GetColor(x, y) == color) {
						weight++;
						x += MapComponent.ORIENTATION [orientation] [0];
						y += MapComponent.ORIENTATION [orientation] [1];
				}

				x = currentCell.x - MapComponent.ORIENTATION [orientation] [0];
				y = currentCell.y - MapComponent.ORIENTATION [orientation] [1];
				while (x >= 0 && y >= 0 &&
		       x < Gomoku.Map.GetSizeMap() && y < Gomoku.Map.GetSizeMap() && map.GetColor(x, y) == color) {
						weight++;
						x -= MapComponent.ORIENTATION [orientation] [0];
						y -= MapComponent.ORIENTATION [orientation] [1];
				}


				x = currentCell.x + MapComponent.ORIENTATION [orientation] [0];
				y = currentCell.y + MapComponent.ORIENTATION [orientation] [1];
				while (x >= 0 && y >= 0 &&
		       x < Gomoku.Map.GetSizeMap() && y < Gomoku.Map.GetSizeMap() && map.GetColor(x, y) == color) {
						if (map.GetWeight (x, y, color) < weight)
								map.SetWeight (x, y, weight, color);
						x += MapComponent.ORIENTATION [orientation] [0];
						y += MapComponent.ORIENTATION [orientation] [1];
				}
				if (x < Gomoku.Map.GetSizeMap () && x >= 0 &&
						y < Gomoku.Map.GetSizeMap () && y >= 0 && 
						map.GetWeight (x, y, color) < weight)
						map.SetWeight (x, y, weight, color);


				x = currentCell.x - MapComponent.ORIENTATION [orientation] [0];
				y = currentCell.y - MapComponent.ORIENTATION [orientation] [1];
				while (x >= 0 && y >= 0 &&
		       x < Gomoku.Map.GetSizeMap() && y < Gomoku.Map.GetSizeMap() && map.GetColor(x, y) == color) {
						if (map.GetWeight (x, y, color) < weight)
								map.SetWeight (x, y, weight, color);
						x -= MapComponent.ORIENTATION [orientation] [0];
						y -= MapComponent.ORIENTATION [orientation] [1];
				}
				if (x < Gomoku.Map.GetSizeMap () && x >= 0 &&
						y < Gomoku.Map.GetSizeMap () && y >= 0 && 
						map.GetWeight (x, y, color) < weight)
						map.SetWeight (x, y, weight, color);

				if (map.GetWeight (currentCell.x, currentCell.y, color) < weight)
						map.SetWeight (currentCell.x, currentCell.y, weight, color);
		}

		public void Scoring (Gomoku.Color remover, int score, Gomoku.Map map)
		{
				map.scores [(int)remover] += score;
		}

		public bool CanTakePawns (Gomoku.Map map, int x, int y, Gomoku.Orientation orientation)
		{
				return (map.IsTaking (x, y, orientation) && 
						map.GetColor (x + 3 * MapComponent.ORIENTATION [orientation] [0], y + 3 * MapComponent.ORIENTATION [orientation] [1]) == map.GetColor (x, y));
		}

		public void TakePawns (Gomoku.Map map, int X, int Y, Gomoku.Orientation orientation)
		{
				Scoring ((Gomoku.Color)map.GetColor (X, Y), 2, map);
				map.SetIsTaking (X, Y, orientation, false);
				map.SetIsTaking (X + 3 * MapComponent.ORIENTATION [orientation] [0], Y + 3 * MapComponent.ORIENTATION [orientation] [1], orientation, false);
				map.RemovePawn (X + 1 * MapComponent.ORIENTATION [orientation] [0], Y + 1 * MapComponent.ORIENTATION [orientation] [1]);
				map.AddPossibleCell (X + MapComponent.ORIENTATION [orientation] [0], Y + MapComponent.ORIENTATION [orientation] [1]);
				map.RemovePawn (X + 2 * MapComponent.ORIENTATION [orientation] [0], Y + 2 * MapComponent.ORIENTATION [orientation] [1]);
				map.AddPossibleCell (X + 2 * MapComponent.ORIENTATION [orientation] [0], Y + 2 * MapComponent.ORIENTATION [orientation] [1]);
				
				UpdateTakeable (map, X, Y);
				UpdateTakeable (map, X + 3 * MapComponent.ORIENTATION [orientation] [0], Y + 3 * MapComponent.ORIENTATION [orientation] [1]);
		}

		public void UpdateTakeable (Gomoku.Map map, int X, int Y)
		{
				map.SetIsTakeable (X, Y, false);
				UpdateAround (map, new Gomoku.Coord () { x = X, y = Y });
				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						map.SetIsTakeable (X + entry.Value [0], Y + entry.Value [1], false);
						UpdateAround (map, new Gomoku.Coord () { x = X + entry.Value[0], y = Y + entry.Value[1]});
				}
		}

		public Gomoku.Color IsWinner (Gomoku.Map map, Gomoku.Color currentColor)
		{
				Gomoku.Color ennemy = (currentColor == Gomoku.Color.Black) ? Gomoku.Color.White : Gomoku.Color.Black;
				List<Coord> alreadyCheck = new List<Coord> ();

				foreach (Coord item in map.GetBigWeight(currentColor)) {
						if (!alreadyCheck.Contains (item)) {
								alreadyCheck.Add (item);
								if (!FiveBreakable || !CheckIsBreakable (map, item.x, item.y, alreadyCheck))
										return currentColor;
						}
				}

				foreach (Coord item in map.GetBigWeight(ennemy)) {
						if (!alreadyCheck.Contains (item)) {
								alreadyCheck.Add (item);
								if (!FiveBreakable || !CheckIsBreakable (map, item.x, item.y, alreadyCheck))
										return ennemy;
						}
				}
		
				return  Gomoku.Color.Empty;
		}

		private bool CheckIsBreakable (Gomoku.Map map, int x, int y, List<Coord> alreadyCheck)
		{
				bool right = IsBreakable (map, x, y, Gomoku.Orientation.EAST, alreadyCheck);
				bool up = IsBreakable (map, x, y, Gomoku.Orientation.NORTH, alreadyCheck);
				bool rightUp = IsBreakable (map, x, y, Gomoku.Orientation.NORTHEAST, alreadyCheck);
				bool leftUp = IsBreakable (map, x, y, Gomoku.Orientation.NORTHWEST, alreadyCheck);

				if (!right || !up || !rightUp || !leftUp)
						return false;
				return true;
		}

		private bool IsBreakable (Gomoku.Map map, int x, int y, Gomoku.Orientation orientation, List<Coord> alreadyCheck)
		{
				int countPawn = 0;
				Gomoku.Color color = map.GetColor (x, y);

				while (x >= 0 && y >= 0 &&
		              y < Gomoku.Map.GetSizeMap() && x < Gomoku.Map.GetSizeMap()
		              && map.GetColor(x, y) == color) {
						x -= MapComponent.ORIENTATION [orientation] [0];
						y -= MapComponent.ORIENTATION [orientation] [1];
				}

				if (!(x < Gomoku.Map.GetSizeMap () && x >= 0 &&
						y < Gomoku.Map.GetSizeMap () && y >= 0) || map.GetColor (x, y) != color) {
						x += MapComponent.ORIENTATION [orientation] [0];
						y += MapComponent.ORIENTATION [orientation] [1];		
				}
				
				while (x >= 0 && y >= 0 &&
		       		y < Gomoku.Map.GetSizeMap() && x < Gomoku.Map.GetSizeMap()
		      	 	&& map.GetColor(x, y) == color && countPawn < 5) {
						
						if (IsTakeable (map, x, y, orientation))
								countPawn = 0;
						else {
								map.SetIsTakeable (x, y, false);
								countPawn++;
						}		
						alreadyCheck.Add (new Coord () {x = x, y = y});
						x += MapComponent.ORIENTATION [orientation] [0];
						y += MapComponent.ORIENTATION [orientation] [1];
				}
				if (countPawn < 5)
						return true;
				return false;
		}

		private bool IsTakeable (Gomoku.Map map, int x, int y, Gomoku.Orientation orientation)
		{	
				Gomoku.Color ennemy = (map.GetColor (x, y) == Gomoku.Color.Black) ? Gomoku.Color.White : Gomoku.Color.Black;

				foreach (KeyValuePair<Gomoku.Orientation, int[]> entry in MapComponent.ORIENTATION) {
						if (entry.Key != orientation && entry.Key != MapComponent.inverseOrientation (orientation)) {
								if (x - entry.Value [0] >= 0 && y - entry.Value [1] >= 0 && 
										y - entry.Value [0] < Gomoku.Map.GetSizeMap () && y - entry.Value [1] < Gomoku.Map.GetSizeMap () && 
										map.GetColor (x - entry.Value [0], y - entry.Value [1]) == ennemy) {
										if (_Takeable (map, x - entry.Value [0], y - entry.Value [1], entry.Value [0], entry.Value [1]))
												return true;
								} else if (x - 2 * entry.Value [0] >= 0 && y - 2 * entry.Value [1] >= 0 && 
										y - 2 * entry.Value [0] < Gomoku.Map.GetSizeMap () && y - 2 * entry.Value [1] < Gomoku.Map.GetSizeMap () && 
										map.GetColor (x - 2 * entry.Value [0], y - 2 * entry.Value [1]) == ennemy) {
										if (_Takeable (map, x - 2 * entry.Value [0], y - 2 * entry.Value [1], entry.Value [0], entry.Value [1]))
												return true;
								}
						}
				}
				return false;
		}

		private bool _Takeable (Gomoku.Map map, int x, int y, int wayX, int wayY)
		{
				int i = 1;
				Gomoku.Color color = (map.GetColor (x, y) == Gomoku.Color.Black) ? Gomoku.Color.White : Gomoku.Color.Black;

				while (i <= 3 && x + i * wayX >= 0 && y + i * wayY >= 0 &&
		       x + i * wayX < Gomoku.Map.GetSizeMap() && y + i * wayY < Gomoku.Map.GetSizeMap()) {
						if (i == 3 && map.GetColor (x + i * wayX, y + i * wayY) == Gomoku.Color.Empty) {
								return true;
						}	
						if (i < 3 && map.GetColor (x + i * wayX, y + i * wayY) != color)
								return false;
						i++;
				}

				return false;
		}

		private Gomoku.Color WeightToFive (int x, int y, Gomoku.Map map)
		{
				if (map.GetColor (x, y) == Gomoku.Color.Black && map.GetWeight (x, y, Gomoku.Color.Black) >= 5) 
						return Gomoku.Color.Black;
				else if (map.GetColor (x, y) == Gomoku.Color.White && map.GetWeight (x, y, Gomoku.Color.White) >= 5)
						return Gomoku.Color.White;
				return Gomoku.Color.Empty;
		}

		public Gomoku.Color IsScoringWinner (Gomoku.Map map)
		{
				if (map.scores [(int)Gomoku.Color.White] == Gomoku.Map.MAX_SCORE)
						return Gomoku.Color.White;
				if (map.scores [(int)Gomoku.Color.Black] == Gomoku.Map.MAX_SCORE)
						return Gomoku.Color.Black;
				return Gomoku.Color.Empty;
		}

		private int GetColorMod (Gomoku.Map map, int x, int y, Gomoku.Color color)
		{
		
				if (!(x >= 0 && y >= 0 && x < Gomoku.Map.GetSizeMap () && y < Gomoku.Map.GetSizeMap ()))
						return -1;
				if (map.GetColor (x, y) == color)
						return 2;
				else if (map.GetColor (x, y) == Gomoku.Color.Empty)
						return 0;
				else
						return -1;
		}
	
		private bool TestMask (Gomoku.Map map, int[] mask, Gomoku.Color color, Gomoku.Coord cell, Gomoku.Orientation orientation)
		{
		
				if (mask [0] != -1 && GetColorMod (map, cell.x + -4 * MapComponent.ORIENTATION [orientation] [0], cell.y + -4 * MapComponent.ORIENTATION [orientation] [1], color) != mask [0])
						return false;
				if (mask [1] != -1 && GetColorMod (map, cell.x + -3 * MapComponent.ORIENTATION [orientation] [0], cell.y + -3 * MapComponent.ORIENTATION [orientation] [1], color) != mask [1])
						return false;
				if (mask [2] != -1 && GetColorMod (map, cell.x + -2 * MapComponent.ORIENTATION [orientation] [0], cell.y + -2 * MapComponent.ORIENTATION [orientation] [1], color) != mask [2])
						return false;
				if (mask [3] != -1 && GetColorMod (map, cell.x + -1 * MapComponent.ORIENTATION [orientation] [0], cell.y + -1 * MapComponent.ORIENTATION [orientation] [1], color) != mask [3])
						return false;
				if (mask [5] != -1 && GetColorMod (map, cell.x + 1 * MapComponent.ORIENTATION [orientation] [0], cell.y + 1 * MapComponent.ORIENTATION [orientation] [1], color) != mask [5])
						return false;
				if (mask [6] != -1 && GetColorMod (map, cell.x + 2 * MapComponent.ORIENTATION [orientation] [0], cell.y + 2 * MapComponent.ORIENTATION [orientation] [1], color) != mask [6])
						return false;
				if (mask [7] != -1 && GetColorMod (map, cell.x + 3 * MapComponent.ORIENTATION [orientation] [0], cell.y + 3 * MapComponent.ORIENTATION [orientation] [1], color) != mask [7])
						return false;
				if (mask [8] != -1 && GetColorMod (map, cell.x + 4 * MapComponent.ORIENTATION [orientation] [0], cell.y + 4 * MapComponent.ORIENTATION [orientation] [1], color) != mask [8])
						return false;
				return true;
		}
	
		public bool FindDoubleThree (Gomoku.Map map, Gomoku.Coord currentCell, Gomoku.Color color)
		{
				int j;
				bool findMask = false;
				bool breako = false;
				List<int []> threeFree = new List<int[]> ();
				List<int []> threeFreeNext = new List<int[]> ();

		
				foreach (int[] mask in this.masks) {
						foreach (KeyValuePair<Gomoku.Orientation, int[]> orientation in MapComponent.ORIENTATION) {
								if (TestMask (map, mask, color, currentCell, orientation.Key)) {
										findMask = true;
										for (j=0; j < mask.Length; j++) {
												if (mask [j] == 2) {
														threeFree.Add (new int[] {
								currentCell.x + (j - 4) * MapComponent.ORIENTATION [orientation.Key] [0],
								currentCell.y + (j - 4) * MapComponent.ORIENTATION [orientation.Key] [1]
							});
												}
												if (mask [j] == 1) {
														threeFree.Add (new int[] { currentCell.x, currentCell.y });
												}
										}
										breako = true;
										break;
								}
								threeFree.Clear ();
						}
						if (breako)
								break;
						threeFree.Clear ();
				}
				if (!findMask) 
						return false;
		
				foreach (int [] cell in threeFree) {
						foreach (int[] mask in this.masks) {
								foreach (KeyValuePair<Gomoku.Orientation, int[]> orientation in MapComponent.ORIENTATION) {
										Gomoku.Coord tmpCoord = new Gomoku.Coord ();
										tmpCoord.x = cell [0];
										tmpCoord.y = cell [1];

										if (TestMask (map, mask, color, tmpCoord, orientation.Key)) {
												for (j=0; j < mask.Length; j++) {
														if (mask [j] == 2) {
																threeFreeNext.Add (new int[] {
									tmpCoord.x + (j - 4) * MapComponent.ORIENTATION [orientation.Key] [0],
									tmpCoord.y + (j - 4) * MapComponent.ORIENTATION [orientation.Key] [1]
								});
														}
														if (mask [j] == 1) {
																threeFreeNext.Add (new int[] { tmpCoord.x, tmpCoord.y });
														}
												}
												if (TestPrecThree (threeFree, threeFreeNext)) {
														return true;
												}
										}
										threeFreeNext.Clear ();
								}
								threeFreeNext.Clear ();
						}
						threeFreeNext.Clear ();
				}
		
				return false;
		}
	
		public bool TestPrecThree (List<int[]> a, List<int[]> b)
		{
				for (int j=0; j < a.Count; j++) {
						if (!((a [j] [0] == b [j] [0] && a [j] [1] == b [j] [1]) || (a [j] [0] == b [a.Count - j - 1] [0] && a [j] [1] == b [a.Count - j - 1] [1])))
								return true;
				}
				return false;
		}
	
		public bool IsDoubleThree (Gomoku.Map map, int x, int y, Gomoku.Color color)
		{
				map.PutPawn (x, y, color);
				Gomoku.Coord cell = new Gomoku.Coord ();
				cell.x = x;
				cell.y = y;

				bool ret = FindDoubleThree (map, cell, color);
				map.RemovePawn (x, y);
		
				return ret;
		}
}
