using Expenses.ApiRepository.Interfaces;
using Expenses.ApiRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository
{
    public class Repository
    {
        private const string Path = "api";

        public Repository(string host)
        {
            UsersRepository = new Users(host, Path);
            HouseholdsRepository = new Households(host, Path);
            MessagesRepository = new Messages(host, Path);
            CategoriesRepository = new Categories(host, Path);
            CashFlowsRepository = new CashFlows(host, Path);
        }

        public IUsers UsersRepository { get; }
        public IHouseholds HouseholdsRepository { get; }
        public IMessages MessagesRepository { get; }
        public ICategories CategoriesRepository { get; }
        public ICashFlows CashFlowsRepository { get; }
    }
}
