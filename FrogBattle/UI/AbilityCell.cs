using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal record class AbilityCell(string Name, SpriteFont Font, Texture2D Icon, Texture2D BackgroundTexture, Texture2D CursorTexture) : TextCell(Name, Font, BackgroundTexture, CursorTexture)
    {
        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Draw(spriteBatch, bounds);
            spriteBatch.Draw(Icon, bounds, Color.White);
        }
    }
}
