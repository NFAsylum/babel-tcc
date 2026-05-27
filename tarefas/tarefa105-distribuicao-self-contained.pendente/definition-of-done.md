# Definition of Done - Tarefa 105

- [ ] CoreBridge detecta e lanca o executavel nativo self-contained (por SO), com fallback para `dotnet Host.dll` (universal)
- [ ] Testes do CoreBridge cobrem os dois caminhos de launch
- [ ] Tabela de mapeamento RID (.NET) <-> target (vsce) implementada
- [ ] publish-core suporta variante self-contained por RID
- [ ] release.yml empacota e publica os 6 alvos self-contained + 1 universal
- [ ] vsce publica os pacotes por plataforma no Marketplace
- [ ] ovsx publica os pacotes por plataforma no Open VSX
- [ ] .vscodeignore correto para cada alvo (sem vazar binarios de outras plataformas)
- [ ] README (pt-br, en, es) explica dependencias: pacote por plataforma sem .NET; universal com .NET 8
- [ ] README deixa claro que Python e opcional (apenas para `.py`)
- [ ] Smoke test: .vsix self-contained instalado e funcional em maquina SEM .NET
- [ ] Resolucao do Python validada a partir do binario self-contained (so para `.py`)
- [ ] Suite de testes continua passando
