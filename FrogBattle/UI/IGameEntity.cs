using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal interface IGameEntity
    {
        void Draw(SpriteBatch spriteBatch);
    }
    internal interface IDynamicGameEntity : IGameEntity
    {
        void Update(GameTime gameTime);
    }
}
