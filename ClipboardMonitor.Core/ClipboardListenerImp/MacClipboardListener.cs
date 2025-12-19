using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class MacClipboardListener : ClipboardListenerBase, IMacClipboardListener
    {
        public MacClipboardListener() 
        {
            // Not implented yet
        }

        protected override void SetCallbacksNoData(bool unset = false)
        {
            throw new NotImplementedException();
        }

        protected override void SetCallbacksWithData(bool unset = false)
        {
            throw new NotImplementedException();
        }

        protected override void StartListener()
        {
            throw new NotImplementedException();
        }

        protected override void StopListener()
        {
            throw new NotImplementedException();
        }
    }
}
