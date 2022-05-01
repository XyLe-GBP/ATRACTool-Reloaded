#include "Common.h"

BOOL Common::CopyDirectory(LPCTSTR lpExistingDirectoryName, LPCTSTR lpNewDirectoryName)
{
	// 入力値チェック
	if (NULL == lpExistingDirectoryName
		|| NULL == lpNewDirectoryName)
	{
		return FALSE;
	}

	// ディレクトリ名の保存（終端に'\'がないなら付ける）
	TCHAR szDirectoryPathName_existing[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName_existing, _MAX_PATH, lpExistingDirectoryName, _TRUNCATE);
	if ('\\' != szDirectoryPathName_existing[_tcslen(szDirectoryPathName_existing) - 1])
	{	// 一番最後に'\'がないなら付加する。
		_tcsncat_s(szDirectoryPathName_existing, _MAX_PATH, _T("\\"), _TRUNCATE);
	}
	TCHAR szDirectoryPathName_new[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName_new, _MAX_PATH, lpNewDirectoryName, _TRUNCATE);
	if ('\\' != szDirectoryPathName_new[_tcslen(szDirectoryPathName_new) - 1])
	{	// 一番最後に'\'がないなら付加する。
		_tcsncat_s(szDirectoryPathName_new, _MAX_PATH, _T("\\"), _TRUNCATE);
	}

	if (-1 == _taccess(szDirectoryPathName_existing, 0))
	{
		return FALSE;
	}
	if (0 == _tcsicmp(szDirectoryPathName_existing, szDirectoryPathName_new))
	{
		return FALSE;
	}

	// 新たなディレクトリの作成
	CreateDirectory(szDirectoryPathName_new, NULL);

	// ディレクトリ内のファイル走査用のファイル名作成
	TCHAR szFindFilePathName[_MAX_PATH];
	_tcsncpy_s(szFindFilePathName, _MAX_PATH, szDirectoryPathName_existing, _TRUNCATE);
	_tcsncat_s(szFindFilePathName, _T("*"), _TRUNCATE);

	// ディレクトリ内のファイル走査開始
	WIN32_FIND_DATA		fd;
	HANDLE hFind = FindFirstFile(szFindFilePathName, &fd);
	if (INVALID_HANDLE_VALUE == hFind)
	{	// 走査対象フォルダが存在しない。
		return FALSE;
	}

	do
	{
		if ('.' != fd.cFileName[0])
		{
			TCHAR szFoundFilePathName_existing[_MAX_PATH];
			_tcsncpy_s(szFoundFilePathName_existing, _MAX_PATH, szDirectoryPathName_existing, _TRUNCATE);
			_tcsncat_s(szFoundFilePathName_existing, _MAX_PATH, fd.cFileName, _TRUNCATE);

			TCHAR szFoundFilePathName_new[_MAX_PATH];
			_tcsncpy_s(szFoundFilePathName_new, _MAX_PATH, szDirectoryPathName_new, _TRUNCATE);
			_tcsncat_s(szFoundFilePathName_new, _MAX_PATH, fd.cFileName, _TRUNCATE);

			if (FILE_ATTRIBUTE_DIRECTORY & fd.dwFileAttributes)
			{	// ディレクトリなら再起呼び出しでコピー
				if (!Common::CopyDirectory(szFoundFilePathName_existing, szFoundFilePathName_new))
				{
					FindClose(hFind);
					return FALSE;
				}
			}
			else
			{	// ファイルならWin32API関数を用いてコピー
				if (!CopyFile(szFoundFilePathName_existing, szFoundFilePathName_new, FALSE))
				{
					FindClose(hFind);
					return FALSE;
				}
			}
		}
	} while (FindNextFile(hFind, &fd));

	FindClose(hFind);

	return TRUE;
}

BOOL Common::CopyDirectoryUseShellFunc(LPCTSTR lpExistingDirectoryName, LPCTSTR lpNewDirectoryName)
{
	// 入力値チェック
	if (NULL == lpExistingDirectoryName
		|| NULL == lpNewDirectoryName)
	{
		return FALSE;
	}

	// ディレクトリ名の保存（終端に'\'がないなら付ける）（終端を、"\0\0"にする）
	TCHAR szDirectoryPathName_existing[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName_existing, _MAX_PATH, lpExistingDirectoryName, _TRUNCATE);
	if ('\\' == szDirectoryPathName_existing[_tcslen(szDirectoryPathName_existing) - 1])
	{	// 一番最後に'\'があるなら取り去る。
		szDirectoryPathName_existing[_tcslen(szDirectoryPathName_existing) - 1] = '\0';
	}
	szDirectoryPathName_existing[_tcslen(szDirectoryPathName_existing) + 1] = '\0';
	TCHAR szDirectoryPathName_new[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName_new, _MAX_PATH, lpNewDirectoryName, _TRUNCATE);
	if ('\\' == szDirectoryPathName_new[_tcslen(szDirectoryPathName_new) - 1])
	{	// 一番最後に'\'があるなら取り去る。
		szDirectoryPathName_new[_tcslen(szDirectoryPathName_new) - 1] = '\0';
	}
	szDirectoryPathName_new[_tcslen(szDirectoryPathName_new) + 1] = '\0';

	if (-1 == _taccess(szDirectoryPathName_existing, 0))
	{
		return FALSE;
	}
	if (0 == _tcsicmp(szDirectoryPathName_existing, szDirectoryPathName_new))
	{
		return FALSE;
	}

	SHFILEOPSTRUCT fos;
	ZeroMemory(&fos, sizeof(SHFILEOPSTRUCT));
	fos.hwnd = NULL;
	fos.wFunc = FO_COPY;
	fos.pFrom = szDirectoryPathName_existing;
	fos.pTo = szDirectoryPathName_new;
	fos.fFlags = FOF_NOCONFIRMATION | FOF_MULTIDESTFILES | FOF_NOERRORUI | FOF_SILENT;	// | FOF_NOCONFIRMMKDIR
	if (SHFileOperation(&fos))
	{	// 成功すると０が、失敗すると０以外が返る。
		return FALSE;
	}
	return TRUE;
}

bool Common::CopyDirectoryFiles(tstring folderPath, tstring destfolderPath, vector<tstring>& file_names)
{
	HANDLE hFind;
	WIN32_FIND_DATA win32fd;
	tstring search_name = folderPath + _T("\\*");

	hFind = FindFirstFile(search_name.c_str(), &win32fd);

	if (hFind == INVALID_HANDLE_VALUE) {
		throw runtime_error("file not found");
		return false;
	}

	/* 指定のディレクトリ以下のファイル名をファイルがなくなるまで取得する */
	do {
		if (win32fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
			/* ディレクトリの場合は何もしない */
			printf("directory\n");
		}
		else {
			/* ファイルが見つかったらVector配列に保存する */
			file_names.push_back(win32fd.cFileName);
			tstring sp = folderPath + _T("\\") + file_names.back();
			tstring dp = destfolderPath + file_names.back();
			CopyFile(sp.c_str(), dp.c_str(), FALSE);
			printf("copyfile: ");
			printf(Common::TWStringToString(file_names.back().c_str()).c_str());
			printf("\n");
		}
	} while (FindNextFile(hFind, &win32fd));

	FindClose(hFind);

	return true;
}

BOOL Common::DeleteDirectory(LPCTSTR lpPathName)
{
	// 入力値チェック
	if (NULL == lpPathName)
	{
		return FALSE;
	}

	// ディレクトリ名の保存（終端に'\'がないなら付ける）
	TCHAR szDirectoryPathName[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName, _MAX_PATH, lpPathName, _TRUNCATE);
	if ('\\' != szDirectoryPathName[_tcslen(szDirectoryPathName) - 1])
	{	// 一番最後に'\'がないなら付加する。
		_tcsncat_s(szDirectoryPathName, _MAX_PATH, _T("\\"), _TRUNCATE);
	}

	// ディレクトリ内のファイル走査用のファイル名作成
	TCHAR szFindFilePathName[_MAX_PATH];
	_tcsncpy_s(szFindFilePathName, _MAX_PATH, szDirectoryPathName, _TRUNCATE);
	_tcsncat_s(szFindFilePathName, _MAX_PATH, _T("*"), _TRUNCATE);

	// ディレクトリ内のファイル走査開始
	WIN32_FIND_DATA		fd;
	HANDLE hFind = FindFirstFile(szFindFilePathName, &fd);
	if (INVALID_HANDLE_VALUE == hFind)
	{	// 走査対象フォルダが存在しない。
		return FALSE;
	}

	do
	{
		//if( '.' != fd.cFileName[0] )
		if (0 != _tcscmp(fd.cFileName, _T("."))		// カレントフォルダ「.」と
			&& 0 != _tcscmp(fd.cFileName, _T("..")))	// 親フォルダ「..」は、処理をスキップ
		{
			TCHAR szFoundFilePathName[_MAX_PATH];
			_tcsncpy_s(szFoundFilePathName, _MAX_PATH, szDirectoryPathName, _TRUNCATE);
			_tcsncat_s(szFoundFilePathName, _MAX_PATH, fd.cFileName, _TRUNCATE);

			if (FILE_ATTRIBUTE_DIRECTORY & fd.dwFileAttributes)
			{	// ディレクトリなら再起呼び出しで削除
				if (!DeleteDirectory(szFoundFilePathName))
				{
					FindClose(hFind);
					return FALSE;
				}
			}
			else
			{	// ファイルならWin32API関数を用いて削除
				if (!DeleteFile(szFoundFilePathName))
				{
					FindClose(hFind);
					return FALSE;
				}
			}
		}
	} while (FindNextFile(hFind, &fd));

	FindClose(hFind);

	return RemoveDirectory(lpPathName);
}

BOOL Common::DeleteDirectoryUseShellFunc(LPCTSTR lpPathName)
{
	// 入力値チェック
	if (NULL == lpPathName)
	{
		return FALSE;
	}

	// ディレクトリ名の保存（終端を、"\0\0"にする）
	TCHAR szDirectoryPathName[_MAX_PATH];
	_tcsncpy_s(szDirectoryPathName, _MAX_PATH, lpPathName, _TRUNCATE);
	if ('\\' == szDirectoryPathName[_tcslen(szDirectoryPathName) - 1])
	{	// 一番最後に'\'があるなら取り去る。
		szDirectoryPathName[_tcslen(szDirectoryPathName) - 1] = '\0';
	}
	szDirectoryPathName[_tcslen(szDirectoryPathName) + 1] = '\0';

	SHFILEOPSTRUCT fos;
	ZeroMemory(&fos, sizeof(SHFILEOPSTRUCT));
	fos.hwnd = NULL;
	fos.wFunc = FO_DELETE;
	fos.pFrom = szDirectoryPathName;
	fos.fFlags = FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT; // | FOF_ALLOWUNDO;
	if (SHFileOperation(&fos))
	{	// 成功すると０が、失敗すると０以外が返る。
		return FALSE;
	}
	return TRUE;
}

BOOL Common::ExtractZip(IShellDispatch* pShellDisp, TCHAR* ZipPath, TCHAR* OutPath) {
	HRESULT hr;
	VARIANT vDtcDir;
	Folder* pOutDtc;

	VariantInit(&vDtcDir);
	vDtcDir.vt = VT_BSTR;
	vDtcDir.bstrVal = SysAllocString(OutPath);
	hr = pShellDisp->NameSpace(vDtcDir, &pOutDtc);
	VariantClear(&vDtcDir);
	if (hr != S_OK) {
		MessageBox(NULL, TEXT("The destination folder could not be found."), TEXT("Error"), MB_ICONWARNING);
		return FALSE;
	}

	VARIANT varZip;
	Folder* pZipFile;
	VariantInit(&varZip);
	varZip.vt = VT_BSTR;
	varZip.bstrVal = SysAllocString(ZipPath);
	hr = pShellDisp->NameSpace(varZip, &pZipFile);
	VariantClear(&varZip);
	if (hr != S_OK) {
		pOutDtc->Release();
		MessageBox(NULL, TEXT("ZIP archive could not be found."), TEXT("Error"), MB_ICONWARNING);
		return FALSE;
	}

	FolderItems* pZipItems;
	hr = pZipFile->Items(&pZipItems);
	if (hr != S_OK) {
		pZipFile->Release();
		pOutDtc->Release();
		return FALSE;
	}

	VARIANT vDisp, vOpt;
	VariantInit(&vDisp);
	vDisp.vt = VT_DISPATCH;
	vDisp.pdispVal = pZipItems;
	VariantInit(&vOpt);
	vOpt.vt = VT_I4;
	vOpt.lVal = 0;
	hr = pOutDtc->CopyHere(vDisp, vOpt);
	if (hr != S_OK) {
		pZipItems->Release();
		pZipFile->Release();
		pOutDtc->Release();
		MessageBox(NULL, TEXT("Failed to unzip file."), TEXT("Error"), MB_ICONWARNING);
		return FALSE;
	}

	pZipItems->Release();
	pZipFile->Release();
	pOutDtc->Release();

	return TRUE;
}

void Common::FindDirectory(wstring oFolderPath)
{
	WIN32_FIND_DATA tFindFileData;

	// 全てのファイル
	oFolderPath += L"¥¥*.*";

	// 最初に一致するファイルを取得
	HANDLE hFile = ::FindFirstFile(oFolderPath.c_str(), &tFindFileData);
	if (INVALID_HANDLE_VALUE == hFile) {
		return;
	}

	// L"¥¥*.*"を削除
	oFolderPath = oFolderPath.substr(0, oFolderPath.size() - 4);

	do {

		TCHAR* wpFileName = tFindFileData.cFileName;

		// フォルダかどうかを判定
		if (tFindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {

			/*
				L"."とL".."はスキップ
			*/
			if (L'.' == wpFileName[0]) {
				if ((L'¥0' == wpFileName[1])
					|| (L'.' == wpFileName[1] && L'¥0' == wpFileName[2])
					) {
					continue;
				}
			}

			// フルパスの生成
			std::wstring oFullPath = oFolderPath + L"¥¥" + wpFileName;
			std::wcout << L"(dir )" << oFullPath << std::endl;

			// 再起してサブフォルダを巡回する
			Common::FindDirectory(oFullPath);
		}
		else {

			// フルパスの生成
			std::wstring oFullPath = oFolderPath + L"¥¥" + wpFileName;
			std::wcout << L"(file)" << oFullPath << std::endl;
		}

		// 次に一致するファイルの検索
	} while (::FindNextFile(hFile, &tFindFileData));

	// 検索ハンドルを閉じる
	::FindClose(hFile);
}

// wstringをstringに変換する
string Common::TWStringToString(const wstring& arg_wstr)
{
	string result;
	size_t length = arg_wstr.size();
	size_t convLength;
	char* c = (char*)malloc(sizeof(char) * length * 2);
	wcstombs_s(&convLength, c, sizeof(char) * length * 2, arg_wstr.c_str(), length * 2);
	if (c) {
		result = c;
	}
	SAFE_FREE(c);

	return result;
};

// stringをwstringに変換する
wstring Common::StringToWString(const string& arg_str)
{
	std::wstring result;
	size_t length = arg_str.size();
	size_t convLength;
	wchar_t* wc = (wchar_t*)malloc(sizeof(wchar_t) * (length + 2));
	mbstowcs_s(&convLength, wc, length + 1, arg_str.c_str(), _TRUNCATE);
	if (wc) {
		result = wc;
	}
	SAFE_FREE(wc);

	return result;
};

TCHAR* Common::ConvertTCHAR(LPCTSTR string) {
	TCHAR* tmpT = (TCHAR*)malloc(sizeof(TCHAR) * 256); // Convert TCHAR.
	if (NULL == tmpT) {
		perror("can not malloc");
		OutputDebugString(_T("TCHAR syntax (tmpT) malloc failed.\n"));
		MessageBox(NULL, TEXT("malloc failed."), TEXT("Error"), MB_ICONWARNING);
		return NULL;
	}
	else {
		ZeroMemory(&tmpT[0], 256);
		_tcscpy_s(&tmpT[0], 256, string);
	}
	return tmpT;
};
