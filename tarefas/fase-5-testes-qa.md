# Fase 5 - Testes e QA

**Objetivo:** Garantir qualidade, estabilidade e performance.
**Pre-requisito:** Fases 1-3 concluidas.

---

## Tarefa 5.1 - Cobertura de testes unitarios

**Escopo:**
- [ ] Atingir 80%+ de cobertura no Core (C#)
- [ ] Atingir 70%+ de cobertura na Extension (TypeScript)
- [ ] Adicionar testes para edge cases:
  - Codigo vazio
  - Codigo com erros de sintaxe
  - Keywords nao mapeadas
  - Identificadores com caracteres especiais
  - Arquivos grandes (1000+ linhas)
- [ ] Configurar relatorio de cobertura no CI

**Entrega:** Cobertura atinge metas, relatorio disponivel.

---

## Tarefa 5.2 - Testes de integracao

**Escopo:**
- [ ] Cenarios end-to-end:
  - Traducao completa C# -> PT-BR
  - Traducao reversa PT-BR -> C#
  - Round-trip: original -> traduzido -> original == original
  - Edicao e save workflow completo
  - Multiplos idiomas no mesmo projeto
- [ ] Testes com projetos reais (nao apenas exemplos)

**Entrega:** Todos os cenarios de integracao passam.

---

## Tarefa 5.3 - Testes de performance

**Escopo:**
- [ ] Medir tempo de traducao por tamanho de arquivo:
  - Arquivo pequeno (< 100 linhas): < 100ms
  - Arquivo medio (100-500 linhas): < 500ms
  - Arquivo grande (500-2000 linhas): < 2s
- [ ] Medir uso de memoria do Core
- [ ] Medir tempo de startup da extensao
- [ ] Identificar e corrigir gargalos
- [ ] Documentar benchmarks

**Entrega:** Performance dentro dos limites aceitaveis, benchmarks documentados.

---

## Tarefa 5.4 - Testes de compatibilidade

**Escopo:**
- [ ] VS Code versoes 1.80+
- [ ] .NET 8+
- [ ] Windows 10/11
- [ ] macOS 12+ (se possivel)
- [ ] Ubuntu 22.04+ (se possivel)
- [ ] Documentar plataformas testadas

**Entrega:** Extensao funciona nas plataformas alvo.

---

## Tarefa 5.5 - Security review

**Escopo:**
- [ ] Validacao de input (codigo fonte, JSON)
- [ ] Verificar path traversal no FileSystemHelper
- [ ] Verificar code injection via JSON malicioso
- [ ] Verificar permissoes de arquivo
- [ ] Auditar dependencias com `dotnet list package --vulnerable` e `npm audit`
- [ ] Corrigir vulnerabilidades encontradas

**Entrega:** Nenhuma vulnerabilidade critica.

---

## Dependencias

```
5.1, 5.2, 5.3 (podem ser paralelas)
5.4 (apos 5.1 e 5.2)
5.5 (independente)
```
