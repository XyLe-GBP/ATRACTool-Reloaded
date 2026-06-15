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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ModernTheme.Apply(this);
        }
    }
}
