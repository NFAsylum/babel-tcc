# Contexto - Tarefa 043

## Dependencias
Nenhuma dependencia estrita. Pode ser realizada em paralelo com outras tarefas.
- Tarefa 031 (README e usado como descricao do Marketplace - recomendado mas nao bloqueante)

## Bloqueia
- Tarefa 045 (deploy requer publisher e assets prontos)

## Arquivos relevantes
- package.json (metadata principal da extensao)
- .vscodeignore (arquivos excluidos do pacote)
- README.md (descricao do Marketplace)
- assets/ ou images/ (diretorio para icone, banner, screenshots)
- CHANGELOG.md (exibido no Marketplace)
- LICENSE (exibido no Marketplace)

## Notas
- O icone deve ser simples e reconhecivel em tamanho pequeno (16x16 no sidebar).
- Screenshots devem mostrar a extensao com tema claro E escuro para apelo visual.
- A descricao curta (description) aparece nos resultados de busca - ser conciso e atraente.
- Tags bem escolhidas melhoram a descoberta da extensao no Marketplace.
- O .vscodeignore e critico para manter o tamanho do .vsix pequeno.
- Testar a instalacao do .vsix em uma instancia limpa do VS Code (sem outras extensoes).
- Considerar usar o VS Code Extension Marketplace guidelines como referencia.
