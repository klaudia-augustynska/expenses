using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class GetMembersResponseDto
    {
        public List<MemberDto> Members { get; set; }

        public class MemberDto
        {
            public string Name { get; set; }
            public List<Money> WalletSummary { get; set; }
        }
    }
}
