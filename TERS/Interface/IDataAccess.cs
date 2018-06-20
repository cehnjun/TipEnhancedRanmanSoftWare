using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TERS
{
    public interface IDataAccess
    {
        /// <summary>
        /// 保存IData实例
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        void Save(IData data, string id = null);

        /// <summary>
        /// 在DataAccess中查找并返回一个IData实例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IData RetrieveById(string id);

        /// <summary>
        /// 返回DataAccess中所有IData id
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> RetrieveAllId();

        /// <summary>
        /// 删除DataAccess中Id对应的IData数据
        /// </summary>
        /// <param name="id"></param>
        void DeleteById(string id);

        /// <summary>
        /// 更新DataAccess中Id对应的IData数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        void UpdateById(IData data, string id);

        string GetName();
    }
}
