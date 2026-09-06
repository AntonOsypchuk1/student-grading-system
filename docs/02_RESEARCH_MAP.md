# Research Map

## Existing assets and their role

| Existing asset | What already exists | Role in the master thesis |
|---|---|---|
| `student-grading-system` | Full SGS application | Product shell / final integration target |
| `SGS.SemanticAnalysis` | LLM-based semantic analysis | AI service foundation |
| gRPC reengineering work | Service isolation and contracts | Deployment architecture |
| `AIGradingService` | Grading baselines and metrics | Research harness / benchmark framework |
| Programming test pipeline | Runtime tests, weighted tests | Programming evidence provider |
| Static analysis | Structural code checks | Programming evidence provider |
| LLM-only grading | Semantic grading | Baseline |
| Test-aware LLM | LLM + execution evidence | Strong baseline |
| Guardrails | Fixed deterministic safety rules | Baseline for new adaptive risk policy |
| Rubrics | Criterion-based grading | Common evaluation structure |
| RAG/retrieval experiments | Retrieval, chunking, reranking | Optional evidence provider for textual tasks |
| Human-in-the-loop workflow | Teacher confirmation | Final decision policy |

---

## Research layers

### Layer A — Domain

Open-ended student work:

- theoretical answers;
- lab/report responses;
- programming assignments.

### Layer B — Evidence providers

Examples:

- rubric matching;
- semantic similarity;
- reference concept coverage;
- retrieval from course materials;
- LLM criterion evaluation;
- test execution;
- static analysis;
- structural checks;
- numerical/checkable-result validation.

### Layer C — Evidence fusion

The proposed method combines the available evidence.

### Layer D — Risk estimation

The system estimates how likely the predicted grade is to be wrong.

### Layer E — Decision policy

The system chooses between:

- low-risk recommendation;
- recommended teacher review;
- mandatory teacher review.

### Layer F — Product integration

SGS presents:

- predicted score;
- explanation;
- evidence;
- risk;
- review requirement;
- teacher correction;
- audit trail.
