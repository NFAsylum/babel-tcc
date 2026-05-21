# Exemplos VisuAlg

Arquivos `.alg` que demonstram o adapter VisuAlg da extensao Babel TCC.

## Como testar

1. Instalar a extensao no VS Code
2. Abrir um destes arquivos `.alg`
3. Configurar `babel-tcc.languageOverrides`:
   ```json
   {
     "babel-tcc.languageOverrides": { "VisuAlg": "en-us" }
   }
   ```
4. O arquivo aparece traduzido para ingles (`algoritmo` → `algorithm`, `se` → `if`, etc.)

VisuAlg e **case-insensitive**: voce pode escrever `ALGORITMO`, `Se`, `se`, `SE` — todos sao
reconhecidos como a mesma keyword. A normalizacao para minusculas acontece na traducao.

## Arquivos

- `media_notas.alg` — leitura, condicionais aninhadas, estruturas de repeticao
