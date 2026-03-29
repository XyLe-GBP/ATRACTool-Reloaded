using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ATRACTool_Reloaded.Common;

namespace ATRACTool_Reloaded
{
    /// <summary>
    /// ループポイントに関する「状態の更新」と
    /// FormMain UI の同期をまとめるコントローラ
    /// </summary>
    internal static class LoopPointController
    {
        /// <summary>
        /// 単一／複数ファイル共通のループ開始位置の更新
        /// （Generic.* と FormMain のテキストボックスを更新）
        /// </summary>
        public static void UpdateLoopStart(long startSamples, uint buttonIndex)
        {
            if (FormMain.FormMainInstance is null)
                return;

            var main = FormMain.FormMainInstance;

            // FormMain 側のテキストボックスを更新
            main.textBox_LoopStart.Text = startSamples.ToString();

            if (Generic.IsOpenMulti)
            {
                int idx = (int)buttonIndex - 1;
                if (idx < 0 || idx >= Generic.MultipleLoopStarts.Length)
                    return;

                Generic.MultipleLoopStarts[idx] = (int)startSamples;
                Generic.MultipleFilesLoopOKFlags[idx] =
                    Generic.MultipleLoopEnds[idx] != 0;
            }
            else
            {
                Generic.MultipleLoopStarts[0] = (int)startSamples;
                if (Generic.MultipleLoopEnds[0] != 0)
                {
                    Generic.MultipleFilesLoopOKFlags[0] = true;
                    Generic.LoopStartNG = false;
                }
                else
                {
                    Generic.MultipleFilesLoopOKFlags[0] = false;
                    // テキストボックスが空なら NG フラグ
                    Generic.LoopStartNG = string.IsNullOrWhiteSpace(main.textBox_LoopStart.Text);
                }

                Debug.WriteLine("MultipleFilesLoopOKFlags[]: " + string.Join(", ", Generic.MultipleFilesLoopOKFlags));
                Debug.WriteLine("[Flag] LoopStartNG: " + Generic.LoopStartNG);
            }
        }

        /// <summary>
        /// 単一／複数ファイル共通のループ終了位置の更新
        /// （Generic.* と FormMain のテキストボックスを更新）
        /// </summary>
        public static void UpdateLoopEnd(long endSamples, uint buttonIndex)
        {
            if (FormMain.FormMainInstance is null)
                return;

            var main = FormMain.FormMainInstance;

            // FormMain 側のテキストボックスを更新
            main.textBox_LoopEnd.Text = endSamples.ToString();

            if (Generic.IsOpenMulti)
            {
                int idx = (int)buttonIndex - 1;
                if (idx < 0 || idx >= Generic.MultipleLoopEnds.Length)
                    return;

                Generic.MultipleLoopEnds[idx] = (int)endSamples;
                Generic.MultipleFilesLoopOKFlags[idx] =
                    Generic.MultipleLoopStarts[idx] != 0;
            }
            else
            {
                Generic.MultipleLoopEnds[0] = (int)endSamples;
                if (Generic.MultipleLoopStarts[0] != 0)
                {
                    Generic.MultipleFilesLoopOKFlags[0] = true;
                    Generic.LoopEndNG = false;
                }
                else
                {
                    Generic.MultipleFilesLoopOKFlags[0] = false;
                    Generic.LoopEndNG = string.IsNullOrWhiteSpace(main.textBox_LoopEnd.Text);
                }

                Debug.WriteLine("MultipleFilesLoopOKFlags[]: " + string.Join(", ", Generic.MultipleFilesLoopOKFlags));
                Debug.WriteLine("[Flag] LoopEndNG: " + Generic.LoopEndNG);
            }
        }

        /// <summary>
        /// FormMain 側のループ UI を無効化（LPC 側から呼び出す）
        /// </summary>
        public static void DisableMainLoopUi()
        {
            if (FormMain.FormMainInstance is null)
                return;

            var main = FormMain.FormMainInstance;

            main.groupBox_Loop.Enabled = false;
            main.textBox_LoopStart.Enabled = false;
            main.textBox_LoopEnd.Enabled = false;
            main.label_LoopStart.Enabled = false;
            main.label_LoopEnd.Enabled = false;
            main.label_SSample.Enabled = false;
            main.label_ESample.Enabled = false;
        }

        /// <summary>
        /// FormMain 側のループ UI を有効化（LPC 側から呼び出す）
        /// </summary>
        public static void EnableMainLoopUi()
        {
            if (FormMain.FormMainInstance is null)
                return;

            var main = FormMain.FormMainInstance;

            main.groupBox_Loop.Enabled = true;
            main.textBox_LoopStart.Enabled = true;
            main.textBox_LoopEnd.Enabled = true;
            main.label_LoopStart.Enabled = true;
            main.label_LoopEnd.Enabled = true;
            main.label_SSample.Enabled = true;
            main.label_ESample.Enabled = true;
        }

        private static int GetIndex(uint buttonIndex)
        {
            if (Generic.IsOpenMulti)
            {
                var idx = (int)buttonIndex - 1;
                if (idx < 0) idx = 0;
                if (idx >= Generic.MultipleLoopStarts.Length)
                    idx = Generic.MultipleLoopStarts.Length - 1;
                return idx;
            }

            // 単一ファイル時は常に 0 番目を使う
            return 0;
        }

        /// <summary>
        /// 指定ファイル（ボタン）のループ状態を取得します。
        /// </summary>
        public static (int Start, int End, bool IsLoopOk) GetLoopState(uint buttonIndex)
        {
            int idx = GetIndex(buttonIndex);

            int start = Generic.MultipleLoopStarts[idx];
            int end = Generic.MultipleLoopEnds[idx];
            bool ok = Generic.MultipleFilesLoopOKFlags[idx];

            return (start, end, ok);
        }

        /// <summary>
        /// 指定ファイル（ボタン）のループ情報をリセットします。
        /// </summary>
        public static void ResetLoop(uint buttonIndex)
        {
            int idx = GetIndex(buttonIndex);

            Generic.MultipleLoopStarts[idx] = 0;
            Generic.MultipleLoopEnds[idx] = 0;
            Generic.MultipleFilesLoopOKFlags[idx] = false;

            if (FormMain.FormMainInstance is null)
                return;

            // 単一ファイル時のみ NG フラグとテキストボックスを同期
            if (!Generic.IsOpenMulti)
            {
                var main = FormMain.FormMainInstance;

                main.textBox_LoopStart.Text = string.Empty;
                main.textBox_LoopEnd.Text = string.Empty;

                Generic.LoopStartNG = true;
                Generic.LoopEndNG = true;
            }
        }

        /// <summary>
        /// Generic のループ情報を基に、FormMain 側のテキストボックスと NG フラグを同期します。
        /// </summary>
        public static void SyncMainLoopTextFromGeneric(uint buttonIndex)
        {
            if (FormMain.FormMainInstance is null)
                return;

            var main = FormMain.FormMainInstance;
            int idx = GetIndex(buttonIndex);

            int start = Generic.MultipleLoopStarts[idx];
            int end = Generic.MultipleLoopEnds[idx];

            main.textBox_LoopStart.Text = start == 0 ? string.Empty : start.ToString();
            main.textBox_LoopEnd.Text = end == 0 ? string.Empty : end.ToString();

            // 単一ファイル時だけ NG フラグも更新（従来仕様を維持）
            if (!Generic.IsOpenMulti)
            {
                Generic.LoopStartNG = string.IsNullOrWhiteSpace(main.textBox_LoopStart.Text);
                Generic.LoopEndNG = string.IsNullOrWhiteSpace(main.textBox_LoopEnd.Text);

                Debug.WriteLine("MultipleFilesLoopOKFlags[]: " + string.Join(", ", Generic.MultipleFilesLoopOKFlags));
                Debug.WriteLine("[Flag] LoopStartNG: " + Generic.LoopStartNG);
                Debug.WriteLine("[Flag] LoopEndNG: " + Generic.LoopEndNG);
            }
        }
    }
}
