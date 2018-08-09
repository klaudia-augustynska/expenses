using Expenses.Api.Helpers;
using Expenses.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.UnitTests
{
    [TestFixture]
    class AggregationTests
    {
        [Test]
        public void MergeWallets_Test()
        {
            var wallets1 = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "aaa",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.PLN,
                        Amount = 1
                    }
                },
                new Wallet()
                {
                    Name = "bbb",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 2
                    }
                }
            };
            var wallets2 = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "ccc",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.PLN,
                        Amount = 3
                    }
                },
                new Wallet()
                {
                    Name = "ddd",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 4
                    }
                },
                new Wallet()
                {
                    Name = "ddd",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 4
                    }
                }
            };
            var expectedPln = new Money
            {
                Currency = Model.Enums.Currency.PLN,
                Amount = 4
            };
            var expectedEur = new Money
            {
                Currency = Model.Enums.Currency.EUR,
                Amount = 10
            };

            var merged = Aggregation.MergeWallets(wallets1, wallets2);

            Assert.AreEqual(expectedPln.Amount, merged.First(x => x.Currency == Model.Enums.Currency.PLN).Amount);
            Assert.AreEqual(expectedEur.Amount, merged.First(x => x.Currency == Model.Enums.Currency.EUR).Amount);
        }

        [Test]
        public void MergeWallets_OneArgument_AggregatedThatList()
        {
            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "ddd",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 4
                    }
                },
                new Wallet()
                {
                    Name = "ddd",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 4
                    }
                }
            };
            var expectedEur = new Money()
            {
                Currency = Model.Enums.Currency.EUR,
                Amount = 8
            };

            var merged = Aggregation.MergeWallets(wallets);

            Assert.IsNotNull(merged);
            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(expectedEur.Currency, merged[0].Currency);
            Assert.AreEqual(expectedEur.Amount, merged[0].Amount);
        }

        [Test]
        public void MergeWallets_ArgumentNull_EmptyList()
        {
            var merged = Aggregation.MergeWallets(null);

            Assert.IsNotNull(merged);
            Assert.AreEqual(0, merged.Count);
            Assert.AreEqual(typeof(List<Money>), merged.GetType());
        }
    }
}
