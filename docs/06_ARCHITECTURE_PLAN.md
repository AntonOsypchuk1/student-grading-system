# Architecture Plan

## 1. Repository roles

Do not immediately merge all repositories.

Recommended roles:

### `student-grading-system`

The production/demo application:

- users;
- assignments;
- rubrics;
- submissions;
- teacher review;
- audit;
- UI.

### `AIGradingService`

The research and grading engine:

- evidence providers;
- grading baselines;
- adaptive fusion;
- risk model;
- metrics;
- experimental endpoints.

### `SGS.SemanticAnalysis`

Can either remain the LLM/semantic-analysis service or evolve into the broader grading service depending on how much refactoring is justified.

---

## 2. Suggested internal structure

```text
AIGrading.Core
├── Evidence
│   ├── IGradingEvidenceProvider
│   ├── GradingEvidence
│   └── GradingContext
│
├── Fusion
│   ├── IGradeFusionStrategy
│   ├── FixedWeightFusion
│   └── AdaptiveFusion
│
├── Risk
│   ├── IRiskEstimator
│   ├── GradingRiskResult
│   └── ReviewPolicy
│
└── Evaluation
    ├── Metrics
    └── ExperimentRunner

AIGrading.Text
├── RubricEvidenceProvider
├── SemanticEvidenceProvider
├── ReferenceConceptEvidenceProvider
├── RetrievalEvidenceProvider
└── TextLlmEvidenceProvider

AIGrading.Programming
├── TestExecutionEvidenceProvider
├── WeightedTestEvidenceProvider
├── StaticAnalysisEvidenceProvider
└── ProgrammingLlmEvidenceProvider

AIGrading.Reports
├── RequiredSectionsEvidenceProvider
├── ExpectedResultEvidenceProvider
└── ReportLlmEvidenceProvider
```

---

## 3. Final SGS response contract

Example:

```json
{
  "predictedScore": 8.4,
  "risk": 0.18,
  "reviewPolicy": "recommended",
  "criteria": [],
  "evidence": [],
  "riskFactors": [],
  "explanation": ""
}
```

Teacher actions:

- accept;
- edit;
- add comment;
- confirm final result.

Audit should store:

- AI score;
- risk;
- evidence;
- model/prompt version;
- rubric version;
- teacher score;
- teacher modification;
- timestamp.

---

## 4. Role of RAG

RAG should not be the thesis topic by itself.

It should be one optional evidence provider for textual/open-ended work:

```text
student answer
+ rubric
+ retrieved course material
→ semantic/LLM evidence
```

This keeps previous retrieval experiments useful without letting them dominate the thesis.
