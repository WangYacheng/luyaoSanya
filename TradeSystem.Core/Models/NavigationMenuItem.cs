using System;
using System.Collections.Generic;
using System.Text;

namespace TradeSystem.Core.Models
{
    public class NavigationMenuItem
    {
        // 主键
        public int Id { get; set; }

        // 父级菜单 ID (如果为 0 或 null，表示这是一级菜单)
        public int? ParentId { get; set; }

        // 菜单名称 (如: 日排行榜, K线分析)
        public string Title { get; set; } = string.Empty;

        // 对应的图标名称 (WPF-UI 里的 Icon 类型)
        public string Icon { get; set; } = string.Empty;

        // 对应的页面 Tag 或命名空间
        public string TargetPageTag { get; set; } = string.Empty;

        // 排序优先级
        public int Order { get; set; }

        // 是否可见
        public bool IsVisible { get; set; } = true;
    }
}
