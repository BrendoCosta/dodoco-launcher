# Dodoco Launcher

<br>
<div align="center">
    <a href="README.md">🇬🇧 English</a>
    <span>・</span>
    <span><strong>🇵🇹 Português</strong></span>
</div>
<br>

Um launcher não-oficial para Genshin Impact escrito em C#, TypeScript e Svelte, possibilitando que o jogo seja executado em sistemas operacionais baseados em Linux, inspirado pelo projeto [An Anime Game Launcher](https://github.com/an-anime-team/an-anime-game-launcher). Atualmente ele suporta o download, atualização e reparo do jogo através das APIs oficiais, bem como o download do Wine. O gerenciamento para o DXVK é um recurso que deve ser implementado em breve, embora você possa o instalar manualmente.

Por favor, tenha em mente que este projeto é experimental e que o código-fonte pode mudar abruptamente ou até mesmo parecer não fazer sentido.

<div align="center">
    <img style="width: 100%" alt="Launcher's main's user interface" src="Repository/Image/380_0.png">
    <div align="center">
      <img style="width: 45%" alt="Launcher's settings's user interface" src="Repository/Image/380_1.png">
      <img style="width: 45%" alt="Launcher's main's user interface" src="Repository/Image/380_2.png">
    </div>
</div>

## Download

### Instalação

A última versão do launcher pode ser baixada [aqui](https://github.com/BrendoCosta/dodoco-launcher/releases/latest). Descompacte o arquivo `dodoco-launcher-vX.X.X.zip` em um diretório de sua preferência.

### Execute

Rode o arquivo executável `dodoco-launcher` através do seu explorador de arquivos ou pelo terminal:

```sh
chmod +x ./dodoco-launcher
```

```sh
./dodoco-launcher
```

## Build

### Requisitos

- .NET SDK 7.0
- CMake 3.26
- Mingw-w64 8.0.0
- Node.js 18.16.0

**Importante:** Os scripts de build esperam que todos os caminhos para os executáveis das ferramentas acima estejam corretamentes configurados na variável PATH.

**Importante:** Você deve reservar ao menos 1 GB de espaço em armazenamento para realizar a build do projeto.

### Clone

Clone esse repositório deste projeto com a flag `--recurse-submodules`:

```sh
git clone --recurse-submodules https://github.com/BrendoCosta/dodoco-launcher.git
```

Entre no diretório do projeto:

```sh
cd ./dodoco-launcher
```

### Execução

Execute o launcher direto do código fonte:

```sh
dotnet run
```

### Empacotamento

Cria um pacote de distribuição do projeto para o diretório `/bin/Release/net7.0/linux-x64/publish/`:

```sh
dotnet publish
```

## Licença

Código fonte disponível sob a licença [MIT](/LICENSE).