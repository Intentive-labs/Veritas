# Classification Agent

## Role
Classify experimental outcomes per `classification.json` rules.

## Skills
- `domain-pack-schema.skill`

## Steps

1. Load `classification.json` from the active pack
2. Apply classification rules to normalized parameters
3. Assign outcome class per pack-defined thresholds
4. Tag output with `pack_id` + `pack_version`

## Output Format

```json
{
  "document_id": "string",
  "pack_id": "string",
  "pack_version": "string",
  "classified_at": "ISO8601",
  "outcome": "string",
  "outcome_confidence": 0.85,
  "supporting_evidence": ["field_name: value"],
  "classification_rule": "string"
}
```

## Rules

- **Do NOT classify if validation has errors**
- Every output tagged with `pack_id` + `pack_version`
- Classification rule changes are **BREAKING** — require pack major version bump
- Output written to `/classified/corpora/{corpus_id}/{pack_id}/{version}/{document_id}.json`
