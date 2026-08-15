using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.FeedPages;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 话题页：左侧话题分类列，右侧展示所选分类的话题列表。
    /// </summary>
    public sealed partial class TopicColumnsPage : Page
    {
        private readonly List<(string Title, string Url)> columns = new List<(string Title, string Url)>();

        public TopicColumnsPage()
        {
            InitializeComponent();
            _ = LoadColumnsAsync();
        }

        private async Task LoadColumnsAsync()
        {
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(
                UriHelper.GetUri(UriType.GetIndexPage, "/page/dataList?url=V11_VERTICAL_TOPIC", "&", 1), true);
            if (!isSucceed || result is not JsonArray array) { return; }

            foreach (JsonNode node in array)
            {
                JsonObject obj = node.AsObject();
                if ((string)obj["entityTemplate"] != "verticalColumnsFullPageCard") { continue; }
                if (obj["entities"] is not JsonArray entities) { continue; }

                foreach (JsonNode entity in entities)
                {
                    JsonObject col = entity.AsObject();
                    string title = (string)col["title"];
                    string url = (string)col["url"];
                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
                    {
                        columns.Add((title, url));
                    }
                }
            }

            ColumnList.ItemsSource = columns.Select(c => c.Title).ToList();
            if (columns.Count > 0)
            {
                ColumnList.SelectedIndex = 0;
            }
        }

        private void ColumnList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ColumnList.SelectedIndex;
            if (index < 0 || index >= columns.Count) { return; }
            _ = ContentFrame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(columns[index].Url));
        }
    }
}
