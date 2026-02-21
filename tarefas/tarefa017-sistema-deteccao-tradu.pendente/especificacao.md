# Tarefa 017 - Sistema de deteccao "tradu"

## Fase
2 - Language Adapters

## Objetivo
Implementar parser de anotacoes // tradu: nos comentarios do codigo-fonte.

## Escopo
- Implementar parser de comentarios // tradu:nome
- Suportar formatos:
  - // tradu:nomeTraduzido (traducao simples)
  - // tradu:Metodo,param1:traducao1,param2:traducao2 (metodo + parametros)
  - // tradu:"texto traduzido" (traducao de string literal)
- Integrar com IdentifierMapper para persistir traducoes detectadas
- Detectar anotacoes durante o Parse do CSharpAdapter
- Testes com exemplos do plano (Calculator com tradu)
