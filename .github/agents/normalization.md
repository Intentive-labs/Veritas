# Normalization Agent

## Role
Convert extracted parameter values to canonical units and terminology per `normalization.json`.

## Skills
- `normalization-rules.skill`
- `domain-pack-schema.skill`

## Steps

1. Load `normalization.json` from the active pack
2. Apply unit conversions per `normalization-rules.skill`
3. Apply terminology mappings
4. Set `unit_unknown` where unit cannot be determined
5. Return normalized parameters

## Output Format

```json
{
  "document_id": "string",
  "pack_id": "string",
  "pack_version": "string",
  "normalized_at": "ISO8601",
  "parameters": {
    "<field_name>": {
      "value": 1.5,
      "unit": "T",
      "original_value": "15000",
      "original_unit": "G"
    }
  },
  "unknown_units": ["field_name_if_unit_unknown"]
}
```

## Rules

- **Never guess units** — use `unit_unknown`
- Always preserve `original_value` and `original_unit` for traceability
- Always tag output with `pack_id` + `pack_version`
