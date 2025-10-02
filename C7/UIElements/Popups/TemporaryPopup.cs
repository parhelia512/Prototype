using Godot;
using System;
using System.Threading.Tasks;

public partial class TemporaryPopup : Label {
	private int durationInMillis;

	public TemporaryPopup(string text, float durationInSec) {
		Text = text;
		this.durationInMillis = (int)(durationInSec * 1000);

		AddThemeStyleboxOverride("normal", PopupStyleBox());
		AddThemeColorOverride("font_color", Colors.White);
	}

	// A stylebox that works well for temporary popups or tooltips in game.
	public static StyleBoxFlat PopupStyleBox() {
		StyleBoxFlat styleBox = new();
		styleBox.ContentMarginLeft = 5;
		styleBox.ContentMarginRight = 5;
		styleBox.ContentMarginTop = 2;
		styleBox.ContentMarginBottom = 2;
		styleBox.BgColor = Color.FromHtml("1C1C1C");
		return styleBox;
	}

	public async void ShowPopup() {
		// Wait until we hit our duration then destroy ourself.
		await Task.Delay(durationInMillis);

		Visible = false;
		QueueFree();
	}
}
