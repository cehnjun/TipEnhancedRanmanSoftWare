using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    public interface IData
    {
        void IEnumsToList();
        string ToString();
        string GetExtension();
    }
}
