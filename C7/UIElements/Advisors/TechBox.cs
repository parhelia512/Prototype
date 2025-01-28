
using C7GameData;
using Godot;

public partial class TechBox : TextureButton {
	private Tech tech;

	public TechBox(Tech tech) {
		this.tech = tech;
	}

	public override void _Ready() {
		// TODO: Figure out how to pick which of the different sized tech boxes
		// we should use for a given tech.
		//
		// NOTE: this pcx has 4 columns (discovered, planned, possible,
		// not yet researchable), and 16 rows, 4 per era, with different sizes.
		ImageTexture techBox = Util.LoadTextureFromPCX("Art/Advisors/techboxes.pcx", 1, 1, 180, 80);
		TextureNormal = techBox;

		ImageTexture iconTexture = Util.LoadTextureFromPCX(tech.SmallIconPath);
		TextureRect icon = new() {
			Texture = iconTexture
		};
		icon.SetPosition(new Vector2(12, 32));
		AddChild(icon);

		// TODO: Have the name trail off if it is too large for the box.
		Label techName = new() {
			Text = tech.Name,
			OffsetLeft = 12,
			OffsetTop = 12
		};
		AddChild(techName);
	}
}
