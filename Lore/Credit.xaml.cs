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

		public Credit()
		{
			this.InitializeComponent();

			mWidth = (int)((Frame)Window.Current.Content).ActualWidth;
			mHeight = (int)((Frame)Window.Current.Content).ActualHeight;
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
			}


			var text = "이 게임의 끝마무리에 공헌한 인물";
				var symbolText = new CanvasTextFormat()
				{
					FontSize = 30,
					FontFamily = "Segoe UI",
					HorizontalAlignment = CanvasHorizontalAlignment.Center,
					VerticalAlignment = CanvasVerticalAlignment.Center
				};

				var textLayout = new CanvasTextLayout(sender, text, symbolText, (float)sender.Size.Width, (float)sender.Size.Height);
				args.DrawingSession.DrawTextLayout(textLayout, 0, 0, new CanvasSolidColorBrush(sender, Colors.LightBlue));

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
			}
			catch (Exception e)
			{
				Debug.WriteLine($"에러: {e.Message}");
			}
		}

		private CanvasCommandList GenerateTextDisplay(ICanvasResourceCreator resourceCreator, float width, float height)
		{
			var cl = new CanvasCommandList(resourceCreator);

			using (var ds = cl.CreateDrawingSession())
			{
				float top = 10;

				//float center = width / 2.0f;
				//float symbolPos = center - 5.0f;
				//float labelPos = center + 5.0f;

				var symbolText = new CanvasTextFormat()
				{
					FontSize = 30,
					FontFamily = "Segoe UI",
					HorizontalAlignment = CanvasHorizontalAlignment.Center,
					VerticalAlignment = CanvasVerticalAlignment.Center
				};

				float y = top;

				ds.DrawText("이 게임의 끝마무리에 공헌한 인물", 0, 0, Color.FromArgb(0xff, 0x53, 0xef, 0xef), symbolText);
			}

			return cl;
		}
	}
}
