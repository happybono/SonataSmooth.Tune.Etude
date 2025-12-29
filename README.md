# SonataSmooth.Tune.Etude
A C# / .NET library for advanced smoothing and signal processing (SonataSmooth.Tune), with a hands-on study application (SonataSmooth.Tune.Etude) that lets you experiment, learn, and prepare for your own data processing masterpieces : just as an Etude serves as a creative exercise before the final work.

## SonataSmooth.Tune & SonataSmooth.Tune.Etude
### Naming Philosophy

[**SonataSmooth.Tune**](https://www.nuget.org/packages/SonataSmooth.Tune) is a C# / .NET library for advanced smoothing and signal processing, designed to help you "tune" your data with precision and clarity. The name "Tune" reflects the library's purpose : to refine and harmonize data, much like tuning an instrument to achieve the desired sound.

**SonataSmooth.Tune.Etude** is the companion sample application : an "Etude" in the artistic sense. In music and art, an Etude is a study or exercise, often created to explore techniques or prepare for a final masterpiece. This sample application is crafted as a practical, hands-on study : a place to experiment, learn, and understand the capabilities of the SonataSmooth.Tune library before building your own full-scale applications. Think of it as a sketch or clay model - an essential step toward your own data processing "masterpiece."

## What's New
### v1.0.0.0
#### January 01, 2026
> Initial release.

## How to Use SonataSmooth.Tune (with Etude Sample)

This section describes how to use the SonataSmooth.Tune library, as demonstrated in the `SonataSmooth.Tune.Etude` sample application (`FrmMain.cs` and `FrmMain.Designer.cs`).

### 1. Add the Library

Reference the `SonataSmooth.Tune` library in your .NET Framework 4.8 project.

### 2. Prepare Input Data

Input data should be a sequence of numbers (e.g., `double[]`). The sample application allows you to paste or type values separated by spaces, commas, or line breaks.

### 3. Select Smoothing Methods

The UI provides checkboxes for various smoothing algorithms :
- Rectangular Average
- Binomial Average
- Binomial Median
- Gaussian Weighted Median
- Gaussian
- Savitzky-Golay Filter

You can select one or more methods to apply.

### 4. Configure Parameters

Adjust parameters using the UI controls :
- **Kernel Radius** : Size of the smoothing window.
- **Polynomial Order** : For Savitzky-Golay, sets the polynomial degree.
- **Derivative Order** : For Savitzky-Golay, computes derivatives.
- **Boundary Handling** : Symmetric, Adaptive, Replicate, or ZeroPad.
- **Alpha Blend** : Blending factor for certain filters.
- **Sigma Value** : Standard deviation for Gaussian-based methods.

### 5. Run Smoothing

Click **Start Smoothing** to process the input. The results for each selected method are displayed in the output box.

### 6. Export Results

- **Export to CSV** : Save results in a CSV file.
- **Export to Excel** : Save results in an Excel file.

#### Example: Exporting to CSV or Excel

After smoothing, you can export results using the **Export to Excel** or **Export to CSV** buttons in the sample application's UI.  
These features are implemented in the `btnExportCSV_Click` and `btnExportExcel_Click` methods of `FrmMain.cs`.

**Export to CSV**  
- Prompts for a file path, then gathers input data and smoothing parameters.
- Creates a `CsvScoreRequest` and calls `CsvScoreWriter.ExportAsync`.

```csharp
var req = new CsvScoreRequest
{
    Title = "SonataSmooth Test",
    InitialData = values,
    Radius = radius,
    PolyOrder = polyOrder,
    BoundaryMode = boundary,
    Flags = flags,
    DerivOrder = derivOrder,
    Alpha = alpha,
    SigmaFactor = sigmaFactor,
    BaseFilePath = Path.GetFileNameWithoutExtension(filePath),
    SavePath = filePath
};

await CsvScoreWriter.ExportAsync(req, progress, CancellationToken.None);
```

**Export to Excel**  
- Gathers input data and smoothing parameters (radius, polynomial order, boundary mode, alpha, sigma, etc.).
- Creates an `ExcelScoreRequest` and calls `ExcelScoreWriter.ExportAsync`.
- Progress is shown in the status bar.

```csharp
var req = new ExcelScoreRequest
{
    DatasetTitle = "SonataSmooth Test",
    InitialData = values,
    Radius = radius,
    PolyOrder = polyOrder,
    BoundaryMode = boundary,
    Flags = flags,
    DerivOrder = derivOrder,
    Alpha = alpha,
    OpenAfterExport = false,
    SigmaFactor = sigmaFactor
};

await ExcelScoreWriter.ExportAsync(req, progress);
```

> Both export features include the original and all selected smoothing results in the output file.  
> For parameter details, see the [doc/SonataSmooth_API_Documentation.xml](doc/SonataSmooth_API_Documentation.xml).

### 7. Example : Applying Smoothing in Code

Below is a simplified example based on the sample application's logic :
```csharp
using SonataSmooth.Tune;
using SonataSmooth.Tune.Export;

// Prepare input
double[] input = { 1.0, 2.0, 3.0, 4.0, 5.0 };
int radius = 2;
int polyOrder = 3;
double alpha = 1.0;
double? sigmaFactor = 6.0;
BoundaryMode boundary = BoundaryMode.Symmetric;

// Apply smoothing
var (rect, binom, median, gaussMed, gauss, sg) = SmoothingConductor.ApplySmoothing(
    input: input,
    r: radius,
    polyOrder: polyOrder,
    boundaryMode: boundary,
    doRect: true,
    doAvg: true,
    doMed: false,
    doGaussMed: false,
    doGauss: true,
    doSG: true,
    alpha: alpha,
    sigmaFactor: sigmaFactor
);

// Use results as needed
```

### 8. Advanced : Derivative Calculation

To compute derivatives with Savitzky-Golay :
```csharp
int derivOrder = 1; // e.g., first derivative
double delta = 1.0; // sample spacing

double[] sgDerivative = SmoothingConductor.ApplySGDerivative(
    input,
    radius,
    polyOrder,
    derivOrder,
    delta,
    boundary
);
```

## Installation
Get SonataSmooth.Tune from NuGet : [![NuGet](https://img.shields.io/nuget/v/SonataSmooth.Tune.svg)](https://www.nuget.org/packages/SonataSmooth.Tune)

## Further Reading
- See the [doc/SonataSmooth_API_Documentation.xml](doc/SonataSmooth_API_Documentation.xml) for full API details.
- Explore `FrmMain.cs` and `FrmMain.Designer.cs` for practical usage patterns and UI integration.

---
**SonataSmooth.Tune** : Tune your data.  
**SonataSmooth.Tune.Etude** : Study, experiment, and prepare for your own data processing masterpiece.
