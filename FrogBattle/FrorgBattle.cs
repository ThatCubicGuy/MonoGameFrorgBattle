using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FrogBattle
{
    public class FrorgBattle : Game
    {
        private static class Tools
        {
            public static Rectangle Chunk(int x, int y)
            {
                return new Rectangle(16 * x, 16 * y, 16, 16);
            }
            public static Vector2 ChunkVec(int x, int y)
            {
                return new Vector2(16 * x, 16 * y);
            }
        }
        Texture2D resources1;
        Texture2D hpbar;
        Texture2D battleui;

        private static readonly Random rand = new();
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static double RNG
        {
            get
            {
                return rand.NextDouble();
            }
        }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public FrorgBattle()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = "Frorg Battle";
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            resources1 = Content.Load<Texture2D>("resources");
            hpbar = Content.Load<Texture2D>("bayonetta-hp-bar");
            battleui = Content.Load<Texture2D>("frorgbattle-ui-draft-2");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) ;
                // how tf do i fullscreen

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            

            // just messing around here
            _spriteBatch.Begin();
            _spriteBatch.Draw(resources1, Tools.ChunkVec(0, 0), Tools.Chunk(15, 0), Color.White);
            _spriteBatch.Draw(resources1, Tools.ChunkVec(1, 0), Tools.Chunk(15, 1), Color.White);
            _spriteBatch.Draw(resources1, Tools.ChunkVec(2, 0), Tools.Chunk(15, 2), Color.White);
            _spriteBatch.Draw(resources1, Tools.ChunkVec(0, 1), Tools.Chunk(15, 3), Color.White);
            _spriteBatch.Draw(resources1, Tools.ChunkVec(1, 1), Tools.Chunk(15, 4), Color.White);
            _spriteBatch.Draw(resources1, Tools.ChunkVec(2, 1), Tools.Chunk(15, 5), Color.White);
            _spriteBatch.Draw(hpbar, new Vector2(120, 70), Color.White);
            _spriteBatch.Draw(battleui, new Vector2((Window.ClientBounds.Width - battleui.Width) / 2, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
