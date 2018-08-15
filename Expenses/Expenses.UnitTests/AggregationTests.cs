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
            var merged = Aggregation.MergeWallets();

            Assert.IsNotNull(merged);
            Assert.AreEqual(0, merged.Count);
            Assert.AreEqual(typeof(List<Money>), merged.GetType());
        }

        [Test]
        public void MergeWallets_InitialMoneySpecified_Aggregates()
        {
            var moneyList = new List<Money>()
            {
                new Money()
                {
                    Currency = Model.Enums.Currency.EUR,
                    Amount = 2
                }
            };
            var walletList = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Gotówka w EUR",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 2
                    }
                }
            };

            Aggregation.MergeWallets(moneyList, walletList);

            Assert.AreEqual(4, moneyList[0].Amount);
        }

        [Test]
        public void ExcludeWallets_Test()
        {
            var merged = new List<Money>()
            {
                new Money()
                {
                    Currency = Model.Enums.Currency.EUR,
                    Amount = 20
                }
            };
            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "ssdsdsa",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 1
                    }
                },
                new Wallet()
                {
                    Name = "poidg",
                    Money = new Money()
                    {
                        Currency = Model.Enums.Currency.EUR,
                        Amount = 2
                    }
                }
            };

            Aggregation.ExcludeWallets(merged, wallets);

            Assert.AreEqual(17, merged[0].Amount);
        }

        [Test]
        public void MergeCategories_JoinsDefaultCategoriesOfType_Calories()
        {
            var klaudiaLogin = "Klaudia";
            var paawelLogin = "Paweł";
            var klaudiaKcal = 1234;
            var pawelKcal = 2345;
            var list1 = new List<Category>()
            {
                new Category()
                {
                    DefaultCategory = Model.Enums.DefaultCategory.NormalFood,
                    Name = "A",
                    Factor = new Dictionary<string, double>()
                    {
                        { klaudiaLogin, 100 }
                    }
                }
            };
            var list2 = new List<Category>()
            {
                new Category()
                {
                    DefaultCategory = Model.Enums.DefaultCategory.NormalFood,
                    Name = "B",
                    Factor = new Dictionary<string, double>()
                    {
                        { paawelLogin, 100 }
                    }
                }
            };
            var members = new List<Member>
            {
                new Member()
                {
                    Login = klaudiaLogin,
                    Tmr = klaudiaKcal
                },
                new Member()
                {
                    Login = paawelLogin,
                    Tmr = pawelKcal
                }
            };

            var oneHundredPercent = members.Select(x => x.Tmr).Sum();

            var expected = new Category()
            {
                DefaultCategory = Model.Enums.DefaultCategory.NormalFood,
                Name = "A",
                Factor = new Dictionary<string, double>()
                    {
                        { klaudiaLogin, (klaudiaKcal*100.0)/oneHundredPercent.Value },
                        { paawelLogin, (pawelKcal*100.0)/oneHundredPercent.Value }
                    }
            };

            var result = Aggregation.MergeCategories(members, list1, list2);

            Assert.AreEqual(
                expected: Math.Round(expected.Factor[klaudiaLogin],2), 
                actual: Math.Round(result[0].Factor[klaudiaLogin], 2));
            Assert.AreEqual(
                expected: Math.Round(expected.Factor[paawelLogin], 2),
                actual: Math.Round(result[0].Factor[paawelLogin], 2));
            Assert.AreEqual(
                expected: "A",
                actual: result[0].Name);
            Assert.AreEqual(expected: 1, actual: result.Count);
        }


        [Test]
        public void MergeCategories_JoinsDefaultCategoriesOfType_EqualDivision()
        {
            var klaudiaLogin = "Klaudia";
            var paawelLogin = "Paweł";
            var list1 = new List<Category>()
            {
                new Category()
                {
                    DefaultCategory = Model.Enums.DefaultCategory.Hygiene,
                }
            };
            var list2 = new List<Category>()
            {
                new Category()
                {
                    DefaultCategory = Model.Enums.DefaultCategory.Hygiene,
                }
            };
            var members = new List<Member>
            {
                new Member()
                {
                    Login = klaudiaLogin
                },
                new Member()
                {
                    Login = paawelLogin
                }
            };

            var expected = new Category()
            {
                DefaultCategory = Model.Enums.DefaultCategory.Hygiene,
                Factor = new Dictionary<string, double>()
                    {
                        { klaudiaLogin,50 },
                        { paawelLogin, 50 }
                    }
            };

            var result = Aggregation.MergeCategories(members, list1, list2);

            Assert.AreEqual(
                expected: Math.Round(expected.Factor[klaudiaLogin], 2),
                actual: Math.Round(result[0].Factor[klaudiaLogin], 2));
            Assert.AreEqual(
                expected: Math.Round(expected.Factor[paawelLogin], 2),
                actual: Math.Round(result[0].Factor[paawelLogin], 2));
        }
    }
}
