#pragma once
#include <windows.h>
#include <stdint.h>

#ifdef TAVRYNEXO_EXPORTS
#define TN_API __declspec(dllexport)
#else
#define TN_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

#pragma pack(push, 8)
typedef struct _TN_ProcessInfo {
    uint32_t ProcessId;
    wchar_t ApplicationName[260];
    uint32_t ApplicationType;
    uint32_t ProcessFlags;
} TN_ProcessInfo;
#pragma pack(pop)

#define TN_SUCCESS 0
#define TN_ERROR_INVALID_PARAMETER 1
#define TN_ERROR_BUFFER_TOO_SMALL 2
#define TN_ERROR_RESTART_MANAGER_FAILED 3
#define TN_ERROR_PATH_NOT_FOUND 4

TN_API int32_t TN_FindProcessesLockingFile(
    const wchar_t* pszPath,
    TN_ProcessInfo* pOutProcesses,
    uint32_t maxProcesses,
    uint32_t* pdwProcessCount);

TN_API int32_t TN_FastFolderScan(
    const wchar_t* pszDirPath,
    uint64_t* pullTotalSizeBytes,
    uint32_t* pdwTotalFiles);

#ifdef __cplusplus
}
#endif
