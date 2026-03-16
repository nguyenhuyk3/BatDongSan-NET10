namespace VTTGROUP.Domain.Model
{
    public class SelectItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
    public class FixColumnConfig
    {
        public int? left { get; set; }
        public int? right { get; set; }
    }
    public class InfoItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public bool? LargeWidth { get; set; } = false;

        // optional
        public bool IsClickable { get; set; } = false;
        public string? Key { get; set; } = null;        // ví dụ: "DON_GIA_TB"
        public string? Tooltip { get; set; } = null;

        public bool HasAction { get; set; } = false;
        public string? ActionIcon { get; set; } = "mdi mdi-pencil";
        public string? ActionTooltip { get; set; } = "Cập nhật";
    }
}
