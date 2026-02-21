# Tarefa 039 - Testes de compatibilidade

## Fase
5 - Testes e QA

## Objetivo
Verificar que a extensao funciona corretamente nas plataformas e versoes alvo, documentando compatibilidade oficial.

## Escopo
- Testar compatibilidade com versoes do VS Code:
  - VS Code 1.80 (versao minima declarada no package.json engines)
  - VS Code 1.85 (versao intermediaria)
  - VS Code latest (versao mais recente estavel)
  - VS Code Insiders (opcional, para detectar problemas futuros)
- Testar compatibilidade com versoes do .NET:
  - .NET 8 (versao minima suportada)
  - .NET 9 (se disponivel)
  - Verificar que Runtime e suficiente (nao requer SDK)
- Testar em sistemas operacionais:
  - Windows 10 (build 19041+)
  - Windows 11
  - macOS 12 Monterey ou superior
  - Ubuntu 22.04 LTS
  - Ubuntu 24.04 LTS (opcional)
- Verificar em cada plataforma:
  - Extensao instala e ativa corretamente
  - Processo Core inicia sem erros
  - Traducao funciona end-to-end
  - Caminhos de arquivo funcionam (separadores, unicode em paths)
  - Encodings de arquivo funcionam (UTF-8, UTF-8 BOM)
- Documentar matriz de compatibilidade:
  - Tabela com plataforma x versao x status
  - Problemas conhecidos por plataforma
  - Instrucoes especificas por plataforma (se necessario)
- Configurar CI para testar em multiplas plataformas:
  - GitHub Actions matrix strategy (windows, macos, ubuntu)
  - Testar com multiplas versoes de .NET
