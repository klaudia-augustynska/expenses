using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Entities
{
    public class Message : TableEntity
    {
        /// <summary>
        /// UserShort
        /// </summary>
        public string From { get; set; }
        public string Topic { get; set; }
        public string Content { get; set; }
    }
}
