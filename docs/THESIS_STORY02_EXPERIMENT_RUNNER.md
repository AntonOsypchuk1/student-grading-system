# Story 02 — Benchmark dataset and experiment runner

## Objective

Compare grading approaches and later models using the same frozen documents,
rubrics and reference grades. Reuse `EvaluateDocument` and the registered
`open-answer-v1` pipeline; do not change production grading, the database or UI.

`SGS.Experiments` is a .NET 9 console project. Preparation is offline,
including optional native OCR. A run calls the existing semantic service over
gRPC. No live-model call occurs until the operator explicitly invokes `run`.

## Dataset

`experiments/datasets/smoke-v1.json` contains three constructed theoretical
answers (correct, partial, and deliberately incorrect keyword-rich negation)
and the existing real `lab1.pdf` fixture with a pilot rubric but **no teacher
grade**. These are engineering smoke cases, not an empirical benchmark.

Every case specifies a unique ID, work type (`theoretical`, `laboratory`, or
`programming`), assignment question, maximum grade, rubric criteria (relative
weights, criterion scales, and expected concepts), exactly one `submissionFile`
(PDF/DOCX/UTF-8 TXT; relative to the manifest) or inline `submissionText`, and
reference provenance. Reference kinds:

- `teacher`: a real grade on the same rubric and total scale, with provenance
  in `note` (anonymous rater IDs, rubric version, adjudication when applicable).
- `synthetic`: an intentionally constructed answer and illustrative label.
- `unlabeled`: no reference grade; `score` must be null.

Example reference entry for a *template* (replace with actual teacher grade):

```json
"reference": { "kind": "teacher", "score": 8, "note": "Rater T01; rubric v1" }
```

Before measuring quality, collect anonymized real samples and their actual
scores from the instructor. A feasible first pilot is 20–30 works across
several grade bands and at least two assignment types, with an independent
second rater for a subset. Do not treat repeated model predictions as extra
independent student submissions. Use a larger set where possible; freeze
development and held-out test partitions before tuning risk or fusion.
Programming evidence/test execution is not part of this story.

## Prepare input once

From the solution root:

```powershell
dotnet run --project SGS.Experiments -- prepare experiments/datasets/smoke-v1.json experiments/prepared/smoke-native.json native
dotnet run --project SGS.Experiments -- prepare experiments/datasets/smoke-v1.json experiments/prepared/smoke-ocr.json ocr
```

`native` uses the structured text layer. `ocr` adds screenshot text using the
existing Tesseract implementation and portable `tessdata` folder. Optional
environment variables: `SGS_OCR_LANGUAGES` (default `eng`),
`SGS_OCR_TESSDATA_PATH`, `SGS_OCR_NATIVE_LIBRARY_DIRECTORY` (Linux setup in
`THESIS_STORY01_IMAGE_OCR.md`). Use the same language files for all comparable
runs. Keep the prepared snapshot immutable and archive the original manifest
and source files privately.

The snapshot contains source/manifest SHA-256 hashes, extraction mode, language
configuration, canonical sources, chunks and non-overlapping full-document
text (not concatenated overlapping chunks). Inline and TXT documents also use
the existing chunker. Retain extraction status and warnings; unreadable input
must not be graded. Partial extraction remains visible. OCR ordering and image
semantics remain known limitations. A 100,000-character RPC limit is enforced
without truncation.

## Run the existing baseline

Start `SemanticAnalysis.Service` with one configured model and valid service
API key as described in its README. In a separate PowerShell session, set
`SGS_EXPERIMENT_API_KEY` to that service key (not the model provider key). Do not
commit credentials. Specify the actual provider/model/deployment as the model
label and optionally set `SGS_EXPERIMENT_REVISION` to the service commit.

```powershell
dotnet run --project SGS.Experiments -- run experiments/prepared/smoke-ocr.json experiments/runs/smoke-baseline-01 http://localhost:5042 open-answer-v1 azure-gpt-4.1 3
```

The service selects its model independently; the label is operator-supplied
research metadata, not a model switch or independently verified model ID.
Run different pipelines against the identical snapshot with the model held
constant first. Change the provider/model only in the later model comparison,
and use a separate output directory for every run. The runner repeats every
case, interleaves repetitions and never sends reference scores to the model.
Each call has a 120-second deadline; RPC failures and missing grades are
recorded instead of becoming zero. Use HTTPS for non-loopback endpoints.

## Output and metrics

| Artifact | Contents |
| --- | --- |
| `run.json` | Prepared input hash, dataset version, pipeline, declared model, repetitions, time and optional revision |
| `results.jsonl` | One row per attempt: case, reference, score, extraction status, latency, failure, risk/review and full gRPC response |
| `summary.json` | Metrics by reference provenance, and separately by provenance/work type |

Scores are normalized to 0–100 percentage points before computing MAE, RMSE,
signed bias, within 10 percentage points, over/underestimation and repeat score
range. MAE/RMSE are calculated over successful labeled attempts only, and must
be reported beside failure rate and denominators. Unlabeled cases have null error
metrics. Synthetic cases are never pooled with teacher grades. Review rate
includes failures; automatic coverage is the fraction of all attempts scored
without review. Latency measures RPC time, not extraction or end-to-end upload.
Mean repeated score range includes only cases with at least two valid scores.
Attempt-level descriptive metrics do not establish statistical significance.

Do not interpret the uncalibrated risk indicator as an error probability. Risk
calibration, QWK, risk-coverage curves, retrieval quality, cost/token telemetry
and paired statistical evaluation will be implemented in later stories when
sufficient real labels and service telemetry are available.

The runner refuses to overwrite existing files and directories containing
results. Interrupted runs retain completed JSONL lines but may lack a summary.
Only use authorized/anonymized work: prepared data and raw responses can contain
student text or screenshot contents. `experiments/private`, `prepared`, and
`runs` are git-ignored; they are not safe to publish automatically.

## Tests

```powershell
dotnet test SGS.ServicesTests --filter FullyQualifiedName~ExperimentRunnerTests
```

The tests use a fake evaluator to verify stable preparation, source offsets,
validation, mixed grading scales, repetition, missing scores, transport failure,
and separated reference provenance. They incur no model cost.

Local Linux smoke validation against main `1ab8b37`: both prepare modes
completed on all four cases; `lab1.pdf` remained Partial and grew from 8 native
chunks to 31 combined chunks, with 22 OCR sources. The runner also recorded four
RPC failures against a deliberately unavailable loopback endpoint, with null
grades and no fabricated error metrics. No live model was invoked.
