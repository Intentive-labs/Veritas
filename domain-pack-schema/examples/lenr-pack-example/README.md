# LENR Example Pack — Generic Placeholder

> ⚠️ **This is a placeholder only.** All values are synthetic and must be replaced after the physicist ontology session.

This example pack lives in `intentive-labs/veritas` as a structural reference for how a LENR domain pack should look.

## What needs to be filled in (requires physicist session)

| File | What is missing |
|------|----------------|
| `manifest.json` | Real assumptions, ignores, source citation |
| `ontology.json` | Actual LENR parameter names, types, units |
| `classification.json` | Real outcome classes and threshold rules |
| `validation.json` | Physical plausibility rules |
| `normalization.json` | Real unit conversions (e.g. Gauss → Tesla) and terminology |
| `hypotheses/hypothesis-v1.json` | Actual hypothesis, variables, expected direction |

## Real pack location

The real customer pack lives in `customer/lenr-pack` (private repo) and is customer IP.

## Schema reference

Pack files must validate against `domain-pack-schema/schema-v1/*.schema.json`.
