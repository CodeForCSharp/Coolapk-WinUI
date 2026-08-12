using Windows.ApplicationModel.DataTransfer;

namespace CoolapkUWP.Helpers
{
    internal static class ClipboardHelper
    {
        public static void SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            DataPackage dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
        }
    }
}
