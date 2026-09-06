# Proposed Thesis Structure

## Introduction

- relevance;
- problem;
- object;
- subject;
- goal;
- tasks;
- methods;
- scientific novelty;
- practical value.

---

# Chapter 1 — Analysis of AI-assisted grading of open-ended student work

## 1.1 Open-ended grading problem

- subjectivity;
- teacher workload;
- explainability;
- different work types.

## 1.2 Existing grading approaches

- rule-based;
- rubric-based;
- semantic similarity;
- LLM-based;
- retrieval-augmented;
- hybrid grading.

## 1.3 Reliability problems

- nondeterminism;
- hallucination;
- overestimation;
- evidence disagreement;
- uncertainty.

## 1.4 Human-in-the-loop approaches

- selective automation;
- teacher review;
- auditability.

## 1.5 Research gap

Need for adaptive multi-evidence grading with explicit risk estimation.

---

# Chapter 2 — Development of the adaptive hybrid grading method

## 2.1 Formal problem statement

## 2.2 Common grading-evidence model

## 2.3 Evidence providers by work type

## 2.4 Adaptive evidence selection

## 2.5 Adaptive fusion model

## 2.6 Grading-risk estimator

## 2.7 Selective review policy

## 2.8 Algorithm description

---

# Chapter 3 — Software implementation

## 3.1 Existing SGS architecture

## 3.2 Role of gRPC SemanticAnalysis service

## 3.3 AIGradingService as research engine

## 3.4 Evidence-provider architecture

## 3.5 Fusion and risk components

## 3.6 Integration with SGS

## 3.7 Audit and teacher review

---

# Chapter 4 — Experimental evaluation

## 4.1 Dataset

## 4.2 Human grading protocol

## 4.3 Baselines

## 4.4 Metrics

## 4.5 Grade-quality experiment

## 4.6 Risk-estimation experiment

## 4.7 Selective-review experiment

## 4.8 Ablation study

## 4.9 Cross-work-type analysis

## 4.10 Limitations and threats to validity

---

# Conclusions

Summarize:

- method;
- implementation;
- experimental results;
- practical value;
- limitations;
- future work.
