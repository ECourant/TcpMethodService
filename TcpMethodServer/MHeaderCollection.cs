using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodServer
{
    public sealed class MHeaderCollection : IEnumerable<MHeader>
    {
        internal MHeaderCollection()
        {

        }

        private List<MHeader> Headers = new List<MHeader>();
        #region Enumerable Functions
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MHeader> GetEnumerator()
        {
            return this.Headers.GetEnumerator();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Headers.GetEnumerator();
        }
        #endregion

        public void Add(string Name, object Value)
        {
            this.Add(new MHeader(Name, Value));
        }
        public void Add(MHeader Header)
        {
            this.Headers.Add(Header);
        }

        public object this [string Name]
        {
            get
            {
                if (Headers.Select(Header => Header.Name).Contains(Name))
                    return Headers.Where(Header => Header.Name == Name).FirstOrDefault();
                else
                    return null;
            }
            set
            {
                if (Headers.Select(Header => Header.Name).Contains(Name))
                    Headers[Headers.IndexOf(Headers.Where(Header => Header.Name == Name).FirstOrDefault())].Value = value;
                else
                    this.Add(Name, value);
            }
        }
    }
}
