﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Lore
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GamePage : Page
	{
		private bool mSpriteBatchSupported;

		private int mMapWidth = 0;
		private int mMapHeight = 0;

		private SpriteSheet mMapTiles;
		private SpriteSheet mCharacterTiles;
		private byte[] mMapLayer = null;
		private readonly object mapLock = new object();

		bool ClampToSourceRect = true;
		string InterpolationMode = CanvasImageInterpolation.Linear.ToString();

		private LorePlayer mParty;
		private List<Lore> mPlayerList;

		private PositionType mPosition;
		private int mFace = 0;
		private int mEncounter = 0;
		private int mMaxEnemy = 0;

		private int mTalkMode = 0;
		private bool mTriggeredDownEvent = false;
		private int mTalkX = 0;
		private int mTalkY = 0;

		private List<TextBlock> mPlayerNameList = new List<TextBlock>();
		private List<TextBlock> mPlayerHPList = new List<TextBlock>();
		private List<TextBlock> mPlayerSPList = new List<TextBlock>();
		private List<TextBlock> mPlayerESPList = new List<TextBlock>();
		private List<TextBlock> mPlayerACList = new List<TextBlock>();
		private List<TextBlock> mPlayerLevelList = new List<TextBlock>();
		private List<TextBlock> mPlayerConditionList = new List<TextBlock>();
		
		private List<TextBlock> mMenuList = new List<TextBlock>();
		private MenuMode mMenuMode = MenuMode.None;
		private int mMenuCount = 0;
		private int mMenuFocusID = 0;
		private Lore mMagicPlayer = null;
		private Lore mMagicWhomPlayer = null;

		private int mOrderFromPlayerID = -1;

		private bool mWeaponShopEnd = false;
		private int mBuyWeaponID = -1;

		private Lore mCurePlayer = null;
		private CureMenuState mCureMenuState = CureMenuState.None;

		private bool mTrainingEnd = false;

		// KeyDown에서 이벤트 처리가 이루어졌으면, KeyUp에서는 이벤트를 처리하지 않도록 체크
		private bool mKeyDownShowMenu = false;

		// 1 - 로어성에서 병사를 만났을때 발생하는 이벤트
		// 2 - 로어성을 떠날때 스켈레톤을 만나는 이벤트
		// 3 - 스켈레톤의 답을 거절했을때 이벤트
		// 4 - 게임 세계 도전 여부 Y/N
		private int mSpecialEvent = 0;

		private volatile bool mMoveEvent = false;

		private Random mRand = new Random();

		private List<EnemyData> mEnemyDataList = null;

		public GamePage()
		{
			var rootFrame = Window.Current.Content as Frame;

			this.InitializeComponent();

			mPlayerNameList.Add(PlayerName0);
			mPlayerNameList.Add(PlayerName1);
			mPlayerNameList.Add(PlayerName2);
			mPlayerNameList.Add(PlayerName3);
			mPlayerNameList.Add(PlayerName4);
			mPlayerNameList.Add(PlayerName5);

			mPlayerHPList.Add(PlayerHP0);
			mPlayerHPList.Add(PlayerHP1);
			mPlayerHPList.Add(PlayerHP2);
			mPlayerHPList.Add(PlayerHP3);
			mPlayerHPList.Add(PlayerHP4);
			mPlayerHPList.Add(PlayerHP5);

			mPlayerSPList.Add(PlayerSP0);
			mPlayerSPList.Add(PlayerSP1);
			mPlayerSPList.Add(PlayerSP2);
			mPlayerSPList.Add(PlayerSP3);
			mPlayerSPList.Add(PlayerSP4);
			mPlayerSPList.Add(PlayerSP5);

			mPlayerESPList.Add(PlayerESP0);
			mPlayerESPList.Add(PlayerESP1);
			mPlayerESPList.Add(PlayerESP2);
			mPlayerESPList.Add(PlayerESP3);
			mPlayerESPList.Add(PlayerESP4);
			mPlayerESPList.Add(PlayerESP5);

			mPlayerACList.Add(PlayerAC0);
			mPlayerACList.Add(PlayerAC1);
			mPlayerACList.Add(PlayerAC2);
			mPlayerACList.Add(PlayerAC3);
			mPlayerACList.Add(PlayerAC4);
			mPlayerACList.Add(PlayerAC5);

			mPlayerLevelList.Add(PlayerLevel0);
			mPlayerLevelList.Add(PlayerLevel1);
			mPlayerLevelList.Add(PlayerLevel2);
			mPlayerLevelList.Add(PlayerLevel3);
			mPlayerLevelList.Add(PlayerLevel4);
			mPlayerLevelList.Add(PlayerLevel5);

			mPlayerConditionList.Add(PlayerCondition0);
			mPlayerConditionList.Add(PlayerCondition1);
			mPlayerConditionList.Add(PlayerCondition2);
			mPlayerConditionList.Add(PlayerCondition3);
			mPlayerConditionList.Add(PlayerCondition4);
			mPlayerConditionList.Add(PlayerCondition5);

			mMenuList.Add(GameMenuText0);
			mMenuList.Add(GameMenuText1);
			mMenuList.Add(GameMenuText2);
			mMenuList.Add(GameMenuText3);
			mMenuList.Add(GameMenuText4);
			mMenuList.Add(GameMenuText5);
			mMenuList.Add(GameMenuText6);
			mMenuList.Add(GameMenuText7);
			mMenuList.Add(GameMenuText8);
			mMenuList.Add(GameMenuText9);

			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;
			gamePageKeyDownEvent = async (sender, args) =>
			{

				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (mMoveEvent)
					return;
				else if (ContinueText.Visibility == Visibility.Visible)
				{
					//if (mNextMode)
					//{
					//	ContinueText.Visibility = Visibility.Collapsed;

					//	DialogText.TextHighlighters.Clear();
					//	DialogText.Blocks.Clear();

					//	mNextMode = false;

					//}
					//else
						return;
				}

				
				if (mMenuMode == MenuMode.None && (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right ||
				 args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight ||
				 args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadDPadLeft || args.VirtualKey == VirtualKey.GamepadDPadRight))
				{
					void MovePlayer(int moveX, int moveY)
					{
						mParty.XAxis = moveX;
						mParty.YAxis = moveY;
					}

					var x = mParty.XAxis;
					var y = mParty.YAxis;

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
					{
						y--;
						if (mPosition == PositionType.Town)
							mFace = 1;
						else
							mFace = 5;
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
					{
						y++;
						if (mPosition == PositionType.Town)
							mFace = 0;
						else
							mFace = 4;
					}
					else if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						x--;
						if (mPosition == PositionType.Town)
							mFace = 3;
						else
							mFace = 7;
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						x++;
						if (mPosition == PositionType.Town)
							mFace = 2;
						else
							mFace = 6;
					}

					if (mParty.Map == 26)
						mFace += 4;

					if (x > 4 && x < mMapWidth - 3 && y > 4 && y < mMapHeight - 3)
					{
						if (mPosition == PositionType.Town)
						{
							if (mMapLayer[x + mMapWidth * y] == 0)
							{
								MovePlayer(x, y);
								InvokeSpecialEvent();
								mTriggeredDownEvent = true;
							}
							else if (1 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 21)
							{
								// Don't Move
							}
							else if (mMapLayer[x + mMapWidth * y] == 22)
							{
								// Enter Mode
							}
							else if (mMapLayer[x + mMapWidth * y] == 23)
							{
								ShowSign(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 24)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (mMapLayer[x + mMapWidth * y] == 25)
							{
								// Swarm
								MovePlayer(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 26)
							{
								// lava
								MovePlayer(x, y);
							}
							else if (27 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
						else if (mPosition == PositionType.Ground)
						{
							if (mMapLayer[x + mMapWidth * y] == 0)
							{
								MovePlayer(x, y);
								InvokeSpecialEvent();
								mTriggeredDownEvent = true;
							}
							else if (1 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 21)
							{
								// Don't Move
							}
							else if (mMapLayer[x + mMapWidth * y] == 22)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (mMapLayer[x + mMapWidth * y] == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (mMapLayer[x + mMapWidth * y] == 23 || mMapLayer[x + mMapWidth * y] == 49)
							{
							}
							else if (mMapLayer[x + mMapWidth * y] == 50)
							{
							}
							else if (27 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{

							}
						}
					}
				}
			};

			gamePageKeyUpEvent = async (sender, args) =>
			{
				int GetWeaponPrice(int weapon)
				{
					switch (weapon)
					{
						case 1:
							return 500;
						case 2:
							return 1500;
						case 3:
							return 3000;
						case 4:
							return 5000;
						case 5:
							return 10000;
						case 6:
							return 30000;
						case 7:
							return 60000;
						case 8:
							return 80000;
						case 9:
							return 100000;
						default:
							return 0;
					}
				}

				int GetShieldPrice(int shield)
				{
					switch (shield)
					{
						case 1:
							return 1000;
						case 2:
							return 5000;
						case 3:
							return 25000;
						case 4:
							return 80000;
						case 5:
							return 100000;
						default:
							return 0;
					}
				}

				int GetArmorPrice(int shield)
				{
					switch (shield)
					{
						case 1:
							return 5000;
						case 2:
							return 25000;
						case 3:
							return 80000;
						case 4:
							return 100000;
						case 5:
							return 200000;
						default:
							return 0;
					}
				}

				void ShowHealType()
				{
					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까 ?[/color]" });

					ShowMenu(MenuMode.HealType, new string[]
					{
							"상처를 치료",
							"독을 제거",
							"의식의 회복",
							"부활"
					});
				}

				if (mMoveEvent)
					return;
				else if (mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (mSpecialEvent == 2)
				{
					mMoveEvent = true;
					for (var y = mParty.YAxis - 4; y < mParty.YAxis; y++)
					{
						mMapLayer[mParty.XAxis + mMapWidth * (y - 1)] = 44;
						mMapLayer[mParty.XAxis + mMapWidth * y] = 48;
						Task.Delay(500).Wait();
					}
					mMoveEvent = false;

					TalkMode(mTalkX, mTalkY, args.VirtualKey);
					mSpecialEvent = 0;
				}
				else if (mSpecialEvent == 3)
				{
					mSpecialEvent = 0;
					mParty.Etc[30] |= 1;

					mParty.XAxis = 19;
					mParty.YAxis = 11;
					mParty.Map = 1;

					await LoadMapData();
					InitializeMap();
				}
				else if (mSpecialEvent == 4) {
					TalkMode(mTalkX, mTalkY, args.VirtualKey);
				}
				else if (ContinueText.Visibility == Visibility.Visible) {
					ContinueText.Visibility = Visibility.Collapsed;

					if (mSpecialEvent == 1)
					{
						mParty.Etc[49] |= 1 << 4;
						mSpecialEvent = 0;
					}
					else if (mTalkMode > 0)
					{
						if (mSpecialEvent == 0)
							TalkMode(mTalkX, mTalkY, args.VirtualKey);
					}
				}
				else if (mWeaponShopEnd)
				{
					mWeaponShopEnd = false;
					GoWeaponShop();
				}
				else if (mCureMenuState == CureMenuState.NotCure)
				{
					mCureMenuState = CureMenuState.None;
					ShowHealType();
				}
				else if (mCureMenuState == CureMenuState.CureEnd)
				{
					mCureMenuState = CureMenuState.None;
					GoHospital();
				}
				else if (mTrainingEnd == true)
				{
					mTrainingEnd = false;
					GoTrainingCenter();
				}
				else if (mTalkMode == 0 && mMenuMode == MenuMode.None && (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadMenu))
				{
					AppendText(new string[] { "당신의 명령을 고르시오 ===>" });

					ShowMenu(MenuMode.Game, new string[]
					{
						"일행의 상황을 본다",
						"개인의 상황을 본다",
						"일행의 건강 상태를 본다",
						"마법을 사용한다",
						"초능력을 사용한다",
						"여기서 쉰다",
						"게임 선택 상황"
					});
				}
				else if (mMenuMode != MenuMode.None)
				{
					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mMenuFocusID == 0)
							mMenuFocusID = mMenuCount - 1;
						else
							mMenuFocusID--;

						FocusMenuItem();

					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						mMenuFocusID = (mMenuFocusID + 1) % mMenuCount;
						FocusMenuItem();

					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						if (mMenuMode != MenuMode.None && mSpecialEvent == 0)
						{
							AppendText(new string[] { "" });
							HideMenu();

							mMenuMode = MenuMode.None;
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						HideMenu();
						if (mMenuMode == MenuMode.Game)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								string CheckEnable(int i)
								{
									if (mParty.Etc[i] == 0)
										return "불가";
									else
										return "가능";
								}

								AppendText(new string[] { $"X 축 = {mParty.XAxis + 1 }",
									$"Y 축 = {mParty.YAxis + 1}",
									"",
									$"남은 식량 = {mParty.Food}",
									$"남은 황금 = {mParty.Gold}",
									"",
									$"마법의 횃불 : {CheckEnable(0)}",
									$"공중 부상 : {CheckEnable(3)}",
									$"물위를 걸음 : {CheckEnable(1)}",
									$"늪위를 걸음 : {CheckEnable(2)}"
								});
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "능력을 보고싶은 인물을 선택하시오" });
								ShowCharacterMenu(MenuMode.ViewCharacter);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText(new string[] { "[color={RGB.White}]이름[/color]\t\t[color=cd5c5c]중독\t\t의식불명\t죽음[/color]" });
								mPlayerList.ForEach(delegate (Lore player)
								{
									string space;
									if (player.Name.Length >= 4)
										space = "\t";
									else
										space = "\t\t";
									AppendText(new string[] { $"{player.Name}{space}{player.Poison}\t\t{player.Unconscious}\t\t{player.Dead}" }, true);
								});
							}
							else if (mMenuFocusID == 3)
								ShowCharacterMenu(MenuMode.CastSpell);
							else if (mMenuFocusID == 4)
								ShowCharacterMenu(MenuMode.Extrasense);
							else if (mMenuFocusID == 5)
								Rest();
							else if (mMenuFocusID == 6)
							{
								AppendText(new string[] { "게임 선택 상황" });

								var gameOptions = new string[]
								{
									"난이도 조절",
									"정식 일행의 순서 정렬",
									"일행에서 제외 시킴",
									"이전의 게임을 재개",
									"현재의 게임을 저장",
									"게임을 마침",
								};

								ShowMenu(MenuMode.GameOptions, gameOptions);
							}
						}
						else if (mMenuMode == MenuMode.ViewCharacter)
						{
							mMenuMode = MenuMode.None;

							var shieldStr = mPlayerList[mMenuFocusID].Shield != 0 ? $"[color = 00ff00]방패 - {Common.GetDefenseStr(mPlayerList[mMenuFocusID].Weapon)}[/ color]" : "";
							var armorStr = mPlayerList[mMenuFocusID].Armor != 0 ? $"[color=00ff00]갑옷 - {Common.GetDefenseStr(mPlayerList[mMenuFocusID].Weapon)}[/color]" : "";

							AppendText(new string[] { $"# 이름 : {mPlayerList[mMenuFocusID].Name}",
								$"# 성별 : {Common.GetGenderStr(mPlayerList[mMenuFocusID])}",
								$"# 계급 : {Common.GetClassStr(mPlayerList[mMenuFocusID])}",
								"",
								$"[color=00ffff]체력 : {mPlayerList[mMenuFocusID].Strength}[/color]\t[color=00ffff]정신력 : {mPlayerList[mMenuFocusID].Mentality}[/color]\t[color=00ffff]집중력 : {mPlayerList[mMenuFocusID].Concentration}[/color]\t[color=00ffff]인내력 : {mPlayerList[mMenuFocusID].Endurance}[/color]",
								$"[color=00ffff]저항력 : {mPlayerList[mMenuFocusID].Resistance}[/color]\t[color=00ffff]민첩성 : {mPlayerList[mMenuFocusID].Agility}[/color]\t[color=00ffff]행운 : {mPlayerList[mMenuFocusID].Luck}[/color]",
								"",
								$"[color=00ffff]무기의 정확성 : {mPlayerList[mMenuFocusID].Accuracy[0]}[/color]\t\t[color=00ffff]전투 레벨 : {mPlayerList[mMenuFocusID].Level[0]}[/color]",
								$"[color=00ffff]정신력의 정확성 : {mPlayerList[mMenuFocusID].Accuracy[1]}[/color]\t[color=00ffff]마법 레벨 : {mPlayerList[mMenuFocusID].Level[1]}[/color]",
								$"[color=00ffff]초감각의 정확성 : {mPlayerList[mMenuFocusID].Accuracy[2]}[/color]\t[color=00ffff]초감각 레벨 : {mPlayerList[mMenuFocusID].Level[2]}[/color]",
								"",
								$"[color=00ffff]## 경험치 : {mPlayerList[mMenuFocusID].Experience}[/color]",
								"",
								$"[color=00ff00]사용 무기 - {Common.GetWeaponStr(mPlayerList[mMenuFocusID].Weapon)}[/color]\t{shieldStr}\t{armorStr}"
							});
						}
						else if (mMenuMode == MenuMode.CastSpell)
						{
							mMenuMode = MenuMode.None;

							if (IsAvailableMember(mPlayerList[mMenuFocusID]))
							{
								mMagicPlayer = mPlayerList[mMenuFocusID];
								AppendText(new string[] { "사용할 마법의 종류 ===>" });
								ShowMenu(MenuMode.SpellCategory, new string[]
								{
									"공격 마법",
									"치료 마법",
									"변화 마법"
								});
							}
							else
							{
								AppendText(new string[] { $"{GetGenderData(mPlayerList[mMenuFocusID])}는 마법을 사용할수있는 상태가 아닙니다" });
							}
						}
						else if (mMenuMode == MenuMode.SpellCategory)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								AppendText(new string[] { "전투 모드가 아닐때는 공격 마법을 사용할 수 없습니다." });
								ContinueText.Visibility = Visibility.Visible;
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "누구에게" });
								var playerList = new string[mPlayerList.Count + 1];

								for (var i = 0; i < mPlayerList.Count; i++)
									playerList[i] = mPlayerList[i].Name;

								playerList[playerList.Length - 1] = "모든 사람들에게";

								ShowMenu(MenuMode.ChooseCureSpell, playerList);
							}
							else if (mMenuFocusID == 2)
							{
								mMenuMode = MenuMode.None;

								AppendText(new string[] { "선택" });

								int availableCount;
								if (mMagicPlayer.Level[1] > 1)
									availableCount = mMagicPlayer.Level[1] / 2 + 1;
								else
									availableCount = 1;

								if (availableCount > 7)
									availableCount = 7;

								var totalMagicCount = 40 - 33 + 1;
								if (availableCount < totalMagicCount)
									totalMagicCount = availableCount;

								var phenominaMagicMenu = new string[totalMagicCount];
								for (var i = 33; i < 33 + availableCount; i++)
									phenominaMagicMenu[i - 33] = Common.GetMagicStr(i);

								ShowMenu(MenuMode.ApplyPhenominaMagic, phenominaMagicMenu);
							}
						}
						else if (mMenuMode == MenuMode.ChooseCureSpell)
						{
							mMenuMode = MenuMode.None;

							mMagicWhomPlayer = mPlayerList[mMenuFocusID];

							AppendText(new string[] { "선택" });
							if (mMenuFocusID < mPlayerList.Count)
							{
								var availableCount = mMagicPlayer.Level[1] / 2 + 1;
								if (availableCount > 7)
									availableCount = 7;

								var totalMagicCount = 25 - 19 + 1;
								if (availableCount < totalMagicCount)
									totalMagicCount = availableCount;

								var cureMagicMenu = new string[totalMagicCount];
								for (var i = 19; i < 19 + availableCount; i++)
									cureMagicMenu[i - 19] = Common.GetMagicStr(i);

								ShowMenu(MenuMode.ApplyCureMagic, cureMagicMenu);
							}
							else
							{
								var availableCount = mMagicPlayer.Level[1] / 2 - 3;
								if (availableCount < 0)
								{
									AppendText(new string[] { $"{mMagicPlayer.Name}는 강한 치료 마법은 아직 불가능 합니다." });
									ContinueText.Visibility = Visibility.Visible;
								}
								else
								{
									var totalMagicCount = 32 - 26 + 1;
									if (availableCount < totalMagicCount)
										totalMagicCount = availableCount;

									var cureMagicMenu = new string[totalMagicCount];
									for (var i = 26; i < 26 + availableCount; i++)
										cureMagicMenu[i - 26] = Common.GetMagicStr(i);

									ShowMenu(MenuMode.ApplyCureAllMagic, cureMagicMenu);
								}
							}
						}
						else if (mMenuMode == MenuMode.ApplyCureMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							switch (mMenuFocusID)
							{
								case 0:
									HealOne(mMagicWhomPlayer);
									break;
								case 1:
									CureOne(mMagicWhomPlayer);
									break;
								case 2:
									CureOne(mMagicWhomPlayer);
									HealOne(mMagicWhomPlayer);
									break;
								case 3:
									ConsciousOne(mMagicWhomPlayer);
									break;
								case 4:
									RevitalizeOne(mMagicWhomPlayer);
									break;
								case 5:
									ConsciousOne(mMagicWhomPlayer);
									CureOne(mMagicWhomPlayer);
									HealOne(mMagicWhomPlayer);
									break;
								case 6:
									RevitalizeOne(mMagicWhomPlayer);
									ConsciousOne(mMagicWhomPlayer);
									CureOne(mMagicWhomPlayer);
									HealOne(mMagicWhomPlayer);
									break;
							}

							UpdatePlayersStat();
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mMenuMode == MenuMode.ApplyCureAllMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							switch (mMenuFocusID)
							{
								case 0:
									HealAll();
									break;
								case 1:
									CureAll();
									break;
								case 2:
									CureAll();
									HealAll();
									break;
								case 3:
									ConsciousAll();
									break;
								case 4:
									ConsciousAll();
									CureAll();
									HealAll();
									break;
								case 5:
									RevitalizeAll();
									break;
								case 6:
									RevitalizeAll();
									ConsciousAll();
									CureAll();
									HealAll();
									break;

							}

							UpdatePlayersStat();
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mMenuMode == MenuMode.ApplyPhenominaMagic)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mMagicPlayer.SP < 1)
									ShowNotEnoughSP();
								else
								{
									if (mParty.Etc[0] < 255)
										mParty.Etc[0]++;

									AppendText(new string[] { $"[color={RGB.White}]일행은 마법의 횃불을 밝혔습니다.[/color]" });
									mMagicPlayer.SP--;
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[3] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 공중부상중 입니다.[/color]" });
									mMagicPlayer.SP -= 5;
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.SP < 10)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[1] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 물위를 걸을수 있습니다.[/color]" });
									mMagicPlayer.SP -= 10;
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mMagicPlayer.SP < 20)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[2] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 늪위를 걸을수 있습니다.[/color]" });
									mMagicPlayer.SP -= 20;
								}
							}
							else if (mMenuFocusID == 4)
							{
								if (mMagicPlayer.SP < 25)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" });

									ShowMenu(MenuMode.VaporizeMoveDirection, new string[] { "북쪽으로 기화 이동",
										"남쪽으로 기화 이동",
										"동쪽으로 기화 이동",
										"서쪽으로 기화 이동" });
								}
							}
							else if (mMenuFocusID == 5)
							{
								if (mParty.Map == 20 || mParty.Map == 25 || mParty.Map == 26)
									AppendText(new string[] { $"[color=ff00ff]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" }, true);
								else if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TransformDirection, new string[] { "북쪽에 지형 변화",
										"남쪽에 지형 변화",
										"동쪽에 지형 변화",
										"서쪽에 지형 변화" });
								}
							}
							else if (mMenuFocusID == 6)
							{
								if (mParty.Map == 20 || mParty.Map == 25 || mParty.Map == 26)
									AppendText(new string[] { $"[color=ff00ff]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" }, true);
								else if (mMagicPlayer.SP < 50)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.VaporizeMoveDirection, new string[] { "북쪽으로 공간이동",
										"남쪽으로 공간이동",
										"동쪽으로 공간이동",
										"서쪽으로 공간이동" });
								}
							}
							else if (mMenuFocusID == 7)
							{
								if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									var count = mPlayerList.Count;
									if (mParty.Food + count > 255)
										mParty.Food = 255;
									else
										mParty.Food = mParty.Food + count;
									mMagicPlayer.SP -= 30;

									AppendText(new string[] { $"[color={RGB.White}]식량 제조 마법은 성공적으로 수행되었습니다[/color]",
									$"[color={RGB.White}]            {count} 개의 식량이 증가됨[/color]",
									$"[color={RGB.White}]      일행의 현재 식량은 {mParty.Food} 개 입니다[/color]" }, true);
								}
							}

							DisplaySP();
						}
						else if (mMenuMode == MenuMode.VaporizeMoveDirection)
						{
							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + 2 * xOffset;
							var newY = mParty.YAxis + 2 * yOffset;

							var canMove = false;

							var moveTile = mMapLayer[newX + mMapWidth * newY];
							switch (mPosition)
							{
								case PositionType.Town:
									if (moveTile == 0 || (27 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Ground:
									if (moveTile == 0 || (24 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Den:
									if (moveTile == 0 || (41 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Keep:
									if (moveTile == 0 || (40 <= moveTile && moveTile <= 47))
										canMove = true;
									break;

							}

							if (!canMove)
								AppendText(new string[] { $"기화 이동이 통하지 않습니다." }, true);
							else
							{
								mMagicPlayer.SP -= 25;
								if (mMapLayer[(mParty.XAxis + xOffset) + mMapWidth * (mParty.YAxis + yOffset)] == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[(newX + xOffset) + mMapWidth * (newY + yOffset)] == 52))
								{
									AppendText(new string[] { $"[color=ff00ff]알수없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
								}
								else
								{
									mParty.XAxis = newX;
									mParty.YAxis = newY;

									AppendText(new string[] { $"[color={RGB.White}]기화 이동을 마쳤습니다.[/color]" }, true);
								}

							}
						}
						else if (mMenuMode == MenuMode.TransformDirection)
						{

							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + 2 * xOffset;
							var newY = mParty.YAxis + 2 * yOffset;


							mMagicPlayer.SP -= 30;

							if (mMapLayer[(mParty.XAxis + xOffset) + mMapWidth * (mParty.YAxis + yOffset)] == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[(mParty.XAxis + xOffset) + mMapWidth * (mParty.YAxis + yOffset)] == 52))
							{
								AppendText(new string[] { $"[color=ff00ff]알수없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
							}
							else
							{
								// 공간 이동은 UI에 대해서 좀더 고민이 필요
							}
						}
						else if (mMenuMode == MenuMode.Extrasense)
						{
							mMenuMode = MenuMode.None;

							if (IsAvailableMember(mPlayerList[mMenuFocusID]))
							{
								if (mPlayerList[mMenuFocusID].Class != 2 && mPlayerList[mMenuFocusID].Class != 3 && mPlayerList[mMenuFocusID].Class != 6 && (mParty.Etc[37] & 1) == 0)
								{
									AppendText(new string[] { $"당신에게는 아직 능력이 없습니다.'" }, true);
									ContinueText.Visibility = Visibility.Visible;
								}
								else
								{
									mMagicPlayer = mPlayerList[mMenuFocusID];

									AppendText(new string[] { "사용할 초감각의 종류 ===>" });

									var extrsenseMenu = new string[5];
									for (var i = 41; i < 45; i++)
										extrsenseMenu[i - 41] = Common.GetMagicStr(i);

									ShowMenu(MenuMode.ChooseExtrasense, extrsenseMenu);
								}
							}
							else
							{
								AppendText(new string[] { $"{GetGenderData(mPlayerList[mMenuFocusID])}는 초감각을 사용할수있는 상태가 아닙니다" });
							}
						}
						else if (mMenuMode == MenuMode.ChooseExtrasense)
						{
							HideMenu();
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mParty.Map > 24)
									AppendText(new string[] { $" 이 동굴의 악의 힘이 이 마법을 방해합니다." });
								else if (mMagicPlayer.ESP < 10)
									ShowNotEnoughESP();
								else
								{
									// 투시 구현 필요
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.ESP < 5)
									ShowNotEnoughESP();
								else
								{
									int GetPredict()
									{
										int predict = -1;
										switch (mParty.Etc[9])
										{
											case 0:
											case 1:
											case 2:
												predict = 0;
												break;
											case 3:
												predict = 1;
												break;
											case 4:
											case 5:
												predict = 2;
												break;
											case 6:
												predict = 3;
												break;
										}

										if (predict == 3 && mParty.Map == 7)
											predict = 4;

										switch (mParty.Etc[12])
										{
											case 1:
												predict = 5;
												break;
											case 2:
												predict = 6;
												break;
											case 3:
												predict = 7;
												break;
										}

										if (mParty.Etc[12] == 3)
										{
											switch (mParty.Etc[13])
											{
												case 0:
													predict = 8;
													break;
												case 1:
													predict = 9;
													break;
												case 2:
													predict = 10;
													break;
												case 3:
													predict = 8;
													break;
												case 4:
													predict = 11;
													break;
												case 5:
													predict = 10;
													break;
												case 6:
													predict = 12;
													break;
											}

											if (mParty.Etc[13] == 6 && mParty.Map == 15)
												predict = 13;

											if (mParty.Etc[13] == 6 && mParty.Map == 2)
												predict = 12;
											else if (predict == 12)
											{
												switch (mParty.Etc[14])
												{
													case 0:
														predict = 14;
														break;
													case 1:
														predict = 15;
														break;
													case 2:
														predict = 14;
														break;
													case 3:
														predict = 16;
														break;
													case 4:
														predict = 14;
														break;
													case 5:
														predict = 17;
														break;
												}

												if (mParty.Map == 13 && mParty.Etc[39] % 2 == 0 && mParty.Etc[39] % 2 == 0)
													predict = 18;

												if (mParty.Etc[14] == 5)
												{
													if (mParty.Map == 4 || (19 <= mParty.Map && mParty.Map <= 21))
														predict = 20;

													if (mParty.Etc[39] % 2 == 1 && mParty.Etc[40] % 2 == 1)
														predict = 19;
												}

												if (mParty.Map == 5)
													predict = 22;

												if (mParty.Etc[14] == 5 && mParty.Map > 21)
												{
													switch (mParty.Map)
													{
														case 22:
														case 24:
															predict = 21;
															break;
														case 23:
															predict = 22;
															break;
														case 25:
															predict = 23;
															break;
														case 26:
															predict = 24;
															break;
													}
												}
											}
										}


										return predict;
									}

									var predictStr = new string[]
									{
										"로드 안을 만날",
										"메나스를 탐험할",
										"로드 안에게 다시 돌아갈",
										"라스트디치로 갈",
										"라스트디치의 성주를 만날",
										"PYRAMID 속의 Major Mummy를 물리칠",
										"라스트디치의 성주에게로 돌아갈",
										"라스트디치의 GROUND GATE로 갈",
										"GAIA TERRA의 성주를 만날",
										"EVIL SEAL에서 황금의 봉인을 발견할",
										"GAIA TERRA의 성주에게 돌아갈",
										"QUAKE에서 ArchiGagoyle를 물리칠",
										"북동쪽의 WIVERN 동굴에 갈",
										"WATER FIELD로 갈",
										"WATER FIELD의 군주를 만날",
										"NOTICE 속의 Hidra를 물리칠",
										"LOCKUP 속의 Dragon을 물리칠",
										"GAIA TERRA 의 SWAMP GATE로 갈",
										"위쪽의 게이트를 통해 SWAMP KEEP으로 갈",
										"SWAMP 대륙에 존재하는 두개의 봉인을 풀",
										"SWAMP KEEP의 라바 게이트를 작동 시킬",
										"적의 집결지인 EVIL CONCENTRATION으로 갈",
										"숨겨진 적의 마지막 요새로 들어갈",
										"위쪽의 동굴에서 네크로맨서를 만날",
										"네크로맨서와 마지막 결전을 벌일"
									};

									if (mMagicPlayer.ESP < 5)
										ShowNotEnoughESP();
									else
									{
										var predict = GetPredict();

										AppendText(new string[] { $" 당신은 당신의 미래를 예언한다 ...", "" });
										if (0 <= predict && predict < predictStr.Length)
											AppendText(new string[] { $" # 당신은 [color={RGB.LightGreen}]{predict} 것이다[/color]" }, true);
										else
											AppendText(new string[] { $" # [color={RGB.LightGreen}]당신은 어떤 힘에 의해 예언을 방해 받고 있다[/color]" }, true);

										mMagicPlayer.ESP -= 5;
										ContinueText.Visibility = Visibility.Visible;
									}
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.ESP < 20)
									ShowNotEnoughESP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]당신은 잠시동안 다른 사람의 마음을 읽을수 있다.[/color]" });
									mParty.Etc[4] = 3;
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mParty.Map > 24)
									AppendText(new string[] { $"[color={RGB.LightMagenta}] 이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
								else if (mMagicPlayer.ESP < mMagicPlayer.Level[2] * 5)
									ShowNotEnoughESP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TelescopeDirection, new string[] { "북쪽으로 천리안을 사용",
										"남쪽으로 천리안을 사용",
										"동쪽으로 천리안을 사용",
										"서쪽으로 천리안을 사용" });
								}
							}
							else if (mMenuFocusID == 4)
							{
								AppendText(new string[] { $"{Common.GetMagicStr(45)}은 전투 모드에서만 사용됩니다." });
							}
						}
						else if (mMenuMode == MenuMode.TelescopeDirection)
						{
							var xOffset = 0;
							var yOffset = 0;

							mMagicPlayer.ESP -= mMagicPlayer.Level[2] * 5;

							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							// 천리안 구현중...
						}
						else if (mMenuMode == MenuMode.GameOptions)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]" });

								var maxEnemyStr = new string[5];
								for (var i = 0; i < maxEnemyStr.Length; i++)
									maxEnemyStr[i] = $"{i + 3} 명의 적들";

								ShowMenu(MenuMode.SetMaxEnemy, maxEnemyStr);
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]",
								"[color=e0ffff]순서를 바꿀 일원[/color]" });

								ShowCharacterMenu(MenuMode.OrderFromCharacter);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]일행에서 제외 시키고 싶은 사람을 고르십시오.[/color]" });

								ShowCharacterMenu(MenuMode.DelistCharacter);
							}
							else if (mMenuFocusID == 3)
							{
								await LoadFile();

								AppendText(new string[] { $"[color={RGB.LightCyan}]저장했던 게임을 다시 불러옵니다[/color]" });
							}
							else if (mMenuFocusID == 4)
							{
								var saveData = new SaveData()
								{
									PlayerList = mPlayerList,
									Party = mParty,
									Map = new Map()
									{
										Width = mMapWidth,
										Height = mMapHeight,
										Data = mMapLayer
									},
									Encounter = mEncounter,
									MaxEnemy = mMaxEnemy
								};

								var saveJSON = JsonConvert.SerializeObject(saveData);

								var storageFolder = ApplicationData.Current.LocalFolder;
								var saveFile = await storageFolder.CreateFileAsync("loreSave.dat", CreationCollisionOption.ReplaceExisting);
								await FileIO.WriteTextAsync(saveFile, saveJSON);

								AppendText(new string[] { $"[color={RGB.LightRed}]현재의 게임을 저장합니다.[/color]" });
							}
							else if (mMenuFocusID == 5)
							{
								AppendText(new string[] { $"[color={RGB.LightGreen}]정말로 끝내겠습니까 ?[/color]" });

								ShowMenu(MenuMode.ConfirmExit, new string[] {
									"<< 아니오 >>",
									"<<   예   >>"
								});
							}
						}
						else if (mMenuMode == MenuMode.SetMaxEnemy)
						{
							mMenuMode = MenuMode.None;

							mMaxEnemy = mMenuFocusID + 3;
							if (mMaxEnemy == 2)
								mMaxEnemy = 5;

							AppendText(new string[] { $"[color={RGB.LightRed}]일행들의 지금 성격은 어떻습니까 ?[/color]" });

							ShowMenu(MenuMode.SetEncounterType, new string[]
							{
								"일부러 전투를 피하고 싶다",
								"너무 잦은 전투는 원하지 않는다",
								"마주친 적과는 전투를 하겠다",
								"보이는 적들과는 모두 전투하겠다",
								"그들은 피에 굶주려 있다"
							});
						}
						else if (mMenuMode == MenuMode.SetEncounterType)
						{
							mMenuMode = MenuMode.None;

							mEncounter = 6 - (mMenuFocusID + 1);
						}
						else if (mMenuMode == MenuMode.OrderFromCharacter)
						{
							mMenuMode = MenuMode.None;

							mOrderFromPlayerID = mMenuFocusID;

							AppendText(new string[] { "[color=e0ffff]순서를 바꿀 일원[/color]" });

							ShowCharacterMenu(MenuMode.OrderToCharacter);
						}
						else if (mMenuMode == MenuMode.OrderToCharacter)
						{
							mMenuMode = MenuMode.None;

							var tempPlayer = mPlayerList[mOrderFromPlayerID];
							mPlayerList[mOrderFromPlayerID] = mPlayerList[mMenuFocusID];
							mPlayerList[mMenuFocusID] = tempPlayer;

							DisplayPlayerInfo();

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();
						}
						else if (mMenuMode == MenuMode.DelistCharacter)
						{
							mMenuMode = MenuMode.None;

							mPlayerList.RemoveAt(mMenuFocusID);

							DisplayPlayerInfo();

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();
						}
						else if (mMenuMode == MenuMode.ConfirmExit)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();
							}
							else
								CoreApplication.Exit();
						}
						else if (mMenuMode == MenuMode.JoinMadJoe)
						{
							mMenuMode = MenuMode.None;
							// Mad Joe 참여
						}
						else if (mMenuMode == MenuMode.Grocery)
						{
							mMenuMode = MenuMode.None;

							if (mParty.Gold < (mMenuFocusID + 1) * 100)
								ShowNotEnoughMoney();
							else
							{
								mParty.Gold -= (mMenuFocusID + 1) * 100;
								var food = (mMenuFocusID + 1) * 10;

								if (mParty.Food + food > 255)
									mParty.Food = 255;
								else
									mParty.Food += food;

								ShowThankyou();
							}
						}
						else if (mMenuMode == MenuMode.WeaponType)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								AppendText(new string[] { $"[color={RGB.White}]어떤 무기를 원하십니까 ?[/color]" });

								ShowMenu(MenuMode.BuyWeapon, new string[]
								{
									$"{Common.GetWeaponStr(1)} : 금 500 개",
									$"{Common.GetWeaponStr(2)} : 금 1500 개",
									$"{Common.GetWeaponStr(3)} : 금 3000 개",
									$"{Common.GetWeaponStr(4)} : 금 5000 개",
									$"{Common.GetWeaponStr(5)} : 금 10000 개",
									$"{Common.GetWeaponStr(6)} : 금 30000 개",
									$"{Common.GetWeaponStr(7)} : 금 60000 개",
									$"{Common.GetWeaponStr(8)} : 금 80000 개",
									$"{Common.GetWeaponStr(9)} : 금 100000 개"
								});
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { $"[color={RGB.White}]어떤 방패를 원하십니까 ?[/color]" });

								ShowMenu(MenuMode.BuyShield, new string[]
								{
									$"{Common.GetDefenseStr(1)}방패 : 금 1000 개",
									$"{Common.GetDefenseStr(2)}방패 : 금 5000 개",
									$"{Common.GetDefenseStr(3)}방패 : 금 25000 개",
									$"{Common.GetDefenseStr(4)}방패 : 금 80000 개",
									$"{Common.GetDefenseStr(5)}방패 : 금 100000 개"
								});
							}
							else if (mMenuFocusID == 2)
							{
								AppendText(new string[] { $"[color={RGB.White}]어떤 갑옷를 원하십니까 ?[/color]" });

								ShowMenu(MenuMode.BuyArmor, new string[]
								{
									$"{Common.GetDefenseStr(1)}갑옷 : 금 5000 개",
									$"{Common.GetDefenseStr(2)}갑옷 : 금 25000 개",
									$"{Common.GetDefenseStr(3)}갑옷 : 금 80000 개",
									$"{Common.GetDefenseStr(4)}갑옷 : 금 100000 개",
									$"{Common.GetDefenseStr(5)}갑옷 : 금 200000 개"
								});
							}
						}
						else if (mMenuMode == MenuMode.BuyWeapon)
						{
							mMenuMode = MenuMode.None;

							var price = GetWeaponPrice(mMenuFocusID + 1);

							if (mParty.Gold < price)
							{
								mWeaponShopEnd = true;
								ShowNotEnoughMoney();
							}
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetWeaponStr(mBuyWeaponID)}를 사용하시겠습니까 ?[/color]" });

								ShowCharacterMenu(MenuMode.UseWeaponCharacter);
							}
						}
						else if (mMenuMode == MenuMode.UseWeaponCharacter)
						{
							mMenuMode = MenuMode.None;

							var player = mPlayerList[mMenuFocusID];

							if (player.Class == 5)
							{
								AppendText(new string[] { $"[color={RGB.LightMagenta}]전투승은 이 무기가 필요없습니다.[/color]" });

								mWeaponShopEnd = true;
								ContinueText.Visibility = Visibility.Visible;
							}
							else
							{
								var power = 0;
								switch (mBuyWeaponID)
								{
									case 1:
										power = 5;
										break;
									case 2:
										power = 7;
										break;
									case 3:
										power = 9;
										break;
									case 4:
										power = 10;
										break;
									case 5:
										power = 15;
										break;
									case 6:
										power = 20;
										break;
									case 7:
										power = 30;
										break;
									case 8:
										power = 40;
										break;
									case 9:
										power = 50;
										break;
								}

								player.Weapon = mBuyWeaponID;
								player.WeaPower = power;

								if (player.Class == 1)
									player.WeaPower = (int)Math.Round(player.WeaPower * 1.5);

								mParty.Gold -= GetWeaponPrice(mBuyWeaponID);

								GoWeaponShop();
							}
						}
						else if (mMenuMode == MenuMode.BuyShield)
						{
							mMenuMode = MenuMode.None;

							var price = GetShieldPrice(mMenuFocusID + 1);

							if (mParty.Gold < price)
							{
								mWeaponShopEnd = true;
								ShowNotEnoughMoney();
							}
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}방패를 사용하시겠습니까 ?[/color]" });

								ShowCharacterMenu(MenuMode.UseShieldCharacter);
							}
						}
						else if (mMenuMode == MenuMode.UseShieldCharacter)
						{
							mMenuMode = MenuMode.None;

							var player = mPlayerList[mMenuFocusID];

							player.Shield = mBuyWeaponID;
							player.ShiPower = mBuyWeaponID;
							player.AC = player.ShiPower + player.ArmPower;

							if (player.Class == 1)
								player.AC++;

							if (player.AC > 10)
								player.AC = 10;

							mParty.Gold -= GetWeaponPrice(mBuyWeaponID);

							GoWeaponShop();
						}
						else if (mMenuMode == MenuMode.BuyArmor)
						{
							mMenuMode = MenuMode.None;

							var price = GetArmorPrice(mMenuFocusID + 1);

							if (mParty.Gold < price)
							{
								mWeaponShopEnd = true;
								ShowNotEnoughMoney();
							}
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}갑옷을 사용하시겠습니까 ?[/color]" });

								ShowCharacterMenu(MenuMode.UseArmorCharacter);
							}
						}
						else if (mMenuMode == MenuMode.UseArmorCharacter)
						{
							mMenuMode = MenuMode.None;

							var player = mPlayerList[mMenuFocusID];

							player.Shield = mBuyWeaponID;
							player.ShiPower = mBuyWeaponID + 1;
							player.AC = player.ShiPower + player.ArmPower;

							if (player.Class == 1)
								player.AC++;

							if (player.AC > 10)
								player.AC = 10;

							mParty.Gold -= GetWeaponPrice(mBuyWeaponID);

							GoWeaponShop();
						}
						else if (mMenuMode == MenuMode.Hospital)
						{
							mMenuMode = MenuMode.None;

							mCurePlayer = mPlayerList[mMenuFocusID];

							ShowHealType();
						}
						else if (mMenuMode == MenuMode.HealType)
						{
							if (mMenuFocusID == 0)
							{
								if (mCurePlayer.Dead > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
								else if (mCurePlayer.Unconscious > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 의식불명입니다" });
								else if (mCurePlayer.Poison > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 독이 퍼진 상태입니다" });
								else if (mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level[0])
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 치료가 필요하지 않습니다" });

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison > 0 || mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level[0])
								{
									ContinueText.Visibility = Visibility;
									
									mCureMenuState = CureMenuState.NotCure;
								}
								else
								{
									var payment = mCurePlayer.Endurance * mCurePlayer.Level[0] - mCurePlayer.HP;
									payment = payment * mCurePlayer.Level[0] / 2 + 1;

									if (mParty.Gold < payment)
									{
										mCureMenuState = CureMenuState.NotCure;
										ShowNotEnoughMoney();
									}
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.HP = mCurePlayer.Endurance * mCurePlayer.Level[0];

										AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}의 모든 건강이 회복되었다[/color]" });

										DisplayHP();

										ContinueText.Visibility = Visibility;

										mCureMenuState = CureMenuState.CureEnd;
									}
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mCurePlayer.Dead > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
								else if (mCurePlayer.Unconscious > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 의식불명입니다" });
								else if (mCurePlayer.Poison == 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 독에 걸리지 않았습니다" });

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison == 0)
								{
									ContinueText.Visibility = Visibility;

									mCureMenuState = CureMenuState.NotCure;
								}
								else
								{
									var payment = mCurePlayer.Level[0] * 10;

									if (mParty.Gold < payment)
									{
										mCureMenuState = CureMenuState.NotCure;
										ShowNotEnoughMoney();
									}
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Poison = 0;

										AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 독이 제거 되었습니다[/color]" });

										DisplayCondition();

										ContinueText.Visibility = Visibility;
										mCureMenuState = CureMenuState.CureEnd;
									}
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mCurePlayer.Dead > 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
								else if (mCurePlayer.Unconscious == 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 의식불명이 아닙니다" });

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious == 0)
								{
									ContinueText.Visibility = Visibility;

									mCureMenuState = CureMenuState.NotCure;
								}
								else
								{
									var payment = mCurePlayer.Unconscious * 2;

									if (mParty.Gold < payment)
									{
										mCureMenuState = CureMenuState.NotCure;
										ShowNotEnoughMoney();
									}
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Unconscious = 0;
										mCurePlayer.HP = 1;

										AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 의식을 차렸습니다[/color]" });

										DisplayCondition();
										DisplayHP();

										ContinueText.Visibility = Visibility;
										mCureMenuState = CureMenuState.CureEnd;
									}
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mCurePlayer.Dead == 0)
									AppendText(new string[] { $"{mCurePlayer.Name}(은)는 죽지 않았습니다" });

								if (mCurePlayer.Dead == 0)
								{
									ContinueText.Visibility = Visibility;

									mCureMenuState = CureMenuState.NotCure;
								}
								else
								{
									var payment = mCurePlayer.Dead * 100 + 400;

									if (mParty.Gold < payment)
									{
										mCureMenuState = CureMenuState.NotCure;
										ShowNotEnoughMoney();
									}
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Dead = 0;

										if (mCurePlayer.Unconscious > mCurePlayer.Endurance * mCurePlayer.Level[0])
											mCurePlayer.Unconscious = mCurePlayer.Endurance * mCurePlayer.Level[0];

										AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 다시 살아났습니다[/color]" });

										DisplayCondition();

										ContinueText.Visibility = Visibility;

										mCureMenuState = CureMenuState.CureEnd;
									}
								}
							}
						}
						else if (mMenuMode == MenuMode.TrainingCenter)
						{
							var player = mPlayerList[mMenuFocusID];

							var exp = player.Experience;
							var nextLevel = 0;
							if (exp < 20000)
							{
								if (0 <= exp && exp <= 1499)
									nextLevel = 1;
								else if (1500 <= exp && exp <= 5999)
									nextLevel = 2;
								else if (6000 <= exp && exp <= 19999)
									nextLevel = 3;
							}
							else
							{
								exp /= 10000;

								if (2 <= exp && exp <= 4)
									nextLevel = 4;
								else if (5 <= exp && exp <= 14)
									nextLevel = 5;
								else if (15 <= exp && exp <= 24)
									nextLevel = 6;
								else if (25 <= exp && exp <= 49)
									nextLevel = 7;
								else if (50 <= exp && exp <= 79)
									nextLevel = 8;
								else if (80 <= exp && exp <= 104)
									nextLevel = 9;
								else if (105 <= exp && exp <= 131)
									nextLevel = 10;
								else if (132 <= exp && exp <= 161)
									nextLevel = 11;
								else if (162 <= exp && exp <= 194)
									nextLevel = 12;
								else if (195 <= exp && exp <= 230)
									nextLevel = 13;
								else if (231 <= exp && exp <= 269)
									nextLevel = 14;
								else if (270 <= exp && exp <= 311)
									nextLevel = 15;
								else if (312 <= exp && exp <= 356)
									nextLevel = 16;
								else if (357 <= exp && exp <= 404)
									nextLevel = 17;
								else if (405 <= exp && exp <= 455)
									nextLevel = 18;
								else if (456 <= exp && exp <= 509)
									nextLevel = 19;
								else
									nextLevel = 20;
							}

							var payment = 0;
							if (player.Level[0] < nextLevel)
							{
								switch (nextLevel)
								{
									case 1:
										payment = 2;
										break;
									case 2:
										payment = 3;
										break;
									case 3:
										payment = 5;
										break;
									case 4:
										payment = 8;
										break;
									case 5:
										payment = 15;
										break;
									case 6:
										payment = 25;
										break;
									case 7:
										payment = 40;
										break;
									case 8:
										payment = 70;
										break;
									case 9:
										payment = 120;
										break;
									case 10:
										payment = 200;
										break;
									case 11:
										payment = 350;
										break;
									case 12:
										payment = 600;
										break;
									case 13:
										payment = 1000;
										break;
									case 14:
										payment = 1700;
										break;
									case 15:
										payment = 3000;
										break;
									case 16:
										payment = 5000;
										break;
									case 17:
										payment = 8300;
										break;
									case 18:
										payment = 14000;
										break;
									case 19:
										payment = 24000;
										break;
									case 20:
										payment = 40000;
										break;
								}

								if (nextLevel == 20)
								{
									player.Level[0] = 20;

									AppendText(new string[] { $"당신은 최고 레벨에 도달했습니다.",
									"더 이상 저희들은 가르칠 필요가 없습니다." });

									ContinueText.Visibility = Visibility.Visible;

									mTrainingEnd = true;
								}
								else if (mParty.Gold < payment)
								{
									AppendText(new string[] { $"당신은 금 {payment - mParty.Gold}개가 더 필요합니다." });

									ContinueText.Visibility = Visibility.Visible;

									mTrainingEnd = true;
								}
								else
								{
									mParty.Gold -= payment;

									AppendText(new string[] { $"[color={RGB.White}]{player.Name}의 레벨은 {nextLevel}입니다." });

									player.Level[0] = nextLevel;

									if (player.Class == 1)
									{
										if (player.Luck > mRand.Next(30))
										{
											if (player.Strength < 20)
												player.Strength++;
											else if (player.Endurance < 20)
												player.Endurance++;
											else if (player.Accuracy[0] < 20)
												player.Accuracy[0]++;
											else
												player.Agility++;
										}
									}
									else if (player.Class == 2 || player.Class == 9)
									{
										player.Level[1] = nextLevel;
										player.Level[2] = (int)Math.Round((double)nextLevel / 2);

										if (player.Luck > mRand.Next(30))
										{
											if (player.Mentality < 20)
												player.Mentality++;
											else if (player.Concentration < 20)
												player.Concentration++;
											else if (player.Accuracy[1] < 20)
												player.Accuracy[1]++;
										}
									}
									else if (player.Class == 3)
									{
										player.Level[1] = (int)Math.Round((double)nextLevel / 2);
										player.Level[2] = nextLevel;

										if (player.Luck > mRand.Next(30))
										{
											if (player.Concentration < 20)
												player.Concentration++;
											else if (player.Accuracy[2] < 20)
												player.Accuracy[2]++;
											else if (player.Mentality < 20)
												player.Mentality++;
										}
									}
									else if (player.Class == 4)
									{
										if (nextLevel < 16)
											player.Level[1] = nextLevel;
										else
											player.Level[1] = 16;

										if (player.Luck > mRand.Next(30))
										{
											if (player.Strength < 20)
												player.Strength++;
											else if (player.Mentality < 20)
												player.Mentality++;
											else if (player.Accuracy[0] < 20)
												player.Accuracy[0]++;
											else if (player.Accuracy[1] < 20)
												player.Accuracy[1]++;
										}
									}
									else if (player.Class == 5)
									{
										player.WeaPower = player.Level[0] * 2 + 10;

										if (player.Luck > mRand.Next(30))
										{
											if (player.Strength < 20)
												player.Strength++;
											else if (player.Accuracy[1] < 20)
												player.Accuracy[1]++;
											else if (player.Endurance < 20)
												player.Endurance++;
										}
									}
									else if (player.Class == 6)
									{
										player.Level[1] = (int)Math.Round((double)nextLevel / 2);
										player.Level[2] = nextLevel;

										if (player.Luck > mRand.Next(30))
										{
											if (player.Resistance < 18)
												player.Resistance++;
											else if (player.Resistance < 20)
											{
												if (player.Luck > mRand.Next(21))
													player.Resistance++;
											}
											else
												player.Agility++;
										}
									}
									else if (player.Class == 7 || player.Class == 8)
									{
										if (player.Luck > mRand.Next(30))
										{
											if (player.Endurance < 20)
												player.Endurance++;
											else if (player.Strength < 20)
												player.Strength++;
											else
												player.Agility++;
										}
									}
									else if (player.Class == 10)
									{
										player.Level[1] = nextLevel;
										player.Level[2] = nextLevel;

										if (player.Strength < 20)
											player.Strength++;
										if (player.Mentality < 20)
											player.Mentality++;
										if (player.Concentration < 20)
											player.Concentration++;
										if (player.Endurance < 20)
											player.Endurance++;
										else if (player.Agility < 20)
											player.Agility++;

										for (var i = 0; i < player.Accuracy.Length; i++)
										{
											if (player.Accuracy[i] < 20)
												player.Accuracy[i]++;
										}
									}

									ContinueText.Visibility = Visibility.Visible;

									mTrainingEnd = true;
								}
							}
							else
							{
								var needExp = new string[]
								{
									"1500",
									"6000",
									"20000",
									"50000",
									"150000",
									"250000",
									"500000",
									"800000",
									"1050000",
									"1320000",
									"1620000",
									"1950000",
									"2310000",
									"2700000",
									"3120000",
									"3570000",
									"4050000",
									"4560000",
									"5100000"
								};

								AppendText(new string[] { $" 당신은 아직 전투 경험이 부족합니다.",
								"",
								"",
								$"당신이 다음 레벨이 되려면 경험치가 {needExp[player.Level[0] - 1]} 이상 이어야 합니다."
								});

								ContinueText.Visibility = Visibility.Visible;

								mTrainingEnd = true;
							}
						}
						else if (mMenuMode == MenuMode.ConfirmExitMap) {
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0) {
								if ((mParty.Etc[30] & 1) == 0) {
									AppendText(new string[] {
										" 당신이 로어 성을 떠나려는 순간 누군가가 당신을 불렀다."
									});
									
									mTalkMode = 1;
									mTalkX = mParty.XAxis;
									mTalkY = mParty.YAxis;
									mSpecialEvent = 2;
									
									ContinueText.Visibility = Visibility.Visible;
								}
							}
							else {
								if (mParty.Map == 6)
								{
									AppendText(new string[] { "" });
									mParty.YAxis--;
								}
							}
						}
						else if (mMenuMode == MenuMode.JoinSkeleton) {
							mMenuMode = MenuMode.None;


							if (mMenuFocusID == 1) {
								ShowNoThanks();
								mSpecialEvent = 3;
							}
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		private void GoWeaponShop()
		{
			AppendText(new string[] {
						$"[color={RGB.White}]여기는 무기상점입니다.[/color]",
						$"[color={RGB.White}]우리들은 무기, 방패, 갑옷을 팔고있습니다.[/color]",
						$"[color={RGB.White}]어떤 종류를 원하십니까 ?[/color]"
					});

			ShowMenu(MenuMode.WeaponType, new string[]
			{
					"무기류",
					"방패류",
					"갑옷류"
			});
		}

		private void GoHospital()
		{
			AppendText(new string[] {
						$"[color={RGB.White}]여기는 병원입니다.[/color]",
						$"[color={RGB.White}]누가 치료를 받겠습니까 ?[/color]"
					});

			ShowCharacterMenu(MenuMode.Hospital);
		}
		private void GoTrainingCenter()
		{
			AppendText(new string[] {
						$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
						$"[color={RGB.White}] 만약 당신이 충분한 전투 경험을 쌓았다면, 당신은 더욱 능숙하게 무기를 다룰것입니다.[/color]",
						"",
						$"[color={RGB.White}]누가 훈련을 받겠습니까 ?[/color]"
					});

			ShowCharacterMenu(MenuMode.TrainingCenter);
		}

		private void Rest()
		{
			var append = false;
			mPlayerList.ForEach(delegate (Lore player)
			{
				if (mParty.Food <= 0)
					AppendText(new string[] { $"[color={RGB.Red}]일행은 식량이 바닥났다[/color]" }, append);
				else if (player.Dead > 0)
					AppendText(new string[] { $"{player.Name}는 죽었다" }, append);
				else if (player.Unconscious > 0 && player.Poison == 0)
				{
					player.Unconscious = player.Unconscious - player.Level[0] - player.Level[1] - player.Level[2];
					if (player.Unconscious <= 0)
					{
						AppendText(new string[] { $"{player.Name}는 의식이 회복되었다" }, append);
						player.Unconscious = 0;
						if (player.HP <= 0)
							player.HP = 1;

						mParty.Food--;
					}
					else
						AppendText(new string[] { $"[color={RGB.White}]{player.Name}는 의식이 회복되었다[/color]" }, append);
				}
				else if (player.Unconscious > 0 && player.Poison > 0)
					AppendText(new string[] { $"독때문에 {player.Name}의 의식은 회복되지 않았다" }, append);
				else if (player.Poison > 0)
					AppendText(new string[] { $"독때문에 {player.Name}의 건강은 회복되지 않았다" }, append);
				else
				{
					var recoverPoint = (player.Level[0] + player.Level[1] + player.Level[2]) * 2;
					if (player.HP >= player.Endurance * player.Level[0])
					{
						if (mParty.Food < 255)
							mParty.Food++;
					}

					player.HP += recoverPoint;

					if (player.HP >= player.Endurance * player.Level[0])
					{
						player.HP = player.Endurance * player.Level[0];

						AppendText(new string[] { $"[color={RGB.White}]{player.Name}는 모든 건강이 회복되었다[/color]" }, append);
					}
					else
						AppendText(new string[] { $"[color={RGB.White}]{player.Name}는 모든 건강이 회복되었다[/color]" }, append);

					mParty.Food--;
				}

				if (append == false)
					append = true;
			});

			if (mParty.Etc[0] > 0)
				mParty.Etc[0]--;

			for (var i = 1; i < 4; i++)
				mParty.Etc[i] = 0;

			mPlayerList.ForEach(delegate (Lore player)
			{
				player.SP = player.Mentality * player.Level[1];
				player.ESP = player.Concentration * player.Level[2];
			});

			UpdatePlayersStat();
			ContinueText.Visibility = Visibility.Visible;
		}

		private void InvokeSpecialEvent() {
			void InvokeSpeicalEventType1() {
				if (mParty.Map == 6) {
					if (mParty.XAxis == 61 && mParty.YAxis == 81) {
						AppendText(new string[] { "일행은 상자 속에서 약간의 금을 발견했다." });
						mParty.Gold += 1000;
						mMapLayer[61 + mMapWidth * 81] = 44;
					}
					else if ((mParty.XAxis == 50 && mParty.YAxis == 11) || (mParty.XAxis == 51 && mParty.YAxis == 11)) {
						if ((mParty.Etc[50] & (1 << 1)) > 0) {
							var enemyNumber = 0;

							if ((mParty.Etc[50] & (1 << 2)) > 0) {
								AppendText(new string[] { $" 다시 돌아오다니, {mPlayerList[0].Name}",
									" 이번에는 기어이 네놈들을 해치우고야 말겠다. 나의 친구들도 이번에 거들것이다."
								});

								enemyNumber = 7;
							}
							else {
								AppendText(new string[] { $" 아니! 당신이 우리들을 배신하고 죄수를 풀어주다니... 그렇다면 우리들은 결투로서 당신들과 승부할수 밖에 없군요."
								});

								mParty.Etc[50] |= (1 << 2);

								enemyNumber = 2;
							}

							// 적 정보 추가
						}
					}
					else if (mParty.XAxis == 40 && mParty.YAxis == 78)
					{
						if ((mParty.Etc[49] & (1 << 3)) == 0) {
							mParty.Etc[49] |= (1 << 3);

							mMapLayer[40 + mMapWidth * 78] = 44;

							mMoveEvent = true;

							for (var i = 0; i < 3; i++)
							{
								Task.Delay(500).Wait();
								mParty.XAxis--;
							}

							AppendText(new string[] { $"  일행은 가장 기본적인 무기로  모두  무장을 하였다." });
							
							mPlayerList.ForEach(delegate(Lore player) {
								if (player.Weapon == 0 && player.Class != 5) {
									player.Weapon = 1;
									player.WeaPower = 5;
								}
							});

							ContinueText.Visibility = Visibility.Visible;

							mMoveEvent = false;
						}
					}
					else
						ShowExitMenu();
				}
			}

			void InvokeSpeicalEventType2() {

			}

			if (mParty.Map < 17)
				InvokeSpeicalEventType1();
			else
				InvokeSpeicalEventType2();
		}

		private void HealOne(Lore whomPlayer)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0 || whomPlayer.Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 치료될 상태가 아닙니다." }, true);
			}
			else if (whomPlayer.HP >= whomPlayer.Endurance * whomPlayer.Level[0])
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 치료할 필요가 없습니다.." }, true);
			}
			else
			{
				var needSP = 2 * mMagicPlayer.Level[1];
				if (mMagicPlayer.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					mMagicPlayer.SP -= needSP;
					whomPlayer.HP += needSP * 3 / 2;
					if (whomPlayer.HP > whomPlayer.Level[0] * mMagicWhomPlayer.Endurance)
						whomPlayer.HP = whomPlayer.Level[0] * mMagicWhomPlayer.Endurance;

					AppendText(new string[] { $"[color={RGB.White}]{whomPlayer.Name}(은)는 치료되어 졌습니다.[/color]" }, true);
				}
			}
		}

		private void CureOne(Lore whomPlayer)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 독이 치료될 상태가 아닙니다." }, true);
			}
			else if (whomPlayer.Poison == 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 독에 걸리지 않았습니다." }, true);
			}
			else if (whomPlayer.SP < 15)
			{
				if (mParty.Etc[5] == 0)
					ShowNotEnoughSP();

			}
			else
			{
				mMagicPlayer.SP -= 15;
				whomPlayer.Poison = 0;

				AppendText(new string[] { $"[color={RGB.White}]{whomPlayer.Name}의 독은 제거 되었습니다.[/color]" }, true);
			}
		}

		private void ConsciousOne(Lore whomPlayer)
		{
			if (whomPlayer.Dead > 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 의식이 돌아올 상태가 아닙니다." }, true);
			}
			else if (whomPlayer.Unconscious == 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 의식불명이 아닙니다." }, true);
			}
			else
			{
				var needSP = 10 * whomPlayer.Unconscious;
				if (mMagicPlayer.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					mMagicPlayer.SP -= needSP;
					whomPlayer.Unconscious = 0;
					if (whomPlayer.HP <= 0)
						whomPlayer.HP = 1;

					AppendText(new string[] { $"{whomPlayer.Name}(은)는 의식을 되찾았습니다." }, true);
				}
			}
		}

		private void RevitalizeOne(Lore whomPlayer)
		{
			if (whomPlayer.Dead == 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{whomPlayer.Name}(은)는 아직 살아 있습니다." }, true);
			}
			else
			{
				var needSP = mMagicPlayer.Dead * 30;
				if (mMagicPlayer.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					mMagicPlayer.SP -= needSP;
					whomPlayer.Dead = 0;
					if (whomPlayer.Unconscious > whomPlayer.Endurance * whomPlayer.Level[0])
						whomPlayer.Unconscious = whomPlayer.Endurance * whomPlayer.Level[0];

					if (whomPlayer.Unconscious == 0)
						whomPlayer.Unconscious = 1;

					AppendText(new string[] { $"{whomPlayer.Name}(은)는 다시 생명을 얻었습니다." }, true);

				}
			}
		}

		private void HealAll()
		{
			mPlayerList.ForEach(delegate (Lore player)
			{
				HealOne(player);
			});
		}

		private void CureAll()
		{
			mPlayerList.ForEach(delegate (Lore player)
			{
				CureOne(player);
			});
		}

		private void ConsciousAll()
		{
			mPlayerList.ForEach(delegate (Lore player)
			{
				ConsciousOne(player);
			});
		}

		private void RevitalizeAll()
		{
			mPlayerList.ForEach(delegate (Lore player)
			{
				RevitalizeOne(player);
			});
		}

		private void ShowNotEnoughSP()
		{
			AppendText(new string[] { "그러나, 마법 지수가 충분하지 않습니다." }, true);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void ShowNotEnoughESP()
		{
			AppendText(new string[] { "ESP 지수가 충분하지 않습니다." }, true);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void ShowNotEnoughMoney()
		{
			AppendText(new string[] { "당신은 충분한 돈이 없습니다." }, true);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void ShowThankyou()
		{
			AppendText(new string[] { "매우 고맙습니다." }, true);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void ShowNoThanks() {
			AppendText(new string[] { "당신이 바란다면 ..." });
			ContinueText.Visibility = Visibility.Visible;
		}

		private bool IsAvailableMember(Lore player)
		{
			if (player.Unconscious == 0 && player.Dead == 0 && player.HP >= 0)
				return true;
			else
				return false;
		}

		private string GetGenderData(Lore player)
		{
			if (player.Gender == "male")
				return "그";
			else
				return "그녀";
		}

		private void ShowCharacterMenu(MenuMode menuMode)
		{
			AppendText(new string[] { "[color=LightGreen]한명을 고르시오 ---[/color]" }, true);

			var menuStr = new string[mPlayerList.Count];
			for (var i = 0; i < mPlayerList.Count; i++)
				menuStr[i] = mPlayerList[i].Name;

			ShowMenu(menuMode, menuStr);
		}

		private void FocusMenuItem()
		{
			for (var i = 0; i < mMenuCount; i++)
			{
				if (i == mMenuFocusID)
					mMenuList[i].Foreground = new SolidColorBrush(Colors.Yellow);
				else
					mMenuList[i].Foreground = new SolidColorBrush(Colors.LightGray);
			}
		}

		private void ShowMenu(MenuMode menuMode, string[] menuItem)
		{
			mMenuMode = menuMode;
			mMenuCount = menuItem.Length;
			mMenuFocusID = 0;

			for (var i = 0; i < mMenuList.Count; i++)
			{
				if (i < mMenuCount)
				{
					mMenuList[i].Text = menuItem[i];
					mMenuList[i].Visibility = Visibility.Visible;
				}
				else
					mMenuList[i].Visibility = Visibility.Collapsed;
			}

			
			FocusMenuItem();
		}

		private void HideMenu()
		{
			for (var i = 0; i < mMenuCount; i++)
			{
				mMenuList[i].Visibility = Visibility.Collapsed;
			}
		}

		private void AppendText(string[] text, bool append = false)
		{
			var totalLen = 0;

			if (append)
			{
				foreach (Paragraph prevParagraph in DialogText.Blocks)
				{
					foreach (Run prevRun in prevParagraph.Inlines)
					{
						totalLen += prevRun.Text.Length;
					}
				}
			}
			else
			{
				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();
			}

			var paragraph = new Paragraph();
			DialogText.Blocks.Add(paragraph);

			for (var i = 0; i < text.Length; i++)
			{
				string str;
				if (i == 0)
					str = text[i];
				else
					str = "\r\n" + text[i];

				var startIdx = 0;
				while ((startIdx = str.IndexOf("[")) >= 0)
				{
					var preRun = new Run();
					preRun.Text = str.Substring(0, startIdx);
					
					paragraph.Inlines.Add(preRun);
					DialogText.TextHighlighters.Add(new TextHighlighter()
					{
						Foreground = new SolidColorBrush(Colors.LightGray),
						Background = new SolidColorBrush(Colors.Transparent),
						Ranges = { new TextRange()
							{
								StartIndex = totalLen,
								Length = preRun.Text.Length
							}
						}
					});

					totalLen += preRun.Text.Length;
					str = str.Substring(startIdx + 1);

					startIdx = str.IndexOf("]");
					if (startIdx < 0)
						break;

					var tag = str.Substring(0, startIdx);
					str = str.Substring(startIdx + 1);
					var tagData = tag.Split("=");

					var endTag = $"[/{tagData[0]}]";
					startIdx = str.IndexOf(endTag);

					if (startIdx < 0)
						break;


					if (tagData[0] == "color" && tagData.Length > 1 && tagData[1].Length == 6)
					{
						var tagRun = new Run();
						tagRun.Text = str.Substring(0, startIdx);

						paragraph.Inlines.Add(tagRun);
						DialogText.TextHighlighters.Add(new TextHighlighter()
						{
							Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(tagData[1].Substring(0, 2), 16), Convert.ToByte(tagData[1].Substring(2, 2), 16), Convert.ToByte(tagData[1].Substring(4, 2), 16))),
							Background = new SolidColorBrush(Colors.Transparent),
							Ranges = { new TextRange()
											{
												StartIndex = totalLen,
												Length = tagRun.Text.Length
											}
										}
						});

						totalLen += tagRun.Text.Length;
					}

					str = str.Substring(startIdx + endTag.Length);
				}

				var run = new Run();
				run.Text = str;

				paragraph.Inlines.Add(run);
				DialogText.TextHighlighters.Add(new TextHighlighter()
				{
					Foreground = new SolidColorBrush(Colors.LightGray),
					Background = new SolidColorBrush(Colors.Transparent),
					Ranges = { new TextRange()
						{
							StartIndex = totalLen,
							Length = run.Text.Length
						}
					}
				});

				totalLen += run.Text.Length;
			}
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			void GoGrocery()
			{
				AppendText(new string[] {
						$"[color={RGB.White}]여기는 식료품점 입니다.[/color]",
						$"[color={RGB.White}]몇개를 원하십니까 ?[/color]"
					});


				var foodMenuItem = new string[5];
				for (var i = 0; i < foodMenuItem.Length; i++)
					foodMenuItem[i] = $"{(i + 1) * 10} 인분 : 금 {(i + 1) * 100} 개";

				ShowMenu(MenuMode.Grocery, foodMenuItem);
				mKeyDownShowMenu = true;
			}

			if (mParty.Map == 6)
			{
				if (moveX == 8 && moveY == 63)
				{
					AppendText(new string[] {
						" 당신이 모험을 시작한다면, 많은 괴물들을 만날 것이오.",
						"무엇보다도, 써펀트와 인젝트들과 파이썬은 맹독이 있으니 주의 하시기 바라오."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 71 && moveY == 72)
				{
					AppendText(new string[] { "오크는 가장 하급 괴물이오." });

					ContinueText.Visibility = Visibility;
				}
				else if (moveX == 50 && moveY == 71)
				{
					if ((mParty.Etc[49] & (1 << 4)) == 0)
					{
						AppendText(new string[] { " 당신이  네크로맨서에 진정으로  대항하고자 한다면, 이 성의 바로위에 있는 피라밋에 가보도록하시오." +
							$"그 곳은 네크로맨서와 동시에 바다에서 떠오른 [color={RGB.LightCyan}]또다른 지식의 성전[/color]이기 때문이오." +
							"  당신이 어느 수준이 되어 그 곳에 들어간다면  진정한 이 세계의 진실을 알수 있을것이오." });

						mSpecialEvent = 1;
						ContinueText.Visibility = Visibility.Visible;
					}
					else
					{
						AppendText(new string[] { " '메나스' 속에는 드워프, 자이언트, 울프, 파이썬 같은 괴물들이 살고 있소." });

						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (moveX == 57 && moveY == 73)
				{
					AppendText(new string[] { " 나의 부모님은 파이썬 의 독에 의해 돌아 가셨습니다. 파이썬은 정말 위험한 존재입니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 62 && moveY == 26)
				{
					AppendText(new string[] {
						" 단지 로드 안만이 능력 상으로 네크로맨서에게 도전할 수 있습니다.",
						" 하지만 로드 안 자신이 대립을 싫어해서, 현재는 네크로맨서에게 대항할 자가 없습니다."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 89 && moveY == 81)
				{
					AppendText(new string[] { " 우리는 에이션트 이블을 배척하고 로드 안님을 받들어야 합니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 93 && moveY == 67)
				{
					AppendText(new string[] { " 우리는 메나스의 동쪽에 있는 나무로부터 많은 식량을 얻은적이 있습니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 18 && moveY == 52)
				{
					AppendText(new string[] { $"[color={RGB.LightGreen}] 이 세계의 창시자는 안 영기님 이시며, 그는 위대한 프로그래머 입니다.[/color]" });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((moveX == 12 && moveY == 26) || (moveX == 17 && moveY == 26))
				{
					AppendText(new string[] { " 어서 오십시오. 여기는 로어 주점입니다.",
						mRand.Next(2) == 0 ?  $"거기 {Common.GetGenderStr(mPlayerList[0])}분 어서 오십시오." : " 위스키에서 칵테일까지 마음껏 선택하십시오."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 20 && moveY == 32)
				{
					AppendText(new string[] { "..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 9 && moveY == 29)
				{
					AppendText(new string[] { "요새 무덤쪽에서 유령이 떠돈다던데..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 12 && moveY == 31)
				{
					AppendText(new string[] { "하하하, 자네도 한번 마셔보게나." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 14 && moveY == 34)
				{
					AppendText(new string[] { " 이제 로드 안의 시대도 끝나가는가 ? 그까짓 네크로맨서라는 작자에게 쩔쩔 매는 꼴이라니 ...  차라리 내가 나가서 그 놈과 싸우는게 났겠다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 17 && moveY == 32)
				{
					AppendText(new string[] { " 당신은 스켈레톤족의 한명이 우리와 함께 생활하려 한다는 것에 대해서 어떻게 생각하십니까 ?",
					" 저는 그 말을 들었을때 너무 혐오스러웠습니다. 어서 빨리 그 살아있는 뼈다귀를 여기서 쫒아냈으면 좋겠습니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 20 && moveY == 35)
				{
					AppendText(new string[] { " ... 끄~~윽 ... ..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 17 && moveY == 37)
				{
					AppendText(new string[] { " 이보게 자네, 내말 좀 들어 보게나.  나의 친구들은 이제 이 세상에 없다네. 그들은 너무나도 용감하고 믿음직스런 친구들이었는데..." +
						"내가 다리를 다쳐 병원에 있을 동안 그들은 모두 이 대륙의 평화를 위해 로어 특공대에 지원 했다네.  하지만 그들은 아무도 다시는 돌아오지못했어." +
						" 그런 그들에게 이렇게 살아있는 나로서는 미안할 뿐이네  그래서 술로 나날을 보내고 있지. 죄책감을 잊기위해서 말이지..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 71 && moveY == 77)
				{
					AppendText(new string[] { " 물러나십시오.  여기는 용사의 유골들을 안치해 놓은 곳입니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 62 && moveY == 75 && (mParty.Etc[49] & 1) == 0)
				{
					if (mTalkMode == 0)
					{
						AppendText(new string[] { " 당신이  한 유골 앞에 섰을때  이상한 느낌과 함께 먼곳으로 부터 어떤 소리가 들려왔다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 안녕하시오. 대담한 용사여.[/color]",
							$"[color={RGB.LightMagenta}] 당신이 나의 잠을 깨웠소 ?  나는 고대에 이곳을 지키다가 죽어간 기사 Jr. Antares 라고 하오." +
							"  저의 아버지는 Red Antares 라고 불리웠던 최강의 마법사였소.  그는 말년에  어떤 동굴로 은신을 한 후 아무에게도 모습을 나타내지 않았소." +
							"  하지만 당신의 운명은 나의 아버지를 만나야만하는 운명이라는 것을 알수있소.  반드시 나의 아버지를 만나서 당신이 알지 못했던 새로운 능력들을 배우시오." +
							" 그리고 나의 아버지를 당신의 동행으로 참가시키도록 하시오. 물론 좀 어렵겠지만 ...[/color]" });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 아참,  그리고 내가 죽기전에 여기에 뭔가를 여기에 숨겨 두었는데  당신에게 도움이 될지모르겠소. 그럼, 나는 다시 오랜 잠으로 들어가야 겠소.[/color]" });

						mMapLayer[61 + mMapWidth * 78] = 44;
						mMapLayer[61 + mMapWidth * 79] = 44;
						mMapLayer[61 + mMapWidth * 80] = 44;
						mMapLayer[61 + mMapWidth * 81] = 0;
						mMapLayer[61 + mMapWidth * 82] = 14;

						ContinueText.Visibility = Visibility.Visible;

						mParty.Etc[49] |= 1;

						mTalkMode = 0;
					}
				}
				else if (moveX == 23 && moveY == 49)
				{
					AppendText(new string[] { $"힘내게, {mPlayerList[0].Name}",
					"자네라면 충분히 네크로맨서를 무찌를수 있을 걸세. 자네만 믿겠네."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 23 && moveY == 53)
				{
					AppendText(new string[] { $" 위의 저 친구로부터  당신 얘기 많이 들었습니다. 저는 우리성에서 당신같은 용감한 사람이 있다는걸 자랑스럽게 생각합니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 12 || moveY == 54)
				{
					AppendText(new string[] { $" 만약, 당신들이  그 일을 해내기가 어렵다고 생각되시면 라스트디치 성에서  성문을 지키고있는 Polaris란 청년을 일행에 참가시켜 주십시오." +
						"  분명 그 사람이라면 쾌히 승락할 겁니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 49 && moveY == 10)
				{
					AppendText(new string[] { $" 이 안에 갇혀있는 사람들에게는 일체 면회가 허용되지 않습니다. 나가 주십시오." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 52 && moveY == 10)
				{
					AppendText(new string[] { $" 여기는 로드 안의 체제에 대해서 깊은 반감을 가지고 있는 자들을 수용하고 있습니다.",
					" 아마 그들은 죽기전에는 이곳을 나올수 없을겁니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 40 && moveY == 9)
				{
					if (mTalkMode == 0)
					{
						AppendText(new string[] { $" 나는 이곳의 기사로서  이 세계의 모든 대륙을 탐험하고 돌아왔었습니다." +
						" 내가 마지막 대륙을 돌았을때  나는 새로운 존재를 발견했습니다. 그는 바로 예전까지도 로드 안과 대립하던 에이션트 이블이라는 존재였습니다." +
						" 지금 우리의 성에서는 철저하게 배격하도록 어릴때부터 가르침 받아온 그 에이션트 이블이었습니다." +
						"  하지만 그곳에서 본 그는 우리가 알고있는 그와는 전혀 다른 인간미를 가진  말 그대로  신과같은 존재였습니다." +
						"내가 그의 신앙아래 있는 어느 도시를 돌면서 내가 느낀것은 정말 로드 안에게서는 찾아볼수가 없는 그런 자애와 따뜻한 정이었습니다." +
						"  그리고 여태껏 내가 알고 있는 그에 대한 지식이  정말 잘못되었다는 것과  이런 사실을 다른 사람에게도 알려주고 싶다는 이유로  그의 사상을 퍼뜨리다 이렇게 잡히게 된것입니다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { " 하지만 더욱 이상한것은 로드 안 자신도 그에 대한 사실을 인정하면서도  왜 우리에게는 그를 배격하도록만 교육시키는 가를  알고 싶을뿐입니다." +
							"로드 안께서는 나를 이해한다고 하셨지만 사회 혼란을 방지하기 위해 나를 이렇게 밖에 할수 없다고 말씀하시더군요. 그리 이것은 선을 대표하는 자기로서는 이 방법 밖에는 없다고 하시더군요." +
							" 하지만 로드 안의 마음은 사실 이렇지 않다는걸 알수 있었습니다.  에이션트 이블의 말로는 사실 서로가 매우 절친한 관계임을 알수가 있었기 때문입니다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 0;
					}

				}
				else if (moveX == 39 && moveY == 14)
				{
					AppendText(new string[] { " 히히히... 위대한 용사님. 낄낄낄.. 내가 당신들의 일행에 끼이면 안될까요 ? 우히히히.." });

					ShowMenu(MenuMode.JoinMadJoe, new string[]
					{
						"그렇다면 당신을 받아들이지요",
						"당신은 이곳에 그냥 있는게 낫겠소"
					});
				}
				else if (moveX == 62 && moveY == 9)
				{
					AppendText(new string[] { " 안녕하시오. 나는 한때 이 곳의 유명한 도둑이었던 사람이오.  결국 그 때문에 나는 잡혀서 평생 여기에 있게 되었지만...",
					$" 그건 그렇고, 내가 로어 성의 보물인 [color={RGB.LightCyan}]황금의 방패[/color]를 훔쳐 달아나다. 그만 그것을 메나스라는 금광에 숨겨 놓은채 잡혀 버리고 말았소.",
					"나는 이제 그것을 가져봤자 쓸때도 없으니 차라리 당신이 그걸 가지시오. 가만있자...  어디였더라...  그래 ! 메나스의 가운데쯤에 벽으로 사방이 둘러 싸여진 곳이었는데.." +
					"  당신들이라면  지금 여기에 들어온것과 같은 방법으로 들어가서 방패를 찾을수 있을것이오. 행운을 빌겠소."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 59 && moveY == 14)
				{
					AppendText(new string[] { " 당신들에게 경고해 두겠는데 건너편 방에 있는 Joe는 오랜 수감생활 끝에 미쳐 버리고 말았소.  그의 말에 속아서 당신네 일행에 참가시키는 그런 실수는 하지마시오." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((moveX == 41 && moveY == 77) || (moveX == 41 && moveY == 79))
				{
					if ((mParty.Etc[49] & (1 << 3)) == 0)
						AppendText(new string[] { " 로드안 님의 명령에 의해서 당신들에게 한가지의 무기를 드리겠습니다.  들어가셔서 무기를 선택해 주십시오." });
					else
						AppendText(new string[] { " 여기서 가져가신 무기를 잘 사용하셔서 세계의 적인 네크로맨서를 무찔러 주십시오." });

					ContinueText.Visibility = Visibility.Visible;

				}
				else if (moveX == 50 && moveY == 13)
				{
					AppendText(new string[] { "메나스 에는 금덩이가 많다던데..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 82 && moveY == 26)
				{
					AppendText(new string[] { "메나스 는 한때 금광이었습니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((moveX == 86 && moveY == 72) || (moveX == 90 && moveY == 64))
					GoGrocery();
				else if ((moveX == 7 && moveY == 72) || (moveX == 13 && moveY == 68) || (moveX == 13 && moveY == 72))
				{
					GoWeaponShop();
					mKeyDownShowMenu = true; ;
				}
				else if ((moveX == 86 && moveY == 13) || (moveX == 85 && moveY == 11))
				{
					GoHospital();
					mKeyDownShowMenu = true;
				}
				else if ((moveX == 20 && moveY == 11) || (moveX == 24 && moveY == 12))
				{
					GoTrainingCenter();
					mKeyDownShowMenu = true;
				}
				else if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
				{
					if (mTalkMode == 0)
					{
						if ((mParty.Etc[29] & 1) == 1)
							AppendText(new string[] { "행운을 빌겠소 !!!" });
						else if (mParty.Etc[9] < 3)
							AppendText(new string[] { "저희 성주님을 만나 보십시오." });
						else
						{
							AppendText(new string[] { "당신은 이 게임 세계에 도전하고 싶습니까 ?",
							AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" ? "(A: 예 / B: 아니오)" : "(Y/n)" });

							mTalkMode = 1;
							mTalkX = moveX;
							mTalkY = moveY;

							mSpecialEvent = 4;
						}
					}
					else if (mTalkMode == 1 && (key == VirtualKey.Y || key == VirtualKey.GamepadA))
					{
						mMapLayer[48 + mMapWidth * 51] = 47;
						mMapLayer[49 + mMapWidth * 51] = 44;
						mMapLayer[50 + mMapWidth * 51] = 44;
						mMapLayer[51 + mMapWidth * 51] = 44;
						mMapLayer[52 + mMapWidth * 51] = 47;
						mMapLayer[48 + mMapWidth * 52] = 47;
						mMapLayer[49 + mMapWidth * 52] = 44;
						mMapLayer[50 + mMapWidth * 52] = 44;
						mMapLayer[51 + mMapWidth * 52] = 44;
						mMapLayer[52 + mMapWidth * 52] = 45;

						AppendText(new string[] { "",
						"예.",
						"",
						"이제부터 당신은 진정한 이 세계에 발을 디디게 되는 것입니다." }, true);

						ContinueText.Visibility = Visibility.Visible;
						mParty.Etc[29] = mParty.Etc[29] | 1;

						mTalkMode = 0;
						mSpecialEvent = 0;
					}
					else if (mTalkMode == 1 && (key == VirtualKey.N || key == VirtualKey.GamepadB))
					{
						AppendText(new string[] { "",
						"아니오.",
						"",
						"다시 생각 해보십시오." }, true);

						ContinueText.Visibility = Visibility.Visible;
						mTalkMode = 0;
						mSpecialEvent = 0;
					}
				}
				else if (moveX == 50 && moveY == 86)
				{
					if ((mParty.Etc[29] & (1 << 1)) == 0)
					{
						for (var i = 48; i < 53; i++)
							mMapLayer[i + mMapWidth * 87] = 44;

						AppendText(new string[] { $"난 당신을 믿소, {mPlayerList[0].Name}." });

						ContinueText.Visibility = Visibility.Visible;
						mParty.Etc[29] |= 1 << 1;
					}
					else
					{
						AppendText(new string[] { $"힘내시오, {mPlayerList[0].Name}." });
						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (47 <= moveX && moveX <= 53 && 30 <= moveY && moveY <= 36)
				{
					if (mParty.Etc[9] == 0)
						AppendText(new string[] { "저희 성주님을 만나십시오." });
					else
						AppendText(new string[] { "당신이 성공하기를 빕니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 50 && moveY == 27)
				{
					if (mTalkMode == 0)
					{
						if (mParty.Etc[9] == 0)
						{
							AppendText(new string[] { $"나는 [color={RGB.LightCyan}]로드 안[/color]이오.",
							"이제부터 당신은 이 게임에서 새로운 인물로서 생을 시작하게 될것이오. 그럼 나의 이야기를 시작하겠소." });
							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[9]++;

							mTalkMode = 1;
							mTalkX = moveX;
							mTalkY = moveY;
						}
						else if (mParty.Etc[9] == 3)
						{
							AppendText(new string[] { $" 대륙의 남서쪽에 있는 '[color={RGB.LightCyan}]메나스[/color]'를 탐사해 주시오." });
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mParty.Etc[9] == 4)
						{
							AppendText(new string[] { "당신들의 성공을 축하하오 !!",
						"[color={RGB.LightCyan}][EXP + 1000][/color]" });

							mPlayerList.ForEach(delegate (Lore player)
							{
								player.Experience += 1000;
							});

							mParty.Etc[9]++;
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mParty.Etc[9] == 5)
						{
							AppendText(new string[] { " 드디어 나는 당신들의 능력을 믿을수 있게 되었소.  그렇다면 당신들에게 네크로맨서 응징이라는 막중한 임무를 한번 맡겨 보겠소.",
							$"먼저 대륙의 동쪽에 있는 '[color={RGB.LightCyan}]라스트디치[/color]'에 가보도록 하시오. '[color={RGB.LightCyan}]라스트디치[/color]'성에는 지금 많은 근심에 쌓여있소. 그들을 도와 주시오." });

							mParty.Etc[9]++;
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mParty.Etc[9] == 6)
						{
							AppendText(new string[] { " 당신은 이제 스스로 행동해 나가시오." });
							ContinueText.Visibility = Visibility.Visible;
						}
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { " 이 세계는 내가 통치하는 동안에는 무척 평화로운 세상이 진행되어 왔었소. 그러나 그것은 한 운명의 장난으로 무참히 깨어져 버렸소.",
						" 한날, 대기의 공간이 진동하며 난데없는 푸른 번개가 대륙들 중의 하나를 강타했소. 공간은 휘어지고 시간은 진동하며  이 세계를 공포 속으로 몰고 갔소." +
						"  그 번개의 위력으로 그 불운한 대륙은  황폐화된 용암 대지로 변하고 말았고, 다른 하나의 대륙은 충돌시의 진동에 의해 바다 깊이 가라앉아 버렸소.",
						" 그런 일이 있은 한참 후에,  이상하게도 용암대지의 대륙으로부터 강한 생명의 기운이 발산되기 시작 했소." +
						"  그래서, 우리들은 그 원인을 알아보기 위해 '로어 특공대'를 조직하기로 합의를 하고 " +
						"이곳에 있는 거의 모든 용사들을 모아서 용암 대지로 변한 그 대륙으로 급히 그들을 파견하였지만 여태껏 아무 소식도 듣지못했소. 그들이 생존해 있는지 조차도 말이오.",
						$" 이런 저런 방법을 통하여 그들의 생사를 알아려던중 우연히 우리들은 '[color={RGB.LightCyan}]네크로맨서[/color]'라고 불리우는  용암 대지속의  새로운 세력의 존재를" +
						"알아내었고,  그때의 그들은 이미 막강한 세력으로 성장해가고 있는중 이었소.  그때의 번개는 그가 이 공간으로 이동하는 수단이었소. 즉 그는 이 공간의 인물이 아닌 다른 차원을 가진공간에서 왔던 것이오."
					});
						ContinueText.Visibility = Visibility.Visible;
						mParty.Etc[9]++;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $" 그는 현재 이 세계의 반을  그의 세력권 안에 넣고 있소. 여기서 당신의 궁극적인 임무는 바로 '[color={RGB.LightCyan}]네크로맨서의 야심을 봉쇄 시키는 것[/color]'이라는 걸 명심해 두시오.",
						" 네크로맨서 의 영향력은 이미 로어 대륙까지 도달해있소.  또한 그들은 이 대륙의 남서쪽에 '메나스' 라고 불리우는 지하 동굴을 얼마전에 구축했소.  그래서, 그 동굴의 존재 때문에 우리들은 그에게 위협을 당하게 되었던 것이오.",
						" 하지만, 로어 특공대가 이 대륙을 떠난후로는 그 일당들에게 대적할 용사는  이미 남아있지 않았소. 그래서 부탁하건데, 그 동굴을 중심부까지 탐사해 주시오.",
						" 나는 당신들에게 네크로맨에 대한 일을 맡기고 싶지만, 아직은 당신들의  확실한 능력을 모르는 상태이지요.  그래서 이 일은 당신들의 잠재력을 증명해 주는 좋은 기회가 될것이오.",
						" 만약 당신들이 무기가 필요하다면 무기고에서 약간의 무기를 가져가도록 허락하겠소."
					});
						ContinueText.Visibility = Visibility.Visible;

						mParty.Etc[9]++;

						mTalkMode = 0;
					}
				}
				else if ((mParty.Etc[30] & 1) == 0 && mTalkMode == 1) {
					AppendText(new string[] { $" 나는 스켈레톤이라 불리는 종족의 사람이오.",
						" 우리 종족의 사람들은 나를 제외하고는  모두 네크로맨서에게 굴복하여 그의 부하가 되었지만 나는 그렇지 않소." +
						" 나는 네크로맨서 의 영향을 피해서 이곳 로어 성으로 왔지만 나의 혐오스런 생김새 때문에 이곳 사람들에게 배척되어서 지금은 어디로도 갈 수 없는 존재가 되었소." +
						" 이제 나에게 남은 것은 네크로맨서 의 타도 밖에 없소.  그래서  당신들의 일행에 끼고 싶소."
					});

					ShowMenu(MenuMode.JoinSkeleton, new string[] {
						"당신을 환영하오.",
						"미안하지만 안되겠소."
					});

					mTalkMode = 0;
				}
			}
		}

		private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			mSpriteBatchSupported = CanvasSpriteBatch.IsSupported(sender.Device);

			if (!mSpriteBatchSupported)
				return;

			args.TrackAsyncAction(LoadImages(sender.Device).AsAsyncAction());
		}

		private async Task LoadImages(CanvasDevice device)
		{
			try
			{
				await LoadEnemyData();
				await LoadFile();

				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.png"), new Vector2(64, 64), Vector2.Zero);
				mCharacterTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_sprite.png"), new Vector2(64, 64), Vector2.Zero);
			}
			catch (Exception e)
			{
				Debug.WriteLine($"에러: {e.Message}");
			}
		}

		private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			if (!mSpriteBatchSupported)
			{
				args.DrawingSession.DrawText("This version of Windows does not support sprite batch, so this example is not available.",
					new Rect(new Point(0, 0), sender.Size), Colors.White, new CanvasTextFormat()
					{
						HorizontalAlignment = CanvasHorizontalAlignment.Center,
						VerticalAlignment = CanvasVerticalAlignment.Center
					});
				return;
			}

			var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(64 * (mParty.XAxis - 4), 64 * (mParty.YAxis - 4)));
			args.DrawingSession.Transform = transform;

			var size = sender.Size.ToVector2();

			var options = ClampToSourceRect ? CanvasSpriteOptions.ClampToSourceRect : CanvasSpriteOptions.None;
			//var options = CanvasSpriteOptions.None;
			//var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), InterpolationMode);
			var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), CanvasImageInterpolation.HighQualityCubic.ToString());

			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, options))
			{
				lock (mapLock)
				{
					if (mMapLayer != null)
						DrawLayer(sb, mMapLayer);
				}

				if (mCharacterTiles != null) {
					mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(mParty.XAxis, mParty.YAxis), Vector4.One);

					if (mSpecialEvent == 1)
						mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(50, 71), Vector4.One);
				}
			}
		}

		void DrawLayer(CanvasSpriteBatch sb, byte[] layer)
		{
			for (int i = 0; i < layer.Length; ++i)
			{
				DrawTile(sb, layer, i);
			}
		}

		void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index)
		{
			int row = index / mMapWidth;
			int column = index % mMapWidth;

			Vector4 tint = Vector4.One;

			if (mMapTiles != null)
			{
				if (mSpecialEvent == 1 && (index == 50 + mMapWidth * 71))
					mMapTiles.Draw(sb, 44, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				else
				{
					var mapIdx = 56;

					if (mPosition == PositionType.Town)
						mapIdx = 0;
					else if (mPosition == PositionType.Ground)
						mapIdx *= 2;

					mMapTiles.Draw(sb, layer[index] + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
			}
		}

		private async Task LoadMapData() {
			var mapFileName = "";
			switch (mParty.Map)
			{
				case 1:
					mapFileName = "GROUND1";
					break;
				case 2:
					mapFileName = "GROUND2";
					break;
				case 3:
					mapFileName = "WATER";
					break;
				case 4:
					mapFileName = "SWAMP";
					break;
				case 5:
					mapFileName = "LAVA";
					break;
				case 6:
					mapFileName = "TOWN1";
					break;
				case 7:
					mapFileName = "TOWN2";
					break;
				case 8:
					mapFileName = "TOWN3";
					break;
				case 9:
					mapFileName = "TOWN4";
					break;
				case 10:
					mapFileName = "TOWN5";
					break;
				case 11:
					mapFileName = "T_DEN1";
					break;
				case 12:
					mapFileName = "T_DEN2";
					break;
				case 13:
					mapFileName = "DEN4";
					break;
				case 14:
					mapFileName = "DEN1";
					break;
				case 15:
					mapFileName = "DEN2";
					break;
				case 16:
					mapFileName = "DEN3";
					break;
				case 17:
					mapFileName = "DEN4";
					break;
				case 18:
					mapFileName = "DEN5";
					break;
				case 19:
					mapFileName = "DEN6";
					break;
				case 20:
					mapFileName = "DEN7";
					break;
				case 21:
					mapFileName = "KEEP1";
					break;
				case 22:
					mapFileName = "KEEP2";
					break;
				case 23:
					mapFileName = "KEEP3";
					break;
				case 24:
					mapFileName = "K_DEN1";
					break;
				case 25:
					mapFileName = "K_DEN2";
					break;
				case 26:
					mapFileName = "K_DEN2";
					break;
				case 27:
					mapFileName = "PYRAMID1";
					break;
			}

			var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{mapFileName}.MAP"));
			var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			var reader = new BinaryReader(stream);

			lock (mapLock)
			{
				mMapWidth = reader.ReadByte();
				mMapHeight = reader.ReadByte();

				mMapLayer = new byte[mMapWidth * mMapHeight];

				for (var i = 0; i < mMapWidth * mMapHeight; i++)
				{
					mMapLayer[i] = reader.ReadByte();
				}
			}
		}

		private void InitializeMap() {
			if (1 <= mParty.Map && mParty.Map <= 5)
				mPosition = PositionType.Ground;
			else if ((6 <= mParty.Map && mParty.Map <= 10) || mParty.Map == 24 || (26 <= mParty.Map && mParty.Map <= 27))
				mPosition = PositionType.Town;
			else if ((11 <= mParty.Map && mParty.Map <= 20) || mParty.Map == 25)
				mPosition = PositionType.Den;
			else
				mPosition = PositionType.Keep;

			if (mMapHeight / 2 > mParty.YAxis)
				mFace = 0;
			else
				mFace = 1;

			if (mPosition != PositionType.Town || mParty.Map == 26)
				mFace = mFace + 4;

			switch (mParty.Etc[11])
			{

			}
		}

		private async Task LoadEnemyData() {
			//var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/FOEDATA.DAT"));
			//var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			//var reader = new BinaryReader(stream);

			var storageFolder = ApplicationData.Current.LocalFolder;
			var enemyFileFile = await storageFolder.CreateFileAsync("enemyData.dat", CreationCollisionOption.OpenIfExists);
			mEnemyDataList = JsonConvert.DeserializeObject<List<EnemyData>>(await FileIO.ReadTextAsync(enemyFileFile));


			//var enemyList = new List<EnemyData>();

			//for (int i = 0; i < 75; i++)
			//{
			//	var strLen = reader.ReadByte();

			//	var buffer = reader.ReadBytes(16);

			//	//int strLen = 0;
			//	//while (strLen < buffer.Length && buffer[strLen] != 0)
			//	//	strLen++;

			//	var name = Encoding.UTF8.GetString(buffer, 0, strLen);
			//	Debug.WriteLine($"name : {name}");

			//	enemyList.Add(new EnemyData()
			//	{
			//		Name = name,
			//		Strength = reader.ReadByte(),
			//		Mentality = reader.ReadByte(),
			//		Endurance = reader.ReadByte(),
			//		Resistance = reader.ReadByte(),
			//		Agility = reader.ReadByte(),
			//		Accuracy = new int[] { reader.ReadByte(), reader.ReadByte() },
			//		AC = reader.ReadByte(),
			//		Special = reader.ReadByte(),
			//		CastLevel = reader.ReadByte(),
			//		SpecialCastLevel = reader.ReadByte(),
			//		Level = reader.ReadByte()
			//	});

			//	//Debug.WriteLine($"Strengh: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Mentality: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Endurance: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Resistance: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Agility: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Accuracy0: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Accuracy1: {reader.ReadByte()}");
			//	//Debug.WriteLine($"AC: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Special: {reader.ReadByte()}");
			//	//Debug.WriteLine($"CastLevel: {reader.ReadByte()}");
			//	//Debug.WriteLine($"SpecialCastLevel: {reader.ReadByte()}");
			//	//Debug.WriteLine($"Level: {reader.ReadByte()}");
			//}

			//var saveJSON = JsonConvert.SerializeObject(enemyList);

			//await FileIO.WriteTextAsync(saveFile, saveJSON);
		}

		private async Task LoadFile() {
			var storageFolder = ApplicationData.Current.LocalFolder;

			var saveFile = await storageFolder.CreateFileAsync("loreSave.dat", CreationCollisionOption.OpenIfExists);
			var saveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(saveFile));

			mParty = saveData.Party;
			mPlayerList = saveData.PlayerList;

			if (saveData.Map.Data.Length == 0)
			{
				await LoadMapData();
			}
			else {
				lock (mapLock)
				{
					mMapWidth = saveData.Map.Width;
					mMapHeight = saveData.Map.Height;

					mMapLayer = saveData.Map.Data;
				}
			}

			mEncounter = saveData.Encounter;
			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			mMaxEnemy = saveData.MaxEnemy;
			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			DisplayPlayerInfo();

			InitializeMap();
		}

		private void DisplayPlayerInfo() {
			for (var i = 0; i < 6; i++) {
				if (i < mPlayerList.Count)
				{
					mPlayerNameList[i].Text = mPlayerList[i].Name;
					mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.White);

					mPlayerACList[i].Text = mPlayerList[i].AC.ToString();
					mPlayerLevelList[i].Text = mPlayerList[i].Level[0].ToString();
				}
				else
				{
					mPlayerNameList[i].Text = "빈 슬롯";
					mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.DarkRed);

					mPlayerACList[i].Text = "";
					mPlayerLevelList[i].Text = "";
				}
			}

			DisplayHP();
			DisplaySP();
			DisplayESP();
			DisplayCondition();
		}

		private void UpdatePlayersStat()
		{
			DisplayHP();
			DisplaySP();
			DisplayESP();
			DisplayCondition();
		}

		private void DisplayHP() {
			for (var i = 0; i < 6; i++) {
				if (i < mPlayerList.Count)
					mPlayerHPList[i].Text = mPlayerList[i].HP.ToString();
				else
					mPlayerHPList[i].Text = "";
			}
		}

		private void DisplaySP()
		{
			for (var i = 0; i < 6; i++) {
				if (i < mPlayerList.Count)
					mPlayerSPList[i].Text = mPlayerList[i].SP.ToString();
				else
					mPlayerSPList[i].Text = "";
			}
		}

		private void DisplayESP()
		{
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerESPList[i].Text = mPlayerList[i].ESP.ToString();
				else
					mPlayerESPList[i].Text = "";
			}
		}

		private void DisplayCondition()
		{
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerConditionList[i].Text = GetConditionName(i);
				else
					mPlayerConditionList[i].Text = "";
			}
		}

		private string GetConditionName(int index) {
			if (mPlayerList[index].HP <= 0 && mPlayerList[index].Unconscious == 0)
				mPlayerList[index].Unconscious = 1;

			if (mPlayerList[index].Unconscious > mPlayerList[index].Endurance * mPlayerList[index].Level[0] && mPlayerList[index].Dead == 0)
				mPlayerList[index].Dead = 1;

			if (mPlayerList[index].Dead > 0)
				return "사망";

			if (mPlayerList[index].Unconscious > 0)
				return "의식불명";

			if (mPlayerList[index].Poison > 0)
				return "중독";

			return "좋음";
		}

		private bool EnterWater() {
			if (mParty.Etc[1] > 0)
			{
				mParty.Etc[1]--;

				if (mRand.Next(mEncounter * 30) == 0)
					EncounterEnemy();

				return true;
			}
			else
				return false;
		}

		private void EncounterEnemy() {

		}

		private void ShowExitMenu() {
			AppendText(new string[] { $"[color={RGB.LightCyan}]여기서 나가기를 원합니까 ?[/color]" });

			ShowMenu(MenuMode.ConfirmExitMap, new string[] {
			"예, 그렇습니다.",
			"아니오, 원하지 않습니다."});

			mKeyDownShowMenu = true;
		}

		private void ShowSign(int x, int y)
		{
			AppendText(new string[] { "푯말에 쓰여있기로 ...\r\n\r\n" });

			if (mParty.Map == 2)
			{
				if (x == 30 && y == 43)
					AppendText(new string[] { "          WIVERN 가는길" }, true);
				else if ((x == 28 && y == 49) || (x == 34 && x == 71))
				{

					AppendText(new string[] { "  북쪽 :",
						"       VALIANT PEOPLES 가는길",
						"  남쪽 :",
						"       GAIA TERRA 가는길" }, true);
				}
				else if (x == 43 && y == 76)
				{
					AppendText(new string[] { "  북동쪽 :",
						"       QUAKE 가는길",
						"  남서쪽 :",
						"       GAIA TERRA 가는길" }, true);
				}
			}
			else if (mParty.Map == 6)
			{
				if (x == 50 && y == 83)
				{
					AppendText(new string[] { $"       여기는 '[color={RGB.LightCyan}]캐슬 로어[/color]'성",
						"         여러분을 환영합니다",
						"",
						"[color=ff00ff]로드 안[/color]" }, true);
				}
				else if (x == 23 && y == 30)
				{
					AppendText(new string[] { "",
						"             여기는 로어 주점",
						"       여러분 모두를 환영합니다 !!" }, true);
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
					AppendText(new string[] { "",
					"          로어 왕립  죄수 수용소" }, true);
			}
			else if (mParty.Map == 7)
			{
				if (x == 38 && y == 67)
				{
					AppendText(new string[] { $"        여기는 '[color={RGB.LightCyan}]라스트디치[/color]'성",
						"         여러분을 환영합니다" }, true);
				}
				else if (x == 38 && y == 7)
					AppendText(new string[] { "       여기는 PYRAMID 의 입구" }, true);
				else if (x == 53 && y == 8)
					AppendText(new string[] { "       여기는 PYRAMID 의 입구" }, true);
			}
			else if (mParty.Map == 8)
			{
				if (x == 38 && y == 66)
				{
					AppendText(new string[] { $"      여기는 '[color={RGB.LightCyan}]VALIANT PEOPLES[/color]'성",
					"    우리의 미덕은 굽히지 않는 용기",
					"   우리는 어떤 악에도 굽히지 않는다" }, true);
				}
				else
					AppendText(new string[] { "     여기는 EVIL SEAL 의 입구" }, true);
			}
			else if (mParty.Map == 9)
			{
				if (x == 23 && y == 25)
					AppendText(new string[] { "       여기는 국왕의 보물 창고" }, true);
				else
				{
					AppendText(new string[] { $"         여기는 '[color={RGB.LightCyan}]GAIA TERRAS[/color]'성",
						"          여러분을 환영합니다" }, true);
				}
			}
			else if (mParty.Map == 12)
			{
				if (x == 23 && y == 67)
					AppendText(new string[] { "               X 는 7" }, true);
				else if (x == 26 && y == 67)
					AppendText(new string[] { "               Y 는 9" }, true);
				else if (y == 55)
				{
					var j = ((x + 1) - 6) / 7 + 12;
					AppendText(new string[] { $"           문의 번호는 '[color={RGB.LightCyan}]{j}[/color]'" }, true);
				}
				else if (x == 25 && y == 41)
					AppendText(new string[] { "            Z 는 2 * Y + X" }, true);
				else if (x == 25 && y == 32)
				{
					AppendText(new string[] { "        패스코드 x 패스코드 는 Z 라면",
						"",
						"            패스코드는 무엇인가 ?" }, true);
				}
				else if (y == 28)
				{
					var j = ((x + 1) - 3) / 5 + 2;
					AppendText(new string[] { $"           패스코드는 '[color={RGB.LightCyan}]{j}[/color]'" }, true);
				}
			}
			else if (mParty.Map == 15)
			{
				if (x == 25 && y == 62)
					AppendText(new string[] { "", "            길의 마지막" }, true);
				else if (x == 21 && y == 14)
					AppendText(new string[] { "", "     (12,15) 로 공간이동 하시오" }, true);
				else if (x == 10 && y == 13)
					AppendText(new string[] { "", "     (13,7) 로 공간이동 하시오" }, true);
				else if (x == 26 && y == 13)
					AppendText(new string[] { "", "   황금의 갑옷은 (45,19) 에 숨겨져있음" }, true);

			}
			else if (mParty.Map == 17)
			{
				if (x == 67 && y == 46)
					AppendText(new string[] { "", "    하! 하! 하!  너는 우리에게 속았다" }, true);
				else if (x == 57 && y == 52)
				{
					AppendText(new string[] { "", "      [color=LightGreen]이 게임을 만든 사람[/color]",
						"  : 동아 대학교 전기 공학과",
						"        92 학번  안 영기" }, true);
				}
				else if (x == 50 && y == 29)
				{
					AppendText(new string[] { "", "       오른쪽 : Hidra 의 보물창고",
						"         위쪽이 진짜 보물창고임" }, true);
				}
			}
			else if (mParty.Map == 19)
			{
				AppendText(new string[] { "       이 길을 통과하고자하는 사람은",
					"     양측의 늪속에 있는 레버를 당기시오" }, true);
			}
			else if (mParty.Map == 23)
			{
				AppendText(new string[] { "      (25,27)에 있는 레버를 움직이면",
				"          성을 볼수 있을 것이오.",
				"",
				"             [color=LightGreen]제작자 안 영기 씀[/color]" }, true);
				mMapLayer[24 + mMapWidth * 26] = 52;
			}
		}

		private enum PositionType {
			Town,
			Ground,
			Den,
			Keep
		}

		private enum MenuMode
		{
			None,
			Game,
			Battle,
			ViewCharacter,
			CastSpell,
			SpellCategory,
			CureWhom,
			ChooseCureSpell,
			ApplyCureMagic,
			ApplyCureAllMagic,
			ApplyPhenominaMagic,
			VaporizeMoveDirection,
			TransformDirection,
			Extrasense,
			ChooseExtrasense,
			TelescopeDirection,
			GameOptions,
			SetMaxEnemy,
			SetEncounterType,
			OrderFromCharacter,
			OrderToCharacter,
			DelistCharacter,
			ConfirmExit,
			JoinMadJoe,
			Grocery,
			WeaponType,
			BuyWeapon,
			UseWeaponCharacter,
			Hospital,
			HealType,
			TrainingCenter,
			BuyShield,
			UseShieldCharacter,
			BuyArmor,
			UseArmorCharacter,
			ConfirmExitMap,
			JoinSkeleton
		}

		private enum CureMenuState
		{
			None,
			NotCure,
			CureEnd
		}
	}

}
