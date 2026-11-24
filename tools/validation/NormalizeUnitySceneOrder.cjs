// Unity may reorder unchanged YAML objects while saving a large scene. Preserve
// the baseline order for review; retain every authored object and field value.
const fs = require('fs');
const cp = require('child_process');
for (const scene of process.argv.slice(2)) {
  const baseline = cp.execFileSync('git', ['show', `HEAD:${scene}`], { encoding: 'utf8', maxBuffer: 350e6 });
  const authored = fs.readFileSync(scene, 'utf8');
  const split = text => text.replace(/\r/g, '').split(/(?=^--- !u!)/m);
  const original = split(baseline), current = split(authored);
  const id = block => block.match(/^--- !u!\d+ &(-?\d+)/)[1];
  const records = new Map(current.slice(1).map(block => [id(block), block]));
  if (records.size !== current.length - 1) throw new Error('Duplicate scene object IDs');
  const ordered = [current[0]];
  const trim = text => text.replace(/[\t ]+$/gm, '');
  for (const old of original.slice(1)) {
    const key = id(old);
    if (records.has(key)) {
      const authoredBlock = records.get(key);
      const originalLines = new Map(old.split('\n').map(line => [trim(line), line]));
      ordered.push(trim(authoredBlock) === trim(old) ? old : authoredBlock.split('\n').map(line => originalLines.get(trim(line)) ?? trim(line)).join('\n'));
      records.delete(key);
    }
  }
  ordered.push(...[...records.values()].map(trim));
  const result = ordered.join('');
  const normalize = text => split(text).slice(1).map(b => b.replace(/[\t ]+$/gm, '')).sort().join('');
  if (normalize(result) !== normalize(authored)) throw new Error('Scene contents changed');
  fs.writeFileSync(scene, result);
  console.log(`${scene}: preserved ${current.length - 1} authored records`);
}
