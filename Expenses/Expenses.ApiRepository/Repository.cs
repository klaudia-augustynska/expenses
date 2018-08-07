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
        }

        public IUsers UsersRepository { get; }
        public IHouseholds HouseholdsRepository { get; }
    }
}
