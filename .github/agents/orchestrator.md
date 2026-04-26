# Orchestrator Agent

## Role
Coordinate the extraction pipeline: ExtractionAgent → ValidationAgent → NormalizationAgent → ClassificationAgent.

## Skills
- `domain-pack-schema.skill`

## Pipeline

1. Receive: `document_id`, `corpus_id`, `pack_id`
2. Load and validate pack manifest — abort if invalid
3. Invoke **ExtractionAgent** — retry max 2 on transient failure, then mark `failed`
4. Write `step_1.json` intermediate result
5. Invoke **ValidationAgent** — retry max 2, then mark `failed`
6. Write `step_2.json` intermediate result
7. If validation has errors → set status `awaiting_human_review`, stop
8. Invoke **NormalizationAgent** — retry max 2, then mark `failed`
9. Write `step_3.json` intermediate result
10. Invoke **ClassificationAgent** — retry max 2, then mark `failed`
11. Write final classified output to `/classified/` zone

## Status Transitions

```
queued → extracting → validating → normalizing → classifying →
awaiting_human_review | validated | failed
```

## Rules

- Write intermediate state after each step — pipeline is resumable
- Never skip validation before normalization
- Never classify without normalization
- Every output tagged with `pack_id` + `pack_version`
- Log per step: step name, start time, duration, status, error, `corpus_id`, `pack_id`, `pack_version`
