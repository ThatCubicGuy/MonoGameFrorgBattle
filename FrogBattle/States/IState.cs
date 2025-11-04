using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.States
{
    internal interface IState
    {
        void Update();
        void Draw(SpriteBatch spriteBatch);
    }
}
