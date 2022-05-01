#include <iostream>
#include <direct.h>
#include <fstream>
#include <sstream>
#include <shlobj.h>
#include <shlwapi.h>
#include <tchar.h>
#include <tlhelp32.h>
#include <vector>

#pragma comment(lib, "Shlwapi.lib")

using namespace std;
typedef std::basic_string<TCHAR, char_traits<TCHAR>, allocator<TCHAR> > tstring;

#define SAFE_FREE(ptr) { free(ptr); ptr = NULL; }
#define SAFE_DELETE(ptr) { delete ptr; ptr = NULL; }

#pragma once
class Common {
public:
	BOOL CopyDirectory(LPCTSTR lpExistingDirectoryName, LPCTSTR lpNewDirectoryName);
	BOOL CopyDirectoryUseShellFunc(LPCTSTR lpExistingDirectoryName, LPCTSTR lpNewDirectoryName);
	bool CopyDirectoryFiles(tstring folderPath, tstring destfolderPath, vector<tstring>& file_names);
	BOOL DeleteDirectory(LPCTSTR lpPathName);
	BOOL DeleteDirectoryUseShellFunc(LPCTSTR lpPathName);
	BOOL ExtractZip(IShellDispatch* pShellDisp, TCHAR* ZipPath, TCHAR* OutPath);
	void FindDirectory(wstring oFolderPath);
	string TWStringToString(const wstring& arg_wstr);
	wstring StringToWString(const string& arg_str);
	TCHAR* ConvertTCHAR(LPCTSTR string);
};

