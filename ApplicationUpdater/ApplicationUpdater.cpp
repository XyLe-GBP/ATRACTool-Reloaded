// ApplicationUpdater.cpp : このファイルには 'main' 関数が含まれています。プログラム実行の開始と終了がそこで行われます。
//

#include "Common.h"

int main()
{
	Common* common = new Common;
	TCHAR Path[MAX_PATH + 1];
	string AppPath, AppType;

	if (0 != GetModuleFileName(NULL, Path, MAX_PATH)) {

		TCHAR drive[MAX_PATH + 1]
			, dir[MAX_PATH + 1]
			, fname[MAX_PATH + 1]
			, ext[MAX_PATH + 1];

		_tsplitpath_s(Path, drive, dir, fname, ext);

		string sdrive = common->TWStringToString(drive)
			, sdir = common->TWStringToString(dir)
			, sfname = common->TWStringToString(fname)
			, sext = common->TWStringToString(ext);

		if (PathFileExists(common->StringToWString(sdrive + sdir + "updater.dat").c_str())) {
			cout << "Updater - Application Update Utility v1.2" << endl;
			cout << "Copyright (C) 2022 XyLe. All Rights Reserved.\n" << endl;

			ifstream ifs(common->StringToWString(sdrive + sdir + "updater.dat").c_str());
			if (!ifs) {
				MessageBox(NULL, TEXT("Could not open file."), TEXT("Error"), MB_ICONWARNING);
				SAFE_DELETE(common);
				return -1;
			}
			else {
				string buf;
				int count = 0;
				while (getline(ifs, buf))
				{
					if (count == 0) {
						AppPath = buf;
					}
					else {
						AppType = buf;
					}
					count++;
				}
			}
			ifs.close();
			DeleteFile(common->StringToWString(sdrive + sdir + "updater.dat").c_str());
			
			common->DeleteDirectory(common->StringToWString(AppPath).c_str());
			if (PathFileExists(common->StringToWString(sdrive + sdir + "atractool-rel.zip").c_str())) {
				HRESULT hr;
				IShellDispatch* pShellDisp{};
				if (CoInitializeEx(0, COINIT_MULTITHREADED) == 0) {
					cout << "CoInitializeEx: S_OK" << endl;
					OutputDebugString(_T("CoInitializeEx: S_OK\n"));
				}
				else {
					cout << "WARNING: CoInitializeEx: S_FALSE" << endl;
					OutputDebugString(_T("CoInitializeEx: S_FALSE\n"));
				}
				hr = CoCreateInstance(CLSID_Shell, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pShellDisp));
				if (FAILED(hr)) {
					CoUninitialize();
					SAFE_DELETE(common);
					return -1;
				}

				string tmpd = sdrive + sdir + "updater-temp";
				if (_mkdir(tmpd.c_str()) == 0) {
					cout << "_mkdir(" + tmpd + ") completed." << endl;
				}
				else {
					cout << "ERROR: _mkdir(" + tmpd + ") failed." << endl;
					MessageBox(NULL, TEXT("failed create directory."), TEXT("Error"), MB_ICONWARNING);
					SAFE_DELETE(common);
					return -1;
				}

				TCHAR* SourcePath = common->ConvertTCHAR(common->StringToWString(sdrive + sdir + "atractool-rel.zip").c_str());
				TCHAR* ExtPath = common->ConvertTCHAR(common->StringToWString(sdrive + sdir + "updater-temp").c_str());

				if (common->ExtractZip(pShellDisp, SourcePath, ExtPath) == TRUE) {
					cout << "'" + sdrive + sdir + "atractool-rel.zip' to '" + sdrive + sdir + "updater-temp' extracted." << endl;
					CoUninitialize();
					SAFE_FREE(SourcePath);
					SAFE_FREE(ExtPath);
					SAFE_DELETE(common);
				}
				else {
					cout << "ERROR: '" + sdrive + sdir + "atractool-rel.zip' to '" + sdrive + sdir + "updater-temp' extract failed." << endl;
					CoUninitialize();
					SAFE_FREE(SourcePath);
					SAFE_FREE(ExtPath);
					SAFE_DELETE(common);
					return -1;
				}

				if (AppType == "release") {
					cout << "Type: release" << endl;
					vector<tstring> tvector;
					common->CopyDirectory(common->StringToWString(sdrive + sdir + "updater-temp\\release\\res").c_str(), common->StringToWString(AppPath + "\\res").c_str());
					common->CopyDirectoryFiles(common->StringToWString(sdrive + sdir + "updater-temp\\release").c_str(), common->StringToWString(AppPath + "\\").c_str(), tvector);
				}
				else if (AppType == "portable") {
					cout << "Type: portable" << endl;
					vector<tstring> tvector;
					common->CopyDirectory(common->StringToWString(sdrive + sdir + "updater-temp\\release-portable\\res").c_str(), common->StringToWString(AppPath + "\\res").c_str());
					common->CopyDirectoryFiles(common->StringToWString(sdrive + sdir + "updater-temp\\release-portable").c_str(), common->StringToWString(AppPath + "\\").c_str(), tvector);
				}
				else {
					cout << "ERROR: Unknown type." << endl;
					MessageBox(NULL, TEXT("unknown release type."), TEXT("Error"), MB_ICONWARNING);
					SAFE_DELETE(common);
					return -1;
				}

				ofstream ofs;
				string fn = AppPath + "\\updated.dat";
				ofs.open(fn, ios::out);
				ofs << "updated." << endl;
				ofs.close();

				wstring ExectablePath = common->StringToWString(AppPath) + L"\\atractool-reloaded.exe";
				wstring Current = common->StringToWString(AppPath);
				TCHAR* TExectablePath = common->ConvertTCHAR(ExectablePath.c_str());
				TCHAR* TCurrent = common->ConvertTCHAR(Current.c_str());

				ShellExecute(NULL, _T("open"), TExectablePath, NULL, TCurrent, SW_SHOWNORMAL);

				SAFE_FREE(TExectablePath);
				SAFE_FREE(TCurrent);
				SAFE_DELETE(common);

				return 0;
			}
			else {
				MessageBox(NULL, TEXT("Target file not found."), TEXT("Error"), MB_ICONWARNING);
				SAFE_DELETE(common);
				return -1;
			}
		}
		else {
			SAFE_DELETE(common);
			cout << "Updater - Application Auto Update Utility v1.2" << endl;
			cout << "Copyright (C) 2022 XyLe. All Rights Reserved." << endl;
			MessageBox(NULL, TEXT("Update information file not found."), TEXT("Warning"), MB_ICONWARNING);
			return -1;
		}
	}
	else {
		SAFE_DELETE(common);
		return -1;
	}
}

// プログラムの実行: Ctrl + F5 または [デバッグ] > [デバッグなしで開始] メニュー
// プログラムのデバッグ: F5 または [デバッグ] > [デバッグの開始] メニュー

// 作業を開始するためのヒント: 
//    1. ソリューション エクスプローラー ウィンドウを使用してファイルを追加/管理します 
//   2. チーム エクスプローラー ウィンドウを使用してソース管理に接続します
//   3. 出力ウィンドウを使用して、ビルド出力とその他のメッセージを表示します
//   4. エラー一覧ウィンドウを使用してエラーを表示します
//   5. [プロジェクト] > [新しい項目の追加] と移動して新しいコード ファイルを作成するか、[プロジェクト] > [既存の項目の追加] と移動して既存のコード ファイルをプロジェクトに追加します
//   6. 後ほどこのプロジェクトを再び開く場合、[ファイル] > [開く] > [プロジェクト] と移動して .sln ファイルを選択します
