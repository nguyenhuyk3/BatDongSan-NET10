using System.ComponentModel;

namespace VTTGROUP.Domain.Helpers
{
    public enum VuViec
    {
        [Description("001")]
        Xem,

        [Description("002")]
        Them,

        [Description("003")]
        CapNhat,

        [Description("004")]
        Xoa,

        [Description("005")]
        ChiTiet,

        [Description("006")]
        InDanhSach,

        [Description("007")]
        Duyet,

        [Description("008")]
        QuyenTruyCap,

        [Description("009")]
        DoiMatKhau,

        [Description("010")]
        Import,

        [Description("011")]
        Export,

        [Description("012")]
        DongPhieu,

        [Description("013")]
        ResetSTT
    }
}
