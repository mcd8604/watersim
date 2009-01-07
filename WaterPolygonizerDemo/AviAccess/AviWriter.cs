using System;
using System.Collections.Generic;
using System.Text;
using AviFile;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AviAccess
{
    public class AviWriter : DrawableGameComponent
    {
        protected AviManager aviManager;
        protected VideoStream videoStream;

        protected ResolveTexture2D resolveTexture;
        protected byte[] textureData;

        protected string fileName;

        protected SpriteBatch spriteBatch;

        public AviWriter(Game game, string fileName)
            : base(game)
        {
            this.fileName = fileName;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            resolveTexture = new ResolveTexture2D(
                GraphicsDevice,
                width,
                height,
                1,
                GraphicsDevice.PresentationParameters.BackBufferFormat);

            // Assuming 32 bpp => 4 bytes per pixel
            textureData = new byte[4 * width * height];
            aviManager = new AviManager(fileName, false);
            videoStream = aviManager.AddVideoStream(true, 30, textureData.Length, width, height, PixelFormat.Format32bppArgb); 
            
            //Avi.AVICOMPRESSOPTIONS compressOptions = new Avi.AVICOMPRESSOPTIONS();
            //compressOptions.fccType = (uint)Avi.streamtypeVIDEO;
            //compressOptions.cbParms = 0x00000078;
            //compressOptions.fccHandler = (uint)Avi.mmioFOURCC('m', 'p', '4', '3');
            //compressOptions.dwFlags = Avi.AVICOMPRESSF_VALID;
            //videoStream = aviManager.AddVideoStream(compressOptions, 30, textureData.Length, resolveTexture.Width, resolveTexture.Height, PixelFormat.Format32bppArgb);
            
            base.LoadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (aviManager != null)
            {
                aviManager.Close();
                aviManager = null;
            }
            base.Dispose(disposing);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.ResolveBackBuffer(resolveTexture);
            resolveTexture.GetData<byte>(textureData);
            videoStream.AddFrame(textureData);
            
            spriteBatch.Begin();
            spriteBatch.Draw(resolveTexture, Vector2.Zero, Microsoft.Xna.Framework.Graphics.Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
