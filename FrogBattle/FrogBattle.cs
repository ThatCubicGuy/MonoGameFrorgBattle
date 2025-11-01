using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrogBattle
{
    public class FrogBattle : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        static readonly int WindowWidth = 640;
        static readonly int WindowHeight = 360;
        static readonly Point WindowSize = new(WindowWidth, WindowHeight);

        bool fullscreen = false;
        bool queueFullscreenValue = false;
        Dictionary<Keys, bool> ActiveLastFrame = [];

        private Texture2D battleUI;
        public readonly List<UI.ColoredSprite> Sprites = [];

        static readonly Random random = new();
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static double RNG
        {
            get
            {
                return random.NextDouble();
            }
        }

        public FrogBattle()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = "Frorg Battle";
            Window.BeginScreenDeviceChange(false);
            Window.EndScreenDeviceChange(Window.ScreenDeviceName, WindowWidth, WindowHeight);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            battleUI = Content.Load<Texture2D>("frorgbattle-ui-draft-2");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!ActiveLastFrame[Keys.Space])
                {
                    ActiveLastFrame[Keys.Space] = true;
                    Debug.WriteLine("aaaaaaaaaaamoooooooooooguuuuuuuuuuuuuuus");
                }
            }
            else ActiveLastFrame[Keys.Space] = false;

                // TODO: Add your update logic here

                base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DeepSkyBlue);
            
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Sprites.ForEach(sprite => _spriteBatch.Draw(sprite.texture, sprite.position, sprite.color));

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
