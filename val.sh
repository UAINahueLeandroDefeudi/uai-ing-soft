SP="$(cd "$(dirname "$0")" && pwd)"
cd ~/.claude/skills/archify
for f in "$SP"/specs/*.json; do t=$(basename "$f" | sed 's/.*\.\(architecture\|sequence\)\.json/\1/')
 node bin/archify.mjs validate $t "$f" --quality showcase --json 2>&1 | python -c "
import sys,json
d=json.load(sys.stdin)
print('OK ' if d.get('ok') else 'FAIL', '$(basename $f)')
if not d.get('ok'):
  for e in d.get('diagnostics',[])[:2]: print('    ',e['message'][:330])
"
done
