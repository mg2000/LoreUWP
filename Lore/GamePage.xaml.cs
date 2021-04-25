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
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
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

		private List<HealthTextBlock> mHealthTextList = new List<HealthTextBlock>();

		private List<TextBlock> mMenuList = new List<TextBlock>();
		private MenuMode mMenuMode = MenuMode.None;
		private int mMenuCount = 0;
		private int mMenuFocusID = 0;

		private SpinnerType mSpinnerType = SpinnerType.None;
		private Tuple<string, int>[] mSpinnerItems;
		private int mSpinnerID;

		private Lore mMagicPlayer = null;
		private Lore mMagicWhomPlayer = null;

		private int mOrderFromPlayerID = -1;

		private bool mWeaponShopEnd = false;
		private int mBuyWeaponID = -1;

		private Lore mCurePlayer = null;
		private CureMenuState mCureMenuState = CureMenuState.None;

		private bool mTrainingEnd = false;

		private int mTeleportationDirection = 0;

		private SpecialEventType mSpecialEvent = SpecialEventType.None;

		// 전투 이벤트
		// 1 - 로어성 경비병과의 전투
		// 2 - 미이라 장군과의 전투
		// 3 - 이블 컨센트레이션 입구 전투
		// 4 - 아키가고일과의 전투
		// 5 - 와이번과의 전투
		// 6 - 히드라와의 전투
		// 7 - 미노타우루스 전투
		// 8 - 거대 드래곤 전투
		// 9 - 이블 갓 전투
		// 10 - 크랩 갓 전투
		// 11 - 미노타우루스 전투2
		// 12 - 드래곤 3마리 전투
		// 13 - 머드맨 전투
		// 14 - 아스트랄 머드 전투
		// 15 - 스왐프 킵 나가기 전투
		// 16 - 스왐프 킵 던전 전투
		// 17 - 던전 오브 이블 나가기 전투 
		// 18 - 죽음의 기사 전투
		// 19 - 던전 오브 이블 전투
		// 20 - 던전 오브 이블 전투2
		// 21 - 가짜 네크로만서 환영과의 전투
		// 22 - 가짜 네크로만서 전투
		// 23 - 팬저 바이퍼 전투
		// 24 - 네크로만서 전투
		// 25 - 고르곤 전투
		// 26 - 임페리움 마이너 입구 전투
		// 27 - 던전 오브 이블 입구 전투
		// 28 - 네크로만서의 방 입구 전투
		private int mBattleEvent = 0;

		private volatile AnimationType mAnimationEvent = AnimationType.None;
		private int mAnimationFrame = 0;

		private bool mCureBattle = false;

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

		private Lore mReserveMember = null;
		private int mMemberX = -1;
		private int mMemberY = -1;
		private byte mMemberLeftTile = 0;

		private bool mPenetration = false;
		private int mTelescopePeriod = 0;
		private int mTelescopeXCount = 0;
		private int mTelescopeYCount = 0;

		// 동굴 질문 번호
		private int mQuestionID = 0;

		// 블랙 나이트 등장 위치
		private int xEnemyOffset = 0;
		private int yEnemyOffset = -1;

		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;

		private const int DIALOG_MAX_LINES = 13;

		private readonly List<string> mCureResult = new List<string>();
		private readonly List<string> mRemainBattleResult = new List<string>();


		public GamePage()
		{
			var rootFrame = Window.Current.Content as Frame;

			this.InitializeComponent();

			DialogText.Tag = 0;

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
			mEnterTypeMap[EnterType.Menace] = "메너스";
			mEnterTypeMap[EnterType.LastDitch] = "라스트디치";
			mEnterTypeMap[EnterType.GroundGate] = "지하 출입구";
			
			mEnterTypeMap[EnterType.AnotherLore] = "또다른 성전";
			mEnterTypeMap[EnterType.ValiantPeoples] = "배리안트 피플즈";
			mEnterTypeMap[EnterType.GaiaTerra] = "가이아 테라";
			mEnterTypeMap[EnterType.Quake] = "퀘이크";
			mEnterTypeMap[EnterType.Wivern] = "와이번";
			mEnterTypeMap[EnterType.WaterField] = "워터 필드";
			mEnterTypeMap[EnterType.Notice] = "노티스";
			mEnterTypeMap[EnterType.LockUp] = "락업";
			mEnterTypeMap[EnterType.SwampKeep] = "스웜프 킵";
			mEnterTypeMap[EnterType.EvilGod] = "이블 갓";
			mEnterTypeMap[EnterType.Muddy] = "머디";
			mEnterTypeMap[EnterType.ImperiumMinor] = "임페리움 마이너";
			mEnterTypeMap[EnterType.EvilConcentration] = "이블 컨센츄레이션";
			mEnterTypeMap[EnterType.Pyramid] = "피라미드";
			mEnterTypeMap[EnterType.EvilSeal] = "이블 씰";
			mEnterTypeMap[EnterType.SwampGate] = "스웜프 게이트";
			mEnterTypeMap[EnterType.LastShelter] = "라스트 쉘터";
			mEnterTypeMap[EnterType.DungeonOfEvil] = "던전 오브 이블";
			mEnterTypeMap[EnterType.ChamberOfNecromancer] = "챔버 오브 네크로만서";

			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName1, HealthPoison1, HealthUnconscious1, HealthDead1));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName2, HealthPoison2, HealthUnconscious2, HealthDead2));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName3, HealthPoison3, HealthUnconscious3, HealthDead3));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName4, HealthPoison4, HealthUnconscious4, HealthDead4));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName5, HealthPoison5, HealthUnconscious5, HealthDead5));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName6, HealthPoison6, HealthUnconscious6, HealthDead6));

			gamePageKeyDownEvent = (sender, args) =>
			{
				if (mLoading || mMoveEvent || mSpecialEvent > 0 || mAnimationEvent != AnimationType.None)
					return;
				else if (ContinueText.Visibility == Visibility.Visible)
				{
					return;
				}


				if (mMenuMode == MenuMode.None && mSpinnerType == SpinnerType.None && (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right ||
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

						if (!(mMapLayer[moveX + mMapWidth * moveY] == 0 || (mPosition == PositionType.Den && mMapLayer[moveX + mMapWidth * moveY] == 52)) && mRand.Next(mEncounter * 20) == 0)
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

					if (x > 3 && x < mMapWidth - 3 && y > 3 && y < mMapHeight - 3)
					{
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
								else if (x == 19 && y == 5)
									ShowEnterMenu(EnterType.AnotherLore);
							}
							else if (mParty.Map == 2) {
								if (x == 18 && y == 25)
									ShowEnterMenu(EnterType.ValiantPeoples);
								else if (x == 30 && y == 81)
									ShowEnterMenu(EnterType.GaiaTerra);
								else if (x == 81 && y == 46)
									ShowEnterMenu(EnterType.Quake);
								else if (x == 43 && y == 6)
									ShowEnterMenu(EnterType.Wivern);
							}
							else if (mParty.Map == 3) {
								if (x == 73 && y == 18)
									ShowEnterMenu(EnterType.WaterField);
								else if (x == 22 && y == 61)
									ShowEnterMenu(EnterType.Notice);
								else if (x == 95 && y == 41)
									ShowEnterMenu(EnterType.LockUp);
							}
							else if (mParty.Map == 4) {
								if (x == 47 && y == 34)
									ShowEnterMenu(EnterType.SwampKeep);
								else if (x == 47 && y == 56)
									ShowEnterMenu(EnterType.EvilGod);
								else if (x == 81 && y == 15)
									ShowEnterMenu(EnterType.Muddy);
							}
							else if (mParty.Map == 5) {
								if (x == 14 && y == 30)
									ShowEnterMenu(EnterType.ImperiumMinor);
								else if (x == 33 && y == 13)
									ShowEnterMenu(EnterType.EvilConcentration);
							}
							else if (mParty.Map == 7) {
								ShowEnterMenu(EnterType.Pyramid);
							}
							else if (mParty.Map == 8) {
								ShowEnterMenu(EnterType.EvilSeal);
							}
							else if (mParty.Map == 10) {
								ShowEnterMenu(EnterType.Wivern);
							}
							else if (mParty.Map == 13) {
								ShowEnterMenu(EnterType.SwampKeep);
							}
							else if (mParty.Map == 16) {
								ShowEnterMenu(EnterType.WaterField);
							}
							else if (mParty.Map == 21) {
								if (y < 6)
									ShowEnterMenu(EnterType.SwampGate);
								else
									ShowEnterMenu(EnterType.ImperiumMinor);
							}
							else if (mParty.Map == 22) {
								if (y < 6)
									ShowEnterMenu(EnterType.SwampKeep);
								else
									ShowEnterMenu(EnterType.LastShelter);
							}
							else if (mParty.Map == 23) {
								ShowEnterMenu(EnterType.DungeonOfEvil);
							}
							else if (mParty.Map == 25) {
								ShowEnterMenu(EnterType.ChamberOfNecromancer);
							}
						}

						if (mPosition == PositionType.Town)
						{
							if (mMapLayer[x + mMapWidth * y] == 0)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);
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
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);

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
							else if (24 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
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
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);

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
							else if (mMapLayer[x + mMapWidth * y] == 49)
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
								EnterMap();
								mTriggeredDownEvent = true;
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
						else if (mPosition == PositionType.Keep)
						{
							if (mMapLayer[x + mMapWidth * y] == 0 || mMapLayer[x + mMapWidth * y] == 52)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);

								mTriggeredDownEvent = true;
							}
							else if ((1 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 39) || mMapLayer[x + mMapWidth * y] == 51)
							{

							}
							else if (mMapLayer[x + mMapWidth * y] == 53)
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
							else if (mMapLayer[x + mMapWidth * y] == 49)
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
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (40 <= mMapLayer[x + mMapWidth * y] && mMapLayer[x + mMapWidth * y] <= 47)
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
					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까?[/color]" });

					ShowMenu(MenuMode.HealType, new string[]
					{
							"상처를 치료",
							"독을 제거",
							"의식의 회복",
							"부활"
					});
				}

				async Task ExitCastleLore()
				{
					mParty.XAxis = 19;
					mParty.YAxis = 11;
					mParty.Map = 1;

					await RefreshGame();
				}

				async Task EndBattle()
				{
					void DefeatAstralMud() {
						mEncounterEnemyList.Clear();
						mBattleEvent = 0;

						ShowMap();

						Talk(" 당신은 이 동굴에 보관되어 있는 봉인을 발견했다. 그리고는 봉쇄 되었던 봉인을 풀어버렸다.");
						mSpecialEvent = SpecialEventType.DefeatAstralMud;
					}
					
					async Task CheckPassSwampKeepExitEvent() {
						if (mEncounterEnemyList[0].Dead)
							mParty.Etc[41] |= 1 << 2;

						if (mEncounterEnemyList[1].Dead)
							mParty.Etc[41] |= 1 << 3;

						if (mEncounterEnemyList[0].Dead && mEncounterEnemyList[1].Dead)
							mParty.Etc[41] |= 1;

						mParty.Map = 4;
						mParty.XAxis = 47;
						mParty.YAxis = 35;

						await RefreshGame();
					}

					void SwampKeepBattleEvent()
					{
						if (mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] == 0)
							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
						else
							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 46;
					}

					async Task DefeatImperiumMinorKeeper() {
						if (mEncounterEnemyList[6].Dead)
							mParty.Etc[42] |= 1 << 2;

						mParty.Map = 5;
						mParty.XAxis = 14;
						mParty.YAxis = 31;

						await RefreshGame();
					}

					void WinNecromancer() {
						Talk(new string[] {
							$" [color={RGB.LightMagenta}]욱!!! 역시 너희들의 능력으로 여기까지 뚫고 들어왔다는게 믿어지는구나. 대단한 힘이다.[/color]",
							$" [color={RGB.LightMagenta}]내가 졌다는걸 인정하마. 하지만 나는 완전히 너에게 진것은 아니야. 나에게는 탈출할 수단이 있기 때문이지. 안심해라." +
							" 그렇지만 다시는 나와 만날 인연은 없으니까. 블랙홀이 생기기 시작하는구나. 다음 공간에서 또다시 힘을 길러야 겠군." +
							" 내가 이 블랙홀로 들어간다면 다시 이 공간으로 올 확률이 거의 제로이지. 흠, 멋진 나의 도전자여 안녕. 나는 이런 공간의 패러독스를 운명적으로 반복하는 생명체로 태어난 내가 참으로 비참하지." +
							" 무한히 많은 3 차원의 공간중에서 내가 여기로 온것도 이 공간의 생명이 끝날때까지도 한번 있을까 말까한 희귀한 일이었다고 기억해다오." +
							" 이제 블랙홀이 완전히 생겼군. 자! 나의 멋진 도전자 친구여 영원히 안녕 !![/color]",
						});

						mSpecialEvent = SpecialEventType.Ending;
					}

					async Task CheckImperiumMinorEntraceBattleResult() {
						var allDead = true;
						foreach (var enemy in mEncounterEnemyList) {
							if (enemy.ENumber == 64 && enemy.Dead)
								mParty.Etc[41] |= 1 << 4;
							else if (enemy.ENumber == 63 && enemy.Dead)
								mParty.Etc[41] |= 1 << 5;

							if (!allDead)
								allDead = false;
						}

						if (allDead)
							mParty.Etc[41] |= 1 << 1;

						mEncounterEnemyList.Clear();
						mBattleEvent = 0;

						mParty.Map = 22;
						mParty.XAxis = 24;
						mParty.YAxis = 5;

						if ((mParty.Etc[41] & (1 << 6)) == 0)
						{
							JoinEnemy(67);

							DisplayEnemy();

							Talk(new string[] { " 역시 당신들은 나의 예상대로 마지막 대륙까지 무난하게 왔군요. 이번에 가게될 라바 대륙은 이 세계에 있는 모든 대륙중에서 가장 작은 대륙이오." +
							" 적의 요새도 또한 2개 밖에 없는 곳이오. 하지만 이번에 도착할 임페리움 마이너나 마지막으로 거칠 이블 컨센츄레이션 은 말 그대로 악의 집결지인 것이오." +
							" 거기에는 최강의 괴물들과 네크로만서의 심복들로 가득차있는 곳이지만 임페리움 마이너의 지하에는 마지막으로 살아남은 사람들의 도시가 있소." +
							" 원래 거기는 에인션트 이블이 전에 세운 악의 동굴이었지만 네크로만서의 침략으로 지상의 도시가 함락되자 그 곳의 사람들은 모두 거기로 피난 했던 것이고" +
							" 거기는 에인션트 이블의 영적인 힘으로 보호되고 있어서 적들이 침략을 하지 못하는 이유가 되지요. 그러므로 모든 도움과 물자는 거기서 받도록 하시오.",
							" 그러면, 나는 당신이 네크로만서와 상대하게 될 때 다시 에인션트 이블과 같이 나타나겠소."});

							mSpecialEvent = SpecialEventType.EnterImperiumMinor;
						}
						else
						{
							await RefreshGame();
						}
					}

					async Task CheckDungeonOfEvilBattleResult() {
						if (mEncounterEnemyList[2].Dead)
							mParty.Etc[43] |= 1 << 1;

						mParty.Map = 25;
						mParty.XAxis = 24;
						mParty.YAxis = 44;

						await RefreshGame();
					}

					mBattleCommandQueue.Clear();
					mBatteEnemyQueue.Clear();

					if (mBattleTurn == BattleTurn.Win) {
						
						var endMessage = "";

						if (mParty.Etc[5] == 2)
							endMessage = "";
						else
						{
							#if DEBUG
							var goldPlus = 10000;
							#else	
							var goldPlus = 0;
							foreach (var enemy in mEncounterEnemyList)
							{
								var enemyInfo = mEnemyDataList[enemy.ENumber];
								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
								var plus = enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= point;
								goldPlus += plus;
							}
							#endif

							mParty.Gold += goldPlus;

							endMessage = $"일행은 {goldPlus}개의 금을 얻었다.";

							AppendText(new string[] { endMessage, "" });
						}

						if (mBattleEvent == 1)
						{
							AppendText(new string[] { $"[color={RGB.White}]당신들은 수감소 병사들을 물리쳤다.[/color]" }, true);
							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[49] |= 1 << 2;
							mMapLayer[50 + mMapWidth * 11] = 44;
							mMapLayer[51 + mMapWidth * 11] = 44;
							mMapLayer[49 + mMapWidth * 10] = 44;
							mMapLayer[52 + mMapWidth * 10] = 44;
						}
						else if (mBattleEvent == 2)
						{
							if (mParty.Etc[5] != 255)
							{
								if (mParty.Etc[5] == 0)
								{
									AppendText(new string[] {
										$"[color={RGB.White}]당신들은 미이라 장군을 물리쳤다.[/color]",
										$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]"
									}, true);

									ContinueText.Visibility = Visibility.Visible;

									mParty.Etc[12]++;
								}
							}
						}
						else if (mBattleEvent == 3)
						{
							mParty.Etc[43] |= 1;

							mParty.Map = 23;
							mParty.XAxis = 24;
							mParty.YAxis = 44;

							await RefreshGame();
						}
						else if (mBattleEvent == 4)
						{
							AppendText(new string[] { $"[color={RGB.White}]당신은 아키가고일을 물리쳤다.[/color]" }, true);

							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[13]++;
						}
						else if (mBattleEvent == 5)
						{
							mParty.Etc[36] = 3;
						}
						else if (mBattleEvent == 6)
						{
							AppendText(new string[] {
								$"[color={RGB.White}]당신들은 히드라를 물리쳤다.[/color]",
								$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]",
								$"[color={RGB.White}]다시 워터 필드의 군주에게로 돌아가라.[/color]",
							}, true);

							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[14] = 2;
							mSpecialEvent = SpecialEventType.AfterBattleHydra;
						}
						else if (mBattleEvent == 7)
							mParty.Etc[38] |= 1 << 2;
						else if (mBattleEvent == 8)
						{
							AppendText(new string[] {
								$"[color={RGB.White}]당신들은 거대 드래곤을 물리쳤다.[/color]",
								$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]",
								$"[color={RGB.White}]다시 워터 필드의 군주에게로 돌아가라.[/color]",
							}, true);

							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[14] = 4;
						}
						else if (mBattleEvent == 9)
						{
							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 49;
						}
						else if (mBattleEvent == 10)
						{
							AppendText(new string[] { " 당신은 이 동굴에 보관되어 있는 봉인을 발견했다. 그러고는 봉쇄되었던 봉인을 풀어버렸다." }, true);

							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[39] |= 1;
						}
						else if (mBattleEvent == 11)
							mParty.Etc[40] |= 1 << 3;
						else if (mBattleEvent == 12)
						{
							mParty.Etc[40] |= 1 << 1;
							await CheckMuddyFinalBattle();
							return;
						}
						else if (mBattleEvent == 13)
						{
							mParty.Etc[40] |= 1 << 2;
							await CheckMuddyFinalBattle();
							return;
						}
						else if (mBattleEvent == 14)
						{
							DefeatAstralMud();
						}
						else if (mBattleEvent == 15)
							await CheckPassSwampKeepExitEvent();
						else if (mBattleEvent == 16)
							SwampKeepBattleEvent();
						else if (mBattleEvent == 17)
							await DefeatImperiumMinorKeeper();
						else if (mBattleEvent == 18)
							mParty.Etc[42] |= 1 << 1;
						else if (mBattleEvent == 19)
							mParty.Etc[42] |= 1;
						else if (mBattleEvent == 20)
							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
						else if (mBattleEvent == 21)
						{
							mBattleTurn = BattleTurn.None;

							mEncounterEnemyList.Clear();

							var enemy = JoinEnemy(60);
							enemy.Name = "네크로만서";
							enemy.ENumber = 0;

							DisplayEnemy();

							AppendText(new string[] {
								$"[color={RGB.LightMagenta}] 환상에서 벗어나다니 대단한 의지력이군.[/color]",
								$"[color={RGB.LightMagenta}] 하지만 진짜 적은 바로 나다. 받아라 !![/color]"
							}, true);

							ContinueText.Visibility = Visibility.Visible;

							mBattleEvent = 0;
							mSpecialEvent = SpecialEventType.BattleFackNecromancer;
							return;
						}
						else if (mBattleEvent == 22)
						{
							AppendText(new string[] { $"[color={RGB.LightMagenta}] 욱! 너의 힘은 대단하구나. 나는 너에게 졌다고 인정하겠다. 흐흐, 그러나 사실 나는 너희 찾던 네크로만서님이 아니다. 만약 그분이라 이렇게 쉽게 당하지는 않았을 테니까." +
							" 내 생명이 얼마 안남았구나. 네크로만서님 만세 !![/color]" }, true);

							ContinueText.Visibility = Visibility.Visible;

							mSpecialEvent = SpecialEventType.AfterBattleFakeNecromancer;
						}
						else if (mBattleEvent == 23)
						{
							for (var x = 23; x < 27; x++)
								mMapLayer[x + mMapWidth * mParty.YAxis] = 41;

							AppendText(new string[] { " 당신이 적을 물리치자 조금후에 이상하리만큼 편안한 기운이 일행을 감쌌다." }, true);

							ContinueText.Visibility = Visibility.Visible;

							mSpecialEvent = SpecialEventType.AfterBattlePanzerViper;
						}
						else if (mBattleEvent == 24)
							WinNecromancer();
						else if (mBattleEvent == 25)
						{
							AppendText(new string[] { $"[color={RGB.White}]당신들은 고르곤을 물리쳤다.[/color]" }, true);

							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[37] |= 1 << 4;
						}
						else if (mBattleEvent == 26)
						{
							await CheckImperiumMinorEntraceBattleResult();

							return;
						}
						else if (mBattleEvent == 27)
							await CheckDungeonOfEvilBattleResult();
						else if (mBattleEvent == 28) {
							mParty.Map = 26;
							mParty.XAxis = 24;
							mParty.YAxis = 14;

							await RefreshGame();

							mFace = 5;

							InvokeAnimation(AnimationType.EnterChamberOfNecromancer);
						}

						mEncounterEnemyList.Clear();
						mBattleEvent = 0;

						ShowMap();

					}
					else if (mBattleTurn == BattleTurn.RunAway) {
						AppendText(new string[] { "" });

						if (mBattleEvent == 5)
						{
							var aliveWivernCount = 0;
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead)
									aliveWivernCount++;
							}

							mParty.Etc[36] = 3 - aliveWivernCount;
						}
						else if (mBattleEvent == 6)
						{
							mParty.XAxis++;
						}
						else if (mBattleEvent == 8)
						{
							mParty.XAxis = 24;
							mParty.YAxis = 93;
						}
						else if (mBattleEvent == 10)
							mParty.YAxis++;
						else if (mBattleEvent == 12)
							mParty.YAxis++;
						else if (mBattleEvent == 13)
							mParty.YAxis++;
						else if (mBattleEvent == 14)
						{
							if (mEncounterEnemyList[6].Dead)
								DefeatAstralMud();
							else
								mParty.YAxis++;
						}
						else if (mBattleEvent == 15)
							await CheckPassSwampKeepExitEvent();
						else if (mBattleEvent == 16)
							SwampKeepBattleEvent();
						else if (mBattleEvent == 17)
							await DefeatImperiumMinorKeeper();
						else if (mBattleEvent == 20)
							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
						else if (mBattleEvent == 21)
						{
							Talk(" 하지만 당신은 환상에서 벗어나지 못했다.");
							mSpecialEvent = SpecialEventType.FailRunawayBattleFakeNecromancer;

							return;
						}
						else if (mBattleEvent == 22)
							mParty.YAxis++;
						else if (mBattleEvent == 23)
							mParty.YAxis++;
						else if (mBattleEvent == 24)
						{
							if (!mEncounterEnemyList[6].Dead)
							{
								Talk($" [color={RGB.LightMagenta}]하지만 나에게 도전한 이상 도주는 허용할 수 없다는 점이 안타깝군.[/color]");

								mSpecialEvent = SpecialEventType.FailRunawayBattleNecromancer;
								return;
							}
							else
								WinNecromancer();
						}
						else if (mBattleEvent == 25)
						{
							mParty.YAxis++;
						}
						else if (mBattleEvent == 26)
						{
							await CheckImperiumMinorEntraceBattleResult();
							return;
						}
						else if (mBattleEvent == 27)
							await CheckDungeonOfEvilBattleResult();
						else if (mBattleEvent == 28) {
							mParty.XAxis = 24;
							mParty.YAxis = 44;
						}

						mEncounterEnemyList.Clear();
						mBattleEvent = 0;
						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.Lose) {
						ShowGameOver(new string[] {
							$"[color={RGB.LightMagenta}]일행은 모두 전투에서 패했다 !![/color]",
							$"[color={RGB.LightGreen}]    어떻게 하시겠습니까?[/color]"
						});
					}
					
					mBattleTurn = BattleTurn.None;
				}

				void AddBattleCommand(bool skip = false)
				{
					mMenuMode = MenuMode.None;

					if (!skip)
					{
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
					}

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

				void ShowCureResult()
				{
					var resultPart = new List<string>();
					if (mCureResult.Count > DIALOG_MAX_LINES)
					{
						for (var i = 0; i < DIALOG_MAX_LINES; i++)
						{
							resultPart.Add(mCureResult[0]);
							mCureResult.RemoveAt(0);
						}
					}
					else
					{
						resultPart.AddRange(mCureResult);
						mCureResult.Clear();
					}

					Talk(resultPart.ToArray());
				}

				if (mMoveEvent || mLoading || (mAnimationEvent != AnimationType.None && ContinueText.Visibility != Visibility.Visible && mMenuMode == MenuMode.None) || mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (mSpecialEvent == SpecialEventType.RefuseJoinSkeleton)
				{
					mSpecialEvent = SpecialEventType.None;

					mAnimationEvent = AnimationType.None;
					mAnimationFrame = 0;

					await ExitCastleLore();
				}
				else if (mSpecialEvent == SpecialEventType.StartGame)
				{
					TalkMode(mTalkX, mTalkY, args.VirtualKey);
				}
				else if (ContinueText.Visibility == Visibility.Visible)
				{
					async Task InvokeSpecialEventLaterPart()
					{
						if (mSpecialEvent == SpecialEventType.MeetLoreSolider)
						{
							mParty.Etc[49] |= 1 << 4;
							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetSkeleton)
						{
							InvokeAnimation(AnimationType.MeetSkeleton);
							
							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetDraconian)
						{
							if ((mParty.Etc[15] & (1 << 1)) > 0)
							{
								AppendText(new string[] { "그러나, 아무도 살고 있지 않았다." });

								mSpecialEvent = SpecialEventType.None;
							}
							else if (mParty.Etc[4] == 0)
							{
								Talk(new string[] {
									" 나는 드래코니안이라고 하오. 이런 곳까지 사람이 찾아오리라고는 생각하지 못했는데 참 의외로군요. 나는 숨어 지내는 오랜 세월 동안에 여러 가지 학문을 연구하고 있었소." +
									" 특히 천문학에 대해서는 특별히 전념하여 다루었소. 내가 그동안 알아낸 지식들은 다음과 같은 것이오.",
									" 지금 우리가 살고 있는 지구는 태양계에서 세 번째로 위치한 행성인데 이상하게도 위성인 달이 지구보다도 더 오래되었다는 사실이 연구되었소." +
									" 또한 이 지구가 세차운동에 의해 2 등성이었던 북극성이 다른 별로 바뀐다는 걸 알았소",
									" 물리학에서 알아낸 것은, 중수 반응에서 매질의 차이로 빛이 속도가 느려진 틈을 이용해 빛 보다 빠른 입자가 생겨났소. 하지만 절대적인 빛의 속도에는 미치지 못했지만 말이오." +
									" 이것을 잘만 이용하면 미래로의 타임머신이 가능하다는 걸 입증할 수가 있소.",
									" 사실 이런 것은 당신에게는 별 의미가 없는 것일지도 모르오. 하지만 마지막으로 알아낸 것은 바로 당신이 응징하고자 하는 네크로만서의 출처에 관한 것이오." +
									" 당신은 시그너스 X1과 같은 블랙홀에 대해서 알고 있을 것이오. 이런 물리학적인 패러독스에 의해 그는 생겨났던 것이오."
								});

								mSpecialEvent = SpecialEventType.MeetDraconian2;
							}
							else
							{
								AppendText(new string[] { " 나의 운명을 생각해 보니 나 역시 당신들을 필연적으로 만나 네크로만서를 물리쳐야 한다는 걸 깨달았소." +
								" 당신들 일행의 제일 뒤에서 도와주고 싶소. 어떻소." });

								ShowMenu(MenuMode.JoinDraconian, new string[] {
									"저도 바라던 차입니다",
									"별로 좋지는 않군요"
								});

								mSpecialEvent = SpecialEventType.None;
							}
						}
						else if (mSpecialEvent == SpecialEventType.MeetDraconian2)
						{
							Talk(new string[] {
								" 지금 우리가 있는 3차원 위에 또 다른 차원이 있다고 생각하오? 생각은 하더라도 눈으로 확인은 못해 봤을 것이오. 우리는 2차원을 인식할 수가 있소." +
								" 하지만 실제로 2차원에 살고 있는 생물은 자신의 차원은 인식해도 우리가 살고 있는 3차원은 인식할 수가 없는 것이오." +
								" 그의 차원에는 우리의 차원이 존재하지 않기 때문에 개념을 잡을 수가 없는 것이오. 마찬가지로 우리 또한 지금의 차원보다 한 차원 높은 4 차원을 개념을 잡기가 어려운 것이오.",
								" 어떤 이는 4차원은 시간의 축이 보태어진다고 하는 이론을 세우더군요. 하지만 나의 이론은 시간은 모든 차원의 기준이며 4차원에 국한되지는 않는다는 생각이오." +
								" 3차원에서는 동시간대에 무한한 2차원을 포함하듯이 4차원에서는 동시간대에 무한한 3차원을 포함하고 있다는 이론이 성립되오." +
								" 말이 조금은 빗나갔지만 이 이론으로 네크로만서의 출처를 해명해 보겠소",
								" 방금 말했듯이 3차원은 이 공간만이 존재하는 것은 아니오. 동시대를 살아가는 다른 공간도 인정해야 한다는 말이오. 그 공간들을 이어주는 것이 바로 블랙홀이란 것이지요." +
								" 그는 원래 그가 있던 공간에서 블랙홀을 통해서 다른 공간으로 가려고 시도를 했고 웜홀을 통해 시공간을 가로질러 오래전 우리의 공간에 화이트홀이 생기는 틈을 이용하여 내려왔던 것이오."
							});

							mSpecialEvent = SpecialEventType.MeetDraconian3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetDraconian3)
						{
							Talk(new string[] {
								" 하지만 3차원에 사는 나로서는 그가 전에 있던 공간에서 왜 이쪽으로 왔는지 알 수가 없었지요. 그래서 그와 같이 이 공간으로 들어왔던 심복들을 통해 그 사정을 알게 되었소.",
								" 네크로만서는 저쪽의 공간에서도 지금과 마찬가지로 차원을 통해 그 공간에 도달했소. 역시 거기서도 악을 뿌리며 거기의 생명체들을 위협했소." +
								" 하지만 어떤 선택된 6인의 용사들에 의해 쫓겨겨나서 여기로 온 것이오. 지금의 당신들과 비슷하다고 생각되지 않소? 그렇소." +
								" 다른 공간의 당신들에게 쫓겨난 후 다시 여기서 당신들을 또 만나게 된 것이오. 네크로만서와 당신들의 운명은 언제까지나 쫓고 쫓기며 대립하여야하 하는 운명으로 탄생되었던 것이오." +
								" 이런 이유에서 당신은 그를 반드시 무찌를 수가 있다는 근거가 되는 것이오.",
								" 당신들의 건투를 빌어주겠소."
							});

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAncientEvil)
						{
							mParty.XAxis = 47;
							mParty.YAxis = 56;

							Talk(" 여기는 이블 갓이라는 동굴이오. 여기의 보스는 크랩 갓인데 적 자체는 별거 아니지만 떼를 지어서 다니기 때문에 약간의 애를 먹을 것이오." +
							" 7갈래의 길 중에서 한 곳에 봉인이 숨겨져 있을 것이오.");

							mSpecialEvent = SpecialEventType.MeetAncientEvil2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAncientEvil2)
						{
							mParty.XAxis = 81;
							mParty.YAxis = 15;

							Talk(" 여기는 머디라는 동굴이오. 여기의 보스는 아스트랄 머드라는 자인데 그리 실력이 있다고는 볼 수가 없소." +
							" 다만 동굴 자체가 어려운 미로라는 것과 시야가 좁아지는 불편 등은 감수해야만 할 것이오." +
							" 그리고 마지막에 3마리의 드래곤과 7마리의 진흙 인간을 거쳐야만 그가 나타나기 때문에 약간 까다로울 것이오.");

							mSpecialEvent = SpecialEventType.MeetAncientEvil3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAncientEvil3)
						{
							mParty.XAxis = 15;
							mParty.YAxis = 14;

							Talk(" 여기는 이 대륙의 외진 곳이오. 여기서는 어떤 만남이 기다리고 있을 것이오.");

							mParty.Etc[15] |= 1;
							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.EnterSwampGate)
						{
							Talk(new string[] {
								$" 나는 로어 성의 성주 [color={RGB.LightCyan}]로드 안[/color]이오.",
								" 역시 내가 예상한 대로 당신들은 훌륭한 용사로 성장해 나가고 있소. 여태까지는 모험이 순조롭게 진행되었지만 이제부터는 완전한 적들의 소굴이오." +
								" 그래서 나도 직접적인 도움은 못 주더라도 여러 가지 조언을 해주겠소."
							});

							mSpecialEvent = SpecialEventType.EnterSwampGate2;
						}
						else if (mSpecialEvent == SpecialEventType.EnterSwampGate2)
						{
							Talk(" 당신들은 식량을 많이 가지고 있소? 이 식량은 당신들을 회복시키기 위해 필요한 것이니 절대 바닥나게 해서는 안 되오." +
							" 왜냐하면 이 이후에 전개되는 모험에서는 식량을 파는 곳이 거의 없다고 생각해도 될 만큼 식량이 귀중하므로 낭패를 보는 일이 없도록 하시오.");

							mSpecialEvent = SpecialEventType.EnterSwampGate3;
						}
						else if (mSpecialEvent == SpecialEventType.EnterSwampGate3)
						{
							Talk(new string[] {
								" 스왐프의 대륙에서의 할 일을 요약하면 이렇소.",
								" 스왐프 게이트와 통하는 스왐프 킵에는 많은 강한 괴물들이 버티고 있소." +
								" 하지만 이전에 그 대륙에 있는 2개의 동굴 요새를 점령한 뒤에야 스왐프 킵의 중앙에 있는 라바 게이트를 작동시킬 수 있을 것이오." +
								" 그곳의 괴물들은 매우 힘든 상대일 것이오. 하지만 당신들의 능력이라면 충분히 가능할 것이오. 나는 당신들이 라바 게이트를 통과하려 할 때 다시 조언을 해주겠소.",
								" 그때까지 건투를 비는 바이오."
							});

							mParty.Etc[34] |= 1 << 5;
							mSpecialEvent = SpecialEventType.EnterSwampGate4;
						}
						else if (mSpecialEvent == SpecialEventType.EnterSwampGate4)
						{
							await EnterSwampGate();

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.FindOedipusSpear)
						{
							Talk(new string[] {
								"그 창의 손잡이에 쓰인 문구를 따르면..",
								"",
								$"        [color={RGB.LightCyan}]이것은 오이디푸스의 창[/color]",
								$"   [color={RGB.LightCyan}]이것으로 전에 스핑크스를 무찌르다[/color]"
							});

							mSpecialEvent = SpecialEventType.FindOedipusSpear2;
						}
						else if (mSpecialEvent == SpecialEventType.FindOedipusSpear2)
						{
							AppendText($"[color={RGB.LightCyan}]누가 오이디푸스의 창을 다루겠습니까?[/color]");

							ShowCharacterMenu(MenuMode.ChooseOedipusSpear);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MajorMummyRoom)
						{
							mEncounterEnemyList.Clear();
							for (var i = 0; i < 2; i++)
							{
								var enemy = JoinEnemy(34);
								enemy.Name = $"스핑크스";
								enemy.Level = 4;
								enemy.Special = 0;
								enemy.ENumber = 19;
							}

							var majorMummy = JoinEnemy(25);
							majorMummy.Name = "미이라 장군";
							majorMummy.AC = 1;

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 2;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.MeetRigel)
						{
							AppendText(new string[] {
							$" 나는 배리안트 피플즈의 용사였던 [color={RGB.LightCyan}]리겔[/color]이오." +
							" 내가 동굴 속에서 적들을 막아내는 동안 지각변동으로 인해 이런 절벽이 군데군데 생겼소." +
							" 나는 이제 너무 지치고 많은 상처를 입어서 혼자 힘으로는 이곳을 빠져나갈 수가 없소.",
							" 나를 도와주시오."
							});

							ShowMenu(MenuMode.JoinRigel, new string[] {
								"좋소, 같이 모험을 합시다",
								"식량과 치료는 해결해 주겠소",
								"당신을 도와줄 시간이 없소"
							});
						}
						else if (mSpecialEvent == SpecialEventType.EnterEvilConcentration)
						{
							mSpecialEvent = SpecialEventType.None;

							mBattleEvent = 3;
							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.FindGoldenShield)
						{
							AppendText($"[color={RGB.LightCyan}]누가 이 황금의 방패를 장착하겠습니까?[/color]", true);

							ShowCharacterMenu(MenuMode.ChooseGoldShield2);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.FindGoldenArmor)
						{
							AppendText($"[color={RGB.LightCyan}]누가 이 황금의 방패를 장착하겠습니까?[/color]", true);

							ShowCharacterMenu(MenuMode.ChooseGoldArmor);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleArchiGagoyle)
						{
							mEncounterEnemyList.Clear();
							for (var i = 0; i < 2; i++)
							{
								var enemy = JoinEnemy(35);
								enemy.Name = $"좀비";
							}

							var majorMummy = JoinEnemy(41);
							majorMummy.Name = "아키가고일";

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 4;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleWivern)
						{
							mEncounterEnemyList.Clear();
							for (var i = 0; i < 3 - mParty.Etc[36]; i++)
								JoinEnemy(42);

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 5;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.MeetRedAntares)
						{
							Talk(new string[] {
								$"나는 고대의 강력한 마법사였던 [color={RGB.LightCyan}]레드 안타레스[/color]의 영이오.",
								" 한때, 나는 이 세계의 모든 지역을 통괄하는 마법을 가지고 이 세계를 통치했지만 내가 죽고 난 뒤로는 여러 지역으로 나의 마법이 분산되어 제대로 힘을 발휘하지 못하는 것 같소." +
								" 당신들이 네크로만서에 대항하려 한다는 걸 알고 있소. 나는 그가 이 동굴을 요새화 시킬 때 이미 그의 마법 능력을 지켜보았기 때문에 그의 능력을 알 수 있었소." +
								" 그러나 그의 능력에 비교해 볼 때 당신들의 마법 능력은 상당히 저조하오. 그래서 당신들을 위해 나의 마법 중 \"간접 공격\"이란 기법을 전해 주겠소.",
								" 만약 당신들의 마법 능력이 도달한다면 다음의 마법을 사용할 수 있을 것이오."
							});

							mSpecialEvent = SpecialEventType.MeetRedAntares2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetRedAntares2)
						{
							Talk(new string[] {
								$"[color={RGB.White}]1.     독      - 적을 중독 시킴[/color]",
								$"[color={RGB.White}]2. 기술 무력화 - 적의 특수 공격 능력 제거[/color]",
								$"[color={RGB.White}]3. 방어 무력화 - 적의 방어력 감소[/color]",
								$"[color={RGB.White}]4. 능력   저하 - 적의 모든 능력 감소[/color]",
								$"[color={RGB.White}]5. 마법   불능 - 적의 마법 능력 제거[/color]",
								$"[color={RGB.White}]6. 탈   초인화 - 적의 초자연력 제거[/color]"
							});

							mSpecialEvent = SpecialEventType.MeetRedAntares3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetRedAntares3)
						{
							Talk(new string[] {
								" 이 여섯 가지의 마법은 사용하기 까다롭고 직접적인 공격은 아니지만 큰 도움을 줄 것이오.",
								"하지만, 사실 이 마법들을 모두 동원하더라도 네크로만서를 만나기조차 어려울 것이오. 그래서 당신들은 초자연력 또한 익혀야만 그에게 대항할 수 있을 것이오."
							});

							mParty.Etc[37] |= 1;
							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleHydra)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 3; i++)
							{
								var enemy = JoinEnemy(48);
								enemy.Name = $"히드라의 머리 {i + 1}";
								enemy.Level = 8;
								enemy.ENumber = 33;
							}

							mEncounterEnemyList[2].Level = 10;
							mEncounterEnemyList[2].ENumber = 38;

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 6;

							HideMap();
							DisplayEnemy();

							StartBattle();
						}
						else if (mSpecialEvent == SpecialEventType.AfterBattleHydra)
						{
							mParty.XAxis = 55;
							mParty.YAxis = 92;

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleMinotaur)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							JoinEnemy(52);

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 7;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.MeetSpica)
						{
							Talk(new string[] {
								" 나는 이곳 락업이 적들에게 점령되기 전부터 여기서 수도하고 있는 스피카란 사람입니다.",
								" 오랜 수도 끝에 나는 초자연력의 존재와 사용법을 알게 되었습니다. 대충 요약하면 이렇습니다.",
								"",
								$"[color={RGB.White}]투  시 : 변화의 여지가 있는 지역을 탐지[/color]",
								$"[color={RGB.White}]예  언 : 다음의 할 일을 알아냄[/color]",
								$"[color={RGB.White}]독  심 : 남의 마음을 자기 쪽으로 끌어들임[/color]",
								$"[color={RGB.White}]천리안 : 능력에 따라 먼곳의 광경을 봄[/color]",
								$"[color={RGB.White}]염  력 : 주위 환경 조절에 의한 공격[/color]",
								"",
								" 이것으로 네크로만서에게 도전하십시오. 또한, 그도 초자연력의 존재를 알고 있고 사용할 줄 안다는 걸 염두에 두고 사용하십시오."
							});

							mParty.Etc[38] |= 1;
							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleHugeDragon)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							var enemy = JoinEnemy(53);
							enemy.Name = "거대 드래곤";
							var enemyTail = JoinEnemy(38);
							enemyTail.Name = $"드래곤의 꼬리";
							for (var i = 0; i < 5; i++)
							{
								JoinEnemy(mRand.Next(3) + 29);
							}

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 8;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleCrabGod)
						{
							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 10;

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleMinotaur2)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							JoinEnemy(52);

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 11;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleThreeDragon)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 3; i++)
								JoinEnemy(53);

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 12;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleAstralMud)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 6; i++)
								JoinEnemy(30);
							JoinEnemy(56);

							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 14;

							HideMap();
							DisplayEnemy();

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.ExitImperiumMinor)
						{
							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 17;

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattleDeathKnight)
						{
							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 18;

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.MeetFaceNecromancer)
						{
							Talk(new string[] {
								$" [color={RGB.LightMagenta}]너희들은 곧 환상에 빠져들게 될 것이다.[/color]",
								$" [color={RGB.LightMagenta}]나는 벌써 너희들의 약점을 파악했지. 너희 일행들은 항상 자신을 너무 신뢰하고 믿고 있더군. 그러나 그 착각은 곧 깨어질 것이다.[/color]",
								$" [color={RGB.White}]어둠의 신이여, 당신의 힘으로 이들을 환상에 빠져들게 하소서. 인 쿠아스 젠 ~~[/color]"
							});

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 6; i++)
							{
								BattleEnemyData enemy;
								if (mPlayerList.Count < i)
									enemy = JoinEnemy(59);
								else
									enemy = TurnMind(mPlayerList[i]);
								enemy.ENumber = 1;
							}

							HideMap();
							DisplayEnemy();

							mSpecialEvent = SpecialEventType.BattleDual;
						}
						else if (mSpecialEvent == SpecialEventType.BattleDual)
						{
							mBattleEvent = 21;

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.FailRunawayBattleFakeNecromancer)
						{
							mBattleEvent = 21;

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleFackNecromancer)
						{
							mBattleEvent = 22;

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.AfterBattleFakeNecromancer)
						{
							Talk(" 그는 숨이 끊어졌고 주위의 기둥도 그와 함께 사라져 버렸다.");

							mMapLayer[28 + mMapWidth * 26] = 53;

							for (var y = 24; y < 27; y++)
							{
								for (var x = 23; x < 27; x++)
									mMapLayer[x + mMapWidth * y] = 46;
							}

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattlePanzerViper)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 4; i++)
								JoinEnemy(65);
							JoinEnemy(70);

							mBattleEvent = 23;

							HideMap();
							DisplayEnemy();

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.AfterBattlePanzerViper)
						{
							mEncounterEnemyList.Clear();
							JoinEnemy(67);
							JoinEnemy(66);

							HideMap();
							DisplayEnemy();

							Talk(new string[] {
								$" 매우 수고하시는군요. {mPlayerList[0].Name}",
								" 당신이 네크로만서에게 가기 전에 한 가지 일러 두고자 하오.",
								" 이곳에는 비밀스러운 문이 두 군데 있소. 지금은 보이지가 않지만 양쪽의 벽을 살피다 보면 숨겨진 문안에 레버가 각각 하나씩 있소." +
								" 그걸 모두 작동시키면 용암의 중앙에서 네크로만서의 방으로 통하는 입구가 보일 것이오. 여기까지만 내가 알려줄 수가 있는 부분이오. 마지막으로 당신의 건투를 빌겠소."
							});

							foreach (var player in mPlayerList)
							{
								player.Class = 10;
								player.Level[1] = player.Level[0];
								player.Level[2] = player.Level[0];
							}

							mSpecialEvent = SpecialEventType.AfterLordAhnAndAncientEvil;
						}
						else if (mSpecialEvent == SpecialEventType.AfterLordAhnAndAncientEvil) {
							mEncounterEnemyList.Clear();

							ShowMap();

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleBlackKnight)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 5; i++)
								JoinEnemy(62);
							JoinEnemy(71);

							mBattleEvent = 28;

							HideMap();
							DisplayEnemy();

							StartBattle(true);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleNecromancer)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							mEncounterEnemyList.Clear();
							for (var i = 0; i < 7; i++)
							{
								JoinEnemy(68 + i);
							}

							mBattleEvent = 24;

							HideMap();
							DisplayEnemy();

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.FailRunawayBattleNecromancer)
						{
							mBattleEvent = 24;

							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.Ending)
						{
							Window.Current.CoreWindow.KeyDown -= gamePageKeyDownEvent;
							Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
							Frame.Navigate(typeof(Ending), mPlayerList[0].Name);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAhnInLastShelter)
						{
							mParty.Etc[42] |= 1 << 3;
							mMapLayer[32 + mMapWidth * 9] = 47;

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore)
						{
							Talk(new string[] {
								" 당신과는 초면이 아니지요? 로어 성에서도 봤으니까 말이죠." +
								" 당신은 나의 정체가 어떨지 궁금하기도 하겠지만 나중에 밝혀질 일이이까 천천히 알아보기로 하고 먼저 이 피라미드에 대해서 말하기로 하지요.",
								" 이 피라미드는 네크로만서와 함께 저편의 공간에서 퉁겨져 나왔지요. 이곳은 육신은 죽고 의지만 남은 사람들의 안식처라고도 할 수 있죠.",
								" 여기의 '의지'들 중에서 당신과 관계가 없는 의지는 모두 재가 되어버릴 거요." +
								" 여기서 얻은 정보는 모두 당신의 운명을 더욱더 모질게 만들어 버릴 것들이지만, 만약 당신이 알지 못한다면 더더욱 더 당신을 힘겹게 하는 것들만 있지요." +
								" 당신의 현명한 판단에 모든 걸 맡기도록 하지요." });

							mSpecialEvent = SpecialEventType.MeetAhnInAnotherLore2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore2)
						{
							mMapLayer[14 + mMapWidth * 5] = 52;

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb)
						{
							Talk(new string[] {
								" 오! 당신이 나의 잠을 깨웠나?",
								$" 실로 몇 천년 만에 보는 세상이군. 나는 당신의 운명적인 만남을 관장하는 [color={RGB.LightRed}]데네브의 의지[/color]라고 불리고 있지." +
								" 당신이 만나게 될 사람들은 이미 자네가 세상에 나기 전부터 정해져 있었다네." +
								" 만약 그 사람들을 만나지 않고 지나쳐 버린다거나 못 만나는 경우가 생긴다면 절대로 네크로만서를 물리치지 못할걸세. 그렇다면 당신이 꼭 만나야 할 사람들을 말해 보겠네."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfDeneb2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb2)
						{
							Talk(new string[] {
								$"[color={RGB.White}] 레드 안타레스[/color]",
								" 그는 이미 죽은 지가 수 천년이 지났지만 그의 의지는 아직 노티스란 동굴속에 잠들어 있네.",
								" 그는 과거 최강의 마법사로서 이 땅을 통치하였고 다시 세계가 혼미스러울 때 새로이 나타나겠노라고 말하며 홀로 그 동굴에서 살다가 죽었지." +
								" 하지만 지금이 그가 말한 때라는 걸 그의 영혼이 알 수 있게 만 한다면, 그는 다시금 최강의 마법사로 부활해서 당신들을 도와주게 되는 사람이지."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfDeneb3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb3)
						{
							Talk(new string[] {
								$"[color={RGB.White}] 스피카[/color]",
								" 지금 초자연력에 대해 알고 있고 사용할 수 있는 사람은 몇 명 되지 않는데, 그중의 한 사람이 네크로만서이고 또 지금 말하는 스피카라네.",
								" 자네가 이 기술을 그녀에게 배우지 않는다면 네크로만서를 만나기 위한 도중에 무릎을 꿇고 말 것이며 설령 그와 대결하게 된다 해도 자네들은 참패를 하게 될걸세." +
								" 이 기술로 자연을 조작하고 인간의 마음을 읽으며 시공간을 넘겨 볼 수만 있다면 분명 당신은 세계 최강의 전사가 되어있을 걸세." +
								" 그리고, 그녀가 있는 곳은 바로 물로 덮인 대륙의 드래곤이 사는 동굴의 어느 깊숙한 곳이라네."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfDeneb4;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb4)
						{
							Talk(new string[] {
								$"[color={RGB.White}] 에인션트 이블[/color]",
								" 그는 지금, 늪의 대륙의 서쪽 바다 건너 작은 섬에 살고 있다네." +
								" 그가 사는 섬을 찾기는 무척 어렵겠지만 초자연력으로 공간을 넘겨 보아 위치를 파악한후 텔리포트 마법을 통해 이동하면 다다를수 있을 걸세." +
								" 그는 당신의 미래를 쉽게 풀어주고, 또 새로운 만남을 이어주게 될걸세."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfDeneb5;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb5)
						{
							Talk(new string[] {
								$" 이 외에도 폴라리스, 주니어 안타레스, 로어 헌터, 리겔 등등의 사람들을 수도 없이 만나게 되겠지만 반드시 당신에게 도움을 주지만은 않을 것이며," +
								"만약 당신이 남을 도와준다면 반드시 그도 당신에게 보이지 않는 도움을 주게 될 거라는 말을 끝으로 나는 다시 몇천 년의 잠으로 빠져들어야겠네. 그럼 안녕히 ..."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfDeneb6;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfDeneb6)
						{
							InvokeAnimation(AnimationType.Remains1);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfSirius)
						{
							Talk(new string[] {
								" 당신과 나의 만남은 이렇듯 운명적이네. 나는 당신이 이때쯤 나를 찾아오리라고 내가 잠들기 전 몇 천년 전에 이미 알고 있었다네.",
								$" 소개하지. 나는 당신의 순회적 운명을 관장하는 [color={RGB.LightRed}]시리우스의 의지[/color]라고 하네." +
								" 당신은 나를 처음 봤겠지만 나는 원래 당신의 또 다른 분신으로서 당신은 결코 나에게는 낯설지가 않다네." +
								"당신은 어느 순간에도 당신이 단지 이 세계에서만 당신의 삶이 존재한다고 생각하는가? 분명 지금 현재로서는 그렇게 밖에 생각을 못 하지만 실제는 그렇지 않지." +
								" 당신은 분명 이 순간에도 다른 공간에서는 또 다른 삶을 살고 있다네. 하지만 ... '또 다른 삶'이라고 내가 말했지만 결국은 한 운명을 가지고 계속 윤회하는 것일 뿐이지." +
								" 나는 이렇게 당신이 찾아오는 것만도 헤아릴 수 없이 겪었지. 언제나 당신은 정해진 운명 때문에 어쩔 수 없이 나를 계속 찾아오게 되는 거라네." +
								" 이해가 가지 않을 거라고 나도 생각하면서도 달리 설명할 방도가 없어서 이런 애매한 말만 되풀이할 뿐이네."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfSirius2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfSirius2)
						{
							Talk(new string[] {
								" 더 자세한 운명을 알고 싶다면 스왐프 게이트에 있는 피라미드의 예언서를 읽어 보게나. 그러면 더 이해가 빠를걸세.",
								" 그래도 잘 이해가 안 된다면 '늪의 대륙'의 외딴섬에 은둔하고 있는 드래코니안을 만나 보도록 하게." +
								" 그는 네크로만서가 이전의 공간에서 이 공간으로 온 이유를 알고 있기 때문이라네.",
								" 그럼 우리의 만남은 다음 공간의 또 다른 운명에 의해 다시 시작될걸세. 그럼 그때까지 안녕히!"
							});

							mSpecialEvent = SpecialEventType.MeetWillOfSirius3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfSirius3)
						{
							InvokeAnimation(AnimationType.Remains2);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfAlbireo)
						{
							Talk(new string[] {
								$" 나는 이 세계의 운명을 담당하는 [color={RGB.LightRed}]알비레오의 의지[/color]라고 하오." +
								" 나는 이 세상이 생기면서부터 늘 이런 모습으로 여기서 지내왔소. 나는 이제껏 세상의 많은 위기들을 보아왔소." +
								" 하지만 지금과 같은 공간을 뛰어넘은 침입자에 의한 위기는 처음이라고 기억되오. 나는 이곳에서도 이 세상 모든 것을 볼 수있소." +
								" 그리고 여태껏 당신의 자라온 모습과 여기에 서 있는 이유도 알고 있소. 또한 네크로만서가 이 세상에 온 이후로부터의 그의 행동도 보아왔소." +
								" 그리고 결국은 ... ... 당신과 그의 미래도 나에게는 보이고 있소. 나는 이 자리에서 미래의 일을 말할 수는 없소." +
								" 당신이 그 결말을 알고 싶다면 '늪의 대륙'에 있는 드래코니안을 만나 보는 게 좋을 거요. 그전에 에인션트 이블도 역시 만나게 될 것이오. 그리고 한 가지를 명심하시오.",
								$" [color={RGB.White}]당신이 옳다고 생각하는 게 항상 옳은 것은 아니오. 남들이 증오하는 것이 항상 당신에게 그릇되게 작용하지는 않을 것이오." +
								" 또한, 현재의 증오가 미래의 증오가 되지도 않을 것이오. 그리고 결국에는 네크로만서를 용서하시오.[/color]"
							});

							mSpecialEvent = SpecialEventType.MeetWillOfAlbireo2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfAlbireo2)
						{
							InvokeAnimation(AnimationType.Remains3);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfCanopus)
						{
							Talk(new string[] {
								$" 안녕하시오. 나는 네크로만서의 운명을 관장하고 있는 [color={RGB.LightRed}]카노푸스의 의지[/color]라고 하오." +
								" 이 공간뿐만 아니라 다른 공간에서 존재하고 있는 네크로만서의 운명 또한 내가 담당하는 범주에 들어간다오." +
								" 하지만 그는 그의 운명을 따르는 존재일 뿐 그 이상의 의미는 없소. 그는 수도없는 저편의 공간에서 지워진 운명을 다해갔소." +
								" 그리고 또 여기서 다시 그의 운명을 시작하려 하고 있소. 이 공간 하나 밖에 인식하지 못하는 인간의 사고로는 별 가치가 없는 일이지만," +
								" 운명을 따르기 위해 다음 공간에서도 같은 운명을 반복해야 한다는 그 금단의 이치는 분명 그도 따르고 싶지 않았을 것이오." +
								" 하지만 그는 그 자신도 그것이 절대 바뀔 수 없는 패러독스라는 걸 알고 있을게요. 네크로만서 그 자신은 정말 불행한 존재라오." +
								" 스스로의 운명을 등에 지고 힘겹게 차원을 쫓기어 다니는 힘없는 짐승일 뿐이오."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfCanopus2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfCanopus2)
						{
							Talk(new string[] {
								" 당신이 마지막의 그에게 받을 수 있는 느낌은 진정한 마음에서 우러나오는 동정일 것이오. 당신은 저번 공간에서도 역시 그것을 느꼈소." +
								" 하지만 그때의 기억은 당신이 이 세계에 그를 물리치려는 운명을 지니고 다시 태어났을 때 이미 사라져 버렸소. 당신은 내 말을 이해하지 못하겠지만 그것은 사실이오." +
								" 지금은 그를 증오로서 맞이하려 하겠지만 그 증오는 다시 다음 공간으로 이어지려 하오. 당신이 그를 물리친다고 하여도 다시 다음 공간에서도 지금과 같은 대립을 반복할 뿐이오." +
								" 당신과 그와의 대립은 이 우주가 생기기 시작할 때 부터 지금까지 수도 없이 반복했고 그 이후로도 이 우주가 사라질 때까지 영원히 반복될 것이오. 그리고 마지막으로 내가 하고 싶은 말은 이런것이오.",
								$"[color={RGB.White}] 당신은 결국 네크로만서를 동정할지 모르오. 하지만 결코 그만이 동정의 대상이 아니오. 결국은 그와 연관되어 운명이 결정지워진 당신도 예외가 될수는 없소.[/color]"
							});

							mSpecialEvent = SpecialEventType.MeetWillOfCanopus3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfCanopus3)
						{
							InvokeAnimation(AnimationType.Remains4);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfArcturus)
						{
							Talk(new string[] {
								$" 당신 앞의 유골은 바로 나의 것이오. 나를 소개하자면 [color={RGB.LightRed}]아크투루스의 의지[/color]라고 불리는 존재로서 에인션트 이블의 운명을 관장하고 있소." +
								" 이름에서 알 수 있듯이 그를 수호하는 의지는 목자의 성격을 갖고 있소. 하지만 그의 이름이나 평판으로 볼 때는 전혀 그런 성격을 알지 못하게 되고 마는 게 보통이오." +
								" 그는 로드 안과 대립하는 운명을 지니고 세상에 존재하게 되었소. 마치 당신과 네크로만서와 같이 말이오." +
								" 로드 안과의 대립 관계란 운명 때문에 그는 그의 모든 성격을 포기한 채 로드 안을 위하여 악의 편에 서게 되었던 것이오." +
								" 그때, 둘 중에 누구 하나가 악의 대표가 되지 않으면 안 되었고 로드 안은 결코 세인의 지탄을 받는 악을 대표하려고 하지 않음으로 해서 그가 직접 악의 대표가 되기로 하였던 것이오." +
								" 하지만 원래 그의 마음은 극도의 선에 있었기 때문에 결국 그의 악이란 악의 대표 정도 밖에는 될 수 없었던 것이오."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfArcturus2;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfArcturus2)
						{
							Talk(new string[] {
							" 하지만 반대로 로드 안의 입장에서는 그를 계속 비판하며 사람들에게 선의 개념을 심어주려 하였으므로 결국은 그에 대한 철저한 조작으로 위장해서 그를 비하 시키고 자신을 부각 시켜," +
								" 어릴 때부터 선과 악의 개념을 구분 시키고 악을 배척하는 생활을 하여 사회를 순탄하게 이끌어 나가려고 하였소. 물론 로드 안의 생각이 틀렸다고는 할 수 없소." +
								" 그게 로드 안의 운명이라 할 수 있기 때문이오. 항상 로드 안의 마음도 편하지 않다는 걸 알고 있소. 절친한 동반자인 그를 적으로 돌려버린 것도 그의 운명을 벗어날 수 없었기 때문이오." +
								" 그리고 그들의 경지는 반신반인이라는 최고의 경지에 올랐소. 당신 역시 네크로만서에게 도전하고자 한다면 그 경지에 다다라야 하오. 분명 그들 둘의 능력으로는 당신과 당신 일행들을 반신반인으로 만들어 줄 수 있을 것이오."
							});

							mSpecialEvent = SpecialEventType.MeetWillOfArcturus3;
						}
						else if (mSpecialEvent == SpecialEventType.MeetWillOfArcturus3)
						{
							InvokeAnimation(AnimationType.Remains5);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.ReadScroll)
						{
							InvokeAnimation(AnimationType.Remains6);

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid)
						{
							mMapLayer[80 + mMapWidth * 75] = 48;
							Talk($"[color={RGB.LightCyan}] 갑자기 피라미드가 아래로 가라앉기 시작했다[/color]");

							mSpecialEvent = SpecialEventType.SwampGatePyramid2;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid2)
						{
							Talk(new string[] {
								$"[color={RGB.White}] 그 물속에서 당신은 한 시대의 운명을 바라다 보고있었다[/color]",
								$"[color={RGB.White}] 당신은 왜 하필이면 당신이 이 세계에 뛰어들어 단신으로 악과 싸워야 하는 이유를 아는가?[/color]",
								$"[color={RGB.White}] 여기서 당신은 로드 안, 에인션트 이블, 네크로만서의 관계를 기술한 예언서를 발견하여 읽기 시작했다.[/color]"
							});

							mSpecialEvent = SpecialEventType.SwampGatePyramid3;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid3)
						{
							Talk(new string[] {
								$"[color={RGB.White}]CHAPTER 1[/color]",
								$"",
								" 이 세상에는 두 개의 개념이 필요하다.",
								" 그것은 바로 선과 악이다.",
								" 전자의 상징은 로드 안이고, 후자의 상징은 에인션트 이블이다."
							});

							mSpecialEvent = SpecialEventType.SwampGatePyramid4;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid4)
						{
							Talk(new string[] {
								$"[color={RGB.White}]CHAPTER 2[/color]",
								$"",
								" 만약 당신이 황야에서 에인션트 이블을 만나더라도 두려워하지 말라. 그는 비록 악의 표상이지만 네크로만서가 행하는 악과는 다른 표현임을 명심하라." +
								" 만약 세상이 \"선\"만이 있고 이런 \"악\"은 존재하지 않는다면 누구도 선의 중요성을 인식하지 못한 채 보편적인 진리로만 인식되어가는 시대가 올 것이며" +
								" 선으로 둘러쌓여진 생활에 대한 고마움을 망각하는 시대가 우리 앞에 도래하는 때가 결국 올 것이다." +
								" 그런 때가 오기 전에 사람들이 이런 선의 소중함을 느끼고 스스로 지키려고 노력하게 만들 하나의 개념이 필요하게 되었는데 이것이 바로 태초에 생겨난 악의 개념이었다." +
								" 하지만 일부러 뭇사람들에게 비난을 사면서까지 악을 대표해 줄 만한 자는 나타나지 않았다." +
								" 이에 스스로를 악의 집대성으로 불러주기를 요구하는 한 현자가 있었으니 본명은 알 수 없지만 그가 바로 에인션트 이블이라고 칭하는 자였다." +
								" 선에 의해 보호되어 너무나도 평화로운 생활을 해왔던 사람들은 이제 새로운 마음을 갖고 그에게 대항하는 자세를 취하게 되었다." +
								" 하지만 그는 실지로 사람들에게 해를 입히지 않았으며 그의 본심은 선에 있다는 걸 알아두기 바란다."
							});

							mSpecialEvent = SpecialEventType.SwampGatePyramid5;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid5)
						{
							Talk(new string[] {
								$"[color={RGB.White}]CHAPTER 3[/color]",
								$"",
								" 위에서 기술한 에인션트 이블이 의미하는 악과는 달리 네크로만서는 진정한 악의 의미를 알지 못한다. 그것으로 인해 에인션트 이블은 그를 벌하려 하는 것이다." +
								" 하지만 육체가 없어진 에인션트 이블의 능력으로는 그에게 대항하기가 어렵다고 단정하고는 그의 강력한 마력으로 미래의 역사를 뒤틀어 운명적으로 네크로만서에" +
								"대항하여야 하는 한 희생물을 창조해 냈으니..",
								".. 그는 바로 당신인 것이다."
							});

							mSpecialEvent = SpecialEventType.SwampGatePyramid6;
						}
						else if (mSpecialEvent == SpecialEventType.SwampGatePyramid6)
						{
							Talk(new string[] {
								$"[color={RGB.White}]CHAPTER 4[/color]",
								$"",
								" 네크로만서에게 대항할 수 있는 단 두 명의 존재는 바로 로드 안과 그의 대립자이며 깊은 관계를 가진 에인션트 이블이다.",
								" 그들은 모두 반신반인이라는 계급의 인물들이며 보통의 사람들은 상대하기조차 어려운 인물들이며 능력 또한 인간을 초월하는 것뿐이다." +
								" 그러므로 만약 당신이 네크로만서를 응징하려고 한다면 먼저 당신 자신이 반신반인이 되어야만 될 것이다."
							});

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.BattleExitSwampGate)
						{
							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 25;

							StartBattle(true);
						}
						else if (mSpecialEvent == SpecialEventType.DefeatAstralMud) {
							mParty.Etc[40] |= 1;

							mParty.Map = 4;
							mParty.XAxis = 81;
							mParty.YAxis = 16;

							await RefreshGame();

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.EnterImperiumMinor) {
							AppendText("");

							mParty.Etc[41] |= 1 << 6;

							await RefreshGame();

							mSpecialEvent = SpecialEventType.None;
						}
						else if (mSpecialEvent == SpecialEventType.EnterDungeonOfEvil) {
							var hasDraconian = false;
							foreach (var player in mPlayerList)	{
								if (player.Name == "드라코니안") {
									Talk(new string[] {
										" 아키드라코니안은 마지막에 있는 드래코니안을 발견했다.", 
										$"[color={RGB.LightMagenta}] 아니 너는 누구냐! 감히 드래코니안 족이면서 네크로만서님에게 반기를 들다니... 그것은 바로 죽음이다. 받아랏!![/color]"
									});

									player.HP = 0;
									player.Unconscious = 1;
									player.Dead = 30000;

									UpdatePlayersStat();

									mSpecialEvent = SpecialEventType.KillDraconian;

									hasDraconian = true;
									break;
								}
							}

							if (!hasDraconian)
							{
								mSpecialEvent = SpecialEventType.None;
								mBattleEvent = 27;

								StartBattle(true);
							}
						}
						else if (mSpecialEvent == SpecialEventType.KillDraconian) {
							mSpecialEvent = SpecialEventType.None;
							mBattleEvent = 27;

							StartBattle(false);
						}
						else if (mSpecialEvent == SpecialEventType.BattlePrison) {
							mBattleEvent = 1;

							var hasMadJoe = false;
							foreach (var player in mPlayerList)
							{
								if (player.Name == "미친 조")
								{
									Talk(new string[] { " 우리들 뒤에 있던 미친 조가 전투가 일어나자마자 도망을 가버렸다." });
									mPlayerList.Remove(player);
									DisplayPlayerInfo();

									hasMadJoe = true;
									break;
								}
							}

							if (!hasMadJoe)
							{
								mSpecialEvent = SpecialEventType.None;
								StartBattle(false);
							}
							else
								mSpecialEvent = SpecialEventType.RunawayMadJoe;
						}
						else if (mSpecialEvent == SpecialEventType.RunawayMadJoe)
						{
							StartBattle(false);

							mSpecialEvent = SpecialEventType.None;
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp ||
						args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft ||
						args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight ||
						args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
						return;

					if (StatPanel.Visibility == Visibility.Visible)
						StatPanel.Visibility = Visibility.Collapsed;

					if (StatHealthPanel.Visibility == Visibility.Visible)
						StatHealthPanel.Visibility = Visibility.Collapsed;

					if (DialogText.Visibility == Visibility.Collapsed)
						DialogText.Visibility = Visibility.Visible;

					ContinueText.Visibility = Visibility.Collapsed;

					if (mBattleTurn == BattleTurn.None)
					{
						DialogText.Blocks.Clear();
						DialogText.TextHighlighters.Clear();
					}

					if (mCureResult.Count > 0)
						ShowCureResult();
					else if (mRemainBattleResult.Count > 0) {
						DialogText.Blocks.Clear();
						DialogText.TextHighlighters.Clear();

						var added = true;
						while (added && mRemainBattleResult.Count > 0) {
							added = AppendText(mRemainBattleResult[0], true);
							if (added)
								mRemainBattleResult.RemoveAt(0);
						}

						ContinueText.Visibility = Visibility.Visible;
					}
					else if (mSpecialEvent > 0)
						await InvokeSpecialEventLaterPart();
					else if (mCureBattle)
					{
						mCureBattle = false;
						AddBattleCommand(true);
					}
					else if (mPenetration == true)
					{
						mPenetration = false;
					}
					else if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
					{
						if ((mTelescopeXCount != 0 && (mParty.XAxis + mTelescopeXCount <= 4 || mParty.XAxis + mTelescopeXCount >= mMapWidth - 3)) ||
							(mTelescopeXCount != 0 && (mParty.YAxis + mTelescopeYCount <= 4 || mParty.YAxis + mTelescopeYCount >= mMapHeight - 3)))
						{
							mTelescopeXCount = 0;
							mTelescopeYCount = 0;
							return;
						}

						if (mTelescopeXCount < 0)
							mTelescopeXCount++;

						if (mTelescopeXCount > 0)
							mTelescopeXCount--;

						if (mTelescopeYCount < 0)
							mTelescopeYCount++;

						if (mTelescopeYCount > 0)
							mTelescopeYCount--;

						if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
							Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
					}
					else if (mTalkMode > 0)
					{
						if (mSpecialEvent == 0)
							TalkMode(mTalkX, mTalkY, args.VirtualKey);
					}
					else if (mBattleTurn == BattleTurn.Player)
					{
						if (mBattleCommandQueue.Count == 0)
						{
							var allUnavailable = true;
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && !enemy.Unconscious)
								{
									allUnavailable = false;
									break;
								}
							}

							if (allUnavailable)
							{
								mBattleTurn = BattleTurn.Win;
								await EndBattle();
							}
							else
								ExecuteBattle();
						}
						else
							ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.Enemy)
					{
						ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.RunAway || mBattleTurn == BattleTurn.Win || mBattleTurn == BattleTurn.Lose)
					{
						await EndBattle();
					}
					else if (mWeaponShopEnd)
					{
						mWeaponShopEnd = false;
						GoWeaponShop();
					}
					else if (mTrainingEnd == true)
					{
						mTrainingEnd = false;
						GoTrainingCenter();
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
				else if (mSpinnerType != SpinnerType.None) {
					void CloseSpinner() {
						SpinnerText.TextHighlighters.Clear();
						SpinnerText.Blocks.Clear();
						SpinnerText.Visibility = Visibility.Collapsed;

						mSpinnerItems = null;
						mSpinnerID = 0;
						mSpinnerType = SpinnerType.None;
					}

					if(args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						mSpinnerID = (mSpinnerID + 1) % mSpinnerItems.Length;

						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mSpinnerID == 0)
							mSpinnerID = mSpinnerItems.Length - 1;
						else
							mSpinnerID--;
						
						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						if (mSpinnerType == SpinnerType.TeleportationRange) {
							AppendText("");

							CloseSpinner();
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mSpinnerType == SpinnerType.TeleportationRange) {
							int moveX = mParty.XAxis;
							int moveY = mParty.YAxis;

							switch (mTeleportationDirection) {
								case 0:
									moveY -= mSpinnerItems[mSpinnerID].Item2;
									break;
								case 1:
									moveY += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 2:
									moveX += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 3:
									moveX -= mSpinnerItems[mSpinnerID].Item2;
									break;
							}

							if (moveX < 4 || moveX > mMapWidth - 4 || moveY < 4 || moveY > mMapHeight - 4)
								AppendText("공간 이동이 통하지 않습니다.");
							else {
								var valid = false;
								if (mPosition == PositionType.Town) {
									if (27 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Ground) {
									if (24 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Den) {
									if (41 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Keep)
								{
									if (27 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
										valid = true;
								}

								if (!valid)
									AppendText("공간 이동 장소로 부적합 합니다.");
								else {
									mMagicPlayer.SP -= 50;

									if (mMapLayer[moveX + mMapWidth * moveY] == 0 || ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[moveX + mMapWidth * moveY] == 52))
										AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신을 배척합니다.[/color]");
									else {
										mParty.XAxis = moveX;
										mParty.YAxis = moveY;

										AppendText($"[color={RGB.White}]공간 이동 마법이 성공했습니다.[/color]");
									}
								}
							}
						}

						CloseSpinner();
					}
				}
				else if (mMenuMode != MenuMode.None)
				{
					void ShowCastOneMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount;
						if (0 <= player.Level[1] && player.Level[1] <= 1)
							availCount = 1;
						else if (2 <= player.Level[1] && player.Level[1] <= 3)
							availCount = 2;
						else if (4 <= player.Level[1] && player.Level[1] <= 7)
							availCount = 3;
						else if (8 <= player.Level[1] && player.Level[1] <= 11)
							availCount = 4;
						else if (12 <= player.Level[1] && player.Level[1] <= 15)
							availCount = 5;
						else
							availCount = 6;

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
							availCount = 0;
						else if (player.Level[1] == 2)
							availCount = 1;
						else if (3 <= player.Level[1] && player.Level[1] <= 5)
							availCount = 2;
						else if (6 <= player.Level[1] && player.Level[1] <= 9)
							availCount = 3;
						else if (10 <= player.Level[1] && player.Level[1] <= 13)
							availCount = 4;
						else if (14 <= player.Level[1] && player.Level[1] <= 17)
							availCount = 5;
						else
							availCount = 6;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							const int allMagicIdx = 6;
							for (var i = 1 + allMagicIdx; i <= availCount + allMagicIdx; i++)
							{
								menuStr[i - allMagicIdx - 1] = Common.GetMagicStr(i);
							}

							ShowMenu(MenuMode.CastAllMagic, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastSpecialMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount;
						if (0 <= player.Level[1] && player.Level[1] <= 4)
							availCount = 0;
						else if (5 <= player.Level[1] && player.Level[1] <= 9)
							availCount = 1;
						else if (10 <= player.Level[1] && player.Level[1] <= 11)
							availCount = 2;
						else if (12 <= player.Level[1] && player.Level[1] <= 13)
							availCount = 3;
						else if (14 <= player.Level[1] && player.Level[1] <= 15)
							availCount = 4;
						else if (16 <= player.Level[1] && player.Level[1] <= 17)
							availCount = 5;
						else
							availCount = 6;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							const int allSpecialIdx = 12;
							for (var i = 1 + allSpecialIdx; i <= availCount + allSpecialIdx; i++)
							{
								menuStr[i - allSpecialIdx - 1] = Common.GetMagicStr(i);
							}

							ShowMenu(MenuMode.CastSpecial, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCureDestMenu(Lore player, MenuMode menuMode)
					{
						AppendText(new string[] { "누구에게" });
						string[] playerList;

						if (player.Level[1] / 2 - 3 <= 0)
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
						else
						{
							if (mMenuFocusID == 0)
								mMenuFocusID = mMenuCount - 1;
							else
								mMenuFocusID--;

							FocusMenuItem();
						}

					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								mEnemyFocusID = (mEnemyFocusID + 1) % mEncounterEnemyList.Count;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else
						{
							mMenuFocusID = (mMenuFocusID + 1) % mMenuCount;
							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						if (mMenuMode != MenuMode.None && mMenuMode != MenuMode.BattleLose && mMenuMode != MenuMode.ChooseGameOverLoadGame && mSpecialEvent == 0)
						{
							if (mMenuMode == MenuMode.CastOneMagic ||
							mMenuMode == MenuMode.CastAllMagic ||
							mMenuMode == MenuMode.CastSpecial ||
							mMenuMode == MenuMode.ChooseBattleCureSpell ||
							mMenuMode == MenuMode.ApplyBattleCureSpell ||
							mMenuMode == MenuMode.ApplyBattleCureAllSpell ||
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

						void ShowFileMenu(MenuMode mode) {
							if (mode == MenuMode.ChooseLoadGame || mode == MenuMode.ChooseGameOverLoadGame)
								AppendText("불러내고 싶은 게임을 선택하십시오.");
							else
								AppendText("게임의 저장 장소를 선택하십시오.");

							ShowMenu(mode, new string[] {
								"본 게임 데이터",
								"게임 데이터 1 (부)",
								"게임 데이터 2 (부)",
								"게임 데이터 3 (부)"
							});
						}

						async Task<bool> LoadGame(int id)
						{
							mMenuMode = MenuMode.None;

							var success = await LoadFile(id);
							if (success)
							{
								mBattlePlayerID = 0;
								mBattleFriendID = 0;
								mBattleCommandID = 0;
								mBattleToolID = 0;
								mEnemyFocusID = 0;
								mBattleCommandQueue.Clear();
								mBatteEnemyQueue.Clear();
								mBattleTurn = BattleTurn.None;

								mSpecialEvent = SpecialEventType.None;
								mBattleEvent = 0;

								ShowMap();

								AppendText(new string[] { $"[color={RGB.LightCyan}]저장했던 게임을 다시 불러옵니다.[/color]" });

								return true;
							}
							else {
								AppendText(new string[] { $"[color={RGB.LightRed}]해당 슬롯에는 저장된 게임이 없습니다. 다른 슬롯을 선택해 주십시오.[/color]" });

								ShowFileMenu(MenuMode.ChooseLoadGame);
								return false;
							}
						}



						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							AddBattleCommand();
						}
						else
						{
							void ShowCureSpellMenu(Lore player, int whomID, MenuMode applyCureMode, MenuMode applyAllCureMode)
							{
								if (whomID < mPlayerList.Count)
								{
									ShowMenu(applyCureMode, GetCureSpellList(player));
								}
								else
								{
									var availableCount = player.Level[1] / 2 - 3;
									var totalMagicCount = 32 - 26 + 1;
									if (availableCount < totalMagicCount)
										totalMagicCount = availableCount;

									var cureMagicMenu = new string[totalMagicCount];
									for (var i = 26; i < 26 + availableCount; i++)
										cureMagicMenu[i - 26] = Common.GetMagicStr(i);

									ShowMenu(applyAllCureMode, cureMagicMenu);
								}
							}

							string[] GetCureSpellList(Lore player)
							{
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
									ShowPartyStatus();
								}
								else if (mMenuFocusID == 1)
								{
									AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
									ShowCharacterMenu(MenuMode.ViewCharacter);
								}
								else if (mMenuFocusID == 2)
								{
									AppendText("");
									DialogText.Visibility = Visibility.Collapsed;

									for (var i = 0; i < 6; i++) {
										if (i < mPlayerList.Count)
											mHealthTextList[i].Update(mPlayerList[i].Name, mPlayerList[i].Poison, mPlayerList[i].Unconscious, mPlayerList[i].Dead);
										else
											mHealthTextList[i].Clear();
									}

									StatHealthPanel.Visibility = Visibility.Visible;
									ContinueText.Visibility = Visibility.Visible;
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

								AppendText("");

								StatPlayerName.Text = mPlayerList[mMenuFocusID].Name;
								StatPlayerGender.Text = mPlayerList[mMenuFocusID].GenderName;
								StatPlayerClass.Text = Common.GetClassStr(mPlayerList[mMenuFocusID]);

								StatStrength.Text = mPlayerList[mMenuFocusID].Strength.ToString();
								StatMentality.Text = mPlayerList[mMenuFocusID].Mentality.ToString();
								StatConcentration.Text = mPlayerList[mMenuFocusID].Concentration.ToString();
								StatEndurance.Text = mPlayerList[mMenuFocusID].Endurance.ToString();

								StatResistance.Text = mPlayerList[mMenuFocusID].Resistance.ToString();
								StatAgility.Text = mPlayerList[mMenuFocusID].Agility.ToString();
								StatLuck.Text = mPlayerList[mMenuFocusID].Luck.ToString();

								StatWeaponAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[0].ToString();
								StatMagicAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[1].ToString();
								StatESPAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[2].ToString();

								StatAttackLevel.Text = mPlayerList[mMenuFocusID].Level[0].ToString();
								StatMagicLevel.Text = mPlayerList[mMenuFocusID].Level[1].ToString();
								StatESPLevel.Text = mPlayerList[mMenuFocusID].Level[2].ToString();

								StatExp.Text = mPlayerList[mMenuFocusID].Experience.ToString();

								StatWeapon.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Weapon);
								StatShield.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Shield);
								StatArmor.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Armor);

								DialogText.Visibility = Visibility.Collapsed;
								StatPanel.Visibility = Visibility.Visible;

								ContinueText.Visibility = Visibility.Visible;
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
									AppendText(new string[] { $"{GetGenderData(mPlayerList[mMenuFocusID])}는 마법을 사용할 수 있는 상태가 아닙니다" });
								}
							}
							else if (mMenuMode == MenuMode.SpellCategory)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									AppendText(new string[] { "전투 모드가 아닐 때는 공격 마법을 사용할 수 없습니다." });
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
							else if (mMenuMode == MenuMode.ChooseBattleCureSpell)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID < mPlayerList.Count)
									mMagicWhomPlayer = mPlayerList[mMenuFocusID];

								ShowCureSpellMenu(mPlayerList[mBattlePlayerID], mMenuFocusID, MenuMode.ApplyBattleCureSpell, MenuMode.ApplyBattleCureAllSpell);
							}
							else if (mMenuMode == MenuMode.ApplyCureMagic)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

								ShowCureResult();
							}
							else if (mMenuMode == MenuMode.ApplyCureAllMagic)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

								ShowCureResult();
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
										DisplaySP();
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
										DisplaySP();
									}
								}
								else if (mMenuFocusID == 2)
								{
									if (mMagicPlayer.SP < 10)
										ShowNotEnoughSP();
									else
									{
										mParty.Etc[1] = 255;

										AppendText(new string[] { $"[color={RGB.White}]일행은 물 위를 걸을 수 있습니다.[/color]" });
										mMagicPlayer.SP -= 10;
										DisplaySP();
									}
								}
								else if (mMenuFocusID == 3)
								{
									if (mMagicPlayer.SP < 20)
										ShowNotEnoughSP();
									else
									{
										mParty.Etc[2] = 255;

										AppendText(new string[] { $"[color={RGB.White}]일행은 늪 위를 걸을 수 있습니다.[/color]" });
										mMagicPlayer.SP -= 20;
										DisplaySP();
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
										AppendText(new string[] { $"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
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
										AppendText(new string[] { $"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
									else if (mMagicPlayer.SP < 50)
										ShowNotEnoughSP();
									else
									{
										AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

										ShowMenu(MenuMode.TeleportationDirection, new string[] { "북쪽으로 공간이동",
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
										DisplaySP();

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
									DisplaySP();

									if (mMapLayer[(mParty.XAxis + xOffset) + mMapWidth * (mParty.YAxis + yOffset)] == 0 ||
										((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[(newX + xOffset) + mMapWidth * (newY + yOffset)] == 52))
									{
										AppendText(new string[] { $"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
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

								var newX = mParty.XAxis + xOffset;
								var newY = mParty.YAxis + yOffset;


								mMagicPlayer.SP -= 30;
								DisplaySP();

								if (mMapLayer[newX + mMapWidth * newY] == 0 ||
										((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[newX + mMapWidth * newY] == 52))
								{
									AppendText(new string[] { $"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
								}
								else
								{
									byte tile;

									switch (mPosition)
									{
										case PositionType.Town:
											tile = 47;
											break;
										case PositionType.Ground:
											tile = 41;
											break;
										case PositionType.Den:
											tile = 43;
											break;
										default:
											tile = 43;
											break;
									}

									mMapLayer[newX + mMapWidth * newY] = tile;

									AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
								}
							}
							else if (mMenuMode == MenuMode.TeleportationDirection) {
								mMenuMode = MenuMode.None;

								mTeleportationDirection = mMenuFocusID;

								var rangeItems = new List<Tuple<string, int>>();
								for (var i = 1; i <= 9; i++) {
									rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i * 1000}[/color] [color={RGB.White}] 공간 이동력[/color]", i));
								}

								ShowSpinner(SpinnerType.TeleportationRange, rangeItems.ToArray(), 5);
							}
							else if (mMenuMode == MenuMode.Extrasense)
							{
								mMenuMode = MenuMode.None;

								if (mPlayerList[mMenuFocusID].IsAvailable)
								{
									if (mPlayerList[mMenuFocusID].Class != 2 && mPlayerList[mMenuFocusID].Class != 3 && mPlayerList[mMenuFocusID].Class != 6 && (mParty.Etc[37] & 1) == 0)
									{
										AppendText(new string[] { $"당신에게는 아직 능력이 없습니다." }, true);
										ContinueText.Visibility = Visibility.Visible;
									}
									else
									{
										mMagicPlayer = mPlayerList[mMenuFocusID];

										AppendText(new string[] { "사용할 초감각의 종류 ===>" });

										var extrsenseMenu = new string[5];
										for (var i = 41; i <= 45; i++)
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
										mPenetration = true;
										Talk($"[color={RGB.White}]일행은 주위를 투시하고 있다.[/color]");
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
										"메너스를 탐험할",
										"로드 안에게 다시 돌아갈",
										"라스트디치로 갈",
										"라스트디치의 성주를 만날",
										"피라미드 속의 미이라 장군을 물리칠",
										"라스트디치의 성주에게로 돌아갈",
										"라스트디치의 지하 출입구로 갈",
										"가이아 테라의 성주를 만날",
										"이블 씰에서 황금의 봉인을 발견할",
										"가이아 테라의 성주에게 돌아갈",
										"퀘이크에서 아키가고일를 물리칠",
										"북동쪽의 와이번 동굴에 갈",
										"워터 필드로 갈",
										"워터 필드의 군주를 만날",
										"노티스 속의 히드라를 물리칠",
										"락업 속의 드래곤을 물리칠",
										"가이아 테라의 스왐프 게이트로 갈",
										"위쪽의 게이트를 통해 스왐프 킵으로 갈",
										"스왐프 대륙에 존재하는 두개의 봉인을 풀",
										"스왐프 킵의 라바 게이트를 작동시킬",
										"적의 집결지인 이블 컨센츄레이션으로 갈",
										"숨겨진 적의 마지막 요새로 들어갈",
										"위쪽의 동굴에서 네크로만서를 만날",
										"네크로만서와 마지막 결전을 벌일"
										};

										if (mMagicPlayer.ESP < 5)
											ShowNotEnoughESP();
										else
										{
											var predict = GetPredict();

											AppendText(new string[] { $" 당신은 당신의 미래를 예언한다 ...", "" });
											if (0 <= predict && predict < predictStr.Length)
												AppendText(new string[] { $" # 당신은 [color={RGB.LightGreen}]{predictStr[predict]} 것이다[/color]" }, true);
											else
												AppendText(new string[] { $" # [color={RGB.LightGreen}]당신은 어떤 힘에 의해 예언을 방해 받고 있다[/color]" }, true);

											mMagicPlayer.ESP -= 5;
											DisplayESP();

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
										AppendText(new string[] { $"[color={RGB.White}]당신은 잠시 동안 다른 사람의 마음을 읽을 수 있다.[/color]" });
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
								mMagicPlayer.ESP -= mMagicPlayer.Level[2] * 5;
								DisplayESP();

								mTelescopePeriod = mMagicPlayer.Level[2];
								switch (mMenuFocusID)
								{
									case 0:
										mTelescopeYCount = -mMagicPlayer.Level[2];
										break;
									case 1:
										mTelescopeYCount = mMagicPlayer.Level[2];
										break;
									case 2:
										mTelescopeXCount = mMagicPlayer.Level[2];
										break;
									case 3:
										mTelescopeXCount = -mMagicPlayer.Level[2];
										break;
								}

								Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
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
									ShowFileMenu(MenuMode.ChooseLoadGame);
								}
								else if (mMenuFocusID == 4)
								{
									ShowFileMenu(MenuMode.ChooseSaveGame);
								}
								else if (mMenuFocusID == 5)
								{
									AppendText(new string[] { $"[color={RGB.LightGreen}]정말로 끝내겠습니까?[/color]" });

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

								AppendText(new string[] { $"[color={RGB.LightRed}]일행들의 지금 성격은 어떻습니까?[/color]" });

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

								AppendText(new string[] { "" });

								mEncounter = 6 - (mMenuFocusID + 1);
							}
							else if (mMenuMode == MenuMode.OrderFromCharacter)
							{
								mMenuMode = MenuMode.None;

								mOrderFromPlayerID = mMenuFocusID;

								AppendText(new string[] { $"[color={RGB.LightCyan}]순서를 바꿀 일원[/color]" });

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

								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(0);
									player.Name = "미친 조";
									player.Class = 8;
									player.Weapon = 0;
									player.Shield = 0;
									player.Armor = 0;
									player.WeaPower = 0;
									player.ShiPower = 0;
									player.ArmPower = 0;
									player.AC = 0;

									JoinMember(player);

									mMapLayer[39 + mMapWidth * 14] = 47;

									mParty.Etc[49] |= 1 << 1;
								}
								else
									ShowNoThanks();
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
									AppendText(new string[] { $"[color={RGB.White}]어떤 무기를 원하십니까?[/color]" });

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
									AppendText(new string[] { $"[color={RGB.White}]어떤 방패를 원하십니까?[/color]" });

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
									AppendText(new string[] { $"[color={RGB.White}]어떤 갑옷를 원하십니까?[/color]" });

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

									AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetWeaponStr(mBuyWeaponID)}를 사용하시겠습니까?[/color]" });

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

									AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}방패를 사용하시겠습니까?[/color]" });

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

								mParty.Gold -= GetShieldPrice(mBuyWeaponID);

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

									AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}갑옷을 사용하시겠습니까?[/color]" });

									ShowCharacterMenu(MenuMode.UseArmorCharacter);
								}
							}
							else if (mMenuMode == MenuMode.UseArmorCharacter)
							{
								mMenuMode = MenuMode.None;

								var player = mPlayerList[mMenuFocusID];

								player.Armor = mBuyWeaponID;
								player.ArmPower = mBuyWeaponID + 1;
								player.AC = player.ShiPower + player.ArmPower;

								if (player.Class == 1)
									player.AC++;

								if (player.AC > 10)
									player.AC = 10;

								mParty.Gold -= GetArmorPrice(mBuyWeaponID);

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
								var nextFinalLevel = 0;
								if (exp < 20000)
								{
									if (0 <= exp && exp <= 1499)
										nextFinalLevel = 1;
									else if (1500 <= exp && exp <= 5999)
										nextFinalLevel = 2;
									else if (6000 <= exp && exp <= 19999)
										nextFinalLevel = 3;
								}
								else
								{
									exp /= 10000;

									if (2 <= exp && exp <= 4)
										nextFinalLevel = 4;
									else if (5 <= exp && exp <= 14)
										nextFinalLevel = 5;
									else if (15 <= exp && exp <= 24)
										nextFinalLevel = 6;
									else if (25 <= exp && exp <= 49)
										nextFinalLevel = 7;
									else if (50 <= exp && exp <= 79)
										nextFinalLevel = 8;
									else if (80 <= exp && exp <= 104)
										nextFinalLevel = 9;
									else if (105 <= exp && exp <= 131)
										nextFinalLevel = 10;
									else if (132 <= exp && exp <= 161)
										nextFinalLevel = 11;
									else if (162 <= exp && exp <= 194)
										nextFinalLevel = 12;
									else if (195 <= exp && exp <= 230)
										nextFinalLevel = 13;
									else if (231 <= exp && exp <= 269)
										nextFinalLevel = 14;
									else if (270 <= exp && exp <= 311)
										nextFinalLevel = 15;
									else if (312 <= exp && exp <= 356)
										nextFinalLevel = 16;
									else if (357 <= exp && exp <= 404)
										nextFinalLevel = 17;
									else if (405 <= exp && exp <= 455)
										nextFinalLevel = 18;
									else if (456 <= exp && exp <= 509)
										nextFinalLevel = 19;
									else
										nextFinalLevel = 20;
								}

								var payment = 0;
								if (player.Level[0] == 20)
								{
									AppendText(new string[] { $"당신은 최고 레벨에 도달했습니다.",
									"더 이상 저희들은 가르칠 필요가 없습니다." });

									ContinueText.Visibility = Visibility.Visible;

									mTrainingEnd = true;
								}
								else if (player.Level[0] < nextFinalLevel)
								{
									switch (nextFinalLevel)
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

									if (mParty.Gold < payment)
									{
										AppendText(new string[] { $"당신은 금 {payment - mParty.Gold}개가 더 필요합니다." });

										ContinueText.Visibility = Visibility.Visible;

										mTrainingEnd = true;
									}
									else
									{
										mParty.Gold -= payment;

										AppendText(new string[] { $"[color={RGB.White}]{player.Name}의 레벨은 {nextFinalLevel}입니다." });

										for (var nextLevel = player.Level[0] + 1; nextLevel <= nextFinalLevel; nextLevel++)
										{
											player.Level[0] = nextLevel;

											if (player.Class == 1)
											{
												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Strength < 20)
													player.Strength++;
												else if (player.Endurance < 20)
													player.Endurance++;
												else if (player.Accuracy[0] < 20)
													player.Accuracy[0]++;
												else
													player.Agility++;
												//}
											}
											else if (player.Class == 2 || player.Class == 9)
											{
												player.Level[1] = nextLevel;
												player.Level[2] = (int)Math.Round((double)nextLevel / 2);

												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Mentality < 20)
													player.Mentality++;
												else if (player.Concentration < 20)
													player.Concentration++;
												else if (player.Accuracy[1] < 20)
													player.Accuracy[1]++;
												//}
											}
											else if (player.Class == 3)
											{
												player.Level[1] = (int)Math.Round((double)nextLevel / 2);
												player.Level[2] = nextLevel;

												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Concentration < 20)
													player.Concentration++;
												else if (player.Accuracy[2] < 20)
													player.Accuracy[2]++;
												else if (player.Mentality < 20)
													player.Mentality++;
												//}
											}
											else if (player.Class == 4)
											{
												if (nextFinalLevel < 16)
													player.Level[1] = nextLevel;
												else
													player.Level[1] = 16;

												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Strength < 20)
													player.Strength++;
												else if (player.Mentality < 20)
													player.Mentality++;
												else if (player.Accuracy[0] < 20)
													player.Accuracy[0]++;
												else if (player.Accuracy[1] < 20)
													player.Accuracy[1]++;
												//}
											}
											else if (player.Class == 5)
											{
												player.WeaPower = player.Level[0] * 2 + 10;

												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Strength < 20)
													player.Strength++;
												else if (player.Accuracy[1] < 20)
													player.Accuracy[1]++;
												else if (player.Endurance < 20)
													player.Endurance++;
												//}
											}
											else if (player.Class == 6)
											{
												player.Level[1] = (int)Math.Round((double)nextLevel / 2);
												player.Level[2] = nextLevel;

												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Resistance < 18)
													player.Resistance++;
												else if (player.Resistance < 20)
												{
													if (player.Luck > mRand.Next(21))
														player.Resistance++;
												}
												else
													player.Agility++;
												//}
											}
											else if (player.Class == 7 || player.Class == 8)
											{
												//if (player.Luck > mRand.Next(30))
												//{
												if (player.Endurance < 20)
													player.Endurance++;
												else if (player.Strength < 20)
													player.Strength++;
												else
													player.Agility++;
												//}
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
										}

										ContinueText.Visibility = Visibility.Visible;

										DisplayPlayerInfo();

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
											Talk(" 당신이 로어 성을 떠나려는 순간 누군가가 당신을 불렀다.");

											mSpecialEvent = SpecialEventType.MeetSkeleton;
										}
										else
											await ExitCastleLore();
									}
									else if (mParty.Map == 7)
									{
										mParty.Map = 1;
										mParty.XAxis = 76;
										mParty.YAxis = 56;

										await RefreshGame();
									}
									else if (mParty.Map == 8)
									{
										mParty.Map = 2;
										mParty.XAxis = 18;
										mParty.YAxis = 26;

										await RefreshGame();
									}
									else if (mParty.Map == 9)
									{
										mParty.Map = 2;
										mParty.XAxis = 31;
										mParty.YAxis = 81;

										await RefreshGame();
									}
									else if (mParty.Map == 10)
									{
										mParty.Map = 3;
										mParty.XAxis = 73;
										mParty.YAxis = 19;

										await RefreshGame();
									}
									else if (mParty.Map == 11)
									{
										mParty.Map = 7;
										mParty.XAxis = 37;
										mParty.YAxis = 6;

										await RefreshGame();

										foreach (var player in mPlayerList)
										{
											if (player.Name == "폴라리스")
											{
												mMapLayer[36 + mMapWidth * 40] = 44;
												break;
											}
										}
									}
									else if (mParty.Map == 12)
									{
										mParty.Map = 8;
										mParty.XAxis = 38;
										mParty.YAxis = 6;

										await RefreshGame();
									}
									else if (mParty.Map == 13) {
										mParty.Map = 9;
										mParty.XAxis = 25;
										mParty.YAxis = 5;

										await RefreshGame();
									}
									else if (mParty.Map == 14)
									{
										mParty.Map = 1;
										mParty.XAxis = 17;
										mParty.YAxis = 88;

										await RefreshGame();
									}
									else if (mParty.Map == 15)
									{
										mParty.Map = 2;
										mParty.XAxis = 81;
										mParty.YAxis = 47;

										await RefreshGame();
									}
									else if (mParty.Map == 16)
									{
										mParty.Map = 2;
										mParty.XAxis = 43;
										mParty.YAxis = 7;

										await RefreshGame();
									}
									else if (mParty.Map == 17)
									{
										mParty.Map = 3;
										mParty.XAxis = 22;
										mParty.YAxis = 62;

										await RefreshGame();
									}
									else if (mParty.Map == 18)
									{
										mParty.Map = 3;
										mParty.XAxis = 95;
										mParty.YAxis = 42;

										await RefreshGame();
									}
									else if (mParty.Map == 19)
									{
										mParty.Map = 4;
										mParty.XAxis = 47;
										mParty.YAxis = 57;

										await RefreshGame();
									}
									else if (mParty.Map == 20)
									{
										mParty.Map = 4;
										mParty.XAxis = 81;
										mParty.YAxis = 16;

										await RefreshGame();
									}
									else if (mParty.Map == 21)
									{
										if ((mParty.Etc[41] & 1) == 0)
										{
											mEncounterEnemyList.Clear();
											if ((mParty.Etc[41] & (1 << 2)) == 0)
												JoinEnemy(54);

											if ((mParty.Etc[41] & (1 << 3)) == 0)
												JoinEnemy(55);

											if (mEncounterEnemyList.Count == 0)
											{
												mParty.Etc[41] |= 1;
												return;
											}

											for (var i = 0; i < 4; i++)
											{
												JoinEnemy(34);
											}

											HideMap();
											DisplayEnemy();

											mBattleEvent = 15;

											StartBattle(false);
										}
									}
									else if (mParty.Map == 22)
									{
										if ((mParty.Etc[42] & (1 << 2)) == 0)
										{
											mEncounterEnemyList.Clear();

											var enemyID = mRand.Next(5) + 41;
											if (enemyID == 41)
												enemyID = 34;

											for (var i = 0; i < 6; i++)
											{
												JoinEnemy(enemyID);
											}

											JoinEnemy(65);

											HideMap();
											DisplayEnemy();

											Talk($"[color={RGB.LightCyan}]{mPlayerList[0].Name}, 나의 힘을 보여주겠다.[/color]");

											mSpecialEvent = SpecialEventType.ExitImperiumMinor;
										}
										else {
											mParty.Map = 5;
											mParty.XAxis = 14;
											mParty.YAxis = 31;

											await RefreshGame();
										}
									}
									else if (mParty.Map == 23)
									{
										mParty.Map = 5;
										mParty.XAxis = 33;
										mParty.YAxis = 14;

										await RefreshGame();
									}
									else if (mParty.Map == 24)
									{
										mParty.Map = 22;
										mParty.XAxis = 24;
										mParty.YAxis = 23;

										await RefreshGame();
									}
									else if (mParty.Map == 25)
									{
										mParty.Map = 23;
										mParty.XAxis = 24;
										mParty.YAxis = 44;

										await RefreshGame();
									}
									else if (mParty.Map == 27)
									{
										mParty.Map = 1;
										mParty.XAxis = 19;
										mParty.YAxis = 7;

										await RefreshGame();
									}
								}
								else
								{
									AppendText("");

									if (mParty.Map == 27 && mParty.YAxis < 24)
										mParty.YAxis++;
									else
										mParty.YAxis--;
								}
							}
							else if (mMenuMode == MenuMode.JoinSkeleton)
							{
								mMenuMode = MenuMode.None;

								mParty.Etc[30] |= 1;

								if (mMenuFocusID == 0)
								{
									mAnimationEvent = AnimationType.None;
									mAnimationFrame = 0;

									JoinMemberFromEnemy(18);
									await ExitCastleLore();
								}
								else if (mMenuFocusID == 1)
								{
									ShowNoThanks();
									mSpecialEvent = SpecialEventType.RefuseJoinSkeleton;
								}
							}
							else if (mMenuMode == MenuMode.BattleStart)
							{
								mMenuMode = MenuMode.None;

								var avgLuck = 0;
								mPlayerList.ForEach(delegate (Lore player)
								{
									avgLuck += player.Luck;
								});
								avgLuck /= mPlayerList.Count;

								var avgEnemyAgility = 0;
								mEncounterEnemyList.ForEach(delegate (BattleEnemyData enemy)
								{
									avgEnemyAgility += enemy.Agility;
								});
								avgEnemyAgility /= mEncounterEnemyList.Count;

								if (mMenuFocusID == 0)
								{
									var avgAgility = 0;
									foreach (var player in mPlayerList)
									{
										avgAgility += player.Agility;
									}
									avgAgility /= mPlayerList.Count;

									if (avgAgility > avgEnemyAgility)
										StartBattle(true);
									else
										StartBattle(false);
								}
								else if (mMenuFocusID == 1)
								{
									if (avgLuck > avgEnemyAgility)
									{
										mBattleTurn = BattleTurn.RunAway;
										await EndBattle();
									}
									else
										StartBattle(false);
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
								else if (mMenuFocusID == 6)
								{
									if (mBattlePlayerID == 0)
									{
										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable)
											{
												var method = 0;
												var tool = 0;
												var enemyID = 0;

												if (player.Class == 0 || player.Class == 1 || (4 <= player.Class && player.Class <= 8) || player.Class == 10)
												{
													method = 0;
													tool = 0;
												}
												else if (player.Class == 2 || player.Class == 9)
												{
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
												else if (player.Class == 3)
												{
													method = 5;
													tool = 4;

													for (var i = 0; i < mEncounterEnemyList.Count; i++)
													{
														if (!mEncounterEnemyList[i].Unconscious && !mEncounterEnemyList[i].Dead)
														{
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
									else
									{
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
							else if (mMenuMode == MenuMode.CastSpecial)
							{
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
							else if (mMenuMode == MenuMode.ApplyBattleCureSpell)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureSpell(mPlayerList[mBattlePlayerID], mMagicWhomPlayer, mMenuFocusID, mCureResult);

								mCureBattle = true;

								ShowCureResult();
							}
							else if (mMenuMode == MenuMode.ApplyBattleCureAllSpell)
							{
								mMenuMode = MenuMode.None;

								DialogText.TextHighlighters.Clear();
								DialogText.Blocks.Clear();

								CureAllSpell(mPlayerList[mBattlePlayerID], mMenuFocusID, mCureResult);

								mCureBattle = true;

								ShowCureResult();
							}
							else if (mMenuMode == MenuMode.BattleLose)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
									ShowFileMenu(MenuMode.ChooseGameOverLoadGame);
								else
									CoreApplication.Exit();
							}
							else if (mMenuMode == MenuMode.AskEnter)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									switch (mTryEnterType)
									{
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
										case EnterType.AnotherLore:
											mParty.Map = 27;
											mParty.XAxis = 14;
											mParty.YAxis = 44;

											await RefreshGame();

											break;
										case EnterType.LastDitch:
											mParty.Map = 7;
											mParty.XAxis = 36;
											mParty.YAxis = 69;

											await RefreshGame();

											foreach (var player in mPlayerList)
											{
												if (player.Name == "폴라리스")
												{
													mMapLayer[36 + mMapWidth * 40] = 44;
													break;
												}
											}

											break;
										case EnterType.GroundGate:
											if (mParty.Map == 7)
											{
												mParty.Map = 8;
												mParty.XAxis = 49;
												mParty.YAxis = 9;
											}
											else if (mParty.Map == 8)
											{
												mParty.Map = 7;
												mParty.XAxis = 49;
												mParty.YAxis = 9;
											}

											await RefreshGame();
											break;
										case EnterType.Pyramid:
											mParty.Map = 11;
											mParty.XAxis = 24;
											mParty.YAxis = 44;

											await RefreshGame();

											if (mParty.Etc[12] > 1)
											{
												mMapLayer[24 + mMapWidth * 43] = 50;
												mMapLayer[25 + mMapWidth * 43] = 50;
											}

											break;
										case EnterType.SwampGate:
											if (mParty.Map == 9)
											{
												AppendText("");

												InvokeAnimation(AnimationType.EnterSwampGate);
											}
											else if (mParty.Map == 21)
											{
												mParty.Map = 13;
												mParty.XAxis = 80;
												mParty.YAxis = 67;

												await RefreshGame();
											}

											break;
										case EnterType.ValiantPeoples:
											mParty.Map = 8;
											mParty.XAxis = 37;
											mParty.YAxis = 69;

											await RefreshGame();
											break;
										case EnterType.GaiaTerra:
											mParty.Map = 9;
											mParty.XAxis = 25;
											mParty.YAxis = 44;

											await RefreshGame();
											break;
										case EnterType.Quake:
											mParty.Map = 15;
											mParty.XAxis = 24;
											mParty.YAxis = 69;

											await RefreshGame();
											break;
										case EnterType.Wivern:
											if (mParty.Map == 2)
											{
												mParty.Map = 16;
												mParty.XAxis = 19;
												mParty.YAxis = 34;
											}
											else if (mParty.Map == 10)
											{
												mParty.Map = 16;
												mParty.XAxis = 19;
												mParty.YAxis = 9;
											}

											await RefreshGame();
											break;
										case EnterType.WaterField:
											if (mParty.Map == 3)
											{
												mParty.Map = 10;
												mParty.XAxis = 24;
												mParty.YAxis = 69;
											}
											else if (mParty.Map == 16)
											{
												mParty.Map = 10;
												mParty.XAxis = 25;
												mParty.YAxis = 7;
											}


											await RefreshGame();

											if ((mParty.Etc[37] & (1 << 3)) > 0)
												mMapLayer[39 + mMapWidth * 55] = 44;

											break;
										case EnterType.Notice:
											mParty.Map = 17;
											mParty.XAxis = 55;
											mParty.YAxis = 93;

											await RefreshGame();
											break;
										case EnterType.LockUp:
											mParty.Map = 18;
											mParty.XAxis = 24;
											mParty.YAxis = 93;

											await RefreshGame();
											break;
										case EnterType.SwampKeep:
											if (mParty.Map == 4)
											{
												mParty.Map = 21;
												mParty.XAxis = 24;
												mParty.YAxis = 44;
											}
											else if (mParty.Map == 13)
											{
												mParty.Map = 21;
												mParty.XAxis = 24;
												mParty.YAxis = 5;
											}
											else if (mParty.Map == 22) {
												mParty.Map = 21;
												mParty.XAxis = 24;
												mParty.YAxis = 19;
											}

											await RefreshGame();
											break;
										case EnterType.EvilGod:
											mParty.Map = 19;
											mParty.XAxis = 25;
											mParty.YAxis = 44;

											await RefreshGame();
											break;
										case EnterType.Muddy:
											mParty.Map = 20;
											mParty.XAxis = 24;
											mParty.YAxis = 94;

											await RefreshGame();
											break;
										case EnterType.ImperiumMinor:
											if (mParty.Map == 5)
											{
												mParty.Map = 22;
												mParty.XAxis = 24;
												mParty.YAxis = 44;

												await RefreshGame();
											}
											else if (mParty.Map == 21)
											{
												if (mParty.Etc[39] % 2 == 0 || mParty.Etc[40] % 2 == 0)
													AppendText(" 라바 게이트는 작동되지 않았다.");
												else
												{
													mEncounterEnemyList.Clear();

													if ((mParty.Etc[41] & (1 << 1)) == 0)
													{
														if ((mParty.Etc[41] & (1 << 4)) == 0)
															JoinEnemy(64);

														if ((mParty.Etc[41] & (1 << 5)) == 0)
															JoinEnemy(63);

														if (mEncounterEnemyList.Count == 0)
															mParty.Etc[41] |= 1 << 1;
														else {
															HideMap();
															DisplayEnemy();

															mBattleEvent = 26;

															StartBattle(false);
														}
													}
													else {
														mParty.Map = 22;
														mParty.XAxis = 24;
														mParty.YAxis = 5;

														await RefreshGame();
													}
												}
											}
											break;
										case EnterType.EvilConcentration:
											if ((mParty.Etc[43] & 1) == 0)
											{
												mEncounterEnemyList.Clear();
												for (var i = 0; i < 7; i++)
													JoinEnemy(53);
												mEncounterEnemyList[mRand.Next(6) + 1] = new BattleEnemyData(68, mEnemyDataList[68]);

												HideMap();
												DisplayEnemy();

												Talk("나는 이블 컨센츄레이션 성의 입구를 지키는 임무를 맡고 있는 서리 드래곤이다. 내가 지키고 있는 한 너희들은 한 발자국도 들여놓지 못할 것이다.");

												mSpecialEvent = SpecialEventType.EnterEvilConcentration;
											}
											else
											{
												mParty.Map = 23;
												mParty.XAxis = 24;
												mParty.YAxis = 44;

												await RefreshGame();
											}

											break;
										case EnterType.EvilSeal:
											mParty.Map = 12;
											mParty.XAxis = 24;
											mParty.YAxis = 69;

											await RefreshGame();

											if (mParty.Etc[13] > 1)
												mMapLayer[17 + mMapWidth * 8] = 0;
											break;
										case EnterType.LastShelter:
											mParty.Map = 24;
											mParty.XAxis = 24;
											mParty.YAxis = 44;

											if ((mParty.Etc[42] & (1 << 3)) > 0)
												mMapLayer[32 + mMapWidth * 9] = 47;

											await RefreshGame();
											break;
										case EnterType.DungeonOfEvil:
											if ((mParty.Etc[43] & (1 << 1)) == 0) {
												mEncounterEnemyList.Clear();

												for (var i = 0; i < 2; i++)
													JoinEnemy(61);

												JoinEnemy(69);

												for (var i = 0; i < 4; i++)
													JoinEnemy(61);

												HideMap();
												DisplayEnemy();

												Talk(new string[] {
													$"[color={RGB.LightMagenta}] 이 동굴에 들어 가겠다고?[/color]",
													$"[color={RGB.LightMagenta}] 하!, 우습군. 너희들에게는 여기의 드래코니안족들의 모습이 보이지 않는 모양이군.[/color]"
												});

												mSpecialEvent = SpecialEventType.EnterDungeonOfEvil;
											}
											break;
										case EnterType.ChamberOfNecromancer:
											mParty.Etc[0] = 1;
											AppendText("");
											InvokeAnimation(AnimationType.BlackKnight);
											break;

									}
								}
								else
								{
									AppendText(new string[] { "" });
								}
							}
							else if (mMenuMode == MenuMode.ChooseGoldShield)
							{
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
							else if (mMenuMode == MenuMode.SwapMember)
							{
								mMenuMode = MenuMode.None;

								mPlayerList[mMenuFocusID + 1] = mReserveMember;
								mReserveMember = null;

								LeaveMemberPosition();

								DisplayPlayerInfo();

								AppendText(new string[] { "" });
							}
							else if (mMenuMode == MenuMode.JoinPolaris)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(8);
									player.Name = "폴라리스";
									player.Class = 4;
									player.Level[1] = 3;
									player.Weapon = 4;
									player.Shield = 1;
									player.WeaPower = 10;
									player.ShiPower = 1;
									player.ArmPower = 2;
									player.AC = 3;

									JoinMember(player);

									mMapLayer[36 + mMapWidth * 40] = 44;
								}
								else
									ShowNoThanks();
							}
							else if (mMenuMode == MenuMode.JoinDraconian)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(61);
									player.Class = 4;
									player.Level[0] = 17;

									JoinMember(player);

									mParty.Etc[15] |= (1 << 1);
								}
								else
									Talk(" 다시 생각해 보니 나는 당신들과 같이 싸울 운명이 아닌 것 같소.");
							}
							else if (mMenuMode == MenuMode.ChooseOedipusSpear)
							{
								mMenuMode = MenuMode.None;

								var player = mPlayerList[mMenuFocusID];
								if (player.Class == 5)
								{
									AppendText(new string[] { "전투승은 이 무기가 필요없습니다." });
								}
								else
								{
									player.Shield = 3;
									player.ShiPower = 12;

									if (player.Class == 1)
										player.WeaPower += (int)Math.Round(player.WeaPower * 0.5);

									AppendText(new string[] { $"{player.Name}(이)가 오이디푸스의 창을 장착했다." });

									mParty.Etc[32] |= (1 << 7);
								}
							}
							else if (mMenuMode == MenuMode.JoinLoreHunter)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(38);
									player.Name = "로어 헌터";
									player.Class = 7;
									player.Weapon = 5;
									player.Shield = 2;
									player.Armor = 1;
									player.WeaPower = 15;
									player.ShiPower = 2;
									player.ArmPower = 2;
									player.AC = 4;

									JoinMember(player);

									mMapLayer[39 + mMapWidth * 55] = 44;
									mParty.Etc[37] |= (1 << 3);
								}
								else
									ShowNoThanks();
							}
							else if (mMenuMode == MenuMode.JoinRigel)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(13);
									player.Name = "리겔";
									player.Class = 7;
									player.Weapon = 4;
									player.Shield = 1;
									player.Armor = 1;
									player.WeaPower = 10;
									player.ShiPower = 1;
									player.ArmPower = 2;
									player.AC = 3;
									player.HP = 1;

									JoinMember(player);
								}
								else if (mMenuFocusID == 1)
								{
									Talk(" 일행은 그에게 치료 마법을 사용하여 상처를 모두 치료한 후 그가 이곳을 빠져나갈 수 있을 정도의 식량을 나누어 주었다." +
									" 그러자 리겔이란 그 용사는 우리의 무기에 신의 축복을 내려주고는 자신의 길을 떠났다.");

									if (mParty.Food > 4)
										mParty.Food -= 5;
									else
										mParty.Food = 0;

									mPlayerList[0].SP = 0;

									foreach (var player in mPlayerList)
									{
										if (mRand.Next(20) < player.Luck && mRand.Next(20) < player.Luck)
											player.WeaPower = (int)Math.Round(player.WeaPower * 1.2);
									}

									DisplayPlayerInfo();
								}
								else
								{
									AppendText(new string[] { "" });
								}

								mParty.Etc[30] |= 1 << 1;
								mSpecialEvent = SpecialEventType.None;
							}
							else if (mMenuMode == MenuMode.ChooseGoldShield2)
							{
								mMenuMode = MenuMode.None;

								var player = mPlayerList[mMenuFocusID];
								player.Shield = 5;
								player.ShiPower = 5;
								player.AC = player.ShiPower + player.ArmPower;

								if (player.Class == 1)
									player.AC++;

								if (player.AC > 10)
									player.AC = 10;

								AppendText(new string[] { $"[color={RGB.White}]{player.Name}(이)가 황금의 방패를 장착했다.[/color]" });

								DisplayPlayerInfo();

								mParty.Etc[35] |= 1 << 2;
							}
							else if (mMenuMode == MenuMode.ChooseGoldArmor)
							{
								mMenuMode = MenuMode.None;

								var player = mPlayerList[mMenuFocusID];
								player.Armor = 5;
								player.ArmPower = 5;
								player.AC = player.ShiPower + player.ArmPower;

								if (player.Class == 1)
									player.AC++;

								if (player.AC > 10)
									player.AC = 10;

								AppendText(new string[] { $"[color={RGB.White}]{player.Name}(이)가 황금의 갑옷을 장착했다.[/color]" });

								DisplayPlayerInfo();

								mParty.Etc[35] |= 1 << 3;
							}
							else if (mMenuMode == MenuMode.JoinRedAntares)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(54);
									player.Name = "레드 안타레스";
									player.Class = 9;
									player.Weapon = 0;
									player.Shield = 0;
									player.Armor = 0;
									player.WeaPower = 0;
									player.ShiPower = 0;
									player.ArmPower = 0;
									player.AC = 0;
									player.HP = 0;
									player.Resistance = 15;
									player.Endurance = 10;

									JoinMember(player);
								}
								else if (mMenuFocusID == 1)
									ShowNoThanks();

								mParty.Etc[37] |= 1 << 1;
							}
							else if (mMenuMode == MenuMode.JoinSpica)
							{
								mMenuMode = MenuMode.None;

								if (mMenuFocusID == 0)
								{
									var player = GetMemberFromEnemy(42);
									player.Name = "스피카";
									player.Gender = "female";
									player.Class = 3;
									player.Strength = 10;
									player.Mentality = 17;
									player.Concentration = 20;
									player.Endurance = 9;
									player.Resistance = 15;
									player.Agility = 7;
									player.Accuracy[0] = 8;
									player.Accuracy[1] = 15;
									player.Accuracy[2] = 20;
									player.Luck = 10;
									player.Poison = 0;
									player.Unconscious = 0;
									player.Dead = 0;
									player.Level[0] = 11;
									player.Level[1] = 6;
									player.Level[2] = 11;
									player.HP = player.Endurance * player.Level[0];
									player.SP = player.Mentality * player.Level[1];
									player.ESP = player.Concentration * player.Level[2];
									player.AC = 3;
									player.Weapon = 1;
									player.Shield = 1;
									player.Armor = 1;
									player.WeaPower = 5;
									player.ShiPower = 3;
									player.ArmPower = 2;

									JoinMember(player);
								}
								else if (mMenuFocusID == 1)
									AppendText(new string[] { "" });

								mParty.Etc[38] |= 1 << 1;
							}
							else if (mMenuMode == MenuMode.QnA)
							{
								mMenuMode = MenuMode.None;

								AppendText("");

								for (var x = 22; x < 25; x++)
									mMapLayer[x + mMapWidth * mParty.YAxis] = 44;

								if ((mMenuFocusID == 0 && mQuestionID > 3) || (mMenuFocusID == 1 && mQuestionID < 4))
								{
									for (var y = 48; y < 52; y++)
									{
										mMapLayer[21 + mMapWidth * y] = 25;
										mMapLayer[25 + mMapWidth * y] = 23;

										for (var x = 22; x < 25; x++)
											mMapLayer[x + mMapWidth * y] = 44;
									}
								}
								else
								{
									mParty.Map = 4;
									mParty.XAxis = 81;
									mParty.YAxis = 16;

									await RefreshGame();
								}
							}
							else if (mMenuMode == MenuMode.ReadScroll)
							{
								mMenuMode = MenuMode.None;

								Talk(new string[] {
									$"[color={RGB.White}] Durant l'estoille cheuelue apparente,[/color]",
									" 머리를 푼 별이 나타날 때",
									$"[color={RGB.White}] Les trois grand princes seront faits ennemies,[/color]",
									" 거대한 세 왕자가 서로를 적대한다",
									$"[color={RGB.White}] Frappez du ciel paix terre trembulente,[/color]",
									" 평화는 하늘에서 당하고 대지는 요동한다",
									$"[color={RGB.White}] En son haut auge de l'exaltation,[/color]",
									" 그 찬미해야 할 높은 오류 속에서",
									$"[color={RGB.White}] Neromancer sur le bord mis.[/color]",
									" 네크로만서는 해안으로 밀려나리라."
								});

								mSpecialEvent = SpecialEventType.ReadScroll;
							}
							else if (mMenuMode == MenuMode.ChooseLoadGame || mMenuMode == MenuMode.ChooseGameOverLoadGame) {
								mMenuMode = MenuMode.None;

								await LoadGame(mMenuFocusID);
							}
							else if (mMenuMode == MenuMode.ChooseSaveGame) {
								mMenuMode = MenuMode.None;

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
									MaxEnemy = mMaxEnemy,
									SaveTime = DateTime.Now.Ticks
								};

								var saveJSON = JsonConvert.SerializeObject(saveData);

								var idStr = "";
								if (mMenuFocusID > 0)
									idStr = mMenuFocusID.ToString();

								var storageFolder = ApplicationData.Current.LocalFolder;
								var saveFile = await storageFolder.CreateFileAsync($"loreSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
								await FileIO.WriteTextAsync(saveFile, saveJSON);

								var users = await User.FindAllAsync();
								var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-000063336555");

								if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
								{
									var gameSaveProvider = gameSaveTask.Value;

									var gameSaveContainer = gameSaveProvider.CreateContainer("LoreSaveContainer");

									var buffer = Encoding.UTF8.GetBytes(saveJSON);

									var writer = new DataWriter();
									writer.WriteUInt32((uint)buffer.Length);
									writer.WriteBytes(buffer);
									var dataBuffer = writer.DetachBuffer();

									var blobsToWrite = new Dictionary<string, IBuffer>();
									blobsToWrite.Add($"loreSave{idStr}", dataBuffer);

									var gameSaveOperationResult = await gameSaveContainer.SubmitUpdatesAsync(blobsToWrite, null, "LoreSave");
									if (gameSaveOperationResult.Status == GameSaveErrorStatus.Ok)
										AppendText(new string[] { $"[color={RGB.LightRed}]현재의 게임을 저장합니다.[/color]" });
									else
										AppendText(new string[] {
											$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 저장하지 못했습니다.[/color]",
											$"[color={RGB.LightRed}]에러 코드: {gameSaveOperationResult.Status}[/color]"
										});
								}
								else
								{
									AppendText(new string[] {
											$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 연결할 수 없습니다.[/color]",
											$"[color={RGB.LightRed}]에러 코드: {gameSaveTask.Status}[/color]"
										});
								}
							}
						}
					}
				}
				else if (args.VirtualKey == VirtualKey.P || args.VirtualKey == VirtualKey.GamepadView)
				{
					ShowPartyStatus();
				}
				else if (args.VirtualKey == VirtualKey.V || args.VirtualKey == VirtualKey.GamepadLeftTrigger) {
					AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
					ShowCharacterMenu(MenuMode.ViewCharacter);
				}
				else if (args.VirtualKey == VirtualKey.C || args.VirtualKey == VirtualKey.GamepadRightShoulder) {
					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					ShowCharacterMenu(MenuMode.CastSpell);
				}
				else if (args.VirtualKey == VirtualKey.E || args.VirtualKey == VirtualKey.GamepadRightTrigger) {
					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					ShowCharacterMenu(MenuMode.Extrasense);
				}
				else if (args.VirtualKey == VirtualKey.R || args.VirtualKey == VirtualKey.GamepadLeftShoulder)
				{
					// 휴식 단축키
					Rest();
				}
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		private void StartBattle(bool assualt = true)
		{
			mParty.Etc[5] = 1;

			DialogText.TextHighlighters.Clear();
			DialogText.Blocks.Clear();

			if (assualt)
			{
				mBattleTurn = BattleTurn.Player;

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
			else
			{
				mBattleTurn = BattleTurn.Player;
				ExecuteBattle();
			}
		}

		private async Task RefreshGame()
		{
			AppendText(new string[] { "" });
			await LoadMapData();
			InitializeMap();
		}

		private void ExecuteBattle()
		{
			void CheckBattleStatus()
			{
				var allPlayerDead = true;
				foreach (var player in mPlayerList)
				{
					if (player.IsAvailable)
					{
						allPlayerDead = false;
						break;
					}
				}

				if (allPlayerDead)
				{
					mBattleTurn = BattleTurn.Lose;
					mParty.Etc[5] = 0;
				}
				else
				{
					var allEnemyDead = true;

					foreach (var enemy in mEncounterEnemyList)
					{
						if (!enemy.Dead)
						{
							allEnemyDead = false;
							break;
						}
					}

					if (allEnemyDead)
					{
						mBattleTurn = BattleTurn.Win;
						mParty.Etc[5] = 0;
					}
				}
			}

			void ShowBattleResult(List<string> battleResult)
			{
				var lineHeight = 0d;
				if (DialogText.Blocks.Count > 0)
				{
					var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
					lineHeight = startRect.Height;
				}

				var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

				var append = false;
				if (lineCount + battleResult.Count + 1 <= DIALOG_MAX_LINES)
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


			if (mBattleCommandQueue.Count == 0 && mBatteEnemyQueue.Count == 0)
			{
				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();

				switch (mBattleTurn)
				{
					case BattleTurn.Player:
						mBattleTurn = BattleTurn.Enemy;
						break;
					case BattleTurn.Enemy:
						mBattleTurn = BattleTurn.Player;
						break;
				}

				switch (mBattleTurn)
				{
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
						foreach (var enemy in mEncounterEnemyList)
						{
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
							battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 {enemy.Name}에게 '{Common.GetMagicStr(battleCommand.Tool + 41)}'{Common.GetMagicMokjukStr(battleCommand.Tool + 40)} 사용했다[/color]");
							break;
						case 6:
							battleResult.Add($"[color={RGB.White}]일행은 도망을 시도했다[/color]");
							break;
						default:
							battleResult.Add($"[color={RGB.White}]{battleCommand.Player.Name}(은)는 잠시 주저했다[/color]");
							break;
					}
				}

				void PlusExperience(BattleEnemyData enemy)
				{
#if DEBUG
					var exp = 50000;
#else
							var exp = (enemy.ENumber + 1) * (enemy.ENumber + 1) * (enemy.ENumber + 1) / 8;
							if (exp == 0)
								exp = 1;
#endif

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

						PlusExperience(enemy);
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
						PlusExperience(enemy);
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

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

#if DEBUG
					var magicPoint = 1;
#else
					var magicPoint = (int)Math.Round((double)battleCommand.Player.Level[1] * battleCommand.Tool * battleCommand.Tool / 2);
					if (battleCommand.Player.SP < magicPoint)
					{
						battleResult.Add($"마법 지수가 부족했다");
						return;
					}
#endif
					battleCommand.Player.SP -= magicPoint;
					DisplaySP();

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
						battleResult.Add($"그러나, 적은 {battleCommand.Player.GenderPronoun}의 마법 공격을 막았다");
						return;
					}

					enemy.HP -= magicPoint;
					if (enemy.HP <= 0)
					{
						battleResult.Add($"[color={RGB.LightRed}]적은 {battleCommand.Player.GenderPronoun}의 마법에 의해 의식불능이 되었다[/color]");
						PlusExperience(enemy);

						enemy.HP = 0;
						enemy.Unconscious = true;
					}
					else
					{
						battleResult.Add($"적은 [color={RGB.White}]{magicPoint}[/color]만큼의 피해를 입었다");
					}
				}

				void CastSpecialMagic()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

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
						DisplaySP();

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
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"기술 무력화 공격은 저지 당했다");
							return;
						}

						if (mRand.Next(60) > battleCommand.Player.Accuracy[1])
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
						DisplaySP();

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

						if (mRand.Next(resistancePoint) > battleCommand.Player.Accuracy[1])
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
						DisplaySP();

						if (mRand.Next(200) < enemy.Resistance)
						{
							battleResult.Add($"능력 저하 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(30) > battleCommand.Player.Accuracy[1])
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
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"마법 불능 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy[1])
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
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"탈 초인화 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy[1])
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

				void CastESP()
				{
					if (battleCommand.Player.Class != 2 && battleCommand.Player.Class != 3 && battleCommand.Player.Class != 6 && (mParty.Etc[38] & 1) == 0)
					{
						battleResult.Add($"당신에게는 아직 능력이 없다.");
						return;
					}

					if (battleCommand.Tool == 0 || battleCommand.Tool == 1 || battleCommand.Tool == 3)
					{
						battleResult.Add($"{Common.GetMagicStr(battleCommand.Tool + 41)}(은)는 전투모드에서는 사용할 수가 없습니다.");
						return;
					}

					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					if (battleCommand.Tool == 2)
					{
						if (battleCommand.Player.ESP < 15)
						{
							battleResult.Add($"초감각 지수가 부족했다.");
							return;
						}

						battleCommand.Player.ESP -= 15;
						DisplayESP();

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
						JoinMemberFromEnemy(enemy.ENumber);
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
						DisplayESP();

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
								PlusExperience(enemy);
							}
							else if (impactPoint == 0 && !enemy.Unconscious)
							{
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
						}
						else if (7 <= espType && espType <= 10)
						{
							if (espType == 7 || espType == 8)
								battleResult.Add($"갑자기 땅속의 우라늄이 핵분열을 일으켜 고온의 열기가 적의 주위를 감싸기 시작한다");
							else
								battleResult.Add($"공기 중의 수소가 돌연히 핵융합을 일으켜 질량 결손의 에너지를 적들에게 방출하기 시작한다");

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
									PlusExperience(enemyOne);
								}
								else if (impactPoint == 0 && !enemyOne.Unconscious)
								{
									enemyOne.Unconscious = true;
									PlusExperience(enemyOne);
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
							battleResult.Add($"{enemy.Name}(은)는 겁을 먹고는 도망가 버렸다");
						}
						else if (13 <= espType && espType <= 14)
						{
							battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적의 신진대사를 조절하여 적의 체력을 점차 약화 시키려 한다");

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
						if (!mEncounterEnemyList[i].Dead)
						{
							battleCommand.EnemyID = i;
							CastOne();
						}
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
						battleResult.Add($"[color={RGB.LightCyan}]성공적으로 도망을 갔다[/color]");

						mParty.Etc[5] = 2;
					}
				}

				ShowBattleResult(battleResult);
			}
			else
			{
				BattleEnemyData enemy = null;

				do
				{
					if (mBatteEnemyQueue.Count == 0)
						break;

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
										battleResult.Add($"[color={RGB.Red}]{player.Name}(은)는 죽었다 !![/color]");

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

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 중독 되었다 !![/color]");

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

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 의식불명이 되었다 !![/color]");

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

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 죽었다 !![/color]");

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

						if (mRand.Next(enemy.Accuracy[0] * 1000) > mRand.Next(enemy.Accuracy[1] * 1000) && enemy.Strength > 0 || enemy.CastLevel == 0)
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

		private void CureSpell(Lore player, Lore whomPlayer, int magic, List<string> cureResult = null)
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

		private void CureAllSpell(Lore player, int magic, List<string> cureResult = null)
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

		private void BattleMode()
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

		private void GoWeaponShop()
		{
			AppendText(new string[] {
						$"[color={RGB.White}]여기는 무기상점입니다.[/color]",
						$"[color={RGB.White}]우리들은 무기, 방패, 갑옷을 팔고있습니다.[/color]",
						$"[color={RGB.White}]어떤 종류를 원하십니까?[/color]"
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
						$"[color={RGB.White}]누가 치료를 받겠습니까?[/color]"
					});

			ShowCharacterMenu(MenuMode.Hospital);
		}
		private void GoTrainingCenter()
		{
			AppendText(new string[] {
						$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
						$"[color={RGB.White}] 만약 당신이 충분한 전투 경험을 쌓았다면, 당신은 더욱 능숙하게 무기를 다룰 것입니다.[/color]",
						"",
						$"[color={RGB.White}]누가 훈련을 받겠습니까?[/color]"
					});

			ShowCharacterMenu(MenuMode.TrainingCenter);
		}

		private void ShowPartyStatus() {
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

		private void Rest()
		{
			var append = false;
			mPlayerList.ForEach(delegate (Lore player)
			{
				if (mParty.Food <= 0)
					AppendText(new string[] { $"[color={RGB.Red}]일행은 식량이 바닥났다[/color]" }, append);
				else
				{
					if (player.Dead > 0)
						AppendText(new string[] { $"{player.Name}(은)는 죽었다" }, append);
					else if (player.Unconscious > 0 && player.Poison == 0)
					{
						player.Unconscious = player.Unconscious - player.Level[0] - player.Level[1] - player.Level[2];
						if (player.Unconscious <= 0)
						{
							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 의식이 회복되었다[/color]" }, append);
							player.Unconscious = 0;
							if (player.HP <= 0)
								player.HP = 1;

#if DEBUG
							//mParty.Food--;
#else
							mParty.Food--;
#endif

						}
						else
							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 여전히 의식 불명이다[/color]" }, append);
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
#if DEBUG
								//mParty.Food++;
#else
								mParty.Food++;
#endif
							}
						}

						player.HP += recoverPoint;

						if (player.HP >= player.Endurance * player.Level[0])
						{
							player.HP = player.Endurance * player.Level[0];

							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 모든 건강이 회복되었다[/color]" }, append);
						}
						else
							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 치료되었다[/color]" }, append);

#if DEBUG
						//mParty.Food--;
#else
						mParty.Food--;
#endif
					}

					if (append == false)
						append = true;
				}
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

		private void ShowEnterMenu(EnterType enterType)
		{
			mTryEnterType = enterType;

			AppendText(new string[] { $"{mEnterTypeMap[enterType]}에 들어가기를 원합니까?" });

			ShowMenu(MenuMode.AskEnter, new string[] {
				"예, 그렇습니다.",
				"아니오, 원하지 않습니다."
			});
		}

		private async void InvokeSpecialEvent(int prevX, int prevY) {
			void FindGold(int gold) {
				AppendText(new string[] { $"당신은 금화 {gold}개를 발견했다." });

				mParty.Gold += gold;
			}

			if (mParty.Map == 1)
			{
				if ((mParty.Etc[31] & (1 << 7)) == 0)
				{
					Talk("일행들은 100 인분의 식량을 발견했다.");

					if (mParty.Food > 155)
						mParty.Food = 255;
					else
						mParty.Food += 100;

					mParty.Etc[31] |= 1 << 7;
				}
				else
					Talk("우리들은 아무것도 발견할수 없었다.");


				mParty.XAxis = prevX;
				mParty.YAxis = prevY;
			}
			else if (mParty.Map == 4)
			{
				if (mParty.XAxis == 39 && mParty.YAxis == 17)
				{
					AppendText(new string[] { " 일행은 공간 이동이 되었다" });

					mParty.XAxis = 45;
					mParty.YAxis = 40;
				}
				else if (mParty.XAxis == 25 && mParty.YAxis == 15)
				{
					Talk(" 여기는 인간과 드래곤의 중간 종족이며 혼란스런 세상을 피해 은둔하고 있는 드래코니안이란자가 살고 있는 피라미드였다.");
					mSpecialEvent = SpecialEventType.MeetDraconian;
				}
				else if (mParty.XAxis == 19 && mParty.YAxis == 38)
				{
					if ((mParty.Etc[15] & 1) > 0)
					{
						mParty.XAxis = 45;
						mParty.YAxis = 40;

						Talk(" 이제는 더 이상 할말이 없소.");
					}
					else
					{
						Talk(new string[] {
							" 나는 에인션트 이블이란 존재이오. 이제 나는 육신은 없는 영이오. 당신은 로드 안을 만나보았겠군요. 그리고 우리들의 운명적인 만남도 역시 예시 받았겠군요.",
							" 사실이야 어떻든 당신에게 이 대륙에서의 할 일을 말해 주겠소."
						});

						mSpecialEvent = SpecialEventType.MeetAncientEvil;
					}
				}
			}
			else if (mParty.Map == 6)
			{
				if (mParty.XAxis == 61 && mParty.YAxis == 81)
				{
					AppendText(new string[] { "일행은 상자 속에서 약간의 금을 발견했다." });
					mParty.Gold += 1000;
					mMapLayer[61 + mMapWidth * 81] = 44;
				}
				else if ((mParty.XAxis == 50 && mParty.YAxis == 11) || (mParty.XAxis == 51 && mParty.YAxis == 11))
				{
					if ((mParty.Etc[49] & (1 << 1)) > 0)
					{
						var enemyNumber = 0;

						if ((mParty.Etc[49] & (1 << 2)) > 0)
						{
							Talk(new string[] { $" 다시 돌아오다니, {mPlayerList[0].Name}",
								" 이번에는 기어이 네놈들을 해치우고야 말겠다. 나의 친구들도 이번에 거들 것이다."
							});

							enemyNumber = 7;
						}
						else
						{
							Talk(new string[] { $" 아니! 당신이 우리들을 배신하고 죄수를 풀어주다니... 그렇다면 우리들은 결투로서 당신들과 승부할 수밖에 없군요."
							});

							mParty.Etc[49] |= 1 << 2;

							enemyNumber = 2;
						}

						mEncounterEnemyList.Clear();
						for (var i = 0; i < enemyNumber; i++)
						{
							var enemy = JoinEnemy(25);
							enemy.Name = $"병사{i + 1}";
							enemy.Special = 0;
							enemy.CastLevel = 0;
							enemy.ENumber = 0;
						}

						HideMap();
						DisplayEnemy();

						mSpecialEvent = SpecialEventType.BattlePrison;
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

						AppendText(new string[] { $" 일행은 가장 기본적인 무기로 모두 무장을 하였다." });

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
			else if (mParty.Map == 7)
			{
				if (mParty.XAxis == 49)
				{
					ShowEnterMenu(EnterType.GroundGate);
				}
				else if (mParty.XAxis == 29 || mParty.XAxis == 31)
				{
					mMapLayer[30 + mMapWidth * mParty.YAxis] = 45;
				}
				else if (mParty.YAxis == 70)
				{
					ShowExitMenu();
				}
			}
			else if (mParty.Map == 8)
			{
				if (mParty.XAxis == 49)
				{
					ShowEnterMenu(EnterType.GroundGate);
				}
				else if (mParty.YAxis == 70)
					ShowExitMenu();
			}
			else if (mParty.Map == 9)
			{
				if ((mParty.XAxis == 9 && mParty.YAxis == 24) && (mParty.Etc[34] & 1) == 0)
				{
					FindGold(5000);
					mParty.Etc[34] |= 1;
				}
				else if ((mParty.XAxis == 11 && mParty.YAxis == 25) && (mParty.Etc[34] & (1 << 1)) == 0)
				{
					FindGold(5000);
					mParty.Etc[34] |= (1 << 1);
				}
				else if ((mParty.XAxis == 14 && mParty.YAxis == 24) && (mParty.Etc[34] & (1 << 2)) == 0)
				{
					FindGold(5000);
					mParty.Etc[34] |= (1 << 2);
				}
				else if ((mParty.XAxis == 15 && mParty.YAxis == 22) && (mParty.Etc[34] & (1 << 3)) == 0)
				{
					FindGold(5000);
					mParty.Etc[34] |= (1 << 3);
				}
				else if ((mParty.XAxis == 17 && mParty.YAxis == 26) && (mParty.Etc[34] & (1 << 4)) == 0)
				{
					FindGold(5000);
					mParty.Etc[34] |= (1 << 4);
				}
				else if (mParty.YAxis == 9 && mParty.Etc[14] < 5)
				{
					AppendText(new string[] { "알 수 없는 힘이 당신을 배척합니다." });
					mParty.YAxis++;
				}
				else if (mParty.YAxis == 4)
				{
					ShowEnterMenu(EnterType.SwampGate);
				}
				else if (mParty.YAxis == 45)
					ShowExitMenu();
			}
			else if (mParty.Map == 10)
			{
				if (mParty.YAxis == 45)
					mParty.YAxis = 49;
				else if (mParty.YAxis == 48)
					mParty.YAxis = 44;
				else if (mParty.YAxis == 70)
					ShowExitMenu();
			}
			else if (mParty.Map == 11)
			{
				if (mParty.XAxis == 19 && mParty.YAxis == 29 && (mParty.Etc[32] & 1) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= 1;
				}
				else if (mParty.XAxis == 17 && mParty.YAxis == 35 && (mParty.Etc[32] & (1 << 1)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 1);
				}
				else if (mParty.XAxis == 34 && mParty.YAxis == 31 && (mParty.Etc[32] & (1 << 2)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 2);
				}
				else if (mParty.XAxis == 32 && mParty.YAxis == 35 && (mParty.Etc[32] & (1 << 3)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 3);
				}
				else if (mParty.XAxis == 34 && mParty.YAxis == 13 && (mParty.Etc[32] & (1 << 4)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 4);
				}
				else if (mParty.XAxis == 13 && mParty.YAxis == 15 && (mParty.Etc[32] & (1 << 5)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 5);
				}
				else if (mParty.XAxis == 36 && mParty.YAxis == 11 && (mParty.Etc[32] & (1 << 6)) == 0)
				{
					FindGold(5000);
					mParty.Etc[32] |= (1 << 6);
				}
				else if (mParty.YAxis == 43 && (mParty.Etc[32] & (1 << 7)) == 0)
				{
					Talk("당신은 어떤 창을 발견했다.");
					mSpecialEvent = SpecialEventType.FindOedipusSpear;
				}
				else if (mParty.YAxis == 45)
				{
					ShowExitMenu();
				}
				else if (mParty.YAxis == 23 && mParty.Etc[12] == 1)
				{
					Talk($"[color={RGB.LightCyan}]당신은 미이라의 방을 발견했다.[/color]");

					mSpecialEvent = SpecialEventType.MajorMummyRoom;
				}
			}
			else if (mParty.Map == 12)
			{
				if (mParty.YAxis == 70)
					ShowExitMenu();
				else if (mParty.YAxis == 49 && (mParty.YAxis - prevY) != 1)
				{
					if (mParty.XAxis == 32)
					{
						AppendText(new string[] { "여기는 옳은 문이었다." });
						mMapLayer[32 + mMapWidth * 48] = 0;
					}
					else
					{
						AppendText(new string[] { "당신은 바보군요, 다시 생각하십시오." });
						mParty.XAxis = 24;
						mParty.YAxis = 69;
					}
				}
				else if (mParty.YAxis == 9 && mParty.Etc[13] < 2)
				{
					if (mParty.XAxis == 17)
					{
						mMapLayer[17 + mMapWidth * 8] = 0;
						mParty.Etc[13] = 2;

						Talk(new string[] {
						$"[color={RGB.Yellow}]당신은 황금의 봉인을 찾았다 !![/color]",
						$"[color={RGB.White}]그러므로 당신의 임무는 성공했다.[/color]",
						$"[color={RGB.White}]이제는 가이아 테라로 돌아가라.[/color]"
						});
					}
					else
					{
						for (var y = 9; y < 23; y++)
							mMapLayer[mParty.XAxis + mMapWidth * y] = 49;
					}
				}
				else if (mParty.XAxis == 11 && mParty.YAxis == 47 && (mParty.Etc[30] & (1 << 1)) == 0)
				{
					Talk(" 일행들은 심한 부상 때문에 거의 몸을 가누지 못하는 한 남자와 마주쳤다.");

					mSpecialEvent = SpecialEventType.MeetRigel;
				}
				else if (mParty.Etc[3] == 0 && !(mParty.XAxis == 11 && mParty.YAxis == 47))
				{
					AppendText("일행들은 절벽으로 떨어질뻔 했다.");

					mParty.XAxis = prevX;
					mParty.YAxis = prevY;
				}
			}
			else if (mParty.Map == 13) {
				if (mParty.YAxis == 95)
				{
					ShowExitMenu();
				}
				else if (75 <= mParty.XAxis && mParty.YAxis <= 85 && 70 <= mParty.YAxis && mParty.YAxis <= 80) {
					AppendText($"[color={RGB.White}]알 수 없는 힘이 당신을 당기는걸 느꼈다[/color]");

					for (var y = 70; y < 81; y++) {
						for (var x = 75; x < 86; x++) {
							if (mMapLayer[x + mMapWidth * y] == 52)
								mMapLayer[x + mMapWidth * y] = 44;
							else if (mMapLayer[x + mMapWidth * y] == 40 || mMapLayer[x + mMapWidth * y] == 51)
								mMapLayer[x + mMapWidth * y] = 42;
						}
					}

					InvokeAnimation(AnimationType.SwampGatePyramid);
				}
				else if (mParty.YAxis == 67 && (mParty.Etc[37] & (1 << 4)) == 0) {
					mEncounterEnemyList.Clear();
					for (var i = 49; i < 52; i++) {
						var enemy = JoinEnemy(i);
						enemy.ENumber = 0;
					}

					HideMap();
					DisplayEnemy();

					Talk($"[color={RGB.LightMagenta}]우리들의 영역을 침범하는 자는 가만두지 않겠다!!![/color]");

					mSpecialEvent = SpecialEventType.BattleExitSwampGate;
				}
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
						$"여기가 '메너스'의 중심이다.",
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
						$"[color={RGB.LightCyan}]누가 이 황금의 방패를 장착 하겠습니까?[/color]"
					});

					ShowCharacterMenu(MenuMode.ChooseGoldShield);
				}
			}
			else if (mParty.Map == 15)
			{
				if (mParty.YAxis == 70)
					ShowExitMenu();
				else if (mParty.YAxis == 17 && (mParty.XAxis == 9 || mParty.XAxis == 10 || mParty.XAxis == 39 || mParty.XAxis == 40))
				{
					if ((mParty.Etc[35] & (1 << 1)) == 0)
					{
						if ((mParty.Etc[35] & 1) == 0)
						{
							mParty.Etc[35] |= 1;
							FindGold(6000);
						}
						else
						{
							mParty.Etc[35] |= 1 << 1;
							FindGold(4000);
						}

						mMapLayer[mParty.XAxis + mMapWidth * 47] = 44;
						mMapLayer[mParty.XAxis + mMapWidth * 46] = 44;
					}
				}
				else if (mParty.XAxis == 13 && mParty.YAxis == 6 && (mParty.Etc[36] & (1 << 2)) == 0)
				{
					Talk($"[color={RGB.White}]당신은 황금의 방패를 발견했다.[/color]");

					mSpecialEvent = SpecialEventType.FindGoldenShield;
				}
				else if (mParty.XAxis == 44 && mParty.YAxis == 18 && (mParty.Etc[36] & (1 << 3)) == 0)
				{
					Talk($"[color={RGB.White}]당신은 황금의 갑옷을 발견했다.[/color]");

					mSpecialEvent = SpecialEventType.FindGoldenArmor;
				}
				else if (mParty.YAxis == 26 && mParty.Etc[13] == 4)
				{
					Talk($"[color={RGB.LightCyan}]당신은 아키가고일과 두 마리의 좀비를 발견했다.[/color]");

					mSpecialEvent = SpecialEventType.BattleArchiGagoyle;
				}
			}
			else if (mParty.Map == 16)
			{
				if (mParty.YAxis == 35)
					ShowExitMenu();
				else if (mParty.YAxis == 9)
				{
					if (mParty.Etc[36] < 3)
					{
						string wivernCountStr;
						switch (mParty.Etc[36])
						{
							case 2:
								wivernCountStr = "한";
								break;
							case 1:
								wivernCountStr = "두";
								break;
							default:
								wivernCountStr = "세";
								break;
						}

						Talk($"[color={RGB.LightCyan}]당신은 {wivernCountStr} 마리의 와이번과 마주쳤다.[/color]");
						mSpecialEvent = SpecialEventType.BattleWivern;
					}
					else
						AppendText(new string[] { $"[color={RGB.White}]여기에는 와이번의 시체만이 있다.[/color]" });
				}
			}
			else if (mParty.Map == 17)
			{
				if (mParty.YAxis == 94)
					ShowExitMenu();
				else if (mParty.YAxis == 79)
					mParty.YAxis = 5;
				else if (mParty.YAxis == 43)
				{
					for (var x = 66; x < 69; x++)
					{
						mMapLayer[x + mMapWidth * 43] = 44;
						mMapLayer[x + mMapWidth * 37] = 52;
					}
				}
				else if (mParty.XAxis == 74 && mParty.YAxis == 51)
				{
					if ((mParty.Etc[37] & (1 << 1)) == 1)
						return;
					else if ((mParty.Etc[37] & 1) == 1 && mParty.Etc[4] > 0)
					{
						AppendText(new string[] {
							" 다시 생각해 보니 나도 직접 네크로만서에 도전하고픈 마음이 생겼소. 비록 육체적인 힘은 전혀 없는 영이지만 당신들과 같이 모험을 하고 싶소." +
							" 당신들의 생각은 어떻소."
						});

						ShowMenu(MenuMode.JoinRedAntares, new string[] {
							"당신의 제의을 받아 들이겠소",
							"당신이 전해준 마법만으로도 족하오"
						});
					}
					else if ((mParty.Etc[37] & 1) == 1 && (mParty.Etc[37] & (1 << 1)) == 0)
						Talk("나는 다시 영혼의 세계로 돌아가야 겠소.");
					else if ((mParty.Etc[37] & 1) == 0)
					{
						for (var y = 46; y < 57; y++)
						{
							for (var x = 70; x < 82; x++)
							{
								if (mMapLayer[x + mMapWidth * y] == 40)
									mMapLayer[x + mMapWidth * y] = 50;
							}

							Talk("갑자기 주위가 용암으로 변하면서 한 영혼이 당신 앞에 나타났다.");
							mSpecialEvent = SpecialEventType.MeetRedAntares;
						}
					}
				}
				else if (mParty.XAxis == 71)
				{
					for (var y = 18; y < 21; y++)
						mMapLayer[71 + mMapWidth * y] = 44;
					mParty.YAxis -= 7;
				}
				else if (mParty.YAxis == 37)
				{
					for (var x = 67; x < 69; x++)
					{
						mMapLayer[x + mMapWidth * 37] = 44;
						mMapLayer[x + mMapWidth * 43] = 52;
					}

					mParty.XAxis = 55;
					mParty.YAxis = 92;
				}
				else if (mParty.XAxis == 21 && mParty.Etc[14] < 2)
				{
					if (mParty.Etc[0] == 0)
					{
						mParty.Etc[0] = 1;
						ShowMap();
					}

					AppendText(new string[] { $"[color={RGB.LightCyan}]당신은 보스인 히드라를 만났다.[/color]" });

					InvokeAnimation(AnimationType.Hydra);
				}
			}
			else if (mParty.Map == 18)
			{
				if (mParty.YAxis == 94)
					ShowExitMenu();
				else if (mParty.XAxis == 21 && mParty.YAxis == 40)
				{
					mMapLayer[21 + mMapWidth * 40] = 44;
					mMapLayer[20 + mMapWidth * 40] = 52;
				}
				else if (mParty.XAxis == 20 && mParty.YAxis == 40) { 
					if ((mParty.Etc[38] & (1 << 2)) == 0)
					{
						if (mParty.Etc[0] == 0)
						{
							mParty.Etc[0] = 1;
							ShowMap();
						}

						AppendText($"[color={RGB.LightCyan}]미로속에서 소를 닮은 괴물이 나타났다[/color]");

						InvokeAnimation(AnimationType.Minotaur);
					}
				}
				else if (mParty.XAxis == 36 && mParty.YAxis == 30)
				{
					if ((mParty.Etc[38] & (1 << 1)) > 0)
						return;
					else if ((mParty.Etc[38] & 1) > 0)
					{
						if (mParty.Etc[4] > 0)
						{
							var espLevel = 0;
							foreach (var player in mPlayerList)
							{
								if (player.Level[2] > espLevel)
									espLevel = player.Level[2];
							}

							if (espLevel < 5)
								Talk(" 당신이 나의 마음을 읽으려 하지만 아직 당신의 능력으로는 나의 마음을 끌어낼 수는 없습니다.");
							else
							{
								AppendText(new string[] { " 갑자기 네크로만서에게 대항 하고픈 결의가 생기는군요. 나도 당신들을 도와 그를 무찌르겠습니다." });

								ShowMenu(MenuMode.JoinSpica, new string[] {
									"저도 원했던 바입니다",
									"말씀은 고맙지만 사양하겠습니다"
								});
							}
						}
						else
						{
							Talk(" 지체할 시간이 없습니다. 신속히 행동을 취하십시오.");
						}
					}
					else if ((mParty.Etc[38] & 1) == 0)
					{
						Talk($"[color={RGB.LightCyan}]여기에는 어떤 여자가 수도하고 있었다[/color]");
						mSpecialEvent = SpecialEventType.MeetSpica;
					}
				}
				else if (mParty.XAxis == 30 && mParty.Etc[14] < 4)
				{
					AppendText(new string[] { "당신은 여기가 거대 드래곤의 거처임을 느꼈다" });

					InvokeAnimation(AnimationType.HugeDragon);
				}
			}
			else if (mParty.Map == 19)
			{
				if (mParty.YAxis == 45)
					ShowExitMenu();
				else if (mParty.XAxis == 10 && mParty.YAxis == 39)
				{
					if (mParty.Etc[2] > 0)
						Talk(" 늪 아래를 보니 무언가 반짝이는 물체가 있었다. 하지만 늪 위를 걷는 마법 때문에 늪 속으로 들어갈수가 없다.");
					else
					{
						Talk(" 일행은 독을 무릅쓰고 늪 속에 빠져있는 레버를 당겼다. 순간 동굴 중심부에서 굉음이 들렸다.");

						mMapLayer[10 + mMapWidth * 39] = 49;
						mMapLayer[40 + mMapWidth * 38] = 0;
					}
				}
				else if (mParty.XAxis == 40 && mParty.YAxis == 38)
				{
					if (mParty.Etc[2] > 0)
						Talk(" 늪 아래를 보니 무언가 반짝이는 물체가 있었다. 하지만 늪 위를 걷는 마법 때문에 늪 속으로 들어갈수가 없다.");
					else {
						Talk(" 일행은 독을 무릅쓰고 늪 속에 빠져있는 레버를 당겼다. 순간 동굴 중심부에서 조금 전 보다 더 큰 굉음이 들렸다.");

						mMapLayer[40 + mMapWidth * 38] = 49;

						if (mParty.Etc[39] % 2 == 0) {
							for (var y = 26; y < 36; y++) {
								mMapLayer[23 + mMapWidth * y] = 25;
								mMapLayer[27 + mMapWidth * y] = 23;
							}

							mMapLayer[23 + mMapWidth * 36] = 17;
							mMapLayer[27 + mMapWidth * 36] = 19;

							for (var y = 26; y < 37; y++) {
								for (var x = 24; x < 27; x++)
									mMapLayer[x + mMapWidth * y] = 44;
							}

							mParty.Etc[39] = (mRand.Next(7) + 1) << 1;
						}
					}
				}
				else if (7 <= mParty.YAxis && mParty.YAxis <= 11 && mParty.Etc[39] % 2 == 0) {
					mEncounterEnemyList.Clear();
					for (var i = 0; i < mRand.Next(3) + 3; i++)
					{
						var enemy = JoinEnemy(58);
						enemy.HP = 210;
						enemy.Level = 7;
						enemy.ENumber = 24;
					}

					mBattleEvent = 9;

					HideMap();
					DisplayEnemy();

					StartBattle(false);
				}
				else if (mParty.YAxis == 5 && mParty.Etc[39] % 2 == 0) {
					mMapLayer[mParty.XAxis + mMapWidth * (mParty.YAxis - 1)] = 49;

					var foundEnemy = mParty.Etc[39] >> 1;
					if (foundEnemy != (mParty.XAxis - 9) / 4) {
						Talk("여기에는 봉인이 발견되지 않았다");
						mMapLayer[mParty.XAxis + mMapWidth * (mParty.YAxis)] = 49;
					}
					else {
						for (var i = 0; i < 3; i++)
							JoinEnemy(58);
						for (var i = 0; i < 4; i++) {
							var enemy = JoinEnemy(58);
							enemy.ENumber = 24;
							enemy.HP = 210;
							enemy.Level = 7;
						}

						HideMap();
						DisplayEnemy();

						Talk("나는 이블 갓의 봉인을 지키고 있는 크랩 갓의 왕이다. 크랩 갓 족의 명예를 걸고 절대로 너희 같은 자들에게 봉인을 넘겨주지 않겠다!!");
						mSpecialEvent = SpecialEventType.BattleCrabGod;
					}
				}
			}
			else if (mParty.Map == 20) {
				if (mParty.YAxis == 95)
					ShowExitMenu();
				else if (mParty.YAxis == 87)
				{
					if (mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] == 0)
						mParty.YAxis = 79;
					else
					{
						mParty.Map = 4;
						mParty.XAxis = 81;
						mParty.YAxis = 16;

						await RefreshGame();
					}
				}
				else if (mParty.YAxis == 70)
				{
					if (mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] == 0)
						mParty.YAxis = 62;
					else
					{
						mParty.Map = 4;
						mParty.XAxis = 81;
						mParty.YAxis = 16;

						await RefreshGame();
					}
				}
				else if (mParty.YAxis == 90)
				{
					AppendText(new string[] {
						$" [color={RGB.White}]다음 물음이 맞다면 왼쪽길로, 아니면 오른쪽길로 가시오.[/color]",
						""
					});

					var question = mRand.Next(8);
					switch (question)
					{
						case 0:
							AppendText(new string[] { "문> CONFIG.SYS가 없으면 부팅이 안된다" }, true);
							break;
						case 1:
							AppendText(new string[] { "문> Quick-BASIC은 인터프리터어 이다" }, true);
							break;
						case 2:
							AppendText(new string[] { "문> Super VGA는 호환이 잘된다" }, true);
							break;
						case 3:
							AppendText(new string[] { "문> 8-bit APPLE의 CPU는 Z - 80 이다" }, true);
							break;
						case 4:
							AppendText(new string[] { "문> COMMAND.COM 안에 도스 명령이 들어있다" }, true);
							break;
						case 5:
							AppendText(new string[] { "문> AdLib 카드는 9 채널이다" }, true);
							break;
						case 6:
							AppendText(new string[] { "문> Ultima의 제작자는 리차드 게리오트이다" }, true);
							break;
						case 7:
							AppendText(new string[] { "문> 당신의 컴퓨터는 IBM 계열이다" }, true);
							break;
					}

					for (var x = 22; x < 26; x++)
						mMapLayer[x + mMapWidth * mParty.YAxis] = 44;

					if (question < 4)
					{
						mMapLayer[7 + mMapWidth * 87] = 52;
						mMapLayer[42 + mMapWidth * 87] = 0;
					}
					else
					{
						mMapLayer[7 + mMapWidth * 87] = 0;
						mMapLayer[42 + mMapWidth * 87] = 52;
					}

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (mParty.YAxis == 74)
				{
					AppendText(new string[] {
						$" [color={RGB.White}]다음 물음이 맞다면 왼쪽길로, 아니면 오른쪽길로 가시오.[/color]",
						""
					});

					var question = mRand.Next(8);
					switch (question)
					{
						case 0:
							AppendText(new string[] { "문> 태양계의 제 4 혹성은 지구이다" }, true);
							break;
						case 1:
							AppendText(new string[] { "문> 북극성이 가장 밝은 별이다" }, true);
							break;
						case 2:
							AppendText(new string[] { "문> 1월의 수호성좌는 1월에 볼수있다" }, true);
							break;
						case 3:
							AppendText(new string[] { "문> 빛보다 빠른 입자는 실험상 없었다" }, true);
							break;
						case 4:
							AppendText(new string[] { "문> 달이 지구보다 먼저 생겨났다" }, true);
							break;
						case 5:
							AppendText(new string[] { "문> 시그너스 X1은 블랙홀이다" }, true);
							break;
						case 6:
							AppendText(new string[] { "문> 과거로의 타임머신은 불가능하다" }, true);
							break;
						case 7:
							AppendText(new string[] { "문> 북극성은 주기적으로 달라진다" }, true);
							break;
					}

					for (var x = 22; x < 26; x++)
						mMapLayer[x + mMapWidth * mParty.YAxis] = 44;

					if (question < 4)
					{
						mMapLayer[7 + mMapWidth * 70] = 52;
						mMapLayer[42 + mMapWidth * 70] = 0;
					}
					else
					{
						mMapLayer[7 + mMapWidth * 70] = 0;
						mMapLayer[42 + mMapWidth * 70] = 52;
					}

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (mParty.YAxis == 53)
				{
					AppendText(new string[] {
						$" [color={RGB.White}]<< 다음의 옳고 그름을 가리시오 >>[/color]",
						""
					});

					mQuestionID = mRand.Next(8);
					switch (mQuestionID)
					{
						case 0:
							AppendText(new string[] { "문> 이 게임의 배경은 4개의 대륙이다" }, true);
							break;
						case 1:
							AppendText(new string[] { "문> 에인션트 이블은 응징되어야 한다" }, true);
							break;
						case 2:
							AppendText(new string[] { "문> 로드 안만이 유일한 반신반인이다" }, true);
							break;
						case 3:
							AppendText(new string[] { "문> 이 세계의 모든 악은 응징되어야 한다" }, true);
							break;
						case 4:
							AppendText(new string[] { "문> 이 게임의 제작자는 안 영기이다" }, true);
							break;
						case 5:
							AppendText(new string[] { "문> 게임속의 인물은 거의 별의 이름을 가졌다" }, true);
							break;
						case 6:
							AppendText(new string[] { "문> 네크로만서는 신의 경지에 이르렀다" }, true);
							break;
						case 7:
							AppendText(new string[] { "문> 네크로만서는 이 세계의 존재가 아니었다" }, true);
							break;
					}

					ShowMenu(MenuMode.QnA, new string[] {
						"위의 말은 옳다",
						"위의 말은 잘못되었다"
					});
				}
				else if (7 <= mParty.XAxis && mParty.XAxis <= 41 && 18 <= mParty.YAxis && mParty.YAxis <= 42)
				{
					if (mParty.Etc[0] > 0)
						mParty.Etc[0]--;

					ShowMap();
				}
				else if (mParty.YAxis == 17)
				{
					mParty.Etc[0] = 1;
					ShowMap();
				}
				else if (mParty.YAxis == 47 && (mParty.Etc[40] & (1 << 3)) == 0) {
					if (mParty.Etc[0] == 0)
					{
						mParty.Etc[0] = 1;
						ShowMap();
					}

					AppendText(new string[] { $"[color={RGB.LightCyan}]미로속에서 소를 닮은 괴물이 나타났다[/color]" });

					InvokeAnimation(AnimationType.Minotaur2);
				}
				else if (mParty.YAxis == 12) {
					await CheckMuddyFinalBattle();
				}
			}
			else if (mParty.Map == 21) {
				if (mParty.YAxis == 45)
					ShowExitMenu();
				else if (mParty.XAxis == 24 && mParty.YAxis == 19) {
					if (mParty.Etc[39] % 2 == 0 || mParty.Etc[40] % 2 == 0)
					{
						Talk(new string[] {
							$"[color={RGB.LightCyan}] 당신은 아직 라바 게이트를 열수가 없다",
							"",
							"아직 당신은 이 대륙의 동굴 속에 존재하는 2개의 봉인을 풀지 못했기 때문이다."
						});

						mParty.YAxis++;
					}
				}
				else {
					mEncounterEnemyList.Clear();

					var enemyCount = mRand.Next(4) + 3;
					for (var i = 0; i < enemyCount; i++)
						JoinEnemy(57);

					HideMap();
					DisplayEnemy();

					mBattleEvent = 16;

					StartBattle(false);
				}
			}
			else if (mParty.Map == 22) {
				if (mParty.YAxis == 45)
					ShowExitMenu();
				else if (mParty.XAxis == 24 && mParty.YAxis == 17) {
					if ((mParty.Etc[42] & (1 << 1)) == 0) {
						var boss = false;
						for (var i = 0; i < 6; i++) {
							if (!boss)
							{
								if (mRand.Next(2) == 0)
								{
									JoinEnemy(62);
									boss = true;
								}
							}
							else
								JoinEnemy(59);
						}

						HideMap();
						DisplayEnemy();

						Talk($"[color={RGB.LightCyan}] 나는 이 요새의 유령을 조종하는 죽음의 기사이다. 나에게 도전하다니 가소로운 것들. 으하하....");

						mSpecialEvent = SpecialEventType.BattleDeathKnight;
					}
				}
				else if (mParty.YAxis == 24 && 23 <= mParty.XAxis && mParty.XAxis <= 25) {
					if ((mParty.Etc[42] & 1) == 0) {
						mEncounterEnemyList.Clear();

						JoinEnemy(60);
						JoinEnemy(57);
						JoinEnemy(55);
						JoinEnemy(54);
						JoinEnemy(59);

						mBattleEvent = 19;

						HideMap();
						DisplayEnemy();

						StartBattle(false);
					}
				}
				else if ((mParty.Etc[42] & (1 << 1)) == 0) {
					for (var i = 0; i < 5; i++)
						JoinEnemy(59);

					mBattleEvent = 20;

					HideMap();
					DisplayEnemy();

					StartBattle(false);
				}
			}
			else if (mParty.Map == 23) {
				if (mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] == 0)
					return;
				else if (mParty.YAxis == 45)
					ShowExitMenu();
				else if (mParty.YAxis == 25) {
					mEncounterEnemyList.Clear();

					var enemy = JoinEnemy(69);
					enemy.Name = "네크로만서";
					enemy.ENumber = 0;

					HideMap();
					DisplayEnemy();

					Talk(new string[] {
						$"[color={RGB.LightCyan}] 잘도 여기까지 찾아왔구나 {mPlayerList[0].Name}.",
						$"[color={RGB.LightCyan}] 네가 찾던 그 네크로만서가 바로 나다. 드디어 너의 실력을 보게 되겠구나. 하지만 분명히 나보다는 떨어지겠지만. 으하하하."
					});

					mSpecialEvent = SpecialEventType.MeetFaceNecromancer;
				}
				else if (mParty.XAxis == 24 && mParty.YAxis == 26) {
					mMapLayer[24 + mMapWidth * 26] = 46;
					mMapLayer[28 + mMapWidth * 42] = 44;

					for (var y = 6; y < 34; y++) {
						for (var x = 11; x < 39; x++) {
							if (mMapLayer[x + mMapWidth * y] == 0)
								mMapLayer[x + mMapWidth * y] = 39;
						}
					}

					for (var x = 24; x < 26; x++)
						mMapLayer[x + mMapWidth * 11] = 54;

					Talk(" 푯말에 쓰여 있는 대로 이곳의 레버를 당겼더니 굉음과 함께 감추어져 있었던 성이 지하로부터 떠올랐다.");
				}
			}
			else if (mParty.Map == 24) {
				if (mParty.YAxis == 45)
					ShowExitMenu();
			}
			else if (mParty.Map == 25) {
				if (mParty.YAxis == 45)
					ShowExitMenu();
				else if (mParty.YAxis == 42) {
					if (mParty.Etc[0] == 0)
					{
						mParty.Etc[0] = 1;
						ShowMap();
					}

					AppendText(new string[] { $" [color={RGB.White}]금속으로 된 어떤 적이 나타났다.[/color]" });

					InvokeAnimation(AnimationType.PanzerViper);
				}
				else if (mParty.XAxis == 14 && mParty.YAxis == 33) {
					mMapLayer[14 + mMapWidth * 33] = 41;
					for (var x = 10; x < 14; x++) {
						mMapLayer[x + mMapWidth * 32] = 24;
						mMapLayer[x + mMapWidth * 34] = 26;
						mMapLayer[x + mMapWidth * 33] = 42;
					}
					mMapLayer[13 + mMapWidth * 32] = 17;
					mMapLayer[13 + mMapWidth * 34] = 18;
				}
				else if (mParty.XAxis == 35 && mParty.YAxis == 33)
				{
					mMapLayer[35 + mMapWidth * 33] = 41;
					for (var x = 36; x < 40; x++)
					{
						mMapLayer[x + mMapWidth * 32] = 24;
						mMapLayer[x + mMapWidth * 34] = 26;
						mMapLayer[x + mMapWidth * 33] = 42;
					}
					mMapLayer[36 + mMapWidth * 32] = 19;
					mMapLayer[36 + mMapWidth * 34] = 22;
				}
				else if (mParty.XAxis == 4 && mParty.YAxis == 33) {
					mParty.Etc[44] |= 1 << 6;

					string[] message;
					if ((mParty.Etc[44] & (1 << 6)) > 0 && (mParty.Etc[44] & (1 << 7)) > 0)
					{
						mMapLayer[24 + mMapWidth * 26] = 54;
						mMapLayer[25 + mMapWidth * 26] = 54;

						message = new string[2];
						message[1] = " 곧 이어 기계 작동하는 큰 소리가 들렸다.";
					}
					else
						message = new string[1];

					message[0] = " 당신이 레버를 당기자 철컥하는 소리가 동굴에 울려 퍼졌다.";

					Talk(message);
				}
				else if (mParty.XAxis == 45 && mParty.YAxis == 33) {
					mParty.Etc[44] |= 1 << 7;

					string[] message;
					if ((mParty.Etc[44] & (1 << 6)) > 0 && (mParty.Etc[44] & (1 << 7)) > 0)
					{
						mMapLayer[24 + mMapWidth * 26] = 54;
						mMapLayer[25 + mMapWidth * 26] = 54;

						message = new string[2];
						message[1] = " 곧 이어 기계 작동하는 큰 소리가 들렸다.";
					}
					else
						message = new string[1];

					message[0] = " 당신이 레버를 당기자 철컥하는 소리가 동굴에 울려 퍼졌다.";

					Talk(message);
				}
			}
			else if (mParty.Map == 26) {
				InvokeAnimation(AnimationType.FinalBattle);
			}
			else if (mParty.Map == 27) {
				ShowExitMenu();
			}
		}

		private async Task CheckMuddyFinalBattle() {
			if (mParty.Etc[0] == 0)
			{
				mParty.Etc[0] = 1;
				ShowMap();
			}

			if ((mParty.Etc[40] & (1 << 1)) == 0)
			{
				InvokeAnimation(AnimationType.Dragon3);
			}
			else if ((mParty.Etc[40] & (1 << 2)) == 0)
			{
				mEncounterEnemyList.Clear();

				for (var i = 0; i < 7; i++)
					JoinEnemy(30);

				HideMap();
				DisplayEnemy();

				mBattleEvent = 13;

				StartBattle(false);
			}
			else if ((mParty.Etc[40] & 1) == 0) {
				ShowMap();
				InvokeAnimation(AnimationType.AstralMud);
			}
			else {
				mParty.Map = 4;
				mParty.XAxis = 81;
				mParty.YAxis = 16;

				await RefreshGame();
			}
		}

		private void HealOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0 || whomPlayer.Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.HP >= whomPlayer.Endurance * whomPlayer.Level[0])
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 치료할 필요가 없습니다.");
			}
			else
			{
				var needSP = 2 * player.Level[1];
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.HP += needSP * 3 / 2;
					if (whomPlayer.HP > whomPlayer.Level[0] * whomPlayer.Endurance)
						whomPlayer.HP = whomPlayer.Level[0] * whomPlayer.Endurance;

						cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}(은)는 치료되어 졌습니다.[/color]");
				}
			}
		}

		private void CureOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 독이 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.Poison == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 독에 걸리지 않았습니다.");
			}
			else if (player.SP < 15)
			{
				if (mParty.Etc[5] == 0)
					ShowNotEnoughSP(cureResult);

			}
			else
			{
				player.SP -= 15;
				DisplaySP();

				whomPlayer.Poison = 0;

				cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}의 독은 제거 되었습니다.[/color]");
			}
		}

		private void ConsciousOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 의식이 돌아올 상태가 아닙니다.");
			}
			else if (whomPlayer.Unconscious == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 의식불명이 아닙니다.");
			}
			else
			{
				var needSP = 10 * whomPlayer.Unconscious;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Unconscious = 0;
					if (whomPlayer.HP <= 0)
						whomPlayer.HP = 1;

					cureResult.Add($"{whomPlayer.Name}(은)는 의식을 되찾았습니다.");
				}
			}
		}

		private void RevitalizeOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.Name}(은)는 아직 살아 있습니다.");
			}
			else
			{
				var needSP = player.Dead * 30;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Dead = 0;
					if (whomPlayer.Unconscious > whomPlayer.Endurance * whomPlayer.Level[0])
						whomPlayer.Unconscious = whomPlayer.Endurance * whomPlayer.Level[0];

					if (whomPlayer.Unconscious == 0)
						whomPlayer.Unconscious = 1;

					cureResult.Add($"{whomPlayer.Name}(은)는 다시 생명을 얻었습니다.");

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

		private void ShowNotEnoughSP(List<string> result = null)
		{
			var message = "그러나, 마법 지수가 충분하지 않습니다.";
			if (result == null)
			{
				AppendText(new string[] { message }, true);
				ContinueText.Visibility = Visibility.Visible;
			}
			else
				result.Add(message);
		}

		private void ShowNotEnoughESP(List<string> result = null)
		{
			var message = "ESP 지수가 충분하지 않습니다.";
			if (result == null)
			{
				AppendText(new string[] { message }, true);
				ContinueText.Visibility = Visibility.Visible;
			}
			else
				result.Add(message);
		}

		private void ShowNotEnoughMoney(List<string> result = null)
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

		private void JoinMember(Lore player)
		{
			if (mPlayerList.Count < 6)
			{
				mPlayerList.Add(player);
				DisplayPlayerInfo();

				if (mMemberLeftTile > 0)
					LeaveMemberPosition();
			}
			else
			{
				AppendText(new string[] { "교체 시킬 인물은 누구입니까?" });
				var memberNames = new string[mPlayerList.Count - 1];
				for (var i = 1; i < mPlayerList.Count; i++)
					memberNames[i - 1] = mPlayerList[i].Name;

				mReserveMember = player;

				ShowMenu(MenuMode.SwapMember, memberNames);
			}
		}

		private void LeaveMemberPosition() {
			if (mMemberX >= 0 && mMemberY >= 0)
				mMapLayer[mMemberX + mMapWidth * mMemberY] = mMemberLeftTile;

			mMemberLeftTile = 0;
			mMemberX = -1;
			mMemberY = -1;
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

		private void ShowSpinner(SpinnerType spinnerType, Tuple<string, int>[] items, int defaultId) {
			mSpinnerType = spinnerType;

			mSpinnerItems = items;
			mSpinnerID = defaultId;

			AppendText(SpinnerText, mSpinnerItems[defaultId].Item1);
			SpinnerText.Visibility = Visibility.Visible;
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

		private bool AppendText(string str, bool append = false) {
			return AppendText(DialogText, str, append);
		}

		private void AppendText(string[] text, bool append = false)
		{
			var added = true;
			for (var i = 0; i < text.Length; i++)
			{
				if (added)
				{
					if (i == 0)
						added = AppendText(text[i], append);
					else
						added = AppendText(text[i], true);
				}

				if (!added)
					mRemainBattleResult.Add(text[i]);
			}
		}

		private bool AppendText(RichTextBlock textBlock, string str, bool append = false) {

			var totalLen = 0;

			if (append)
			{
				foreach (Paragraph prevParagraph in textBlock.Blocks)
				{
					foreach (Run prevRun in prevParagraph.Inlines)
					{
						totalLen += prevRun.Text.Length;
					}
				}
			}
			else
			{
				textBlock.TextHighlighters.Clear();
				textBlock.Blocks.Clear();
			}

			var highlighters = new List<TextHighlighter>();
			var paragraph = new Paragraph();
			textBlock.Blocks.Add(paragraph);

			var startIdx = 0;
			while ((startIdx = str.IndexOf("[", startIdx)) >= 0)
			{
				if (startIdx < str.Length - 1 && str[startIdx + 1] == '[')
				{
					str = str.Remove(startIdx, 1);
					startIdx++;
					continue;
				}

				var preRun = new Run
				{
					Text = str.Substring(0, startIdx)
				};

				paragraph.Inlines.Add(preRun);
				var preTextHighlighter = new TextHighlighter()
				{
					Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(RGB.LightGray.Substring(0, 2), 16), Convert.ToByte(RGB.LightGray.Substring(2, 2), 16), Convert.ToByte(RGB.LightGray.Substring(4, 2), 16))),
					Background = new SolidColorBrush(Colors.Transparent),
					Ranges = { new TextRange()
							{
								StartIndex = totalLen,
								Length = preRun.Text.Length
							}
						}
				};

				highlighters.Add(preTextHighlighter);
				textBlock.TextHighlighters.Add(preTextHighlighter);

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
					var tagRun = new Run
					{
						Text = str.Substring(0, startIdx).Replace("[[", "[")
					};

					paragraph.Inlines.Add(tagRun);

					var textHighlighter = new TextHighlighter()
					{
						Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(tagData[1].Substring(0, 2), 16), Convert.ToByte(tagData[1].Substring(2, 2), 16), Convert.ToByte(tagData[1].Substring(4, 2), 16))),
						Background = new SolidColorBrush(Colors.Transparent),
						Ranges = { new TextRange()
							{
								StartIndex = totalLen,
								Length = tagRun.Text.Length
							}
						}
					};

					highlighters.Add(textHighlighter);
					textBlock.TextHighlighters.Add(textHighlighter);

					totalLen += tagRun.Text.Length;
				}

				str = str.Substring(startIdx + endTag.Length);
				startIdx = 0;
			}

			var run = new Run
			{
				Text = str
			};

			paragraph.Inlines.Add(run);
			var postTextHighlighter = new TextHighlighter()
			{
				Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(RGB.LightGray.Substring(0, 2), 16), Convert.ToByte(RGB.LightGray.Substring(2, 2), 16), Convert.ToByte(RGB.LightGray.Substring(4, 2), 16))),
				Background = new SolidColorBrush(Colors.Transparent),
				Ranges = { new TextRange()
						{
							StartIndex = totalLen,
							Length = run.Text.Length
						}
					}
			};

			highlighters.Add(postTextHighlighter);
			textBlock.TextHighlighters.Add(postTextHighlighter);

			totalLen += run.Text.Length;


			textBlock.UpdateLayout();
			//var DialogText.Tag

			var lineHeight = 0d;
			if (textBlock.Blocks.Count > 0)
			{
				var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
				lineHeight = startRect.Height;
			}

			var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);
			
			if (lineCount > DIALOG_MAX_LINES)
			{
				textBlock.Blocks.Remove(paragraph);
				foreach (var highlighter in highlighters)
					textBlock.TextHighlighters.Remove(highlighter);

				return false;
			}
			else
				return true;
		}

		private void Talk(string dialog)
		{
			Talk(new string[] { dialog });
		}

		private void Talk(string[] dialog)
		{
			AppendText(dialog);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			void GoGrocery()
			{
				AppendText(new string[] {
						$"[color={RGB.White}]여기는 식료품점 입니다.[/color]",
						$"[color={RGB.White}]몇개를 원하십니까?[/color]"
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
						"무엇보다도, 왕뱀과 벌레 떼들과 독사는 맹독이 있으니 주의하시기 바라오."
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
						AppendText(new string[] { " 당신이 네크로만서에 진정으로 대항하고자 한다면, 이 성의 바로 위에 있는 피라미드에 가보도록 하시오." +
							$"그 곳은 네크로만서와 동시에 바다에서 떠오른 [color={RGB.LightCyan}]또 다른 지식의 성전[/color]이기 때문이오." +
							" 당신이 어느 수준이 되어 그 곳에 들어간다면 진정한 이 세계의 진실을 알 수 있을 것이오." });

						mSpecialEvent = SpecialEventType.MeetLoreSolider;
						ContinueText.Visibility = Visibility.Visible;
					}
					else
					{
						AppendText(new string[] { " '메너스'속에는 드와프, 거인, 식인 늑대, 독사 같은 괴물들이 살고 있소." });

						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (moveX == 57 && moveY == 73)
				{
					AppendText(new string[] { " 나의 부모님은 독사의 독에 의해 돌아가셨습니다. 독사 정말 위험한 존재입니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 62 && moveY == 26)
				{
					AppendText(new string[] {
						" 단지 로드 안만이 능력 상으로 네크로만서에게 도전할 수 있습니다.",
						" 하지만 로드 안 자신이 대립을 싫어해서, 현재는 네크로만서에게 대항할 자가 없습니다."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 89 && moveY == 81)
				{
					AppendText(new string[] { " 우리는 에인션트 이블을 배척하고 로드 안님을 받들어야 합니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 93 && moveY == 67)
				{
					AppendText(new string[] { " 우리는 메너스의 동쪽에 있는 나무로부터 많은 식량을 얻은 적이 있습니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 18 && moveY == 52)
				{
					AppendText(new string[] { $"[color={RGB.LightGreen}] 이 세계의 창시자는 안 영기님 이시며, 그는 위대한 프로그래머입니다.[/color]" });

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
					AppendText(new string[] { "요새 무덤 쪽에서 유령이 떠돈다던데..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 12 && moveY == 31)
				{
					AppendText(new string[] { "하하하, 자네도 한번 마셔보게나." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 14 && moveY == 34)
				{
					AppendText(new string[] { " 이제 로드 안의 시대도 끝나가는가? 그까짓 네크로만서라는 작자에게 쩔쩔 매는 꼴이라니 ... 차라리 내가 나가서 그놈과 싸우는 게 났겠다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 17 && moveY == 32)
				{
					AppendText(new string[] { " 당신은 스켈레톤족의 한 명이 우리와 함께 생활하려 한다는 것에 대해서 어떻게 생각하십니까?",
					" 저는 그 말을 들었을 때 너무 혐오스러웠습니다. 어서 빨리 그 살아있는 뼈다귀를 여기서 쫓아냈으면 좋겠습니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 20 && moveY == 35)
				{
					AppendText(new string[] { " ... 끄~~윽 ... ..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 17 && moveY == 37)
				{
					AppendText(new string[] { " 이보게 자네, 내말 좀 들어 보게나. 나의 친구들은 이제 이 세상에 없다네. 그들은 너무나도 용감하고 믿음직스러운 친구들이었는데..." +
						"내가 다리를 다쳐 병원에 있을 동안 그들은 모두 이 대륙의 평화를 위해 로어 특공대에 지원했다네. 하지만 그들은 아무도 다시는 돌아오지 못했어." +
						" 그런 그들에게 이렇게 살아있는 나로서는 미안할 뿐이네 그래서 술로 나날을 보내고 있지. 죄책감을 잊기 위해서 말이지..." });

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
						AppendText(new string[] { " 당신이 한 유골 앞에 섰을 때 이상한 느낌과 함께 먼 곳으로부터 어떤 소리가 들려왔다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 안녕하시오. 대담한 용사여.[/color]",
							$"[color={RGB.LightMagenta}] 당신이 나의 잠을 깨웠소? 나는 고대에 이곳을 지키다가 죽어간 기사 주니어 안타레스라고 하오." +
							" 저의 아버지는 레드 안타레스라고 불렸던 최강의 마법사였소. 그는 말년에 어떤 동굴로 은신을 한 후 아무에게도 모습을 나타내지 않았소." +
							" 하지만 당신의 운명은 나의 아버지를 만나야만 하는 운명이라는 것을 알 수 있소. 반드시 나의 아버지를 만나서 당신이 알지 못했던 새로운 능력들을 배우시오." +
							" 그리고 나의 아버지를 당신의 동행으로 참가시키도록 하시오. 물론 좀 어렵겠지만 ...[/color]" });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $"[color={RGB.LightMagenta}] 아 참, 그리고 내가 죽기 전에 여기에 뭔가를 여기에 숨겨 두었는데 당신에게 도움이 될지 모르겠소. 그럼, 나는 다시 오랜 잠으로 들어가야겠소.[/color]" });

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
					"자네라면 충분히 네크로만서를 무찌를 수 있을 걸세. 자네만 믿겠네."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 23 && moveY == 53)
				{
					AppendText(new string[] { $" 위의 저 친구로부터 당신 얘기 많이 들었습니다. 저는 우리성에서 당신 같은 용감한 사람이 있다는 걸 자랑스럽게 생각합니다." });
					 
					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 12 || moveY == 54)
				{
					AppendText(new string[] { $" 만약, 당신들이 그 일을 해내기가 어렵다고 생각되시면 라스트디치 성에서 성문을 지키고 있는 폴라리스란 청년을 일행에 참가시켜 주십시오." +
						" 분명 그 사람이라면 쾌히 승낙할 겁니다." });

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
					" 아마 그들은 죽기 전에는 이곳을 나올 수 없을 겁니다." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 40 && moveY == 9)
				{
					if (mTalkMode == 0)
					{
						AppendText(new string[] { $" 나는 이곳의 기사로서 이 세계의 모든 대륙을 탐험하고 돌아왔었습니다." +
						" 내가 마지막 대륙을 돌았을 때 나는 새로운 존재를 발견했습니다. 그는 바로 예전까지도 로드 안과 대립하던 에인션트 이블이라는 존재였습니다." +
						" 지금 우리의 성에서는 철저하게 배격하도록 어릴 때부터 가르침 받아온 그 에인션트 이블이었습니다." +
						" 하지만 그곳에서 본 그는 우리가 알고 있는 그와는 전혀 다른 인간미를 가진 말 그대로 신과 같은 존재였습니다." +
						"내가 그의 신앙 아래 있는 어느 도시를 돌면서 내가 느낀것은 정말 로드 안에게서는 찾아볼 수가 없는 그런 자애와 따뜻한 정이었습니다." +
						" 그리고 여태껏 내가 알고 있는 그에 대한 지식이 정말 잘못되었다는 것과 이런 사실을 다른 사람에게도 알려주고 싶다는 이유로 그의 사상을 퍼뜨리다 이렇게 잡히게 된 것입니다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 1)
					{
						AppendText(new string[] { " 하지만 더욱 이상한 것은 로드 안 자신도 그에 대한 사실을 인정하면서도 왜 우리에게는 그를 배격하도록만 교육하는 가를 알고 싶을 뿐입니다." +
							"로드 안께서는 나를 이해한다고 하셨지만 사회 혼란을 방지하기 위해 나를 이렇게 밖에 할 수 없다고 말씀하시더군요. 그리 이것은 선을 대표하는 자기로서는 이 방법 밖에는 없다고 하시더군요." +
							" 하지만 로드 안의 마음은 사실 이렇지 않다는 걸 알 수 있었습니다. 에인션트 이블의 말로는 사실 서로가 매우 절친한 관계임을 알 수가 있었기 때문입니다." });

						ContinueText.Visibility = Visibility.Visible;

						mTalkMode = 0;
					}

				}
				else if (moveX == 39 && moveY == 14)
				{
					AppendText(new string[] { " 히히히... 위대한 용사님. 낄낄낄.. 내가 당신들의 일행에 끼이면 안될까요? 우히히히.." });

					ShowMenu(MenuMode.JoinMadJoe, new string[]
					{
						"그렇다면 당신을 받아들이지요",
						"당신은 이곳에 그냥 있는 게 낫겠소"
					});
				}
				else if (moveX == 62 && moveY == 9)
				{
					AppendText(new string[] { " 안녕하시오. 나는 한때 이곳의 유명한 도둑이었던 사람이오. 결국 그 때문에 나는 잡혀서 평생 여기에 있게 되었지만...",
					$" 그건 그렇고, 내가 로어 성의 보물인 [color={RGB.LightCyan}]황금의 방패[/color]를 훔쳐 달아나다. 그만 그것을 메너스라는 금광에 숨겨 놓은 채 잡혀 버리고 말았소.",
					"나는 이제 그것을 가져봤자 쓸 때도 없으니 차라리 당신이 그걸 가지시오. 가만있자... 어디였더라... 그래! 메너스의 가운데쯤에 벽으로 사방이 둘러 싸여진 곳이었는데.." +
					" 당신들이라면 지금 여기에 들어온 것과 같은 방법으로 들어가서 방패를 찾을수 있을 것이오. 행운을 빌겠소."
					});

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 59 && moveY == 14)
				{
					AppendText(new string[] { " 당신들에게 경고해 두겠는데 건너편 방에 있는 조는 오랜 수감생활 끝에 미쳐 버리고 말았소. 그의 말에 속아서 당신네 일행에 참가시키는 그런 실수는 하지 마시오." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if ((moveX == 41 && moveY == 77) || (moveX == 41 && moveY == 79))
				{
					if ((mParty.Etc[49] & (1 << 3)) == 0)
						AppendText(new string[] { " 로드 안님의 명령에 의해서 당신들에게 한 가지의 무기를 드리겠습니다. 들어가셔서 무기를 선택해 주십시오." });
					else
						AppendText(new string[] { " 여기서 가져가신 무기를 잘 사용하셔서 세계의 적인 네크로만서를 무찔러 주십시오." });

					ContinueText.Visibility = Visibility.Visible;

				}
				else if (moveX == 50 && moveY == 13)
				{
					AppendText(new string[] { "메너스 에는 금덩이가 많다던데..." });

					ContinueText.Visibility = Visibility.Visible;
				}
				else if (moveX == 82 && moveY == 26)
				{
					AppendText(new string[] { "메너스는 한때 금광이었습니다." });

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
							AppendText(new string[] { "당신은 이 게임 세계에 도전하고 싶습니까?",
							AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" ? "(A: 예 / B: 아니오)" : "(Y/n)" });

							mTalkMode = 1;
							mTalkX = moveX;
							mTalkY = moveY;

							mSpecialEvent = SpecialEventType.StartGame;
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
						mSpecialEvent = SpecialEventType.None;
					}
					else if (mTalkMode == 1 && (key == VirtualKey.N || key == VirtualKey.GamepadB))
					{
						AppendText(new string[] { "",
						"아니오.",
						"",
						"다시 생각해 보십시오." }, true);

						ContinueText.Visibility = Visibility.Visible;
						mTalkMode = 0;
						mSpecialEvent = SpecialEventType.None;
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
							"이제부터 당신은 이 게임에서 새로운 인물로서 생을 시작하게 될 것이오. 그럼 나의 이야기를 시작하겠소." });
							ContinueText.Visibility = Visibility.Visible;

							mParty.Etc[9]++;

							mTalkMode = 1;
							mTalkX = moveX;
							mTalkY = moveY;
						}
						else if (mParty.Etc[9] == 3)
						{
							AppendText(new string[] { $" 대륙의 남서쪽에 있는 '[color={RGB.LightCyan}]메너스[/color]'를 탐사해 주시오." });
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mParty.Etc[9] == 4)
						{
							AppendText(new string[] { "당신들의 성공을 축하하오!!",
						$"[color={RGB.LightCyan}][[EXP + 1000][/color]" });

							mPlayerList.ForEach(delegate (Lore player)
							{
								player.Experience += 1000;
							});

							mParty.Etc[9]++;
							ContinueText.Visibility = Visibility.Visible;
						}
						else if (mParty.Etc[9] == 5)
						{
							AppendText(new string[] { " 드디어 나는 당신들의 능력을 믿을 수 있게 되었소. 그렇다면 당신들에게 네크로만서 응징이라는 막중한 임무를 한번 맡겨 보겠소.",
							$"먼저 대륙의 동쪽에 있는 '[color={RGB.LightCyan}]라스트디치[/color]'에 가보도록 하시오. '[color={RGB.LightCyan}]라스트디치[/color]'성에는 지금 많은 근심에 쌓여있소. 그들을 도와주시오." });

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
							" 한 날, 대기의 공간이 진동하며 난데없는 푸른 번개가 대륙들 중의 하나를 강타했소. 공간은 휘어지고 시간은 진동하며 이 세계를 공포 속으로 몰고 갔소." +
							" 그 번개의 위력으로 그 불운한 대륙은 황폐화된 용암 대지로 변하고 말았고, 다른 하나의 대륙은 충돌 시의 진동에 의해 바다 깊이 가라앉아 버렸소.",
							" 그런 일이 있은 한참 후에, 이상하게도 용암대지의 대륙으로부터 강한 생명의 기운이 발산되기 시작했소." +
							" 그래서, 우리들은 그 원인을 알아보기 위해 '로어 특공대'를 조직하기로 합의를 하고 " +
							"이곳에 있는 거의 모든 용사들을 모아서 용암 대지로 변한 그 대륙으로 급히 그들을 파견하였지만 여태껏 아무 소식도 듣지 못했소. 그들이 생존해 있는지조차도 말이오.",
							$" 이런저런 방법을 통하여 그들의 생사를 알아내려던 중 우연히 우리들은 '[color={RGB.LightCyan}]네크로만서[/color]'라고 불리는 용암 대지속의 새로운 세력의 존재를" +
							"알아내었고, 그때의 그들은 이미 막강한 세력으로 성장해가고 있는 중이었소. 그때의 번개는 그가 이 공간으로 이동하는 수단이었소. 즉 그는 이 공간의 인물이 아닌 다른 차원을 가진 공간에서 왔던 것이오."
						});

						ContinueText.Visibility = Visibility.Visible;
						mParty.Etc[9]++;

						mTalkMode = 2;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mTalkMode == 2)
					{
						AppendText(new string[] { $" 그는 현재 이 세계의 반을 그의 세력권 안에 넣고 있소. 여기서 당신의 궁극적인 임무는 바로 '[color={RGB.LightCyan}]네크로만서의 야심을 봉쇄 시키는 것[/color]'이라는 걸 명심해 두시오.",
						" 네크로만서의 영향력은 이미 로어 대륙까지 도달해있소. 또한 그들은 이 대륙의 남서쪽에 '메너스'라고 불리는 지하 동굴을 얼마 전에 구축했소. 그래서, 그 동굴의 존재 때문에 우리들은 그에게 위협을 당하게 되었던 것이오.",
						" 하지만, 로어 특공대가 이 대륙을 떠난 후로는 그 일당들에게 대적할 용사는 이미 남아있지 않았소. 그래서 부탁하건대, 그 동굴을 중심부까지 탐사해 주시오.",
						" 나는 당신들에게 네크로만서에 대한 일을 맡기고 싶지만, 아직은 당신들의 확실한 능력을 모르는 상태이지요. 그래서 이 일은 당신들의 잠재력을 증명해 주는 좋은 기회가 될 것이오.",
						" 만약 당신들이 무기가 필요하다면 무기고에서 약간의 무기를 가져가도록 허락하겠소."
						});

						ContinueText.Visibility = Visibility.Visible;

						mParty.Etc[9]++;

						mTalkMode = 0;
					}
				}
			}
			else if (mParty.Map == 7)
			{
				if (moveX == 50 && moveY == 54)
					Talk($"라스트디치 성과 배리안트 피플즈 성은 매우 닮았다는 말이 있습니다.");
				else if (moveX == 7 && moveY == 43)
					Talk("이 세계는 다섯 개의 대륙으로 되어 있다더군요.");
				else if (moveX == 67 && moveY == 34)
					Talk("각각의 대륙에는 서로 통하는 문이 존재합니다.");
				else if (moveX == 42 && moveY == 8)
					Talk("당신은 피라미드 안에서 쉽게 창을 발견할 수 있을 것입니다.");
				else if (moveX == 64 && moveY == 9)
					Talk("지하 출입구는 여기로부터 서쪽에 나타나곤 합니다.");
				else if (moveX == 13 && moveY == 67)
					Talk("로어 특공대의 지휘관은 저의 남편인데 '로어 헌터'라고 불렸습니다.");
				else if (moveX == 56 && moveY == 41)
					Talk("미이라 장군과 두 마리의 스핑크스의 공격은 가히 치명적입니다.");
				else if (moveX == 43 && moveY == 33)
					Talk("지하 출입구는 당신을 다른 대륙으로 인도해 줄 것입니다.");
				else if (moveX == 31 && moveY == 55)
					Talk("로어 특공대의 지휘관은 상당히 능력 있는 인물이었습니다.");
				else if ((moveX == 35 && moveY == 18) || (moveX == 35 && moveY == 20) || (moveX == 40 && moveY == 17) || (moveX == 40 && moveY == 19) || (moveX == 40 && moveY == 21) || (moveX == 39 && moveY == 40))
				{
					if (mParty.Etc[12] == 0)
						Talk("성주님을 만나 보십시오.");
					else
						Talk("당신이 성공하기를 빕니다.");
				}
				else if (moveX == 36 && moveY == 40 && mParty.Etc[13] < 2)
				{
					AppendText(new string[] {
						$"나의 이름은 [color={RGB.LightCyan}]폴라리스[/color]요.",
						"당신들과 같이 미이라 장군을 물리치고 싶소.",
						$"[color={RGB.LightGreen}]내가 당신의 일행에 끼여도 되겠소?[/color]"
					});

					ShowMenu(MenuMode.JoinPolaris, new string[] {
						"나는 당신의 제안을 받아들이겠소",
						"나는 당신의 도움은 필요 없소"
					});

					mMemberLeftTile = 44;
					mMemberX = moveX;
					mMemberY = moveY;
				}
				else if ((moveX == 17 && moveY == 18) || (moveX == 23 && moveY == 18) || (moveX == 20 && moveY == 20) || (moveX == 15 && moveY == 23))
					GoTrainingCenter();
				else if ((moveX == 56 && moveY == 16) || (moveX == 53 && moveY == 19) || (moveX == 57 && moveY == 21) || (moveX == 58 && moveY == 24))
					GoGrocery();
				else if ((moveX == 58 && moveY == 55) || (moveX == 58 && moveY == 57) || (moveX == 58 && moveY == 59))
					GoWeaponShop();
				else if ((moveX == 16 && moveY == 55) || (moveX == 16 && moveY == 57) || (moveX == 16 && moveY == 59))
					GoHospital();
				else if (moveX == 37 && moveY == 16)
				{
					if (mParty.Etc[12] == 0)
					{
						Talk(new string[] {
							$" 당신이 {mPlayerList[0].Name}이오?",
							" 나는 로드 안에게 당신이 온다는 소식을 전해 받았소. 들었다시피 우리성에는 큰 문제가 있소." +
							" 그것은 이 성의 북쪽에 위치해있는 동굴 때문이오. 그 동굴 때문에 우리들은 상당한 압박을 받고 있소.",
							$" 만약 당신들이 위대한 영웅이라면, [color={RGB.LightCyan}]피라미드[/color]라는 동굴에 있는 [color={RGB.LightCyan}]미이라 장군[/color]을 처단해 주시오.",
							" 당신들이 이 임무를 완수하면 나는 당신들에게 도움에 대한 댓가를 치러주겠소."
						});

						mParty.Etc[12]++;
					}
					else if (mParty.Etc[12] == 1)
					{
						Talk($" 부탁하건대, 피라미드의 '[color={RGB.LightCyan}]미이라 장군[/color]'을 처단해 주시오.");
					}
					else if (mParty.Etc[12] == 2)
					{
						Talk(new string[] {
							"당신의 성공에 경의를 표하오.",
							$"[color={RGB.LightCyan}][[EXP + 10000][/color]"
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 10000;
						}

						mParty.Etc[12]++;
					}
					else if (mParty.Etc[12] == 3)
					{
						Talk(new string[] {
							$" 이 성의 북동쪽에 '[color={RGB.LightCyan}]지하 출입구[/color]'라는 것이 있소." +
							$" 만약 당신이 '[color={RGB.LightCyan}]지하 출입구[/color]' 속에 들어간다면 '[color={RGB.LightCyan}]배리안트 피플즈[/color]'성으로 통하게 될것이오.",
							$"'[color={RGB.LightCyan}]배리안트 피플즈[/color]'는 네크로만서에게 매우 심하게 영향을 받고 있소"
						});
					}
				}
			}
			else if (mParty.Map == 9)
			{
				if ((moveX == 11 && moveY == 10) || (moveX == 14 && moveY == 11) || (moveX == 11 && moveY == 14))
					GoTrainingCenter();
				else if ((moveX == 39 && moveY == 36) || (moveX == 36 && moveY == 38) || (moveX == 40 && moveY == 40))
					GoGrocery();
				else if ((moveX == 36 && moveY == 9) || (moveX == 39 && moveY == 11) || (moveX == 40 && moveY == 14))
					GoWeaponShop();
				else if ((moveX == 8 && moveY == 38) || (moveX == 11 && moveY == 40) || (moveX == 15 && moveY == 39))
					GoHospital();
				else if (moveX == 23 && moveY == 37)
					Talk(" 이블 씰의 어디엔가에 '황금의 봉인'이 숨겨져 있다더군요.");
				else if (moveX == 22 && moveY == 11)
					Talk(" 황금의 갑옷이 퀘이크 동굴 안에 숨겨져있다는 소문이 떠돌고 있습니다.");
				else if (moveX == 27 && moveY == 17)
					Talk(" 배리안트 피플즈 성은 네크로만서에 대한 강한 저항 때문에 그에 의해 쑥밭이 되어 버렸습니다.");
				else if (moveX == 29 && moveY == 30)
					Talk(" 배리안트 피플즈 최대의 사냥꾼인 리겔은 성을 파괴시킨 적들을 물리치기 위해서 이블 씰로 들어갔습니다.");
				else if (moveX == 33 && moveY == 37)
					Talk("위쪽에는 스왐프 대륙으로 통하는 문이 있지만 아무도 접근할 수가 없습니다.");
				else if (moveX == 37 && moveY == 13)
					Talk("퀘이크 속에는 많은 비밀문이 있다고 들었습니다.");
				else if (moveX == 14 && moveY == 41)
					Talk("워터 필드로 통하는 문에는 세 마리의 와이번이 지키고 있습니다.");
				else if (moveX == 25 && moveY == 6)
					Talk("스왐프 대륙으로 통하는 문에는 고르곤 세 자매가 살고 있소.");
				else if ((moveX == 33 && moveY == 23) || (moveX == 36 && moveY == 23) || (moveX == 40 && moveY == 23) || (moveX == 34 && moveY == 26) || (moveX == 37 && moveY == 26) || (moveX == 40 && moveY == 26))
				{
					if (mParty.Etc[13] == 0)
						Talk("우리 성주님을 만나보십시오.");
					else
						Talk("당신의 성공을 빌겠습니다.");
				}
				else if (moveX == 41 && moveY == 24)
				{
					if (mParty.Etc[13] == 0)
					{
						Talk(new string[] {
							" 당신을 만나게 되어 영광이오.",
							" 나는 로어 대륙에서의 당신의 공훈을 높이 평가하며, 또한 라스트디치성을 구제하것에 대해서도 감사를 표하오." +
							$" 라스트디치 성의 쌍둥이 성인 '[color={RGB.LightCyan}]배리안트 피플즈[/color]'는 네크로만서에 대한 강한 저항 때문에 그에 의해서 처참히 파괴되었소." +
							$" 그런 후에 그는 '배리안트 피플즈'의 지하에다가 [color={RGB.LightCyan}]이블 씰[/color]이라는 동굴을 구축하였소.",
							"그리고 그는 이블 씰의 어디엔가에 이 대륙의 운명을 담고 있는 황금의 봉인을 숨겨놓았소. 만약 그 봉인이 풀어진다면, 이 대륙은 봉인 속에서 나타난 괴물들에 의해서 황폐화될 것이오.",
							$" 한시바삐 이블 씰로 가시오, 그리고 '[color={RGB.LightCyan}]황금의 봉인[/color]'을 찾으시오."
						});
						mParty.Etc[13]++;
					}
					else if (mParty.Etc[13] == 1)
					{
						Talk(new string[] {
							$" 배리안트 피플즈에 있는 이블 씰로 가서 [color={RGB.LightCyan}]황금의 봉인[/color]을 찾아오시오.",
							" 지금 지체할 시간이 없소."
						});
					}
					else if (mParty.Etc[13] == 2)
					{
						Talk(new string[] {
							" 오, 당신은 황금의 봉인을 찾았군요!",
							$" [color={RGB.LightCyan}][[ EXP + 10000 ][/color]"
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 10000;
						}

						mParty.Etc[13]++;
					}
					else if (mParty.Etc[13] == 3)
					{
						Talk(new string[] {
							" 그러나, 이 대륙에는 아직 위험한 장소가 많이 있소.",
							$" 여기로부터 북동쪽에 '[color={RGB.LightCyan}]퀘이크[/color]'라고 불리는 동굴이 있소. 만약 퀘이크마저 무너뜨리면, 이 대륙은 다시 평화롭게 될 것이오.",
							$" [color={RGB.LightCyan}]퀘이크[/color]로 가서 보스인 아키가고일과 좀비들을 물리쳐 주십시오."
						});
						mParty.Etc[13]++;
					}
					else if (mParty.Etc[13] == 4)
					{
						Talk($"퀘이크의 [color={RGB.LightCyan}]아키가고일[/color]을 물리치십시오.");
					}
					else if (mParty.Etc[13] == 5)
					{
						Talk(new string[] {
							" 당신들은 위대한 영웅임에 틀림없군요.",
							$" [color={RGB.LightCyan}][[ EXP + 40000 ][/color]",
							"",
							$" 여기에 [color={RGB.LightCyan}]워터 키[/color]가 있소.",
							$" 이 열쇠는 [color={RGB.LightCyan}]워터 필드[/color]의 문을 열 것이오."
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 40000;
						}

						mParty.Etc[13]++;
					}
					else if (mParty.Etc[13] == 6)
						Talk($"와이번 동굴의 [color={RGB.LightCyan}]워터 필드의 문[/color]을 통하여 다음 대륙으로 가십시오.");
				}
			}
			else if (mParty.Map == 10)
			{
				if ((moveX == 35 && moveY == 31) || (moveX == 37 && moveY == 32) || (moveX == 38 && moveY == 34))
					GoTrainingCenter();
				else if ((moveX == 16 && moveY == 56) || (moveX == 11 && moveY == 58) || (moveX == 10 && moveY == 54))
					GoGrocery();
				else if ((moveX == 10 && moveY == 29) || (moveX == 10 && moveY == 31) || (moveX == 12 && moveY == 33))
					GoWeaponShop();
				else if ((moveX == 32 && moveY == 59) || (moveX == 34 && moveY == 53) || (moveX == 40 && moveY == 57))
					GoHospital();
				else if (moveX == 10 && moveY == 15)
					Talk("노티스 동굴의 히드라는 머리가 셋이나 달렸다더군요.");
				else if (moveX == 13 && moveY == 17)
					Talk("노티스 동굴은 혼란스러운 미로라서 항상 주위를 염두에 두셔야 합니다.");
				else if (moveX == 23 && moveY == 21)
					Talk("락업 동굴은 미로로 구성된 동굴이오.");
				else if (moveX == 26 && moveY == 21)
					Talk("락업 속에 미노타우루스는 네크로만서의 부하는 아닙니다.");
				else if (moveX == 23 && moveY == 68)
					Talk(" 락업 의 보스인 거대 드래곤은 아주 거대한 용이라는데, 그것의 꼬리 또한 강력한 무기라서 조심해야 할 것이오.");
				else if (moveX == 36 && moveY == 15)
					Talk(" 고르곤 세 자매의 힘은 네크로만서의 힘과 필적하지만 중대한 약점이 하나 있소.");
				else if (moveX == 39 && moveY == 17)
					Talk("스테노와 에우리알레는 거의 불멸의 생명체 입니다.");
				else if (moveX == 39 && moveY == 55)
				{
					AppendText(new string[] {
					" 나는 로어 특공대의 대장인 로어 헌터라고 하오. 여기서의 적들과는, 이제 대항하기가 혼자서는 무리라고 판단했소." +
					" 그래서, 나는 여태껏 여기서 새로운 영웅들을 기다리고 있었소.",
					" 내가 당신의 일행에 끼게 되는 걸 어떻게 생각하오?"
					});

					ShowMenu(MenuMode.JoinLoreHunter, new string[] {
						"우리도 그러기를 바라오",
						"몸이 완전히 회복될 때까지 기다리시오"
					});
				}
				else if (moveX == 24 && moveY == 17) {
					if (mParty.Etc[14] == 0) {
						Talk(new string[] {
						" 여기는 워터 필드라는 성이오. 이곳이 당신이 마지막으로 거칠 우리편의 성이오.",
						" 차원의 틈을 통해 네크로만서가 내려 오던 날이 대륙은 거의 전부가 바다로 가라앉았소." +
						" 하지만 그때에 살아남은 사람들은 아직 가라앉지 않은 이곳에 찾아와서 이 성을 건립했소. 그러고는 공이 컸던 나를 왕으로 추대했던 것이오.",
						" 당신도 생각하고 있다시피 이 대륙은 거의 물로 덮여있소. 하지만 이곳처럼 물이 차지 않은 두 곳에 네크로만서는 이미 이 대륙의 지배를 위한 동굴을 만들었소." +
						" 그 두 곳의 적들은 여타의 대륙과는 비교가 안될 정도의 거대한 적들이 많이 있소.",
						" 우리로서는 더 이상 손을 쓸 수가 없소. 이곳 사람들의 마지막 희망인 이곳이 적들에게 점령된다면 와이번 동굴로 이어지는 워터 게이트를 통해 다른 대륙도 하나둘씩 점령되어 갈 것이오."
						});

						mParty.Etc[14]++;
					}
					else if (mParty.Etc[14] == 1)
						Talk(" 먼저, 여기서 남서쪽의 어느 섬에 있는 동굴인 노티스에 가서 보스인 히드라를 물리쳐 주십시오.");
					else if (mParty.Etc[14] == 2) {
						Talk(new string[] {
							"히드라를 물리치다니 ... 대단한 능력이오.",
							$" [color={RGB.LightCyan}][[ EXP + 150000 ][/color]",
							"이제는 소문으로만 듣던 당신들의 능력을 믿을 수 있겠소."
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 150000;
						}

						mParty.Etc[14]++;
					}
					else if (mParty.Etc[14] == 3)
						Talk(" 이번에는 대륙의 동쪽에 있는 락업 동굴 속의 거대 드래곤을 물리쳐 주시오.");
					else if (mParty.Etc[14] == 4) {
						Talk(new string[] {
							"역시 위대한 영웅이오!!",
							$" [color={RGB.LightCyan}][[ EXP + 300000 ][/color]",
							"여기에 스왐프 키가 있소."
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 300000;
						}

						mParty.Etc[14]++;
					}
					else if (mParty.Etc[14] == 5) {
						Talk(" 스왐프 키는 가이아 테라 의 스왐프 게이트를 여는데 사용되오. 거기서 늪의 대륙으로 가시오. 늪의 대륙은 완전한 적들의 소굴이므로 매우 주의하시오.");
					}
				}
			}
			else if (mParty.Map == 24) {
				if ((moveX == 10 && moveY == 21) || (moveX == 13 && moveY == 23))
					GoTrainingCenter();
				else if ((moveX == 32 && moveY == 34) || (moveX == 34 && moveY == 36))
					GoGrocery();
				else if ((moveX == 32 && moveY == 20) || (moveX == 36 && moveY == 23) || (moveX == 39 && moveY == 22))
					GoWeaponShop();
				else if ((moveX == 14 && moveY == 35) || (moveX == 10 && moveY == 37) || (moveX == 13 && moveY == 39))
					GoHospital();
				else if (moveX == 16 && moveY == 14)
					Talk(" 에인션트 이블은 우리의 구세주였습니다.");
				else if (moveX == 17 && moveY == 9)
					Talk(" 이 세상을 이렇게 불행하게 한자는 바로 안 영기라는 프로그래머입니다.");
				else if (moveX == 19 && moveY == 12)
					Talk(" 우리는 여태껏 당신들이 오기를 기다렸습니다.");
				else if (moveX == 26 && moveY == 7)
					Talk(" 에인션트 이블은 결코 평판과 같이 나쁜 존재가 아닙니다.");
				else if (moveX == 30 && moveY == 12)
					Talk(" 당신들은 분명히 우리들을 밖으로 나가게 해줄 것입니다.");
				else if (moveX == 32 && moveY == 9) {
					Talk(new string[] {
						" 이 사람들은 잘 모르겠지만 사실 나는 이 게임의 제작자인 안 영기요.",
						" 나는 여태껏 계속 당신들이 가는 도시마다 주민으로 가장한 채 당신들을 지켜보았소. 이 게임의 버그를 찾거나 난이도를 조절하기 위해서 말이요." +
						" 만약 당신들이 네크로만서를 물리친다면 내가 마지막으로 당신 앞에 나타나겠소.",
						" 그럼, 이만 나는 가보겠소."
					});

					mSpecialEvent = SpecialEventType.MeetAhnInLastShelter;
				}
			}
			else if (mParty.Map == 27) {
				if (moveX == 14 && moveY == 5)
				{
					mSpecialEvent = SpecialEventType.MeetAhnInAnotherLore;

					Talk(" 당신 앞의 사람이 갑자기 어떤 남자로 변하였다.");
				}
				else if (moveX == 9 && moveY == 13)
				{
					Talk(" 당신이 유골에 다가서자 어디선가 소리가 들려왔다.");

					mSpecialEvent = SpecialEventType.MeetWillOfDeneb;
				}
				else if (moveX == 9 && moveY == 17)
				{
					Talk(" 당신이 유골에 다가서자 어디선가 소리가 들려왔다.");

					mSpecialEvent = SpecialEventType.MeetWillOfSirius;
				}
				else if (moveX == 9 && moveY == 29)
				{
					Talk(" 당신이 유골에 다가서자 어디선가 소리가 들려왔다.");

					mSpecialEvent = SpecialEventType.MeetWillOfAlbireo;
				}
				else if (moveX == 20 && moveY == 31)
				{
					Talk(" 당신이 유골에 다가서자 어디선가 소리가 들려왔다.");

					mSpecialEvent = SpecialEventType.MeetWillOfCanopus;
				}
				else if (moveX == 20 && moveY == 21)
				{
					Talk(" 당신이 유골에 다가서자 어디선가 소리가 들려왔다.");

					mSpecialEvent = SpecialEventType.MeetWillOfArcturus;
				}
				else if (moveX == 20 && moveY == 11)
				{
					AppendText(" 당신 앞에 있는 유골의 손에는 어떤 두루마리가 쥐어져 있었다.");

					ShowMenu(MenuMode.ReadScroll, new string[] {
						"그 문서를 읽어 보고 싶다",
						"그냥 지나가겠다"
					});
				}
				else
				{
					AppendText(" 당신이 유골에 다가가자 재로 변하였다.");
					InvokeAnimation(AnimationType.Remains7, moveX, moveY);
				}
			}
		}
		private async Task EnterSwampGate()
		{
			mAnimationEvent = AnimationType.None;
			mAnimationFrame = 0;

			mParty.Map = 13;
			mParty.XAxis = 80;
			mParty.YAxis = 94;

			await RefreshGame();
		}

		private async void InvokeAnimation(AnimationType animationEvent, int aniX = 0, int aniY = 0) {
			void RestRemains(int x, int y) {
				for (var i = 1; i <= 30; i++) {
					Task.Delay(i * 2).Wait();
					mMapLayer[x + mMapWidth * y] = 48;
					Task.Delay((31 - i) * 2).Wait();
					mMapLayer[x + mMapWidth * y] = 35;
				}

				mMapLayer[x + mMapWidth * y] = 35;
			}

			mAnimationEvent = animationEvent;

			var animationTask = Task.Run(() =>
			{
				if (mAnimationEvent == AnimationType.MeetSkeleton) {
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						if (i < 4)
							Task.Delay(500).Wait();
					}
				}
				if (mAnimationEvent == AnimationType.Remains1)
					RestRemains(9, 13);
				else if (mAnimationEvent == AnimationType.Remains2)
					RestRemains(9, 17);
				else if (mAnimationEvent == AnimationType.Remains3)
					RestRemains(9, 29);
				else if (mAnimationEvent == AnimationType.Remains4)
					RestRemains(20, 31);
				else if (mAnimationEvent == AnimationType.Remains5)
					RestRemains(20, 21);
				else if (mAnimationEvent == AnimationType.Remains6)
					RestRemains(20, 11);
				else if (mAnimationEvent == AnimationType.Remains7)
					RestRemains(aniX, aniY);
				else if (mAnimationEvent == AnimationType.Hydra)
				{
					for (var i = 1; i <= 2; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}

					for (var i = 3; i <= 5; i++)
					{
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}

					mAnimationFrame = 6;
				}
				else if (mAnimationEvent == AnimationType.Minotaur || mAnimationEvent == AnimationType.Minotaur2)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.HugeDragon)
				{
					mFace = 6;
					for (var x = 0; x < 6; x++)
					{
						mParty.XAxis++;
						Task.Delay(200).Wait();
					}

					mFace = 4;
					for (var y = mParty.YAxis + 1; y < 13; y++)
					{
						mParty.YAxis = y;
						Task.Delay(200).Wait();
					}

					mFace = 5;
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						if (i < 4)
							Task.Delay(2000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.HugeDragon)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						if (i < 4)
							Task.Delay(2000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.Dragon3)
				{
					for (var i = 1; i <= 6; i++)
					{
						mAnimationFrame = i;
						if (i < 6)
							Task.Delay(2000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.EnterSwampGate)
				{
					for (var i = 1; i < 6; i++)
					{
						Task.Delay(1000).Wait();
						mAnimationFrame = i;
					}
				}
				else if (mAnimationEvent == AnimationType.SwampGatePyramid)
				{
					while (mParty.XAxis != 80)
					{
						Task.Delay(1500).Wait();
						if (mParty.XAxis > 80)
						{
							mFace = 7;
							mParty.XAxis--;
						}
						else if (mParty.XAxis < 80)
						{
							mFace = 6;
							mParty.XAxis++;
						}
					}

					while (mParty.YAxis != 76)
					{
						Task.Delay(1500).Wait();
						if (mParty.YAxis > 76)
						{
							mFace = 5;
							mParty.YAxis--;
						}
						else if (mParty.YAxis < 76)
						{
							mFace = 4;
							mParty.YAxis++;
						}
					}
				}
				else if (mAnimationEvent == AnimationType.AstralMud)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						if (i < 4)
							Task.Delay(1500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.PanzerViper)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						if (i < 4)
							Task.Delay(2000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.BlackKnight)
				{
					for (var i = 1; i <= 10; i++)
					{
						mAnimationFrame = i;

						if (mAnimationFrame == 10)
						{
							xEnemyOffset = 0;
							yEnemyOffset = -1;
						}
						else
						{
							xEnemyOffset = mRand.Next(9) - 4;
							yEnemyOffset = mRand.Next(9) - 4;

							if (xEnemyOffset == 0 && yEnemyOffset == 0)
								yEnemyOffset = -1;
						}

						if (i < 10)
							Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.EnterChamberOfNecromancer) {
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;

						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.FinalBattle) {
					mFace = 5;
					Task.Delay(1500).Wait();
					for (var i = 0; i < 3; i++) {
						mParty.YAxis--;
						Task.Delay(1500).Wait();
					}

					mFace = 6;
					while (mParty.XAxis < 25) {
						mParty.XAxis++;
						Task.Delay(1500).Wait();
					}

					mFace = 5;

					for (var i = 1; i <= 11; i++)
					{
						mAnimationFrame = i;

						if (i < 11)
						{
							if (i > 6)
								Task.Delay(2000).Wait();
							else
								Task.Delay(1000).Wait();
						}
					}
				}
			});

			await animationTask;

			if (mAnimationEvent == AnimationType.MeetSkeleton) {
				AppendText(new string[] { $" 나는 스켈레톤이라 불리는 종족의 사람이오.",
						" 우리 종족의 사람들은 나를 제외하고는 모두 네크로만서에게 굴복하여 그의 부하가 되었지만 나는 그렇지 않소." +
						" 나는 네크로만서의 영향을 피해서 이곳 로어 성으로 왔지만 나의 혐오스러운 생김새 때문에 이곳 사람들에게 배척되어서 지금은 어디로도 갈 수 없는 존재가 되었소." +
						" 이제 나에게 남은 것은 네크로만서의 타도밖에 없소. 그래서 당신들의 일행에 끼고 싶소."
					});

				ShowMenu(MenuMode.JoinSkeleton, new string[] {
						"당신을 환영하오.",
						"미안하지만 안되겠소."
					});
			}
			else if (mAnimationEvent == AnimationType.Hydra) {
				ContinueText.Visibility = Visibility.Visible;
				mSpecialEvent = SpecialEventType.BattleHydra;
			}
			else if (mAnimationEvent == AnimationType.Minotaur) {
				ContinueText.Visibility = Visibility.Visible;
				mSpecialEvent = SpecialEventType.BattleMinotaur;
			}
			else if (mAnimationEvent == AnimationType.Minotaur2)
			{
				ContinueText.Visibility = Visibility.Visible;
				mSpecialEvent = SpecialEventType.BattleMinotaur2;
			}
			else if (mAnimationEvent == AnimationType.HugeDragon) {
				ContinueText.Visibility = Visibility.Visible;
				mSpecialEvent = SpecialEventType.BattleHugeDragon;
			}
			else if (mAnimationEvent == AnimationType.Dragon3) {
				ContinueText.Visibility = Visibility.Visible;
				mSpecialEvent = SpecialEventType.BattleThreeDragon;
			}
			else if (mAnimationEvent == AnimationType.EnterSwampGate) {
				if ((mParty.Etc[34] & (1 << 5)) == 0)
				{
					Talk($"[color={RGB.White}] 스왐프 게이트로 들어가고 있는 당신에게 허공에서 갑자가 누군가가 말을 꺼낸다[/color]");
					mSpecialEvent = SpecialEventType.EnterSwampGate;
				}
				else
					await EnterSwampGate();
			}
			else if (mAnimationEvent == AnimationType.SwampGatePyramid) {
				for (var y = 70; y < 81; y++) {
					for (var x = 75; x < 86; x++) {
						if (mMapLayer[x + mMapWidth * y] == 42)
							mMapLayer[x + mMapWidth * y] = 51;
					}
				}

				mFace = 5;

				Talk($"[color={RGB.LightCyan}] 여기에는 기묘한 피라미드가 있었다[/color]");
				mSpecialEvent = SpecialEventType.SwampGatePyramid;

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.AstralMud) {
				Talk($" [color={RGB.LightMagenta}]나는 네크로만서와 함께 다른 차원에서 내려온 아스트랄 머드 이다. 여기는 그가 세운 최고의 동굴이자 너가 마지막으로 거칠 동굴이다." +
				" 나를 만만하게 보지 마라. 다른 차원의 능력들을 너가 맛볼 기회를 가진다는 것에 대해 고맙게 생각하기 바란다. 하하하 ...[/color]");

				mSpecialEvent = SpecialEventType.BattleAstralMud;
			}
			else if (mAnimationEvent == AnimationType.PanzerViper) {
				Talk(new string[] { $" [color={RGB.LightMagenta}] 여기까지 잘도 왔구나. 나의 임무는 너희 같은 쓰레기들 때문에 네크로만서님이 수고하시지 않도록 미리 처단해 버리는 것이다.[/color]" });

				mSpecialEvent = SpecialEventType.BattlePanzerViper;
			}
			else if (mAnimationEvent == AnimationType.BlackKnight) {
				Talk(new string[] { $" 두말이 필요 없다. 덤벼라!![/color]" });

				mSpecialEvent = SpecialEventType.BattleBlackKnight;
			}
			else if (mAnimationEvent == AnimationType.EnterChamberOfNecromancer) {
				for (var y = 15; y < 19; y++) {
					for (var x = 23; x < 26; x++)
						mMapLayer[x + mMapWidth * y] = 16;
				}

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.FinalBattle) {
				Talk(new string[] {
					" 당신들이 나를 없애겠다고 온 자들인가?",
					" 그럼 예의를 갖추고 소개를 하지. 당신의 오른쪽의 사람은 아키몽크라고 하며 맨손을 사용하는 무예의 일인자로 통하지." +
					" 그리고 당신의 정면의 사람은 아키메이지라고 하는 마법사 중의 마법사이라네. 당신들은 우리 셋보다도 숫자가 많군." +
					" 그렇다면 나도 그것에 대비를 해야겠지. 내가 여기서 약간의 인원을 늘린다고 너무 섭섭하게 생각말게. 그렇다면 이제 서로의 실력을 겨뤄볼 시간이 다 되었나 보군." +
					" 당신의 행운을 빌겠네."
				});

				mSpecialEvent = SpecialEventType.BattleNecromancer;
			}
			else {
				mAnimationEvent = AnimationType.None;
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

			var playerX = mParty.XAxis;
			var playerY = mParty.YAxis;

			var xOffset = 0;
			var yOffset = 0;
			if (mTelescopeXCount != 0) {
				if (mTelescopeXCount < 0)
					xOffset = -(mTelescopePeriod - Math.Abs(mTelescopeXCount));
				else
					xOffset = mTelescopePeriod - Math.Abs(mTelescopeXCount);
			}

			if (mTelescopeYCount != 0)
			{
				if (mTelescopeYCount < 0)
					yOffset = -(mTelescopePeriod - Math.Abs(mTelescopeYCount));
				else
					yOffset = mTelescopePeriod - Math.Abs(mTelescopeYCount);
			}

			var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(64 * (playerX - 4 + xOffset), 64 * (playerY - 4 + yOffset)));
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
					if (mMapLayer != null) {
						for (int i = 0; i < mMapLayer.Length; ++i)
						{
							DrawTile(sb, mMapLayer, i, playerX, playerY);
						}
					}
				}

				if (mCharacterTiles != null) {
					if (mAnimationEvent == AnimationType.EnterSwampGate)
					{
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - mAnimationFrame), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.EnterChamberOfNecromancer)
					{
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY + (5 - mAnimationFrame)), Vector4.One);
					}
					else
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), Vector4.One);

					if (mSpecialEvent == SpecialEventType.MeetLoreSolider)
						mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(50, 71), Vector4.One);
					else if (mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore || mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore2)
						mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(14, 5), Vector4.One);
					if (mSpecialEvent == SpecialEventType.MeetAhnInLastShelter)
						mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(32, 9), Vector4.One);
					else if (mAnimationEvent == AnimationType.Hydra) {
						if (mAnimationFrame > 0)
						{
							int firstHead;
							if (mAnimationFrame == 6)
								firstHead = 3;
							else
								firstHead = mAnimationFrame > 2 ? 2 : mAnimationFrame;

							for (var i = 4; i > 4 - firstHead; i--)
								mCharacterTiles.Draw(sb, 8, mCharacterTiles.SpriteSize * new Vector2(playerX - i, playerY), Vector4.One);
							mCharacterTiles.Draw(sb, 12, mCharacterTiles.SpriteSize * new Vector2(playerX - (4 - firstHead), playerY), Vector4.One);

							if (mAnimationFrame > 3)
							{
								int remainHead = mAnimationFrame > 5 ? 3 : mAnimationFrame - 2;
								for (var i = 4; i > 4 - remainHead; i--)
								{
									mCharacterTiles.Draw(sb, 8, mCharacterTiles.SpriteSize * new Vector2(playerX - i, playerY - 1), Vector4.One);
									mCharacterTiles.Draw(sb, 8, mCharacterTiles.SpriteSize * new Vector2(playerX - i, playerY + 1), Vector4.One);
								}
								mCharacterTiles.Draw(sb, 12, mCharacterTiles.SpriteSize * new Vector2(playerX - (4 - remainHead), playerY - 1), Vector4.One);
								mCharacterTiles.Draw(sb, 12, mCharacterTiles.SpriteSize * new Vector2(playerX - (4 - remainHead), playerY + 1), Vector4.One);
							}
						}
						else
							Console.WriteLine("히드라 안그리기");
					}
					else if (mAnimationEvent == AnimationType.Minotaur || mAnimationEvent == AnimationType.Minotaur2) {
						if (mAnimationFrame > 0)
							mCharacterTiles.Draw(sb, 9, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (5 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.HugeDragon) {
						if (mAnimationFrame > 0) {
							mCharacterTiles.Draw(sb, 10, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - (6 - mAnimationFrame)), Vector4.One);
							mCharacterTiles.Draw(sb, 14, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (6 - mAnimationFrame)), Vector4.One);
							mCharacterTiles.Draw(sb, 18, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - (6 - mAnimationFrame)), Vector4.One);


							mCharacterTiles.Draw(sb, 11, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - (5 - mAnimationFrame)), Vector4.One);
							mCharacterTiles.Draw(sb, 15, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (5 - mAnimationFrame)), Vector4.One);
							mCharacterTiles.Draw(sb, 19, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - (5 - mAnimationFrame)), Vector4.One);
						}
					}
					else if (mAnimationEvent == AnimationType.Dragon3)
					{
						if (mAnimationFrame > 0)
						{
							if (mAnimationFrame >= 4) {
								mCharacterTiles.Draw(sb, 10, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - (9 - mAnimationFrame)), Vector4.One);
								mCharacterTiles.Draw(sb, 14, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (9 - mAnimationFrame)), Vector4.One);
								mCharacterTiles.Draw(sb, 18, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - (9 - mAnimationFrame)), Vector4.One);


								mCharacterTiles.Draw(sb, 11, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - (8 - mAnimationFrame)), Vector4.One);
								mCharacterTiles.Draw(sb, 15, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (8 - mAnimationFrame)), Vector4.One);
								mCharacterTiles.Draw(sb, 19, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - (8 - mAnimationFrame)), Vector4.One);
							}

							var aniYOffset = mAnimationFrame > 4 ? 2 : 6 - mAnimationFrame;
								
							mCharacterTiles.Draw(sb, 10, mCharacterTiles.SpriteSize * new Vector2(playerX - 3, playerY - aniYOffset), Vector4.One);
							mCharacterTiles.Draw(sb, 14, mCharacterTiles.SpriteSize * new Vector2(playerX - 2, playerY - aniYOffset), Vector4.One);
							mCharacterTiles.Draw(sb, 18, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - aniYOffset), Vector4.One);


							mCharacterTiles.Draw(sb, 11, mCharacterTiles.SpriteSize * new Vector2(playerX - 3, playerY - aniYOffset + 1), Vector4.One);
							mCharacterTiles.Draw(sb, 15, mCharacterTiles.SpriteSize * new Vector2(playerX - 2, playerY - aniYOffset + 1), Vector4.One);
							mCharacterTiles.Draw(sb, 19, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY - aniYOffset + 1), Vector4.One);

							mCharacterTiles.Draw(sb, 10, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - aniYOffset), Vector4.One);
							mCharacterTiles.Draw(sb, 14, mCharacterTiles.SpriteSize * new Vector2(playerX + 2, playerY - aniYOffset), Vector4.One);
							mCharacterTiles.Draw(sb, 18, mCharacterTiles.SpriteSize * new Vector2(playerX + 3, playerY - aniYOffset), Vector4.One);


							mCharacterTiles.Draw(sb, 11, mCharacterTiles.SpriteSize * new Vector2(playerX + 1, playerY - aniYOffset + 1), Vector4.One);
							mCharacterTiles.Draw(sb, 15, mCharacterTiles.SpriteSize * new Vector2(playerX + 2, playerY - aniYOffset + 1), Vector4.One);
							mCharacterTiles.Draw(sb, 19, mCharacterTiles.SpriteSize * new Vector2(playerX + 3, playerY - aniYOffset + 1), Vector4.One);
						}
					}
					else if (mAnimationEvent == AnimationType.AstralMud) {
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(playerX - (5 - mAnimationFrame), playerY - (5 - mAnimationFrame)), Vector4.One);
						mCharacterTiles.Draw(sb, 28, mCharacterTiles.SpriteSize * new Vector2(playerX - (4 - mAnimationFrame), playerY - (5 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.PanzerViper) {
						mCharacterTiles.Draw(sb, 23, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (5 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.BlackKnight) {
						mCharacterTiles.Draw(sb, 22, mCharacterTiles.SpriteSize * new Vector2(playerX + xEnemyOffset, playerY + yEnemyOffset), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.FinalBattle) {
						if (mAnimationFrame > 0)
						{
							var archiMonkOffset = mAnimationFrame > 3 ? 3 : mAnimationFrame;

							mCharacterTiles.Draw(sb, 26, mCharacterTiles.SpriteSize * new Vector2(playerX + 2, playerY - (5 - archiMonkOffset)), Vector4.One);

							if (mAnimationFrame >= 4) {
								var archiMageOffset = mAnimationFrame > 6 ? 3 : mAnimationFrame - 3;

								mCharacterTiles.Draw(sb, 25, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (5 - archiMageOffset)), Vector4.One);
							}

							if (mAnimationFrame >= 7) {
								var necromancerXOffset = mAnimationFrame > 9 ? 3 : mAnimationFrame - 6;
								var necromancerYOffset = mAnimationFrame > 9 ? mAnimationFrame - 9 : 0;

								mCharacterTiles.Draw(sb, 16, mCharacterTiles.SpriteSize * new Vector2(playerX - (5 - necromancerXOffset), playerY - 3 + necromancerYOffset), Vector4.One);
								mCharacterTiles.Draw(sb, 17, mCharacterTiles.SpriteSize * new Vector2(playerX - (5 - necromancerXOffset), playerY - 2 + necromancerYOffset), Vector4.One);
							}
						}
					}
				}
			}
		}

		void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index, int playerX, int playerY)
		{
			int row = index / mMapWidth;
			int column = index % mMapWidth;

			Vector4 tint = Vector4.One;

			if (mMapTiles != null)
			{
				if (mAnimationEvent == AnimationType.MeetSkeleton && mAnimationFrame > 0 && column == playerX && playerY - row == 5 - mAnimationFrame)
					mMapTiles.Draw(sb, 48, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				else if (mSpecialEvent == SpecialEventType.MeetLoreSolider && (index == 50 + mMapWidth * 71))
					mMapTiles.Draw(sb, 44, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				else if ((mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore || mSpecialEvent == SpecialEventType.MeetAhnInAnotherLore2) && (index == 14 + mMapWidth * 5))
					mMapTiles.Draw(sb, 44, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				else if (mSpecialEvent == SpecialEventType.MeetAhnInLastShelter && (index == 32 + mMapWidth * 9))
					mMapTiles.Draw(sb, 47, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				else
				{
					var mapIdx = 56;

					if (mPosition == PositionType.Town)
						mapIdx = 0;
					else if (mPosition == PositionType.Keep)
						mapIdx *= 1;
					else if (mPosition == PositionType.Ground)
						mapIdx *= 2;
					else if (mPosition == PositionType.Den)
						mapIdx *= 3;
					

					byte tileIdx = layer[index];

					if (mPenetration) {
						if ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && tileIdx == 52)
							tileIdx = 0;
					}
					else if (tileIdx == 0) {
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
			Uri musicUri;
			if (1 <= mParty.Map && mParty.Map <= 5)
			{
				mPosition = PositionType.Ground;
				musicUri = new Uri("ms-appx:///Assets/ground.mp3");
			}
			else if ((6 <= mParty.Map && mParty.Map <= 10) || mParty.Map == 24 || (26 <= mParty.Map && mParty.Map <= 27))
			{
				mPosition = PositionType.Town;
				musicUri = new Uri("ms-appx:///Assets/town.mp3");
			}
			else if ((11 <= mParty.Map && mParty.Map <= 20) || mParty.Map == 25)
			{
				mPosition = PositionType.Den;
				musicUri = new Uri("ms-appx:///Assets/den.mp3");
			}
			else
			{
				mPosition = PositionType.Keep;
				musicUri = new Uri("ms-appx:///Assets/keep.mp3");
			}

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

			BGMPlayer.Source = musicUri;
		}

		private async Task LoadEnemyData() {
			var enemyFileFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/enemyData.dat"));
			mEnemyDataList = JsonConvert.DeserializeObject<List<EnemyData>>(await FileIO.ReadTextAsync(enemyFileFile));
		}

		private async Task<bool> LoadFile(int id = 0) {
			mLoading = true;

			var storageFolder = ApplicationData.Current.LocalFolder;

			var idStr = "";
			if (id > 0)
				idStr = id.ToString();

			var saveFile = await storageFolder.CreateFileAsync($"loreSave{idStr}.dat", CreationCollisionOption.OpenIfExists);
			var saveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(saveFile));

			if (saveData == null)
			{
				mLoading = false;
				return false;
			}

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

			return true;
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


		private Lore GetMemberFromEnemy(int id) {
			var enemy = mEnemyDataList[id];

			var player = new Lore()
			{
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

			return player;
		}

		private void JoinMemberFromEnemy(int id)
		{
			var player = GetMemberFromEnemy(id);

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

				ShowGameOver(new string[] { "일행은 모험 중에 모두 목숨을 잃었다." });
				mTriggeredDownEvent = true;
			}
		}


		private void ShowExitMenu() {
			AppendText(new string[] { $"[color={RGB.LightCyan}]여기서 나가기를 원합니까?[/color]" });

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
					AppendText(new string[] { $"          [color={RGB.White}]와이번 가는길[/color]" }, true);
				else if ((x == 28 && y == 49) || (x == 34 && y == 71))
				{

					AppendText(new string[] { $"  [color={RGB.White}]북쪽 :[/color]",
						$"       [color={RGB.White}]배리안트 피플즈 가는 길[/color]",
						$"  [color={RGB.White}]남쪽 :[/color]",
						$"       [color={RGB.White}]가이아 테라 가는 길[/color]" }, true);
				}
				else if (x == 43 && y == 76)
				{
					AppendText(new string[] { $"  [color={RGB.White}]북동쪽 :[/color]",
						$"       [color={RGB.White}]퀘이크 가는 길[/color]",
						$"  [color={RGB.White}]남서쪽 :[/color]",
						$"       [color={RGB.White}]가이아 테라 가는 길[/color]" }, true);
				}
			}
			else if (mParty.Map == 6)
			{
				if (x == 50 && y == 83)
				{
					AppendText(new string[] { $"       [color={RGB.White}]여기는[/color] '[color={RGB.LightCyan}]로어 성[/color]'",
						$"         [color={RGB.White}]여러분을 환영합니다[/color]",
						"",
						$"[color={RGB.LightMagenta}]로드 안[/color]" }, true);
				}
				else if (x == 23 && y == 30)
				{
					AppendText(new string[] { "",
						$"             [color={RGB.White}]여기는 로어 주점[/color]",
						$"       [color={RGB.White}]여러분 모두를 환영합니다 !![/color]" }, true);
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
					AppendText(new string[] { "",
					$"          [color={RGB.White}]로어 왕립 죄수 수용소[/color]" }, true);
			}
			else if (mParty.Map == 7)
			{
				if (x == 38 && y == 67)
				{
					AppendText(new string[] { $"        여기는 '[color={RGB.LightCyan}]라스트디치[/color]'성",
						"         여러분을 환영합니다" }, true);
				}
				else if (x == 38 && y == 7)
					AppendText(new string[] { $"       [color={RGB.LightRed}]여기는 피라미드의 입구[/color]" }, true);
				else if (x == 53 && y == 8)
					AppendText(new string[] { $"     [color={RGB.LightGreen}]여기는 지하 출입구의 입구[/color]" }, true);
			}
			else if (mParty.Map == 8)
			{
				if (x == 38 && y == 66)
				{
					AppendText(new string[] { $"      [color={RGB.White}]여기는 '[/color][color={RGB.LightCyan}]배리안트 피플즈[/color][color={RGB.White}]'성[/color]",
					$"    [color={RGB.White}]우리의 미덕은 굽히지 않는 용기[/color]",
					$"   [color={RGB.White}]우리는 어떤 악에도 굽히지 않는다[/color]" }, true);
				}
				else
					AppendText(new string[] { $"     [color={RGB.LightRed}]여기는 이블 씰의 입구[/color]" }, true);
			}
			else if (mParty.Map == 9)
			{
				if (x == 23 && y == 25)
					AppendText(new string[] { $"       [color={RGB.White}]여기는 국왕의 보물 창고[/color]" }, true);
				else
				{
					AppendText(new string[] { $"         [color={RGB.White}]여기는 '[/color][color={RGB.LightCyan}]가이아 테라[/color][color={RGB.White}]'성[/color]",
						$"          [color={RGB.White}]여러분을 환영합니다[/color]" }, true);
				}
			}
			else if (mParty.Map == 12)
			{
				if (x == 23 && y == 67)
					AppendText(new string[] { $"               [color={RGB.White}]X 는 7[/color]" }, true);
				else if (x == 26 && y == 67)
					AppendText(new string[] { $"               [color={RGB.White}]Y 는 9[/color]" }, true);
				else if (x == 24 && y == 62)
					AppendText(new string[] { $"       [color={RGB.White}]바른 문의 번호는 X + Y[/color]" }, true);
				else if (y == 55)
				{
					var j = ((x + 1) - 6) / 7 + 12;
					AppendText(new string[] { $"           [color={RGB.White}]문의 번호는 '[/color][color={RGB.LightCyan}]{j}[/color][color={RGB.White}]'[/color]" }, true);
				}
				else if (x == 25 && y == 41)
					AppendText(new string[] { $"            [color={RGB.White}]Z 는 2 * Y + X[/color]" }, true);
				else if (x == 25 && y == 32)
				{
					AppendText(new string[] { $"        [color={RGB.White}]패스코드 x 패스코드 는 Z 라면[/color]",
						"",
						$"            [color={RGB.White}]패스코드는 무엇인가?[/color]" }, true);
				}
				else if (y == 28)
				{
					var j = ((x + 1) - 3) / 5 + 2;
					AppendText(new string[] { $"           [color={RGB.White}]패스코드는 '[/color][color={RGB.LightCyan}]{j}[/color][color={RGB.White}]'[/color]" }, true);
				}
			}
			else if (mParty.Map == 15)
			{
				if (x == 25 && y == 62)
					AppendText(new string[] { $"            [color={RGB.White}]길의 마지막[/color]" }, true);
				else if (x == 21 && y == 14)
					AppendText(new string[] { $"     [color={RGB.White}](12,15) 로 공간이동 하시오[/color]" }, true);
				else if (x == 10 && y == 13)
					AppendText(new string[] { $"     [color={RGB.White}](13,7) 로 공간이동 하시오[/color]" }, true);
				else if (x == 26 && y == 13)
					AppendText(new string[] { $"   [color={RGB.White}]황금의 갑옷은 (45,19) 에 숨겨져있음[/color]" }, true);

			}
			else if (mParty.Map == 17)
			{
				if (x == 67 && y == 46)
					AppendText(new string[] { $"    [color={RGB.White}]하! 하! 하! 너는 우리에게 속았다[/color]" }, true);
				else if (x == 57 && y == 52)
				{
					AppendText(new string[] { $"      [color={RGB.LightGreen}]이 게임을 만든 사람[/color]",
						$"  [color={RGB.White}]: 동아 대학교 전기 공학과[/color]",
						$"        [color={RGB.White}]92 학번  안 영기[/color]" }, true);
				}
				else if (x == 50 && y == 29)
				{
					AppendText(new string[] { "", $"       [color={RGB.White}]오른쪽 : 히드라의 보물창고[/color]",
						$"         [color={RGB.White}]왼  쪽 : 히드라가 있는 방[/color]" }, true);
				}
				else if (x == 65 && y == 12)
					AppendText($"     [color={RGB.White}]일찌감치 이곳 탐험을 포기해라[/color]", true);
				else if (x == 8 && y == 27)
					AppendText($"[color={RGB.White}]         위쪽이 진짜 보물창고임[/color]", true);
			}
			else if (mParty.Map == 19)
			{
				AppendText(new string[] { $"       [color={RGB.White}]이 길을 통과하고자 하는 사람은[/color]",
					$"     [color={RGB.White}]양측의 늪 속에 있는 레버를 당기시오[/color]" }, true);
			}
			else if (mParty.Map == 23)
			{
				AppendText(new string[] { $"      [color={RGB.White}](25,27)에 있는 레버를 움직이면[/color]",
				$"          [color={RGB.White}]성을 볼 수 있을 것이오.[/color]",
				"",
				$"             [color={RGB.LightGreen}]제작자 안 영기 씀[/color]" }, true);
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
			ApplyBattleCureAllSpell,
			CastESP,
			BattleLose,
			AskEnter,
			ChooseGoldShield,
			SwapMember,
			JoinPolaris,
			JoinDraconian,
			ChooseOedipusSpear,
			JoinLoreHunter,
			JoinRigel,
			ChooseGoldShield2,
			ChooseGoldArmor,
			JoinRedAntares,
			JoinSpica,
			QnA,
			ReadScroll,
			TeleportationDirection,
			ChooseLoadGame,
			ChooseGameOverLoadGame,
			ChooseSaveGame
		}

		private enum SpinnerType {
			None,
			TeleportationRange
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
			AlmostWin,
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
			LastDitch,
			GroundGate,
			AnotherLore,
			ValiantPeoples,
			GaiaTerra,
			Quake,
			Wivern,
			WaterField,
			Notice,
			LockUp,
			SwampKeep,
			EvilGod,
			Muddy,
			ImperiumMinor,
			EvilConcentration,
			Pyramid,
			EvilSeal,
			SwampGate,
			LastShelter,
			DungeonOfEvil,
			ChamberOfNecromancer
		}

		private enum AnimationType {
			None,
			MeetSkeleton,
			Remains1,
			Remains2,
			Remains3,
			Remains4,
			Remains5,
			Remains6,
			Remains7,
			Hydra,
			Minotaur,
			Minotaur2,
			HugeDragon,
			EnterSwampGate,
			SwampGatePyramid,
			Dragon3,
			AstralMud,
			PanzerViper,
			BlackKnight,
			EnterChamberOfNecromancer,
			FinalBattle,
		}

		private enum SpecialEventType {
			None,
			StartGame,
			MeetLoreSolider,
			MeetSkeleton,
			RefuseJoinSkeleton,
			MeetDraconian,
			MeetDraconian2,
			MeetDraconian3,
			MeetAncientEvil,
			MeetAncientEvil2,
			MeetAncientEvil3,
			EnterSwampGate,
			EnterSwampGate2,
			EnterSwampGate3,
			EnterSwampGate4,
			FindOedipusSpear,
			FindOedipusSpear2,
			MajorMummyRoom,
			MeetRigel,
			EnterEvilConcentration,
			FindGoldenShield,
			FindGoldenArmor,
			BattleHydra,
			AfterBattleHydra,
			BattleArchiGagoyle,
			MeetRedAntares,
			MeetRedAntares2,
			MeetRedAntares3,
			BattleMinotaur,
			MeetSpica,
			BattleHugeDragon,
			BattleCrabGod,
			BattleMinotaur2,
			BattleThreeDragon,
			BattleAstralMud,
			ExitImperiumMinor,
			BattleDeathKnight,
			MeetFaceNecromancer,
			BattleDual,
			BattleFackNecromancer,
			AfterBattleFakeNecromancer,
			FailRunawayBattleFakeNecromancer,
			BattlePanzerViper,
			AfterBattlePanzerViper,
			AfterLordAhnAndAncientEvil,
			FailRunawayBattleNecromancer,
			BattleNecromancer,
			MeetAhnInLastShelter,
			MeetAhnInAnotherLore,
			MeetAhnInAnotherLore2,
			MeetWillOfDeneb,
			MeetWillOfDeneb2,
			MeetWillOfDeneb3,
			MeetWillOfDeneb4,
			MeetWillOfDeneb5,
			MeetWillOfDeneb6,
			MeetWillOfSirius,
			MeetWillOfSirius2,
			MeetWillOfSirius3,
			MeetWillOfAlbireo,
			MeetWillOfAlbireo2,
			MeetWillOfCanopus,
			MeetWillOfCanopus2,
			MeetWillOfCanopus3,
			MeetWillOfArcturus,
			MeetWillOfArcturus2,
			MeetWillOfArcturus3,
			SwampGatePyramid,
			SwampGatePyramid2,
			SwampGatePyramid3,
			SwampGatePyramid4,
			SwampGatePyramid5,
			SwampGatePyramid6,
			ReadScroll,
			BattleWivern,
			BattleExitSwampGate, 
			DefeatAstralMud,
			EnterImperiumMinor,
			EnterDungeonOfEvil,
			KillDraconian,
			BattleBlackKnight,
			BattlePrison,
			RunawayMadJoe,
			Ending
		}

		private class HealthTextBlock {
			private TextBlock mName;
			private TextBlock mPoison;
			private TextBlock mUnconscious;
			private TextBlock mDead;

			public HealthTextBlock(TextBlock name, TextBlock poison, TextBlock unconscious, TextBlock dead) {
				mName = name;
				mPoison = poison;
				mUnconscious = unconscious;
				mDead = dead;
			}

			public void Update(string name, int poison, int unconscious, int dead) {
				mName.Text = name;
				mPoison.Text = poison.ToString();
				mUnconscious.Text = unconscious.ToString();
				mDead.Text = dead.ToString();
			}

			public void Clear() {
				mName.Text = "";
				mPoison.Text = "";
				mUnconscious.Text = "";
				mDead.Text = "";
			}
		}
	}

}
