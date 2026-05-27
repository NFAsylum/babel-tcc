# Tarefa 108 - Corrigir regex de locale nas anotacoes tradu

## Fase
6 - Polimento e Deploy

## Objetivo
Permitir que anotacoes `// tradu[lang]:` reconhecam codigos de idioma com mais de dois
segmentos (ex.: `ja-jp-romaji`, `pt-br-ascii`), corrigindo um descompasso entre o regex e a
convencao de idiomas suportados.

## Contexto do bug
O `LanguagePrefixRegex` em `TraduAnnotationParser` usava `^\[([a-z]{2}(-[a-z]+)?)\]:(.+)$`, que
so aceita `xx` ou `xx-yy`. O regex foi escrito (2026-03-01) seguindo a convencao `xx-yy` entao
documentada em `docs/guia-traducoes.md`. As variantes com sufixo (`ja-jp-romaji`, `pt-br-ascii`)
entraram depois no repo de traducoes (abril/2026) e nunca foram acomodadas no regex.

Efeito: keywords traduzem nos 10 idiomas (nao passam pelo regex), mas identificadores so podiam
ser anotados em 8 deles - comportamento assimetrico e nao intencional (nenhuma decisao ou
comentario justifica a exclusao).

## Escopo
- Trocar `(-[a-z]+)?` por `(-[a-z]+)*` no `LanguagePrefixRegex` (aceita multiplos segmentos).
- Testes cobrindo locale multi-segmento (`ja-jp-romaji`, `pt-br-ascii`), single e multi-idioma.
- Atualizar `docs/guia-traducoes.md` (estava desatualizada: dizia `xx-yy` e listava `ja-jp`).

## Fora de escopo
- Mudancas no formato do mapeamento de parametros/literais.
- Suporte a digitos no codigo de idioma (nenhum idioma atual usa).
