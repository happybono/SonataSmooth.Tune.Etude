# SonataSmooth API Reference

Comprehensive reference for the public surface of the smoothing & export components contained in : 
- SmoothingConductor.cs
- SmoothingNotation.cs
- SmoothingScore.cs
- CsvScoreRequest.cs / CsvScoreWriter.cs
- ExcelScoreRequest.cs / ExcelScoreWriter.cs

This document reflects the code exactly as implemented (no speculative APIs). Any legacy names (e.g. `SmoothingTuner`, `ExcelExportRequest`) mentioned in conditional compilation blocks are noted as legacy / compatibility artifacts and are not part of the visible .NET Standard 2.0 public surface unless you build the .NET Framework 4.8 (NET48) variant with matching source.

---

## Table of Contents
1. [Namespace Overview](#1-namespace-overview)
2. [Boundary Handling (`BoundaryMode`)](#2-boundary-handling--boundarymode-enum)
3. [Core Smoothing Engine (`SmoothingConductor`)](#3-core-smoothing-engine--smoothingconductor)
   - Tuple Output Order
   - Method : `ApplySmoothing`
   - Method : `GetValueWithBoundary`
   - Method : `CalcBinomialCoefficients`
   - Method : `ComputeGaussianCoefficients`
   - Method : `ComputeSGCoefficients`
   - Method : `ValidateSmoothingParameters`
   - Method : `GetBoundaryMethodText`
   - Method : `ExportCsvAsyncFromResults`
   - Method : `ExportCsvAsync`
   - Method : `ExportExcelAsync`
   - Internal Helpers (non‑API)
4. [Data Containers](#4-data-containers)
   - `SmoothingNotation`
   - `SmoothingScore`
5. [CSV Export Layer](#5-csv-export-layer)
   - `CsvScoreRequest`
   - `CsvExportResult`
   - `CsvScoreWriter`
   - `CSV Header Line Ordering (High-Level vs Core)`
6. [Excel Export Layer](#6-excel-export-layer)
   - `ExcelScoreRequest`
   - `ExcelScoreWriter`
   - `ExcelInteropNotAvailableException`
7. [Validation Helpers (External Dependency Note)](#7-validation-helpers-scoreconformancechecker)
8. [Error & Exception Summary](#8-error--exception-summary)
9. [Performance & Parallelism](#9-performance--parallelism)
10. [Thread Safety](#10-thread-safety)
11. [Usage Examples](#11-usage-examples)
12. [Behavioral Guarantees & Invariants](#12-behavioral-guarantees--invariants)
13. [Limitations / Edge Cases](#13-limitations--edge-cases)
14. [Version & Legacy Name Notes](#14-version--legacy-name-notes)
15. [Appendix : Quick Exception Matrix (Condensed)](#appendix--quick-exception-matrix-condensed)
16. [Addendum : Savitzky-Golay Derivative Order](#addendum--savitzky-golay-derivative-order)
17. [Summary](#summary)
---

## 1. Namespace Overview

Primary namespaces : 

| Namespace | Purpose |
|-----------|---------|
| `SonataSmooth.Tune` | Core smoothing algorithms + unified export helpers. |
| `SonataSmooth.Tune.Export` | Request / result DTOs, export support flags, score container classes, high-level CSV / Excel writers. |

---

## 2. Boundary Handling : `BoundaryMode` (enum)
```csharp
public enum BoundaryMode
{
    Symmetric,  // Mirror indices at boundaries (e.g. -1 → 0, N → N - 1)
    Replicate,  // Clamp to endpoints
    ZeroPad,    // Outside indices treated as zero
    Adaptive    // Slide window fully inside data (no synthetic samples); asymmetric near edges; SG uses asymmetric coefficients
}
```

| Mode       | Mechanism                                                      | Synthetic Samples | Edge Trade‑off                              |
|------------|----------------------------------------------------------------|-------------------|---------------------------------------------|
| Symmetric  | Reflect ‑ 1 mirror                                             | Yes (mirrored)    | Good continuity                             |
| Replicate  | Clamp endpoints                                                | Yes (cloned)      | Plateau edges                               |
| ZeroPad    | Fill with 0 outside                                            | Yes (zeros)       | Energy loss near edges                      |
| Adaptive   | Slide full window inside range; use only actual samples        | No                | Asymmetric window → slight phase shift      |
  

Resolution logic inside `GetValueWithBoundary` : 

| Mode | Negative Index Mapping | Index ≥ n Mapping | Notes |
|------|------------------------|-------------------|-------|
| Symmetric | i → -i - 1 | i → 2n - i - 1 | Secondary reflection may still produce negative for empty arrays → returns 0. |
| Replicate | 0 | n - 1 | Flat extension at edges. |
| ZeroPad | 0.0 (no access) | 0.0 (no access) | External zeros; edge averages are biased low. |


### 2.1 Adaptive Details
Adaptive windows :   
left / right extents chosen so (left + right + 1) = 2 × r + 1 and [center - left, center + right] lies entirely in [0, N - 1]. All filters consume this real-sample block; SG builds per-position asymmetric coefficients.  
The boundary sampler is largely bypassed for Adaptive. Weighted median builds value / weight pairs from the in-range sequence (no synthetic values).

> **Note** :   
> Adaptive boundary handling applies equally to derivative kernels; per-position asymmetric fits are used for all supported filters.
---

## 3. Core Smoothing Engine : `SmoothingConductor`

Static class providing multi-filter computation + export utilities.

### 3.1 Tuple Output Order

All smoothing computations return a fixed 6-tuple : 
`(Rect, Binom, Median, GaussMed, Gauss, SG)`

Each array length equals the input length `n`. If a filter is disabled, its array is still allocated and returned.

### 3.2 Method : `ApplySmoothing`
```csharp
public static (
    double[] Rect,
    double[] Binom,
    double[] Median,
    double[] GaussMed,
    double[] Gauss,
    double[] SG
) ApplySmoothing(
    double[] input,
    int r,
    int polyOrder,
    BoundaryMode boundaryMode,
    bool doRect,
    bool doAvg,
    bool doMed,
    bool doGaussMed,
    bool doGauss,
    bool doSG,
    double alpha = 1.0,
    double? sigmaFactor = null
)
```

| Parameter | Description |
|-----------|-------------|
| `input` | Source sequence (non-null). |
| `r` | Kernel radius (≥ 0). Window size = `2 × r + 1`. |
| `polyOrder` | Savitzky-Golay polynomial order (used only if `doSG`). Must satisfy `polyOrder < windowSize`. |
| `boundaryMode` | Edge sampling behavior. |
| Filter flags | `doRect`, `doAvg`, `doMed`, `doGaussMed`, `doGauss`, `doSG` control which filters run. |
| `alpha` | Blend factor applied ONLY to Binomial Average, Weighted Median (Binomial), Gaussian Weighted Median, and Gaussian outputs. Range [0, 1]. Not applied to Rectangular or Savitzky-Golay outputs. |
| `sigmaFactor` | Optional. Controls the denominator in the Gaussian sigma calculation: `sigma = windowSize / sigmaFactor`. If null (default), legacy behavior is used (`sigmaFactor = 6.0`). If set, allows custom control of Gaussian smoothing width. Applies to both standard and adaptive Gaussian / Gaussian weighted-median filters. |

**Implementation Details** : 
- Window size : `2r + 1` computed once.
- Binomial coefficients (`long[]`) computed if `doAvg || doMed`.
- Gaussian σ chosen as `(2r + 1) / 6.0` (legacy) or `(2r + 1) / sigmaFactor` (if set).
- Savitzky-Golay coefficients computed via normal equations (AᵀA inversion) and then normalized to sum = 1.
- Weighted median constructs weighted pairs using binomial weights each sample, sorts them (O(w log w)).
- Parallelization : `Parallel.For` engaged when `n >= 2000`; otherwise, sequential loop.
- Derivative Limitation : `ApplySmoothing` never computes Savitzky-Golay derivatives (SG output is always smoothing). Use `ApplySGDerivative` or export writers with `DerivOrder > 0` for derivative series.
- Alpha application :
  - Binomial Average, Weighted Median (Binomial), Gaussian Weighted Median, Gaussian : output[i] = alpha * filtered + (1 - alpha) * input[i]
  - Rectangular, SG : full-filter output without blending.
- Sigma application :
  - If `sigmaFactor` is not set, sigma = windowSize / 6.0 (legacy).
  - If `sigmaFactor` is set, sigma = windowSize / sigmaFactor (user-controlled).

**Usage Example**:
```csharp
// Custom sigmaFactor example
var (rect, binom, med, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    input: samples,
    r: 4,
    polyOrder: 3,
    boundaryMode: BoundaryMode.Symmetric,
    doRect: true,
    doAvg: true,
    doMed: true,
    doGaussMed: true,
    doGauss: true,
    doSG: true,
    alpha: 1.0,
    sigmaFactor: 12.0 // Use sigma = windowSize / 12.0
);
```

**Disabled Filters** :  
Corresponding arrays remain zeroed (for alignment). No lazy null omission.

**Exceptions** : 
- `ArgumentNullException` if `input` is null.
- `ArgumentOutOfRangeException` if `r < 0`.
- `ArgumentOutOfRangeException` if `alpha` ∉ [0, 1].
- Indirect : 
  - `CalcBinomialCoefficients` : `ArgumentException` (< 1), `ArgumentOutOfRangeException` (> 63), `InvalidOperationException` (overflow).
  - `ComputeGaussianCoefficients` : `ArgumentException` (length < 1 or σ ≤ 0), `InvalidOperationException` (sum anomaly).
  - `ComputeSGCoefficients` : `ArgumentOutOfRangeException`, `ArgumentException` (even window, order ≥ window), `InvalidOperationException` (singular / near-zero sum). 

**Filter flags (summary)**
- doRect: Rectangular mean (uniform). No alpha blending.
- doAvg: Binomial (Pascal) weighted average. Alpha applied.
- doMed: Binomial weighted median (robust). Alpha applied.
- doGaussMed: Gaussian weighted median (robust with Gaussian emphasis; local kernel in Adaptive). Alpha applied.
- doGauss: Gaussian mean (convolution). Alpha applied.
- doSG: Savitzky–Golay smoothing (and derivatives via dedicated API). No alpha blending.  

Notes:
- Adaptive boundary mode slides the window fully in-range for all filters; for doGaussMed a local Gaussian kernel is recomputed per position.
- Sigma heuristic: σ ≈ W / 6 (W = 2r + 1).

### 3.3 Method : `GetValueWithBoundary`
```csharp
public static double GetValueWithBoundary(
    double[] data,
    int idx,
    BoundaryMode mode
)
```

Resolves an out-of-range index according to the `BoundaryMode`. For empty arrays returns 0 under Symmetric logic (after re-mapping becomes < 0).

> Note : For non‑SG consumers, `BoundaryMode.Adaptive` is treated identically to `Symmetric` inside `GetValueWithBoundary`. Adaptive filter implementations typically bypass this sampler and operate on an in‑range sliding window directly.

### 3.4 Method : `CalcBinomialCoefficients`
| Constraint | Rationale |
|------------|-----------|
| 1 ≤ length ≤ 63 | Prevent overflow (`2 ^ (length - 1)` must fit signed 64-bit margin). |

Exceptions : 
- `ArgumentException` (length < 1)
- `ArgumentOutOfRangeException` (length > 63)
- `InvalidOperationException` (checked multiplication overflow)

### 3.5 Method : `ComputeGaussianCoefficients`
Parameters : 
- length ≥ 1 (typically odd to align with window = 2r + 1)
- sigma > 0

Behavior : 
- Builds symmetric weights exp(-(x ^ 2) / (2σ²)) for x ∈ [-half, half]
- Normalizes so sum ≈ 1 (double precision)

Exceptions : 
- ArgumentException (length < 1 or sigma ≤ 0)
- InvalidOperationException (numerical anomaly : non‑positive sum)

### 3.6 Method : `ComputeSGCoefficients`
```csharp
public static double[] ComputeSGCoefficients(
    int windowSize,
    int polyOrder
)
```

Constraints : 
- `windowSize` > 0 and odd.
- `0 ≤ polyOrder < windowSize`.

Process : 
1. Build Vandermonde-like design matrix A (rows = offsets in [-half, half]).
2. Form `ATA = AᵀA`.
3. Invert `ATA` (strict Gauss–Jordan with pivoting).
4. 0th derivative smoothing coefficients : first row of `(ATA) ^ {-1} Aᵀ`.
5. Normalize sum to 1; if near-zero => `InvalidOperationException`.

### 3.7 Method : `ValidateSmoothingParameters`
Rules : 
- `r >= 0`
- `windowSize = 2r + 1` must be ≤ `dataCount`.
- If `useSG`, `polyOrder < windowSize`.

Returns `true` + `error = null` if valid else `false` with explanatory multi-line message.

> **Note** :   
> This is advisory; `ApplySmoothing` does not enforce this check automatically.
> The validator now enforces a non‑negative kernel radius (`r ≥ 0`) in addition to the existing rules.

### 3.8 Method : `GetBoundaryMethodText`
public static string GetBoundaryMethodText(BoundaryMode mode)

Returns UI / export friendly labels : 
- Symmetric → "Symmetric (Mirror)"
- Replicate → "Replicate (Nearest)"
- ZeroPad → "Zero Padding"
- Default fallback → "Symmetric (Mirror)"

### 3.9 Method : `ExportCsvAsyncFromResults`
```csharp
public static Task ExportCsvAsyncFromResults(
    string filePath,
    string title,
    int kernelRadius,
    int polyOrder,
    BoundaryMode boundaryMode,
    bool doRect,
    bool doAvg,
    bool doMed,
    bool doGauss,
    bool doGaussMedian,
    bool doSG,
    double[] initialData,
    double[] rectAvg,
    double[] binomAvg,
    double[] binomMed,
    double[] gaussMedFilt,
    double[] gaussFilt,
    double[] sgFilt,
    Action<int> progress = null,
    Action<string> showMessage = null,
    int progressCheckInterval = 1,
    int? customFlushLineThreshold = null,
    double alpha = 1.0
)
```

Header block includes :
- Kernel Radius
- Kernel Width
- Boundary Method
- Alpha Blend (always included in SmoothingConductor export path)
- Polynomial Order (if SG)
- Generated
- Column headers

Purpose :  
Export precomputed smoothing arrays (avoids recomputation).  

Requirements : 
- All provided arrays (for enabled flags) must match `initialData.Length`.
- Column order : 
  1. Initial Dataset
  2. Rectangular Averaging (if enabled)
  3. Binomial Averaging (if enabled)
  4. Binomial Median Filtering (if enabled)
  5. Gaussian Weighted Median Filtering (if enabled)
  6. Gaussian Filtering (if enabled)
  7. Savitzky-Golay Filtering (if enabled)

> **Important** :   
> ExportCsvAsyncFromResults does not internally validate that supplied smoothing arrays match initialData.Length. All arrays must be length-aligned; otherwise an index exception will occur during streaming.  

> **Partitioning** :   
Observes Excel row cap (1,048,576 lines total). Deducts header block lines to compute `maxDataRows`. Splits into `PartN` files if necessary.

Progress : 
- Reports percentage based on total rows across full dataset (not per part).
- Resets to 0 at completion (intentional legacy compatibility).

Error Handling : 
- Internal try / catch inside `ExportCsvCore` swallows exceptions, calls `showMessage?.Invoke("CSV export failed : ...")`, sets progress to 0, and returns.

### 3.10 Method : `ExportCsvAsync`
Same signature conceptually but recomputes smoothing internally  : 

```csharp
public static Task ExportCsvAsync(
    string filePath,
    string title,
    int kernelRadius,
    int polyOrder,
    BoundaryMode boundaryMode,
    bool doRect,
    bool doAvg,
    bool doMed,
    bool doGaussMedian,
    bool doGauss,
    bool doSG,
    double[] initialData,
    Action<int> progress = null,
    Action<string> showMessage = null,
    int progressCheckInterval = 1,
    int? customFlushLineThreshold = null,
    double alpha = 1.0
)
```

> Use this if you do **not** already have computed arrays.

#### CSV Tuning Parameters (Performance / Progress)

| Parameter | Default | Purpose |
|-----------|---------|---------|
| `progressCheckInterval` | 1 | Report progress every N rows (use higher to reduce callback overhead). |
| `customFlushLineThreshold` | adaptive | Override internal adaptive flush batch size; higher values reduce input / output calls but raise memory spike risk in very wide column sets. |

### 3.11 Method : `ExportExcelAsync`
```csharp
public static Task ExportExcelAsync(
    string title,
    int kernelRadius,
    int polyOrder,
    BoundaryMode boundaryMode,
    bool doRect,
    bool doAvg,
    bool doMed,
    bool doGaussMedian,
    bool doGauss,
    bool doSG,
    double[] initialData,
    Action<int> progress = null,
    Action<string> showMessage = null,
    double alpha = 1.0
)
```

Worksheet header rows :
- Kernel Radius, Kernel Width
- Boundary Method
- Alpha Blend : {alpha}
- Polynomial Order : {polyOrder} (if SG)
- Subject / Comments rows (core path)

> **Progress Semantics** :   
- CsvScoreWriter.ExportAsync and core CSV helpers reset progress to 0 when finished (legacy UI convention).
- SmoothingConductor.ExportExcelAsync ends at 100% (no reset).
- ExcelScoreWriter.ExportAsync ends at 100% (no reset).

> **Note** :   
SmoothingConductor.ExportExcelAsync writes "Subject" and "Comments" rows directly into the worksheet (lines 8 – 9). ExcelScoreWriter.ExportAsync sets these only as document properties (not duplicated as sheet rows) for the dynamic / high-level path.

Behavior : 
- Recomputes smoothing arrays.
- Each "section" (filter) may overflow into multiple adjacent columns if length exceeds `maxRowsPerColumn = 1,048,573`.
- Chart : A multi-series line chart inserted after the last data block (anchor column + offset).
- Series union : Uses COM `Union` to aggregate multi-column segments per filter into a single series value reference.
- Document properties filled with thematic metadata (Title, Category, etc.).
- Success : Workbook opened in Excel (`excel.Visible = true`); RCWs released but process remains.

Exceptions : 
- Caught and reported via `showMessage`.
- COM objects released in `finally`.

> **Interop availability & UI separation** :  
> `ExcelScoreWriter` throws `ExcelInteropNotAvailableException` with `Reason` = NotInstalled / ClassNotRegistered / ActivationFailed / Unknown (NET48 and dynamic paths). The library does not display UI; UIs should switch on `ex.Reason`.

> **Note** :   
> The code references `Excel = Microsoft.Office.Interop.Excel`. Interop requires Excel installed.

### 3.12 Internal Helpers (Non-Public)
- `WeightedMedianAt` : Per-sample weighted median using binomial weights; sorts value-weight pairs.
- `InvertMatrixStrict` : Gauss–Jordan inversion with partial pivot selection & adaptive tolerance; throws if singular / ill-conditioned.
- `ExportCsvCore` : Shared CSV writing implementation (buffering, partition logic).

---

## 4. Data Containers

### 4.1 `SmoothingNotation` (Export Flags)
```csharp
public sealed class SmoothingNotation
{
    public bool Rectangular { get; set; }
    public bool BinomialAverage { get; set; }
    public bool BinomialMedian { get; set; }
    public bool GaussianMedian { get; set; }
    public bool Gaussian { get; set; }
    public bool SavitzkyGolay { get; set; }

    public bool Any =>
        Rectangular ||
        BinomialAverage ||
        BinomialMedian ||
        GaussianMedian ||
        Gaussian ||
        SavitzkyGolay;
}
```

> Used by export request DTOs to specify active smoothing columns.

### 4.2 `SmoothingScore`
```csharp
public sealed class SmoothingScore
{
    public double[] Initial { get; set; }
    public double[] Rect { get; set; }
    public double[] BinomAvg { get; set; }
    public double[] BinomMed { get; set; }
    public double[] Gauss { get; set; }
    public double[] GaussMed { get; set; }
    public double[] SavitzkyGolay { get; set; }

    public int Length => Initial?.Length ?? 0;
}
```

> Aggregates original and smoothed series (if produced). Plain DTO; no cloning or enumeration helpers are implemented in this version.  
>
> Not directly returned in current API; available for consumer layering (e.g., UI caching or batch analysis).

---

## 5. CSV Export Layer
When Flags.SavitzkyGolay == true and DerivOrder > 0 :
- The output column / series labeled "Savitzky-Golay Filtering" contains the derivative.
- A "Derivative Order : d" header line (CSV) or row (Excel) is emitted.
Adaptive mode still slides windows for derivative computation; no mirrored or zero-padded samples are used.

### CSV Header Line Ordering (High-Level vs Core)
High-level writer (`CsvScoreWriter.ExportAsync`) header order :
1. Kernel Radius  
2. Kernel Width  
3. Boundary Method  
4. Alpha Blend (only if BinomialAverage or BinomialMedian or GaussianWeightedMedian or Gaussian enabled)  
5. Polynomial Order (if SG)  
6. Derivative Order (if SG & d > 0)  
7. Generated, headers, etc.

Core conductor exports (`ExportCsvAsync`, `ExportCsvAsyncFromResults`) use :
1. Kernel Radius  
2. Kernel Width  
3. Boundary Method  
4. Alpha Blend  
5. Polynomial Order (if SG)  
6. Generated, headers, etc.

This preserves compatibility and matches implementation.


### 5.1 `CsvScoreRequest`
```csharp
public sealed class CsvScoreRequest
{
    public string Title { get; set; }

    public string BaseFilePath { get; set; }

    // Preferred absolute save path (overrides BaseFilePath when set)
    public string SavePath { get; set; }

    public double[] InitialData { get; set; }

    public int Radius { get; set; }

    public int PolyOrder { get; set; }

    public int DerivOrder { get; set; }
    // 0 = smoothing; d > 0 = derivative;
    // must satisfy 0 <= DerivOrder <= PolyOrder when Savitzky-Golay enabled

    public BoundaryMode BoundaryMode { get; set; }

    public SmoothingNotation Flags { get; set; }

    public double Alpha { get; set; }
    // Range [0, 1]. Default 1.0.

    public bool OpenAfterExport { get; set; }
}
```
- SavePath : When non-empty, writers use it; otherwise BaseFilePath is used.

### 5.2 `CsvExportResult`
```csharp
public sealed class CsvExportResult
{
    public List<string> GeneratedFiles { get; } = new List<string>();
}
```

### 5.3 `CsvScoreWriter`
```csharp
public static class CsvScoreWriter
{
    public static Task<CsvExportResult> ExportAsync(
        CsvScoreRequest request,
        IProgress<int> progress = null,
        CancellationToken cancellationToken = default
    );
}
```
> **Behavior** :
- Emits "Alpha Blend : {Alpha}" only when any of Binomial Average, Binomial Median, or Gaussian are enabled.
- Calls `SmoothingConductor.ApplySmoothing(..., alpha : request.Alpha)`.

> **Progress Semantics** :   
- CsvScoreWriter.ExportAsync and core CSV helpers reset progress to 0 when finished (legacy UI convention).  
- SmoothingConductor.ExportExcelAsync ends at 100% (no reset).  
- ExcelScoreWriter.ExportAsync ends at 100% (no reset).

> **Note** :   
> When using CsvScoreWriter.ExportAsync the "Boundary Method" header uses abbreviated labels ("Symmetric", "Replicate", "ZeroPad"). The lower-level SmoothingConductor.ExportCsvAsync / ExportCsvAsyncFromResults use extended labels ("Symmetric (Mirror)", "Replicate (Nearest)", "Zero Padding"). This difference is intentional for legacy compatibility.

Processing Steps : 
1. Validates request and `InitialData`.
2. Calls external `ScoreConformanceChecker.Validate` (not in provided snippet) – failure → `InvalidOperationException`.
3. Computes all enabled smoothing arrays via `SmoothingConductor.ApplySmoothing`.
4. Builds column set (static ordering).
5. Partitions output if necessary (Excel row constraint minus header).
6. Streams rows with UTF-8 encoding (buffered).
7. Reports progress (% of total rows) per row change; sets to 0 after completion.

Exceptions : 
- `ArgumentNullException` (request or InitialData null).
- `InvalidOperationException` (validation failure, zero-length data).
- `OperationCanceledException` (cancellation token signaled).
- Input / Output exceptions (not caught here).
- Any smoothing exceptions propagate.

---

## 6. Excel Export Layer
When Flags.SavitzkyGolay == true and DerivOrder > 0 :
- The output column / series labeled "Savitzky-Golay Filtering" contains the derivative.
- A "Derivative Order : d" header line (CSV) or row (Excel) is emitted.
Adaptive mode still slides windows for derivative computation; no mirrored or zero-padded samples are used.

### 6.1 `ExcelScoreRequest`
```csharp
public sealed class ExcelScoreRequest
{
    public string DatasetTitle { get; set; }

    public double[] InitialData { get; set; }

    public int Radius { get; set; }

    public int PolyOrder { get; set; }

    public BoundaryMode BoundaryMode { get; set; }

    public double Alpha { get; set; }
    // Blend factor for Binomial Average / Binomial Median / Gaussian.
    // Range [0, 1]. Default 1.0.

    public int DerivOrder { get; set; }
    // 0 = smoothing; d > 0 = derivative;
    // must satisfy 0 <= DerivOrder <= PolyOrder when Savitzky-Golay enabled

    public SmoothingNotation Flags { get; set; }

    // Open / Save control used by ExcelScoreWriter
    public bool OpenAfterExport { get; set; }
    // true → show Excel UI; false → save and close

    public string SavePath { get; set; }
    // required when OpenAfterExport == false

    public bool UseLocalLanguage { get; set; }
    // forwarded to Excel's SaveAs 'Local' parameter
}

### 6.2 `ExcelScoreWriter`

Two compile paths : 

| Build | Behavior |
|-------|----------|
| NET48 | Strongly-typed COM interop variant referencing legacy names (`ExcelExportRequest`, `SmoothingTuner`). |
| Other (.NET Standard 2.0 consuming environments) | Late-binding dynamic + `ExcelScoreRequest`. |

Public asynchronous method signature (non-NET48 snippet) : 

```csharp
public static Task ExportAsync(
    ExcelScoreRequest request,
    IProgress<int> progress = null
);
```

Behavior similar conceptually to `ExportExcelAsync`, but at the high-level writer layer.

>**Behavior** :
- Applies smoothing and writes series/headers; builds chart; sets document properties.
- Chart axis titles: category (X) = "Sequence Number", value (Y) = "Value".
- Boundary labels use abbreviated forms ("Symmetric", "Replicate", "ZeroPad", "Adaptive").
- Alpha header row:
  - Always present.
  - "Alpha Blend : {Alpha}" when any of Binomial Average, Binomial Median, Gaussian Weighted Median, or Gaussian are enabled.
  - "Alpha Blend : N/A" when none of the affected filters are enabled.

>**Open vs Save** :
- `OpenAfterExport == true` → makes Excel visible and leaves the workbook open (no SaveAs).
- `OpenAfterExport == false` → saves to `SavePath` (.xlsx), honors `UseLocalLanguage` for Excel’s SaveAs `Local` parameter, then closes workbook and quits Excel.

>**Interop availability & UI separation** :
- Throws `ExcelInteropNotAvailableException` with `Reason` = NotInstalled / ClassNotRegistered / ActivationFailed / Unknown (both NET48 and dynamic paths).
- When ProgID lookup fails (`Type.GetTypeFromProgID("Excel.Application") == null`), the current implementation signals `Reason = Unknown`.
- The library does not display UI; callers should switch on `ex.Reason`.

> **Note (Boundary Labels)** :  
> ExcelScoreWriter.ExportAsync uses abbreviated boundary labels ("Symmetric", "Replicate", "ZeroPad"). SmoothingConductor.ExportExcelAsync uses the extended forms ("Symmetric (Mirror)", "Replicate (Nearest)", "Zero Padding").

> **Note** :   
> SmoothingConductor.ExportExcelAsync writes "Subject" and "Comments" rows directly into the worksheet (lines 8 – 9). ExcelScoreWriter.ExportAsync sets these only as document properties (not duplicated as sheet rows) for the dynamic / high-level path.

### 6.3 `ExcelInteropNotAvailableException`

Thrown by late-binding path when : 
- Excel is not installed (ProgID lookup fails),
- COM activation fails.

---

## 7. Validation Helpers (`ScoreConformanceChecker`)

References to :  
`ScoreConformanceChecker` is a thin wrapper that converts the out‑parameter pattern of  
`SmoothingConductor.ValidateSmoothingParameters` into a tuple‑return `(Success, Error)` form :

```csharp
var (ok, error) = ScoreConformanceChecker.Validate(
    dataCount : samples.Length,
    r : radius,
    polyOrder : polyOrder,
    useSG : flags.SavitzkyGolay
);

if (!ok)
{
    Console.WriteLine(error);
    return;
}
```

Use this helper in high‑level export or UI flows for concise branching.  
Its logic directly delegates to `SmoothingConductor.ValidateSmoothingParameters` without altering rules,  
but now benefits from the updated negative‑radius guard.

**Rules** : 
- `r >= 0`
- `(2 × r + 1) <= dataCount`
- If `useSG` then `polyOrder < (2 × r + 1)`

> **Notes** :    
> The underlying validator now enforces `r ≥ 0` in addition to the existing rules.  
>
> Callers no longer need to pre‑check for negative radius values before invoking this helper.

---

## 8. Error & Exception Summary

| Component | Primary Exceptions |
|-----------|--------------------|
| `ApplySmoothing` | `ArgumentNullException`, `ArgumentOutOfRangeException`, plus indirect from coefficient builders |
| Binomial Coefficients | `ArgumentException`, `ArgumentOutOfRangeException`, `InvalidOperationException` |
| Gaussian Coefficients | `ArgumentException`, `InvalidOperationException` |
| Savitzky-Golay Coefficients | `ArgumentOutOfRangeException`, `ArgumentException`, `InvalidOperationException` |
| CSV (high-level `CsvScoreWriter`) | `ArgumentNullException`, `InvalidOperationException`, `OperationCanceledException`, Input / Output errors |
| CSV (`ExportCsvAsync*`) | Internal catch → error message via callback; exceptions not rethrown (unless pre-header logic) |
| Excel (dynamic + NET48) | `ExcelInteropNotAvailableException` (Reason classified), `InvalidOperationException` (wrapped); COM exceptions rewrapped |

| Context         | Additional Constraint              |
|-----------------|------------------------------------|
| Derivative (SG) | 0 ≤ DerivOrder ≤ PolyOrder         |

---

## 9. Performance & Parallelism

| Filter | Complexity per Position | Notes |
|--------|-------------------------|-------|
| Rectangular | O(w) | Simple sum & divide. |
| Binomial Average | O(w) | Weighted sum with precomputed coefficients. |
| Weighted Median | O(w log w) | Sort dominates; w = 2r+1. |
| Gaussian Weighted Median | O(w log w) | Robust median with Gaussian emphasis; local kernel in Adaptive |
| Gaussian | O(w) | Precomputed normalized kernel. |
| Savitzky-Golay | O(w) | After precompute of coefficients. |

Parallel threshold : `n >= 2000` triggers `Parallel.For` (default scheduler). Weighted median may reduce scaling gains due to allocation and sorting overhead; still generally benefits on multi-core workloads.

---

## 10. Thread Safety

- All static methods are stateless and reentrant.
- Output arrays are newly allocated per invocation : safe for concurrent calls.
- Do not mutate arrays returned by one thread while another thread expects original values.
- Excel COM automation is inherently single-threaded (STA expectation); calls should be serialized.

---

## 11. Usage Examples
### 11.1 Multi-Filter Smoothing
```csharp
var (rect, binom, med, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    input : samples,
    r : 4,
    polyOrder : 3,
    boundaryMode : BoundaryMode.Symmetric,
    doRect : true,
    doAvg : true,
    doMed : true,
    doGaussMed : true,
    doGauss : true,
    doSG : true
);
```

### 11.2 Validate Before Running Savitzky-Golay
```csharp
var useSG = flags?.SavitzkyGolay == true;

if (!SmoothingConductor.ValidateSmoothingParameters(
    samples.Length,
    r,
    polyOrder,
    useSG,
    out var err))
{
    Console.WriteLine(err);
    return;
}
```

### 11.3 Export CSV (Computed Internally)
```csharp
var csvRequest = new CsvScoreRequest
{
    Title = "Run A",
    BaseFilePath = @"C:\out\runA.csv",
    InitialData = samples,
    Radius = r,
    PolyOrder = polyOrder,
    BoundaryMode = BoundaryMode.Replicate,
    Flags = new SmoothingNotation
    {
        Rectangular = true,
        BinomialAverage = true,
        BinomialMedian = false,
        GaussianMedian = true, // enable Gaussian Weighted Median
        Gaussian = true,
        SavitzkyGolay = true
    },
    Alpha = 0.85,
    SigmaFactor = 12.0, 
    DerivOrder = 0
};

var result = await CsvScoreWriter.ExportAsync(
    csvRequest,
    progress : new Progress<int>(p => Console.WriteLine($"CSV {p}%"))
);
```

### 11.4 Precompute Then Export (Skip Recalculation)
```csharp
var (rArr, bArr, mArr, gaussMedArr, gaussArr, sgArr) = SmoothingConductor.ApplySmoothing(
    samples,
    r,
    polyOrder,
    BoundaryMode.Symmetric,
    doRect: true,
    doAvg: true,
    doMed: true,
    doGaussMed: true,  
    doGauss: true,     
    doSG: true,        
    alpha: 1.0,
    sigmaFactor: 12.0
);

await SmoothingConductor.ExportCsvAsyncFromResults(
    filePath: @"C:\out\precomputed.csv",
    title: "Precomputed",
    kernelRadius: r,
    polyOrder: polyOrder,
    boundaryMode: BoundaryMode.Symmetric,
    doRect: true,
    doAvg: true,
    doMed: true,
    doGauss: true,
    doGaussMedian: true,
    doSG: true,
    initialData: samples,
    rectAvg: rArr,
    binomAvg: bArr,
    binomMed: mArr,
    gaussMedFilt: gaussMedArr,  
    gaussFilt: gaussArr,        
    sgFilt: sgArr,
    progress: p => Console.WriteLine($"Export {p}%")
);
```

### 11.5 Excel Export (Dynamic COM)
```csharp
var excelRequest = new ExcelScoreRequest
{
    DatasetTitle = "Signal 42",
    InitialData = samples,
    Radius = 5,
    PolyOrder = 3,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SmoothingNotation
    {
        Rectangular = true,
        BinomialAverage = true,
        BinomialMedian = true,
        GaussianMedian = true, 
        Gaussian = true,
        SavitzkyGolay = true
    },
    Alpha = 0.85,       // optional: applies to Binomial Avg / Med, Gaussian Weighted Median, Gaussian
    SigmaFactor = 12.0, // optional: custom sigma scaling
    DerivOrder = 0      // 0 = smoothing; >0 = derivative when SG enabled
};
```

---

## 12. Behavioral Guarantees & Invariants

| Aspect | Guarantee |
|--------|-----------|
| Output lengths | All smoothing arrays match `input.Length`. |
| Tuple ordering | Always `(Rect, Binom, Median, GaussMed, Gauss, SG)`. |
| Disabled arrays | Allocated and zero-initialized (no null). |
| Gaussian & SG kernels | Normalized to sum ≈ 1 (floating precision). |
| Binomial coefficient length safety | Hard cap at 63 to avoid overflow. |
| CSV numeric format | Invariant culture for row data. |
| Partition naming | `BaseName_PartN.ext` for N ≥ 2. |
| Progress reporting | CSV export resets to 0 at completion; ExcelScoreWriter.ExportAsync ends at 100%; SmoothingConductor.ExportExcelAsync ends at 100%. |
| Alpha-applied filters | Binomial Average, Binomial Median, Gaussian Weighted Median, Gaussian |
| Alpha not applied     | Rectangular, Savitzky–Golay (smoothing and derivatives)      

---

## 13. Limitations / Edge Cases

| Category | Note |
|----------|------|
| Extremely small data vs large radius | Validation optional; algorithm still runs using boundary reflections / zeros. |
| Weighted median cost | For large radius, can dominate runtime (O(n × w log w)). |
| Savitzky-Golay conditioning | Very high order vs long window can cause inversion failure (throws). |
| Excel interop availability | Dynamic late-binding fails fast with `ExcelInteropNotAvailableException` if ProgID missing. |
| Error handling in CSV core | Swallows internal exceptions, reports via `showMessage`; silent to caller unless callback used. |
| Legacy names (`SmoothingTuner`, `ExcelExportRequest`) | Present only in NET48 conditional branches (not part of core API described here). |
| Thread abort / forced cancellations | Only cooperative cancellation via `CancellationToken` (CSV writer) is supported. |
| Zero-length input | CSV writer blocks (throws `InvalidOperationException`); smoothing returns immediately (empty arrays). |

---

## 14. Version & Legacy Name Notes

- Current canonical smoothing class name : `SmoothingConductor`.
- Any references to `SmoothingTuner` in conditional code regions are retained for backward compatibility with older internal builds; not considered part of the published surface.
- Ensure NuGet packaging excludes unintended legacy forms if not required.

---

## Appendix : Quick Exception Matrix (Condensed)

| Method | Key Exceptions |
|--------|----------------|
| ApplySmoothing | ArgNull (input), ArgRange (r), plus coefficient builder exceptions |
| CalcBinomialCoefficients | Arg / ArgRange / InvalidOperation (overflow) |
| ComputeGaussianCoefficients | ArgumentException, InvalidOperationException |
| ComputeSGCoefficients | ArgOutOfRange, ArgumentException, InvalidOperation |
| ValidateSmoothingParameters | None (returns bool) |
| ExportCsvAsync* | Internal errors → `showMessage`; pre-header misconfig can throw; ArgumentOutOfRangeException (invalid alpha) |
| CsvScoreWriter.ExportAsync | ArgNull, InvalidOperation, OperationCanceledException |
| ExcelScoreWriter.ExportAsync (dynamic) | ExcelInteropNotAvailableException, InvalidOperationException |
| ExportExcelAsync (SmoothingConductor) | Errors reported via callback, COM released |

- `ApplySmoothing`: `ArgumentOutOfRangeException` on `alpha` out of range.

---

## Addendum : Savitzky-Golay Derivative Order

This addendum documents derivative capabilities for Savitzky-Golay. It complements the existing sections without changing prior behavior.

### Core APIs

- 3.6a Method : ApplySGDerivative

```csharp
public static double[] ApplySGDerivative(
    double[] input,
    int r,
    int polyOrder,
    int derivativeOrder,
    double delta,
    BoundaryMode boundaryMode
)
```

Example :
```csharp
var d1 = SmoothingConductor.ApplySGDerivative(
    input : samples,
    r: 4,
    polyOrder: 3,
    derivativeOrder: 1,
    delta: 1.0,
    boundaryMode : BoundaryMode.Symmetric
);
```

- 3.6b Method : ComputeSGCoefficients (derivative overload)
```csharp
public static double[] ComputeSGCoefficients(
    int windowSize,
    int polyOrder,
    int derivativeOrder,
    double delta
)
```

Same constraints / behavior as above. Use the 2-argument wrapper for smoothing (0th derivative).

### Data Containers (Requests)

- 5.1 CsvScoreRequest : property
```csharp
public int DerivOrder { get; set; } // default 0 = smoothing
```

> **Notes** :   
> Effective only when Flags.SavitzkyGolay == true.    
>
> When DerivOrder > 0, the "Savitzky-Golay Filtering" CSV column contains the derivative series and the CSV header includes "Derivative Order : d".

- 6.1 ExcelScoreRequest : property
```csharp
public int DerivOrder { get; set; } // default 0 = smoothing
```
> **Notes** :   
> Effective only when Flags.SavitzkyGolay == true.  
>
> When DerivOrder > 0, the Excel header block shows "Derivative Order : d", and the "Savitzky-Golay Filtering" plotted series is the derivative.

### CSV / Excel Export Behavior

- CsvScoreWriter.ExportAsync :
  - Validates DerivOrder (≥ 0 and ≤ PolyOrder) when Savitzky-Golay is enabled.
  - If DerivOrder == 0 → SG column = smoothing.
  - If DerivOrder > 0 → SG column = d‑th derivative (Δ = 1.0); header adds "Derivative Order : d".

- ExcelScoreWriter.ExportAsync :
  - Same validation and column semantics as CSV.
  - Header block includes "Derivative Order : d" when d > 0.
  - Chart series labeled "Savitzky-Golay Filtering" corresponds to smoothing (d = 0) or derivative (d > 0) accordingly.

> **Notes** :   
> Sample spacing :   
> Export paths assume Δ = 1.0. For Δ ≠ 1.0, compute derivatives via SmoothingConductor.ApplySGDerivative and export using SmoothingConductor.ExportCsvAsyncFromResults.  
>
> Validation :   
ScoreConformanceChecker.Validate does not include DerivOrder; derivative constraints are enforced by export writers and core SG builders.

### Example : First Derivative Export
```csharp
var req = new CsvScoreRequest
{
    Title = "Spectra d = 1",
    BaseFilePath = @"C:\out\spectra_d1.csv",
    InitialData = samples,
    Radius = 5,
    PolyOrder = 3,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SmoothingNotation
    {
        SavitzkyGolay = true
    },
    DerivOrder = 1
};

await CsvScoreWriter.ExportAsync(req);
```

---

## Summary

This API centers on a single, cache-efficient smoothing pass that can yield six classical smoothing outputs, with optional alpha blending applied to Binomial Average, Binomial Median, Gaussian Weighted Median, and Gaussian, coupled with robust, partition-aware export facilities (CSV / Excel).

---

Happy smoothing.