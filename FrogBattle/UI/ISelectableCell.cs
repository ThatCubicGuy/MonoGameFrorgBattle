using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FrogBattle.UI
{
    public interface ISelectableCell
    {
        void Draw(SpriteBatch spriteBatch, Rectangle bounds);
        void DrawCursorOverlay(SpriteBatch spriteBatch, Rectangle bounds);
    }
}
