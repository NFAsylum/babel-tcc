# Tarefa 112 - Adequar codigo existente aos padroes unificados

## Fase
6 - Polimento e Deploy

## Objetivo
Trazer o codigo que ja existia para as regras de docs/padroes-codigo.md, depois que as
proibicoes absolutas passaram a valer para todas as linguagens e nao so para C#.

## Contexto
O projeto tinha duas fontes de regra: docs/padroes-codigo.md, com a tabela completa de
proibicoes, e um resumo de 6 linhas no CONTRIBUTING.md. As duas divergiam. A unificacao
deixou docs/padroes-codigo.md como fonte unica e declarou que as proibicoes valem para as
quatro linguagens do projeto.

O codigo escrito antes disso nao e conforme. As regras valem para codigo novo desde ja;
esta tarefa e a adequacao do que ficou para tras, feita de proposito em separado para nao
inflar PRs que tocam esses arquivos por outro motivo.

## Inventario (retrato de 2026-08-16)

| Camada | Desvio | Quantidade |
|---|---|---|
| Plugin IntelliJ | `private` | 63 |
| Plugin IntelliJ | `throw` | 4 |
| Plugin IntelliJ | Tipos anulaveis em assinatura | 22 |
| Tokenizer Python | Anotacoes de tipo ausentes | arquivo inteiro |
| Scripts de build | Indentacao de 4 espacos contra os 2 do .editorconfig | 2 arquivos |

Os numeros envelhecem a cada commit. Recontar com os comandos de contexto.md.

## Escopo
- Kotlin: remover `private` e `internal`; trocar `throw CoreBridgeException` por Result,
  preservando a degradacao das bordas; eliminar tipos anulaveis fora dos boundaries com
  APIs Java; tipo explicito nas declaracoes; `ProcessTransport` sai para arquivo proprio.
- Python: anotacoes de tipo nas assinaturas de tokenizer_service.py.
- JavaScript: reindentar os dois scripts para 2 espacos, conforme .editorconfig.

## Fora de escopo
- Mudar as regras. Se alguma se mostrar impraticavel na adequacao, discutir e alterar
  docs/padroes-codigo.md em PR proprio, nao contornar em silencio.
- Classificacao de niveis de log no plugin (error/warn/debug) - trabalho proprio.
- Reescrever o CONTRIBUTING.md, ja unificado.
