using System;
using System.IO;
using System.Text;
using CSVFile;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using VersOne.Epub;

namespace EGOIST.Presentation.UI.Services.Utilities;

public static class FileTextExtractor
{
    public static string ExtractText(string filePath)
    {
        return ExtractText(filePath, null);
    }

    public static string ExtractText(byte[] fileBytes, string fileExtension)
    {
        return ExtractText(null, fileBytes, fileExtension);
    }

    private static string ExtractText(string? filePath, byte[]? fileBytes, string? fileExtension = null)
    {
        try
        {
            if (filePath != null)
            {
                fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
                return ExtractTextFromFile(filePath, fileExtension);
            }

            if (fileBytes != null && !string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = fileExtension.ToLowerInvariant();
                return ExtractTextFromBytes(fileBytes, fileExtension);
            }

            throw new ArgumentException("Either filePath or (fileBytes and fileExtension) must be provided.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text: {ex.Message}");
            return "Error extracting text.";
        }
    }

    private static string ExtractTextFromFile(string filePath, string fileExtension)
    {
        return fileExtension switch
        {
            ".pdf" => ExtractTextFromPdf(filePath),
            ".csv" => ExtractTextFromCsv(filePath),
            ".doc" or ".docx" => ExtractTextFromDocx(filePath),
            ".epub" => ExtractTextFromEpub(filePath),
            ".txt" or ".md" or ".cs" or ".js" or ".py" or ".java" or ".cpp" or ".h" or ".hpp" or ".c" or ".cc"
            or ".cxx" or ".go" or ".rb" or ".php" or ".swift" or ".ts" or ".rs" or ".kt" or ".scala" or ".vb"
            or ".fs" or ".lua" or ".pl" or ".r" or ".m" or ".sql" or ".sh" or ".bat" or ".ps1" or ".yml" or ".yaml"
            or ".json" or ".xml" or ".html" or ".css" or ".less" or ".sass" or ".scss" => ExtractTextFromTxt(filePath),
            _ => throw new NotSupportedException("Unsupported file type.")
        };
    }

    private static string ExtractTextFromBytes(byte[] fileBytes, string fileExtension)
    {
        return fileExtension switch
        {
            ".pdf" => ExtractTextFromPdf(fileBytes),
            ".csv" => ExtractTextFromCsv(fileBytes),
            ".doc" or ".docx" => ExtractTextFromDocx(fileBytes),
            ".epub" => ExtractTextFromEpub(fileBytes),
            ".txt" or ".md" or ".cs" or ".js" or ".py" or ".java" or ".cpp" or ".h" or ".hpp" or ".c" or ".cc"
            or ".cxx" or ".go" or ".rb" or ".php" or ".swift" or ".ts" or ".rs" or ".kt" or ".scala" or ".vb"
            or ".fs" or ".lua" or ".pl" or ".r" or ".m" or ".sql" or ".sh" or ".bat" or ".ps1" or ".yml" or ".yaml"
            or ".json" or ".xml" or ".html" or ".css" or ".less" or ".sass" or ".scss" => ExtractTextFromTxt(fileBytes),
            _ => throw new NotSupportedException("Unsupported file type.")
        };
    }

    private static string ExtractTextFromTxt(string filePath)
    {
        return File.ReadAllText(filePath, Encoding.UTF8);
    }

    private static string ExtractTextFromPdf(string filePath)
    {
        using var pdfDoc = PdfDocument.Open(filePath);
        return ExtractTextFromPdfDocument(pdfDoc);
    }

    private static string ExtractTextFromCsv(string filePath)
    {
        using var csv = CSVReader.FromFile(filePath);
        return ExtractTextFromCsvReader(csv);
    }

    private static string ExtractTextFromDocx(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        return ExtractTextFromWordDocument(doc);
    }

    private static string ExtractTextFromEpub(string filePath)
    {
        var epubBook = EpubReader.ReadBook(filePath);
        return ExtractTextFromEpubBook(epubBook);
    }

    private static string ExtractTextFromPdf(byte[] fileBytes)
    {
        using var pdfDoc = PdfDocument.Open(fileBytes);
        return ExtractTextFromPdfDocument(pdfDoc);
    }

    private static string ExtractTextFromCsv(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var csv = new CSVReader(stream);
        return ExtractTextFromCsvReader(csv);
    }

    private static string ExtractTextFromDocx(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var doc = WordprocessingDocument.Open(stream, false);
        return ExtractTextFromWordDocument(doc);
    }

    private static string ExtractTextFromEpub(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        var epubBook = EpubReader.ReadBook(stream);
        return ExtractTextFromEpubBook(epubBook);
    }

    private static string ExtractTextFromTxt(byte[] fileBytes)
    {
        return Encoding.UTF8.GetString(fileBytes);
    }

    // Helper methods to extract text from different document types
    private static string ExtractTextFromPdfDocument(PdfDocument pdfDoc)
    {
        var text = new StringBuilder();
        foreach (var page in pdfDoc.GetPages())
        {
            text.Append(page.Text);
        }
        return text.ToString();
    }

    private static string ExtractTextFromCsvReader(CSVReader csv)
    {
        var text = new StringBuilder();
        foreach (var row in csv)
        {
            text.AppendLine(string.Join(", ", row));
        }
        return text.ToString();
    }

    private static string ExtractTextFromWordDocument(WordprocessingDocument doc)
    {
        var body = doc.MainDocumentPart?.Document.Body;
        if (body == null)
            return string.Empty;

        var text = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            text.AppendLine(paragraph.InnerText);
        }
        return text.ToString();
    }

    private static string ExtractTextFromEpubBook(EpubBook epubBook)
    {
        var text = new StringBuilder();
        foreach (var htmlContentFile in epubBook.Content.Html.Local)
        {
            text.AppendLine(htmlContentFile.Content);
        }
        return text.ToString();
    }
}