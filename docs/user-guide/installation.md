# Instalacao

## Indice

- [Pre-requisitos](#pre-requisitos)
- [Via VS Code Marketplace](#via-vs-code-marketplace)
- [Via .vsix manual](#via-vsix-manual)
- [Build a partir do codigo-fonte](#build-a-partir-do-codigo-fonte)
- [Verificacao](#verificacao)
- [Atualizacao](#atualizacao)

## Pre-requisitos

- **VS Code** 1.85 ou superior
- **.NET 8.0 Runtime** - necessario para o motor de traducao C#
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verificar: `dotnet --version`

## Via VS Code Marketplace

<!-- TODO: a extensao ainda nao esta publicada no Marketplace -->
Quando publicada:
1. Abrir VS Code
2. Ir em Extensions (`Ctrl+Shift+X`)
3. Pesquisar "Babel TCC"
4. Clicar em "Install"

## Via .vsix manual

1. Obter o ficheiro `.vsix` (releases do GitHub ou build local)
2. No VS Code: Extensions (`Ctrl+Shift+X`) > `...` > `Install from VSIX...`
3. Selecionar o ficheiro `.vsix`
4. Reiniciar VS Code quando solicitado

## Build a partir do codigo-fonte

```bash
# Clonar repositorio
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode

# Instalar dependencias
npm install

# Compilar
npm run build

# <!-- TODO: adicionar scripts "publish-core" e "package" ao package.json -->
```

O ficheiro `.vsix` sera gerado na pasta `packages/ide-adapters/vscode/`.

## Verificacao

Apos instalar, verificar que a extensao esta funcional:

1. Abrir um ficheiro `.cs` no VS Code
2. Verificar que "Babel TCC" aparece no Output Channel (`View > Output` e selecionar "Babel TCC")
3. A mensagem "Babel TCC extension activated." deve aparecer
4. Na barra de status (canto inferior direito) deve aparecer o idioma ativo (ex: `PT-BR`)

## Atualizacao

- **Marketplace:** Atualizacoes automaticas pelo VS Code
- **Manual:** Repetir o processo de instalacao com a nova versao
- **Codigo-fonte:** `git pull` e repetir o build
