#!/bin/bash
set -euo pipefail

CLAUDE_MODEL="${CLAUDE_MODEL:-claude-sonnet-4-6}"
CODEX_MODEL="${CODEX_MODEL:-gpt-5.5}"

mkdir -p .ai-loop
SPEC=".ai-loop/current-spec.md"
REVIEW=".ai-loop/feedback.md"
DIFF=".ai-loop/last-diff.txt"
TASK="${1:?Uso: ~/Proyectos/AspireApp/orchestrator-repo.sh \"descripción de la tarea\"}"
MAX_ITER=5

rm -f "$SPEC" "$REVIEW" "$DIFF"

echo ">>> [Claude/$CLAUDE_MODEL] escribiendo spec..."
claude -p --model "$CLAUDE_MODEL" --permission-mode acceptEdits \
  "Sos arquitecto. Escribí el archivo $SPEC con un spec técnico para esta tarea
   en el contexto de este repo: '$TASK'. Incluí qué archivos tocar, firmas con
   tipos, casos de borde y tests. Solo escribí ese archivo."

for i in $(seq 1 "$MAX_ITER"); do
  echo ""; echo "===== ITERACIÓN $i ====="

  echo ">>> [Codex/$CODEX_MODEL] implementando (encerrado en el workspace)..."
  RESUME=""; [ "$i" -gt 1 ] && RESUME="resume --last"
  codex exec $RESUME -m "$CODEX_MODEL" --sandbox workspace-write \
    "Leé $SPEC y, si existe, $REVIEW. Implementá/corregí el código de este repo
     según el spec. Corré los tests si hay. No expliques."

  echo ">>> [Claude/$CLAUDE_MODEL] revisando..."
  git diff > "$DIFF"
  rm -f "$REVIEW"
  claude -c -p --model "$CLAUDE_MODEL" --permission-mode acceptEdits \
    "Leé $SPEC y el diff en $DIFF (lo que cambió Codex). Si cumple el spec, escribí
     exactamente 'APROBADO' en $REVIEW. Si no, qué falta en $REVIEW y, si el spec
     quedó corto, refiná $SPEC. No toques el código."

  echo ""; echo "----- CAMBIOS DE ESTA ITERACIÓN -----"
  git --no-pager diff --stat
  echo ""; echo "----- REVIEW DE CLAUDE -----"; sed -n '1,25p' "$REVIEW" 2>/dev/null
  echo ""
  read -rp "¿Aceptás? [s=commit / n=descartar / q=salir] " r </dev/tty
  case "$r" in
    s|S) git add -A && git commit -q -m "ai-loop: iter $i" && echo "✓ commiteado" ;;
    q|Q) echo "Salgo, cambios sin commitear."; break ;;
    *)   git checkout -- . && git clean -fd -e .ai-loop && echo "✗ descartado" ;;
  esac

  grep -q "APROBADO" "$REVIEW" 2>/dev/null && { echo ">>> Claude aprobó. Fin."; break; }
done
