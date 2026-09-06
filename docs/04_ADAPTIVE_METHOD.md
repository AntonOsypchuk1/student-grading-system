# Proposed Adaptive Grading Method

## 1. Common grading context

Each evaluated work is represented by:

```text
GradingContext
├── WorkType
├── Assignment
├── Rubric
├── StudentWork
├── ReferenceMaterials
└── Optional deterministic checks
```

---

## 2. Evidence abstraction

Each evidence provider outputs a common structure:

```text
GradingEvidence
├── Source
├── Criterion
├── Score
├── Confidence
├── Importance
├── Explanation
└── Metadata
```

This allows the fusion method to work with evidence without knowing whether it came from:

- tests;
- static analysis;
- retrieval;
- LLM evaluation;
- structural rules;
- semantic similarity.

---

## 3. Evidence providers

### Common providers

- `RubricEvidenceProvider`
- `LlmEvidenceProvider`
- `ReferenceConceptEvidenceProvider`
- `RetrievalEvidenceProvider`

### Programming providers

- `TestExecutionEvidenceProvider`
- `WeightedTestEvidenceProvider`
- `StaticAnalysisEvidenceProvider`

### Lab/report providers

- `RequiredSectionsEvidenceProvider`
- `ExpectedResultEvidenceProvider`
- `NumericalConsistencyEvidenceProvider`

---

## 4. Adaptive evidence selection

The evidence set depends on work type.

Example:

```text
ShortAnswer
→ rubric + reference concepts + retrieval + LLM

LabReport
→ rubric + structure checks + expected results + LLM

ProgrammingAssignment
→ rubric + tests + static analysis + LLM
```

This is the first meaning of **adaptive**.

---

## 5. Adaptive fusion

Let:

```text
E(x) = {e1, e2, ..., en}
```

be the evidence available for work `x`.

The predicted grade is:

```text
ŷ = F(E(x), rubric, workType)
```

The simplest initial implementation can use a weighted model:

```text
ŷ = Σ wi * ei
```

where weights are not globally fixed.

Instead:

```text
wi = f(workType, rubricCriterion, reliability_i)
```

This is the second meaning of **adaptive**.

Possible implementations:

- constrained linear regression;
- stacking;
- gradient boosting;
- per-work-type calibrated weighted fusion.

The thesis should prefer interpretable methods unless a more complex model gives a clear measurable advantage.

---

## 6. Disagreement features

The risk estimator should use not only raw scores but also disagreement.

Examples:

- LLM grade vs deterministic score;
- LLM correctness vs test pass rate;
- semantic coverage vs LLM confidence;
- repeated LLM evaluations disagreeing with each other;
- critical deterministic failure despite a high semantic score.

Example:

```text
LLM score = 9.0
Weighted tests = 4.5
Critical test failed = true
LLM confidence = 0.92
```

A confidence-only policy may mark this as safe.

A disagreement-aware policy should mark it as risky.

---

## 7. Risk model

Define a dangerous prediction, for example:

```text
|predicted_grade - teacher_grade| > 1
```

Then estimate:

```text
Risk(x) = P(dangerous error | evidence features)
```

Possible models:

- logistic regression;
- calibrated tree model;
- isotonic/calibration layer.

---

## 8. Review policy

Example policy:

```text
risk < T1
→ low-risk recommendation

T1 <= risk < T2
→ recommended teacher review

risk >= T2
→ mandatory teacher review
```

This is the third meaning of **adaptive**.

The thresholds should be selected empirically on validation data.

---

## 9. Why this is stronger than the existing guardrails

Existing fixed guardrails use explicit rules such as:

```text
runtime error → score cap
low weighted pass rate → score cap
```

The new method instead learns/calibrates:

- which combinations of signals are unreliable;
- when evidence disagreement matters;
- what review threshold achieves a target quality/coverage tradeoff.

Therefore the fixed guardrail logic becomes a baseline rather than the final method.
