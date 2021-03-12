using Microsoft.Graphics.Canvas;
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
				else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right)
				{
					void MovePlayer(int moveX, int moveY)
					{
						mParty.XAxis = moveX;
						mParty.YAxis = moveY;
					}

					var x = mParty.XAxis;
					var y = mParty.YAxis;

					if (args.VirtualKey == VirtualKey.Up)
					{
						y--;
						if (mPosition == PositionType.Town)
							mFace = 1;
						else
							mFace = 5;
					}
					else if (args.VirtualKey == VirtualKey.Down)
					{
						y++;
						if (mPosition == PositionType.Town)
							mFace = 0;
						else
							mFace = 4;
					}
					else if (args.VirtualKey == VirtualKey.Left)
					{
						x--;
						if (mPosition == PositionType.Town)
							mFace = 3;
						else
							mFace = 7;
					}
					else if (args.VirtualKey == VirtualKey.Right)
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
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		private int AppendText(RichTextBlock textBlock, Paragraph paragraph, int prevLen, string text)
		{
			var totalLen = prevLen;
			var startIdx = 0;
			while ((startIdx = text.IndexOf("[")) >= 0)
			{
				var preRun = new Run();
				preRun.Text = text.Substring(0, startIdx);
				totalLen += preRun.Text.Length;
				text = text.Substring(startIdx + 1);

				paragraph.Inlines.Add(preRun);
				textBlock.TextHighlighters.Add(new TextHighlighter()
				{
					Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x95, 0x95, 0x95)),
					Background = new SolidColorBrush(Colors.Transparent),
					Ranges = { new TextRange()
										{
											StartIndex = prevLen,
											Length = preRun.Text.Length
										}
									}
				});

				startIdx = text.IndexOf("]");
				if (startIdx < 0)
					break;

				var tag = text.Substring(0, startIdx);
				text = text.Substring(startIdx + 1);
				var tagData = tag.Split("=");

				var endTag = $"[/{tagData[0]}]";
				startIdx = text.IndexOf(endTag);

				if (startIdx < 0)
					break;


				if (tagData[0] == "color" && tagData.Length > 1 && tagData[1].Length == 6)
				{
					var tagRun = new Run();
					tagRun.Text = text.Substring(0, startIdx);

					paragraph.Inlines.Add(tagRun);
					textBlock.TextHighlighters.Add(new TextHighlighter()
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

				text = text.Substring(startIdx + endTag.Length);
			}

			var run = new Run();
			run.Text = text;

			paragraph.Inlines.Add(run);
			textBlock.TextHighlighters.Add(new TextHighlighter()
			{
				Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x95, 0x95, 0x95)),
				Background = new SolidColorBrush(Colors.Transparent),
				Ranges = { new TextRange()
					{
						StartIndex = totalLen,
						Length = run.Text.Length
					}
				}
			});

			totalLen += run.Text.Length;

			return totalLen;
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			int GetPrevTextLength() {
				var strLen = 0;

				foreach (Paragraph prevParagraph in DialogText.Blocks)
				{
					foreach (Run prevRun in prevParagraph.Inlines)
					{
						strLen += prevRun.Text.Length;
					}
				}

				return strLen;
			}

			if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
			{
				if (mTalkMode == 0)
				{
					DialogText.TextHighlighters.Clear();
					DialogText.Blocks.Clear();

					var paragraph = new Paragraph();
					DialogText.Blocks.Add(paragraph);

					if ((mParty.Etc[29] & 1) == 1)
						AppendText(DialogText, paragraph, 0, "행운을 빌겠소 !!!");
					else if (mParty.Etc[9] < 3)
						AppendText(DialogText, paragraph, 0, "저희 성주님을 만나 보십시오.");
					else
					{
						AppendText(DialogText, paragraph, 0, "당신은 이 게임 세계에 도전하고 싶습니까 ?\r\n(Y/n)");

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
				}
				else if (mTalkMode == 1 && key == VirtualKey.Y) {
					var paragraph = new Paragraph();
					DialogText.Blocks.Add(paragraph);

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

					var strLen = GetPrevTextLength();
					strLen = AppendText(DialogText, paragraph, strLen, "\r\n예.\r\n\r\n");
					AppendText(DialogText, paragraph, strLen, "이제부터 당신은 진정한 이 세계에 발을 디디게 되는 것입니다.");

					ContinueText.Visibility = Visibility.Visible;
					mParty.Etc[29] = mParty.Etc[29] | 1;
					
					mTalkMode = 0;
				}
				else if (mTalkMode == 1 && key == VirtualKey.N)
				{
					var paragraph = new Paragraph();
					DialogText.Blocks.Add(paragraph);

					var strLen = GetPrevTextLength();
					strLen = AppendText(DialogText, paragraph, strLen, "\r\n아니오.\r\n\r\n");
					AppendText(DialogText, paragraph, strLen, "다시 생각 해보십시오.");

					ContinueText.Visibility = Visibility.Visible;
					mTalkMode = 0;
				}
			}
			else if (moveX == 50 && moveY == 27)
			{
				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();

				var paragraph = new Paragraph();
				DialogText.Blocks.Add(paragraph);

				if (mTalkMode == 0)
				{
					var strLen = 0;
					if (mParty.Etc[9] == 0)
					{
						strLen = AppendText(DialogText, paragraph, strLen, "나는 [color=62e4f2]로드 안[/color]이오.\r\n");
						AppendText(DialogText, paragraph, strLen, "이제부터 당신은 이 게임에서 새로운 인물로서 생을 시작하게 될것이오. 그럼 나의 이야기를 시작하겠소.");
						ContinueText.Visibility = Visibility.Visible;

						mParty.Etc[9]++;

						mTalkMode = 1;
						mTalkX = moveX;
						mTalkY = moveY;
					}
					else if (mParty.Etc[9] == 3) {
						strLen = AppendText(DialogText, paragraph, strLen, " 대륙의 남서쪽에 있는 '[color=62e4f2]메나스[/color]'를 탐사해 주시오.");
						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (mTalkMode == 1) {
					var strLen = 0;
					strLen = AppendText(DialogText, paragraph, strLen, " 이 세계는 내가 통치하는 동안에는 무척 평화로운 세상이 진행되어 왔었소. 그러나 그것은 한 운명의 장난으로 무참히 깨어져 버렸소.\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, " 한날, 대기의 공간이 진동하며 난데없는 푸른 번개가 대륙들 중의 하나를 강타했소." +
					"  공간은 휘어지고 시간은 진동하며  이 세계를 공포 속으로 몰고 갔소.\r\n" +
					"  그 번개의 위력으로 그 불운한 대륙은  황폐화된 용암 대지로 변하고 말았고, 다른 하나의 대륙은 충돌시의 진동에 의해 바다 깊이 가라앉아 버렸소.\r\n");
					ContinueText.Visibility = Visibility.Visible;
					mParty.Etc[9]++;

					mTalkMode = 2;
					mTalkX = moveX;
					mTalkY = moveY;
				}
				else if (mTalkMode == 2) {
					var strLen = 0;
					strLen = AppendText(DialogText, paragraph, strLen, " 네크로맨서 의 영향력은 이미 로어 대륙까지 도달해있소.  또한 그들은 이 대륙의 남서쪽에 '메나스' 라고 불리우는 지하 동굴을 얼마전에 구축했소.  그래서, 그 동굴의 존재 때문에 우리들은 그에게 위협을 당하게 되었던 것이오.");
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
			for (var i = 0; i < mPlayerList.Count; i++) {
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
			for (var i = 0; i < mPlayerList.Count; i++)
				mPlayerHPList[i].Text = mPlayerList[i].HP.ToString();
		}

		private void DisplaySP()
		{
			for (var i = 0; i < mPlayerList.Count; i++)
				mPlayerSPList[i].Text = mPlayerList[i].SP.ToString();
		}

		private void DisplayESP()
		{
			for (var i = 0; i < mPlayerList.Count; i++)
				mPlayerESPList[i].Text = mPlayerList[i].ESP.ToString();
		}

		private void DisplayCondition()
		{
			for (var i = 0; i < mPlayerList.Count; i++)
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
			DialogText.TextHighlighters.Clear();
			DialogText.Blocks.Clear();

			var paragraph = new Paragraph();
			DialogText.Blocks.Add(paragraph);

			var strLen = 0;
			strLen = AppendText(DialogText, paragraph, strLen, "푯말에 쓰여있기로 ...\r\n\r\n");

			if (mParty.Map == 2)
			{
				if (x == 30 && y == 43)
					AppendText(DialogText, paragraph, strLen, "          WIVERN 가는길\r\n");
				else if ((x == 28 && y == 49) || (x == 34 && x == 71))
				{
					strLen = AppendText(DialogText, paragraph, strLen, "  북쪽 :\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "       VALIANT PEOPLES 가는길\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "  남쪽 :\r\n");
					AppendText(DialogText, paragraph, strLen, "       GAIA TERRA 가는길");
				}
				else if (x == 43 && y == 76)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "  북동쪽 :\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "       QUAKE 가는길\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "  남서쪽 :\r\n");
					AppendText(DialogText, paragraph, strLen, "       GAIA TERRA 가는길\r\n");
				}
			}
			else if (mParty.Map == 6)
			{
				if (x == 50 && y == 83)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "       여기는 '[color=62e4f2]CASTLE LORE[/color]'성\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "         여러분을 환영합니다\r\n\r\n");
					AppendText(DialogText, paragraph, strLen, "[color=ff00ff]Lord Ahn[/color]");
				}
				else if (x == 23 && y == 30)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "\r\n             여기는 LORE 주점\r\n");
					AppendText(DialogText, paragraph, strLen, "       여러분 모두를 환영합니다 !!");
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
					AppendText(DialogText, paragraph, strLen, "\r\n          LORE 왕립  죄수 수용소");
			}
			else if (mParty.Map == 7)
			{
				if (x == 38 && y == 67)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "        여기는 '[color=62e4f2]LASTDITCH[/color]'성\r\n");
					AppendText(DialogText, paragraph, strLen, "         여러분을 환영합니다");
				}
				else if (x == 38 && y == 7)
					AppendText(DialogText, paragraph, strLen, "       여기는 PYRAMID 의 입구");
				else if (x == 53 && y == 8)
					AppendText(DialogText, paragraph, strLen, "       여기는 PYRAMID 의 입구");
			}
			else if (mParty.Map == 8)
			{
				if (x == 38 && y == 66)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "      여기는 '[color=62e4f2]VALIANT PEOPLES[/color]'성\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "    우리의 미덕은 굽히지 않는 용기\r\n");
					AppendText(DialogText, paragraph, strLen, "   우리는 어떤 악에도 굽히지 않는다");
				}
				else
					AppendText(DialogText, paragraph, strLen, "     여기는 EVIL SEAL 의 입구");
			}
			else if (mParty.Map == 9)
			{
				if (x == 23 && y == 25)
					AppendText(DialogText, paragraph, strLen, "       여기는 국왕의 보물 창고");
				else
				{
					strLen = AppendText(DialogText, paragraph, strLen, "         여기는 '[color=62e4f2]GAIA TERRAS[/color]'성\r\n");
					AppendText(DialogText, paragraph, strLen, "          여러분을 환영합니다");
				}
			}
			else if (mParty.Map == 12)
			{
				if (x == 23 && y == 67)
					AppendText(DialogText, paragraph, strLen, "               X 는 7");
				else if (x == 26 && y == 67)
					AppendText(DialogText, paragraph, strLen, "               Y 는 9");
				else if (y == 55)
				{
					var j = (x - 6) / 7 + 12;
					AppendText(DialogText, paragraph, strLen, $"           문의 번호는 '[color=62e4f2]{j}[/color]'");
				}
				else if (x == 25 && y == 41)
					AppendText(DialogText, paragraph, strLen, "            Z 는 2 * Y + X");
				else if (x == 25 && y == 32)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "        패스코드 x 패스코드 는 Z 라면\r\n\r\n");
					AppendText(DialogText, paragraph, strLen, "            패스코드는 무엇인가 ?");
				}
				else if (y == 28)
				{
					var j = (x - 3) / 5 + 2;
					AppendText(DialogText, paragraph, strLen, $"           패스코드는 '[color=62e4f2]{j}[/color]'");
				}
			}
			else if (mParty.Map == 15)
			{
				if (x == 25 && y == 62)
					AppendText(DialogText, paragraph, strLen, "\r\n            길의 마지막");
				else if (x == 21 && y == 14)
					AppendText(DialogText, paragraph, strLen, "\r\n     (12,15) 로 공간이동 하시오");
				else if (x == 10 && y == 13)
					AppendText(DialogText, paragraph, strLen, "\r\n     (13,7) 로 공간이동 하시오");
				else if (x == 26 && y == 13)
					AppendText(DialogText, paragraph, strLen, "\r\n   황금의 갑옷은 (45,19) 에 숨겨져있음");

			}
			else if (mParty.Map == 17)
			{
				if (x == 67 && y == 46)
					AppendText(DialogText, paragraph, strLen, "\r\n    하! 하! 하!  너는 우리에게 속았다");
				else if (x == 57 && y == 52)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "\r\n      [color=90ee90]이 게임을 만든 사람[/color]\r\n");
					strLen = AppendText(DialogText, paragraph, strLen, "  : 동아 대학교 전기 공학과");
					AppendText(DialogText, paragraph, strLen, "        92 학번  안 영기");
				}
				else if (x == 50 && y == 29)
				{
					strLen = AppendText(DialogText, paragraph, strLen, "\r\n       오른쪽 : Hidra 의 보물창고\r\n");
					AppendText(DialogText, paragraph, strLen, "         위쪽이 진짜 보물창고임");
				}
			}
			else if (mParty.Map == 19)
			{
				strLen = AppendText(DialogText, paragraph, strLen, "       이 길을 통과하고자하는 사람은\r\n");
				AppendText(DialogText, paragraph, strLen, "     양측의 늪속에 있는 레버를 당기시오");
			}
			else if (mParty.Map == 23)
			{
				strLen = AppendText(DialogText, paragraph, strLen, "      (25,27)에 있는 레버를 움직이면");
				strLen = AppendText(DialogText, paragraph, strLen, "          성을 볼수 있을 것이오.\r\n\r\n");
				AppendText(DialogText, paragraph, strLen, "             [color=90ee90]제작자 안 영기 씀[/color]");
				mMapLayer[24 + mMapWidth * 26] = 52;
			}
		}

		private enum PositionType {
			Town,
			Ground,
			Den,
			Keep
		}
	}

}
