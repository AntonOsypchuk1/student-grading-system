# Story 02 — Benchmark dataset and experiment runner

## Objective

Compare grading approaches and later models using the same frozen documents,
rubrics and reference grades. Reuse `EvaluateDocument` and the registered
`open-answer-v1` pipeline; do not change production grading, the database or UI.

`SGS.Experiments` is a .NET 9 console project. Preparation is offline,
including optional native OCR. A run calls the existing semantic service over
gRPC. No live-model call occurs until the operator explicitly invokes `run`.

## Dataset

`SGS.Experiments/experiments/datasets/smoke-v1.json` contains three constructed theoretical
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

## Configure an experiment

Edit `SGS.Experiments/appsettings.json`. Its `Experiment` section holds the
dataset manifest, prepared snapshot and run output paths, extraction mode,
OCR and chunking settings, service URL, pipeline, model label, repetitions,
per-call timeout and optional service revision.

Copy `SGS.Experiments/.env.example` to `SGS.Experiments/.env` and fill
`Experiment__ApiKey` with the semantic service's `Auth:ApiKey` (not the model
provider key). The file is git-ignored and is not copied to the build/publish
output. Any setting can be overridden with a double-underscore environment
variable such as `Experiment__Ocr__Languages=eng+ukr`. The optional dotenv
reader supports one `KEY=VALUE` per line, whole-line comments, and optional
matching quotes; no shell expansion or inline comments.

Precedence (highest first): legacy positional CLI arguments, actual process
environment variables, project `.env`, then `appsettings.json`. Legacy
`SGS_EXPERIMENT_API_KEY`, `SGS_EXPERIMENT_REVISION`, `SGS_OCR_TESSDATA_PATH`,
`SGS_OCR_NATIVE_LIBRARY_DIRECTORY` and `SGS_OCR_LANGUAGES` still work.

For development, the runner reads configuration from the experiment project
directory independently of the working directory. Dataset, prepared snapshot
and output paths in configuration are relative to that directory (absolute
paths also work). For a standalone published executable, these paths and the
configuration files are relative to the executable directory. Copy the dataset
and sources or configure their absolute paths. Submission file paths **inside
a manifest** remain relative to that manifest. OCR runtime paths remain
relative to the executable directory, consistent with the application OCR
provider; use absolute overrides for external native libraries.

## Prepare input once

From the solution root:

```powershell
dotnet run --project SGS.Experiments -- prepare
```

`native` uses the structured text layer. `ocr` adds screenshot text using the
existing Tesseract implementation and portable `tessdata` folder. To prepare
both modes, change `ExtractionMode` and `PreparedPath` for the second snapshot.
No service API key is needed for preparation. For Linux native library setup
see `THESIS_STORY01_IMAGE_OCR.md`. Use the same language files and chunking
settings for comparable runs. Keep the prepared snapshot immutable and archive
the original manifest and source files privately.

The snapshot contains source/manifest SHA-256 hashes, extraction mode, language
configuration, chunking parameters, OCR segmentation mode, canonical sources,
chunks and non-overlapping full-document
text (not concatenated overlapping chunks). Inline and TXT documents also use
the existing chunker. Retain extraction status and warnings; unreadable input
must not be graded. Partial extraction remains visible. OCR ordering and image
semantics remain known limitations. A 100,000-character RPC limit is enforced
without truncation.

## Run the existing baseline

Start `SemanticAnalysis.Service` with one configured model and valid service
API key as described in its README. Configure the same service key in the
runner's `.env`. Specify the actual provider/model/deployment as
`Experiment:ModelLabel` and optionally set `Experiment:Revision` to the
service commit. Do not commit credentials.

```powershell
dotnet run --project SGS.Experiments -- run
```

The service selects its model independently; the label is operator-supplied
research metadata, not a model switch or independently verified model ID.
Run different pipelines against the identical snapshot with the model held
constant first. Change the provider/model only in the later model comparison,
and use a separate output directory for every run. The runner repeats every
case, interleaves repetitions and never sends reference scores to the model.
Each call uses the configured deadline (120 seconds by default); RPC failures and missing grades are
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

For the comparative pipelines, additional stress dataset and offline replay
command, see [Story 03](THESIS_STORY03_GRADING_STRATEGIES.md).

```powershell
dotnet test SGS.ServicesTests --filter "FullyQualifiedName~ExperimentRunnerTests|FullyQualifiedName~ExperimentConfigurationTests"
```

The tests use a fake evaluator to verify stable preparation, source offsets,
validation, mixed grading scales, repetition, missing scores, transport failure,
and separated reference provenance. They incur no model cost.

Local Linux smoke validation against main `1ab8b37`: both prepare modes
completed on all four cases; `lab1.pdf` remained Partial and grew from 8 native
chunks to 31 combined chunks, with 22 OCR sources. The runner also recorded four
RPC failures against a deliberately unavailable loopback endpoint, with null
grades and no fabricated error metrics. No live model was invoked.
