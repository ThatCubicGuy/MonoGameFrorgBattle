using Microsoft.Xna.Framework.Graphics;
using FrogBattle.States;

namespace FrogBattle.UI
{
    internal class Section
    {
        private Texture2D background;
        private int id;
        public Section(MenuState menu, Texture2D background)
        {
            this.background = background;
        }
    }
}
