using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.Scene
{
    internal class GameSceneManager
    {
        public GameSceneManager() { }
        public IGameScene CurrentScene { get; private set; }
        public void ChangeScene(IGameScene scene) 
        {
            CurrentScene = scene;
        }
        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (var item in CurrentScene)
            {
                item.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
