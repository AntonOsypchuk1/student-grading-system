# Implementation Roadmap

## Phase 0 — Supervisor alignment

Before coding:

- approve the theme wording;
- approve supported work categories;
- approve the scientific contribution;
- agree on expected dataset size;
- agree on acceptable novelty wording.

Deliverable:
`Research Specification v1`.

---

## Phase 1 — Freeze previous work

Document existing systems as baselines.

Tasks:

- freeze current SGS state;
- freeze current AIGradingService baselines;
- document existing metrics;
- preserve current datasets/results.

Goal:
prevent old and new work from becoming mixed.

---

## Phase 2 — Common grading abstraction

Implement:

- `GradingContext`;
- `GradingEvidence`;
- `IGradingEvidenceProvider`;
- common rubric model;
- work-type abstraction.

Goal:
make programming only one specialization.

---

## Phase 3 — Text/open-answer providers

Implement at least:

- rubric-based LLM provider;
- reference concept provider;
- optional retrieval provider.

Goal:
support theoretical answers.

---

## Phase 4 — Report/lab providers

Implement:

- required sections;
- deterministic expected-value checks where possible;
- LLM semantic evaluation.

Goal:
support a second non-programming work category.

---

## Phase 5 — Migrate programming grading

Wrap existing:

- test execution;
- weighted tests;
- static analysis;
- test-aware LLM

as evidence providers.

Goal:
reuse old work instead of rewriting it.

---

## Phase 6 — Dataset and annotation

Collect:

- assignments;
- rubrics;
- student submissions;
- teacher grades;
- second-grader subset.

Create train/validation/test split.

---

## Phase 7 — Fixed fusion baselines

Implement:

- simple average;
- manually weighted fusion;
- work-type-specific fixed weights.

Goal:
create strong baselines.

---

## Phase 8 — Adaptive fusion

Implement and calibrate:

- adaptive weights;
- criterion-specific fusion;
- work-type-specific reliability.

Goal:
main scientific contribution #1.

---

## Phase 9 — Risk estimation

Create features:

- evidence disagreement;
- confidence;
- critical failures;
- historical residuals;
- work type.

Train/calibrate risk estimator.

Goal:
main scientific contribution #2.

---

## Phase 10 — Selective review

Choose thresholds using validation data.

Measure:

- automatic coverage;
- review rate;
- dangerous-error capture.

Goal:
main scientific contribution #3.

---

## Phase 11 — Full experiment

Run:

- baseline comparison;
- adaptive-vs-fixed comparison;
- risk calibration;
- ablation;
- selective review;
- cross-domain analysis.

---

## Phase 12 — SGS integration

Integrate final method through the existing service architecture.

UI should show:

- score;
- explanation;
- risk;
- review reason;
- evidence summary.

---

## Phase 13 — Thesis writing

Write the thesis from experiment artifacts rather than trying to reconstruct results later.
