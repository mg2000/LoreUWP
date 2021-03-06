using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
	public sealed partial class Ending : Page
	{
		private string mPlayerName;

		public Ending()
		{
			this.InitializeComponent();

			TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;
			gamePageKeyUpEvent = (sender, args) =>
			{
				Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
				Frame.Navigate(typeof(Credit), mPlayerName);
			};

			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;

			ShowEnding();
		}

		private async void ShowEnding() {
			var animationTask = Task.Run(() =>
			{
				Task.Delay(2000).Wait();
			});

			await animationTask;

			Background = new SolidColorBrush(Colors.Black);
			EndingText.Visibility = Visibility;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			mPlayerName = e.Parameter.ToString();

			base.OnNavigatedTo(e);
		}
	}
}
