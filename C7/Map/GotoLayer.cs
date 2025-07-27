using System;
using System.Collections.Generic;
using C7GameData;
using C7Engine;
using ConvertCiv3Media;
using Godot;

// The layer responsible for drawing the cursor when the player is selecting a
// move via the "goto" command.
public partial class GotoLayer : LooseLayer {
	public GotoLayer() {
		// Load the font we'll use for the goto move counter.
		//
		// We skip the cache so that we can change the size without affecting other
		// code using the same font.
		//
		// We use FixedSize so Godot can calculate the width of the text for centering.
		gotoLabelFont = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf", null, ResourceLoader.CacheMode.Ignore);
		gotoLabelFont.FixedSize = 25;

		whiteFontTheme.DefaultFont = gotoLabelFont;
		whiteFontTheme.SetColor("font_color", "Label", Colors.White);
		whiteFontTheme.SetFontSize("font_size", "Label", 20);

		redFontTheme.DefaultFont = gotoLabelFont;
		redFontTheme.SetColor("font_color", "Label", Colors.Red);
		redFontTheme.SetFontSize("font_size", "Label", 20);
	}

	// The GoTo cursor and label fields.
	private AnimatedSprite2D gotoCursorSprite = null;
	private Label gotoLabel = null;
	Theme whiteFontTheme = new();
	Theme redFontTheme = new();
	FontFile gotoLabelFont = new();

	public void drawGotoCursor(LooseView looseView, Vector2 position, int moves, bool attackingMove) {
		// Initialize cursor if necessary
		if (gotoCursorSprite == null) {
			gotoCursorSprite = new AnimatedSprite2D();
			gotoCursorSprite.SpriteFrames = TextureLoader.LoadAnimation("animations.cursor", "cursor");
			gotoCursorSprite.Animation = "cursor";

			gotoLabel = new() {
				Theme = whiteFontTheme
			};

			looseView.AddChild(gotoLabel);
			looseView.AddChild(gotoCursorSprite);
			gotoCursorSprite.Play("cursor");
		}

		gotoLabel.Theme = attackingMove ? redFontTheme : whiteFontTheme;
		gotoLabel.Text = moves.ToString();
		Vector2 labelSize = gotoLabelFont.GetStringSize(gotoLabel.Text);
		gotoLabel.Position = position - labelSize / 2;
		gotoCursorSprite.Position = position;

		// Now that we're positioned our objects, we can display them again.
		gotoCursorSprite.Show();
		gotoLabel.Show();
	}

	public override void onBeginDraw(LooseView looseView, GameData gameData) {
		// Hide the cursor if it has been initialized
		gotoCursorSprite?.Hide();
		gotoLabel?.Hide();

		looseView.mapView.game.updateAnimations(gameData);
	}

	public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
		if (looseView.mapView.game.gotoInfo == null) {
			return;
		}
		Game.GotoInfo gotoInfo = looseView.mapView.game.gotoInfo;

		// We set the move cost to -1 in Game.cs if the move is invalid for some reason.
		if (gotoInfo.destinationTile == tile && gotoInfo.moveCost >= 0) {
			drawGotoCursor(looseView, tileCenter, gotoInfo.moveCost, gotoInfo.attackingMove);
		} else if (gotoInfo.pathCoords
				?.Contains(new System.Numerics.Vector2(tile.XCoordinate, tile.YCoordinate)) == true) {
			// If this tile is part of the path, draw a little dot to represent that.
			looseView.DrawCircle(tileCenter, 5, Colors.White);
		}
	}
}
