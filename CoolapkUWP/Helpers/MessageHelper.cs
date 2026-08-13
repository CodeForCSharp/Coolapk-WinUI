using CoolapkUWP.Pages;
using CommunityToolkit.WinUI;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 应用内消息提示（写入主窗口标题栏），自带排队与去重。
    /// </summary>
    internal static class MessageHelper
    {
        public const int Duration = 3000;

        private static int isShowingMessage;
        public static ConcurrentQueue<string> MessageList { get; } = new ConcurrentQueue<string>();

        public static void ShowMessage(string message)
        {
            MessageList.Enqueue(message);
            if (Interlocked.Exchange(ref isShowingMessage, 1) == 0)
            {
                DispatcherQueue dispatcher = App.MainPage?.DispatcherQueue;
                if (dispatcher != null)
                {
                    _ = dispatcher.EnqueueAsync(ShowMessagesCoreAsync);
                }
                else
                {
                    while (MessageList.TryDequeue(out _)) { }
                    Interlocked.Exchange(ref isShowingMessage, 0);
                }
            }
        }

        private static async Task ShowMessagesCoreAsync()
        {
            try
            {
                while (MessageList.TryDequeue(out string current))
                {
                    MainPage mainPage = App.MainPage;
                    if (mainPage != null && !string.IsNullOrEmpty(current))
                    {
                        string messages = $"[{MessageList.Count + 1}] {current.Replace("\n", " ")}";
                        mainPage.ShowMessage(messages);
                        await Task.Delay(Duration);
                    }
                    if (MessageList.IsEmpty)
                    {
                        mainPage?.ShowMessage();
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref isShowingMessage, 0);
            }

            if (!MessageList.IsEmpty && Interlocked.CompareExchange(ref isShowingMessage, 1, 0) == 0)
            {
                DispatcherQueue dispatcher = App.MainPage?.DispatcherQueue;
                if (dispatcher != null)
                {
                    _ = dispatcher.EnqueueAsync(ShowMessagesCoreAsync);
                }
                else
                {
                    while (MessageList.TryDequeue(out _)) { }
                    Interlocked.Exchange(ref isShowingMessage, 0);
                }
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowMessage("无法连接网络。"); }
            else { ShowMessage($"请检查网络连接。 {e.Message}"); }
        }
    }
}
