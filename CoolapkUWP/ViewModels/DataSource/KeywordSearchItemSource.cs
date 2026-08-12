using System.ComponentModel;

namespace CoolapkUWP.ViewModels.DataSource
{
    /// <summary>
    /// 支持按关键词动态重建 Provider 的搜索型数据源基类。
    /// </summary>
    public abstract partial class KeywordSearchItemSource : EntityItemSource, INotifyPropertyChanged
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider();
                }
            }
        }

        protected KeywordSearchItemSource(string keyword = " ")
        {
            Keyword = keyword;
        }

        protected abstract void UpdateProvider();
    }
}
