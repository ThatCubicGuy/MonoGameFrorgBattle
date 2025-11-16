using FrogBattle.Classes;
using FrogBattle.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using FrogBattle.Input;

namespace FrogBattle.Scene
{
	internal class BattleControlScene : IGameScene, IEnumerable<IGameEntity>
	{
		private readonly BattleManager _battle;
		private readonly Game _game;
		private readonly Texture2D _abilityMenuCellTexture;
		private readonly Texture2D _abilityMenuCursorTexture;
		private readonly SelectableTable _abilityMenu;
		private BattleUIState uiState;

		public BattleControlScene(FrogBattle game)
		{
			//_battle = new BattleManager();
			_game = game;
			_abilityMenuCellTexture = _game.Content.Load<Texture2D>("UI/AbilityMenu/Cell");
			_abilityMenuCursorTexture = _game.Content.Load<Texture2D>("UI/AbilityMenu/Cursor");
			var selectableCells = new SelectableCell[]
			{
				new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new(_abilityMenuCellTexture, _abilityMenuCursorTexture),
            };
			var emptyCell = new SelectableCell(_game.Content.Load<Texture2D>("Big_chungus"), _game.Content.Load<Texture2D>("raiden-hd"));
			_abilityMenu = new(selectableCells, emptyCell, new(4, 3), Point.Zero, _abilityMenuCellTexture.Bounds.Size, new(20, 20));
		}
		public Character ActiveCharacter { get; set; }

		public void Update(GameTime gameTime)
		{
			if (GameInput.InputPressed(InputTypes.Left)) _abilityMenu.MoveCursorLeft();
            if (GameInput.InputPressed(InputTypes.Right)) _abilityMenu.MoveCursorRight();
            if (GameInput.InputPressed(InputTypes.Up)) _abilityMenu.MoveCursorUp();
            if (GameInput.InputPressed(InputTypes.Down)) _abilityMenu.MoveCursorDown();
        }

		public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
		{
			_abilityMenu.Draw(spriteBatch);
			switch (uiState)
			{
				case BattleUIState.SelectingAbility:
					{
						break;
					}
				case BattleUIState.SelectingTarget:
					{
						break;
					}
				case BattleUIState.ProcessingAbility:
					{
						break;
					}
				default:
					throw new System.InvalidOperationException($"Unknown UI State: {uiState}");
			}
		}

		public IEnumerator<IGameEntity> GetEnumerator()
		{
			yield return _abilityMenu;
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public enum BattleUIState
		{
			SelectingAbility,
			SelectingTarget,
			ProcessingAbility
		}
	}
}
