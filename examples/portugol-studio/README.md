# Exemplos Portugol Studio

Arquivos `.por` que demonstram o adapter Portugol Studio (dialeto UNIVALI) da extensao Babel TCC.

## Como testar

1. Instalar a extensao no VS Code
2. Abrir um destes arquivos `.por`
3. Configurar `babel-tcc.languageOverrides`:
   ```json
   {
     "babel-tcc.languageOverrides": { "PortugolStudio": "en-us" }
   }
   ```
4. O arquivo aparece traduzido para ingles (`programa` → `program`, `funcao` → `function`, etc.)

Portugol Studio e **case-sensitive** (diferente de VisuAlg): `Se` e `SE` nao sao reconhecidos
como a keyword `se`. Use sempre minusculas para keywords. O dialeto suporta tanto comentarios
de linha (`//`) quanto de bloco (`/* */`).

## Arquivos

- `calculadora.por` — funcoes, parametros, estrutura `escolha/caso/contrario`
