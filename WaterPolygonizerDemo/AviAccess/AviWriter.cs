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

        protected RenderTarget2D renderTarget;
        //protected ResolveTexture2D resolveTexture;
        protected byte[] textureData;

        protected string fileName;

        public AviWriter(Game game, string fileName)
            : base(game)
        {
            this.fileName = fileName;
        }

        protected override void  LoadContent()
        {
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                GraphicsDevice.PresentationParameters.BackBufferFormat);
            GraphicsDevice.SetRenderTarget(1, renderTarget);

            // 32 bpp, 4 bytes per pixel
            textureData = new byte[4 * GraphicsDevice.PresentationParameters.BackBufferWidth * GraphicsDevice.PresentationParameters.BackBufferHeight];
            aviManager = new AviManager(fileName, false);
            videoStream = aviManager.AddVideoStream(true, 30, textureData.Length, renderTarget.Width, renderTarget.Height, PixelFormat.Format32bppArgb); 
            
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
            //GraphicsDevice.ResolveBackBuffer(renderTarget);
            GraphicsDevice.SetRenderTarget(1, null);
            renderTarget.GetTexture().GetData<byte>(textureData);
            videoStream.AddFrame(textureData);
            GraphicsDevice.SetRenderTarget(1, renderTarget);

            base.Draw(gameTime);
        }
        
    }
}
