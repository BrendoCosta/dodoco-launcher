# Dodoco Launcher

<br>
<div align="center">
    <span><strong>🇬🇧 English</strong></span>
    <span>・</span>
    <a href="README_pt.md">🇵🇹 Português</a>
</div>
<br>

A work in progress (WIP) unofficial Genshin Impact launcher written in C#, TypeScript and Svelte, enabling the game be played from Linux-based operating systems, inspired by [An Anime Game Launcher](https://github.com/an-anime-team/an-anime-game-launcher) project. Currently it supports game's download, update and repair through official APIs, alongside Wine's download. DXVK management is upcoming feature, although you can install it manually.

Please keep in mind that this project is experimental and that the source code may change abruptly or even seem to make no sense.

<div align="center">
    <img style="width: 100%" alt="Launcher's main's user interface" src="Repository/Image/380_0.png">
    <div align="center">
      <img style="width: 45%" alt="Launcher's settings's user interface" src="Repository/Image/380_1.png">
      <img style="width: 45%" alt="Launcher's main's user interface" src="Repository/Image/380_2.png">
    </div>
</div>

# Build

### Requirements

- .NET SDK 7.0
- CMake 3.26
- Mingw-w64 8.0.0
- Node.js 18.16.0

**Important:** The build scripts expect all the paths to the executables of the above tools to be correctly configured in the PATH variable.

### Clone

Clone this project's repository with `--recurse-submodules` flag

```sh
git clone --recurse-submodules https://github.com/BrendoCosta/dodoco-launcher.git
```

Enter project's directory

```sh
cd ./dodoco-launcher
```

### Run

Run the launcher directly from the source code

```sh
dotnet run
```

### Release

Creates a release build of the project to `/bin/Release/net7.0/linux-x64/publish/` directory

```sh
dotnet publish
```

## License

Source code avaliable under [MIT](/LICENSE) license.