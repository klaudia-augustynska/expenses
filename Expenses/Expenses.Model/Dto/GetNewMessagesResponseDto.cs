using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class GetNewMessagesResponseDto
    {
        public UserShort From { get; set; }
        public string Topic { get; set; }
        public string Content { get; set; }
        public string RowKey { get; set; }
    }
}
