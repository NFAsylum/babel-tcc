# Tarefa 043 - Preparacao para VS Code Marketplace

## Fase
6 - Polimento e Deploy

## Objetivo
Preparar todos os assets e metadata necessarios para publicacao profissional no VS Code Marketplace.

## Escopo
- Criar conta de publisher:
  - Criar conta no Azure DevOps (se nao existir)
  - Criar publisher no VS Code Marketplace
  - Gerar Personal Access Token (PAT) para publicacao
  - Configurar publisher ID no package.json
- Criar assets visuais:
  - Icone da extensao: 128x128 pixels, PNG, fundo transparente
  - Banner do Marketplace: 1280x640 pixels, cores consistentes com marca
  - Screenshots da extensao em uso (minimo 3):
    - Screenshot mostrando traducao ativa
    - Screenshot mostrando hover/completions traduzidos
    - Screenshot mostrando settings/configuracao
  - GIF animado demonstrando workflow principal (opcional mas recomendado)
- Preparar metadata no package.json:
  - displayName atraente e descritivo
  - description concisa (max 200 caracteres)
  - categories apropriadas (Programming Languages, Other)
  - tags relevantes (multilingual, translation, localization, i18n, csharp, portuguese)
  - repository URL
  - bugs URL
  - homepage URL
  - icon path
  - galleryBanner color e theme
- Preparar descricao longa do Marketplace:
  - README.md e usado como descricao - garantir que e atraente
  - Incluir badges, screenshots, GIFs
  - Incluir secao de Quick Start
  - Incluir link para documentacao completa
- Testar preview local:
  - Usar `vsce ls` para verificar arquivos incluidos
  - Usar `vsce package` para gerar .vsix local
  - Instalar .vsix manualmente e verificar aparencia no VS Code
  - Verificar que .vscodeignore exclui arquivos desnecessarios
