#include "pch.h"
#include "SonataSmooth.Tune.Etude.h"

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;
using namespace System::Collections::Generic;
using namespace SonataSmooth::Tune;
using namespace SonataSmooth::Tune::Export;

ref class InputHelper
{
public:
    // Parse input string into a double array
    static array<double>^ ParseInputSeries(String^ input, [System::Runtime::InteropServices::Out] String^% warn)
    {
        auto tokens = Regex::Split(input == nullptr ? "" : input, "[\\s,]+");
        auto list = gcnew List<double>();
        auto bads = gcnew List<String^>();
        for each (String ^ t in tokens)
        {
            double v;
            if (Double::TryParse(t, System::Globalization::NumberStyles::Float | System::Globalization::NumberStyles::AllowThousands,
                System::Globalization::CultureInfo::InvariantCulture, v) ||
                Double::TryParse(t, System::Globalization::NumberStyles::Float | System::Globalization::NumberStyles::AllowThousands,
                    System::Globalization::CultureInfo::CurrentCulture, v))
            {
                list->Add(v);
            }
            else if (!String::IsNullOrWhiteSpace(t))
            {
                bads->Add(t);
            }
        }
        warn = bads->Count > 0 ? String::Format("Ignored tokens: {0}", String::Join(", ", bads)) : nullptr;
        return list->ToArray();
    }
};

static void PrintIfSmoothed(String^ label, array<double>^ result, array<double>^ original)
{
    bool isSame = true;
    if (result->Length == original->Length) {
        for (int i = 0; i < result->Length; ++i) {
            if (result[i] != original[i]) {
                isSame = false;
                break;
            }
        }
    }
    if (isSame)
        Console::WriteLine("{0}: No smoothing effect (identical to original)", label);
    else {
        Console::Write("{0}: ", label);
        for each (double v in result)
            Console::Write("{0} ", v);
        Console::WriteLine();
    }
}

static void ReportProgress(int p)
{
    System::Console::WriteLine("Progress: {0}%", p);
}

void Etude::Run()
{
    // Enter common parameters
    Console::Write("Input data (space / comma separated): ");
    String^ inputText = Console::ReadLine();
    String^ warn;
    array<double>^ values = InputHelper::ParseInputSeries(inputText, warn);

    if (values->Length == 0)
    {
        Console::WriteLine("No input values.");
        return;
    }
    if (!String::IsNullOrEmpty(warn))
        Console::WriteLine(warn);

    // Input selection for Smoothing Method (filter)
    Console::WriteLine("Select smoothing methods to apply (Y/N, default N):");
    Console::Write("Rectangular? (Y / N): ");
    bool doRect = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);
    Console::Write("Binomial Average? (Y / N): ");
    bool doBinomAvg = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);
    Console::Write("Binomial Median? (Y / N): ");
    bool doBinomMed = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);
    Console::Write("Gaussian Median? (Y / N): ");
    bool doGaussMed = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);
    Console::Write("Gaussian? (Y / N): ");
    bool doGauss = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);
    Console::Write("Savitzky-Golay? (Y / N): ");
    bool doSG = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);

    SmoothingNotation^ flags = gcnew SmoothingNotation();
    flags->Rectangular = doRect;
    flags->BinomialAverage = doBinomAvg;
    flags->BinomialMedian = doBinomMed;
    flags->GaussianMedian = doGaussMed;
    flags->Gaussian = doGauss;
    flags->SavitzkyGolay = doSG;

    Console::Write("Smoothing radius (int, default 4): ");
    String^ radiusStr = Console::ReadLine();
    int radius = 4;
    int parsedRadius = 0;
    if (!String::IsNullOrWhiteSpace(radiusStr) && Int32::TryParse(radiusStr, parsedRadius))
        radius = parsedRadius; 

    Console::Write("Polynomial order (int, default 2): ");
    String^ polyOrderStr = Console::ReadLine();
    int polyOrder = 2;
    int parsedPolyOrder = 0;
    if (!String::IsNullOrWhiteSpace(polyOrderStr) && Int32::TryParse(polyOrderStr, parsedPolyOrder))
        polyOrder = parsedPolyOrder;

    Console::Write("Boundary mode (0: Symmetric, 1: Replicate, 2: ZeroPad, 3: Adaptive, default 0): ");
    String^ boundaryStr = Console::ReadLine();
    int boundaryInt = 0;
    int parsedBoundary = 0;
    if (!String::IsNullOrWhiteSpace(boundaryStr) && Int32::TryParse(boundaryStr, parsedBoundary))
        boundaryInt = parsedBoundary;
    BoundaryMode boundary = static_cast<BoundaryMode>(boundaryInt);

    // Enter additional parameters depending on the selected filter
    int derivOrder = 0;
    if (doSG)
    {
        Console::Write("Savitzky-Golay Derivative order (int, default 0): ");
        String^ derivOrderStr = Console::ReadLine();
        int parsedDeriv = 0;
        if (!String::IsNullOrWhiteSpace(derivOrderStr) && Int32::TryParse(derivOrderStr, parsedDeriv))
            derivOrder = parsedDeriv;
        // Else keep derivOrder as 0
    }

    double alpha = 1.0;
    if (doBinomAvg || doBinomMed || doGaussMed || doGauss)
    {
        Console::Write("Alpha (double, default 1.0): ");
        String^ alphaStr = Console::ReadLine();
        if (!String::IsNullOrWhiteSpace(alphaStr))
        {
            if (!Double::TryParse(alphaStr, alpha))
                alpha = 1.0;
        }
        else
        {
            alpha = 1.0;
        }
    }

    double sigmaFactor = 6.0;
    if (doGauss || doGaussMed)
    {
        Console::Write("Sigma factor (double, default 6.0): ");
        String^ sigmaStr = Console::ReadLine();
        double parsedSigma = 0.0;
        if (!String::IsNullOrWhiteSpace(sigmaStr) && Double::TryParse(sigmaStr, parsedSigma))
            sigmaFactor = parsedSigma;
        // Else keep sigmaFactor as 6.0
    }

    // Select export method
    Console::Write("Export to CSV? (Y / N, default Y): ");
    bool exportCsv = !String::Equals(Console::ReadLine(), "N", StringComparison::OrdinalIgnoreCase);
    Console::Write("Export to Excel? (Y / N, default N): ");
    bool exportExcel = String::Equals(Console::ReadLine(), "Y", StringComparison::OrdinalIgnoreCase);

// Progress handler for displaying progress
auto progress = gcnew Progress<int>(gcnew Action<int>(&ReportProgress));


// CSV Export
if (exportCsv)
{
    CsvScoreRequest^ req = gcnew CsvScoreRequest();
    req->Title = "SonataSmooth C++ Sample";
    req->InitialData = values;
    req->Radius = radius;
    req->PolyOrder = polyOrder;
    req->BoundaryMode = boundary;
    req->Flags = flags;
    req->DerivOrder = derivOrder;
    req->Alpha = alpha;
    req->SigmaFactor = System::Nullable<double>(sigmaFactor);
    req->BaseFilePath = "SonataSmoothCppSample";
    // Save to the executable file location
    String^ exePath = System::Reflection::Assembly::GetEntryAssembly()->Location;
    String^ exeDir = System::IO::Path::GetDirectoryName(exePath);
    String^ csvPath = System::IO::Path::Combine(exeDir, "SonataSmoothCppSample.csv");
    req->SavePath = csvPath;

	req->OpenAfterExport = false;

    try
    {
        auto task = CsvScoreWriter::ExportAsync(req, progress, System::Threading::CancellationToken::None);
        task->Wait();
        Console::WriteLine("CSV export complete: {0}", req->SavePath);
    }
    catch (AggregateException^ ex)
    {
        for each (Exception ^ e in ex->InnerExceptions)
        {
            if (dynamic_cast<ArgumentNullException^>(e) != nullptr)
                Console::WriteLine("CSV Error: Null argument - {0}", e->Message);
            else if (dynamic_cast<InvalidOperationException^>(e) != nullptr)
                Console::WriteLine("CSV Error: Invalid operation - {0}", e->Message);
            else if (dynamic_cast<OperationCanceledException^>(e) != nullptr)
                Console::WriteLine("CSV Export canceled.");
            else
                Console::WriteLine("CSV Error: {0}", e->Message);
        }
    }
}

// Example of directly calling ApplySmoothing (output results)
try
{
    // Validate parameters (based on API documentation)
    int windowSize = 2 * radius + 1;
    if (radius < 1)
        Console::WriteLine("Warning: Smoothing radius should be >= 1 for effective smoothing.");
    if (doSG && (polyOrder < 0 || polyOrder >= windowSize))
        Console::WriteLine("Warning: For Savitzky-Golay, 0 <= polyOrder < (2 * radius + 1) must be satisfied.");

    // Actual smoothing call
    auto smoothingResults = SmoothingConductor::ApplySmoothing(
        values,                               // array<double>^ input
        radius,                               // int r
        polyOrder,                            // int polyOrder
        boundary,                             // BoundaryMode
        flags->Rectangular,                   // bool doRect
        flags->BinomialAverage,               // bool doAvg
        flags->BinomialMedian,                // bool doMed
        flags->GaussianMedian,                // bool doGaussMed
        flags->Gaussian,                      // bool doGauss
        flags->SavitzkyGolay,                 // bool doSG
        alpha,                                // double alpha
        System::Nullable<double>(sigmaFactor) // wrap in Nullable
    );

    auto original = values;
    auto rect = smoothingResults.Item1;
    auto binomAvg = smoothingResults.Item2;
    auto binomMed = smoothingResults.Item3;
    auto gaussMed = smoothingResults.Item4;
    auto gauss = smoothingResults.Item5;
    auto sg = smoothingResults.Item6;

    if (flags->Rectangular)
        PrintIfSmoothed("Rectangular Smoothing", rect, original);
    if (flags->BinomialAverage)
        PrintIfSmoothed("Binomial Average Smoothing", binomAvg, original);
    if (flags->BinomialMedian)
        PrintIfSmoothed("Binomial Median Smoothing", binomMed, original);
    if (flags->GaussianMedian)
        PrintIfSmoothed("Gaussian Median Smoothing", gaussMed, original);
    if (flags->Gaussian)
        PrintIfSmoothed("Gaussian Smoothing", gauss, original);
    if (flags->SavitzkyGolay)
        PrintIfSmoothed("Savitzky-Golay Smoothing", sg, original);
}
catch (ArgumentNullException^ e)
{
    Console::WriteLine("Smoothing Error: Null argument - {0}", e->Message);
}
catch (ArgumentOutOfRangeException^ e)
{
    Console::WriteLine("Smoothing Error: Out of range - {0}", e->Message);
}
catch (Exception^ e)
{
    Console::WriteLine("Smoothing Error: {0}", e->Message);
}


// Excel Export
if (exportExcel)
{
    ExcelScoreRequest^ excelReq = gcnew ExcelScoreRequest();
    excelReq->DatasetTitle = "SonataSmooth C++ Sample";
    excelReq->InitialData = values;
    excelReq->Radius = radius;
    excelReq->PolyOrder = polyOrder;
    excelReq->BoundaryMode = boundary;
    excelReq->Flags = flags;
    excelReq->DerivOrder = derivOrder;
    excelReq->Alpha = alpha;
    excelReq->SigmaFactor = System::Nullable<double>(sigmaFactor);

    // Save to the executable file location
    String^ exePath = System::Reflection::Assembly::GetEntryAssembly()->Location;
    String^ exeDir = System::IO::Path::GetDirectoryName(exePath);
    String^ excelPath = System::IO::Path::Combine(exeDir, "SonataSmoothCppSample.xlsx");
    excelReq->SavePath = excelPath;

    excelReq->OpenAfterExport = false;

    try
    {
        auto excelTask = ExcelScoreWriter::ExportAsync(excelReq, progress);
        excelTask->Wait();
        Console::WriteLine("Excel export complete: {0}", excelReq->SavePath);
    }
    catch (AggregateException^ ex)
    {
        for each (Exception ^ e in ex->InnerExceptions)
        {
            if (dynamic_cast<ArgumentNullException^>(e) != nullptr)
                Console::WriteLine("Excel Error: Null argument - {0}", e->Message);
            else if (dynamic_cast<InvalidOperationException^>(e) != nullptr)
                Console::WriteLine("Excel Error: Invalid operation - {0}", e->Message);
            else if (dynamic_cast<OperationCanceledException^>(e) != nullptr)
                Console::WriteLine("Excel Export canceled.");
            else if (e->GetType()->FullName->Contains("ExcelInteropNotAvailableException"))
                Console::WriteLine("Excel Error: Excel interop not available - {0}", e->Message);
            else
                Console::WriteLine("Excel Error: {0}", e->Message);
        }
    }
}

Console::WriteLine("Press Enter to exit.");
Console::ReadLine();
}