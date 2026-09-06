# Big Picture

## 1. What has already been done

### Stage 1 — Bachelor SGS

The Student Grading System established the main product idea:

- teachers create assignments and grading criteria;
- students submit work;
- AI assists with evaluation;
- the teacher retains the final decision;
- results are explained and auditable;
- the system has a real backend/frontend architecture.

This is the **application platform**.

---

### Stage 2 — Reengineering coursework

The semantic-analysis module was extracted into an independent gRPC microservice.

This work contributed:

- service isolation;
- loose coupling;
- independent deployment/scaling of AI processing;
- gRPC contracts;
- a client abstraction;
- tests around the new architecture.

This is the **technical architecture foundation**.

---

### Stage 3 — AIGradingService coursework

The grading problem was narrowed to programming assignments in order to run controlled experiments.

Implemented/compared approaches included:

- equal-weight test grading;
- weighted test grading;
- static analysis;
- hybrid rule-based grading;
- LLM-only grading;
- test-aware LLM grading;
- guarded LLM grading.

This is the **experimental grading laboratory**.

---

### Stage 4 — Master thesis

The master thesis generalizes the previous results.

Instead of focusing only on programming assignments, it targets **student works of open-ended type** and asks:

> How can different evidence sources be combined adaptively, and how can the system estimate when its own grading decision is too risky to trust?

The new method should support multiple work categories, for example:

- short/theoretical answers;
- laboratory/report-style answers;
- programming assignments.

The method does not need to support every possible student work type. It only needs to demonstrate that one grading framework can adapt to multiple categories.

---

## 2. The complete research story

```text
Bachelor SGS
    ↓
General AI-assisted grading platform
    ↓
Reengineering coursework
    ↓
SemanticAnalysis becomes independent gRPC service
    ↓
AIGradingService coursework
    ↓
Controlled comparison of grading strategies for code
    ↓
Master thesis
    ↓
Adaptive hybrid grading across open-ended work types
    +
grading-risk estimation
    +
selective human review
```

---

## 3. What is NOT the master's contribution

The thesis should not be framed as:

- adding more LLM models;
- adding another prompt;
- adding RAG as a standalone feature;
- adding microservices;
- improving the UI;
- merging two repositories;
- simply integrating the existing AIGradingService into SGS.

Those can be implementation tasks, but not the central scientific contribution.

---

## 4. What IS the master's contribution

The research contribution is a method that:

1. collects grading evidence from different sources;
2. selects evidence depending on work type;
3. combines evidence adaptively;
4. estimates grading risk;
5. decides whether teacher review is required;
6. is experimentally compared against existing fixed strategies.

This gives a clean progression from software engineering work to a proper master's research problem.
