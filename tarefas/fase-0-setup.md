# Fase 0 - Setup e Onboarding

**Objetivo:** Preparar ambiente de desenvolvimento e validar viabilidade tecnica.

---

## Tarefa 0.1 - Inicializar repositorio principal (multilingual-code)

**Escopo:**
- [ ] Criar solution C# `MultiLingualCode.sln` com .NET 8
- [ ] Criar projeto `MultiLingualCode.Core` (class library)
- [ ] Criar projeto `MultiLingualCode.Core.Tests` (xUnit)
- [ ] Configurar estrutura de pastas conforme plano-geral.txt
- [ ] Adicionar `.gitignore` para C# e TypeScript
- [ ] Adicionar `.editorconfig` com padroes do projeto
- [ ] Configurar `Directory.Build.props` com versao e metadata comuns

**Entrega:** Solution compila sem erros, estrutura de pastas criada.

---

## Tarefa 0.2 - Inicializar projeto VS Code Extension

**Escopo:**
- [ ] Inicializar projeto com `yo code` ou manualmente em `packages/ide-adapters/vscode/`
- [ ] Configurar `tsconfig.json` com strict mode
- [ ] Configurar `package.json` com metadata da extensao
- [ ] Configurar ESLint + Prettier para TypeScript
- [ ] Configurar scripts de build e watch
- [ ] Configurar launch.json para debugging da extensao

**Entrega:** Extensao VS Code esqueleto que ativa e mostra mensagem no console.

---

## Tarefa 0.3 - Configurar CI/CD basico

**Escopo:**
- [ ] Criar `.github/workflows/build-core.yml` (build + test C#)
- [ ] Criar `.github/workflows/build-vscode.yml` (build + lint TypeScript)
- [ ] Criar `.github/workflows/test.yml` (rodar testes em PR)
- [ ] Configurar badges no README

**Entrega:** CI roda em push/PR e reporta status.

---

## Tarefa 0.4 - Proof of Concept (PoC)

**Escopo:**
- [ ] Criar mini-parser que usa Roslyn para ler codigo C# simples
- [ ] Traduzir keywords basicas (if -> se, class -> classe, void -> vazio)
- [ ] Validar comunicacao entre TypeScript e C# (spawn processo, JSON via stdin/stdout)
- [ ] Documentar resultado da PoC

**Entrega:** Demo funcional que traduz um arquivo C# simples para PT-BR via console.

---

## Tarefa 0.5 - Inicializar repositorio de traducoes (babel-tcc-translations)

**Escopo:**
- [ ] Criar estrutura de pastas: `programming-languages/csharp/`, `natural-languages/pt-br/`
- [ ] Criar `keywords-base.json` para C# (todas as 78 keywords)
- [ ] Criar `pt-br/csharp.json` com traducoes PT-BR completas
- [ ] Criar `schema/keyword-table.schema.json` (JSON Schema para validacao)
- [ ] Criar `schema/translation.schema.json`
- [ ] Adicionar `.gitignore` e atualizar README

**Entrega:** Repositorio com tabelas C# + PT-BR completas e schemas de validacao.

---

## Dependencias

```
0.1 ──┐
      ├──> 0.4 (PoC precisa de solution e extension)
0.2 ──┘
0.3 (independente)
0.5 (independente, pode ser paralela)
```
