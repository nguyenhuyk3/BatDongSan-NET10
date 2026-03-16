using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtSendEmailAttachment
{
    public int Id { get; set; }

    public string EmailId { get; set; } = null!;

    public string? FileName { get; set; }

    public byte[]? FileBytes { get; set; }
}
