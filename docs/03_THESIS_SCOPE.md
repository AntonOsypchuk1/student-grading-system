# Thesis Scope

## Working title

> **Розробка адаптивного методу гібридного ШІ-оцінювання студентських робіт відкритого типу з оцінюванням ризику помилки**

Alternative title:

> **Адаптивний метод гібридного ШІ-оцінювання студентських робіт відкритого типу на основі комбінування різнорідних оцінювальних сигналів**

The first version is preferable because it clearly includes the risk-estimation component.

---

## Problem statement

Different types of open-ended student work require different grading signals.

A programming assignment may provide:

- test results;
- static-analysis findings;
- rubric-based semantic evaluation.

A theoretical answer may provide:

- rubric criteria;
- reference concepts;
- retrieved course context;
- LLM-based semantic evaluation.

A laboratory/report response may additionally provide:

- structural requirements;
- expected values;
- mandatory sections.

A fixed grading strategy is therefore unlikely to be optimal across all work types.

At the same time, LLM-based grading is non-deterministic and may disagree with deterministic evidence. A practical grading system should therefore estimate not only a score, but also the **risk that the score is wrong**.

---

## Object of research

The process of AI-assisted evaluation of student works of open-ended type.

---

## Subject of research

Methods for adaptive combination of heterogeneous grading signals and estimation of grading-decision risk in AI-assisted evaluation systems.

---

## Goal

To develop and experimentally evaluate an adaptive hybrid grading method that combines heterogeneous grading evidence depending on the type of student work and estimates the risk of an incorrect grading decision in order to support selective human review.

---

## Main research tasks

1. Analyze existing approaches to automated and AI-assisted grading of open-ended student work.
2. Formalize a common evidence representation for different grading sources.
3. Define categories of supported student work and evidence applicable to each category.
4. Develop an adaptive evidence-fusion method.
5. Develop a grading-risk estimator.
6. Define a selective-review policy based on estimated risk.
7. Build a research dataset covering several work types.
8. Compare the proposed method with existing rule-based and LLM-based baselines.
9. Evaluate grading quality, reliability, review workload, latency, and cost.
10. Integrate the resulting method into the SGS architecture.

---

## Expected scientific novelty

Potential novelty should be claimed carefully and only after literature review.

The intended contribution is:

- a common representation of heterogeneous grading evidence;
- adaptive selection/weighting of evidence by work type and criterion;
- a risk-estimation mechanism that uses not only LLM confidence but also disagreement between evidence sources;
- a selective-review policy driven by empirical risk rather than fixed manual caps.

---

## Hypotheses

### H1

Adaptive combination of heterogeneous grading signals improves agreement with expert grading compared with isolated rule-based or LLM-only methods.

### H2

A grading-risk estimator based on evidence disagreement can identify a substantial share of unsafe AI grades.

### H3

Adapting evidence composition and weights to the type of student work performs better than a single fixed grading strategy across all work types.

### H4

Selective human review based on estimated risk can reduce teacher workload while preserving a predefined quality threshold.
