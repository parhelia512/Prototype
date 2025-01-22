using Godot;
using System;
using Serilog;
using System.Collections.Generic;

/**
 * Handles managing the advisor screens.
 * Showing them, hiding them... maybe some other things eventually.
 * This is part of the effort to de-centralize from Game.cs and be more event driven.
 */
public partial class Advisors : CenterContainer {
	private ILogger log = LogManager.ForContext<Advisors>();

	private DomesticAdvisor domesticAdvisor;
	private ScienceAdvisor scienceAdvisor;

	// A list of all the non-null advisors, so we can hide them whenever we
	// draw a different advisor.
	private List<TextureRect> advisors = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Center the advisor container.  Following directions at https://docs.godotengine.org/en/stable/tutorials/gui/size_and_anchors.html?highlight=anchor
		//Also taking advantage of it being 1024x768, as the directions didn't really work.  This is not 100% ideal (would be great for a general-purpose solution to work),
		//but does work with the current graphics independent of resolution.
		this.Hide();
	}

	private void ShowLatestAdvisor() {
		log.Debug("Received request to show latest advisor");

		OnShowSpecificAdvisor("F1");
		this.Show();
	}

	private void _on_Advisor_hide() {
		this.Hide();
	}

	private void OnShowSpecificAdvisor(string advisorType) {
		// Hide any existing advisors so we can draw the requested one.
		foreach (TextureRect tr in advisors) {
			tr.Hide();
		}

		if (advisorType.Equals("F1")) {
			if (domesticAdvisor == null) {
				domesticAdvisor = new DomesticAdvisor();
				advisors.Add(domesticAdvisor);
				AddChild(domesticAdvisor);
			} else {
				domesticAdvisor.Show();
			}
			this.Show();
		}
		if (advisorType.Equals("F6")) {
			if (scienceAdvisor == null) {
				scienceAdvisor = new ScienceAdvisor();
				advisors.Add(scienceAdvisor);
				AddChild(scienceAdvisor);
			} else {
				scienceAdvisor.Show();
			}
			this.Show();
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (this.Visible) {
			if (@event is InputEventKey eventKey) {
				//As I've added more shortcuts, I've realized checking all of them here could be irksome.
				//For now, I'm thinking it would make more sense to process or allow through the ones that should go through,
				//as most of the global ones should *not* go through here.
				if (eventKey.Pressed) {
					if (eventKey.Keycode == Godot.Key.Escape) {
						this.Hide();
						GetViewport().SetInputAsHandled();
					} else {
						log.Debug("Advisor received a key press; stopping propagation.");
						GetViewport().SetInputAsHandled();
					}
				}
			}
		}
	}
}
