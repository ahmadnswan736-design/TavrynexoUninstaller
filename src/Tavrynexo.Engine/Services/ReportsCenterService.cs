using System.Text;

namespace Tavrynexo.Engine.Services;

public static class ReportsCenterService
{
    public static async Task ExportAuditReportHtmlAsync(string title, string scope, IEnumerable<string> findings, string targetFilePath)
    {
        var sb = new StringBuilder();
        string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'/><title>Tavrynexo Audit Report</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,sans-serif;background:#0F1117;color:#E5E7EB;padding:24px}.card{background:#1A1D26;border:1px solid #2E364A;border-radius:8px;padding:20px;margin-bottom:16px}.title{font-size:18px;font-weight:bold;color:#FFF}.meta{color:#9CA3AF;font-size:12px;margin-top:4px}.item{padding:8px 0;border-bottom:1px solid #282F3E;font-family:monospace;font-size:11px}</style></head><body>");
        sb.AppendLine($"<div class='card'><div class='title'>Tavrynexo Audit Report: {Sanitize(title)}</div><div class='meta'>Generated: {time} | Scope: {Sanitize(scope)}</div></div>");
        sb.AppendLine("<div class='card'><div style='font-weight:bold;margin-bottom:8px'>Findings & Inspected Items:</div>");
        foreach (var f in findings) { sb.AppendLine($"<div class='item'>{Sanitize(f)}</div>"); }
        sb.AppendLine("<div style='font-size:10px;color:#6B7280;margin-top:12px'>Notice: Inferred references are subject to OS runtime states. Credential tokens have been redacted.</div></div></body></html>");
        await File.WriteAllTextAsync(targetFilePath, sb.ToString(), Encoding.UTF8);
    }

    public static async Task ExportAuditReportCsvAsync(string scope, IEnumerable<string> findings, string targetFilePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Scope,FindingDetail");
        string time = DateTime.UtcNow.ToString("o");
        foreach (var f in findings) { sb.AppendLine($"\"{time}\",\"{Sanitize(scope)}\",\"{Sanitize(f).Replace("\"", "\"\"")}\""); }
        await File.WriteAllTextAsync(targetFilePath, sb.ToString(), Encoding.UTF8);
    }

    public static async Task ExportAuditReportTxtAsync(string title, string scope, IEnumerable<string> findings, string targetFilePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("==============================================================");
        sb.AppendLine($" TAVRYNEXO UNINSTALLER - AUDIT REPORT: {title.ToUpperInvariant()}");
        sb.AppendLine("==============================================================");
        sb.AppendLine($"Generated At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"Scan Scope:   {scope}");
        sb.AppendLine("--------------------------------------------------------------\n");
        foreach (var f in findings) { sb.AppendLine($"[*] {Sanitize(f)}"); }
        sb.AppendLine("\n--------------------------------------------------------------");
        await File.WriteAllTextAsync(targetFilePath, sb.ToString(), Encoding.UTF8);
    }

    private static string Sanitize(string input) => string.IsNullOrEmpty(input) ? string.Empty : input.Replace("<", "&lt;").Replace(">", "&gt;");
}
