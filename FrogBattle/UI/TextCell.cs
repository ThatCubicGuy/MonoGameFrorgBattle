using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal record class TextCell(string Text, SpriteFont Font, Texture2D Texture, Texture2D CursorTexture) : SelectableCell(Texture, CursorTexture)
    {
        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Draw(spriteBatch, bounds);
            spriteBatch.DrawString(Font, Text, bounds.Location.ToVector2(), Color.White);
        }
    }
}
