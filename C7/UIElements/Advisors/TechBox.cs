
using System;
using C7GameData;
using Godot;

public partial class TechBox : TextureButton {
	private Tech tech;
	private TechState techState;

	public enum TechState {
		// This tech is known to the player.
		kKnown,
		// The player is actively researching this tech.
		kInProgress,
		// The player could research this tech.
		kPossible,
		// The player needs to research the prerequisites before this tech can
		// be researched.
		kBlocked,
	}

	public TechBox(Tech tech, TechState techState) {
		this.tech = tech;
		this.techState = techState;
	}

	public override void _Ready() {
		// TODO: Figure out how to pick which of the different sized tech boxes
		// we should use for a given tech.
		//
		// NOTE: this pcx has 16 rows, 4 per era, with different sizes.
		//
		// NOTE: the X coordinates of each column were found via guess+check.
		ImageTexture knownTechBox = TextureLoader.Load("tech_box.known");
		ImageTexture inProgressTechBox = TextureLoader.Load("tech_box.in_progress");
		ImageTexture possibleTechBox = TextureLoader.Load("tech_box.possible");
		ImageTexture blockedTechBox = TextureLoader.Load("tech_box.blocked");

		TextureNormal = techState switch {
			TechState.kKnown => knownTechBox,
			TechState.kInProgress => inProgressTechBox,
			TechState.kPossible => possibleTechBox,
			TechState.kBlocked => blockedTechBox,
			_ => throw new ArgumentOutOfRangeException("Invalid tech state")
		};

		ImageTexture iconTexture = TextureLoader.Load("tech_icons.small", tech, useCache: true);
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

		if (!tech.RequiredForEraAdvancement) {
			TextureRect notRequired = new() {
				Texture = TextureLoader.Load("tech_box.non_required"),
			};
			notRequired.SetPosition(new Vector2(85, 0));
			AddChild(notRequired);
		}
	}
}
