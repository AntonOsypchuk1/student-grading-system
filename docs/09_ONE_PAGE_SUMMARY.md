# Master Thesis — One-Page Summary

## Working title

**Розробка адаптивного методу гібридного ШІ-оцінювання студентських робіт відкритого типу з оцінюванням ризику помилки**

## Starting point

Previous work already includes:

- Student Grading System as a complete application;
- semantic analysis based on LLMs;
- a separate gRPC AI service;
- multiple grading baselines for programming assignments;
- test-aware LLM grading;
- deterministic guardrails;
- human-in-the-loop product requirements.

## Problem

Existing grading strategies are either:

- specialized for one work type;
- fixed;
- highly dependent on LLM output;
- unable to estimate whether a specific grade is unsafe.

Different open-ended works provide different evidence, so one universal fixed grading strategy is insufficient.

## Proposed solution

Develop an adaptive hybrid method that:

1. collects heterogeneous grading evidence;
2. selects evidence based on work type;
3. combines evidence adaptively;
4. estimates grading error risk;
5. sends high-risk cases to the teacher.

## Supported work types

Recommended thesis scope:

- short/theoretical answers;
- laboratory/report answers;
- programming assignments.

## Core method

```text
Student work
    ↓
Work-type-specific evidence providers
    ↓
Adaptive evidence fusion
    ↓
Predicted grade
    +
Disagreement / confidence features
    ↓
Risk estimator
    ↓
Selective teacher review
```

## Scientific contribution

The contribution is not another LLM prompt.

It is:

- a common model of grading evidence;
- adaptive evidence fusion;
- disagreement-aware grading-risk estimation;
- empirical selective-review policy.

## Main hypotheses

- Adaptive fusion improves agreement with expert grading.
- Disagreement-aware risk estimation detects unsafe AI grades.
- Work-type adaptation outperforms one fixed grading strategy.
- Selective review reduces teacher workload while preserving grading quality.

## Experiment

Target: about 200–300 submissions across three work types.

Compare against:

- existing rule-based baselines;
- LLM-only;
- fixed-weight hybrid;
- guarded LLM;
- retrieval-enhanced LLM where appropriate.

Measure:

- MAE / Within ±1 / QWK;
- calibration and Brier score;
- dangerous-error detection;
- teacher review rate;
- automatic coverage;
- latency and cost.

## Final research story

**SGS product → gRPC AI architecture → grading-method experiments → adaptive risk-aware grading framework for open-ended student work.**
