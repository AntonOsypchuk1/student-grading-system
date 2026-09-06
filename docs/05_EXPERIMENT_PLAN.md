# Experiment Plan

## 1. Supported work categories

Do not attempt to validate every open-ended work type.

Recommended scope:

1. **Short/theoretical answers**
2. **Laboratory/report answers**
3. **Programming assignments**

This is enough to demonstrate cross-domain adaptability.

---

## 2. Dataset target

A realistic target:

```text
200–300 total submissions

~100 theoretical answers
~70–100 laboratory/report answers
~80–100 programming assignments
```

The exact distribution may change depending on data availability.

Preferred data sources:

- anonymized real student work;
- teacher-created reference samples;
- controlled synthetic variants only where real data is unavailable.

---

## 3. Human reference grading

At least part of the dataset should be independently graded by more than one human evaluator.

This allows:

- inter-rater agreement;
- adjudicated/reference grade;
- more credible evaluation than a single self-assigned expected score.

---

## 4. Baselines

### Existing programming baselines

- equal-weight tests;
- weighted tests;
- static analysis;
- hybrid rule-based;
- LLM-only;
- test-aware LLM;
- guarded LLM.

### Cross-domain baselines

- rubric-only LLM;
- LLM + reference answer;
- LLM + retrieval context;
- fixed-weight hybrid;
- per-domain fixed strategy.

### Proposed method

- adaptive evidence fusion;
- disagreement-aware risk estimation;
- selective review policy.

---

## 5. Metrics

### Grading quality

- MAE;
- RMSE;
- Within ±1;
- weighted Cohen's kappa / QWK;
- overestimation rate;
- underestimation rate;
- max error.

### Reliability / risk

- Brier score;
- AUROC for dangerous-error detection;
- calibration error;
- precision/recall of risky-case detection.

### Human workload

- review rate;
- automatic coverage;
- risk-coverage curve;
- percentage of dangerous errors intercepted.

### Operational metrics

- latency;
- number of LLM calls;
- token cost;
- failure rate.

---

## 6. Key experiments

### Experiment A — Single-source comparison

Compare isolated evidence sources.

Purpose:
understand which sources are useful for which work types.

### Experiment B — Fixed hybrid vs adaptive hybrid

Compare:

- fixed weights;
- work-type-specific fixed weights;
- adaptive/calibrated fusion.

Purpose:
test H1 and H3.

### Experiment C — Risk estimation

Compare:

- LLM self-confidence only;
- fixed guardrails;
- disagreement-aware risk model.

Purpose:
test H2.

### Experiment D — Selective review

Evaluate:

```text
quality vs % of works sent to teacher
```

Purpose:
test H4.

### Experiment E — Ablation

Remove one evidence source at a time.

Purpose:
determine what actually contributes to performance.

### Experiment F — Cross-work-type generalization

Compare whether one universal policy or work-type-specific adaptation performs better.
