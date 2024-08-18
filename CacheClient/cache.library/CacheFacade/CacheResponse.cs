using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cache.library.CacheFacade
{
    public class CacheResponse<T>
    {
        public string Key { get; set; }
        public string SubKey { get; set; }
        public T Value { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
