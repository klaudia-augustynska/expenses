﻿using Expenses.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expenses.UnitTests
{
    [TestFixture]
    public class HashUtilTests
    {
        [Test]
        public void GenerateSalt_ResultIsBase64String()
        {
            for (int i = 0; i < 1000; ++i)
            {
                var salt = HashUtil.GenerateSalt();
                Assert.DoesNotThrow(() =>
                {
                    var saltBytes = Convert.FromBase64String(salt);
                });
            }
        }

        [Test]
        public void GenerateSalt_WhenInHttpResponseMessage_IsStillBase64String()
        {
            for (int i = 0; i < 10; ++i)
            {
                var httpResponseMessage = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(HashUtil.GenerateSalt())
                };
                var salt = httpResponseMessage.Content.ReadAsStringAsync().Result;
                Assert.DoesNotThrow(() =>
                {
                    var saltBytes = Convert.FromBase64String(salt);
                });
            }
        }
    }
}
