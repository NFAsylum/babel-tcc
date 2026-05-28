# Instalação

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Via VS Code Marketplace](#via-vs-code-marketplace)
- [Via .vsix manual](#via-vsix-manual)
- [Build a partir do código-fonte](#build-a-partir-do-código-fonte)
- [Verificação](#verificação)
- [Atualização](#atualização)

## Pré-requisitos

- **VS Code** 1.85 ou superior
- **.NET 8.0 Runtime** — necessário **apenas** no pacote universal. Os pacotes por plataforma
  (Windows/Linux/macOS, x64 e arm64) embutem o runtime e **não exigem .NET instalado**; o VS Code
  baixa automaticamente o pacote certo para o seu sistema.
  - Só para o pacote universal: download em https://dotnet.microsoft.com/download/dotnet/8.0
  - Verificar o **Runtime** (não o SDK): `dotnet --list-runtimes` (procure uma linha
    `Microsoft.NETCore.App 8.x`)
- **Python 3.8+** (opcional) - necessário apenas para traduzir arquivos `.py`

## Via VS Code Marketplace

1. Abrir VS Code
2. Ir em Extensions (`Ctrl+Shift+X`)
3. Pesquisar "Babel TCC"
4. Clicar em "Install"

## Via .vsix manual

1. Obter o arquivo `.vsix` (releases do GitHub ou build local)
2. No VS Code: Extensions (`Ctrl+Shift+X`) > `...` > `Install from VSIX...`
3. Selecionar o arquivo `.vsix`
4. Reiniciar VS Code quando solicitado

## Build a partir do código-fonte

```bash
# Clonar repositorio
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode

# Instalar dependencias
npm install

# Compilar
npm run build

# Gerar .vsix
npm run package
```

O arquivo `.vsix` será gerado na pasta `packages/ide-adapters/vscode/`.

## Verificação

Após instalar, verificar que a extensão está funcional:

1. Abrir um arquivo `.cs` no VS Code
2. Verificar que "Babel TCC" aparece no Output Channel (`View > Output` e selecionar "Babel TCC")
3. A mensagem "Babel TCC extension activated." deve aparecer
4. Na barra de status (canto inferior direito) deve aparecer o idioma ativo (ex: `PT-BR`)

## Atualização

- **Marketplace:** Atualizações automáticas pelo VS Code
- **Manual:** Repetir o processo de instalação com a nova versão
- **Código-fonte:** `git pull` e repetir o build
