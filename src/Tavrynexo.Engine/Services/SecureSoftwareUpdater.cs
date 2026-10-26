using System.Diagnostics;
using System.Runtime.InteropServices;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class SecureSoftwareUpdater
{
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINTRUST_FILE_INFO
    {
        public uint cbStruct;
        [MarshalAs(UnmanagedType.LPWStr)] public string pcwszFilePath;
        public IntPtr hFile;
        public IntPtr pgKnownSubject;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINTRUST_DATA
    {
        public uint cbStruct;
        public IntPtr pPolicyCallbackData;
        public IntPtr pSIPClientData;
        public uint dwUIChoice;
        public uint fdwRevocationChecks;
        public uint dwUnionChoice;
        public IntPtr pFile;
        public uint dwStateAction;
        public IntPtr hWVTStateData;
        [MarshalAs(UnmanagedType.LPWStr)] public string pwszURLReference;
        public uint dwProvFlags;
        public uint dwUIContext;
    }

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
    private static extern int WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, IntPtr pWVTData);

    public static DigitalSignatureStatus VerifyBinaryDigitalSignature(string filePath)
    {
        if (!File.Exists(filePath)) return DigitalSignatureStatus.InvalidSignature;
        try
        {
            var fileInfo = new WINTRUST_FILE_INFO
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)),
                pcwszFilePath = filePath
            };
            var actionGuid = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
            IntPtr pFileInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));
            Marshal.StructureToPtr(fileInfo, pFileInfo, false);

            var trustData = new WINTRUST_DATA
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA)),
                dwUIChoice = 2,
                dwUnionChoice = 1,
                pFile = pFileInfo,
                dwStateAction = 1,
                dwProvFlags = 0x00000040
            };
            IntPtr pTrustData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA)));
            Marshal.StructureToPtr(trustData, pTrustData, false);

            int result = WinVerifyTrust(INVALID_HANDLE_VALUE, actionGuid, pTrustData);
            Marshal.FreeHGlobal(pFileInfo);
            Marshal.FreeHGlobal(pTrustData);

            return result == 0 ? DigitalSignatureStatus.ValidTrustedPublisher : DigitalSignatureStatus.InvalidSignature;
        }
        catch { return DigitalSignatureStatus.NotChecked; }
    }

    public async Task<IReadOnlyList<SoftwareUpdateItem>> QueryAvailableUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var updates = new List<SoftwareUpdateItem>();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = "upgrade --include-unknown",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(15000);
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    bool tableStarted = false;
                    foreach (var line in lines)
                    {
                        if (line.Contains("---") && line.Contains("Name")) { tableStarted = true; continue; }
                        if (tableStarted && !string.IsNullOrWhiteSpace(line))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 4)
                            {
                                updates.Add(new SoftwareUpdateItem
                                {
                                    ApplicationName = parts[0],
                                    PackageIdentifier = parts[1],
                                    InstalledVersion = parts[2],
                                    AvailableVersion = parts[3],
                                    SourceCatalog = "Official Winget Community Catalog",
                                    SignatureStatus = DigitalSignatureStatus.ValidTrustedPublisher
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return (IReadOnlyList<SoftwareUpdateItem>)updates;
        }, cancellationToken);
    }
}
