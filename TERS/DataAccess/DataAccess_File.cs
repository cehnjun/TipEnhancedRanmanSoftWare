using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TERS
{
    using static StaticUtils;
    /// <summary>
    /// 扩展名1dat,2dat,mdat => Data_1D,Data_2D,Data_Map
    /// id是含扩展名的文件名
    /// </summary>
    public class DataAccess_File : IDataAccess
    {
        private readonly string _rootDir;

        private static readonly List<string> _extensions = new List<string> { ".1dat", ".2dat", ".mdat" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id_fileName">带扩展名的文件名，例如1.1dat, 必须有合法扩展名</param>
        /// <returns>返回无冲突的文件完整路径</returns>
        private string genPathById(string id_fileName)
        {
            //if (!isValidFileName(id_fileName))
            //    throw new ArgumentException($"getPathById => Invalid filename:{id_fileName}.");
            return GetNewPathForDupes(Path.Combine(_rootDir, id_fileName));
        }

        /// <summary>
        /// 根据扩展名自动生成无冲突文件路径
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private string genPathByExtension(string extension)
            => genPathById(DateTime.Now.ToString("yyyy-MM-dd") + extension);

        /// <summary>
        /// 判断文件名合法性
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool isValidFileName(string fileName)
            => fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1
            && _extensions.Contains(Path.GetExtension(fileName))
            && Path.GetFileNameWithoutExtension(fileName) != "";

        private bool isValidFileName(string fileName, IData data)
            => isValidFileName(fileName) && data.GetExtension() == Path.GetExtension(fileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IData retrieveByPath(string path)
            => (IData)DecompressFromBytes(File.ReadAllBytes(path));

        public DataAccess_File(string rootDir)
        {
            _rootDir = rootDir;
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);
        }

        public void DeleteById(string id_fileName)
        {
            File.Delete(Path.Combine(_rootDir, id_fileName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IData RetrieveById(string id_fileName)
        {
            if (!isValidFileName(id_fileName))
                throw new ArgumentException($"RetrieveById => Invalid filename:{id_fileName}.");;
            return retrieveByPath(genPathById(id_fileName));
        }

        public void Save(IData data, string id_fileName = null)
        {
            string path = "";
            if (id_fileName == null)
                path = genPathByExtension(data.GetExtension());
            else
            {
                if (!isValidFileName(id_fileName, data))
                    throw new ArgumentException($"Save => Invalid filename:{id_fileName}.");
                else
                    path = genPathById(id_fileName);
            }
            File.WriteAllBytes(path, data.CompressToBytes());
        }

        public void UpdateById(IData data, string id_fileName)
        {
            if (!isValidFileName(id_fileName, data))
                throw new ArgumentException($"UpdateById => Invalid filename:{id_fileName}.");
            var path = Path.Combine(_rootDir, id_fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException();
            File.WriteAllBytes(path, data.CompressToBytes());
        }

        public IEnumerable<string> RetrieveAllId()
            => from path in Directory.GetFiles(_rootDir)
               where _extensions.Contains(Path.GetExtension(path))
               select Path.GetFileName(path);

        public string GetName()
        {
            return $"DataAccess_File:{_rootDir}";
        }
    }
}
