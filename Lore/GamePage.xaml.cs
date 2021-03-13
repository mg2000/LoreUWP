﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
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
		private bool mNextMode = false;
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
		private int mMagicPlayerID = -1;
		private int mMagicWhomPlayerID = -1;

		private Random mRand = new Random();

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

			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;
			gamePageKeyDownEvent = async (sender, args) =>
			{
				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (ContinueText.Visibility == Visibility.Visible)
				{
					if (mNextMode)
					{
						ContinueText.Visibility = Visibility.Collapsed;
						mNextMode = false;
					}
					else
						return;
				}

				if (mTalkMode > 0)
				{
					ContinueText.Visibility = Visibility.Collapsed;
					TalkMode(mTalkX, mTalkY, args.VirtualKey);
				}
				else if (mMenuMode == MenuMode.None && (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right ||
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
								// Special Event
								MovePlayer(x, y);
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
								// Sign
								ShowSign(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 24)
							{
								if (EnterWater())
									MovePlayer(x, y);
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
							}
						}
					}
				}
			};

			gamePageKeyUpEvent = async (sender, args) => {
				if (ContinueText.Visibility == Visibility.Visible)
					mNextMode = true;
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
						if (mMenuMode == MenuMode.Game)
						{
							AppendText(new string[] { "" });
							HideMenu();
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
                    {
						HideMenu();
						if (mMenuMode == MenuMode.Game)
                        {
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

								mMenuMode = MenuMode.None;
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "능력을 보고싶은 인물을 선택하시오" });
								ShowCharacterMenu(MenuMode.ViewCharacter);
							}
							else if (mMenuFocusID == 2)
                            {
								mMenuMode = MenuMode.None;

								AppendText(new string[] { "[color=ffffff]이름[/color]\t\t[color=cd5c5c]중독\t\t의식불명\t죽음[/color]" });
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
                            {
								ShowCharacterMenu(MenuMode.CastSpell);
							}
                        }
						else if (mMenuMode == MenuMode.ViewCharacter)
                        {
							mMenuMode = MenuMode.None;

							var shieldStr = mPlayerList[mMenuFocusID].Shield != 0 ? $"[color = 00ff00]방패 - {Common.GetDefenceStr(mPlayerList[mMenuFocusID].Weapon)}[/ color]" : "";
							var armorStr = mPlayerList[mMenuFocusID].Armor != 0 ? $"[color=00ff00]갑옷 - {Common.GetDefenceStr(mPlayerList[mMenuFocusID].Weapon)}[/color]" : "";

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

							if (IsAvailableMember(mPlayerList[mMenuFocusID])) {
								mMagicPlayerID = mMenuFocusID;
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
						}
						else if (mMenuMode == MenuMode.ChooseCureSpell)
                        {
							mMenuMode = MenuMode.None;

							mMagicWhomPlayerID = mMenuFocusID;

							AppendText(new string[] { "선택" });
							if (mMenuFocusID < mPlayerList.Count)
                            {
								var availableCount = mPlayerList[mMagicPlayerID].Level[1] / 2 + 1;
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
								var availableCount = mPlayerList[mMagicPlayerID].Level[1] / 2 - 3;
								if (availableCount < 0)
                                {
									AppendText(new string[] { $"{mPlayerList[mMagicPlayerID].Name}는 강한 치료 마법은 아직 불가능 합니다." });
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

							if (mMenuFocusID == 0)
								HealOne();
						}
						else if (mMenuMode == MenuMode.ApplyCureAllMagic)
						{
							mMenuMode = MenuMode.None;
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		private void HealOne()
        {
			if (mPlayerList[mMagicWhomPlayerID].Dead > 0 || mPlayerList[mMagicWhomPlayerID].Unconscious > 0 || mPlayerList[mMagicWhomPlayerID].Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{mPlayerList[mMagicPlayerID].Name}(은)는 치료될 상태가 아닙니다." });
			}
			else if (mPlayerList[mMagicWhomPlayerID].HP >= mPlayerList[mMagicWhomPlayerID].Endurance * mPlayerList[mMagicWhomPlayerID].Level[0])
			{
				if (mParty.Etc[5] == 0)
					AppendText(new string[] { $"{mPlayerList[mMagicPlayerID].Name}(은)는 치료될 상태가 아닙니다." });
			}
			else
			{
				var needSP = 2 * mPlayerList[mMagicPlayerID].Level[1];
				if (mPlayerList[mMagicPlayerID].SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					mPlayerList[mMagicPlayerID].SP -= needSP;
					mPlayerList[mMagicWhomPlayerID].HP += needSP * 3 / 2;
					if (mPlayerList[mMagicWhomPlayerID].HP > mPlayerList[mMagicWhomPlayerID].Level[0] * mPlayerList[mMagicWhomPlayerID].Endurance)
						mPlayerList[mMagicWhomPlayerID].HP = mPlayerList[mMagicWhomPlayerID].Level[0] * mPlayerList[mMagicWhomPlayerID].Endurance;

					AppendText(new string[] { $"[color=ffffff]{mPlayerList[mMagicWhomPlayerID].Name}(은)는 치료되어 졌습니다.[/color]" });
				}
			}
		}

		private void ShowNotEnoughSP()
        {
			AppendText(new string[] { "그러나, 마법 지수가 충분하지 않습니다." });
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
			AppendText(new string[] { "[color=90ee90]한명을 고르시오 ---[/color]" }, true);

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

			for (var i = 0; i < mMenuCount && i < mMenuList.Count; i++)
            {
				mMenuList[i].Text = menuItem[i];
				mMenuList[i].Visibility = Visibility.Visible;
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
			if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
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
                }
                else if (mTalkMode == 1 && (key == VirtualKey.N || key == VirtualKey.GamepadB))
                {
					AppendText(new string[] { "",
						"아니오.",
						"",
						"다시 생각 해보십시오." }, true);

                    ContinueText.Visibility = Visibility.Visible;
                    mTalkMode = 0;
                }
            }
            else if (moveX == 50 && moveY == 27)
            {
                if (mTalkMode == 0)
                {
                    if (mParty.Etc[9] == 0)
                    {
                        AppendText(new string[] { "나는 [color=62e4f2]로드 안[/color]이오.",
							"이제부터 당신은 이 게임에서 새로운 인물로서 생을 시작하게 될것이오. 그럼 나의 이야기를 시작하겠소." });
                        ContinueText.Visibility = Visibility.Visible;

                        mParty.Etc[9]++;

                        mTalkMode = 1;
                        mTalkX = moveX;
                        mTalkY = moveY;
                    }
                    else if (mParty.Etc[9] == 3)
                    {
						AppendText(new string[] { " 대륙의 남서쪽에 있는 '[color=62e4f2]메나스[/color]'를 탐사해 주시오." });
                        ContinueText.Visibility = Visibility.Visible;
                    }
					else if (mParty.Etc[9] == 4)
                    {
						AppendText(new string[] { "당신들의 성공을 축하하오 !!",
						"[color=62e4f2][EXP + 1000][/color]" });

						mPlayerList.ForEach(delegate (Lore player)
						{
							player.Experience += 1000;
						});

						mParty.Etc[9]++;
						ContinueText.Visibility = Visibility.Visible;
					}
					else if (mParty.Etc[9] == 5)
                    {
						AppendText(new string[] { " 드디어 나는 당신들의 능력을 믿을수 있게 되었소.  그렇다면 당신들에게 Necromancer 응징이라는 막중한 임무를 한번 맡겨 보겠소.",
							"먼저 대륙의 동쪽에 있는 '[color=62e4f2]LASTDITCH[/color]'에 가보도록 하시오. '[color=62e4f2]LASTDITCH[/color]'성에는 지금 많은 근심에 쌓여있소. 그들을 도와 주시오." });

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
						"  그래서, 우리들은 그 원인을 알아보기 위해 'LORE 특공대'를 조직하기로 합의를 하고 " +
						"이곳에 있는 거의 모든 용사들을 모아서 용암 대지로 변한 그 대륙으로 급히 그들을 파견하였지만 여태껏 아무 소식도 듣지못했소. 그들이 생존해 있는지 조차도 말이오.",
						" 이런 저런 방법을 통하여 그들의 생사를 알아려던중 우연히 우리들은 '[color=62e4f2]Necromancer[/color]'라고 불리우는  용암 대지속의  새로운 세력의 존재를" +
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
					AppendText(new string[] { " 그는 현재 이 세계의 반을  그의 세력권 안에 넣고 있소. 여기서 당신의 궁극적인 임무는 바로 '[color=62e4f2]Necromancer의 야심을 봉쇄 시키는 것[/color]'이라는 걸 명심해 두시오.",
						" 네크로맨서 의 영향력은 이미 로어 대륙까지 도달해있소.  또한 그들은 이 대륙의 남서쪽에 '메나스' 라고 불리우는 지하 동굴을 얼마전에 구축했소.  그래서, 그 동굴의 존재 때문에 우리들은 그에게 위협을 당하게 되었던 것이오.",
						" 하지만, LORE 특공대가 이 대륙을 떠난후로는 그 일당들에게 대적할 용사는  이미 남아있지 않았소. 그래서 부탁하건데, 그 동굴을 중심부까지 탐사해 주시오.",
						" 나는 당신들에게 Necromancer에 대한 일을 맡기고 싶지만, 아직은 당신들의  확실한 능력을 모르는 상태이지요.  그래서 이 일은 당신들의 잠재력을 증명해 주는 좋은 기회가 될것이오.",
						" 만약 당신들이 무기가 필요하다면 무기고에서 약간의 무기를 가져가도록 허락하겠소."
					});
                    ContinueText.Visibility = Visibility.Visible;

                    mParty.Etc[9]++;

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
				await LoadFile();

				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.bmp"), new Vector2(64, 64), Vector2.Zero);
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

				if (mCharacterTiles != null)
					mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(mParty.XAxis, mParty.YAxis), Vector4.One);
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
				mMapTiles.Draw(sb, layer[index], mMapTiles.SpriteSize * new Vector2(column, row), tint);
		}

		private async Task LoadFile() {
			var storageFolder = ApplicationData.Current.LocalFolder;

			var saveFile = await storageFolder.CreateFileAsync("loreSave.dat", CreationCollisionOption.OpenIfExists);
			var saveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(saveFile));

			mParty = saveData.Party;
			mPlayerList = saveData.PlayerList;

			if (saveData.Map.Data.Length == 0)
			{
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
			else {
				lock (mapLock)
				{
					mMapWidth = saveData.Map.Width;
					mMapHeight = saveData.Map.Height;

					mMapLayer = saveData.Map.Data;
				}
			}

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

			mEncounter = saveData.Encounter;
			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			mMaxEnemy = saveData.MaxEnemy;
			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			DisplayPlayerInfo();

			switch (mParty.Etc[11])
			{

			}
		}

		private void DisplayPlayerInfo() {
			for (var i = 0; i < mPlayerList.Count && i < 6; i++) {
				mPlayerNameList[i].Text = mPlayerList[i].Name;
				mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.White);

				mPlayerACList[i].Text = mPlayerList[i].AC.ToString();
				mPlayerLevelList[i].Text = mPlayerList[i].Level[0].ToString();
			}

			DisplayHP();
			DisplaySP();
			DisplayESP();
			DisplayCondition();
		}

		private void DisplayHP() {
			for (var i = 0; i < mPlayerList.Count && i < 6; i++)
				mPlayerHPList[i].Text = mPlayerList[i].HP.ToString();
		}

		private void DisplaySP()
		{
			for (var i = 0; i < mPlayerList.Count && i < 6; i++)
				mPlayerSPList[i].Text = mPlayerList[i].SP.ToString();
		}

		private void DisplayESP()
		{
			for (var i = 0; i < mPlayerList.Count && i < 6; i++)
				mPlayerESPList[i].Text = mPlayerList[i].ESP.ToString();
		}

		private void DisplayCondition()
		{
			for (var i = 0; i < mPlayerList.Count && i < 6; i++)
				mPlayerConditionList[i].Text = GetConditionName(i);
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
					AppendText(new string[] { "       여기는 '[color=62e4f2]CASTLE LORE[/color]'성",
						"         여러분을 환영합니다",
						"",
						"[color=ff00ff]Lord Ahn[/color]" }, true);
				}
				else if (x == 23 && y == 30)
				{
					AppendText(new string[] { "",
						"             여기는 LORE 주점",
						"       여러분 모두를 환영합니다 !!" }, true);
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
					AppendText(new string[] { "",
					"          LORE 왕립  죄수 수용소" }, true);
			}
			else if (mParty.Map == 7)
			{
				if (x == 38 && y == 67)
				{
					AppendText(new string[] { "        여기는 '[color=62e4f2]LASTDITCH[/color]'성",
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
					AppendText(new string[] { "      여기는 '[color=62e4f2]VALIANT PEOPLES[/color]'성",
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
					AppendText(new string[] { "         여기는 '[color=62e4f2]GAIA TERRAS[/color]'성",
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
					AppendText(new string[] { $"           문의 번호는 '[color=62e4f2]{j}[/color]'" }, true);
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
					AppendText(new string[] { $"           패스코드는 '[color=62e4f2]{j}[/color]'" }, true);
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
					AppendText(new string[] { "", "      [color=90ee90]이 게임을 만든 사람[/color]",
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
				"             [color=90ee90]제작자 안 영기 씀[/color]" }, true);
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
		}
	}

}
