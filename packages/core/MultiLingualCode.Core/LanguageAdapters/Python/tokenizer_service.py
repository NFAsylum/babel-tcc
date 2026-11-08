"""
Babel TCC - Python Tokenizer Service

Processo persistente que tokeniza codigo Python via stdin/stdout usando JSON Lines.
Protocolo: recebe {"source": "..."} via stdin, responde com {"ok": true, "tokens": [...]} via stdout.
Comando {"cmd": "quit"} encerra o processo.
"""

import io
import json
import keyword
import sys
import token
import tokenize
from typing import List, TypedDict


class TokenRecord(TypedDict):
    """One token of the source, as sent to the Core in the response."""

    type: int
    typeName: str
    string: str
    startLine: int
    startCol: int
    endLine: int
    endCol: int
    isKeyword: bool


class TokenizeResponse(TypedDict, total=False):
    """Response envelope of the protocol. `error` is present only when `ok` is false."""

    ok: bool
    tokens: List[TokenRecord]
    error: str


def tokenize_source(source: str) -> TokenizeResponse:
    tokens: List[TokenRecord] = []
    try:
        gen = tokenize.generate_tokens(io.StringIO(source).readline)
        for tok in gen:
            tok_type, tok_string, tok_start, tok_end, tok_line = tok
            tok_name = token.tok_name.get(tok_type, "UNKNOWN")
            tokens.append({
                "type": tok_type,
                "typeName": tok_name,
                "string": tok_string,
                "startLine": tok_start[0],
                "startCol": tok_start[1],
                "endLine": tok_end[0],
                "endCol": tok_end[1],
                "isKeyword": tok_name == "NAME" and keyword.iskeyword(tok_string),
            })
        return {"ok": True, "tokens": tokens}
    except tokenize.TokenError as e:
        return {"ok": False, "error": str(e), "tokens": tokens}
    except IndentationError as e:
        return {"ok": False, "error": str(e), "tokens": tokens}


def main() -> None:
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue
        try:
            request = json.loads(line)
        except json.JSONDecodeError as e:
            sys.stdout.write(json.dumps({"ok": False, "error": f"Invalid JSON: {e}"}) + "\n")
            sys.stdout.flush()
            continue

        # json.loads accepts any JSON value; only an object is a request. Without this check a
        # list or a number reaches .get() below and the AttributeError ends the process.
        if not isinstance(request, dict):
            sys.stdout.write(json.dumps({"ok": False, "error": "Invalid request: expected a JSON object"}) + "\n")
            sys.stdout.flush()
            continue

        if request.get("cmd") == "quit":
            break

        source = request.get("source", "")
        try:
            result: TokenizeResponse = tokenize_source(source)
        except Exception as e:
            result = {"ok": False, "error": f"Unexpected error: {e}", "tokens": []}

        sys.stdout.write(json.dumps(result) + "\n")
        sys.stdout.flush()


if __name__ == "__main__":
    main()
