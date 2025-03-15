using Godot;
using System;
using System.Diagnostics;
using C7GameData;
using Serilog;

// A generic popup for some sort of information.
// TODO: support other advisors
// TODO: support advisor moods
public partial class InformationalPopup : Popup {
	string message;

	public InformationalPopup(string message) {
		alignment = BoxContainer.AlignmentMode.End;
		margins = new Margins(right: 10);
		this.message = message;
	}

	public override void _Ready() {
		base._Ready();

		int width = 430;
		int height = 230;

		ImageTexture AdvisorHappy = Util.LoadTextureFromPCX("Art/SmallHeads/popupFOREIGN.pcx", 1, 40, 149, 110, false);
		TextureRect AdvisorHead = new TextureRect();
		AdvisorHead.Texture = AdvisorHappy;
		AdvisorHead.SetPosition(new Vector2(275, 0));
		AddChild(AdvisorHead);

		AddTexture(width, height);
		AddBackground(width, height - 110, 110);
		AddHeader("Foreign Advisor", 120);

		Label messageLabel = new();
		messageLabel.Text = message;
		messageLabel.SetPosition(new Vector2(25, 160));
		AddChild(messageLabel);

		AddConfirmButton(new Vector2(width - 40, height - 40), () => {
			GetParent().EmitSignal(PopupOverlay.SignalName.HidePopup);
		});
	}
}
