using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Lore
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Credit : Page
	{
		private SpriteSheet mMapTiles;
		private SpriteSheet mCharacterTiles;

		private int mWidth;
		private int mHeight;

		private bool mAnimate = true;
		private int mYOffset = 0;

		private string mPlayerName;

		public Credit()
		{
			this.InitializeComponent();

			mWidth = (int)((Frame)Window.Current.Content).ActualWidth;
			mHeight = (int)((Frame)Window.Current.Content).ActualHeight;

			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;
			gamePageKeyUpEvent = (sender, args) =>
			{
				Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
				Frame.Navigate(typeof(MainPage));
			};

			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}
		private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			args.TrackAsyncAction(LoadImages(sender.Device).AsAsyncAction());
		}

		private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			var options = CanvasSpriteOptions.ClampToSourceRect;
			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, options))
			{
				Vector4 tint = Vector4.One;

				var column = mWidth / 64;
				if (mWidth % 64 > 0)
					column++;

				var row = mHeight / 64;
				if (mHeight % 64 > 0)
					row++;

				for (var y = 0; y < row; y++)
				{
					for (var x = 0; x < column; x++)
						mMapTiles.Draw(sb, 47, mMapTiles.SpriteSize * new Vector2(x, y), tint);
				}

				mCharacterTiles.Draw(sb, 5, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 2), Vector4.One);

				mCharacterTiles.Draw(sb, 8, new Vector2(mCharacterTiles.SpriteSize.X * 1, mCharacterTiles.SpriteSize.Y / 2 * 5), Vector4.One);
				mCharacterTiles.Draw(sb, 8, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 5), Vector4.One);
				mCharacterTiles.Draw(sb, 12, new Vector2(mCharacterTiles.SpriteSize.X * 3, mCharacterTiles.SpriteSize.Y / 2 * 5), Vector4.One);

				mCharacterTiles.Draw(sb, 10, new Vector2(mCharacterTiles.SpriteSize.X * 1, mCharacterTiles.SpriteSize.Y / 2 * 8), Vector4.One);
				mCharacterTiles.Draw(sb, 14, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 8), Vector4.One);
				mCharacterTiles.Draw(sb, 18, new Vector2(mCharacterTiles.SpriteSize.X * 3, mCharacterTiles.SpriteSize.Y / 2 * 8), Vector4.One);
				mCharacterTiles.Draw(sb, 11, new Vector2(mCharacterTiles.SpriteSize.X * 1, mCharacterTiles.SpriteSize.Y / 2 * 10), Vector4.One);
				mCharacterTiles.Draw(sb, 15, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 10), Vector4.One);
				mCharacterTiles.Draw(sb, 19, new Vector2(mCharacterTiles.SpriteSize.X * 3, mCharacterTiles.SpriteSize.Y / 2 * 10), Vector4.One);

				mCharacterTiles.Draw(sb, 9, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 13), Vector4.One);
				
				mCharacterTiles.Draw(sb, 23, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 16), Vector4.One);

				mCharacterTiles.Draw(sb, 22, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 19), Vector4.One);

				mCharacterTiles.Draw(sb, 26, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 22), Vector4.One);

				mCharacterTiles.Draw(sb, 25, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 25), Vector4.One);

				mCharacterTiles.Draw(sb, 16, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 28), Vector4.One);
				mCharacterTiles.Draw(sb, 17, new Vector2(mCharacterTiles.SpriteSize.X * 2, mCharacterTiles.SpriteSize.Y / 2 * 30), Vector4.One);

				int ahnTile;
				if (mYOffset % 2 == 0)
					ahnTile = 20;
				else
					ahnTile = 21;

				mCharacterTiles.Draw(sb, ahnTile, new Vector2(mCharacterTiles.SpriteSize.X * 27, mYOffset), Vector4.One);
			}


			var text = "이 게임의 끝마무리에 공헌한 인물";
			var titlelText = new CanvasTextFormat()
			{
				FontSize = 30,
				FontFamily = "Segoe UI",
				FontWeight = FontWeights.Bold,
				HorizontalAlignment = CanvasHorizontalAlignment.Center
			};

			var textLayout = new CanvasTextLayout(sender, text, titlelText, (float)sender.Size.Width, (float)sender.Size.Height);
			args.DrawingSession.DrawTextLayout(textLayout, 0, 0, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0xff)));

			var descText = new CanvasTextFormat()
			{
				FontSize = 30,
				FontFamily = "Segoe UI"
			};

			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, $"이름은 {mPlayerName}. 바로 당신이다.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 70, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "히드라, 노티스 동굴의 보스였다.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 166, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "거대 드래곤, 락업 동굴의 보스였다.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 292, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "미노타우루스, 여기서 두 번 등장하는 생물이다.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 420, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "기갑 독사, 던전 오브 이블을 지키던 기계 생물.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 516, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "검은 기사, 네크로만서 쪽의 제 2인자 이다.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 612, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "아키몽크, 네크로만서의 왼팔 역할의 실력자.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 708, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "아키메이지, 네크로만서의 오른팔인 마법사.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 804, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));
			args.DrawingSession.DrawTextLayout(new CanvasTextLayout(sender, "네오-네크로만서, 바로 당신의 목표였던 그자.", descText, (float)sender.Size.Width, (float)sender.Size.Height), 400, 932, new CanvasSolidColorBrush(sender, Color.FromArgb(0xff, 0x55, 0xff, 0x55)));

			//var ds = args.DrawingSession;
			//using (ds.CreateLayer(0))
			//{
			//	ds.DrawImage(GenerateTextDisplay(sender, (float)sender.Size.Width, (float)sender.Size.Height));
			//}
		}
		private async Task LoadImages(CanvasDevice device)
		{
			try
			{
				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.png"), new Vector2(64, 64), Vector2.Zero);
				mCharacterTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_sprite.png"), new Vector2(64, 64), Vector2.Zero);

				AnimateAhn();
			}
			catch (Exception e)
			{
				Debug.WriteLine($"에러: {e.Message}");
			}
		}

		private async void AnimateAhn() {
			await Task.Run(() =>
			{
				while (mAnimate)
				{
					if (mYOffset < mHeight)
						mYOffset++;
					else
						mYOffset = 0;

					Task.Delay(200).Wait();
				}
			});
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			mPlayerName = e.Parameter.ToString();

			base.OnNavigatedTo(e);
		}
	}
}
