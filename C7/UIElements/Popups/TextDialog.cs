using Godot;
using System;

public partial class TextDialog : Popup {
	LineEdit textEditBox = new LineEdit();
	private string header;
	private string prompt;
	private string defaultText;
	Action<string> handleText;


	public TextDialog(string header, string prompt, string defaultText, Action<string> handleText) {
		this.defaultText = defaultText;
		this.prompt = prompt;
		this.header = header;
		this.handleText = handleText;

		textEditBox.Theme = ThemeFactory.DefaultTheme;
		textEditBox.CaretBlink = true;
		alignment = BoxContainer.AlignmentMode.End;
		margins = new Margins(right: -10); // 10px margin from the right
	}

	public override void _Ready() {
		base._Ready();

		AddTexture(530, 260);
		AddBackground(530, 150, 110);
		AddHeader(header, 120);

		HBoxContainer labelAndTextBox = new HBoxContainer();
		labelAndTextBox.Alignment = BoxContainer.AlignmentMode.Begin;
		labelAndTextBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		labelAndTextBox.SizeFlagsStretchRatio = 1;
		labelAndTextBox.AnchorLeft = 0.0f;
		labelAndTextBox.AnchorRight = 0.85f;
		labelAndTextBox.SetPosition(new Vector2(30, 170));

		Label promptLabel = new Label();
		promptLabel.Text = prompt;
		labelAndTextBox.AddChild(promptLabel);

		textEditBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		textEditBox.SizeFlagsStretchRatio = 1;
		textEditBox.Text = defaultText;
		labelAndTextBox.AddChild(textEditBox);

		this.AddChild(labelAndTextBox);

		textEditBox.SelectAll();
		textEditBox.GrabFocus();

		textEditBox.TextSubmitted += HandleTextInput;

		//Cancel/confirm buttons.  Note the X button is thinner than the O button.
		ImageTexture circleTexture= Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 1, 1, 19, 19);
		ImageTexture xTexture = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 21, 1, 15, 19);
		ImageTexture circleHover = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 37, 1, 19, 19);
		ImageTexture xHover = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 57, 1, 15, 19);
		ImageTexture circlePressed = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 73, 1, 19, 19);
		ImageTexture xPressed = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 93, 1, 15, 19);
		TextureButton confirmButton = new TextureButton();
		confirmButton.TextureNormal = circleTexture;
		confirmButton.TextureHover = circleHover;
		confirmButton.TexturePressed = circlePressed;
		confirmButton.SetPosition(new Vector2(475, 213));
		AddChild(confirmButton);
		TextureButton cancelButton = new TextureButton();
		cancelButton.TextureNormal = xTexture;
		cancelButton.TextureHover = xHover;
		cancelButton.TexturePressed = xPressed;
		cancelButton.SetPosition(new Vector2(500, 213));
		AddChild(cancelButton);

		confirmButton.Pressed += () => { HandleTextInput(textEditBox.Text); };
		cancelButton.Pressed += GetParent<PopupOverlay>().OnHidePopup;
	}

	private void HandleTextInput(string text) {
		GetViewport().SetInputAsHandled();
		GetParent().EmitSignal(PopupOverlay.SignalName.HidePopup);
		handleText(text);
	}
}
