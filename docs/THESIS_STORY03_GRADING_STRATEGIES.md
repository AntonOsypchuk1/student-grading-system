# Story 03 — Comparative grading strategies

## Research question and background

Does hybrid evidence improve grading accuracy and selective teacher review
relative to a rubric-guided LLM alone? Do not assume a hybrid must win.

Relevant primary research:

- Chamieh, Zesch & Giebermann (2024), *LLMs in Short Answer Scoring:
  Limitations and Promise of Zero-Shot and Few-Shot Approaches*,
  [BEA](https://aclanthology.org/2024.bea-1.25/). Zero- and few-shot results
  varied substantially with question complexity and domain knowledge.
  This motivates a direct LLM baseline and testing genuinely varied answers.
- Kortemeyer (2024), *Performance of the pre-trained large language model
  GPT-4 on automated short answer grading*,
  [Discover Artificial Intelligence](https://doi.org/10.1007/s44163-024-00147-y).
  Compares general-purpose grading, reference-answer availability, and
  specialized approaches on SciEntsBank and Beetle. Our baseline uses
  teacher-authored explicit rubrics, not access to the reference grade.
- Mohler & Mihalcea (2009), *Text-to-Text Semantic Similarity for Automatic
  Short Answer Grading*, [EACL](https://aclanthology.org/E09-1065/).
  Motivates semantic evidence rather than treating literal overlap as truth.
  An independent entailment/retrieval provider is future work.
- *Rubric-guided Prompting for Automatic Assessment Scoring* (2026),
  [Learning@Scale](https://doi.org/10.1145/3774398.3811581). Studies
  decomposition into key elements and rubric aggregation, with results
  dependent on model size. Our criterion-wise prompt schedule is a simpler
  inspired alternative, not a reproduction of that algorithm.
- *GradeOpt: A LLM-Powered Automatic Grading Framework with Human-Level
  Guidelines Optimization* (2025),
  [EDM](https://educationaldatamining.org/EDM2025/proceedings/2025.EDM.long-papers.80/index.html).
  Uses reflection/refinement to improve guidelines; defer automated rubric
  refinement until we have development labels and a sealed test partition.
- Karim, Wang & Yuan (2025), *Beyond the Score: Uncertainty-Calibrated LLMs
  for Automated Essay Assessment*,
  [EMNLP](https://aclanthology.org/2025.emnlp-main.992/).
  Uses conformal prediction to provide calibrated prediction sets. Our
  heuristic review score is **not** a calibrated probability or conformal
  guarantee; calibration needs independent teacher-labeled data.

These papers motivate comparisons, not claims that the SGS prototype
replicates their reported performance.

## Implemented strategies

All use the existing EvaluateDocument RPC, a frozen prepared document,
teacher-defined criterion scales and relative weights. Each criterion score
is normalized to [0,1]; the final score is the weighted mean times MaxScore.
The strategy ID is both the request pipeline and the returned MethodVersion.

| Pipeline | Evidence | Nominal model calls per evaluation |
| --- | --- | --- |
| llm-only-v1 | Joint rubric LLM evaluation | 1 |
| lexical-only-v1 | Literal keyword coverage; always review | 0 |
| fixed-hybrid-v1 | 80% LLM + 20% lexical | 1 |
| open-answer-v1 | Original adaptive baseline, unchanged formula and review | 1 |
| agreement-hybrid-v1 | Disagreement-gated lexical contribution | 1 |
| llm-criterionwise-v1 | One independently evaluated rubric criterion at a time | number of criteria |

Each LLM call may retry once on invalid JSON; transport-level retries may
also occur. Criterion-wise evaluation is sequential, fails the entire
evaluation if a criterion fails, and sends the full document on every call.
It trades more requests for simpler scoring tasks; no assumption that
accuracy or cost will improve. Retrieval is a separate story.

LLM responses must match criterion IDs/scales and contain supporting exact
quotes for positive scores. Lexical-only is a diagnostic baseline and must
not automatically assign student grades. No provider failure silently
switches the strategy or assigns zero. New strategies include sanitized
failure codes (HTTP status, invalid JSON/evidence, timeout, generic error)
and flag partial extraction or a prompt retry for teacher review.

## Proposed agreement-gated fusion (candidate, uncalibrated)

Let L be normalized semantic rubric score, K literal mention coverage,
and d = |L - K|. Define w = 0.8 + 0.2 min(d / 0.25, 1).
Candidate score: w L + (1 - w) K.

When d >= 0.25, use L unchanged and request review. When both signals agree
closely, lexical evidence has at most 20% weight. This is a conservative
heuristic: lexical coverage cannot establish claim correctness, so severe
disagreement must not increase lexical influence. A correct paraphrase with
few keywords also triggers review rather than automatic deduction. It may
still be wrong if the LLM is wrong, or if signals agree on incorrect content.
Thresholds are provisional, frozen for the first comparison, and must be
calibrated on development data rather than the held-out test set.

The previous smoke run (3 synthetic answers, 3 repeats each, one unlabeled
lab) demonstrated the negation problem: normalized L=0, K=1, original
hybrid score=0.5. Replaying recorded evidence algebraically gives:

| Strategy | MAE (percentage points, only 3 unique labeled synthetic cases) |
| --- | ---: |
| llm-only-v1 | 0.00 |
| lexical-only-v1 | 35.56 |
| fixed-hybrid-v1 | 7.11 |
| open-answer-v1 | 17.16 |
| agreement-hybrid-v1 | 0.33 |

This is a post-hoc engineering illustration on data that motivated the
candidate; **not** independent evidence of improved generalization. The
source run label was stale and 2/3 lab attempts failed; preserve those
facts and do not infer a model ID, teacher grade, or new model-call results.

## Run the expanded experiment

The new `experiments/datasets/strategy-stress-v1.json` contains 13 constructed
cases: correct literal answers, correct paraphrases, partial answers, negation,
bare keywords, a mix of correct and false claims, irrelevant content, student
prompt injection, Ukrainian paraphrase/negation, and three laboratory command/
verification cases (including incorrect interface with the expected keyword).
All references are synthetic, not teacher judgments. It is a development
stress test. Keep the original smoke dataset unchanged.

In `SGS.Experiments/appsettings.json` set:

```json
{
  "Experiment": {
    "DatasetPath": "experiments/datasets/strategy-stress-v1.json",
    "PreparedPath": "experiments/prepared/strategy-stress-native-v1.json",
    "ExtractionMode": "native",
    "Pipeline": "open-answer-v1",
    "OutputDirectory": "experiments/runs/strategy-stress-shared-01",
    "ComparisonSourceDirectory": "experiments/runs/strategy-stress-shared-01",
    "ComparisonOutputPath": "experiments/runs/strategy-stress-comparison-01.json",
    "Repetitions": 1,
    "DelayBetweenRequestsMs": 3500
  }
}
```

Merge these properties into the existing section rather than replacing it.
Set ModelLabel to the actual model ID, Revision to the service commit and
configure the service key locally. From the solution root:

```powershell
dotnet run --project SGS.Experiments -- prepare
dotnet run --project SGS.Experiments -- run
dotnet run --project SGS.Experiments -- compare
```

The final command is entirely offline. It checks the prepared SHA-256,
dataset version and every case/repetition, validates that the original
grade can be reproduced from its two evidence scores, and uses the shared
SGS.Grading rules to compare the five replayable methods on the same
successful evidence. Incomplete runs, modified snapshots, mismatched
metadata and missing criterion/source evidence are rejected. Source failures
are retained for every method; this is a conditional fusion comparison,
not a live estimate of lexical-only availability. The output contains
source hashes, declared model, paired/planned denominators, method-specific
metrics and individual case scores and review flags. No replay latency is
fabricated. No output files are overwritten. Comparison does not require
the service or a model API key.

Then set Pipeline to `llm-criterionwise-v1` and choose a new OutputDirectory
for a real independent evaluation on the identical prepared snapshot.
Also run lexical-only if measuring its true independent availability and
latency. To compare actual live pipelines individually, select the relevant
ID and a fresh output directory for each. Repetitions should be 1 for the
initial free-tier smoke, then >=3 for the main evaluation when feasible.
The 13-case dataset contains 16 rubric criteria; one shared run plus a
criterion-wise run needs at least 29 LLM requests before retries. The delay
only paces successive RPCs (not each internal model call), so it is not a
guarantee against provider or daily rate limits.

## Evaluation and acceptance

- Use MAE, RMSE, bias, within-10pp, successful/failed counts, review rate,
  automatic coverage and repeated score range. New metrics additionally
  report MAE among non-reviewed labeled results, that denominator, and
  review recall for successful errors >=20 percentage points.
- Hold rubrics, document snapshot, model, language, prompt and provider
  settings fixed; only vary the strategy. Log provider/actual model IDs
  and prompt/service revision. For criterion-wise compare independent
  live runs, not the offline fusion results as if they were new model calls.
- Collect teacher grades on at least 20–30 anonymized independent submissions
  across two work types and diverse grade bands for a pilot; use a second
  rater on a subset and adjudicate disagreements. Use more samples if
  feasible. Freeze development/test by assignment or student before tuning.
  Synthetic fixtures diagnose behavior, never replace authentic grades.
- Report case-level paired errors (aggregate repeated scores within each
  case, not independent sample inflation), coverage versus error, failure
  reasons and latency separately. Estimate uncertainty using case-level
  paired bootstrap when enough independent cases exist. Do not choose
  a winner from the small smoke set alone.
- Only after comparing approaches on one fixed model, evaluate shortlisted
  methods across models. Keep all original artifacts immutable.

## Verification

```powershell
dotnet test SGS.SemanticAnalysisTests
dotnet test SGS.ServicesTests
```

Tests use deterministic fake LLM responses to verify numeric aggregation,
failure/partial extraction review, dispatch through gRPC, quote validation,
keyword negation, paraphrasing, criterion-wise call count and offline replay.
The lexical stress test uses actual matching without paid model calls.
Live AI grading quality must still be measured separately by executing the
experiment with a configured provider.
