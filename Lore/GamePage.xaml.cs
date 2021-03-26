using Microsoft.Graphics.Canvas;
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
		private List<TextBlock> mEnemyTextList = new List<TextBlock>();
		private List<Border> mEnemyBlockList = new List<Border>();

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

		// 1 - 로어성에서 병사를 만났을때 발생하는 이벤트
		// 2 - 로어성을 떠날때 스켈레톤을 만나는 이벤트
		// 3 - 스켈레톤의 답을 거절했을때 이벤트
		// 4 - 게임 세계 도전 여부 Y/N
		private int mSpecialEvent = 0;

		private volatile bool mMoveEvent = false;

		private Random mRand = new Random();

		private List<EnemyData> mEnemyDataList = null;
		private List<BattleEnemyData> mEncounterEnemyList = new List<BattleEnemyData>();
		private int mBattlePlayerID = 0;
		private int mBattleFriendID = 0;
		private int mBattleCommandID = 0;
		private int mBattleToolID = 0;
		private int mEnemyFocusID = 0;
		private Queue<BattleCommand> mBattleCommandQueue = new Queue<BattleCommand>();
		private Queue<BattleEnemyData> mBatteEnemyQueue = new Queue<BattleEnemyData>();
		private BattleTurn mBattleTurn = BattleTurn.None;

		private bool mLoading = true;

		private Dictionary<EnterType, string> mEnterTypeMap = new Dictionary<EnterType, string>();
		private EnterType mTryEnterType = EnterType.None;

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

			mEnemyBlockList.Add(EnemyBlock0);
			mEnemyBlockList.Add(EnemyBlock1);
			mEnemyBlockList.Add(EnemyBlock2);
			mEnemyBlockList.Add(EnemyBlock3);
			mEnemyBlockList.Add(EnemyBlock4);
			mEnemyBlockList.Add(EnemyBlock5);
			mEnemyBlockList.Add(EnemyBlock6);
			mEnemyBlockList.Add(EnemyBlock7);

			mEnemyTextList.Add(EnemyText0);
			mEnemyTextList.Add(EnemyText1);
			mEnemyTextList.Add(EnemyText2);
			mEnemyTextList.Add(EnemyText3);
			mEnemyTextList.Add(EnemyText4);
			mEnemyTextList.Add(EnemyText5);
			mEnemyTextList.Add(EnemyText6);
			mEnemyTextList.Add(EnemyText7);

			mEnterTypeMap[EnterType.CastleLore] = "로어 성";
			mEnterTypeMap[EnterType.Menace] = "메나스";
			mEnterTypeMap[EnterType.LastDitch] = "라스트디치";


			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;
			gamePageKeyDownEvent = async (sender, args) =>
			{

				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (mLoading || mMoveEvent || mSpecialEvent > 0)
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

						bool needUpdateStat = false;
						mPlayerList.ForEach(delegate (Lore player)
						{
							if (player.Poison > 0)
								player.Poison++;

							if (player.Poison > 10)
							{
								player.Poison = 1;
								if (0 < player.Dead && player.Dead < 100)
									player.Dead++;
								else if (player.Unconscious > 0)
								{
									player.Unconscious++;
									if (player.Unconscious > player.Endurance * player.Level[0])
										player.Dead++;
								}
								else
								{
									player.HP--;
									if (player.HP <= 0)
										player.Unconscious = 1;
								}

								needUpdateStat = true;
							}
						});

						if (needUpdateStat)
						{
							DisplayHP();
							DisplayCondition();
						}

						DetectGameOver();

						if (mParty.Etc[4] > 0)
							mParty.Etc[4]--;

						if (mRand.Next(mEncounter * 20) == 0)
						{
							EncounterEnemy();
							mTriggeredDownEvent = true;
						}
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
						void ShowEnterMenu(EnterType enterType)
						{
							mTryEnterType = enterType;

							AppendText(new string[] { $"{mEnterTypeMap[enterType]}에 들어가기를 원합니까 ?" });

							ShowMenu(MenuMode.AskEnter, new string[] {
									"예, 그렇습니다.",
									"아니오, 원하지 않습니다."
								});
						}

						void EnterMap()
						{
							if (mParty.Map == 1)
							{
								if (x == 19 && y == 10)
									ShowEnterMenu(EnterType.CastleLore);
								else if (x == 75 && y == 56)
									ShowEnterMenu(EnterType.LastDitch);
								else if (x == 16 && y == 88)
									ShowEnterMenu(EnterType.Menace);
							}
						}

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
								EnterMap();
								mTriggeredDownEvent = true;
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
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 26)
							{
								EnterLava();
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
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 50)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (27 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
						}
						else if (mPosition == PositionType.Den) {
							if (mMapLayer[x + mMapWidth * y] == 0 || mMapLayer[x + mMapWidth * y] == 52)
							{
								MovePlayer(x, y);
								InvokeSpecialEvent();
								mTriggeredDownEvent = true;
							}
							else if ((1 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 40) || mMapLayer[x + mMapWidth * y] == 51) {

							}
							else if (mMapLayer[x + mMapWidth * y] == 53) {
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (mMapLayer[x + mMapWidth * y] == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (mMapLayer[x + mMapWidth * y] == 48)
							{
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 50)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (mMapLayer[x + mMapWidth * y] == 54)
							{
								
							}
							else if (41 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else {
								TalkMode(x, y);
								mTriggeredDownEvent = true;
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

				async Task RefreshGame()
				{
					AppendText(new string[] { "" });
					await LoadMapData();
					InitializeMap();
				}

				async Task ExitCastleLore()
				{
					mParty.XAxis = 19;
					mParty.YAxis = 11;
					mParty.Map = 1;

					await RefreshGame();
				}

				void EndBattle()
				{
					if (mBattleTurn == BattleTurn.Win) {
						var endMessage = "";

						if (mParty.Etc[5] == 2)
							endMessage = "";
						else {
							var goldPlus = 0;
							foreach (var enemy in mEncounterEnemyList) {
								var enemyInfo = mEnemyDataList[enemy.ENumber];
								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
								var plus = enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= point;
								goldPlus += plus;
							}

							mParty.Gold += goldPlus;

							endMessage = $"일행은 {goldPlus}개의 금을 얻었다.";
						}

						AppendText(new string[] { endMessage });

						mEncounterEnemyList.Clear();

						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.RunAway) {
						AppendText(new string[] { "" });

						mEncounterEnemyList.Clear();

						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.Lose) {
						ShowGameOver(new string[] {
							$"[color={RGB.LightMagenta}]일행은 모두 전투에서 패했다 !![/color]",
							$"[color={RGB.LightGreen}]    어떻게 하시겠습니까 ?[/color]"
						});
					}
					
					mBattleTurn = BattleTurn.None;
				}

				void CureSpell(Lore player, Lore whomPlayer, int magic, List<string> cureResult = null)
				{
					switch (magic)
					{
						case 0:
							HealOne(player, whomPlayer, cureResult);
							break;
						case 1:
							CureOne(player, whomPlayer, cureResult);
							break;
						case 2:
							CureOne(player, whomPlayer, cureResult);
							HealOne(player, whomPlayer, cureResult);
							break;
						case 3:
							ConsciousOne(player, whomPlayer, cureResult);
							break;
						case 4:
							RevitalizeOne(player, whomPlayer, cureResult);
							break;
						case 5:
							ConsciousOne(player, whomPlayer, cureResult);
							CureOne(player, whomPlayer, cureResult);
							HealOne(player, whomPlayer, cureResult);
							break;
						case 6:
							RevitalizeOne(player, whomPlayer, cureResult);
							ConsciousOne(player, whomPlayer, cureResult);
							CureOne(player, whomPlayer, cureResult);
							HealOne(player, whomPlayer, cureResult);
							break;
					}

					UpdatePlayersStat();
				}

				void CureAllSpell(Lore player, int magic, List<string> cureResult = null)
				{
					switch (magic)
					{
						case 0:
							HealAll(player, cureResult);
							break;
						case 1:
							CureAll(player, cureResult);
							break;
						case 2:
							CureAll(player, cureResult);
							HealAll(player, cureResult);
							break;
						case 3:
							ConsciousAll(player, cureResult);
							break;
						case 4:
							ConsciousAll(player, cureResult);
							CureAll(player, cureResult);
							HealAll(player, cureResult);
							break;
						case 5:
							RevitalizeAll(player, cureResult);
							break;
						case 6:
							RevitalizeAll(player, cureResult);
							ConsciousAll(player, cureResult);
							CureAll(player, cureResult);
							HealAll(player, cureResult);
							break;

					}

					UpdatePlayersStat();
				}

				void BattleMode()
				{
					var player = mPlayerList[mBattlePlayerID];
					mBattleFriendID = 0;
					mBattleToolID = 0;

					AppendText(new string[] {
							$"{player.Name}의 전투 모드 ===>"
						});

					ShowMenu(MenuMode.BattleCommand, new string[] {
							$"한 명의 적을 {Common.GetWeaponStr(player.Weapon)}{Common.GetWeaponJosaStr(player.Weapon)}로 공격",
							"한 명의 적에게 마법 공격",
							"모든 적에게 마법 공격",
							"적에게 특수 마법 공격",
							"일행을 치료",
							"적에게 초능력 사용",
							mBattlePlayerID == 0 ? "일행에게 무조건 공격 할 것을 지시" : "도망을 시도함"
						});
				}

				void ExecuteBattle() {
					void CheckBattleStatus() {
						var allPlayerDead = true;
						foreach (var player in mPlayerList) {
							if (player.IsAvailable) {
								allPlayerDead = false;
								break;
							}
						}

						if (allPlayerDead)
							mBattleTurn = BattleTurn.Lose;
						else {
							var allEnemyDead = true;
							foreach (var enemy in mEncounterEnemyList) {
								if (!enemy.Dead && !enemy.Unconscious)
								{
									allEnemyDead = false;
									break;
								}
							}

							if (allEnemyDead)
								mBattleTurn = BattleTurn.Win;
						}
					}

					void ShowBattleResult(List<string> battleResult) {
						const int maxLines = 13;

						var lineHeight = 0d;
						if (DialogText.Blocks.Count > 0)
						{
							var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
							lineHeight = startRect.Height;
						}

						var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

						var append = false;
						if (lineCount + battleResult.Count + 1 <= maxLines)
						{
							if (lineHeight > 0)
								battleResult.Insert(0, "");
							append = true;
						}

						AppendText(battleResult.ToArray(), append);

						DisplayPlayerInfo();
						DisplayEnemy();
						
						ContinueText.Visibility = Visibility.Visible;

						CheckBattleStatus();
					}
					

					if (mBattleCommandQueue.Count == 0 && mBatteEnemyQueue.Count == 0) {
						DialogText.TextHighlighters.Clear();
						DialogText.Blocks.Clear();

						switch (mBattleTurn) {
							case BattleTurn.Player:
								mBattleTurn = BattleTurn.Enemy;
								break;
							case BattleTurn.Enemy:
								mBattleTurn = BattleTurn.Player;
								break;
						}

						switch (mBattleTurn) {
							case BattleTurn.Player:
								for (var i = 0; i < mPlayerList.Count; i++)
								{
									if (mPlayerList[i].IsAvailable)
									{
										mBattlePlayerID = i;
										break;
									}
								}

								BattleMode();
								return;
							case BattleTurn.Enemy:
								foreach (var enemy in mEncounterEnemyList) {
									if (!enemy.Dead && !enemy.Unconscious)
										mBatteEnemyQueue.Enqueue(enemy);
								}

								break;
						}
					}
					
					
					if (mBattleCommandQueue.Count > 0)
					{
						var battleCommand = mBattleCommandQueue.Dequeue();
						var battleResult = new List<string>();

						BattleEnemyData GetDestEnemy()
						{
							bool AllEnemyDead()
							{
								for (var i = 0; i < mEncounterEnemyList.Count; i++)
								{
									if (!mEncounterEnemyList[i].Dead)
										return false;
								};

								return true;
							}

							if (!AllEnemyDead())
							{
								var enemyID = battleCommand.EnemyID;
								while (mEncounterEnemyList[enemyID].Dead)
									enemyID = (enemyID + 1) % mEncounterEnemyList.Count;

								return mEncounterEnemyList[enemyID];
							}
							else
								return null;
						}

						void GetBattleStatus(BattleEnemyData enemy)
						{
							switch (battleCommand.Method)
							{
								case 0:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 {Common.GetWeaponStr(battleCommand.Player.Weapon)}{Common.GetWeaponJosaStr(battleCommand.Player.Weapon)}로 {enemy.Name}(을)를 공격했다[/color]");
									break;
								case 1:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 '{Common.GetMagicStr(battleCommand.Tool)}'{Common.GetMagicJosaStr(battleCommand.Tool)}로 {enemy.Name}(을)를 공격했다[/color]");
									break;
								case 2:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 '{Common.GetMagicStr(battleCommand.Tool + 6)}'{Common.GetMagicJosaStr(battleCommand.Tool + 6)}로 {enemy.Name}(을)를 공격했다[/color]");
									break;
								case 3:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 {enemy.Name}에게 {Common.GetMagicStr(battleCommand.Tool + 12)}{Common.GetMagicJosaStr(battleCommand.Tool + 12)}로 특수 공격을 했다[/color]");
									break;
								case 4:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 {mPlayerList[battleCommand.FriendID].Name}에게 '{Common.GetMagicStr(battleCommand.Tool + 18)}'{Common.GetMagicJosaStr(battleCommand.Tool + 18)} 사용했다[/color]");
									break;
								case 5:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 {enemy.Name}에게 '{Common.GetMagicStr(battleCommand.Tool + 40)}'{Common.GetMagicJosaStr(battleCommand.Tool + 40)} 사용했다[/color]");
									break;
								case 6:
									battleResult.Add($"[color={RGB.White}]일행은 도망을 시도했다[/color]");
									break;
								default:
									battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 잠시 주저했다[/color]");
									break;
							}
						}

						void plusExperience(BattleEnemyData enemy)
						{
							var exp = enemy.ENumber * enemy.ENumber * enemy.ENumber / 8;
							if (exp == 0)
								exp = 1;

							if (!enemy.Unconscious)
							{
								battleResult.Add($"[color={RGB.Yellow}]{battleCommand.Player.Name}(은)는[/color] [color={RGB.LightCyan}]{exp}[/color][color={RGB.Yellow}]만큼 경험치를 얻었다 ![/color]");
								battleCommand.Player.Experience += exp;
							}
							else
							{
								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
										player.Experience += exp;
								};
							}
						}

						void AttackOne()
						{
							var enemy = GetDestEnemy();
							if (enemy == null)
								return;

							GetBattleStatus(enemy);

							if (enemy.Unconscious)
							{
								switch (mRand.Next(4))
								{
									case 0:
										battleResult.Add($"[color={RGB.LightRed}]{battleCommand.Player.GenderPronoun}의 무기가 {enemy.Name}의 심장을 꿰뚫었다[/color]");
										break;
									case 1:
										battleResult.Add($"[color={RGB.LightRed}]{enemy.Name}의 머리는 {battleCommand.Player.GenderPronoun}의 공격으로 산산 조각이 났다[/color]");
										break;
									case 2:
										battleResult.Add($"[color={RGB.LightRed}]적의 피가 사방에 뿌려졌다[/color]");
										break;
									case 3:
										battleResult.Add($"[color={RGB.LightRed}]적은 비명과 함께 찢겨 나갔다[/color]");
										break;

								}

								plusExperience(enemy);
								enemy.HP = 0;
								enemy.Dead = true;
								DisplayEnemy();
								return;
							}

							if (mRand.Next(20) > battleCommand.Player.Accuracy[0])
							{
								battleResult.Add($"{battleCommand.Player.GenderPronoun}의 공격은 빗나갔다 ....");
								return;
							}

							var attackPoint = (int)Math.Round((double)battleCommand.Player.Strength * battleCommand.Player.WeaPower * battleCommand.Player.Level[0] / 20);
							attackPoint -= attackPoint * mRand.Next(50) / 100;

							if (mRand.Next(100) < enemy.Resistance)
							{
								battleResult.Add($"적은 {battleCommand.Player.GenderPronoun}의 공격을 저지했다");
								return;
							}

							var defensePoint = (int)Math.Round((double)enemy.AC * enemy.Level * (mRand.Next(10) + 1) / 10);
							attackPoint -= defensePoint;
							if (attackPoint <= 0)
							{
								battleResult.Add($"그러나, 적은 {battleCommand.Player.GenderPronoun}의 공격을 막았다");
								return;
							}

							enemy.HP -= attackPoint;
							if (enemy.HP <= 0)
							{
								enemy.HP = 0;
								enemy.Unconscious = false;
								enemy.Dead = false;

								battleResult.Add($"[color={RGB.LightRed}]적은 {battleCommand.Player.GenderPronoun}의 공격으로 의식불명이 되었다[/color]");
								plusExperience(enemy);
								enemy.Unconscious = true;
							}
							else
							{
								battleResult.Add($"적은 [color={RGB.White}]{attackPoint}[/color]만큼의 피해를 입었다");
							}
						}

						void CastOne()
						{
							var enemy = GetDestEnemy();
							if (enemy == null)
								return;

							GetBattleStatus(enemy);

							if (enemy.Unconscious)
							{
								battleResult.Add($"[color={RGB.LightRed}]{battleCommand.Player.GenderPronoun}의 마법은 적의 시체 위에서 작열했다[/color]");

								plusExperience(enemy);
								enemy.HP = 0;
								enemy.Dead = true;
								DisplayEnemy();
								return;
							}

							var magicPoint = (int)Math.Round((double)battleCommand.Player.Level[1] * battleCommand.Tool * battleCommand.Tool / 2);
							if (battleCommand.Player.SP < magicPoint)
							{
								battleResult.Add($"마법 지수가 부족했다");
								return;
							}

							battleCommand.Player.SP -= magicPoint;

							if (mRand.Next(20) >= battleCommand.Player.Accuracy[1])
							{
								battleResult.Add($"그러나, {enemy.Name}(을)를 빗나갔다");
								return;
							}

							magicPoint = battleCommand.Player.Level[1] * battleCommand.Tool * battleCommand.Tool * 2;

							if (mRand.Next(100) < enemy.Resistance)
							{
								battleResult.Add($"{enemy.Name}(은)는 {battleCommand.Player.GenderPronoun}의 마법을 저지했다");
								return;
							}

							var defensePoint = (int)Math.Round((double)enemy.AC * enemy.Level * (mRand.Next(10) + 1) / 10);
							magicPoint -= defensePoint;

							if (magicPoint <= 0)
							{
								battleResult.Add($"그러나, 적은 {battleCommand.Player.GenderPronoun}의 공격을 막았다");
								return;
							}

							enemy.HP -= magicPoint;
							if (enemy.HP <= 0)
							{
								enemy.HP = 0;
								enemy.Unconscious = true;

								battleResult.Add($"[color={RGB.LightRed}]적은 {battleCommand.Player.GenderPronoun}의 공격으로 의식불명이 되었다[/color]");
							}
							else
							{
								battleResult.Add($"적은 [color={RGB.White}]{magicPoint}[/color]만큼의 피해를 입었다");
							}
						}

						void CastSpecialMagic() {
							var enemy = mEncounterEnemyList[battleCommand.EnemyID];
							GetBattleStatus(enemy);

							if ((mParty.Etc[37] & 1) == 0)
							{
								battleResult.Add($"당신에게는 아직 능력이 없다.");
								return;
							}

							if (battleCommand.Tool == 0)
							{
								if (battleCommand.Player.SP < 10)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 10;
								if (mRand.Next(100) < enemy.Resistance)
								{
									battleResult.Add($"적은 독 공격을 저지 했다");
									return;
								}

								if (mRand.Next(40) > enemy.Accuracy[1])
								{
									battleResult.Add($"독 공격은 빗나갔다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{enemy.Name}(은)는 중독 되었다[/color]");
								enemy.Posion = true;
							}
							else if (battleCommand.Tool == 1)
							{
								if (battleCommand.Player.SP < 30)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 30;
								if (mRand.Next(100) < enemy.Resistance)
								{
									battleResult.Add($"기술 무력화 공격은 저지 당했다");
									return;
								}

								if (mRand.Next(60) > enemy.Accuracy[1])
								{
									battleResult.Add($"기술 무력화 공격은 빗나갔다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 특수 공격 능력이 제거되었다[/color]");
								enemy.Special = 0;
							}
							else if (battleCommand.Tool == 2)
							{
								if (battleCommand.Player.SP < 15)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 15;
								if (mRand.Next(100) < enemy.Resistance)
								{
									battleResult.Add($"방어 무력화 공격은 저지 당했다");
									return;
								}


								int resistancePoint;
								if (enemy.AC < 5)
									resistancePoint = 40;
								else
									resistancePoint = 25;

								if (mRand.Next(resistancePoint) > enemy.Accuracy[1])
								{
									battleResult.Add($"방어 무력화 공격은 빗나갔다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 방어 능력이 저하되었다[/color]");
								if ((enemy.Resistance < 31 || mRand.Next(2) == 0) && enemy.AC > 0)
									enemy.AC--;
								else
								{
									enemy.Resistance -= 10;
									if (enemy.Resistance > 0)
										enemy.Resistance = 0;
								}
							}
							else if (battleCommand.Tool == 3)
							{
								if (battleCommand.Player.SP < 20)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 20;
								if (mRand.Next(200) < enemy.Resistance)
								{
									battleResult.Add($"능력 저하 공격은 저지 당했다");
									return;
								}


								if (mRand.Next(30) > enemy.Accuracy[1])
								{
									battleResult.Add($"능력 저하 공격은 빗나갔다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 전체적인 능력이 저하되었다[/color]");
								if (enemy.Level > 0)
									enemy.Level--;

								enemy.Resistance -= 10;
								if (enemy.Resistance > 0)
									enemy.Resistance = 0;
							}
							else if (battleCommand.Tool == 4)
							{
								if (battleCommand.Player.SP < 15)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 15;
								if (mRand.Next(100) < enemy.Resistance)
								{
									battleResult.Add($"마법 불능 공격은 저지 당했다");
									return;
								}


								if (mRand.Next(100) > enemy.Accuracy[1])
								{
									battleResult.Add($"마법 불능 공격은 빗나갔다");
									return;
								}

								if (enemy.CastLevel > 1)
									battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력이 저하되었다[/color]");
								else
									battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력은 사라졌다[/color]");

								if (enemy.CastLevel > 0)
									enemy.CastLevel--;
							}
							else if (battleCommand.Tool == 5)
							{
								if (battleCommand.Player.SP < 20)
								{
									battleResult.Add($"마법 지수가 부족했다");
									return;
								}

								battleCommand.Player.SP -= 20;
								if (mRand.Next(100) < enemy.Resistance)
								{
									battleResult.Add($"탈 초인화 공격은 저지 당했다");
									return;
								}


								if (mRand.Next(100) > enemy.Accuracy[1])
								{
									battleResult.Add($"탈 초인화 공격은 빗나갔다");
									return;
								}

								if (enemy.SpecialCastLevel > 1)
									battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력이 저하되었다[/color]");
								else
									battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력은 사라졌다[/color]");

								if (enemy.SpecialCastLevel > 0)
									enemy.SpecialCastLevel--;
							}
						}

						void CastESP() {
							if ((battleCommand.Player.Class != 2 && battleCommand.Player.Class != 3 && battleCommand.Player.Class != 6) || ((mParty.Etc[38] & 1) > 0))
							{
								battleResult.Add($"당신에게는 아직 능력이 없다.");
								return;
							}

							if (battleCommand.Tool == 0 || battleCommand.Tool == 1 || battleCommand.Tool == 3)
							{
								battleResult.Add($"{Common.GetMagicStr(battleCommand.Tool + 41)}(은)는 전투모드에서는 사용할 수가 없습니다.");
								return;
							}

							var enemy = mEncounterEnemyList[battleCommand.EnemyID];

							if (battleCommand.Tool == 2)
							{
								if (battleCommand.Player.ESP < 15)
								{
									battleResult.Add($"초감각 지수가 부족했다.");
									return;
								}

								battleCommand.Player.ESP -= 15;

								if (enemy.ENumber != 5 &&
									enemy.ENumber != 9 &&
									enemy.ENumber != 19 &&
									enemy.ENumber != 23 &&
									enemy.ENumber != 26 &&
									enemy.ENumber != 28 &&
									enemy.ENumber != 32 &&
									enemy.ENumber != 34 &&
									enemy.ENumber != 39 &&
									enemy.ENumber != 46 &&
									enemy.ENumber != 52 &&
									enemy.ENumber != 61)
								{
									battleResult.Add($"독심술은 전혀 통하지 않았다");
									return;
								}

								var requireLevel = enemy.Level;
								if (enemy.ENumber == 61)
									requireLevel = 17;

								if (requireLevel > battleCommand.Player.Level[2] && mRand.Next(2) == 0)
								{
									battleResult.Add($"적의 마음을 끌어들이기에는 아직 능력이 부족했다");
									return;
								}

								if (mRand.Next(60) > (battleCommand.Player.Level[2] - requireLevel) * 2 + battleCommand.Player.Accuracy[2])
								{
									battleResult.Add($"적의 마음은 흔들리지 않았다");
									return;
								}

								battleResult.Add($"[color={RGB.LightCyan}]적은 우리의 편이 되었다[/color]");
								JoinMember(enemy.ENumber);
								enemy.Dead = true;
								enemy.Unconscious = true;
								enemy.HP = 0;
								enemy.Level = 0;
							}
							else
							{
								if (battleCommand.Player.ESP < 20)
								{
									battleResult.Add($"초감각 지수가 부족했다.");
									return;
								}

								battleCommand.Player.ESP -= 20;
								var espType = mRand.Next(battleCommand.Player.Level[2]) + 1;

								if (1 <= espType && espType <= 6)
								{
									if (espType == 1 || espType == 2)
										battleResult.Add($"주위의 돌들이 떠올라 {enemy.Name}(을)를 공격하기 시작한다");
									else if (espType == 3 || espType == 4)
										battleResult.Add($"{enemy.Name} 주위의 세균이 그에게 침투하여 해를 입히기 시작한다");
									else
									{
										battleResult.Add($"{battleCommand.Player.GenderPronoun}의 무기가 갑자기 {enemy.Name}에게 달려들기 시작한다");
									}

									var impactPoint = enemy.HP;
									if (impactPoint < espType * 10)
										impactPoint = 0;
									else
										impactPoint -= espType * 10;

									enemy.HP = impactPoint;
									if (enemy.Unconscious && !enemy.Dead)
									{
										enemy.Dead = true;
										plusExperience(enemy);
									}
									else if (impactPoint == 0 && !enemy.Unconscious)
									{
										enemy.Unconscious = true;
										plusExperience(enemy);
									}
								}
								else if (7 <= espType && espType == 10)
								{
									if (espType == 1 || espType == 2)
										battleResult.Add($"갑자기 땅속의 우라늄이 핵분열을 일으켜 고온의 열기가 적의 주위를 감싸기 시작한다");
									else
										battleResult.Add($"공기중의 수소가 돌연히 핵융합을 일으켜 질량 결손의 에너지를 적들에게 방출하기 시작한다");

									foreach (var enemyOne in mEncounterEnemyList)
									{
										var impactPoint = enemyOne.HP;
										if (impactPoint < espType * 5)
											impactPoint = 0;
										else
											impactPoint -= espType * 5;
										enemyOne.HP = impactPoint;

										if (enemyOne.Unconscious && !enemyOne.Dead)
										{
											enemyOne.Dead = true;
											plusExperience(enemyOne);
										}
										else if (impactPoint == 0 && !enemyOne.Unconscious)
										{
											enemyOne.Unconscious = true;
											plusExperience(enemyOne);
										}
									}
								}
								else if (11 <= espType && espType <= 12)
								{
									battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적에게 공포심을 불어 넣었다");

									if (mRand.Next(40) < enemy.Resistance)
									{
										if (enemy.Resistance < 5)
											enemy.Resistance = 0;
										else
											enemy.Resistance -= 5;
										return;
									}

									if (mRand.Next(60) > battleCommand.Player.Accuracy[2])
									{
										if (enemy.Endurance < 5)
											enemy.Endurance = 0;
										else
											enemy.Endurance -= 5;

										return;
									}

									enemy.Dead = true;
									battleResult.Add($"{enemy.Name}(은)는 겁을 먹고는 도망 가버렸다");
								}
								else if (13 <= espType && espType <= 14)
								{
									battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적의 신진 대사를 조절하여 적의 체력을 점차 약화 시키려 한다");

									if (mRand.Next(100) < enemy.Resistance)
										return;

									if (mRand.Next(40) > battleCommand.Player.Accuracy[2])
										return;

									enemy.Posion = true;
								}
								else if (15 <= espType && espType <= 17)
								{
									battleResult.Add($"{battleCommand.Player.GenderPronoun}는 염력으로 적의 심장을 멈추려 한다");

									if (mRand.Next(40) < enemy.Resistance)
									{
										if (enemy.Resistance < 5)
											enemy.Resistance = 0;
										else
											enemy.Resistance -= 5;
										return;
									}

									if (mRand.Next(80) > battleCommand.Player.Accuracy[2])
									{
										if (enemy.HP < 10)
										{
											enemy.HP = 0;
											enemy.Unconscious = true;
										}
										else
											enemy.HP -= 5;

										return;
									}

									enemy.Unconscious = true;
								}
								else
								{
									battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적을 환상속에 빠지게 하려한다");

									if (mRand.Next(40) < enemy.Resistance)
									{
										if (enemy.Agility < 5)
											enemy.Agility = 0;
										else
											enemy.Agility -= 5;
										return;
									}

									if (mRand.Next(30) > battleCommand.Player.Accuracy[2])
										return;

									for (var i = 0; i < enemy.Accuracy.Length; i++)
									{
										if (enemy.Accuracy[i] > 0)
											enemy.Accuracy[i]--;
									}
								}
							}
						}


						if (battleCommand.Method == 0)
						{
							AttackOne();
						}
						else if (battleCommand.Method == 1)
						{
							CastOne();
						}
						else if (battleCommand.Method == 2)
						{
							for (var i = 0; i < mEncounterEnemyList.Count; i++)
							{
								battleCommand.EnemyID = i;
								CastOne();
							}
						}
						else if (battleCommand.Method == 3)
						{
							CastSpecialMagic();
						}
						else if (battleCommand.Method == 4)
						{
							if (battleCommand.FriendID < mPlayerList.Count)
								CureSpell(battleCommand.Player, mPlayerList[battleCommand.FriendID], battleCommand.Tool, battleResult);
							else
								CureAllSpell(battleCommand.Player, battleCommand.Tool, battleResult);
						}
						else if (battleCommand.Method == 5)
						{
							CastESP();
						}
						else if (battleCommand.Method == 6)
						{
							if (mRand.Next(50) > battleCommand.Player.Agility)
								battleResult.Add($"그러나, 일행은 성공하지 못했다");
							else
							{
								mBattleTurn = BattleTurn.RunAway;
								battleResult.Add($"[color={RGB.LightCyan}]성공적으로 도망을 갔다");

								mParty.Etc[5] = 2;
							}
						}

						ShowBattleResult(battleResult);
					}
					else {
						BattleEnemyData enemy = null;

						do
						{
							enemy = mBatteEnemyQueue.Dequeue();

							if (enemy.Posion)
							{
								if (enemy.Unconscious)
									enemy.Dead = true;
								else
								{
									enemy.HP--;
									if (enemy.HP <= 0)
										enemy.Unconscious = true;
								}
							}

							if (!enemy.Unconscious && !enemy.Dead)
								break;
							else
								enemy = null;
						} while (mBatteEnemyQueue.Count > 0);


						if (enemy == null)
						{
							mBattleTurn = BattleTurn.Win;
							ContinueText.Visibility = Visibility.Visible;
						}
						else
						{
							var battleResult = new List<string>();

							var liveEnemyCount = 0;
							foreach (var otherEnemy in mEncounterEnemyList)
							{
								if (!otherEnemy.Dead)
									liveEnemyCount++;
							}

							if (enemy.SpecialCastLevel > 0 && enemy.ENumber == 0)
							{
								if (liveEnemyCount < (mRand.Next(3) + 2) && mRand.Next(3) == 0)
								{
									var newEnemy = JoinEnemy(enemy.ENumber + mRand.Next(4) - 20);
									DisplayEnemy();
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {newEnemy.Name}(을)를 생성시켰다[/color]");
								}

								if (enemy.SpecialCastLevel > 1)
								{
									liveEnemyCount = 0;
									foreach (var otherEnemy in mEncounterEnemyList)
									{
										if (!otherEnemy.Dead)
											liveEnemyCount++;
									}

									if (mPlayerList.Count >= 6 && liveEnemyCount < 7 && (mRand.Next(5) == 0))
									{
										var turnEnemy = TurnMind(mPlayerList[5]);
										mPlayerList.RemoveAt(5);

										DisplayPlayerInfo();
										DisplayEnemy();

										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {turnEnemy.Name}(을)를 자기편으로 끌어들였다[/color]");
									}
								}

								if (enemy.SpecialCastLevel > 2 && enemy.Special != 0 && mRand.Next(5) == 0)
								{
									foreach (var player in mPlayerList)
									{
										if (player.Dead == 0)
										{
											battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}에게 죽음의 공격을 시도했다[/color]");

											if (mRand.Next(60) > player.Agility)
												battleResult.Add($"죽음의 공격은 실패했다");
											else if (mRand.Next(20) < player.Luck)
												battleResult.Add($"그러나, {player.Name}(은)는 죽음의 공격을 피했다");
											else
											{
												battleResult.Add($"[color={RGB.Red}]{player.Name}(은)는 죽었다 !!");

												if (player.Dead == 0)
												{
													player.Dead = 1;
													if (player.HP > 0)
														player.HP = 0;
												}
											}
										}
									}
								}
							}

							var agility = enemy.Agility;
							if (agility > 20)
								agility = 20;

							var specialAttack = false;
							if (enemy.Special > 0 && mRand.Next(50) < agility)
							{
								void EnemySpecialAttack()
								{
									if (enemy.Special == 1)
									{
										var normalList = new List<Lore>();

										foreach (var player in mPlayerList)
										{
											if (player.Poison == 0)
												normalList.Add(player);
										}

										var destPlayer = normalList[mRand.Next(normalList.Count)];

										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 독 공격을 시도했다[/color]");
										if (mRand.Next(40) > enemy.Agility)
										{
											battleResult.Add($"독 공격은 실패했다");
											return;
										}

										if (mRand.Next(20) < destPlayer.Luck)
										{
											battleResult.Add($"그러나, {destPlayer.Name}(은)는 독 공격을 피했다");
											return;
										}

										battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 중독 되었다 !!");

										if (destPlayer.Poison == 0)
											destPlayer.Poison = 1;
									}
									else if (enemy.Special == 2)
									{
										var normalList = new List<Lore>();

										foreach (var player in mPlayerList)
										{
											if (player.Unconscious == 0)
												normalList.Add(player);
										}

										var destPlayer = normalList[mRand.Next(normalList.Count)];

										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 치명적 공격을 시도했다[/color]");
										if (mRand.Next(50) > enemy.Agility)
										{
											battleResult.Add($"치명적 공격은 실패했다");
											return;
										}

										if (mRand.Next(20) < destPlayer.Luck)
										{
											battleResult.Add($"그러나, {destPlayer.Name}(은)는 치명적 공격을 피했다");
											return;
										}

										battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 의식불명이 되었다 !!");

										if (destPlayer.Unconscious == 0)
										{
											destPlayer.Unconscious = 1;

											if (destPlayer.HP > 0)
												destPlayer.HP = 0;
										}
									}
									else if (enemy.Special == 3)
									{
										var normalList = new List<Lore>();

										foreach (var player in mPlayerList)
										{
											if (player.Dead == 0)
												normalList.Add(player);
										}

										var destPlayer = normalList[mRand.Next(normalList.Count)];

										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 죽음의 공격을 시도했다[/color]");
										if (mRand.Next(60) > enemy.Agility)
										{
											battleResult.Add($"죽음의 공격은 실패했다");
											return;
										}

										if (mRand.Next(20) < destPlayer.Luck)
										{
											battleResult.Add($"그러나, {destPlayer.Name}(은)는 죽음의 공격을 피했다");
											return;
										}

										battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 죽었다 !!");

										if (destPlayer.Dead == 0)
										{
											destPlayer.Dead = 1;

											if (destPlayer.HP > 0)
												destPlayer.HP = 0;
										}
									}
								}

								liveEnemyCount = 0;
								foreach (var otherEnemy in mEncounterEnemyList)
								{
									if (!otherEnemy.Dead)
										liveEnemyCount++;
								}

								if (liveEnemyCount > 3)
								{
									EnemySpecialAttack();
									specialAttack = true;
								}
							}


							if (!specialAttack)
							{
								void EnemyAttack()
								{
									if (mRand.Next(20) >= enemy.Accuracy[0])
									{
										battleResult.Add($"{enemy.Name}(은)는 빗맞추었다");
										return;
									}

									var normalList = new List<Lore>();

									foreach (var player in mPlayerList)
									{
										if (player.IsAvailable)
											normalList.Add(player);
									}

									var destPlayer = normalList[mRand.Next(normalList.Count)];

									var attackPoint = enemy.Strength * enemy.Level * (mRand.Next(10) + 1) / 10;

									if (mRand.Next(50) < destPlayer.Resistance)
									{
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}(을)를 공격했다[/color]");
										battleResult.Add($"그러나, {destPlayer.Name}(은)는 적의 공격을 저지했다");
										return;
									}

									attackPoint -= destPlayer.AC * destPlayer.Level[0] * (mRand.Next(10) + 1) / 10;

									if (attackPoint <= 0)
									{
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}(을)를 공격했다[/color]");
										battleResult.Add($"그러나, {destPlayer.Name}(은)는 적의 공격을 방어했다");
										return;
									}

									if (destPlayer.Dead > 0)
										destPlayer.Dead += attackPoint;

									if (destPlayer.Unconscious > 0 && destPlayer.Dead == 0)
										destPlayer.Unconscious += attackPoint;

									if (destPlayer.HP > 0)
										destPlayer.HP -= attackPoint;

									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 공격 받았다[/color]");
									battleResult.Add($"[color={RGB.Magenta}]{destPlayer.Name}(은)는[/color] [color={RGB.LightMagenta}]{attackPoint}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
								}

								if (mRand.Next(enemy.Accuracy[0] * 1000) > mRand.Next(enemy.Accuracy[1] * 1000) && enemy.Strength > 0)
								{
									EnemyAttack();
								}
								else
								{
									void CastAttack(int castPower, Lore player)
									{
										if (mRand.Next(20) >= enemy.Accuracy[1])
										{
											battleResult.Add($"{enemy.Name}의 마법공격은 빗나갔다");
											return;
										}

										castPower -= mRand.Next(castPower / 2);
										castPower -= player.AC * player.Level[0] * (mRand.Next(10) + 1) / 10;
										if (castPower <= 0)
										{
											battleResult.Add($"그러나, {player.Name}(은)는 적의 마법을 막아냈다");
											return;
										}

										if (player.Dead > 0)
											player.Dead += castPower;

										if (player.Unconscious > 0 && player.Dead == 0)
											player.Unconscious += castPower;

										if (player.HP > 0)
											player.HP -= castPower;

										battleResult.Add($"[color={RGB.Magenta}]{player.Name}(은)는[/color] [color={RGB.LightMagenta}]{castPower}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
									}

									void CastAttckOne(Lore player)
									{
										string castName;
										int castPower;
										if (1 <= enemy.Mentality && enemy.Mentality <= 3)
										{
											castName = "충격";
											castPower = 1;
										}
										else if (4 <= enemy.Mentality && enemy.Mentality <= 8)
										{
											castName = "냉기";
											castPower = 2;
										}
										else if (9 <= enemy.Mentality && enemy.Mentality <= 10)
										{
											castName = "고통";
											castPower = 4;
										}
										else if (11 <= enemy.Mentality && enemy.Mentality <= 14)
										{
											castName = "고통";
											castPower = 6;
										}
										else if (15 <= enemy.Mentality && enemy.Mentality <= 18)
										{
											castName = "고통";
											castPower = 7;
										}
										else
										{
											castName = "번개";
											castPower = 10;
										}

										castPower *= enemy.Level;
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {player.Name}에게 '{castName}'마법을 사용했다[/color]");

										CastAttack(castPower, player);
									}

									void CastAttackAll(List<Lore> destPlayerList)
									{
										string castName;
										int castPower;
										if (1 <= enemy.Mentality && enemy.Mentality <= 6)
										{
											castName = "열파";
											castPower = 1;
										}
										else if (7 <= enemy.Mentality && enemy.Mentality <= 12)
										{
											castName = "에너지";
											castPower = 2;
										}
										else if (13 <= enemy.Mentality && enemy.Mentality <= 16)
										{
											castName = "초음파";
											castPower = 3;
										}
										else if (17 <= enemy.Mentality && enemy.Mentality <= 20)
										{
											castName = "혹한기";
											castPower = 5;
										}
										else
										{
											castName = "화염폭풍";
											castPower = 8;
										}

										castPower *= enemy.Level;
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 일행 모두에게 '{castName}'마법을 사용했다[/color]");

										foreach (var player in destPlayerList)
											CastAttack(castPower, player);
									}

									void CureEnemy(BattleEnemyData whomEnemy, int curePoint)
									{
										if (enemy == whomEnemy)
											battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 자신을 치료했다[/color]");
										else
											battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {whomEnemy.Name}(을)를 치료했다[/color]");

										if (whomEnemy.Dead)
											whomEnemy.Dead = false;
										else if (whomEnemy.Unconscious)
										{
											whomEnemy.Unconscious = false;
											if (whomEnemy.HP <= 0)
												whomEnemy.HP = 1;
										}
										else
										{
											whomEnemy.HP += curePoint;
											if (whomEnemy.HP > whomEnemy.Endurance * whomEnemy.Level)
												whomEnemy.HP = whomEnemy.Endurance * whomEnemy.Level;
										}
									}

									void CastHighLevel(List<Lore> destPlayerList)
									{
										if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(3) == 0)
										{
											CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
											return;
										}

										var avgAC = 0;
										var avgCount = 0;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable)
											{
												avgAC += player.AC;
												avgCount++;
											}
										}

										avgAC /= avgCount;

										if (avgAC > 4 && mRand.Next(5) == 0)
										{
											foreach (var player in mPlayerList)
											{
												battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {player.Name}의 갑옷파괴를 시도했다[/color]");
												if (player.Luck > mRand.Next(21))
													battleResult.Add($"그러나, {enemy.Name}(은)는 성공하지 못했다");
												else
												{
													battleResult.Add($"[color={RGB.Magenta}]{player.Name}의 갑옷은 파괴되었다[/color]");

													if (player.AC > 0)
														player.AC--;
												}
											}

											DisplayPlayerInfo();
										}
										else
										{
											var totalCurrentHP = 0;
											var totalFullHP = 0;

											foreach (var enemyOne in mEncounterEnemyList)
											{
												totalCurrentHP += enemyOne.HP;
												totalFullHP += enemyOne.Endurance * enemyOne.Level;
											}

											totalFullHP /= 3;

											if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(3) == 0)
											{
												foreach (var enemyOne in mEncounterEnemyList)
													CureEnemy(enemyOne, enemy.Level * enemy.Mentality / 6);
											}
											else if (mRand.Next(destPlayerList.Count) < 2)
											{
												Lore weakestPlayer = null;

												foreach (var player in mPlayerList)
												{
													if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
														weakestPlayer = player;
												}

												CastAttckOne(weakestPlayer);
											}
											else
												CastAttackAll(destPlayerList);
										}
									}


									var normalList = new List<Lore>();

									foreach (var player in mPlayerList)
									{
										if (player.IsAvailable)
											normalList.Add(player);
									}

									var destPlayer = normalList[mRand.Next(normalList.Count)];

									if (enemy.CastLevel == 1)
									{
										CastAttckOne(destPlayer);
									}
									else if (enemy.CastLevel == 2)
									{
										CastAttckOne(destPlayer);
									}
									else if (enemy.CastLevel == 3)
									{
										if (mRand.Next(normalList.Count) < 2)
											CastAttckOne(destPlayer);
										else
											CastAttackAll(normalList);
									}
									else if (enemy.CastLevel == 4)
									{
										if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(2) == 0)
											CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
										else if (mRand.Next(normalList.Count) < 2)
											CastAttckOne(destPlayer);
										else
											CastAttackAll(normalList);
									}
									else if (enemy.CastLevel == 5)
									{
										if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(3) == 0)
											CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
										else if (mRand.Next(normalList.Count) < 2)
										{
											var totalCurrentHP = 0;
											var totalFullHP = 0;

											foreach (var enemyOne in mEncounterEnemyList)
											{
												totalCurrentHP += enemyOne.HP;
												totalFullHP += enemyOne.Endurance * enemyOne.Level;
											}

											totalFullHP /= 3;

											if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(2) == 0)
											{
												foreach (var enemyOne in mEncounterEnemyList)
													CureEnemy(enemyOne, enemy.Level * enemy.Mentality / 6);
											}
											else
											{
												Lore weakestPlayer = null;

												foreach (var player in mPlayerList)
												{
													if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
														weakestPlayer = player;
												}

												CastAttckOne(weakestPlayer);
											}
										}
										else
											CastAttackAll(normalList);
									}
									else if (enemy.CastLevel == 6)
									{
										CastHighLevel(normalList);
									}
								}
							}

							ShowBattleResult(battleResult);
						}
					}
				}

				if (mMoveEvent || mLoading)
					return;
				else if (mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (mSpecialEvent == 3)
				{
					mSpecialEvent = 0;

					await ExitCastleLore();
				}
				else if (mSpecialEvent == 4) {
					TalkMode(mTalkX, mTalkY, args.VirtualKey);
				}
				else if (ContinueText.Visibility == Visibility.Visible) {
					ContinueText.Visibility = Visibility.Collapsed;

					if (mBattleTurn == BattleTurn.None)
					{
						DialogText.Blocks.Clear();
						DialogText.TextHighlighters.Clear();
					}

					if (mSpecialEvent == 1)
					{
						mParty.Etc[49] |= 1 << 4;
						mSpecialEvent = 0;
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
					else if (mTalkMode > 0)
					{
						if (mSpecialEvent == 0)
							TalkMode(mTalkX, mTalkY, args.VirtualKey);
					}
					else if (mBattleTurn == BattleTurn.Player)
					{
						ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.Enemy)
					{
						ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.RunAway || mBattleTurn == BattleTurn.Win || mBattleTurn == BattleTurn.Lose)
					{
						EndBattle();
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
					void ShowCastOneMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount;
						if (0 <= player.Level[1] && player.Level[1] <= 1)
							availCount = 2;
						else if (2 <= player.Level[1] && player.Level[1] <= 3)
							availCount = 3;
						else if (4 <= player.Level[1] && player.Level[1] <= 7)
							availCount = 4;
						else if (8 <= player.Level[1] && player.Level[1] <= 11)
							availCount = 5;
						else if (12 <= player.Level[1] && player.Level[1] <= 15)
							availCount = 6;
						else
							availCount = 7;

						menuStr = new string[availCount];
						for (var i = 1; i <= availCount; i++)
						{
							menuStr[i - 1] = Common.GetMagicStr(i);
						}

						ShowMenu(MenuMode.CastOneMagic, menuStr);
					}

					void ShowCastAllMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount;
						if (0 <= player.Level[1] && player.Level[1] <= 1)
							availCount = 1;
						else if (player.Level[1] == 2)
							availCount = 2;
						else if (3 <= player.Level[1] && player.Level[1] <= 5)
							availCount = 3;
						else if (6 <= player.Level[1] && player.Level[1] <= 9)
							availCount = 4;
						else if (10 <= player.Level[1] && player.Level[1] <= 13)
							availCount = 5;
						else if (14 <= player.Level[1] && player.Level[1] <= 17)
							availCount = 6;
						else
							availCount = 7;

						menuStr = new string[availCount];
						const int allMagicIdx = 6;
						for (var i = 1 + allMagicIdx; i <= availCount + allMagicIdx; i++)
						{
							menuStr[i - allMagicIdx - 1] = Common.GetMagicStr(i);
						}

						ShowMenu(MenuMode.CastAllMagic, menuStr);
					}

					void ShowCastSpecialMenu() {
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount;
						if (0 <= player.Level[1] && player.Level[1] <= 4)
							availCount = 1;
						else if (5 <= player.Level[1] && player.Level[1] <= 9)
							availCount = 2;
						else if (10 <= player.Level[1] && player.Level[1] <= 11)
							availCount = 3;
						else if (12 <= player.Level[1] && player.Level[1] <= 13)
							availCount = 4;
						else if (14 <= player.Level[1] && player.Level[1] <= 15)
							availCount = 5;
						else if (16 <= player.Level[1] && player.Level[1] <= 17)
							availCount = 6;
						else
							availCount = 7;

						menuStr = new string[availCount];
						const int allSpecialIdx = 12;
						for (var i = 1 + allSpecialIdx; i <= availCount + allSpecialIdx; i++)
						{
							menuStr[i - allSpecialIdx - 1] = Common.GetMagicStr(i);
						}

						ShowMenu(MenuMode.CastSpecial, menuStr);
					}

					void ShowCureDestMenu(Lore player, MenuMode menuMode)
					{
						AppendText(new string[] { "누구에게" });
						string[] playerList;

						if (player.Level[1] / 2 - 3 < 0)
							playerList = new string[mPlayerList.Count];
						else
							playerList = new string[mPlayerList.Count + 1];

						for (var i = 0; i < mPlayerList.Count; i++)
							playerList[i] = mPlayerList[i].Name;

						if (player.Level[1] / 2 - 3 > 0)
							playerList[playerList.Length - 1] = "모든 사람들에게";

						ShowMenu(menuMode, playerList);
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								if (mEnemyFocusID == 0)
									mEnemyFocusID = mEncounterEnemyList.Count - 1;
								else
									mEnemyFocusID--;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);


							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else {
							if (mMenuFocusID == 0)
								mMenuFocusID = mMenuCount - 1;
							else
								mMenuFocusID--;

							FocusMenuItem();
						}

					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mMenuMode == MenuMode.EnemySelectMode) {
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								mEnemyFocusID = (mEnemyFocusID + 1) % mEncounterEnemyList.Count;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else {
							mMenuFocusID = (mMenuFocusID + 1) % mMenuCount;
							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						if (mMenuMode != MenuMode.None && mSpecialEvent == 0)
						{
							if (mMenuMode == MenuMode.CastOneMagic ||
							mMenuMode == MenuMode.CastAllMagic ||
							mMenuMode == MenuMode.CastSpecial ||
							mMenuMode == MenuMode.ChooseBattleCureSpell ||
							mMenuMode == MenuMode.CastESP)
							{
								BattleMode();
							}
							else if (mMenuMode == MenuMode.EnemySelectMode)
							{
								mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

								switch (mBattleCommandID)
								{
									case 0:
										BattleMode();
										break;
									case 1:
										ShowCastOneMagicMenu();
										break;
									case 3:
										ShowCastSpecialMenu();
										break;
								}
							}
							else if (mMenuMode == MenuMode.ApplyBattleCureSpell)
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							else if (mMenuMode == MenuMode.BattleStart || 
								mMenuMode == MenuMode.BattleCommand ||
								mMenuMode == MenuMode.JoinSkeleton)
								return;
							else
							{
								AppendText(new string[] { "" });
								HideMenu();

								mMenuMode = MenuMode.None;
							}

						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						void SelectEnemy()
						{
							mMenuMode = MenuMode.EnemySelectMode;
							
							for (var i = 0; i < mEncounterEnemyList.Count; i++)
							{
								if (!mEncounterEnemyList[i].Dead)
								{
									mEnemyFocusID = i;
									break;
								}
							}

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}

						void AddBattleCommand() {
							mMenuMode = MenuMode.None;

							mBattleCommandQueue.Enqueue(new BattleCommand()
							{
								Player = mPlayerList[mBattlePlayerID],
								FriendID = mBattleFriendID,
								Method = mBattleCommandID,
								Tool = mBattleToolID,
								EnemyID = mEnemyFocusID
							});

							if (mEnemyFocusID >= 0)
								mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							do
							{
								mBattlePlayerID++;
							} while (mBattlePlayerID < mPlayerList.Count && !mPlayerList[mBattlePlayerID].IsAvailable);

							if (mBattlePlayerID < mPlayerList.Count)
								BattleMode();
							else
							{
								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								mBattleTurn = BattleTurn.Player;

								ExecuteBattle();
							}
						}

						async Task LoadGame() {
							await LoadFile();

							mMenuMode = MenuMode.None;

							mBattlePlayerID = 0;
							mBattleFriendID = 0;
							mBattleCommandID = 0;
							mBattleToolID = 0;
							mEnemyFocusID = 0;
							mBattleCommandQueue.Clear();
							mBatteEnemyQueue.Clear();
							mBattleTurn = BattleTurn.None;

							ShowMap();

							AppendText(new string[] { $"[color={RGB.LightCyan}]저장했던 게임을 다시 불러옵니다[/color]" });
						}

						if (mMenuMode == MenuMode.EnemySelectMode) {
							AddBattleCommand();
						}
						else {
							void ShowCureSpellMenu(Lore player, int whomID, MenuMode applyCureMode, MenuMode applyAllCureMode) {
								if (whomID < mPlayerList.Count)
								{
									ShowMenu(applyCureMode, GetCureSpellList(player));
								}
								else
								{
									var availableCount = mMagicPlayer.Level[1] / 2 - 3;
									var totalMagicCount = 32 - 26 + 1;
									if (availableCount < totalMagicCount)
										totalMagicCount = availableCount;

									var cureMagicMenu = new string[totalMagicCount];
									for (var i = 26; i < 26 + availableCount; i++)
										cureMagicMenu[i - 26] = Common.GetMagicStr(i);

									ShowMenu(applyAllCureMode, cureMagicMenu);
								}
							}

							string[] GetCureSpellList(Lore player) {
								var availableCount = player.Level[1] / 2 + 1;
								if (availableCount > 7)
									availableCount = 7;

								var totalMagicCount = 25 - 19 + 1;
								if (availableCount < totalMagicCount)
									totalMagicCount = availableCount;

								var cureMagicMenu = new string[totalMagicCount];
								for (var i = 19; i < 19 + availableCount; i++)
									cureMagicMenu[i - 19] = Common.GetMagicStr(i);

								return cureMagicMenu;
							}

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
								{
									AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
									ShowCharacterMenu(MenuMode.CastSpell);
								}
								else if (mMenuFocusID == 4)
								{
									AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
									ShowCharacterMenu(MenuMode.Extrasense);
								}
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
								$"# 성별 : {mPlayerList[mMenuFocusID].GenderPronoun}",
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

								if (mPlayerList[mMenuFocusID].IsAvailable)
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
									ShowCureDestMenu(mMagicPlayer, MenuMode.ChooseCureSpell);
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

								AppendText(new string[] { "선택" });

								if (mMenuFocusID < mPlayerList.Count)
									mMagicWhomPlayer = mPlayerList[mMenuFocusID];

								ShowCureSpellMenu(mMagicPlayer, mMenuFocusID, MenuMode.ApplyCureMagic, MenuMode.ApplyCureAllMagic);
							}
							else if (mMenuMode == MenuMode.ChooseBattleCureSpell) {
								mMenuMode = MenuMode.None;

								mBattleFriendID = mMenuFocusID;

								ShowCureSpellMenu(mPlayerList[mBattlePlayerID], mMenuFocusID, MenuMode.ApplyBattleCureSpell, MenuMode.ApplyBattleCureSpell);
							}
							else if (mMenuMode == MenuMode.ApplyCureMagic)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID);

								ContinueText.Visibility = Visibility.Visible;
							}
							else if (mMenuMode == MenuMode.ApplyCureAllMagic)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureAllSpell(mMagicPlayer, mMenuFocusID);
								
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
										{
											mParty.Etc[0]++;
											ShowMap();
										}

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

								if (mPlayerList[mMenuFocusID].IsAvailable)
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
									LoadGame();
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
							else if (mMenuMode == MenuMode.ConfirmExitMap)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									if (mParty.Map == 6)
									{
										if ((mParty.Etc[30] & 1) == 0)
										{
											AppendText(new string[] {
												" 당신이 로어 성을 떠나려는 순간 누군가가 당신을 불렀다."
											});

											mTalkMode = 1;
											mTalkX = mParty.XAxis;
											mTalkY = mParty.YAxis;
											mSpecialEvent = 2;

											ContinueText.Visibility = Visibility.Visible;
										}
										else
											ExitCastleLore();
									}
									else if (mParty.Map == 14) {
										mParty.Map = 1;
										mParty.XAxis = 17;
										mParty.YAxis = 88;

										await RefreshGame();
									}
								}
								else
								{
									AppendText(new string[] { "" });
									mParty.YAxis--;
								}
							}
							else if (mMenuMode == MenuMode.JoinSkeleton)
							{
								mMenuMode = MenuMode.None;
								
								mParty.Etc[30] |= 1;

								if (mMenuFocusID == 0)
								{
									JoinMember(18);
									ExitCastleLore();
								}
								else if (mMenuFocusID == 1)
								{
									ShowNoThanks();
									mSpecialEvent = 3;
								}
							}
							else if (mMenuMode == MenuMode.BattleStart)
							{
								mMenuMode = MenuMode.None;

								void StartBattle()
								{
									mBattleTurn = BattleTurn.Player;

									mParty.Etc[5] = 1;
									for (var i = 0; i < mPlayerList.Count; i++)
									{
										if (mPlayerList[i].IsAvailable)
										{
											mBattlePlayerID = i;
											break;
										}
									}

									mBattleCommandQueue.Clear();

									BattleMode();
								}

								var avgLuck = 0;
								mPlayerList.ForEach(delegate (Lore player)
								{
									avgLuck += player.Luck;
								});
								avgLuck /= mPlayerList.Count;

								var avgAgility = 0;
								mEncounterEnemyList.ForEach(delegate (BattleEnemyData enemy)
								{
									avgAgility += enemy.Agility;
								});
								avgAgility /= mEncounterEnemyList.Count;

								if (mMenuFocusID == 0)
								{
									StartBattle();
								}
								else if (mMenuFocusID == 1)
								{
									if (avgLuck > avgAgility)
									{
										mBattleTurn = BattleTurn.RunAway;
										EndBattle();
									}
									else
										StartBattle();
								}
							}
							else if (mMenuMode == MenuMode.BattleCommand)
							{
								mMenuMode = MenuMode.None;

								mBattleCommandID = mMenuFocusID;

								if (mMenuFocusID == 0)
								{
									SelectEnemy();
								}
								else if (mMenuFocusID == 1)
								{
									ShowCastOneMagicMenu();
								}
								else if (mMenuFocusID == 2)
								{
									ShowCastAllMagicMenu();
								}
								else if (mMenuFocusID == 3)
								{
									ShowCastSpecialMenu();
								}
								else if (mMenuFocusID == 4)
								{
									ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
								}
								else if (mMenuFocusID == 5)
								{
									var menuStr = new string[5];
									const int allESPIdx = 40;
									for (var i = 1 + allESPIdx; i <= menuStr.Length + allESPIdx; i++)
									{
										menuStr[i - allESPIdx - 1] = Common.GetMagicStr(i);
									}

									ShowMenu(MenuMode.CastESP, menuStr);
								}
								else if (mMenuFocusID == 6) {
									if (mBattlePlayerID == 0) {
										foreach (var player in mPlayerList) {
											if (player.IsAvailable)
											{
												var method = 0;
												var tool = 0;
												var enemyID = 0;

												if (player.Class == 0 || player.Class == 1 || (4 <= player.Class && player.Class <= 8) || player.Class == 10) {
													method = 0;
													tool = 0;
												}
												else if (player.Class == 2 || player.Class == 9) {
													method = 1;
													if (0 <= player.Level[1] && player.Level[1] <= 1)
														tool = 1;
													else if (2 <= player.Level[1] && player.Level[1] <= 3)
														tool = 2;
													else if (4 <= player.Level[1] && player.Level[1] <= 7)
														tool = 3;
													else if (8 <= player.Level[1] && player.Level[1] <= 11)
														tool = 4;
													else if (12 <= player.Level[1] && player.Level[1] <= 15)
														tool = 5;
													else
														tool = 6;

													tool = (int)Math.Round((double)tool / 2);
													if (tool == 0)
														tool = 1;
												}
												else if (player.Class == 3) {
													method = 5;
													tool = 4;

													for (var i = 0; i < mEncounterEnemyList.Count; i++)
													{
														if (!mEncounterEnemyList[i].Unconscious && !mEncounterEnemyList[i].Dead) {
															enemyID = i;
															break;
														}
													}
												}

												mBattleCommandQueue.Enqueue(new BattleCommand()
												{
													Player = player,
													FriendID = 0,
													Method = method,
													Tool = tool,
													EnemyID = enemyID
												});
											}
										}

										DialogText.TextHighlighters.Clear();
										DialogText.Blocks.Clear();

										ExecuteBattle();
									}
									else {
										AddBattleCommand();
									}
								}
							}
							else if (mMenuMode == MenuMode.CastOneMagic)
							{
								mMenuMode = MenuMode.None;

								mBattleToolID = mMenuFocusID + 1;

								SelectEnemy();
							}
							else if (mMenuMode == MenuMode.CastAllMagic)
							{
								mMenuMode = MenuMode.None;

								mBattleToolID = mMenuFocusID + 1;
								mEnemyFocusID = -1;

								AddBattleCommand();
							}
							else if (mMenuMode == MenuMode.CastSpecial) {
								mMenuMode = MenuMode.None;

								mBattleToolID = mMenuFocusID;

								SelectEnemy();
							}
							else if (mMenuMode == MenuMode.CastESP)
							{
								mMenuMode = MenuMode.None;

								mBattleToolID = mMenuFocusID;

								SelectEnemy();
							}
							else if (mMenuMode == MenuMode.ApplyBattleCureSpell) {
								mMenuMode = MenuMode.None;

								mBattleToolID = mMenuFocusID;

								AddBattleCommand();
							}
							else if (mMenuMode == MenuMode.BattleLose) {
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0) {
									await LoadGame();


								}
								else {
									CoreApplication.Exit();
								}
							}
							else if (mMenuMode == MenuMode.AskEnter) {
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0) {
									switch (mTryEnterType) {
										case EnterType.CastleLore:
											mParty.Map = 6;
											mParty.XAxis = 50;
											mParty.YAxis = 94;

											await RefreshGame();

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

											for (var i = 48; i < 53; i++)
												mMapLayer[i + mMapWidth * 87] = 44;

											break;
										case EnterType.Menace:
											mParty.Map = 14;
											mParty.XAxis = 24;
											mParty.YAxis = 44;

											await RefreshGame();

											break;
										case EnterType.LastDitch:
											mParty.Map = 7;
											mParty.XAxis = 36;
											mParty.YAxis = 69;

											await RefreshGame();

											foreach (var player in mPlayerList) {
												if (player.Name == "폴라리스") {
													mMapLayer[36 + mMapWidth * 40] = 44;
													break;
												}
											}

											break;
									}
								}
								else {
									AppendText(new string[] { "" });
								}
							}
							else if (mMenuMode == MenuMode.ChooseGoldShield) {
								mMenuMode = MenuMode.None;

								var player = mPlayerList[mMenuFocusID];
								player.Shield = 5;
								player.ShiPower = 5;
								player.AC = player.ShiPower + player.ArmPower;
								if (player.Class == 1)
									player.AC++;

								if (player.AC > 10)
									player.AC = 10;

								AppendText(new string[] { $"[color={RGB.White}]{player.Name}(이)가 황금의 방패를 장착했다." });

								UpdatePlayersStat();
								mParty.Etc[31] |= (1 << 6);
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
				else 
				if (player.Dead > 0)
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

						//mParty.Food--;
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
						{
							//mParty.Food++;
						}
					}

					player.HP += recoverPoint;

					if (player.HP >= player.Endurance * player.Level[0])
					{
						player.HP = player.Endurance * player.Level[0];

						AppendText(new string[] { $"[color={RGB.White}]{player.Name}는 모든 건강이 회복되었다[/color]" }, append);
					}
					else
						AppendText(new string[] { $"[color={RGB.White}]{player.Name}는 모든 건강이 회복되었다[/color]" }, append);

					//mParty.Food--;
				}

				if (append == false)
					append = true;
			});

			if (mParty.Etc[0] > 0)
			{
				mParty.Etc[0]--;
				ShowMap();
			}

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
			void FindGold(int gold) {
				AppendText(new string[] { $"당신은 금화 {gold}개를 발견했다." });

				mParty.Gold += gold;
			}

			if (mParty.Map == 6)
			{
				if (mParty.XAxis == 61 && mParty.YAxis == 81)
				{
					AppendText(new string[] { "일행은 상자 속에서 약간의 금을 발견했다." });
					mParty.Gold += 1000;
					mMapLayer[61 + mMapWidth * 81] = 44;
				}
				else if ((mParty.XAxis == 50 && mParty.YAxis == 11) || (mParty.XAxis == 51 && mParty.YAxis == 11))
				{
					if ((mParty.Etc[50] & (1 << 1)) > 0)
					{
						var enemyNumber = 0;

						if ((mParty.Etc[50] & (1 << 2)) > 0)
						{
							AppendText(new string[] { $" 다시 돌아오다니, {mPlayerList[0].Name}",
								" 이번에는 기어이 네놈들을 해치우고야 말겠다. 나의 친구들도 이번에 거들것이다."
							});

							enemyNumber = 7;
						}
						else
						{
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
					if ((mParty.Etc[49] & (1 << 3)) == 0)
					{
						mParty.Etc[49] |= (1 << 3);

						mMapLayer[40 + mMapWidth * 78] = 44;

						mMoveEvent = true;

						for (var i = 0; i < 3; i++)
						{
							Task.Delay(500).Wait();
							mParty.XAxis--;
						}

						AppendText(new string[] { $"  일행은 가장 기본적인 무기로 모두 무장을 하였다." });

						mPlayerList.ForEach(delegate (Lore player)
						{
							if (player.Weapon == 0 && player.Class != 5)
							{
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
			else if (mParty.Map == 14)
			{
				if (mParty.YAxis == 45)
				{
					ShowExitMenu();
				}
				else if (((mParty.XAxis == 24 && mParty.YAxis == 7) || (mParty.XAxis == 25 && mParty.YAxis == 7)) && mParty.Etc[9] == 3)
				{
					AppendText(new string[] {
						$"여기가 '메나스'의 중심이다.",
						$"[color={RGB.White}]당신의 탐험은 성공적이었다.[/color]",
						$"[color={RGB.White}]이제 로드 안에게 돌아가는 일만 남았다.[/color]"
					});

					mParty.Etc[9]++;

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((mParty.XAxis == 5 && mParty.YAxis == 5) && (mParty.Etc[31] & 1) == 0)
				{
					FindGold(1000);
					mParty.Etc[31] |= 1;
				}
				else if ((mParty.XAxis == 17 && mParty.YAxis == 9) && (mParty.Etc[31] & (1 << 1)) == 0)
				{
					FindGold(2500);
					mParty.Etc[31] |= (1 << 1);
				}
				else if ((mParty.XAxis == 5 && mParty.YAxis == 43) && (mParty.Etc[31] & (1 << 2)) == 0)
				{
					FindGold(400);
					mParty.Etc[31] |= (1 << 2);
				}
				else if ((mParty.XAxis == 30 && mParty.YAxis == 29) && (mParty.Etc[31] & (1 << 3)) == 0)
				{
					FindGold(600);
					mParty.Etc[31] |= (1 << 3);
				}
				else if ((mParty.XAxis == 30 && mParty.YAxis == 7) && (mParty.Etc[31] & (1 << 4)) == 0)
				{
					FindGold(1500);
					mParty.Etc[31] |= (1 << 4);
				}
				else if ((mParty.XAxis == 13 && mParty.YAxis == 27) && (mParty.Etc[31] & (1 << 5)) == 0)
				{
					FindGold(1000);
					mParty.Etc[31] |= (1 << 5);
				}
				else if ((mParty.XAxis == 15 && mParty.YAxis == 19) && (mParty.Etc[31] & (1 << 6)) == 0)
				{ 
					AppendText(new string[] {
						$"[color={RGB.White}]당신은 황금의 방패를 발견했다.[/color]",
						$"[color={RGB.LightCyan}]누가 이 황금의 방패를 장착 하겠습니까 ?[/color]"
					});

					ShowCharacterMenu(MenuMode.ChooseGoldShield);
				}
			}
		}

		private void ShowCureResult(string message, List<string> cureResult) {
			if (cureResult == null)
				AppendText(new string[] { message }, true);
			else
				cureResult.Add(message);
		}

		private void HealOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0 || whomPlayer.Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 치료될 상태가 아닙니다.", cureResult);
			}
			else if (whomPlayer.HP >= whomPlayer.Endurance * whomPlayer.Level[0])
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 치료할 필요가 없습니다..", cureResult);
			}
			else
			{
				var needSP = 2 * player.Level[1];
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					player.SP -= needSP;
					whomPlayer.HP += needSP * 3 / 2;
					if (whomPlayer.HP > whomPlayer.Level[0] * whomPlayer.Endurance)
						whomPlayer.HP = whomPlayer.Level[0] * whomPlayer.Endurance;

					ShowCureResult($"[color={RGB.White}]{whomPlayer.Name}(은)는 치료되어 졌습니다.[/color]", cureResult);
				}
			}
		}

		private void CureOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 독이 치료될 상태가 아닙니다.", cureResult);
			}
			else if (whomPlayer.Poison == 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 독에 걸리지 않았습니다.", cureResult);
			}
			else if (whomPlayer.SP < 15)
			{
				if (mParty.Etc[5] == 0)
					ShowNotEnoughSP();

			}
			else
			{
				player.SP -= 15;
				whomPlayer.Poison = 0;

				ShowCureResult($"[color={RGB.White}]{whomPlayer.Name}의 독은 제거 되었습니다.[/color]", cureResult);
			}
		}

		private void ConsciousOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 의식이 돌아올 상태가 아닙니다.", cureResult);
			}
			else if (whomPlayer.Unconscious == 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 의식불명이 아닙니다.", cureResult);
			}
			else
			{
				var needSP = 10 * whomPlayer.Unconscious;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					player.SP -= needSP;
					whomPlayer.Unconscious = 0;
					if (whomPlayer.HP <= 0)
						whomPlayer.HP = 1;

					ShowCureResult($"{whomPlayer.Name}(은)는 의식을 되찾았습니다.", cureResult);
				}
			}
		}

		private void RevitalizeOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead == 0)
			{
				if (mParty.Etc[5] == 0)
					ShowCureResult($"{whomPlayer.Name}(은)는 아직 살아 있습니다.", cureResult);
			}
			else
			{
				var needSP = player.Dead * 30;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP();
				}
				else
				{
					player.SP -= needSP;
					whomPlayer.Dead = 0;
					if (whomPlayer.Unconscious > whomPlayer.Endurance * whomPlayer.Level[0])
						whomPlayer.Unconscious = whomPlayer.Endurance * whomPlayer.Level[0];

					if (whomPlayer.Unconscious == 0)
						whomPlayer.Unconscious = 1;

					ShowCureResult($"{whomPlayer.Name}(은)는 다시 생명을 얻었습니다.", cureResult);

				}
			}
		}

		private void HealAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				HealOne(player, whomPlayer, cureResult);
			});
		}

		private void CureAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				CureOne(player, whomPlayer, cureResult);
			});
		}

		private void ConsciousAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				ConsciousOne(player, whomPlayer, cureResult);
			});
		}

		private void RevitalizeAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				RevitalizeOne(player, whomPlayer, cureResult);
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

		private string GetGenderData(Lore player)
		{
			if (player.Gender == "male")
				return "그";
			else
				return "그녀";
		}

		private void ShowCharacterMenu(MenuMode menuMode)
		{
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

		private void ShowMenu(MenuMode menuMode, List<Tuple<string, Color>> menuItem)
		{
			mMenuMode = menuMode;
			mMenuCount = menuItem.Count;
			mMenuFocusID = 0;

			for (var i = 0; i < mMenuList.Count; i++)
			{
				if (i < mMenuCount)
				{
					mMenuList[i].Text = menuItem[i].Item1;
					mMenuList[i].Foreground = new SolidColorBrush(menuItem[i].Item2);
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
			}

			if (mParty.Map == 6)
			{
				if (moveX == 8 && moveY == 63)
				{
					AppendText(new string[] {
						" 당신이 모험을 시작한다면, 많은 괴물들을 만날 것이오.",
						"무엇보다도, 왕뱀과 벌레떼들과 독사는 맹독이 있으니 주의 하시기 바라오."
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
						AppendText(new string[] { " 당신이 네크로맨서에 진정으로 대항하고자 한다면, 이 성의 바로위에 있는 피라밋에 가보도록하시오." +
							$"그 곳은 네크로맨서와 동시에 바다에서 떠오른 [color={RGB.LightCyan}]또다른 지식의 성전[/color]이기 때문이오." +
							" 당신이 어느 수준이 되어 그 곳에 들어간다면 진정한 이 세계의 진실을 알수 있을것이오." });

						mSpecialEvent = 1;
						ContinueText.Visibility = Visibility.Visible;
					}
					else
					{
						AppendText(new string[] { " '메나스' 속에는 드와프, 거인, 식인 늑대, 독사 같은 괴물들이 살고 있소." });

						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (moveX == 57 && moveY == 73)
				{
					AppendText(new string[] { " 나의 부모님은 독사의 독에 의해 돌아 가셨습니다. 독사 정말 위험한 존재입니다." });

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
						mRand.Next(2) == 0 ? $"거기 {mPlayerList[0].GenderName}분 어서 오십시오." : " 위스키에서 칵테일까지 마음껏 선택하십시오."
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
					AppendText(new string[] { " 이보게 자네, 내말 좀 들어 보게나. 나의 친구들은 이제 이 세상에 없다네. 그들은 너무나도 용감하고 믿음직스런 친구들이었는데..." +
						"내가 다리를 다쳐 병원에 있을 동안 그들은 모두 이 대륙의 평화를 위해 로어 특공대에 지원 했다네. 하지만 그들은 아무도 다시는 돌아오지못했어." +
						" 그런 그들에게 이렇게 살아있는 나로서는 미안할 뿐이네 그래서 술로 나날을 보내고 있지. 죄책감을 잊기위해서 말이지..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 71 && moveY == 77)
				{
					AppendText(new string[] { " 물러나십시오. 여기는 용사의 유골들을 안치해 놓은 곳입니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 62 && moveY == 75 && (mParty.Etc[49] & 1) == 0)
				{
					if (mTalkMode == 0)
					{
						AppendText(new string[] { " 당신이 한 유골 앞에 섰을때 이상한 느낌과 함께 먼곳으로 부터 어떤 소리가 들려왔다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 안녕하시오. 대담한 용사여.[/color]",
							$"[color={RGB.LightMagenta}] 당신이 나의 잠을 깨웠소 ? 나는 고대에 이곳을 지키다가 죽어간 기사 Jr. Antares 라고 하오." +
							" 저의 아버지는 Red Antares 라고 불리웠던 최강의 마법사였소. 그는 말년에 어떤 동굴로 은신을 한 후 아무에게도 모습을 나타내지 않았소." +
							" 하지만 당신의 운명은 나의 아버지를 만나야만하는 운명이라는 것을 알수있소. 반드시 나의 아버지를 만나서 당신이 알지 못했던 새로운 능력들을 배우시오." +
							" 그리고 나의 아버지를 당신의 동행으로 참가시키도록 하시오. 물론 좀 어렵겠지만 ...[/color]" });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 아참, 그리고 내가 죽기전에 여기에 뭔가를 여기에 숨겨 두었는데 당신에게 도움이 될지모르겠소. 그럼, 나는 다시 오랜 잠으로 들어가야 겠소.[/color]" });

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
					AppendText(new string[] { $" 위의 저 친구로부터 당신 얘기 많이 들었습니다. 저는 우리성에서 당신같은 용감한 사람이 있다는걸 자랑스럽게 생각합니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 12 || moveY == 54)
				{
					AppendText(new string[] { $" 만약, 당신들이 그 일을 해내기가 어렵다고 생각되시면 라스트디치 성에서 성문을 지키고있는 폴라리스란 청년을 일행에 참가시켜 주십시오." +
						" 분명 그 사람이라면 쾌히 승락할 겁니다." });

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
						AppendText(new string[] { $" 나는 이곳의 기사로서 이 세계의 모든 대륙을 탐험하고 돌아왔었습니다." +
						" 내가 마지막 대륙을 돌았을때 나는 새로운 존재를 발견했습니다. 그는 바로 예전까지도 로드 안과 대립하던 에이션트 이블이라는 존재였습니다." +
						" 지금 우리의 성에서는 철저하게 배격하도록 어릴때부터 가르침 받아온 그 에이션트 이블이었습니다." +
						" 하지만 그곳에서 본 그는 우리가 알고있는 그와는 전혀 다른 인간미를 가진 말 그대로 신과같은 존재였습니다." +
						"내가 그의 신앙아래 있는 어느 도시를 돌면서 내가 느낀것은 정말 로드 안에게서는 찾아볼수가 없는 그런 자애와 따뜻한 정이었습니다." +
						" 그리고 여태껏 내가 알고 있는 그에 대한 지식이 정말 잘못되었다는 것과 이런 사실을 다른 사람에게도 알려주고 싶다는 이유로 그의 사상을 퍼뜨리다 이렇게 잡히게 된것입니다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { " 하지만 더욱 이상한것은 로드 안 자신도 그에 대한 사실을 인정하면서도 왜 우리에게는 그를 배격하도록만 교육시키는 가를 알고 싶을뿐입니다." +
							"로드 안께서는 나를 이해한다고 하셨지만 사회 혼란을 방지하기 위해 나를 이렇게 밖에 할수 없다고 말씀하시더군요. 그리 이것은 선을 대표하는 자기로서는 이 방법 밖에는 없다고 하시더군요." +
							" 하지만 로드 안의 마음은 사실 이렇지 않다는걸 알수 있었습니다. 에이션트 이블의 말로는 사실 서로가 매우 절친한 관계임을 알수가 있었기 때문입니다." });

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
					AppendText(new string[] { " 안녕하시오. 나는 한때 이 곳의 유명한 도둑이었던 사람이오. 결국 그 때문에 나는 잡혀서 평생 여기에 있게 되었지만...",
					$" 그건 그렇고, 내가 로어 성의 보물인 [color={RGB.LightCyan}]황금의 방패[/color]를 훔쳐 달아나다. 그만 그것을 메나스라는 금광에 숨겨 놓은채 잡혀 버리고 말았소.",
					"나는 이제 그것을 가져봤자 쓸때도 없으니 차라리 당신이 그걸 가지시오. 가만있자... 어디였더라... 그래 ! 메나스의 가운데쯤에 벽으로 사방이 둘러 싸여진 곳이었는데.." +
					" 당신들이라면 지금 여기에 들어온것과 같은 방법으로 들어가서 방패를 찾을수 있을것이오. 행운을 빌겠소."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 59 && moveY == 14)
				{
					AppendText(new string[] { " 당신들에게 경고해 두겠는데 건너편 방에 있는 Joe는 오랜 수감생활 끝에 미쳐 버리고 말았소. 그의 말에 속아서 당신네 일행에 참가시키는 그런 실수는 하지마시오." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((moveX == 41 && moveY == 77) || (moveX == 41 && moveY == 79))
				{
					if ((mParty.Etc[49] & (1 << 3)) == 0)
						AppendText(new string[] { " 로드안 님의 명령에 의해서 당신들에게 한가지의 무기를 드리겠습니다. 들어가셔서 무기를 선택해 주십시오." });
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
				}
				else if ((moveX == 86 && moveY == 13) || (moveX == 85 && moveY == 11))
				{
					GoHospital();
				}
				else if ((moveX == 20 && moveY == 11) || (moveX == 24 && moveY == 12))
				{
					GoTrainingCenter();
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
						mParty.Etc[29] |= 1;

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
							AppendText(new string[] { " 드디어 나는 당신들의 능력을 믿을수 있게 되었소. 그렇다면 당신들에게 네크로맨서 응징이라는 막중한 임무를 한번 맡겨 보겠소.",
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
							" 한날, 대기의 공간이 진동하며 난데없는 푸른 번개가 대륙들 중의 하나를 강타했소. 공간은 휘어지고 시간은 진동하며 이 세계를 공포 속으로 몰고 갔소." +
							" 그 번개의 위력으로 그 불운한 대륙은 황폐화된 용암 대지로 변하고 말았고, 다른 하나의 대륙은 충돌시의 진동에 의해 바다 깊이 가라앉아 버렸소.",
							" 그런 일이 있은 한참 후에, 이상하게도 용암대지의 대륙으로부터 강한 생명의 기운이 발산되기 시작 했소." +
							" 그래서, 우리들은 그 원인을 알아보기 위해 '로어 특공대'를 조직하기로 합의를 하고 " +
							"이곳에 있는 거의 모든 용사들을 모아서 용암 대지로 변한 그 대륙으로 급히 그들을 파견하였지만 여태껏 아무 소식도 듣지못했소. 그들이 생존해 있는지 조차도 말이오.",
							$" 이런 저런 방법을 통하여 그들의 생사를 알아려던중 우연히 우리들은 '[color={RGB.LightCyan}]네크로맨서[/color]'라고 불리우는 용암 대지속의 새로운 세력의 존재를" +
							"알아내었고, 그때의 그들은 이미 막강한 세력으로 성장해가고 있는중 이었소. 그때의 번개는 그가 이 공간으로 이동하는 수단이었소. 즉 그는 이 공간의 인물이 아닌 다른 차원을 가진공간에서 왔던 것이오."
						});

						ContinueText.Visibility = Visibility.Visible;
						mParty.Etc[9]++;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $" 그는 현재 이 세계의 반을 그의 세력권 안에 넣고 있소. 여기서 당신의 궁극적인 임무는 바로 '[color={RGB.LightCyan}]네크로맨서의 야심을 봉쇄 시키는 것[/color]'이라는 걸 명심해 두시오.",
						" 네크로맨서 의 영향력은 이미 로어 대륙까지 도달해있소. 또한 그들은 이 대륙의 남서쪽에 '메나스' 라고 불리우는 지하 동굴을 얼마전에 구축했소. 그래서, 그 동굴의 존재 때문에 우리들은 그에게 위협을 당하게 되었던 것이오.",
						" 하지만, 로어 특공대가 이 대륙을 떠난후로는 그 일당들에게 대적할 용사는 이미 남아있지 않았소. 그래서 부탁하건데, 그 동굴을 중심부까지 탐사해 주시오.",
						" 나는 당신들에게 네크로맨에 대한 일을 맡기고 싶지만, 아직은 당신들의 확실한 능력을 모르는 상태이지요. 그래서 이 일은 당신들의 잠재력을 증명해 주는 좋은 기회가 될것이오.",
						" 만약 당신들이 무기가 필요하다면 무기고에서 약간의 무기를 가져가도록 허락하겠소."
						});

						ContinueText.Visibility = Visibility.Visible;

						mParty.Etc[9]++;

						mTalkMode = 0;
					}
				}
				else if ((mParty.Etc[30] & 1) == 0 && mTalkMode == 1) {
					AppendText(new string[] { $" 나는 스켈레톤이라 불리는 종족의 사람이오.",
						" 우리 종족의 사람들은 나를 제외하고는 모두 네크로맨서에게 굴복하여 그의 부하가 되었지만 나는 그렇지 않소." +
						" 나는 네크로맨서 의 영향을 피해서 이곳 로어 성으로 왔지만 나의 혐오스런 생김새 때문에 이곳 사람들에게 배척되어서 지금은 어디로도 갈 수 없는 존재가 되었소." +
						" 이제 나에게 남은 것은 네크로맨서 의 타도 밖에 없소. 그래서 당신들의 일행에 끼고 싶소."
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
				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.png"), new Vector2(64, 64), Vector2.Zero);
				mCharacterTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_sprite.png"), new Vector2(64, 64), Vector2.Zero);

				await LoadEnemyData();
				await LoadFile();
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
					else if (mPosition == PositionType.Den)
						mapIdx *= 3;

					byte tileIdx = layer[index];

					if (tileIdx == 0) {
						switch (mParty.Map) {
							case 1:
								tileIdx = 2;
								break;
							case 2:
								tileIdx = 0;
								break;
							case 3:
								tileIdx = 0;
								break;
							case 4:
								tileIdx = 41;
								break;
							case 5:
								tileIdx = 0;
								break;
							case 6:
								tileIdx = 44;
								break;
							case 7:
								tileIdx = 45;
								break;
							case 8:
								tileIdx = 47;
								break;
							case 9:
								tileIdx = 44;
								break;
							case 10:
								tileIdx = 27;
								break;
							case 11:
								tileIdx = 44;
								break;
							case 12:
								tileIdx = 0;
								break;
							case 13:
								tileIdx = 42;
								break;
							case 14:
								tileIdx = 44;
								break;
							case 15:
								tileIdx = 39;
								break;
							case 16:
								tileIdx = 41;
								break;
							case 17:
								tileIdx = 41;
								break;
							case 18:
								tileIdx = 43;
								break;
							case 19:
								tileIdx = 49;
								break;
							case 20:
								tileIdx = 44;
								break;
							case 21:
								tileIdx = 40;
								break;
							case 22:
								tileIdx = 40;
								break;
							case 23:
								tileIdx = 46;
								break;
							case 24:
								tileIdx = 47;
								break;
							case 25:
								tileIdx = 41;
								break;
							case 26:
								tileIdx = 44;
								break;
							case 27:
								tileIdx = 44;
								break;
						}
					}

					mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
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

		private void ShowMap() {
			BattlePanel.Visibility = Visibility.Collapsed;

			if (mPosition == PositionType.Den && mParty.Etc[0] == 0)
			{
				canvas.Visibility = Visibility.Collapsed;
				DarknessPanel.Visibility = Visibility.Visible;
			}
			else {
				canvas.Visibility = Visibility.Visible;
				DarknessPanel.Visibility = Visibility.Collapsed;
			}
		}

		private void HideMap() {
			BattlePanel.Visibility = Visibility.Visible;
			DarknessPanel.Visibility = Visibility.Collapsed;
			canvas.Visibility = Visibility.Collapsed;
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

			ShowMap();
		}

		private async Task LoadEnemyData() {
			//var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/FOEDATA.DAT"));
			//var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			//var reader = new BinaryReader(stream);

			var enemyFileFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/enemyData.dat"));
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
			mLoading = true;

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

			mLoading = false;
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

		private void EnterSwamp() {
			foreach (var player in mPlayerList) {
				if (player.Poison > 0)
					player.Poison++;

				if (player.Poison > 10) {
					player.Poison = 1;

					if (0 < player.Dead && player.Dead < 100)
						player.Dead++;
					else if (player.Unconscious > 0) {
						player.Unconscious++;

						if (player.Unconscious > player.Endurance * player.Level[0])
							player.Dead = 1;
					}
					else {
						player.HP--;
						if (player.HP <= 0)
							player.Unconscious++;
					}
						
				}
			}

			if (mParty.Etc[2] > 0)
				mParty.Etc[2]--;
			else {
				AppendText(new string[] { $"[color={RGB.LightRed}]일행은 독이 있는 늪에 들어갔다 !!![/color]", "" });

				foreach (var player in mPlayerList) {
					if (mRand.Next(20) + 1 >= player.Luck) {
						AppendText(new string[] { $"[color={RGB.LightMagenta}]{player.Name}(은)는 중독 되었다.[/color]" }, true);
						if (player.Poison == 0)
							player.Poison = 1;
					}
				}
			}

			UpdatePlayersStat();
			DetectGameOver();
		}

		private void EnterLava() {
			AppendText(new string[] { $"[color={RGB.LightRed}]일행은 용암지대로 들어섰다 !!![/color]", "" });

			foreach (var player in mPlayerList) {
				var damage = mRand.Next(40) + 40 - 2 * mRand.Next(player.Luck);

				AppendText(new string[] { $"[color={RGB.LightMagenta}]{player.Name}(은)는 {damage}의 피해를 입었다 ![/color]" }, true);

				if (player.HP > 0 && player.Unconscious == 0)
				{
					player.HP -= damage;
					if (player.HP <= 0)
						player.Unconscious = 1;
				}
				else if (player.HP > 0 && player.Unconscious > 0)
					player.HP -= damage;
				else if(player.Unconscious > 0 && player.Dead == 0) {
					player.Unconscious += damage;
					if (player.Unconscious > player.Endurance * player.Level[0])
						player.Dead = 1;
				}
				else if (player.Dead == 1) {
					if (player.Dead + damage > 30000)
						player.Dead = 30000;
					else
						player.Dead += damage;

				}
			}

			UpdatePlayersStat();
			DetectGameOver();
		}

		private BattleEnemyData JoinEnemy(int ENumber)
		{
			BattleEnemyData enemy = new BattleEnemyData(ENumber, mEnemyDataList[ENumber]);

			AssignEnemy(enemy);

			return enemy;
		}

		private BattleEnemyData TurnMind(Lore player) {
			var enemy = new BattleEnemyData(1, new EnemyData()
			{
				Name = player.Name,
				Strength = player.Strength,
				Mentality = player.Mentality,
				Endurance = player.Endurance,
				Resistance = player.Resistance,
				Agility = player.Agility,
				Accuracy = new int[] { player.Accuracy[0], player.Accuracy[1] },
				AC = player.AC,
				Special = player.Class == 7 ? 2 : 0,
				CastLevel = player.Level[1] / 4,
				SpecialCastLevel = 0,
				Level = player.Level[0],
			});

			AssignEnemy(enemy);

			return enemy;
		}

		private void AssignEnemy(BattleEnemyData enemy) {
			var inserted = false;
			for (var i = 0; i < mEncounterEnemyList.Count; i++)
			{
				if (mEncounterEnemyList[i].Dead)
				{
					mEncounterEnemyList[i] = enemy;
					inserted = true;
					break;
				}
			}

			if (!inserted)
			{
				if (mEncounterEnemyList.Count == 7)
					mEncounterEnemyList[mEncounterEnemyList.Count - 1] = enemy;
				else
					mEncounterEnemyList.Add(enemy);
			}
		}

		private void JoinMember(int id)
		{
			var enemy = mEnemyDataList[id];

			var player = new Lore() {
				Name = enemy.Name,
				Gender = "male",
				Class = 0,
				Strength = enemy.Strength,
				Mentality = enemy.Mentality,
				Concentration = 0,
				Endurance = enemy.Endurance,
				Resistance = enemy.Resistance / 2,
				Agility = enemy.Agility,
				Accuracy = new int[] { enemy.Accuracy[0], enemy.Accuracy[1], 0 },
				Luck = 10,
				Poison = 0,
				Unconscious = 0,
				Dead = 0,
				Level = new int[] { enemy.Level, enemy.CastLevel * 3 > 0 ? enemy.CastLevel * 3 : 1, 1 },
				ESP = 0,
				AC = enemy.AC,
				Armor = 6,

			};

			player.HP = player.Endurance * player.Level[0];
			player.SP = player.Mentality * player.Level[1];
			player.WeaPower = player.Level[0] * 2 + 10;
			player.ShiPower = 0;
			player.ArmPower = player.AC;

			switch (player.Level[0])
			{
				case 1:
					player.Experience = 0;
					break;
				case 2:
					player.Experience = 1500;
					break;
				case 3:
					player.Experience = 6000;
					break;
				case 4:
					player.Experience = 20000;
					break;
				case 5:
					player.Experience = 50000;
					break;
				case 6:
					player.Experience = 150000;
					break;
				case 7:
					player.Experience = 250000;
					break;
				case 8:
					player.Experience = 500000;
					break;
				case 9:
					player.Experience = 800000;
					break;
				case 10:
					player.Experience = 1050000;
					break;
				case 11:
					player.Experience = 1320000;
					break;
				case 12:
					player.Experience = 1620000;
					break;
				case 13:
					player.Experience = 1950000;
					break;
				case 14:
					player.Experience = 2310000;
					break;
				case 15:
					player.Experience = 2700000;
					break;
				case 16:
					player.Experience = 3120000;
					break;
				case 17:
					player.Experience = 3570000;
					break;
				case 18:
					player.Experience = 4050000;
					break;
				case 19:
					player.Experience = 4560000;
					break;
				case 20:
				case 21:
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
					player.Experience = 5100000;
					break;
			}

			if (mPlayerList.Count >= 6)
				mPlayerList[5] = player;
			else
				mPlayerList.Add(player);

			DisplayPlayerInfo();
		}

		private void EncounterEnemy() {
			if (mPosition == PositionType.Town || mParty.Map > 20)
				return;

			var enemyNumber = mRand.Next(mMaxEnemy) + 1;
			if (enemyNumber > mMaxEnemy)
				enemyNumber = mMaxEnemy;

			int range;
			int init;
			switch (mParty.Map)
			{
				case 1:
					range = 10;
					init = 0;
					break;
				case 2:
					range = 13;
					init = 7;
					break;
				case 3:
					range = 15;
					init = 15;
					break;
				case 4:
					range = 17;
					init = 23;
					break;
				case 5:
					range = 17;
					init = 32;
					break;
				case 11:
					range = 10;
					init = 5;
					break;
				case 12:
					range = 5;
					init = 16;
					break;
				case 14:
					range = 8;
					init = 4;
					break;
				case 15:
					range = 8;
					init = 17;
					break;
				case 16:
					range = 5;
					init = 27;
					break;
				case 17:
					range = 6;
					init = 22;
					break;
				case 18:
					range = 3;
					init = 29;
					break;
				case 19:
					range = 4;
					init = 37;
					break;
				case 20:
					range = 5;
					init = 40;
					break;
				default:
					range = 0;
					init = 0;
					break;
			}

			mEncounterEnemyList.Clear();
			for (var i = 0; i < enemyNumber; i++)
			{
				var enemyID = mRand.Next(range) + init;

				mEncounterEnemyList.Add(new BattleEnemyData(enemyID, mEnemyDataList[enemyID]));
			}

			DisplayEnemy();

			var avgAgility = 0;
			mEncounterEnemyList.ForEach(delegate (BattleEnemyData enemy) {
				avgAgility += enemy.Agility;
			});

			avgAgility /= mEncounterEnemyList.Count;

			AppendText(new string[] { $"[color={RGB.LightMagenta}]적이 출현했다 !!![/color]", "", $"[color={RGB.LightCyan}]적의 평균 민첩성 : {avgAgility}[/color]" });

			ShowMenu(MenuMode.BattleStart, new string[] {
				"적과 교전한다",
				"도망간다"
			});

			HideMap();			
		}

		private Color GetEnemyColor(BattleEnemyData enemy) {
			if (enemy.Dead)
				return Colors.Black;
			else if (enemy.HP == 0 || enemy.Unconscious)
				return Colors.DarkGray;
			else if (1 <= enemy.HP && enemy.HP <= 19)
				return Color.FromArgb(0xff, 0xfb, 0x63, 0x63);
			else if (20 <= enemy.HP && enemy.HP <= 49)
				return Colors.Red;
			else if (50 <= enemy.HP && enemy.HP <= 99)
				return Colors.Brown;
			else if (100 <= enemy.HP && enemy.HP <= 199)
				return Colors.Yellow;
			else if (200 <= enemy.HP && enemy.HP <= 299)
				return Colors.Green;
			else
				return Colors.LightGreen;
		}

		private void DisplayEnemy()
		{
			for (var i = 0; i < mEnemyTextList.Count; i++)
			{
				if (i < mEncounterEnemyList.Count)
				{
					mEnemyBlockList[i].Visibility = Visibility.Visible;
					mEnemyTextList[i].Text = mEncounterEnemyList[i].Name;
					mEnemyTextList[i].Foreground = new SolidColorBrush(GetEnemyColor(mEncounterEnemyList[i]));
				}
				else
					mEnemyBlockList[i].Visibility = Visibility.Collapsed;
			}
		}

		private void ShowGameOver(string[] gameOverMessage) {
			AppendText(gameOverMessage);

			ShowMenu(MenuMode.BattleLose, new string[] {
				"이전의 게임을 재개한다",
				"게임을 끝낸다"
			});
		}

		private void DetectGameOver()
		{
			var allPlayerDead = true;
			foreach (var player in mPlayerList) {
				if (player.IsAvailable) {
					allPlayerDead = false;
					break;
				}
			}

			if (allPlayerDead) {
				mParty.Etc[5] = 255;

				ShowGameOver(new string[] { "일행은 모험중에 모두 목숨을 잃었다." });
				mTriggeredDownEvent = true;
			}
		}


		private void ShowExitMenu() {
			AppendText(new string[] { $"[color={RGB.LightCyan}]여기서 나가기를 원합니까 ?[/color]" });

			ShowMenu(MenuMode.ConfirmExitMap, new string[] {
			"예, 그렇습니다.",
			"아니오, 원하지 않습니다."});
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
					AppendText(new string[] { $"       여기는 '[color={RGB.LightCyan}]로어 성[/color]'",
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
			JoinSkeleton,
			BattleStart,
			BattleCommand,
			EnemySelectMode,
			CastOneMagic,
			CastAllMagic,
			CastSpecial,
			ChooseBattleCureSpell,
			ApplyBattleCureSpell,
			CastESP,
			BattleLose,
			AskEnter,
			ChooseGoldShield
		}

		private enum CureMenuState
		{
			None,
			NotCure,
			CureEnd
		}

		private enum BattleTurn
		{
			None,
			Player,
			Enemy,
			RunAway,
			Win,
			Lose
		}

		private class BattleCommand
		{
			public Lore Player {
				get;
				set;
			}

			public int FriendID {
				get;
				set;
			}

			public int EnemyID {
				get;
				set;
			}

			public int Method {
				get;
				set;
			}

			public int Tool {
				get;
				set;
			}
		}

		private enum EnterType {
			None,
			CastleLore,
			Menace,
			LastDitch
		}
	}

}
