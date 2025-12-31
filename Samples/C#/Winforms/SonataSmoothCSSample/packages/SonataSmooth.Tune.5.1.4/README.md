# SonataSmooth.Tune

High‑performance 1D numeric signal smoothing & export toolkit for .NET (C#).  
Implements Rectangular (Moving Average), Binomial (Pascal) Average, Weighted Median (Binomial Weights), Gaussian Weighted Median (GWMF), Gaussian, and Savitzky-Golay smoothing with configurable boundary handling and optional parallelization. Includes CSV and Excel (COM) export helpers that materialize multiple smoothing "voices" side‑by‑side for inspection or charting.

> Target : .NET Standard 2.0 (core algorithms & CSV export)  
> Optional Excel COM automation sections use conditional compilation (`#if NET48`) or late binding (dynamic COM) when available.

---

## Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Concepts & Design Notes](#concepts--design-notes)
- [Boundary Handling](#boundary-handling)
- [Implemented Filters](#implemented-filters-brief)
- [Performance Characteristics](#performance-characteristics)
- [CSV Header Ordering Note](#csv-header-ordering-note)
- [Excel interop availability signaling & UI separation](#excel-interop-availability-signaling--ui-separation)
- [API Reference](#api-reference)
  - Enum : `BoundaryMode`
  - Core static class : `SmoothingConductor`
    - `ApplySmoothing`
    - `GetValueWithBoundary`
    - `CalcBinomialCoefficients`
    - `ComputeGaussianCoefficients`
    - `ComputeSGCoefficients`
    - `ValidateSmoothingParameters`
    - Validation helper : `ScoreConformanceChecker`
    - CSV / Excel export helpers (`ExportCsvAsync`, `ExportCsvAsyncFromResults`, `ExportExcelAsync`)
  - Additional CSV tuning parameters :
    - progressCheckInterval(int, default 1) : Throttles how often progress is recomputed / reported (every N rows).
    - customFlushLineThreshold(int?) : Overrides the adaptive buffered write flush batch size; null keeps adaptive logic.
  - Data containers : `SmoothingScore`, `SmoothingNotation`
  - CSV export types : `CsvScoreRequest`, `CsvExportResult`, `CsvScoreWriter`
  - Excel export types : `ExcelScoreRequest`, `ExcelScoreWriter`, `ExcelInteropNotAvailableException`
- [Error & Exception Reference](#error--exception-reference-selected)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)
- [Thread Safety](#thread-safety)
- [Limitations & Known Inconsistencies](#limitations--known-inconsistencies)
- [License (MIT)](#license)
- [Changelog](#changelog)
- [Disclaimer](#disclaimer)

---

## Features

- Compute up to six smoothing variants in a single pass over the source data.
- Window-based convolution / order polynomial fitting (Savitzky-Golay) with **user-specified radius**.
- Weighted median using binomial coefficients for robust noise suppression.
- Optional alpha blending for Binomial Average, Binomial Median, Gaussian Weighted Median, and Gaussian outputs :
  result = alpha × filtered + (1 − alpha) × original.
- The SigmaFactor parameter is honored for both standard and adaptive (BoundaryMode.Adaptive) Gaussian and Gaussian Weighted Median filters.
- Automatic parallel execution threshold (`n ≥ 2000`) for CPU-bound steps.
- Culture-invariant numeric formatting for export.
- Large dataset CSV partitioning honoring Excel row limit (1,048,576).
- Efficient buffered CSV writer with adaptive flush strategy.
- Excel export :
  - Full multi-series line chart generation.
  - Multi-column continuation for > 1,048,573 rows (per-column fill strategy).
  - Chart axis titles: X = "Sequence Number", Y = "Value".
  - Document properties + playful meta "phrases" (retained).
  - Late binding fallback when strong interop PIAs not available (non-`NET48`).
  - Boundary labels in Excel : high-level `ExcelScoreWriter` uses abbreviated labels ("Symmetric", "Replicate", "ZeroPad", "Adaptive"); the core SmoothingConductor Excel export uses extended forms ("Symmetric (Mirror)", etc.)
  - Worksheet header note : `SmoothingConductor.ExportExcelAsync` writes "Subject" and "Comments" rows into the worksheet; `ExcelScoreWriter.ExportAsync` stores these only as document properties (not repeated as sheet rows).
- Configurable boundary modes : Symmetric (mirror), Replicate (clamp), ZeroPad (explicit zeros), Adaptive (full in‑range sliding window; asymmetric near edges; no synthetic samples).

---

## Installation

NuGet (preferred) : dotnet add package SonataSmooth.Tune  
Package Manager : Install-Package SonataSmooth.Tune

---

## Quick Start
```csharp
using SonataSmooth.Tune;
using SonataSmooth.Tune.Export;

double[] data = { 1, 2, 3, 10, 9, 5, 4, 3, 2 };

int radius = 2;      // Smoothing window size: (2 × radius) + 1 = 5
int polyOrder = 2;   // Degree of the polynomial used in Savitzky-Golay smoothing

var (rect, binomAvg, binomMed, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    data,
    r : radius,
    polyOrder : polyOrder,
    boundaryMode : BoundaryMode.Symmetric,
    doRect : true,
    doAvg : true,
    doMed : true,
    doGaussMed : true,
    doGauss : true,
    doSG : true,
    alpha : 0.85,       // optional : blend only for Binomial Average, Binomial Median, Gaussian Weighted Median, Gaussian
    sigmaFactor = 12.0  // Custom : sigma = windowSize / 12.0
);
```

```
// (Optional) Validate parameters first using the tuple helper
var (ok, validationError) = ScoreConformanceChecker.Validate(
    dataCount : data.Length,
    r : radius,
    polyOrder : polyOrder,
    useSG : true // set to false if you disable SG
);

if (!ok)
{
    Console.WriteLine(validationError);
    return; // Abort before running smoothing / export
}
```

CSV export (computed internally) :
```csharp
using SonataSmooth.Tune.Export;

var req = new CsvScoreRequest
{
    Title = "Demo Dataset",
    BaseFilePath = @"C:\Temp\demo.csv",
    InitialData = data,
    Radius = radius,
    PolyOrder = polyOrder,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SmoothingNotation
    {
        Rectangular = true,
        BinomialAverage = true,
        BinomialMedian = true,
        Gaussian = true,
        SavitzkyGolay = true
    },
    Alpha = 0.85,       // Writer includes "Alpha Blend : 0.85" when relevant
    SigmaFactor = 12.0  // Custom : sigma = windowSize / 12.0
};

var result = await CsvScoreWriter.ExportAsync(req);
```

> **Note** : 
> When using CsvScoreWriter.ExportAsync the "Boundary Method" header uses abbreviated labels ("Symmetric", "Replicate", "ZeroPad"). The lower-level SmoothingConductor.ExportCsvAsync / ExportCsvAsyncFromResults use extended labels ("Symmetric (Mirror)", "Replicate (Nearest)", "Zero Padding").

---

## CSV Header Ordering Note

High-level CSV export (`CsvScoreWriter.ExportAsync`) writes header metadata in this order :
1. Kernel Radius  
2. Kernel Width  
3. Boundary Method  
4. Alpha Blend (only when any of Binomial Average, Binomial Median, Gaussian Weighted Median, or Gaussian are enabled)
5. Gaussian Sigma : {kernelWidth} ÷ {sigmaFactor} = {sigma}" is included in the header when Gaussian or Gaussian Weighted Median is enabled. The value reflects the actual sigma used for smoothing, matching the SigmaFactor parameter in the request.
6. Polynomial Order (only when Savitzky-Golay enabled)  
7. Derivative Order (only when Savitzky-Golay enabled and `DerivOrder > 0`)  

Lower-level conductor exports (`SmoothingConductor.ExportCsvAsync` / `ExportCsvAsyncFromResults`) use :
1. Kernel Radius  
2. Kernel Width  
3. Boundary Method  
4. Alpha Blend  
5. Gaussian Sigma : {kernelWidth} ÷ {sigmaFactor} = {sigma}" is included in the header when Gaussian or Gaussian Weighted Median is enabled. The value reflects the actual sigma used for smoothing, matching the SigmaFactor parameter in the request. 
6. Polynomial Order (if SG)

Core conductor exports always include the line "Alpha Blend : {alpha}" even if Binomial Average, Binomial Median, or Gaussian are disabled (Rectangular and Savitzky-Golay ignore alpha).

This divergence is intentional for legacy compatibility. If you parse headers automatically, normalize by key name rather than relying on positional order.

---

## Excel interop availability signaling & UI separation

- The library throws `ExcelInteropNotAvailableException` when Excel COM automation is unavailable.
- The exception includes `Reason` (`ExcelInteropUnavailableReason`) to classify the cause:
  - `NotInstalled` — Excel not installed or COM ProgID not present.
  - `ClassNotRegistered` — COM class missing (e.g., `REGDB_E_CLASSNOTREG`).
  - `ActivationFailed` — other COM activation failures (bitness mismatch, startup failure, etc.).
  - `Unknown` — undetermined cause.

Behavior by build:
- .NET Framework 4.8 path (strong PIAs) and .NET Standard late-binding path both set `Reason`:
  - NET48: COM exceptions are classified and wrapped with `Reason`.
  - .NET Standard: the `catch (COMException ex)` path classifies and sets `Reason`; ProgID lookup failure throws `ExcelInteropNotAvailableException` and currently sets `Reason = Unknown`.
- Library layer shows no UI (no `MessageBox` anywhere). Forms (WinForms/WPF) handle UX.

Suggested WinForms handling :
```csharp
try
{
    await ExcelScoreWriter.ExportAsync(
        req,
        new Progress<int>(p => progressBar.Value = p)
    );
}
catch (ExcelInteropNotAvailableException ex)
{
    switch (ex.Reason)
    {
        case ExcelInteropUnavailableReason.NotInstalled:
        case ExcelInteropUnavailableReason.ClassNotRegistered:
            var r = MessageBox.Show(
                this,
                "Microsoft Excel is not installed or its COM registration is missing.\n\n" +
                "Open the Microsoft Office download page?",
                "Excel Not Available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (r == DialogResult.Yes)
            {
                OpenOfficeDownloadPage();
            }
            break;

        case ExcelInteropUnavailableReason.ActivationFailed:
            MessageBox.Show(
                this,
                "Excel could not be activated via COM.\n" +
                "Please check the Office installation, add-ins, and bitness compatibility.",
                "Export Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            break;

        case ExcelInteropUnavailableReason.Unknown:
        default:
            MessageBox.Show(
                this,
                "Excel export failed due to an interop error.\n\n" + ex.Message,
                "Export Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            break;
    }
}
```

---

## Savitzky-Golay Derivative Order (d)

This library supports Savitzky-Golay derivative filters in addition to 0th‑order smoothing.

- Core APIs :
  - SmoothingConductor.ComputeSGCoefficients(windowSize, polyOrder, derivativeOrder, delta)
  - SmoothingConductor.ApplySGDerivative(input, r, polyOrder, derivativeOrder, delta, boundaryMode)
- Request DTOs :
  - CsvScoreRequest.DerivOrder (int, default 0)
  - ExcelScoreRequest.DerivOrder (int, default 0)

Behavior :
- d = 0 yields classic smoothing (coefficients normalized to sum = 1).
- d ≥ 1 yields the d‑th derivative estimate at the window center; output is scaled by d! / Δ^d.
- Exports :
  - When Flags.SavitzkyGolay is true and DerivOrder > 0, the "Savitzky-Golay Filtering" column contains the derivative series (not the smoothed series).
  - CSV / Excel headers include "Derivative Order : d". CSV / Excel writers assume Δ = 1.0 for exported derivatives.

Constraints :
- Radius r ≥ 0; windowSize = 2r + 1 must fit in the dataset length.
- 0 ≤ derivativeOrder ≤ polyOrder
- 0 ≤ polyOrder < windowSize
- delta > 0 (only in low‑level APIs; export writers use 1.0)

> **Notes** :   
> ScoreConformanceChecker.Validate does not take DerivOrder. Derivative constraints are validated by CsvScoreWriter / ExcelScoreWriter and by SmoothingConductor at execution time.  
>
> For non‑unit sample spacing (Δ ≠ 1), call SmoothingConductor.ApplySGDerivative directly with your Δ and then export via SmoothingConductor.ExportCsvAsyncFromResults.

### Example : First derivative (d = 1)
```csharp
var d1 = SmoothingConductor.ApplySGDerivative(
    input : samples,
    r: 4,
    polyOrder: 3,
    derivativeOrder: 1,
    delta: 1.0,
    boundaryMode : BoundaryMode.Symmetric
);

// CSV export with derivative (Δ = 1.0)
var csvReq = new SonataSmooth.Tune.Export.CsvScoreRequest
{
    Title = "Run A (d = 1)",
    BaseFilePath = @"C:\out\runA_deriv.csv",
    InitialData = samples,
    Radius = 4,
    PolyOrder = 3,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SonataSmooth.Tune.Export.SmoothingNotation
    {
        SavitzkyGolay = true
    },
    DerivOrder = 1,
    Alpha = 1.0 // derivative path unaffected; alpha applies to Gaussian / Binomial Average / Binomial Median only
};

var csvResult = await SonataSmooth.Tune.Export.CsvScoreWriter.ExportAsync(csvReq);

// Excel export with derivative (Δ = 1.0)
var xlsReq = new SonataSmooth.Tune.Export.ExcelScoreRequest
{
    DatasetTitle = "Signal 42 (d = 1)",
    InitialData = samples,
    Radius = 4,
    PolyOrder = 3,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SonataSmooth.Tune.Export.SmoothingNotation
    {
        SavitzkyGolay = true
    },
    DerivOrder = 1,
    Alpha = 1.0
};

await SonataSmooth.Tune.Export.ExcelScoreWriter.ExportAsync(xlsReq);
```

> **Note (Adaptive + Derivatives)** :  
> Adaptive boundary handling applies equally to derivative kernels (via `ApplySGDerivative`) : the per-position asymmetric fit is still used; no special derivative-only padding is introduced.

---

## Alpha Blend

`alpha` blends the filtered output with the original input for selected filters :
- Applies to : Binomial Average, Weighted Median (binomial), Gaussian Weighted Median, and Gaussian.
- Not applied to : Rectangular (uniform) and Savitzky-Golay outputs (kept as canonical filters to preserve kernel / DC and polynomial reproduction properties).
- Formula: `result = alpha * filtered + (1 - alpha) * original`, with `alpha ∈ [0, 1]`.
- Adaptive mode behavior : alpha is applied after the local adaptive computation for the eligible filters.
- CSV / Excel headers:
  - CsvScoreWriter: emits "Alpha Blend : {alpha}" only when any of Binomial Average, Binomial Median, Gaussian Weighted Median, or Gaussian are enabled.
  - ExcelScoreWriter: always writes an "Alpha Blend" row; when none of the affected filters are enabled, the value is "Alpha Blend : N/A".
  - SmoothingConductor.ExportCsvAsync*, ExportCsvAsyncFromResults, ExportExcelAsync: always include "Alpha Blend : {alpha}".

Guidance:
- Use `alpha < 1.0` to soften aggressive filtering near sharp features.
- Keep `alpha = 1.0` for full-strength filtering.

---

## Concepts & Design Notes

| Concept | Note |
|--------|------|
| Radius | Half-width of kernel. Window size = `2 × r + 1` (must be odd, > 0). |
| One-pass allocation | Arrays allocated once; each enabled filter computed per index. |
| Parallelization | `Parallel.For` engaged when `n >= 2000`. |
| Weighted Median | Re-computes local window list + sort per index (O(w log w)); heavier than simple linear filters. |
| Gaussian σ | Chosen as `(2r + 1) / 6` for approximate coverage of ±3σ across the window. |
| Savitzky-Golay | Normalized to unit DC gain (coefficients sum = 1). |
| Binomial limit | Length ≤ 63 to prevent overflow (64-bit safety). |

---

## Boundary Handling

`BoundaryMode` :

- `Symmetric` : Mirror index (reflective). Example : index -1 → element 0 mirrored (implemented as `-idx - 1` mapping).
- `Replicate` : Clamp to nearest endpoint (edge value continuation).
- `ZeroPad` : Outside indices treated as 0.
- `Adaptive` : Slides the full window fully inside the valid range; uses only real samples (no synthetic padding) and builds asymmetric SG coefficients.

> **Historical Note** :   
Earlier UI tooltips referenced "Adaptive (Local Poly + Median)" implying SG-only behavior. Current implementation applies Adaptive sliding uniformly to all enabled filters.

### Adaptive Mode

Adaptive keeps the nominal window length (2 × r + 1) but slides the window fully inside the valid data range for every index:

- No mirrored, clamped, or zero-padded synthetic samples are introduced.
- For Rectangular, Binomial Average, Weighted Median, and Gaussian filters : an in‑range (possibly asymmetric) block is used directly.
- For Savitzky-Golay : per-position asymmetric coefficients are computed (cached) for (left, right) around the logical center.
- Benefits : avoids artificial edge energy inflation / attenuation.
- Trade‑off : near edges the "center" of the polynomial fit is no longer geometrically centered relative to the original index (phase / alignment shift).
- Implementation detail : loops bypass `GetValueWithBoundary` when `Adaptive` to gather a shifted real-sample window.

Use Symmetric when strict geometric centering at edges is more important than removing padding artifacts.

> **Note** :    
Adaptive boundary handling applies equally to derivative kernels; per-position asymmetric fits are used for all supported filters.

---

## Implemented Filters (Brief)

| Filter | Characteristic | Complexity per sample |
|--------|----------------|-----------------------|
| Rectangular Average | Uniform weights | O(w) |
| Binomial Average | Pascal row weights, smoother than uniform | O(w) |
| Binomial Median | Robust to spikes, preserves edges | O(w log w) |
| Gaussian Weighted Median | Robust median with Gaussian emphasis (local kernel in Adaptive) | O(w log w) |
| Gaussian | Pre-normalized kernel (convolution/mean) | O(w) |
| Savitzky-Golay | Local polynomial least squares (smoothing 0th, derivatives d ≥ 1) | O(w) after precompute |

`w = 2r + 1` where `r` is the kernel radius.

---

## Performance Characteristics

- Time : Dominated by selected filters; enabling all includes one sorting per sample (median).
- Memory : 7 arrays × `n` doubles (56n bytes) if all enabled (includes input + 6 outputs).
- Parallel : Gains most when median disabled or window moderate (sorting cost can reduce scaling).
- CSV Export : Streaming + buffered StringBuilder; partitioning to prevent Excel row overflow.

---

## API Reference

### Enum : `BoundaryMode`
```csharp
public enum BoundaryMode
{
    Symmetric,  // Mirror : Reflect indices across boundaries (e.g., [3, 2, 1, 0, 1, 2, 3])
    Replicate,  // Clamp : Extend edge values beyond boundaries
    ZeroPad,    // Pad with zeros : Use zero-padding for out-of-bound indices
    Adaptive    // Slide window fully inside data (no synthetic samples); all filters use real samples; SG builds asymmetric coefficients
}
```

### Class : `SmoothingConductor` (static)

#### ApplySmoothing
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
    double? sigmaFactor = null // Optional : controls Gaussian sigma denominator
)
```

Parameters :
- `input` (required) : Source sequence.
- `r` : Kernel radius ≥ 0.
- `polyOrder` : Used only if `doSG == true`. Must be `< windowSize`.
- `boundaryMode` : See `BoundaryMode`.
- `doRect` ... `doSG` : Flags enabling each smoothing.
- `alpha` : Blend factor applied ONLY to Binomial Average, Binomial Median, Gaussian Weighted Median, and Gaussian. Ignored for Rectangular and SG.
- `sigmaFactor` (optional) : Controls the denominator in the Gaussian sigma calculation: `sigma = windowSize / sigmaFactor`. If not set, legacy value `6.0` is used. If set, allows custom control of Gaussian smoothing width.  
  **Note** : The `SigmaFactor` parameter is honored for both standard and adaptive (BoundaryMode.Adaptive) Gaussian and Gaussian Weighted Median filters.

Quick Start Example :
```csharp
var (rect, binomAvg, binomMed, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    data,
    r: radius,
    polyOrder: polyOrder,
    boundaryMode: BoundaryMode.Symmetric,
    doRect: true,
    doAvg: true,
    doMed: true,
    doGaussMed: true,
    doGauss: true,
    doSG: true,
    alpha: 0.85,
    sigmaFactor: 12.0 // Custom : sigma = windowSize / 12.0
);
```

Returns:
- Tuple order : (Rectangular, BinomialAverage, BinomialWeightedMedian, GaussianWeightedMedian, GaussianMean, SavitzkyGolay).

Exceptions :
- `ArgumentNullException` (`input`)
- `ArgumentOutOfRangeException` (`r < 0` or `alpha ∉ [0, 1]`)
- From subroutines (e.g., Savitzky-Golay coefficient generation constraints).

#### GetValueWithBoundary
```csharp
public static double GetValueWithBoundary(
    double[] data,
    int idx,
    BoundaryMode mode
)
```

Resolves and returns the value at a logical index using the selected boundary handling policy (mirror, clamp, or zero pad). No smoothing is performed here; this is an index resolution helper used by the filters.

#### ComputeGaussianCoefficients
```csharp
public static double[] ComputeGaussianCoefficients(
    int length,
    double sigma
)
```
Constraints :
- `length` ≥ 1 (typically odd to align with 2r+1 windows)  
- `sigma` > 0  
Returns a symmetric Gaussian weight array normalized so the sum ≈ 1.

Exceptions :
- ArgumentException (length < 1 or sigma ≤ 0)
- InvalidOperationException (numerical anomaly resulting in non-positive sum)


#### ComputeSGCoefficients
```csharp
public static double[] ComputeSGCoefficients(
    int windowSize,
    int polyOrder
)
```

Constraints :
- `windowSize` > 0 and odd.
- `polyOrder >= 0 && polyOrder < windowSize`.
Produces normalized smoothing (0th derivative) coefficients.

#### ValidateSmoothingParameters
```csharp
public static bool ValidateSmoothingParameters(
    int dataCount,
    int r,
    int polyOrder,
    bool useSG,
    out string error
)
```

Checks :
- `(2r + 1) ≤ dataCount`
- If `useSG`, `polyOrder < (2r + 1)`
Returns `false` with descriptive `error` block if invalid.

#### Validation Helper : `ScoreConformanceChecker`

A tuple-return wrapper around `SmoothingConductor.ValidateSmoothingParameters` :

```csharp
var (success, error) = ScoreConformanceChecker.Validate(
    dataCount : input.Length,
    r : r,
    polyOrder : polyOrder,
    useSG : doSG
);

if (!success)
{
    // Abort early - error contains multi-line diagnostic
    Console.WriteLine(error);
    return;
}
```

**Rules enforced (delegated)** :  
- `r >= 0`
- `(2 × r + 1) <= dataCount`
- If `useSG`: `polyOrder < (2 × r + 1)`

Use this for cleaner guard clauses (no `out string`).

> **Note** :   
> The wrapper now enforces a non‑negative kernel radius (`r >= 0`) in addition to the existing rules.  
  
> **Caution** :   
> Callers no longer need to pre‑check `r >= 0` before invoking this validator, as the check is performed internally.


#### ExportCsvAsync / ExportCsvAsyncFromResults

High-level CSV generation with partitioning (honors Excel max row limit).  
Two overload paths :

1. `ExportCsvAsync` : Recomputes smoothing internally.
2. `ExportCsvAsyncFromResults` : Accepts already-computed arrays to avoid recomputation cost.

Notable optional parameters :
- `progress` (`Action<int>`) : Reports rough percent (resets to 0 at completion).
- `progressCheckInterval` : Throttle frequency (default 1 = every line).
- `customFlushLineThreshold` : Controls buffered flush size (auto if null).

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
);


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
);

```
Header block includes :
- Kernel Radius, Kernel Width
- Boundary Method
- Alpha Blend (always included by SmoothingConductor export paths)
- Polynomial Order (if SG enabled)
- Generated timestamp
- Column headers

#### ExportExcelAsync
Creates a live Excel instance (requires Microsoft Excel installed & COM available).  
Populates per-filter columns (with multi-column continuation for very large arrays) and builds a composite line chart.  
Releases COM RCWs after making Excel visible (workbook remains open to user).

Exceptions surfaced via `showMessage` callback; some internal catch blocks suppress to keep UI responsive.

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

Worksheet header includes :
- Kernel Radius, Kernel Width
- Boundary Method
- Alpha Blend
- Polynomial Order (if SG enabled)
- Subject, Comments

### Data Container : `SmoothingNotation`
```csharp
public sealed class SmoothingNotation
{
    public bool Rectangular { get; set; }
    public bool BinomialAverage { get; set; }
    public bool BinomialMedian { get; set; }
    public bool Gaussian { get; set; }
    public bool GaussianMedian { get; set; }
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

Utility flag bundle passed to export requests.

### Data Container : `SmoothingScore`
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

(Not currently returned by core API; available for potential aggregation wrappers.)

### CSV Export Types

#### CsvScoreRequest
```csharp
public sealed class CsvScoreRequest
{
    public string Title { get; set; }

    public string BaseFilePath { get; set; }

    // Preferred absolute save path (when non-empty overrides BaseFilePath)
    public string SavePath { get; set; }

    public double[] InitialData { get; set; }

    public int Radius { get; set; }

    public int PolyOrder { get; set; }

    // 0 = smoothing; d > 0 = derivative
    public int DerivOrder { get; set; } 

    public BoundaryMode BoundaryMode { get; set; }

    public SmoothingNotation Flags { get; set; }

    // Blend for Binomial Average / Binomial Median, Gaussian Weighted Median, Gaussian
    public double Alpha { get; set; } 

    // Optional post-processing flag for UI actions (e.g., auto-open file)
    public bool OpenAfterExport { get; set; }
}
```
Notes :
- When SavePath is non-empty, writers prefer it over BaseFilePath for the primary CSV output. When empty, BaseFilePath is used.

#### CsvExportResult
```csharp
public sealed class CsvExportResult
{
    public List<string> GeneratedFiles { get; } = new List<string>();
}
```

#### CsvScoreWriter
```csharp
public static class CsvScoreWriter
{
    public static Task<CsvExportResult> ExportAsync(
        CsvScoreRequest request,
        IProgress<int> progress = null,
        CancellationToken cancellationToken = default
    )
}
```

Behavior :
- Validates parameters via a checker (`ScoreConformanceChecker.Validate` in consuming project).
- Splits output into parts if row limit exceeded.
- Column order : Initial Dataset + (enabled filters in consistent descriptive naming).
- Progress : 0 - 100 based on rows processed. Resets to 0 at completion (intentionally mirrors legacy semantics).
- Cancellation : Cooperative via `CancellationToken.ThrowIfCancellationRequested()` inside loop.

### Excel Export Types

#### ExcelScoreRequest
```csharp
public sealed class ExcelScoreRequest
{
    public string DatasetTitle { get; set; }

    public double[] InitialData { get; set; }

    public int Radius { get; set; }

    public int PolyOrder { get; set; }

    public BoundaryMode BoundaryMode { get; set; }

    public SmoothingNotation Flags { get; set; }

    // Blend for Binomial Average / Binomial Median / Gaussian Weighted Median / Gaussian
    public double Alpha { get; set; }

    // 0 = smoothing; d > 0 = derivative (if Savitzky-Golay enabled)
    public int DerivOrder { get; set; }

    // Open / Save behavior (used by ExcelScoreWriter)
    // true → open UI and leave workbook open; false → save and close
    public bool OpenAfterExport { get; set; }

    // required when OpenAfterExport == false; .xlsx path
    public string SavePath { get; set; }

    // passed to Excel SaveAs 'Local' parameter
    public bool UseLocalLanguage { get; set; }
}
```
Open vs Save :
- When `OpenAfterExport == true`, the writer makes Excel visible and does not save / close automatically.
- When `OpenAfterExport == false`, the writer saves to `SavePath` (creating the directory if needed) using the `.xlsx` format; COM instance is closed; `UseLocalLanguage` is forwarded to Excel's SaveAs `Local` parameter.

#### ExcelScoreWriter
```csharp
public static class ExcelScoreWriter
{
    public static Task ExportAsync(
        ExcelScoreRequest request,
        IProgress<int> progress = null
    );
}
```

> **Notes** :   
  > Two conditional implementations :  
  > `#if NET48` strong-typed reference to Excel PIA (method signature uses `ExcelExportRequest` in code region - naming mismatch with `ExcelScoreRequest`; see "Limitations & Known Inconsistencies").      
  > Else :  
  > dynamic late binding (`Type.GetTypeFromProgID("Excel.Application")`), throws `ExcelInteropNotAvailableException` if unavailable.

#### ExcelInteropNotAvailableException

Thrown when COM activation cannot proceed (Excel missing / ProgID failure).

---

## Error & Exception Reference (Selected)

| Source | Condition | Exception |
|--------|-----------|-----------|
| `ApplySmoothing` | `input == null` | ArgumentNullException |
| | `r < 0` | ArgumentOutOfRangeException |
| `CalcBinomialCoefficients` | length < 1 | ArgumentException |
| | length > 63 | ArgumentOutOfRangeException |
| `ComputeSGCoefficients` | even window or invalid poly order | ArgumentException |
| Matrix inversion | singular / ill-conditioned | InvalidOperationException |
| CSV export | `InitialData` null | ArgumentNullException |
| CSV export | data length 0 | InvalidOperationException |
| CSV export | cancellation requested (token signaled) | OperationCanceledException |
| Excel interop (NET48 + dynamic) | COM / activation unavailable | ExcelInteropNotAvailableException (Reason = NotInstalled / ClassNotRegistered / ActivationFailed / Unknown) |

---

## Usage Examples

### 1. Savitzky-Golay Only
```csharp
var (_, _, _, _, _, sg) = SmoothingConductor.ApplySmoothing(
    input : signal,
    r : 4,
    polyOrder : 3,
    boundaryMode : BoundaryMode.Replicate,
    doRect : false,
    doAvg : false,
    doMed : false,
    doGaussMed : false,
    doGauss : false,
    doSG : true,
    alpha : 1.0
);
```

### 2. Precompute & Export CSV Without Recalculation
```csharp
int r = 3;
int poly = 2;

// Execute a range of smoothing algorithms on the input data
var (rect, avg, med, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    data,
    r,
    poly,
    BoundaryMode.Symmetric,
    doRect : true,
    doAvg : true,
    doMed : true,
    doGaussMed : true,
    doGauss : true,
    doSG : true,
    alpha : 0.9,
    sigmaFactor = 12.0 // Custom : sigma = windowSize / 12.0
);

// Write smoothing results to a CSV file
await SmoothingConductor.ExportCsvAsyncFromResults(
    filePath : @"C:\out\multi.csv",
    title : "Precomputed Export",
    kernelRadius : r,
    polyOrder : poly,
    boundaryMode : BoundaryMode.Symmetric,
    doRect : true,
    doAvg : true,
    doMed : true,
    doGauss : true,
    doGaussMedian : true,
    doSG : true,
    initialData : data,
    rectAvg : rect,
    binomAvg : avg,
    binomMed : med,
    gaussMedFilt : gaussMed,
    gaussFilt : gauss,
    sgFilt : sg,
    progress : p => Console.WriteLine($"CSV {p}%"),
    customFlushLineThreshold : null,
    alpha : 0.9,
    sigmaFactor = 12.0 // Custom : sigma = windowSize / 12.0
);
```

> **Important** :   
> ExportCsvAsyncFromResults does not internally validate that supplied smoothing arrays match initialData.Length. All arrays must be length-aligned; otherwise an index exception will occur during streaming.


### 3. Excel Export (Late Binding Mode)
```csharp
var req = new ExcelScoreRequest
{
    DatasetTitle = "Signal Run 42",
    InitialData = data,
    Radius = 5,
    PolyOrder = 3,
    BoundaryMode = BoundaryMode.Symmetric,
    Flags = new SmoothingNotation
    {
        Rectangular = true,
        BinomialAverage = true,
        BinomialMedian = false,
        GaussianMedian = true,
        Gaussian = true,
        SavitzkyGolay = true
    },
    Alpha = 0.75,
    SigmaFactor = 12.0  // Custom : sigma = windowSize / 12.0
};

await ExcelScoreWriter.ExportAsync(
    req,
    new Progress<int>(p => Console.WriteLine($"Excel {p}%"))
);
```

### Export Header / Label Differences

| Aspect | CsvScoreWriter | ExcelScoreWriter | SmoothingConductor ExportCsv / ExportExcel |
|--------|----------------|------------------|--------------------------------------------|
| Boundary labels | Short ("Symmetric", "Replicate", "ZeroPad", "Adaptive") | Short | Extended ("Symmetric (Mirror)", "Replicate (Nearest)", "Zero Padding", "Adaptive") |
| Alpha line | Emitted only when BinomialAverage or BinomialMedian or GaussianWeightedMedian or Gaussian enabled | Present; "Alpha Blend : {alpha}" when applicable, otherwise "Alpha Blend : N/A" | Always emitted |
| Derivative line | "Derivative Order : d" when SG & DerivOrder > 0 | Same (row after Polynomial Order) | Not emitted (always smoothing) |
| Subject / Comments rows | Not written | Stored as document properties only | Written explicitly as worksheet rows (Subject, Comments) |

### Progress Semantics

| Operation                          | Completion Behavior       |
|------------------------------------|---------------------------|
| CsvScoreWriter.ExportAsync / ExportCsvAsync* | Resets to 0 after finishing |
| SmoothingConductor.ExportExcelAsync | Ends at 100% (no reset)   |
| ExcelScoreWriter.ExportAsync        | Ends at 100% (no reset)   |

---

## Best Practices

- Keep radius modest (`3 - 9`) for interactive scenarios; large radii escalate median cost.
- Validate parameters before UI commit using `ValidateSmoothingParameters`.
- Reuse precomputed results when exporting multiple times (CSV or Excel) to avoid repeated smoothing.
- Use `BoundaryMode.Symmetric` for most natural edge continuity unless edge clamping desired.
- Consider disabling median for extremely large datasets if throughput is critical.
- Use `ScoreConformanceChecker.Validate` for concise parameter guards.
- Choose `alpha` carefully :
  - `alpha ≈ 1.0` for full-strength filtering.
  - `alpha < 1.0` to retain some original signal and reduce oversmoothing in Binomial Average / Median / Gaussian.

> **Note** :   
> `ValidateSmoothingParameters` now enforces a non‑negative kernel radius (`r ≥ 0`) in addition to existing rules. Callers no longer need to pre‑check for negative radius values before invoking the validator.

---

## Thread Safety

- Static methods are **stateless**; safe for concurrent calls.
- Do **not** reuse output arrays between concurrent operations.
- Excel export methods are inherently single-threaded due to COM constraints.

---

## Limitations & Known Inconsistencies

| Item | Detail |
|------|--------|
| Naming discrepancy | README earlier referenced `SmoothingTuner`; current implementation exposes `SmoothingConductor`. Update code or aliases if backward compatibility needed. |
| Excel `#if NET48` signature | Strong-typed branch references `ExcelExportRequest` (not provided here) while public late-binding branch uses `ExcelScoreRequest`. Harmonize names to reduce confusion. |
| Weighted median performance | O(n × w log w) cost may dominate for large windows; consider optional alternative (e.g., running selection algorithms) if needed. |
| Progress semantics | CSV exports reset progress to 0 at completion; ExcelScoreWriter.ExportAsync ends at 100%; SmoothingConductor.ExportExcelAsync ends at 100%. |
| Localization | CSV uses invariant numeric formatting; Excel writes native numeric values; headers are English. |
| Large matrices | Savitzky-Golay inversion may throw if matrix ill-conditioned for extreme parameter combinations. |
| Excel boundary label variance | High-level ExcelScoreWriter uses abbreviated labels; core SmoothingConductor Excel export uses extended labels. |
| Alpha meta-line variance | CsvScoreWriter emits "Alpha Blend" only when affected filters are enabled; ExcelScoreWriter always writes the Alpha row ("N/A" when not applicable); SmoothingConductor export paths always include "Alpha Blend". |

---

## License

MIT License. Include the full text in distribution (see root LICENSE if present).

---

## Changelog
- Added Savitzky-Golay derivative support :
  - New APIs : SmoothingConductor.ApplySGDerivative(...) and ComputeSGCoefficients(windowSize, polyOrder, derivativeOrder, delta).
  - New DTO fields : CsvScoreRequest.DerivOrder, ExcelScoreRequest.DerivOrder (0 = smoothing).
  - CSV / Excel : if DerivOrder > 0, the SG column / series is the derivative; header shows "Derivative Order : d" (Δ = 1.0).
- Validation : SmoothingConductor.ValidateSmoothingParameters enforces `r ≥ 0.`
- Alpha Blend :
  - SmoothingConductor.ApplySmoothing(alpha) blends Binomial Average, Binomial Median, Gaussian Weighted Median, and Gaussian only; Rectangular and SG remain unblended.
  - CSV / Excel headers include "Alpha Blend : {alpha}" (CsvScoreWriter / ExcelScoreWriter emit when relevant; SmoothingConductor export paths always include).
- CSV export : part-size computation includes the derivative header line only when applicable; progress semantics clarified.
- Gaussian smoothing sigma customization :
  - Added `sigmaFactor` parameter to `SmoothingConductor.ApplySmoothing(double[], int, int, BoundaryMode, bool, bool, bool, bool, bool, bool, double, double?)`.
  - If not specified, legacy behavior is used (`sigma = windowSize / 6.0`) for backward compatibility.
  - If specified, `sigma = windowSize / sigmaFactor` is used for both standard and adaptive Gaussian / weighted-median filters.
  - All related documentation (README, API Reference, XML comments) updated to fully describe `sigmaFactor` usage and behavior.
  - CSV / Excel headers include `"Gaussian Sigma : {kernelWidth} ÷ {sigmaFactor} = {sigma}"` when Gaussian or Gaussian Weighted Median is enabled, reflecting the actual sigma used for smoothing.
  - When exporting, the actual sigma value is computed and applied using the specified `sigmaFactor` parameter, ensuring the smoothing result matches the documented and header value.
  - Existing code remains fully backward compatible.
- Excel export : derivative header line emitted; multi-area range construction optimized to reduce COM interop churn.
- Docs : Updated README, API_Documentation.xml, and SonataSmooth_API_Reference.md; standardized csharp code fences; fixed Windows path examples.
- Backward compatibility : no breaking changes; existing smoothing overloads and tuple order unchanged.

---

## Disclaimer

This library focuses on clarity and predictable numeric stability over hyper-optimized micro-ops. Benchmark with your dataset before large-scale batch deployment.

---

Happy smoothing.

