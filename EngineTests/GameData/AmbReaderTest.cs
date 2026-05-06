using System.IO;
using System.Linq;
using ConvertCiv3Media;
using QueryCiv3;
using Xunit;

namespace EngineTests.GameData;

public class AmbReaderTest {
	[Fact]
	public void WorkerRunAmbTest() {
		string is_on_github = System.Environment.GetEnvironmentVariable("CI");
		if (is_on_github != null) {
			return;
		}

		string path = Path.Combine(Civ3Location.GetCiv3Path(), "Art", "Units", "Worker", "WorkerRun.amb");

		Sfx sfx1 = new Amb(path).soundEffects.First();
		Sfx sfx2 = new Amb(path).soundEffects.Skip(1).First();

		Assert.Equal("WorkRunFoot1.wav", sfx1.wavName);
		Assert.Equal("WorkRunFoot2.wav", sfx2.wavName);
	}
}
