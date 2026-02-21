# Tarefa 027 - Commands e UI basica

## Fase
3 - VS Code Extension

## Objetivo
Implementar comandos da extensao e interface basica na status bar.

## Escopo
- Implementar commands:
  - multilingual.toggleTranslation - ativar/desativar
  - multilingual.selectLanguage - QuickPick para escolher idioma
  - multilingual.openTranslated - abrir versao traduzida do arquivo ativo
  - multilingual.showOriginal - mostrar versao original
- Implementar StatusBar em src/ui/
  - Mostrar idioma ativo na barra de status
  - Clicar abre seletor de idioma
  - Indicador visual quando traducao esta ativa
- Implementar LanguageSelector em src/ui/
  - QuickPick com idiomas disponiveis
  - Persistir selecao na configuracao
