using Expenses.TestApp.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expenses.UnitTests
{
    [TestFixture]
    class ViewModelsTests
    {

        [Test]
        public void PotwierdzExecute_GoodRegex()
        {
            var vm = new WiadomosciVm(null, null);
            var regex = vm.potwierdzRegex;
            var str = "/api/households/accept/klaudia/pawel/7982.05.27_08:18:00";

            Assert.AreEqual("klaudia", regex.Match(str).Groups["from"].Value);
            Assert.AreEqual("pawel", regex.Match(str).Groups["to"].Value);
            Assert.AreEqual("7982.05.27_08:18:00", regex.Match(str).Groups["rowKey"].Value);
        }
    }
}
