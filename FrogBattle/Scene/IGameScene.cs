using FrogBattle.UI;
using System.Collections.Generic;

namespace FrogBattle.Scene
{
    internal interface IGameScene : IEnumerable<IGameEntity>
    {
        void Update(Microsoft.Xna.Framework.GameTime gameTime);
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);
    }
}
