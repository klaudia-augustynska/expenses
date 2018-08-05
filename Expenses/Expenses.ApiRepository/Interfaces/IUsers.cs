﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Interfaces
{
    public interface IUsers
    {
        /// <summary>
        /// HttpStatusCode.Created jeśli sukces
        /// </summary>
        /// <param name="login"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="salt"></param>
        /// <returns>HttpStatusCode.Created jeśli sukces</returns>
        Task<HttpResponseMessage> Add(string login, string hashedPassword, string salt);

        /// <summary>
        /// HttpStatusCode.OK jeśli sukces. Zwraca wówczas klucz potrzebny do uzyskania dostępu do funkcji po zalogowaniu
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>HttpStatusCode.OK jeśli sukces</returns>
        Task<HttpResponseMessage> LogIn(string login, string hashedPassword);

        /// <summary>
        /// Dla poprawnego requestu HttpStatusCode.OK, ale jeśli nie ma loginu to zostanie podana inna sól dla zmyły.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> GetSalt(string login);

        /// <summary>
        /// OK jeśli zadziałało
        /// </summary>
        /// <param name="login"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> Delete(string login, string key);
    }
}
