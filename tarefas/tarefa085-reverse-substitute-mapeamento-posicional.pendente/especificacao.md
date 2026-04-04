# Tarefa 085 - Eliminar ambiguidade no reverse translation de keywords

## Prioridade: HIGH

## Problema
O ReverseSubstituteKeywords atual usa scanner char-by-char que nao distingue identificadores de keywords traduzidas quando tem o mesmo texto. Isso causa corrupcao no round-trip (ex: variavel "e" vira "and" em pt-br).

O problema afeta dois cenarios:
1. **Round-trip**: forward translate -> usuario edita -> reverse translate
2. **Escrita nativa**: usuario escreve codigo traduzido do zero

## Abordagem escolhida: diff de 3 vias no Core

O sistema tem 3 versoes do codigo disponíveis:
1. **Original** (disco): codigo na linguagem de programacao real
2. **Traduzido** (cache do forward): codigo com keywords traduzidas
3. **Traduzido editado** (usuario salvou): codigo com edicoes do usuario

### Fluxo
1. Diff linha-a-linha entre traduzido (2) e traduzido editado (3)
2. Linhas iguais -> copiar do original (1) sem nenhuma reverse translation
3. Linhas modificadas -> reverse translate token-a-token e substituir no original
4. Linhas adicionadas -> reverse translate e inserir no original na mesma posicao
5. Linhas removidas -> remover do original

### Porque resolve a ambiguidade
Tokens que existiam no traduzido original e nao mudaram vem diretamente do original — nenhum reverse necessario. A variavel "e" no original e "e" no traduzido; como nao mudou, o original e copiado intacto.

Tokens novos inseridos pelo usuario sao sempre codigo traduzido (o usuario escreve no idioma traduzido). O reverse translation aplica normalmente.

### Reverse de tokens individuais
Keywords sao separadas por whitespace e delimitadores. Para reverse de uma linha modificada:
1. Tokenizar por whitespace/delimitadores
2. Cada token: lookup na tabela de traducao reversa
3. Se match: substituir pela keyword original
4. Se nao match: manter (e um identifier ou literal)

### Escrita nativa (arquivo novo)
O original começa vazio. O traduzido anterior tambem e vazio. Toda linha e "adicionada" no diff. Reverse translate aplica em todas. Funciona sem caso especial.

### Onde implementar
No Core (TranslationOrchestrator ou novo servico), nao na extensao. Qualquer IDE futura usa a mesma logica.

O Host recebe: codigo original + traduzido anterior + traduzido editado -> retorna original atualizado.

### Impacto na interface
- TranslateFromNaturalLanguageAsync precisa de 2 parametros adicionais: original e traduzido anterior
- Ou: novo metodo `ApplyTranslatedEdits(original, previousTranslated, editedTranslated)`
- ReverseSubstituteKeywords char-by-char mantido como fallback para chamadas diretas sem contexto de diff

## Escopo
- Criar servico/metodo de diff de 3 vias no Core
- Integrar no TranslationOrchestrator
- Expor via Host (novo metodo ou parametros adicionais)
- Atualizar CoreBridge na extensao para enviar os 3 codigos
- Testes unitarios do diff (linhas iguais, modificadas, adicionadas, removidas)
- Teste de integracao: variavel "e" sobrevive round-trip com traducao "e" para "and"
