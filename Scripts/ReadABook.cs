using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;

namespace ReadABook
{
	public class ReadABookMod : MonoBehaviour, IHasModSaveData
	{
		public static int OpenCheck;
		public static string BookName;

		static float CurStatF1;
		static float CurStatF2;
		static int CheckDays;
		static int CurSkill;
		static int CurStat1;
		static int CurStat2;
		static int IndexM;
		static int IndexR;
		static int IndexU;
		static int LevelledUp;
		static int NewDays;
		static int OldDays;
		static int Rando;
		static int ReadCheck;
		static Mod RABMod;
		static ModSettings Settings;
		static PlayerEntity player;
		static ReadABookMod ModInstance;
		static string BookStatus;
		static string LiteracyKey;
		static string UhOh;
		static string Whoopie;

		// - Save Variables ----------------------------------------------------
		static int LiteracySkill;
		static float[] SkillsCeiling;
		static float[] SkillsForget;
		static float[] SkillsLearn;
		static List<string> BooksRead;
		// ---------------------------------------------------------------------

		[Invoke(StateManager.StateTypes.Start, 0)]
		public static void Init(InitParams initParams)
		{
			RABMod = initParams.Mod;
			var go = new GameObject(RABMod.Title);
			ModInstance = go.AddComponent<ReadABookMod>();

			LiteracySkill = Settings.GetValue<int>("General", "LitInit");
			SkillsCeiling = new float[35];
			SkillsForget = new float[35];
			SkillsLearn = new float[35];
			BooksRead = new List<string>();
			for (IndexM = 0; IndexM < 256; IndexM++)
			{
				BooksRead.Add("");
			}

			RABMod.SaveDataInterface = ModInstance;
		}

		void Awake()
		{
			InitMod();
			RABMod.IsReady = true;
		}

		void Update()
		{
			if (GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Game)
			{
				if (CheckDays == 1)
				{
					DaggerfallDateTime BookTime = DaggerfallUnity.Instance.WorldTime.Now;
					NewDays = BookTime.DayOfYear;
					if (OldDays != NewDays)
					{
						for (IndexU = 0; IndexU < 35; IndexU++)
						{
							if (SkillsForget[IndexU] > (float)0)
							{
								SkillsForget[IndexU] -= (float)1;
							}
						}

						OldDays = NewDays;
					}

					CheckDays = 0;
				}

				if (OpenCheck == 1)
				{
					if (!GameManager.Instance.AreEnemiesNearby(false, false))
					{
						for (IndexR = 0; IndexR < 256; IndexR++)
						{
							if (BooksRead[IndexR] == String.Empty)
							{
								BooksRead[IndexR] = BookName;
								ReadCheck = 1;

								IndexR = 256;
							}
						}

						//ReadCheck = 1;
					}
					else
					{
						UhOh = "You cannot read a book while enemies are nearby";
						DaggerfallMessageBox RestMessage = new DaggerfallMessageBox(DaggerfallUI.UIManager, null, true, -1);
						RestMessage.SetText(UhOh, null);
						RestMessage.ClickAnywhereToClose = true;
						RestMessage.AllowCancel = false;
						RestMessage.Show();
					}

					OpenCheck = 0;
				}

				if (ReadCheck == 1)
				{
					for (IndexR = 0; IndexR < 256; IndexR++)
					{
						if (BooksRead[IndexR] != String.Empty)
						{
							BookName = BooksRead[IndexR];
							BookStart(BookName);

							BooksRead[IndexR] = String.Empty;
						}
					}

					//System.Random Fortuna = new System.Random();
					//PlayerEntity player = GameManager.Instance.PlayerEntity;
					//Rando = Fortuna.Next(0, 35);
					//BookProcess(Rando, player.Stats.LiveWillpower, "alteration");

					ReadCheck = 0;
				}

				if (Input.GetKeyUp(KeyCode.K))
				{
					DaggerfallUI.AddHUDText(string.Format("Your literacy skill is {0}%", LiteracySkill));
				}
			}
			else if (GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.UI)
			{
				if (CheckDays == 0)
				{
					DaggerfallDateTime BookTime = DaggerfallUnity.Instance.WorldTime.Now;
					OldDays = BookTime.DayOfYear;

					CheckDays = 1;
				}
			}
		}

		public static void InitMod()
		{
			Debug.Log("Begin mod init: Read A Book");

			Settings = RABMod.GetSettings();
			LiteracyKey = Settings.GetValue<string>("General", "LitKey");

			player = GameManager.Instance.PlayerEntity;

			UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(ReadABookWindow));

			Debug.Log("Finished mod init: Read A Book");
		}

		public static void BookStart(string name)
		{
			switch (name[0])
			{
				case 'A':
					switch (name[1])
					{
						case ' ':
							switch (name[2])
							{
								case 'D':									// A Dubious Tale of the Crystal Tower
									BookProcess(3, player.Stats.LiveStrength, "jumping");
									break;
								case 'H':									// A History of Daggerfall
									BookProcess(18, player.Stats.LiveStrength, "climbing");
									break;
								case 'S':									// A Scholar's Guide to Nymphs
									BookProcess(8, player.Stats.LiveIntelligence, "nymph");
									break;
								case 'T':									// A Tale of Kieran
									BookProcess(2, player.Stats.LivePersonality, "streetwise");
									break;
								default:
									break;
							}
							break;
						case 'n':											// An Overview of Gods And Worship
							BookProcess(9, player.Stats.LiveIntelligence, "daedric");
							break;
						case 'r':											// Ark'ay, der Gott & Ark'ay, the God of Birth and Death
							BookProcess(25, player.Stats.LiveWillpower, "alteration");
							break;
						default:
							break;
					}
					break;
				case 'B':
					switch (name[1])
					{
						case 'a':											// Banker's Bet
							BookProcess(14, player.Stats.LivePersonality, "mercantile");
							break;
						case 'i':
							if (name.Length == 28)                          // Biography of Queen Barenziah
							{
								BookProcess(2, player.Stats.LivePersonality, "streetwise");
								return;
							}

							switch (name[34])
							{
								case '1':									// Biography of Queen Barenziah, Vol 1
									BookProcess(2, player.Stats.LivePersonality, "streetwise");
									break;
								case '2':									// Biography of Queen Barenziah, Vol 2
									BookProcess(15, player.Stats.LiveAgility, "pickpocket");
									break;
								case '3':									// Biography of Queen Barenziah, Vol 3
									BookProcess(26, player.Stats.LiveWillpower, "thaumaturgy");
									break;
								default:
									break;
							}

							break;
						case 'r':
							switch (name[3])
							{
								case 'i':
									if (name.Length == 27)                  // Brief History of the Empire
									{
										BookProcess(20, player.Stats.LiveSpeed, "dodging");
										return;
									}

									switch (name[34])
									{
										case '1':							// Brief History of the Empire, Part 1
											BookProcess(20, player.Stats.LiveSpeed, "dodging");
											break;
										case '2':							// Brief History of the Empire, Part 2
											BookProcess(34, player.Stats.LiveAgility, "critical strike");
											break;
										case '3':							// Brief History of the Empire, Part 3
											BookProcess(31, player.Stats.LiveStrength, "axe");
											break;
										case '4':							// Brief History of the Empire, Part 4
											BookProcess(16, player.Stats.LiveAgility, "stealth");
											break;
										default:
											break;
									}

									break;
								case 'o':									// Broken Diamonds
									BookProcess(19, player.Stats.LiveAgility, "backstabbing");
									break;
								default:
									break;
							}
							break;
						default:
							break;
					}
					break;
				case 'C':													// Confessions Of A Thief
					BookProcess(13, player.Stats.LiveIntelligence, "lockpicking");
					break;
				case 'D':													// Divad the Singer
					BookProcess(30, player.Stats.LiveAgility, "hand-to-hand");
					break;
				case 'E':													// Etiquette With Rulers
					BookProcess(1, player.Stats.LivePersonality, "etiquette");
					break;
				case 'F':
					switch (name[1])
					{
						case 'a':											// Fav'te's War Of Betony
							BookProcess(33, player.Stats.LiveAgility, "archery");
							break;
						case 'o':
							if (name.Length == 12)                          // Fools' Ebony
							{
								BookProcess(0, player.Stats.LiveIntelligence, "medical");
								return;
							}

							switch (name[23])
							{
								case 'O':									// Fools' Ebony, Part the Oneth
									BookProcess(0, player.Stats.LiveIntelligence, "medical");
									break;
								case 'T':
									switch (name[24])
									{
										case 'w':							// Fools' Ebony, Part the Twoeth
											BookProcess(25, player.Stats.LiveWillpower, "alteration");
											break;
										case 'h':							// Fools' Ebony, Part the Threeth
											BookProcess(27, player.Stats.LiveWillpower, "mysticism");
											break;
										default:
											break;
									}
									break;
								case 'F':
									switch (name[24])
									{
										case 'o':							// Fools' Ebony, Part the Fourth
											BookProcess(14, player.Stats.LivePersonality, "mercantile");
											break;
										case 'i':							// Fools' Ebony, Part the Fiveth
											BookProcess(16, player.Stats.LiveAgility, "stealth");
											break;
										default:
											break;
									}
									break;
								case 'S':									// Fools' Ebony, Part the Sixth
									BookProcess(2, player.Stats.LivePersonality, "streetwise");
									break;
								default:
									break;
							}

							break;
						case 'r':
							switch (name[2])
							{
								case 'a':									// Fragment: On Artaeum
									BookProcess(24, player.Stats.LiveWillpower, "illusion");
									break;
								case 'o':									// From The Memory Stone of Makela Leki
									BookProcess(28, player.Stats.LiveAgility, "short blade");
									break;
								default:
									break;
							}
							break;
						default:
							break;
					}
					break;
				case 'G':
					switch (name[1])
					{
						case 'a':											// Galerion the Mystic
							BookProcess(23, player.Stats.LiveWillpower, "restoration");
							break;
						case 'h':											// Ghraewaj
							BookProcess(5, player.Stats.LiveIntelligence, "harpy");
							break;
						default:
							break;
					}
					break;
				case 'H':													// Holidays of the Iliac Bay
					BookProcess(21, player.Stats.LiveSpeed, "running");
					break;
				case 'I':
					switch (name[1])
					{
						case 'n':											// Invocation of Azura
							BookProcess(9, player.Stats.LiveIntelligence, "daedric");
							break;
						case 'u':											// Ius, Animal God
							BookProcess(20, player.Stats.LiveSpeed, "dodging");
							break;
						default:
							break;
					}
					break;
				case 'J':													// Jokes
					BookProcess(2, player.Stats.LivePersonality, "streetwise");
					break;
				case 'K':
					if (name.Length == 11)                                  // King Edward
					{
						BookProcess(1, player.Stats.LivePersonality, "etiquette");
						return;
					}

					switch (name[18])
					{
						case 'I':											// King Edward, Part I
							BookProcess(1, player.Stats.LivePersonality, "etiquette");// King Edward, Part II
							break;											// King Edward, Part III
																			// King Edward, Part IV
																			// King Edward, Part IX
						case 'V':											// King Edward, Part V
							BookProcess(1, player.Stats.LivePersonality, "etiquette");// King Edward, Part VI
							break;											// King Edward, Part VII
																			// King Edward, Part VIII
						case 'X':											// King Edward, Part X
							BookProcess(1, player.Stats.LivePersonality, "etiquette");// King Edward, Part XI
							break;											// King Edward, Part XII
						default:
							break;
					}

					break;
				case 'L':
					BookProcess(1, player.Stats.LivePersonality, "etiquette");// Legal Basics
					break;
				case 'M':
					switch (name[1])
					{
						case 'a':											// Mara's Tear
							BookProcess(4, player.Stats.LiveIntelligence, "orcish");
							break;
						case 'y':											// Mysticism - Unfathomable Voyage
							BookProcess(27, player.Stats.LiveWillpower, "mysticism");
							break;
						default:
							break;
					}
					break;
				case 'N':
					switch (name[1])
					{
						case 'e':											// Newgate's War Of Betony
							BookProcess(29, player.Stats.LiveAgility, "long blade");
							break;
						case 'o':											// Notes For Redguard History
							BookProcess(28, player.Stats.LiveAgility, "short blade");
							break;
						default:
							break;
					}
					break;
				case 'O':
					switch (name[1])
					{
						case 'e':											// Oelander's Hammer
							BookProcess(32, player.Stats.LiveStrength, "blunt weapon");
							break;
						case 'f':											// Of Jephre
							BookProcess(10, player.Stats.LiveIntelligence, "spriggan");
							break;
						case 'n':
							switch (name[3])
							{
								case 'L':									// On Lycanthropy
									BookProcess(25, player.Stats.LiveWillpower, "alteration");
									break;
								case 'O':									// On Oblivion
									BookProcess(9, player.Stats.LiveIntelligence, "daedric");
									break;
								default:
									break;
							}
							break;
						default:
							break;
					}
					break;
				case 'P':
					break;
				case 'Q':
					break;
				case 'R':
					switch (name[1])
					{
						case 'e':											// Redguards, Their History and Their Heroes
							BookProcess(28, player.Stats.LiveAgility, "short blade");
							break;
						case 'u':											// Rude Song
							BookProcess(2, player.Stats.LivePersonality, "streetwise");
							break;
						default:
							break;
					}
					break;
				case 'S':													// Special Flora of Tamriel
					BookProcess(0, player.Stats.LiveIntelligence, "medical");
					break;
				case 'T':
					switch (name[4])
					{
						case 'A':
							switch (name[5])
							{
								case 'l':									// The Alik'r
									BookProcess(18, player.Stats.LiveStrength, "climbing");
									break;
								case 'r':									// The Arrowshot Woman
									BookProcess(33, player.Stats.LiveAgility, "archery");
									break;
								case 's':									// The Asylum Ball
									BookProcess(1, player.Stats.LivePersonality, "etiquette");
									break;
								default:
									break;
							}
							break;
						case 'B':											// The Brothers of Darkness
							BookProcess(19, player.Stats.LiveAgility, "backstabbing");
							break;
						case 'E':
							switch (name[5])
							{
								case 'b':									// The Ebon Arm
									BookProcess(9, player.Stats.LiveIntelligence, "daedric");
									break;
								case 'p':									// The Epic of the Grey Falcon
									BookProcess(17, player.Stats.LiveEndurance, "swimming");
									break;
								default:
									break;
							}
							break;
						case 'F':
							switch (name[5])
							{
								case 'a':
									switch (name[6])
									{
										case 'l':							// The Fall of the Usurper
											BookProcess(9, player.Stats.LiveIntelligence, "daedric");
											break;
										case 'e':							// The Faerie
											BookProcess(27, player.Stats.LiveWillpower, "mysticism");
											break;
										default:
											break;
									}
									break;
								case 'i':									// The First Scroll of Baan Dar
									BookProcess(15, player.Stats.LiveAgility, "pickpocket");
									break;
								default:
									break;
							}
							break;
						case 'H':											// The Healer's Tale
							BookProcess(23, player.Stats.LiveWillpower, "restoration");
							break;
						case 'L':
							switch (name[5])
							{
								case 'e':									// The Legend of Lover's Lament
									BookProcess(2, player.Stats.LivePersonality, "streetwise");
									break;
								case 'i':									// The Light and the Dark
									BookProcess(22, player.Stats.LiveWillpower, "destruction");
									break;
								default:
									break;
							}
							break;
						case 'M':											// The Madness of Pelagius
							BookProcess(34, player.Stats.LiveAgility, "critical strike");
							break;
						case 'O':
							switch (name[5])
							{
								case 'l':									// The Old Ways
									BookProcess(24, player.Stats.LiveWillpower, "illusion");
									break;
								case 'r':									// The Origin of the Mages Guild
									BookProcess(27, player.Stats.LiveWillpower, "mysticism");
									break;
								default:
									break;
							}
							break;
						case 'P':											// The Pig Children
							BookProcess(4, player.Stats.LiveIntelligence, "orcish");
							break;
						case 'R':
							if (name.Length == 18)                          // The Real Barenziah
							{
								BookProcess(1, player.Stats.LivePersonality, "etiquette");
								return;
							}

							switch (name[25])
							{
								case 'I':									// The Real Barenziah, Part I
									BookProcess(1, player.Stats.LivePersonality, "etiquette");// The Real Barenziah, Part II
									break;									// The Real Barenziah, Part III
																			// The Real Barenziah, Part IV
																			// The Real Barenziah, Part IX
								case 'V':									// The Real Barenziah, Part V
									BookProcess(1, player.Stats.LivePersonality, "etiquette");// The Real Barenziah, Part VI
									break;									// The Real Barenziah, Part VII
																			// The Real Barenziah, Part VIII
								case 'X':									// The Real Barenziah, Part X
									BookProcess(1, player.Stats.LivePersonality, "etiquette");
									break;
								default:
									break;
							}

							break;
						case 'S':
							switch (name[5])
							{
								case 'a':									// The Sage
									BookProcess(23, player.Stats.LiveWillpower, "restoration");
									break;
								case 't':									// The Story of Lyrisius
									BookProcess(7, player.Stats.LiveIntelligence, "dragonish");
									break;
								default:
									break;
							}
							break;
						case 'W':											// The Wild Elves
							BookProcess(1, player.Stats.LivePersonality, "etiquette");
							break;
						default:
							break;
					}
					break;
				case 'U':
					break;
				case 'V':													// Vampires of the Iliac Bay, Chapter I
					BookProcess(2, player.Stats.LivePersonality, "streetwise");// Vampires of the Iliac Bay, Chapter II
					break;
				case 'W':
					switch (name[2])
					{
						case 'b':											// Wabbajack
							BookProcess(9, player.Stats.LiveIntelligence, "daedric");
							break;
						case 'y':											// Wayrest, Jewel Of The Bay
							BookProcess(2, player.Stats.LivePersonality, "streetwise");
							break;
						default:
							break;
					}
					break;
				case 'X':
					break;
				case 'Y':
					break;
				default:
					break;
			}
		}

		public static void BookProcess(int Index, int Stat, string Skill)
		{
			//If you uncomment these, make sure to fix the index values
			//switch (Index)
			//{
			//	case 0:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "alteration";
			//		break;
			//	case 1:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "archery";
			//		break;
			//	case 2:
			//		Stat = player.Stats.LiveStrength;
			//		Skill = "axe";
			//		break;
			//	case 3:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "backstabbing";
			//		break;
			//	case 4:
			//		Stat = player.Stats.LiveStrength;
			//		Skill = "blunt weapon";
			//		break;
			//	case 5:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "centaurian";
			//		break;
			//	case 6:
			//		Stat = player.Stats.LiveStrength;
			//		Skill = "climbing";
			//		break;
			//	case 7:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "critical strike";
			//		break;
			//	case 8:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "daedric";
			//		break;
			//	case 9:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "destruction";
			//		break;
			//	case 10:
			//		Stat = player.Stats.LiveSpeed;
			//		Skill = "dodging";
			//		break;
			//	case 11:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "dragonish";
			//		break;
			//	case 12:
			//		Stat = player.Stats.LivePersonality;
			//		Skill = "etiquette";
			//		break;
			//	case 13:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "giantish";
			//		break;
			//	case 14:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "hand-to-hand";
			//		break;
			//	case 15:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "harpy";
			//		break;
			//	case 16:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "illusion";
			//		break;
			//	case 17:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "impish";
			//		break;
			//	case 18:
			//		Stat = player.Stats.LiveStrength;
			//		Skill = "jumping";
			//		break;
			//	case 19:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "lockpicking";
			//		break;
			//	case 20:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "long blade";
			//		break;
			//	case 21:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "medical";
			//		break;
			//	case 22:
			//		Stat = player.Stats.LivePersonality;
			//		Skill = "mercantile";
			//		break;
			//	case 23:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "mysticism";
			//		break;
			//	case 24:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "nymph";
			//		break;
			//	case 25:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "orcish";
			//		break;
			//	case 26:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "pickpocket";
			//		break;
			//	case 27:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "restoration";
			//		break;
			//	case 28:
			//		Stat = player.Stats.LiveSpeed;
			//		Skill = "running";
			//		break;
			//	case 29:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "short blade";
			//		break;
			//	case 30:
			//		Stat = player.Stats.LiveIntelligence;
			//		Skill = "spriggan";
			//		break;
			//	case 31:
			//		Stat = player.Stats.LiveAgility;
			//		Skill = "stealth";
			//		break;
			//	case 32:
			//		Stat = player.Stats.LivePersonality;
			//		Skill = "streetwise";
			//		break;
			//	case 33:
			//		Stat = player.Stats.LiveEndurance;
			//		Skill = "swimming";
			//		break;
			//	case 34:
			//		Stat = player.Stats.LiveWillpower;
			//		Skill = "thaumaturgy";
			//		break;
			//	default:
			//		break;
			//}

			CurSkill = player.Skills.GetLiveSkillValue(Index);

			CurStat1 = player.Stats.LiveIntelligence;
			CurStat2 = Stat;
			CurStat1 += CurStat2;
			CurStatF1 = (float)Convert.ChangeType(CurStat1, typeof(float));
			CurStatF1 /= 2;
			SkillsCeiling[Index] = CurStatF1;

			if (CurSkill < CurStat1)
			{
				CurStat1 = player.Stats.LiveWillpower;
				CurStat2 = Stat;
				CurStat1 += CurStat2;
				CurStat1 *= 3;
				CurStatF1 = (float)Convert.ChangeType(CurStat1, typeof(float));
				CurStatF1 /= 4;
				SkillsForget[Index] = CurStatF1;

				CurStat1 = player.Stats.LiveLuck;
				CurStat2 = Stat;
				CurStat1 += CurStat2;
				CurStatF1 = (float)Convert.ChangeType(CurStat1, typeof(float));
				CurStatF1 /= 4;
				SkillsLearn[Index] = CurStatF1;

				if (SkillsLearn[Index] < SkillsForget[Index])
				{
					DaggerfallDateTime BookTime = DaggerfallUnity.Instance.WorldTime.Now;

					CurStatF1 = SkillsLearn[Index];
					CurStatF1 = (50 - CurStatF1) + 1;
					CurStatF1 /= 10;
					CurStatF2 = CurStatF1;
					CurStatF1 *= 3600;
					BookTime.RaiseTime(CurStatF1);

					if (CurStatF2 > 24)
					{
						CurStatF1 /= 86400;
						BookStatus = string.Format("{0} days have passed. ", CurStatF1);
					}
					else if (CurStatF2 == 24)
					{
						BookStatus = "1 day has passed. ";
					}
					else if (CurStatF2 < 1)
					{
						CurStatF2 *= 60;
						if (CurStatF2 < 1)
						{
							CurStatF2 *= 60;
							BookStatus = string.Format("{0} seconds have passed. ", CurStatF2);
						}
						else
						{
							BookStatus = string.Format("{0} minutes have passed. ", CurStatF2);
						}
					}
					else
					{
						BookStatus = string.Format("{0} hours have passed. ", CurStatF2);
					}

					System.Random Fortuna = new System.Random();
					Rando = Fortuna.Next(0, 101);

					CurStat1 = (int)Convert.ChangeType(CurStatF2, typeof(int));
					CurStat1 = 5 - CurStat1;

					if (Rando <= CurStat1)
					{
						player.TallySkill((DFCareer.Skills)Index, 1);
						LevelledUp = 1;
					}
					else
					{
						BookStatus += string.Format("You are confused by what you read about {0}.", Skill);
					}
				}
				else
				{
					BookStatus += string.Format("You need more time to reflect on {0}.", Skill);
				}

				if (LevelledUp == 1)
				{
					LiteracySkill += 1;

					if (LiteracySkill < 100)
					{
						DaggerfallUI.AddHUDText(string.Format("Your literacy skill increased"));
					}
					else
					{
						DaggerfallAudioSource BookSound = QuestMachine.Instance.GetComponent<DaggerfallAudioSource>();

						if (BookSound != null && !BookSound.IsPlaying())
						{
							BookSound.PlayOneShot(32, 0, DaggerfallUnity.Settings.SoundVolume);
						}

						Whoopie = "Congratulations! " +
						"At each twist and turn of your life, you would always " +
						"still find a way to bury your nose into a nice, thick " +
						"book. You were absolutely determined to acquire as much " +
						"knowledge as possible. And now, thanks to your efforts, " +
						"you are well-read and educated beyond measure.";
						DaggerfallMessageBox BookMessage = new DaggerfallMessageBox(DaggerfallUI.UIManager, null, true, -1);
						BookMessage.SetText(Whoopie, null);
						BookMessage.ClickAnywhereToClose = true;
						BookMessage.AllowCancel = false;
						BookMessage.Show();
					}

					BookStatus += string.Format("You learned something new about {0}.", Skill);
					LevelledUp = 0;
				}
			}
			else
			{
				BookStatus = string.Format("You did not learn anything new about {0}.", Skill);
			}

			DaggerfallMessageBox StatusMessage = new DaggerfallMessageBox(DaggerfallUI.UIManager, null, true, -1);
			StatusMessage.SetText(BookStatus, null);
			StatusMessage.ClickAnywhereToClose = true;
			StatusMessage.AllowCancel = false;
			StatusMessage.Show();
		}

		public Type SaveDataType
		{
			get { return typeof(BookData); }
		}

		public object NewSaveData()
		{
			return new BookData
			{
				literacySkill = LiteracySkill,
				skillsCeiling = SkillsCeiling,
				skillsForget = SkillsForget,
				skillsLearn = SkillsLearn,
				booksRead = BooksRead
			};
		}

		public object GetSaveData()
		{
			return new BookData
			{
				literacySkill = LiteracySkill,
				skillsCeiling = SkillsCeiling,
				skillsForget = SkillsForget,
				skillsLearn = SkillsLearn,
				booksRead = BooksRead
			};
		}

		public void RestoreSaveData(object saveData)
		{
			var myModSaveData = (BookData)saveData;

			LiteracySkill = myModSaveData.literacySkill;
			SkillsCeiling = myModSaveData.skillsCeiling;
			SkillsForget = myModSaveData.skillsForget;
			SkillsLearn = myModSaveData.skillsLearn;
			BooksRead = myModSaveData.booksRead;
		}
	}

	[FullSerializer.fsObject("v1")]
	public class BookData
	{
		public int literacySkill { get; set; }
		public float[] skillsCeiling { get; set; }
		public float[] skillsForget { get; set; }
		public float[] skillsLearn { get; set; }
		public List<string> booksRead { get; set; }
	}
}

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
	public partial class ReadABookWindow : DaggerfallInventoryWindow
	{
		public ReadABookWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
			: base(uiManager, previous){}

		protected override void LocalItemListScroller_OnItemClick(DaggerfallUnityItem item, ActionModes actionMode)
		{
			// Handle click based on action
			if (actionMode == ActionModes.Equip)
			{
				if (item.IsLightSource)
				{
					UseItem(item);
					Refresh(false);
				}
				else
					EquipItem(item);
			}
			else if (actionMode == ActionModes.Use)
			{
				// Allow item to handle its own use, fall through to general use function if unhandled
				if (!item.UseItem(localItems))
					UseItem(item, localItems);
				Refresh(false);

				// Read a Book mod injected code
				if(item.ItemGroup == ItemGroups.Books && !item.IsArtifact)
				{
					ReadABook.ReadABookMod.BookName = DaggerfallUnity.Instance.ItemHelper.GetBookTitle(item.message, item.shortName);
					ReadABook.ReadABookMod.OpenCheck = 1;
				}
				// end injected code
			}
			else if (actionMode == ActionModes.Remove)
			{
				// Transfer to remote items
				if (remoteItems != null && !chooseOne)
				{
					int? canHold = null;
					if (usingWagon)
						canHold = WagonCanHoldAmount(item);
					TransferItem(item, localItems, remoteItems, canHold, true);
					if (theftBasket != null && lootTarget != null && lootTarget.houseOwned)
						theftBasket.RemoveItem(item);
				}
			}
			else if (actionMode == ActionModes.Info)
			{
				ShowInfoPopup(item);
			}
		}
	}
}
