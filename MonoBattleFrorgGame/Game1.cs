using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoBattleFrorgGame
{
    public class Game1 : Game
    {
        Texture2D battleui;
        Texture2D hpbar;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
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
            battleui = Content.Load<Texture2D>("frorgbattle-ui-draft-2");
            hpbar = Content.Load<Texture2D>("bayonetta-hp-bar");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F4) || Keyboard.GetState().IsKeyDown(Keys.F11))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(battleui, new Vector2(80, 0), Color.White);
            _spriteBatch.Draw(hpbar, new Vector2(120, 70), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
