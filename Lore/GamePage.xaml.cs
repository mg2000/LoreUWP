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
		private int mTalkX = 0;
		private int mTalkY = 0;

		public GamePage()
		{
			var rootFrame = Window.Current.Content as Frame;

			this.InitializeComponent();

			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyEvent = null;
			gamePageKeyEvent = async (sender, args) =>
			{
				DialogText.Blocks.Clear();

				if (mTalkMode > 0)
				{
					//ContinueText.Visibility = Visibility.Collapsed;
					//TalkMode(mTalkX, mTalkY);

					DialogText.Blocks.Clear();

					var paragraph = new Paragraph();
					DialogText.Blocks.Add(paragraph);
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
							}
							else if (mMapLayer[x + mMapWidth * y] == 24)
							{
								// Water
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

			Window.Current.CoreWindow.KeyDown += gamePageKeyEvent;
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

		private void TalkMode(int moveX, int moveY)
		{
			if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
			{
				DialogText.Blocks.Clear();

				var paragraph = new Paragraph();
				DialogText.Blocks.Add(paragraph);

				var run = new Run();

				var strLen = 0;
				strLen = AppendText(DialogText, paragraph, strLen, "저희 성주님을 만나 보십시오.");

				paragraph.Inlines.Add(run);
			}
			else if (moveX == 50 && moveY == 27)
			{
				Debug.WriteLine("테스트 중...");
				for (var i = 0; i < DialogText.Blocks.Count; i++)
					DialogText.Blocks.RemoveAt(i);


				var paragraph = new Paragraph();
				DialogText.Blocks.Add(paragraph);

				if (mTalkMode == 0)
				{
					var strLen = 0;
					if (mTalkMode == 0)
					{
						strLen = AppendText(DialogText, paragraph, strLen, "나는 [color=62e4f2]로드 안[/color]이오.\r\n");
						strLen = AppendText(DialogText, paragraph, strLen, "이제부터 당신은 이 게임에서 새로운 인물로서 생을 시작하게 될것이오. 그럼 나의 이야기를 시작하겠소.");
						//ContinueText.Visibility = Visibility.Visible;
					}

					mTalkMode = 1;
					mTalkX = moveX;
					mTalkY = moveY;
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

			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			switch (mParty.Etc[11])
			{

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
