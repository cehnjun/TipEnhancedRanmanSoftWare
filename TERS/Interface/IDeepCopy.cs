using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    public interface IDeepCopy<T>
    {
        T DeepCopy();
    }
}
