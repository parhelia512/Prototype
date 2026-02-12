## macOS Build

Run these commands from the repository root:

```bash
mkdir -p build
cd C7
dotnet build C7.sln
godot-mono --headless --path . --export-release "macOS" ../build/OpenCiv3-mac.zip
zip -r ../build/OpenCiv3-mac.zip Assets Text Lua
```

The output archive is:

`build/OpenCiv3-mac.zip`
