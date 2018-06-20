using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    /// <summary>
    /// 通过外部解释器执行脚本，使用临时文件传递数据和参数
    /// </summary>
    class ScriptExecutor
    {
        private string _interpreterPath { get; }
        private string _scriptPath { get; }
        private string _paraFilePath { get; }
        private string _resFilePath { get; }
        
        /// <summary>
        /// 初始化前应准备好临时文件
        /// </summary>
        /// <param name="interpreterPath"></param>
        /// <param name="scriptPath"></param>
        /// <param name="paraFilePath"></param>
        /// <param name="resFilePath"></param>
        public ScriptExecutor(string interpreterPath, string scriptPath, string paraFilePath, string resFilePath)
        {
            _interpreterPath = interpreterPath;
            _scriptPath = scriptPath;
            _paraFilePath = paraFilePath;
            _resFilePath = resFilePath;
            if (!File.Exists(scriptPath) || !File.Exists(paraFilePath) || !File.Exists(resFilePath))
                throw new ArgumentException("Some files not found.");
        }

        /// <summary>
        /// 异步执行CMD命令
        /// </summary>
        /// <param name="cmmand"></param>
        /// <returns>按行分割的CMD输出字符串数组</returns>
        private async static Task<string[]> runCmdAsync(string cmmand)
        {
            string result;
            cmmand = cmmand.Trim().TrimEnd('&') + "&exit";
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine(cmmand);

                result = await p.StandardOutput.ReadToEndAsync();
                p.WaitForExit();
            }
            return result.Split('\n');
        }

        public async Task RunScript()
        {
            var cmmand = string.Join(" ", _interpreterPath, _scriptPath, _paraFilePath, _resFilePath);
            string[] cmdret = await runCmdAsync(cmmand);
            if (cmdret == null)
                throw new InvalidOperationException("CMD return null, script Execution failed.");
            if (cmdret[cmdret.Count() - 2] != "OK\r")
                throw new InvalidOperationException($"Script return {cmdret[cmdret.Count() - 2]} not \"OK\".");
        }

        public async Task<IEnumerable<double>> GetDoublesAsync()
        {
            await RunScript();
            var res = File.ReadAllText(_resFilePath); 
            return from entry in res.Split(new string[] { "[", "]", ", " }, StringSplitOptions.RemoveEmptyEntries)
                   select double.Parse(entry);
        }
    }
}
