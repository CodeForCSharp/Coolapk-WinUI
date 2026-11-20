using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Converters;
using CoolapkUWP.Models.Images;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using CommunityToolkit.WinUI.Converters;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// 基于 Cyenoch 的 MyRichTextBlock 控件和 Coolapk UWP 项目的 TextBlockEx 控件重制
    /// </summary>
    public sealed partial class TextBlockEx : UserControl
    {
        public const string AuthorBorder = "<div class=\"author-border\"></div>";
        private static readonly Regex EmojisRegex = new Regex(@"(\[\S*?\]|#\(\S*?\))");
        private static readonly HtmlParser SharedParser = new HtmlParser();
        private static readonly FontSizeToHeightConverter SharedFontSizeToHeightConverter = new FontSizeToHeightConverter();
        private static readonly BoolToVisibilityConverter SharedBoolToVisibilityConverter = new BoolToVisibilityConverter();
        private static readonly SolidColorBrush TopicLinkBrush = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88));
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextBlockEx),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                nameof(MaxLines),
                typeof(int),
                typeof(TextBlockEx),
                null);

        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(
                nameof(TextTrimming),
                typeof(TextTrimming),
                typeof(TextBlockEx),
                new PropertyMetadata(TextTrimming.CharacterEllipsis));

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                nameof(TextWrapping),
                typeof(TextWrapping),
                typeof(TextBlockEx),
                new PropertyMetadata(TextWrapping.Wrap));

        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTextSelectionEnabled),
                typeof(bool),
                typeof(TextBlockEx),
                new PropertyMetadata(false));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public TextTrimming TextTrimming
        {
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public bool IsTextSelectionEnabled
        {
            get => (bool)GetValue(IsTextSelectionEnabledProperty);
            set => SetValue(IsTextSelectionEnabledProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextBlockEx).GetTextBlock();
        }

        public TextBlockEx() => InitializeComponent();

        private void GetTextBlock()
        {
            RichTextBlock.Blocks.Clear();
            if (string.IsNullOrEmpty(Text)) { return; }

            // 回复基本是文字、链接和表情。这种不建 DOM，避免每条回复都在 UI 线程跑一遍 AngleSharp。
            string html = Text.Replace("<!--break-->", string.Empty);
            if (CanRenderLight(html))
            {
                RenderLight(html);
                return;
            }

            var doc = SharedParser.ParseDocument(html);
            Paragraph paragraph = new Paragraph { LineHeight = FontSize + 10 };
            List<ImageModel> imageArrayBuider = new List<ImageModel>();
            void NewLine()
            {
                RichTextBlock.Blocks.Add(paragraph);
                paragraph = new Paragraph { LineHeight = FontSize + 10 };
            }
            void AddText(string item) => paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) });

            void AddEmojiInline(string key, string tooltip)
            {
                InlineUIContainer container = new InlineUIContainer();
                Image image = new Image { Source = EmojiHelper.GetBitmap(key) };
                ToolTipService.SetToolTip(image, new ToolTip { Content = tooltip });
                Viewbox viewbox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                };
                viewbox.SetBinding(WidthProperty, new Binding
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Converter = SharedFontSizeToHeightConverter,
                    Path = new PropertyPath(nameof(FontSize))
                });
                container.Child = viewbox;
                paragraph.Inlines.Add(container);
            }
            INodeList nodes = doc.Body.ChildNodes;
            foreach (INode node in nodes)
            {
                switch (node.NodeType)
                {
                    case NodeType.Text:
                        string[] list = EmojisRegex.Split(node.TextContent);
                        foreach (string item in list)
                        {
                            if (string.IsNullOrEmpty(item)) { continue; }
                            switch (item[0])
                            {
                                case '#':
                                    {
                                        string str = item.Substring(1);
                                        if (EmojiHelper.Emojis.Contains(str))
                                        {
                                            AddEmojiInline(str, item);
                                        }
                                        else { AddText(item); }
                                    }
                                    break;
                                case '[':
                                    {
                                        if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.OldEmojis.Contains(item))
                                        {
                                            AddEmojiInline(item, item);
                                        }
                                        else if (EmojiHelper.Emojis.Contains(item))
                                        {
                                            AddEmojiInline(item, item);
                                        }
                                        else { AddText(item); }
                                    }
                                    break;
                                default:
                                    AddText(item);
                                    break;
                            }
                        }
                        break;
                    case NodeType.Element:
                        IElement element = (IElement)node;
                        string content = element.TextContent;
                        switch (element.LocalName)
                        {
                            case "a":
                                string tag = element.GetAttribute("t") ?? string.Empty;
                                string href = element.GetAttribute("href") ?? string.Empty;
                                string type = element.GetAttribute("type") ?? string.Empty;
                                string cls = element.GetAttribute("class") ?? string.Empty;
                                Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None, Foreground = TopicLinkBrush };
                                if (!string.IsNullOrEmpty(href))
                                {
                                    hyperlink.Click += (sender, e) => _ = this.OpenLinkAsync(href);
                                    ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                                }
                                if (!content.StartsWith("@") && !content.StartsWith("#") && !(type == "user-detail"))
                                {
                                    Run run2 = new Run { Text = "\uE167", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                                    hyperlink.Inlines.Add(run2);
                                }
                                else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                                {
                                    content = "查看图片";
                                    Run run2 = new Run { Text = "\uE158", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                                    hyperlink.Inlines.Add(run2);
                                }
                                Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
                                hyperlink.Inlines.Add(run3);
                                paragraph.Inlines.Add(hyperlink);
                                break;

                            case "img":
                                string alt = element.GetAttribute("alt") ?? string.Empty;
                                string src = element.GetAttribute("src") ?? string.Empty;
                                int width = Convert.ToInt32((element.GetAttribute("width") ?? "-1").Replace("\"", string.Empty));
                                int height = Convert.ToInt32((element.GetAttribute("height") ?? "-1").Replace("\"", string.Empty));

                                ImageModel imageModel = new ImageModel(src, SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture) ? ImageType.OriginImage : ImageType.SmallImage);
                                ImageEx image = new ImageEx
                                {
                                    DecodePixelWidth = src.Contains("emoticons") ? 0 : GetInlineImageDecodeWidth(width),
                                    Stretch = Stretch.Uniform,
                                    Model = imageModel
                                };
                                InlineUIContainer container = new InlineUIContainer();

                                if (!string.IsNullOrEmpty(alt))
                                {
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = alt });
                                }

                                if (src.Contains("emoticons"))
                                {
                                    Viewbox viewbox = new Viewbox
                                    {
                                        Child = image,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                                    };
                                    viewbox.SetBinding(WidthProperty, new Binding
                                    {
                                        Source = this,
                                        Mode = BindingMode.OneWay,
                                        Converter = new FontSizeToHeightConverter(),
                                        Path = new PropertyPath(nameof(FontSize))
                                    });
                                    container.Child = viewbox;
                                    paragraph.Inlines.Add(container);
                                }
                                else
                                {
                                    NewLine();
                                    imageArrayBuider.Add(imageModel);

                                    Grid Grid = new Grid
                                    {
                                        Margin = new Thickness(0, 0, 0, 8),
                                        CornerRadius = new CornerRadius(8)
                                    };

                                    Grid IsGIFPanel = new Grid
                                    {
                                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left
                                    };

                                    Grid PicSizePanel = new Grid
                                    {
                                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right
                                    };

                                    Border GIFBorder = new Border
                                    {
                                        Child = new TextBlock
                                        {
                                            Text = _loader.GetString("GIF")
                                        },
                                        Padding = new Thickness(4, 2, 4, 2),
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88)),
                                        CornerRadius = new CornerRadius(8, 0, 8, 0)
                                    };
                                    GIFBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Mode = BindingMode.OneWay,
                                        Converter = SharedBoolToVisibilityConverter,
                                        Path = new PropertyPath(nameof(imageModel.IsGif))
                                    });

                                    IsGIFPanel.Children.Add(GIFBorder);

                                    Border WidePicBorder = new Border
                                    {
                                        Child = new TextBlock
                                        {
                                            Text = _loader.GetString("WidePicText")
                                        },
                                        Padding = new Thickness(4, 2, 4, 2),
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88)),
                                        CornerRadius = new CornerRadius(0, 8, 0, 8)
                                    };
                                    WidePicBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = image,
                                        Mode = BindingMode.OneWay,
                                        Converter = SharedBoolToVisibilityConverter,
                                        Path = new PropertyPath(nameof(ImageEx.IsWidePic))
                                    });

                                    Border LongPicTextBorder = new Border
                                    {
                                        Child = new TextBlock
                                        {
                                            Text = _loader.GetString("LongPicText")
                                        },
                                        Padding = new Thickness(4, 2, 4, 2),
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88)),
                                        CornerRadius = new CornerRadius(0, 8, 0, 8)
                                    };
                                    LongPicTextBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = image,
                                        Mode = BindingMode.OneWay,
                                        Converter = SharedBoolToVisibilityConverter,
                                        Path = new PropertyPath(nameof(ImageEx.IsLongPic))
                                    });

                                    PicSizePanel.Children.Add(WidePicBorder);
                                    PicSizePanel.Children.Add(LongPicTextBorder);

                                    Viewbox viewbox = new Viewbox
                                    {
                                        Child = image,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                                    };
                                    if (width != -1) { viewbox.MaxWidth = width; }
                                    if (height != -1) { viewbox.MaxHeight = height; }

                                    Grid.Children.Add(viewbox);
                                    Grid.Children.Add(PicSizePanel);
                                    Grid.Tapped += (sender, args) => _ = this.ShowImageAsync(imageModel);

                                    container.Child = Grid;

                                    Paragraph paragraph1 = new Paragraph { TextAlignment = TextAlignment.Center };
                                    paragraph1.Inlines.Add(container);
                                    RichTextBlock.Blocks.Add(paragraph1);

                                    if (!string.IsNullOrEmpty(alt))
                                    {
                                        Paragraph paragraph2 = new Paragraph
                                        {
                                            LineHeight = FontSize + 10,
                                            TextAlignment = TextAlignment.Center,
                                            Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                                        };
                                        Run run = new Run { Text = WebUtility.HtmlDecode(alt) };
                                        paragraph2.Inlines.Add(run);
                                        RichTextBlock.Blocks.Add(paragraph2);
                                    }
                                }
                                break;

                            case "div":
                                string divClass = element.GetAttribute("class") ?? string.Empty;
                                if (divClass.IndexOf("rating-tag", StringComparison.Ordinal) >= 0)
                                {
                                    if (paragraph.Inlines.Count > 0) { NewLine(); }
                                    AddRatingTag(paragraph, content);
                                }
                                else
                                {
                                    container = new InlineUIContainer();
                                    Border border = new Border
                                    {
                                        Margin = new Thickness(4, 0, 4, -4),
                                        CornerRadius = new CornerRadius(4),
                                        BorderThickness = new Thickness(1),
                                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                                        BorderBrush = TopicLinkBrush,
                                    };
                                    TextBlock textBlock = new TextBlock
                                    {
                                        FontSize = 12,
                                        Margin = new Thickness(1),
                                        IsTextSelectionEnabled = true,
                                        Text = _loader.GetString("FeedAuthorText"),
                                        Foreground = TopicLinkBrush,
                                    };

                                    border.Child = textBlock;
                                    container.Child = border;
                                    paragraph.Inlines.Add(container);
                                }
                                break;

                            default: break;
                        }
                        break;
                }
            }

            List<ImageModel> array = imageArrayBuider;
            foreach (ImageModel item in array)
            {
                item.ContextArray = array;
            }

            RichTextBlock.Blocks.Add(paragraph);
        }

        /// <summary>
        /// 只有 a、换行和作者标记时返回 true。出现图片或其他标签就交给 AngleSharp。
        /// </summary>
        private static bool CanRenderLight(string html)
        {
            int i = 0;
            while (i < html.Length)
            {
                int lt = html.IndexOf('<', i);
                if (lt < 0) { return true; }
                if (lt + 1 >= html.Length || html[lt + 1] == '!' || html[lt + 1] == '?') { return false; }

                int gt = html.IndexOf('>', lt);
                if (gt < 0) { return false; }

                string raw = html.Substring(lt + 1, gt - lt - 1).Trim();
                if (raw.EndsWith("/")) { raw = raw.Substring(0, raw.Length - 1).Trim(); }
                bool closing = raw.StartsWith("/");
                if (closing) { raw = raw.Substring(1).Trim(); }

                int nameLength = 0;
                while (nameLength < raw.Length && !char.IsWhiteSpace(raw[nameLength])) { nameLength++; }
                if (nameLength == 0) { return false; }
                string name = raw.Substring(0, nameLength).ToLowerInvariant();

                if (name == "a" && !closing)
                {
                    int close = html.IndexOf("</a>", gt + 1, StringComparison.OrdinalIgnoreCase);
                    if (close < 0 || html.IndexOf('<', gt + 1, close - gt - 1) >= 0) { return false; }
                    i = close + 4;
                    continue;
                }
                if (name == "div" && !closing)
                {
                    int close = html.IndexOf("</div>", gt + 1, StringComparison.OrdinalIgnoreCase);
                    if (close < 0) { return false; }
                    bool ratingTag = raw.IndexOf("rating-tag", StringComparison.Ordinal) >= 0;
                    bool authorTag = raw.IndexOf("author-border", StringComparison.Ordinal) >= 0;
                    if (!ratingTag && !authorTag) { return false; }
                    string inner = html.Substring(gt + 1, close - gt - 1);
                    if (inner.IndexOf('<') >= 0) { return false; }
                    if (authorTag && inner.Trim().Length > 0) { return false; }
                    i = close + 6;
                    continue;
                }
                if (name != "br" && name != "a" && name != "div") { return false; }
                i = gt + 1;
            }
            return true;
        }

        private void RenderLight(string html)
        {
            Paragraph paragraph = new Paragraph { LineHeight = FontSize + 10 };
            int i = 0;
            while (i < html.Length)
            {
                int lt = html.IndexOf('<', i);
                if (lt < 0)
                {
                    AppendEmojiText(paragraph, html.Substring(i));
                    break;
                }
                if (lt > i) { AppendEmojiText(paragraph, html.Substring(i, lt - i)); }

                int gt = html.IndexOf('>', lt);
                string raw = html.Substring(lt + 1, gt - lt - 1).Trim();
                if (raw.EndsWith("/")) { raw = raw.Substring(0, raw.Length - 1).Trim(); }
                bool closing = raw.StartsWith("/");
                string body = closing ? raw.Substring(1).Trim() : raw;
                int nameLength = 0;
                while (nameLength < body.Length && !char.IsWhiteSpace(body[nameLength])) { nameLength++; }
                string name = body.Substring(0, nameLength).ToLowerInvariant();

                if (name == "br")
                {
                    RichTextBlock.Blocks.Add(paragraph);
                    paragraph = new Paragraph { LineHeight = FontSize + 10 };
                }
                else if (name == "div" && !closing)
                {
                    int close = html.IndexOf("</div>", gt + 1, StringComparison.OrdinalIgnoreCase);
                    if (body.IndexOf("rating-tag", StringComparison.Ordinal) >= 0)
                    {
                        if (paragraph.Inlines.Count > 0)
                        {
                            RichTextBlock.Blocks.Add(paragraph);
                            paragraph = new Paragraph { LineHeight = FontSize + 10 };
                        }
                        AddRatingTag(paragraph, html.Substring(gt + 1, close - gt - 1));
                    }
                    else
                    {
                        AddAuthorBadge(paragraph);
                    }
                    i = close + 6;
                    continue;
                }
                else if (name == "a" && !closing)
                {
                    int close = html.IndexOf("</a>", gt + 1, StringComparison.OrdinalIgnoreCase);
                    string content = html.Substring(gt + 1, close - gt - 1);
                    AddHyperlink(paragraph, ReadAttr(body, "href"), ReadAttr(body, "type"), content);
                    i = close + 4;
                    continue;
                }
                i = gt + 1;
            }
            RichTextBlock.Blocks.Add(paragraph);
        }

        private void AppendEmojiText(Paragraph paragraph, string text)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            foreach (string item in EmojisRegex.Split(text))
            {
                if (string.IsNullOrEmpty(item)) { continue; }
                switch (item[0])
                {
                    case '#':
                        string key = item.Substring(1);
                        if (EmojiHelper.Emojis.Contains(key)) { AddEmojiInline(paragraph, key, item); }
                        else { paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) }); }
                        break;
                    case '[':
                        if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.OldEmojis.Contains(item)) { AddEmojiInline(paragraph, item, item); }
                        else if (EmojiHelper.Emojis.Contains(item)) { AddEmojiInline(paragraph, item, item); }
                        else { paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) }); }
                        break;
                    default:
                        paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) });
                        break;
                }
            }
        }

        private void AddEmojiInline(Paragraph paragraph, string key, string tooltip)
        {
            Image image = new Image { Source = EmojiHelper.GetBitmap(key) };
            ToolTipService.SetToolTip(image, new ToolTip { Content = tooltip });
            Viewbox viewbox = new Viewbox
            {
                Child = image,
                Margin = new Thickness(0, 0, 0, -4),
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };
            viewbox.SetBinding(WidthProperty, new Binding
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = SharedFontSizeToHeightConverter,
                Path = new PropertyPath(nameof(FontSize))
            });
            paragraph.Inlines.Add(new InlineUIContainer { Child = viewbox });
        }

        private void AddHyperlink(Paragraph paragraph, string href, string type, string content)
        {
            content ??= string.Empty;
            Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None, Foreground = TopicLinkBrush };
            if (!string.IsNullOrEmpty(href))
            {
                string target = href;
                hyperlink.Click += (sender, e) => _ = this.OpenLinkAsync(target);
                ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = target });
            }
            if (!content.StartsWith("@") && !content.StartsWith("#") && type != "user-detail")
            {
                hyperlink.Inlines.Add(new Run { Text = "\uE167", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] });
            }
            else if (content == "\u67E5\u770B\u56FE\u7247" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
            {
                hyperlink.Inlines.Add(new Run { Text = "\uE158", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] });
            }
            hyperlink.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(content) });
            paragraph.Inlines.Add(hyperlink);
        }

        private static void AddRatingTag(Paragraph paragraph, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) { return; }
            Border border = new Border
            {
                Margin = new Thickness(0, 8, 8, -2),
                Padding = new Thickness(6, 1, 6, 1),
                CornerRadius = new CornerRadius(4),
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Background = TopicLinkBrush,
                Child = new TextBlock
                {
                    FontSize = 12,
                    Text = WebUtility.HtmlDecode(title),
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                }
            };
            paragraph.Inlines.Add(new InlineUIContainer { Child = border });
        }

        private void AddAuthorBadge(Paragraph paragraph)
        {
            Border border = new Border
            {
                Margin = new Thickness(4, 0, 4, -4),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                BorderBrush = TopicLinkBrush,
                Child = new TextBlock
                {
                    FontSize = 12,
                    Margin = new Thickness(1),
                    IsTextSelectionEnabled = true,
                    Text = _loader.GetString("FeedAuthorText"),
                    Foreground = TopicLinkBrush,
                }
            };
            paragraph.Inlines.Add(new InlineUIContainer { Child = border });
        }

        private static string ReadAttr(string tag, string name)
        {
            int i = 0;
            while (i < tag.Length && !char.IsWhiteSpace(tag[i])) { i++; }
            while (i < tag.Length)
            {
                while (i < tag.Length && char.IsWhiteSpace(tag[i])) { i++; }
                int start = i;
                while (i < tag.Length && tag[i] != '=' && !char.IsWhiteSpace(tag[i])) { i++; }
                if (i == start) { break; }
                string key = tag.Substring(start, i - start);
                while (i < tag.Length && char.IsWhiteSpace(tag[i])) { i++; }
                if (i >= tag.Length || tag[i] != '=') { continue; }
                i++;
                while (i < tag.Length && char.IsWhiteSpace(tag[i])) { i++; }
                if (i >= tag.Length) { break; }

                string value;
                if (tag[i] == '"' || tag[i] == '\'')
                {
                    char quote = tag[i++];
                    int end = tag.IndexOf(quote, i);
                    if (end < 0) { return string.Empty; }
                    value = tag.Substring(i, end - i);
                    i = end + 1;
                }
                else
                {
                    int end = i;
                    while (end < tag.Length && !char.IsWhiteSpace(tag[end])) { end++; }
                    value = tag.Substring(i, end - i);
                    i = end;
                }
                if (key.Equals(name, StringComparison.OrdinalIgnoreCase)) { return WebUtility.HtmlDecode(value); }
            }
            return string.Empty;
        }

        /// <summary>
        /// 图文内嵌图片按可用宽度解码，避免默认按原图尺寸解码占用大量内存。
        /// </summary>
        private int GetInlineImageDecodeWidth(int width)
        {
            double available = ActualWidth > 0 ? ActualWidth : WindowContext.Bounds.Width;
            if (width > 0) { available = Math.Min(available, width); }
            if (!double.IsFinite(available) || available <= 0) { return 0; }

            double scale = (XamlRoot ?? App.MainWindow?.Content?.XamlRoot)?.RasterizationScale ?? 1.0;
            return (int)Math.Ceiling(available * scale);
        }
    }
}
