# Student Math Test System

An ASP.NET Core 8 Web API with Razor Pages UI that automatically grades student arithmetic exams uploaded as XML files by teachers.

## Architecture

```
StudentMathTest.sln
├── StudentMathTest.Api          → ASP.NET Core Web API, Razor Pages UI, Swagger, DI wiring
├── StudentMathTest.Domain       → Models, Interfaces (zero external dependencies)
├── StudentMathTest.Application  → EF Core DbContext, services, DTOs
├── StudentMathTest.MathEngine   → Independent arithmetic expression evaluator
└── StudentMathTest.Tests        → xUnit unit tests (MathEngine, XML parsing, ExamService)
```

**Tech stack:** .NET 8, ASP.NET Core, Razor Pages, Entity Framework Core 8, SQLite, Swashbuckle/Swagger, xUnit, Moq

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run

```bash
cd src/StudentMathTest.Api
dotnet run
```

The home page opens at **http://localhost:5284** (or the port shown in the console).
Swagger API docs are available at `/swagger`.
The SQLite database (`mathtest.db`) is created automatically on first run.

Or open `StudentMathTest.sln` in Rider/Visual Studio and press Run.

### Run Tests

```bash
dotnet test tests/StudentMathTest.Tests
```

40 unit tests cover the math engine, XML parsing, and exam grading logic.

## UI Pages

| URL | Description |
|-----|-------------|
| `/` | Home page |
| `/Teachers` | Create teachers, manage students, upload XML |
| `/Students` | Enter student ID to view exam results |
| `/swagger` | Full API docs |

## API Endpoints

### Teacher Management
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/teachers` | List all teachers |
| `POST` | `/api/teachers` | Create a teacher |
| `GET` | `/api/teachers/{teacherId}/students` | List students for a teacher |
| `POST` | `/api/teachers/{teacherId}/students` | Add a student to a teacher |

### Exam Upload & Grading
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/exams/upload` | Upload XML file — grades all exams, returns summary |
| `GET` | `/api/exams/{examId}` | Get full graded result for one exam |

### Student Analytics
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/students/{studentId}/exams` | List all exams for a student with scores |
| `GET` | `/api/students/{studentId}/exams/{examId}` | Detailed per-task breakdown |

### Math Engine (direct)
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/math/evaluate?expression=6*2+3-4` | Evaluate any arithmetic expression |

### Third-Party Integration
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/integration/health` | Health check |
| `POST` | `/api/integration/process` | Grade XML (file upload) — stateless, no DB write |
| `POST` | `/api/integration/process/raw` | Grade XML (raw body, `text/xml`) — stateless, no DB write |

## End-to-End Demo Walkthrough

### Step 1 — Create a Teacher

1. Go to `/Teachers`
2. Enter a name and click **Add Teacher**
3. Note the auto-assigned Teacher ID shown in the table (e.g. `#1`)

> **Important:** The `Teacher ID` in your XML file must match the ID assigned here.
> If the system gave the teacher ID `1`, your XML must have `<Teacher ID="1">`.

### Step 2 — Add Students manually (optional)

1. Click **Manage Students** next to the teacher
2. Add two students or how many you want

> Students referenced in the XML are also auto-created on upload if they don't exist yet.

### Step 3 — Create the exam XML file

Save the following as `exam.xml`. **Replace `Teacher ID="1"` with the actual ID from Step 1.**

```xml
<Teacher ID="1">
  <Students>
    <Student ID="12345">
      <Exam Id="1">
        <Task id="1"> 2+3/6-4 = 74 </Task>
        <Task id="2"> 6*2+3-4 = 22 </Task>
        <Task id="3"> 10*2-5 = 99 </Task>
        <Task id="4"> 8/2+2 = 99 </Task>
      </Exam>
    </Student>
    <Student ID="54321">
      <Exam Id="1">
        <Task id="1"> 2+3/6-4 = -1.5 </Task>
        <Task id="2"> 6*2+3-4 = 11 </Task>
        <Task id="3"> 10*2-5 = 15 </Task>
        <Task id="4"> 8/2+2 = 6 </Task>
      </Exam>
    </Student>
  </Students>
</Teacher>
```

> Student `12345` has all **wrong** answers (0%). Student `54321` has all **correct** answers (100%).

### Step 4 — Upload the XML (mass grading)

1. Go to `/Teachers` → click **Upload Exam** next to your teacher
2. Choose `exam.xml` and click **Grade Exams**
3. Results appear immediately: student `12345` scores **0%** (red), student `54321` scores **100%** (green)

### Step 5 — View detailed task breakdown

1. Click **View Detail** on the student `12345` row — every task row is red with the correct answer shown
2. Click **View Detail** on the student `54321` row — every task row is green

### Step 6 — Student self-service analytics

1. Go to `/Students`
2. Enter student ID `12345` → **View My Exams** → **View Tasks**

### Step 7 — Math Engine (direct evaluation)

Go to `/swagger` → **Math** → `GET /api/math/evaluate` → **Try it out**:

| Expression | Expected result |
|-----------|-----------------|
| `6*2+3-4` | `11` |
| `2+3/6-4` | `-1.5` |
| `10*2-5` | `15` |
| `8/2+2` | `6` |

### Step 8 — Third-party integration endpoint

In Swagger → **Integration**:

1. `GET /api/integration/health` → returns `{ "status": "healthy" }`
2. `POST /api/integration/process` → upload `exam.xml` → graded JSON, nothing saved to DB

### Feature coverage summary

| Requirement | Demonstrated in |
|---|---|
| Teacher UI — manage students | Steps 1–2 |
| Teacher UI — upload XML | Step 4 |
| Automatic mass test checking | Step 4 (two students graded at once) |
| Math engine with correct precedence | Steps 4, 7 |
| Student analytics UI | Steps 5–6 |
| Independent integration point | Step 8 |
| Independent math processor | Step 7 |
| Microsoft technologies (EF Core, Razor Pages, ASP.NET Core, SQLite) | Throughout |

---

## XML Schema

```xml
<Teacher ID="11111">
  <Students>
    <Student ID="12345">
      <Exam Id="1">
        <Task id="1"> 2+3/6-4 = 74 </Task>
        <Task id="2"> 6*2+3-4 = 22 </Task>
      </Exam>
    </Student>
  </Students>
</Teacher>
```

Tasks follow the format: `expression = studentAnswer`

## Math Engine

The `ArithmeticEvaluator` uses a recursive-descent parser with standard operator precedence (`*` and `/` before `+` and `-`):

| Expression | Student Answer | Correct Answer | Verdict |
|-----------|----------------|----------------|---------|
| `6*2+3-4` | 22 | **11** | Incorrect |
| `2+3/6-4` | 74 | **-1.5** | Incorrect |
| `6*2+3-4` | 11 | **11** | Correct |

## Assumptions

- Teacher and Student IDs in the XML are used as primary keys. If an ID doesn't exist in the database, the entity is auto-created (upsert).
- Standard operator precedence applies (`*` and `/` before `+` and `-`). Parentheses are supported by the math engine but are not used in the sample exam tasks.
- Decimal division is used (e.g. `3/6 = 0.5`).
- Uploading the same exam XML multiple times creates new exam records (no deduplication).
- No authentication or roles — the Swagger UI is fully open.
