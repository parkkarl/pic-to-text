# Pic to Text

A lightweight Windows tray app that turns any part of your screen into text.

Press **Shift + Win + D**, drag a rectangle around text, and the recognized text is copied to your clipboard automatically.

## Features

- Familiar snipping-style area selection
- Built-in Windows OCR — screenshots are never uploaded
- Automatic clipboard copy
- Multi-monitor support
- Runs quietly in the system tray
- Escape cancels a capture

## Requirements

- Windows 10 version 2004 or newer, or Windows 11
- At least one Windows OCR language installed (`Settings → Time & language → Language & region`)

## Install

Download `PicToText.exe` from the latest [release](../../releases), run it, and look for the tray icon. Windows may show a SmartScreen warning for unsigned community builds.

To start it with Windows, press `Win + R`, enter `shell:startup`, and place a shortcut to `PicToText.exe` in that folder.

## Build

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish src/PicToText/PicToText.csproj -c Release -r win-x64 --self-contained true
```

The executable is created under `src/PicToText/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`.

## Privacy

Pic to Text captures only the rectangle you select. OCR runs locally through the Windows OCR engine. No image or recognized text is sent over the network.

## License

[MIT](LICENSE)
