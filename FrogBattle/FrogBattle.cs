using FrogBattle.Input;
using FrogBattle.Scene;
using FrogBattle.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FrogBattle
{
    public class FrogBattle : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private IGameScene currentScene;

        private MainMenuScene _menuScene;
        private BattleControlScene _battleControlScene;

        private static readonly int WindowWidth = 640;
        private static readonly int WindowHeight = 360;
        private static readonly Point WindowSize = new(WindowWidth, WindowHeight);
        
        private bool fullscreen = false;
        private bool queueFullscreenValue = false;
        private readonly Dictionary<Keys, bool> ActiveLastFrame = [];

        private Texture2D background;

        static readonly Random random = new();
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static double RNG => random.NextDouble();

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
            _battleControlScene = new BattleControlScene(this);
            currentScene = _battleControlScene;
            Task.Run(GameInput.Update);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            background = Content.Load<Texture2D>("frorgbattle-ui-draft-2");

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            GameInput.Update();
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

            currentScene.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DeepSkyBlue);

            _spriteBatch.Begin();

            currentScene.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
