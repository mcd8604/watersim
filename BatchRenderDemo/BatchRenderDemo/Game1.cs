using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Runtime.InteropServices;

namespace BatchRenderDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ResolveTexture2D resolveTexture;
        byte[] textureData;

        string fileName = "test.avi";

        // handles
        IntPtr aviFile = IntPtr.Zero;
        IntPtr aviStream = IntPtr.Zero;
        bool streamOpen = false;

        // Codec - http://www.fourcc.org/ 
        readonly int codec = AviUtil.MakeFourCC('m', 'p', '4', '3');

        int numFrames = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitializeAvi();

            base.Initialize();
        }

        private void InitializeAvi()
        {
            AviAccess.AVIFileInit();

            int result = -1;
            //int i = 0;
            //while (result != 0)
            //{
            //    int mode = (int)Math.Pow(2, i);
            //    ++i;
            //    result = AviAccess.AVIFileOpen(out aviFile, fileName, mode, 0);
            //}

            result = AviAccess.AVIFileOpen(out aviFile, fileName, AviUtil.OF_READWRITE | AviUtil.OF_CREATE, 0);
            if (result != 0) throw new Exception("Error creating avi file");

            //AviAccess.AVIFileAddRef(aviFile);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            AviAccess.AVIFileRelease(aviFile);
            AviAccess.AVIFileExit();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            InitializeTextureData();
            CreateStream();
        }

        private void InitializeTextureData()
        {
            resolveTexture = new ResolveTexture2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                GraphicsDevice.PresentationParameters.BackBufferFormat);

            // 32 bpp, 4 bytes per pixel
            textureData = new byte[4 * GraphicsDevice.PresentationParameters.BackBufferWidth * GraphicsDevice.PresentationParameters.BackBufferHeight];
        }

        private void CreateStream()
        {
            AviStreamInfo psi = new AviStreamInfo();
            psi.fccType = AviUtil.StreamType_Video;
            psi.fccHandler = codec;
            //psi.dwFlags = 0;
            //psi.dwCaps = 0;
            //psi.wPriority = 0;
            //psi.wLanguage = 0;
            psi.dwScale = 1;
            psi.dwRate = 30;
            //psi.dwStart = 0;
            //psi.dwLength = 0;
            //psi.dwInitialFrames = 0;
            psi.dwSuggestedBufferSize = textureData.Length;
            psi.dwQuality = -1;
            //psi.dwSampleSize = 0;
            //psi.rcFrame.x = 0;
            //psi.rcFrame.y = 0;
            psi.rcFrame.bottom = GraphicsDevice.PresentationParameters.BackBufferHeight;
            psi.rcFrame.right = GraphicsDevice.PresentationParameters.BackBufferWidth;
            //psi.dwEditCount = 0;
            //psi.dwFormatChangeCount = 0;

            IntPtr fileStream;

            int result = AviAccess.AVIFileCreateStream(aviFile, out fileStream, ref psi);
            if (result != 0) throw new Exception("Error creating file stream");

            AviCompressOptions_Class plpOptions = new AviCompressOptions_Class();
            //lpOptions.fccType = AviUtil.StreamType_Video;
            //lpOptions.fccHandler = (uint)codec;
            //lpOptions.dwFlags = (uint)DWFLAGS.AVICOMPRESSF_VALID;

            //IntPtr lpOptionsPtr = GCHandle.Alloc(lpOptions, GCHandleType.Pinned).AddrOfPinnedObject();
            //IntPtr[] plpOptions = { lpOptionsPtr };

            bool okay = AviAccess.AVISaveOptions(IntPtr.Zero, AviUtil.ICMF_CHOOSE_KEYFRAME | AviUtil.ICMF_CHOOSE_DATARATE, 1, ref fileStream, ref plpOptions);
            if (!okay) throw new Exception("Error getting save options");

            result = AviAccess.AVISaveOptionsFree(1, ref plpOptions);
            if (result != 0) throw new Exception("Error freeing save options");

            AviCompressOptions lpOptions = plpOptions.ToStruct();
            lpOptions.fccType = (uint)AviUtil.StreamType_Video;
            lpOptions.lpParms = IntPtr.Zero;
            lpOptions.lpFormat = IntPtr.Zero;
            result = AviAccess.AVIMakeCompressedStream(out aviStream, fileStream, ref lpOptions, IntPtr.Zero);
            if (result != 0) throw new Exception("Error creating compressed stream");

            streamOpen = true;

            BitMapInfoHeader bmih = new BitMapInfoHeader();
            bmih.biSize = Marshal.SizeOf(bmih);
            bmih.biHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            bmih.biWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            bmih.biPlanes = 1;
            bmih.biBitCount = 32;
            //bmih.biSizeImage = textureData.Length;

            result = AviAccess.AVIStreamSetFormat(aviStream, 0, ref bmih, bmih.biSize);
            if (result != 0) throw new Exception("Error setting stream format");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        KeyboardState curState;
        KeyboardState lastState = Keyboard.GetState();

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            curState = Keyboard.GetState();

            if (lastState.IsKeyUp(Keys.Space) && curState.IsKeyDown(Keys.Space))
            {
                if (streamOpen)
                    streamOpen = AviAccess.AVIStreamRelease(aviStream) > 0;
            }

            lastState = curState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            WriteTextureData();

            base.Draw(gameTime);
        }

        private void WriteTextureData()
        {
            GraphicsDevice.ResolveBackBuffer(resolveTexture, 0);
            resolveTexture.GetData<byte>(textureData);

            IntPtr bufferPtr = GCHandle.Alloc(textureData, GCHandleType.Pinned).AddrOfPinnedObject();

            // NOTE: produces error on AVIStreamWrite
            if (streamOpen)
            {
                int result = AviAccess.AVIStreamWrite(
                    aviStream,
                    numFrames,
                    1,
                    bufferPtr,
                    textureData.Length,
                    AviUtil.AVIIF_KEYFRAME,
                    IntPtr.Zero,
                    IntPtr.Zero);
                if (result != 0) throw new Exception("Error writing to avi stream");
                ++numFrames;
            }
        }
    }
}
