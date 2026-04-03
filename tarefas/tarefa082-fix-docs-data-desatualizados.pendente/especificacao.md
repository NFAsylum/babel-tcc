# Tarefa 082 - Corrigir documentacao e dados desatualizados

## Prioridade: MEDIUM

## Problemas

### MEDIUM-004: CHANGELOG.md desatualizado
- Diz "77 keywords" (sao 89 C# + 35 Python)
- Diz "338 testes" (sao 581+)
- Menciona EditInterceptor/SaveHandler (nao existem)
- Nao menciona Python nem 10 idiomas

### MEDIUM-005: Docs duplicados e desatualizados
Arquivos com informacao errada:
- docs/arquitetura.md: duplicata desatualizada de architecture.md — deletar ou redirecionar
- docs/compatibility-matrix.md: so C#/pt-br (falta Python, 10 idiomas)
- docs/poc-resultado.md: 77 keywords, 277 testes
- docs/user-guide/faq.md: "apenas C#", "apenas PT-BR", "SaveHandler"
- docs/user-guide/troubleshooting.md: "SaveHandler", so .cs
- docs/developer-guide/creating-translations.md: "77 keywords"
- docs/guia-traducoes.md: "77 keywords"
- docs/security-review.md: diz "v1.0" mas projeto e v0.1.0
- docs/template.json: arquivo orfao com 78 IDs (0-77) — deletar
- docs/padroes-codigo.md: contradiz CONTRIBUTING (kebab vs camelCase, proibe ternarios/constructors que o codigo usa)

### MEDIUM-006: README.md contradicoes
- README diz Python suportado, 10 idiomas — correto
- FAQ diz "apenas C#", "apenas PT-BR" — errado
- CHANGELOG diz 77 keywords, 338 testes — errado
- Compatibility-matrix lista so C# e pt-br — errado
- Links linhas 193-197 apontam para docs/arquitetura.md (desatualizado) em vez de docs/developer-guide/architecture.md

### MEDIUM-008: TestData pt-br/csharp.json incompleto
Arquivo: TestData/translations/natural-languages/pt-br/csharp.json

Faltam 12 IDs (74, 78-88): var, async, await, yield, record, partial, where, dynamic, nameof, init, required, global. O arquivo completo em babel-tcc-translations tem todas as 89. Testes de integracao que usem keywords modernas falham silenciosamente.

## Escopo
- Atualizar CHANGELOG com estado real
- Deletar docs duplicados (arquitetura.md, template.json)
- Atualizar docs restantes com numeros e componentes corretos
- Corrigir links no README
- Copiar pt-br/csharp.json completo para TestData
