namespace VTTGROUP.Domain.Model
{
    public class VuViecModel
    {
        public string? MaVuViec { get; set; }
        public string? TenVuViec { get; set; }
    }
    public class VuViecOfCongViecModel
    {
        public int? Id { get; set; }
        public string? MaVuViec { get; set; }
        public string? MaCongViec { get; set; }
        public bool? IsChecked { get; set; }
        public bool IsCheckedBool
        {
            get => IsChecked == true;
            set => IsChecked = value;
        }
    }
    public class NhomUserOfCongViecModel
    {
        public int? Id { get; set; }
        public string? MaNhom { get; set; }
        public string? MaCongViec { get; set; }
        public string? MaVuViec { get; set; }
        public bool? IsChecked { get; set; }
        public bool IsCheckedBool
        {
            get => IsChecked == true;
            set => IsChecked = value;
        }
    }
}
