using ATRACTool_Reloaded.ModernUI;

namespace ATRACTool_Reloaded
{
    public partial class FormMain
    {
        public static DebugHandleSnapshot GetDebugHandleSnapshotSafe()
        {
            var form = FormMainInstance;
            if (form is null
                || form.IsDisposed
                || form.Disposing
                || !form.IsHandleCreated)
            {
                return new DebugHandleSnapshot();
            }

            try
            {
                if (form.InvokeRequired)
                {
                    return (DebugHandleSnapshot)form.Invoke(
                        new Func<DebugHandleSnapshot>(GetDebugHandleSnapshotSafe));
                }

                if (form.IsDisposed || form.Disposing)
                {
                    return new DebugHandleSnapshot();
                }

                return GetDebugHandleSnapshot();
            }
            catch (ObjectDisposedException)
            {
                return new DebugHandleSnapshot();
            }
            catch (InvalidOperationException)
            {
                return new DebugHandleSnapshot();
            }
        }

        public static bool AreDebugFunctionsEnabled => WindowDebug.DebugFunctionsEnabled;

        public static bool TryExecuteDebugFunction(string target, string functionName, out string result)
        {
            DebugInfo($"[DebugFunction] Execute requested. target={target}, function={functionName}");

            result = "Debug functions are disabled.";
            if (!AreDebugFunctionsEnabled)
            {
                DebugInfo("[DebugFunction] Execution skipped. Debug functions are disabled.");
                return false;
            }

            string normalizedTarget = (target ?? string.Empty).Trim().ToLowerInvariant();
            try
            {
                switch (normalizedTarget)
                {
                    case "main":
                    case "formmain":
                        result = InvokeFormDebugFunction(FormMainInstance, functionName);
                        DebugInfo($"[DebugFunction] Result: {result}");
                        return true;
                    case "lpc":
                    case "formlpc":
                        result = InvokeFormDebugFunction(FormLPC.FormLPCInstance, functionName);
                        DebugInfo($"[DebugFunction] Result: {result}");
                        return true;
                    case "progress":
                    case "formprogress":
                        result = InvokeFormDebugFunction(FormProgress.FormProgressInstance, functionName);
                        DebugInfo($"[DebugFunction] Result: {result}");
                        return true;
                    default:
                        result = $"Unknown debug target: {target}";
                        DebugInfo($"[DebugFunction] {result}");
                        return false;
                }
            }
            catch (Exception ex) when (ex is NullReferenceException or ObjectDisposedException or InvalidOperationException)
            {
                result = $"Debug function failed: {ex.Message}";
                DebugInfo($"[DebugFunction] {result}");
                return false;
            }
        }

        private static string InvokeFormDebugFunction(FormMain form, string functionName)
        {
            if (form is null || form.IsDisposed || form.Disposing || !form.IsHandleCreated)
            {
                return "FormMain is not available.";
            }

            return form.InvokeRequired
                ? (string)form.Invoke(new Func<string>(() => form.ExecuteDebugFunction(functionName)))
                : form.ExecuteDebugFunction(functionName);
        }

        private static string InvokeFormDebugFunction(FormLPC form, string functionName)
        {
            if (form is null || form.IsDisposed || form.Disposing || !form.IsHandleCreated)
            {
                return "FormLPC is not available.";
            }

            return form.InvokeRequired
                ? (string)form.Invoke(new Func<string>(() => form.ExecuteDebugFunction(functionName)))
                : form.ExecuteDebugFunction(functionName);
        }

        private static string InvokeFormDebugFunction(FormProgress form, string functionName)
        {
            if (form is null || form.IsDisposed || form.Disposing || !form.IsHandleCreated)
            {
                return "FormProgress is not available.";
            }

            return form.InvokeRequired
                ? (string)form.Invoke(new Func<string>(() => form.ExecuteDebugFunction(functionName)))
                : form.ExecuteDebugFunction(functionName);
        }

        public string ExecuteDebugFunction(string functionName)
        {
            if (!AreDebugFunctionsEnabled)
            {
                return "Debug functions are disabled.";
            }

            string normalized = (functionName ?? "status").Trim().ToLowerInvariant();
            return normalized switch
            {
                "" or "status" => $"FormMain: visible={Visible}, enabled={Enabled}, closing={_isClosing}, process={Common.Generic.ProcessFlag}, files={Common.Generic.OpenFilePaths?.Length ?? 0}",
                "reset-status" => ExecuteDebugResetStatus(),
                _ => $"Unknown FormMain debug function: {functionName}"
            };
        }

        private string ExecuteDebugResetStatus()
        {
            ResetStatus();
            return "FormMain status reset.";
        }

        private bool PlaceDebugWindowBehindMain()
        {
            if (!IsHandleCreated)
            {
                return false;
            }

            var wd = windowDebug;
            if (wd is null)
            {
                return false;
            }

            try
            {
                return wd.PlaceBehindMain(Handle);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            return false;
        }

        private async Task PlaceDebugWindowBehindMainWhenReadyAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                if (IsDisposed || Disposing)
                {
                    return;
                }

                if (PlaceDebugWindowBehindMain())
                {
                    return;
                }

                await Task.Delay(100);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ModernTheme.Apply(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _ = PlaceDebugWindowBehindMainWhenReadyAsync();
        }
    }
}
