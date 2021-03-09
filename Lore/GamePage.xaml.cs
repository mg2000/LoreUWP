using Microsoft.Graphics.Canvas;
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
using Windows.Storage;
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
    public sealed partial class GamePage : Page
    {
        private bool mSpriteBatchSupported;

        private int mMapWidth = 0;
        private int mMapHeight = 0;

        private SpriteSheet mMapTiles;
        private int[] mMapLayer = null;

        bool ClampToSourceRect = true;
        string InterpolationMode = CanvasImageInterpolation.Linear.ToString();

        public GamePage()
        {
            this.InitializeComponent();
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
                var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/TOWN1.MAP"));
                var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
                var reader = new BinaryReader(stream);
                mMapWidth = reader.ReadByte();
                mMapHeight = reader.ReadByte();

                mMapLayer = new int[mMapWidth * mMapHeight];

                for (var i = 0; i < mMapWidth * mMapHeight; i++)
                {
                    mMapLayer[i] = reader.ReadByte();
                }

                mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.bmp"), new Vector2(64, 64), Vector2.Zero);
                //wizardWalk = await SpriteSheet.LoadAsync(device, "SpriteSheets/WizardWalkRight.png", new Vector2(128, 192), new Vector2(64, 150));
                //wizardIdle = await SpriteSheet.LoadAsync(device, "SpriteSheets/WizardIdleRight.png", new Vector2(128, 192), new Vector2(64, 150));
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

            var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(64 * 50, 64 * 30));
            args.DrawingSession.Transform = transform;

            var size = sender.Size.ToVector2();

            var options = ClampToSourceRect ? CanvasSpriteOptions.ClampToSourceRect : CanvasSpriteOptions.None;
            //var options = CanvasSpriteOptions.None;
            //var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), InterpolationMode);
            var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), CanvasImageInterpolation.HighQualityCubic.ToString());

            using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, options))
            {
                if (mMapLayer != null)
                    DrawLayer(sb, mMapLayer);
            }
        }

        void DrawLayer(CanvasSpriteBatch sb, int[] layer)
        {
            for (int i = 0; i < layer.Length; ++i)
            {
                DrawTile(sb, layer, i);
            }
        }

        void DrawTile(CanvasSpriteBatch sb, int[] layer, int index)
        {
            int row = index / mMapWidth;
            int column = index % mMapWidth;

            Vector4 tint = Vector4.One;

            mMapTiles.Draw(sb, layer[index], mMapTiles.SpriteSize * new Vector2(column, row), tint);
        }
    }

}
