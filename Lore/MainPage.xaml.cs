using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lore
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private int mFirstLine;
		private int mLastLine;

		private float mOffset = 0;
		private float mVelocity = 0;
		private const float mTargetSpeed = 1;
		private float mTargetVelocity = mTargetSpeed;

		private CanvasLinearGradientBrush mTextOpacityBrush;
		private CanvasLinearGradientBrush mBlurOpacityBrush;

		static CanvasTextFormat symbolText = new CanvasTextFormat()
		{
			FontSize = 30,
			FontFamily = "Segoe UI",
			HorizontalAlignment = CanvasHorizontalAlignment.Center,
			VerticalAlignment = CanvasVerticalAlignment.Center
		};

		private static float mLineHeight =  symbolText.FontSize * 1.5f;

		private int mFocusItem = 1;

		public MainPage()
		{
			this.InitializeComponent();

			SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
			{
				if (!e.Handled)
				{
					e.Handled = true;
				}
			};

			SyncSaveData();
		}

		private async void SyncSaveData()
        {
			var users = await User.FindAllAsync();
			var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-000063336555");

			Debug.WriteLine($"클라우드 동기화 연결 결과: {gameSaveTask.Status}");

			InitializeKeyEvent();
		}

		private void InitializeKeyEvent()
        {
			TypedEventHandler<CoreWindow, KeyEventArgs> mainPageKeyUpEvent = null;
			mainPageKeyUpEvent = (sender, args) =>
			{
				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (prologControl.Visibility == Visibility.Visible)
				{
					mTargetVelocity = 0;
					prologControl.Visibility = Visibility.Collapsed;

					mainmenuPanel.Visibility = Visibility.Visible;
				}
				else
				{
					if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mFocusItem == 1)
						{
							Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;
							Frame.Navigate(typeof(NewGamePage));
						}
						else if (mFocusItem == 2)
						{
							Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;
							Frame.Navigate(typeof(GamePage));
						}
						else
						{
							CoreApplication.Exit();
						}
					}
					else if (mFocusItem == 1)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 2;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 3;
						}

					}
					else if (mFocusItem == 2)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 3;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 1;
						}
					}
					else if (mFocusItem == 3)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 1;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 2;
						}

					}
				}
			};

			Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
		}

		private void prologControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			var stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.0f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.1f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.9f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 1.0f }
			};

			mTextOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);

			stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.White, Position=0.0f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.3f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.7f },
				new CanvasGradientStop() { Color=Colors.White, Position = 1.0f },
			};

			mBlurOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);
		}

		private void prologControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
		{
			float height = (float)sender.Size.Height;
			float totalHeight = characters.Length * mLineHeight + height;

			//if (mOffset >= 400)
			//    return;

			mVelocity = mVelocity * 0.90f + mTargetVelocity * 0.10f;

			mOffset = mOffset + mVelocity;

			mOffset = mOffset % totalHeight;
			while (mOffset < 0)
				mOffset += totalHeight;

			float top = height - mOffset;
			mFirstLine = Math.Max(0, (int)(-top / mLineHeight));
			mLastLine = Math.Min(characters.Length, (int)((height + mLineHeight - top) / mLineHeight));
		}

		private void prologControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			var textDisplay = GenerateTextDisplay(sender, (float)sender.Size.Width, (float)sender.Size.Height);

			var blurEffect = new GaussianBlurEffect()
			{
				Source = textDisplay,
				BlurAmount = 10
			};

			mTextOpacityBrush.StartPoint = mBlurOpacityBrush.StartPoint = new Vector2(0, 0);
			mTextOpacityBrush.EndPoint = mBlurOpacityBrush.EndPoint = new Vector2(0, (float)sender.Size.Height);

			var ds = args.DrawingSession;

			//using (ds.CreateLayer(mBlurOpacityBrush))
			//{
			//    ds.DrawImage(blurEffect);
			//}

			using (ds.CreateLayer(mTextOpacityBrush))
			{
				ds.DrawImage(textDisplay);
			}
		}

		private CanvasCommandList GenerateTextDisplay(ICanvasResourceCreator resourceCreator, float width, float height)
		{
			var cl = new CanvasCommandList(resourceCreator);

			using (var ds = cl.CreateDrawingSession())
			{
				float top = height - mOffset;

				float center = width / 2.0f;
				float symbolPos = center - 5.0f;
				float labelPos = center + 5.0f;

				for (int i = mFirstLine; i < mLastLine; ++i)
				{
					float y = top + mLineHeight * i;
					int index = i;

					if (index < characters.Length)
					{
						ds.DrawText(characters[index], labelPos, y, Color.FromArgb(0xff, 0x53, 0xef, 0xef), symbolText);
					}
				}
			}

			return cl;
		}

		private static string[] characters = new string[]
		{
			"오래전 부터 만날 운명이었던 당신에게 드리는 글",
			"",
			"어서 오십시오. LORE의 세계에 당신을 초대합니다.",
			"이곳은 당신이 지금 있는 지구와는 같은 시간대를 지니고 있지만 공",
			"간적으로는 다르게 진화해 온 또다른 지구라고 할 수 있습니다.  이",
			"곳은 아직도 검과 마법이 존재하며 과학 기술은 지구의 중세 정도라",
			"고 해두면 이 곳이 이해가 되겠습니까 ?",
			"지금 이 곳은 다른 공간에서 차원의 문을 이용해 들어온  어떤 자에",
			"의해 고통을 겪고 있습니다. 이 일은 그전부터 이 세계에 지워진 운",
			"명으로 예언 되었었고  그 불가변한 운명에서 벗어나기 위해 당신을",
			"이 세계로 소환하게 되었습니다. 이제부터 당신은 이 세계의 사람입",
			"니다.  지금부터 당신의 일행과 함께 당신의 운명 또한 새롭게 개척",
			"해 보십시오.  당신의 현명한 판단에 의해 당신 앞에 펼쳐질 세계에",
			"도전 하십시오.",
			"언제나 운명의 파문은 당신 앞에 엄습해오고 있으니 ...",
			"",
			"                                                     제작자  안 영기  드림"
		};
	}
}
