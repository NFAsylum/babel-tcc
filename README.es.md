# Babel TCC - MultiLingual Code

[![CI](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml/badge.svg)](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml)

[Portugues](https://github.com/NFAsylum/babel-tcc/blob/main/README.md) | [English](https://github.com/NFAsylum/babel-tcc/blob/main/README.en.md) | **Espanol**

Extensión de VS Code que traduce código de programación visualmente en tiempo real, manteniendo los archivos originales intactos en el disco.

## ¿Qué hace?

Los desarrolladores escriben código en C# o Python, y la extensión muestra las palabras clave e identificadores traducidos al idioma configurado (PT-BR, ES-ES, etc.). Al guardar, el código vuelve automáticamente al lenguaje de programación original.

**Antes (C# original en el disco, con anotaciones `// tradu` en los identificadores):**
```csharp
using System;

namespace HelloWorld // tradu[pt-br]:OlaMundo
{
    class Program // tradu[pt-br]:Programa
    {
        static void Main(string[] args) // tradu[pt-br]:Principal,args:argumentos
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

**Después (lo que ve un desarrollador PT-BR en el editor; los comentarios `// tradu` siguen visibles):**
```csharp
usando System;

espaçonome OlaMundo // tradu[pt-br]:OlaMundo
{
    classe Programa // tradu[pt-br]:Programa
    {
        estático vazio Principal(texto[] argumentos) // tradu[pt-br]:Principal,args:argumentos
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

El archivo en el disco permanece **siempre en el código original**. La traducción es solo visual.

## Características

- **Traducción visual de palabras clave** - Palabras clave de C# y Python traducidas (if->se, class->classe, def->definir, etc.)
- **Traducción de identificadores** - Nombres de variables, métodos y clases mediante la anotación `// tradu:`
- **Traducción inversa al guardar** - Al guardar, el código traducido vuelve al original en el disco
- **Autocompletado traducido** - Sugerencias de palabras clave e identificadores en el idioma configurado
- **Hover con el original** - Al pasar el ratón sobre una palabra clave traducida se muestra la original
- **Barra de estado** - Indicador del idioma activo con un selector rápido
- **Resaltado de sintaxis** - Gramática TextMate personalizada para palabras clave traducidas
- **Colaboración multilingüe** - Varios desarrolladores en el mismo repositorio, cada uno ve su idioma
- **Cero impacto** - Compiladores, CI/CD, Git e IntelliSense funcionan con normalidad
- **Proceso persistente** - El motor de traducción se ejecuta como proceso de larga duración, sin arranque en frío por petición

## Inicio rápido

1. Instalar la extensión en VS Code
2. Abrir un archivo `.cs` o `.py`
3. Pulsar `Ctrl+Shift+P` y ejecutar `Babel TCC: Select Language`
4. Elegir `pt-br`
5. La traducción aparece automáticamente

## Instalación

### Requisitos previos

- VS Code 1.85 o superior
- .NET 8.0 Runtime
- Python 3.8+ (para el soporte de archivos Python)
- Nada adicional para VisuAlg / Portugol Studio (funcionan mediante Text Scan, sin parser externo)

### Desde el código fuente

```bash
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode
npm install
npm run build
```

Para generar el `.vsix`: `npm run package` (requiere [vsce](https://github.com/microsoft/vscode-vsce))

## Lenguajes de programación soportados

| Lenguaje de programación | Extensión | Palabras clave | Modo |
|--------------------------|----------|----------|------|
| C# | `.cs` | 89 | Roslyn + Text Scan, admite tradu |
| Python | `.py` | 35 | Subproceso CPython + Text Scan, admite tradu |
| VisuAlg (Claudio Morgado) | `.alg` | 48 | Text Scan solo palabras clave, sin distinción de mayúsculas |
| Portugol Studio (UNIVALI) | `.por` | 26 | Text Scan solo palabras clave, con distinción de mayúsculas |

## Idiomas disponibles

Portugués (PT-BR), Portugués ASCII, Inglés, Español, Francés, Alemán, Italiano, Japonés (Romaji), Chino, Árabe.

## Arquitectura

```
VS Code Extension (TypeScript)
        |
    CoreBridge (JSON Lines via stdin/stdout)
        |
Core Engine (C# / .NET 8)
    |           |
CSharpAdapter   PythonAdapter
  (Roslyn)     (tokenize stdlib)
        |
Translation Tables (JSON)
```

| Capa | Tecnología | Función |
|--------|-----------|--------|
| Core Engine | C# / .NET 8 | Motor de traducción, parsing mediante Roslyn y el tokenizador de Python |
| Extension | TypeScript / VS Code API | Integración con el editor |
| Traducciones | JSON | Tablas de palabras clave y mapeos |
| Comunicación | JSON Lines via stdin/stdout | Puente persistente entre TS y C# |

## Configuración

Añadir a `settings.json`:

```json
{
  "babel-tcc.enabled": true,
  "babel-tcc.language": "pt-br"
}
```

### Sistema "tradu"

Los desarrolladores anotan identificadores personalizados en el código:

```csharp
public class Calculator // tradu[pt-br]:Calculadora
{
    public int operationCount; // tradu[pt-br]:contagemOperacoes

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }
}
```

Un desarrollador PT-BR ve (las anotaciones `// tradu` siguen visibles):

```csharp
público classe Calculadora // tradu[pt-br]:Calculadora
{
    público inteiro contagemOperacoes; // tradu[pt-br]:contagemOperacoes

    público inteiro Somar(inteiro primeiroNumero, inteiro segundoNumero) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        contagemOperacoes++;
        retornar primeiroNumero + segundoNumero;
    }
}
```

## Stack

- **Core:** C# / .NET 8, Microsoft.CodeAnalysis (Roslyn)
- **Extension:** TypeScript, VS Code Extension API
- **Pruebas:** xUnit (C#) + Vitest (TypeScript), 849 pruebas (667 C#, 182 TS)
- **CI/CD:** GitHub Actions (matriz Ubuntu + Windows)
- **Traducciones:** JSON

## Estructura del proyecto

```
babel-tcc/
  packages/
    core/
      MultiLingualCode.Core/        # Motor de traduccion
      MultiLingualCode.Core.Host/   # Host persistente (stdin/stdout)
      MultiLingualCode.Core.Tests/  # Pruebas xUnit
    ide-adapters/
      vscode/                       # Extension de VS Code
        src/
          extension.ts              # Punto de entrada
          services/                 # CoreBridge, Config, LanguageDetector
          providers/                # Content, Edit, Save, Completion, Hover
          ui/                       # StatusBar
        test/                       # Pruebas Vitest
        syntaxes/                   # Gramaticas TextMate
  scripts/                          # Validacion de traducciones
  tarefas/                          # Seguimiento de tareas
```

## Documentación

- [Arquitectura](docs/developer-guide/architecture.md) - Visión general de la arquitectura y los flujos
- [Convenciones de código](CONTRIBUTING.md#convencoes-de-codigo) - Nomenclatura y estilo
- [Decisiones técnicas](docs/decisoes-tecnicas.md) - Registro de decisiones y justificaciones
- [Guía del usuario](docs/user-guide/) - Instalación, uso y configuración
- [Guía del desarrollador](docs/developer-guide/) - Cómo extender el proyecto

## Contribución

¡Las contribuciones son bienvenidas! Consulta [CONTRIBUTING.md](CONTRIBUTING.md) para más detalles.

## Licencia

[MIT](LICENSE)
