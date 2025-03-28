using Godot;

[GlobalClass]
public partial class DoubleClickHandler : Node {
	int leftMouseButtonClickCount = 0;
	InputEventMouseButton lastEventMouseButton;
	const double DOUBLE_CLICK_DELAY = 0.2;

	Timer timer = new();

	[Signal] public delegate void SingleClickEventHandler(InputEventMouseButton eventMouseButton);
	[Signal] public delegate void DoubleClickEventHandler(InputEventMouseButton eventMouseButton);

	public override void _Ready() {
		AddChild(timer);
		timer.OneShot = true;
		timer.Timeout += OnTimeout;
	}

	public void Accept(InputEventMouseButton eventMouseButton) {
		lastEventMouseButton = eventMouseButton;
		++leftMouseButtonClickCount;

		timer.Stop();

		if (leftMouseButtonClickCount >= 2) {
			EmitSignal(SignalName.DoubleClick, lastEventMouseButton);
			leftMouseButtonClickCount = 0;
		} else {
			timer.Start(DOUBLE_CLICK_DELAY);
		}
	}

	private void OnTimeout() {
		EmitSignal(SignalName.SingleClick, lastEventMouseButton);
		leftMouseButtonClickCount = 0;
	}
}
