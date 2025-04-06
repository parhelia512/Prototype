using Godot;
using System;

[GlobalClass]
[Tool]
public partial class ConsoleButton : Control {
	[Export] string text;
	[Export] string tooltipText;

	[Signal] public delegate void PressedEventHandler();

	private TextureButton button;

	public override void _Ready() {
		button = new();
		button.TextureNormal = Util.LoadTextureFromPCX("Art/interface/consoleButtons.pcx", 1, 1, 16, 16);
		button.TextureHover = Util.LoadTextureFromPCX("Art/interface/consoleButtons.pcx", 17, 1, 16, 16);
		button.TexturePressed = Util.LoadTextureFromPCX("Art/interface/consoleButtons.pcx", 33, 1, 16, 16);
		button.TooltipText = tooltipText;
		button.Pressed += () => { EmitSignal(SignalName.Pressed); };

		Label label = new();
		label.Text = text;
		label.OffsetLeft = 3;
		label.OffsetTop = -3;
		label.MouseFilter = Control.MouseFilterEnum.Ignore;

		AddChild(button);
		button.AddChild(label);

		CustomMinimumSize = button.Size;

		// During the gameplay the button should be hidden by default
		if (!Engine.IsEditorHint()) {
			button.Hide();
		}
	}

	public void ShowButton() {
		button.Show();
	}
}
