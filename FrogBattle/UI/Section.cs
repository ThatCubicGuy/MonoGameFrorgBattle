using Microsoft.Xna.Framework.Graphics;
using FrogBattle.State;

namespace FrogBattle.UI
{
    internal class Section
    {
        private Texture2D background;
        private int id;
        public Section(MainMenuScene menu, Texture2D background)
        {
            this.background = background;
        }
    }
}
