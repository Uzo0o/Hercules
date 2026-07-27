using System.Collections.Generic;

namespace Hercules.Models;

public class VmixInput
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<VmixField> Fields { get; set; } = new();
}