using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication1;

public class EventModel
{
    public int EventID { get; set; }
    public string TimeStamp { get; set; } = "";
    public string Source { get; set; } = "";
    public string Location { get; set; } = "";
    public int EventTypeID { get; set; }
    public string Result { get; set; } = "";
    public string DeviceID { get; set; } = "";
    public string Details { get; set; } = "";
    public int EmpID { get; set; }
}
