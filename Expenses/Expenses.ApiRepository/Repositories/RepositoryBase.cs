using Expenses.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Repositories
{
    abstract class RepositoryBase
    {
        protected readonly string _host, _path, _apiRepositoryName;

        public RepositoryBase(string host, string path, string apiRepositoryName)
        {
            _host = host;
            _path = path;
            _apiRepositoryName = apiRepositoryName;
        }

        private Uri _baseUri;
        /// <summary>
        /// A template for most Uris used in the project
        /// </summary>
        protected Uri BaseUri
        {
            get
            {
                if (_baseUri == null)
                {
                    _baseUri = new Uri(_host).Append(_path, _apiRepositoryName);
                }
                return _baseUri;
            }
        }
    }
}
