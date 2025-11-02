using System.Text;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal class BattleTextBox : TextBox
    {
        private readonly int maxCount;
        private StringBuilder textBuffer;
        public BattleTextBox(Sprite boxSprite, int maxLines) : base(boxSprite)
        {
            maxCount = maxLines;
        }
        public override string Text { get => textBuffer.ToString(); set => AddText(value); }
        private void AddText(string line)
        {
            var localBuffer = textBuffer.ToString().Trim('\n');
            textBuffer.Clear();
            textBuffer = textBuffer.AppendLine(line.Trim()).AppendLine(localBuffer);
        }
    }
}
