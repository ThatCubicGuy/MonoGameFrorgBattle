using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoBattleFrorgGame
{
    public class FrorgBattle : Game
    {
        internal class  TextBox
        {
            public int Width { get; private set; }
            public int Height { get; private set; }
            public TextBox(int width, int height)
            {
                Width = width; Height = height;
            }
        }
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
        Texture2D battleui;
        Texture2D hpbar;

        private static readonly Random rand = new();
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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            battleui = Content.Load<Texture2D>("resources");
            hpbar = Content.Load<Texture2D>("bayonetta-hp-bar");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(battleui, Tools.ChunkVec(0, 0), Tools.Chunk(15, 0), Color.White);
            _spriteBatch.Draw(battleui, Tools.ChunkVec(1, 0), Tools.Chunk(15, 1), Color.White);
            _spriteBatch.Draw(battleui, Tools.ChunkVec(2, 0), Tools.Chunk(15, 2), Color.White);
            _spriteBatch.Draw(battleui, Tools.ChunkVec(0, 1), Tools.Chunk(15, 3), Color.White);
            _spriteBatch.Draw(battleui, Tools.ChunkVec(1, 1), Tools.Chunk(15, 4), Color.White);
            _spriteBatch.Draw(battleui, Tools.ChunkVec(2, 1), Tools.Chunk(15, 5), Color.White);
            _spriteBatch.Draw(hpbar, new Vector2(120, 70), Color.White);
            //_spriteBatch.Draw();
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
