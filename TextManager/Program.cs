////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
///     比較及び分割
///     UNICODE専用
///
///         製造 : Retar.jp   
///         Ver 1.00  2019/10/30
/// </summary>
////////////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextManager
{
    class Program
    {
        /// <summary>
        ///定義
        ///     C++の「#Define」
        /// </summary>
        static class Constants
        {
            public const string sgFileNameDefault = "sg.json";              //設定ファイル
        }

        /// <summary>
        ///設定  
        ///     同一Dirにsg.jsonを入れましょう
        /// </summary>
        public class SG_JSON
        {
            //入力DIR
            public string sgINDIR { get; set; }
            //出力DIR
            public string sgOUTDIR { get; set; }

            //ファイルオリジナル部分サフィックス
            public string sgORGSUFFIX { get; set; }
            //ファイル共通部分サフィックス
            public string sgCOMMONSUFFIX { get; set; }
            //ファイルマージファイル
            public string sgMERGESUFFIX { get; set; }
        }

        /// <summary>
        ///Main
        /// </summary>
        static void Main(string[] args)
        {
            DateTime dtstart = DateTime.Now;

            /// <summary>
            ///SG読み込み
            /// </summary>
            string sgFileName = Constants.sgFileNameDefault;
            if (args.Length > 0) { sgFileName = args[0]; }
            SG_JSON sgjson = ReadJson(sgFileName);

            /// <summary>
            ///ファイル読み込み
            /// </summary>
            List<string> uIds = new List<string>();
            uIds = CONVReadFiles(sgjson);

#if DEBUG
            /// <summary>
            ///Debug
            /// </summary>
            Console.WriteLine("-----------------------------------------------------------------");
            foreach (var item in uIds)
            {
                Console.Write(item + "\t");
            }
            Console.WriteLine("\n");
#endif
            /// <summary>
            ///終了処置
            /// </summary>
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine($"実行時間 : {dtstart}  => {DateTime.Now}");
            Console.WriteLine("\n処理終了 : キー入力");
            Console.ReadKey();
        }

        /// <summary>
        ///ファイル読み込み
        ///<retuen>List<string> mergeList</retuen>
        /// </summary>
        public static List<string> CONVReadFiles(SG_JSON sgjson)
        {
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Conversion : 開始");

            /// <summary>
            ///返却
            /// </summary>
            List<string> mergeList = new List<string>();
            List<string> commonList = new List<string>();

            /// <summary>
            ///設定
            /// </summary>
            var inDir = Directory.GetCurrentDirectory() + "\\" + sgjson.sgINDIR;
            var outDir = Directory.GetCurrentDirectory() + "\\" + sgjson.sgOUTDIR;
            Directory.CreateDirectory(inDir);
            Directory.CreateDirectory(outDir);

            string[] inFiles = System.IO.Directory.GetFiles(inDir, "*", System.IO.SearchOption.AllDirectories);
            if (inFiles.Count() == 0)
            {
                MessageBox.Show("入力ファイルがありません。終了");
                Environment.Exit(0);                    //プログラム終了
            }
            Dictionary<string, string> outFiles = new Dictionary<string, string>();
            foreach (var item in inFiles)
            {
                string front = System.IO.Path.GetFileNameWithoutExtension(item);
                string of = outDir + "\\" + front + sgjson.sgORGSUFFIX;
                outFiles.Add(item, of);
            }

            /// <summary>
            ///COMMON And DIR Setting
            /// </summary>
            string commonFile = outDir + "\\" + sgjson.sgCOMMONSUFFIX;
            string mergeFile = outDir + "\\" + sgjson.sgMERGESUFFIX;

            Console.WriteLine($"入力DIR         : {inDir}");
            foreach (var item in inFiles)
            {
                Console.WriteLine($"入力FILE        : {item}");
            }
            Console.WriteLine($"出力DIR         : {outDir}");
            foreach (var item in outFiles)
            {
                Console.WriteLine($"出力FILE        : {item.Value}");
            }
            Console.WriteLine($"COMMON FILE     : {commonFile}");
            Console.WriteLine($"MERGE FILE      : {mergeFile}");

            /// <summary>
            ///MERGE
            /// </summary>
            foreach (var f in inFiles)
            {
                using (StreamReader reader = new StreamReader(f, Encoding.Unicode))
                {
                    while (reader.EndOfStream == false)
                    {
                        mergeList.Add(reader.ReadLine());
                    }
                }
            }
            List<string> clistbef = new List<string>(mergeList);
            mergeList = (from x in mergeList select x).Distinct().ToList();
            using (StreamWriter writer = new StreamWriter(mergeFile, false, Encoding.Unicode))
            {
                foreach (var item in mergeList)
                {
                    writer.WriteLine(item);
                }
            }

            /// <summary>
            ///COMMON
            /// </summary>
            foreach (var item in mergeList)
            {
                var c = (from x in clistbef select x).Where(y => y == item).Count();
                if (c > 1)
                {
                    commonList.Add(item);
                }
            }
            using (StreamWriter writer = new StreamWriter(commonFile, false, Encoding.Unicode))
            {
                foreach (var item in commonList)
                {
                    writer.WriteLine(item);
                }
            }

            /// <summary>
            ///ORG  入力ファイル独自部分
            /// </summary>
            foreach (var f in outFiles)
            {
                var cl = new List<string>(commonList);
                var orgl = new List<string>();

                using (StreamReader reader = new StreamReader(f.Key, Encoding.Unicode))
                {
                    while (reader.EndOfStream == false)
                    {
                        orgl.Add(reader.ReadLine());
                    }
                }
                foreach (var item in cl)
                {
                    orgl.Remove(item);
                }
                orgl = orgl.Distinct().ToList();    //Bug Fix 2019/11/11
                using (StreamWriter writer = new StreamWriter(f.Value, false, Encoding.Unicode))
                {
                    foreach (var item in orgl)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            Console.WriteLine("Conversion : 完了");
            return mergeList;
        }

        /// <summary>
        ///SG読み込み
        /// </summary>
        public static SG_JSON ReadJson(string sgFileName)
        {
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("SG読み込み : 開始");

            SG_JSON sgjson = new SG_JSON();
            try
            {
                if (System.IO.File.Exists(sgFileName))
                {
                    /// <summary>
                    ///シリアライザ
                    /// <summary>
                    using (var bFs = new StreamReader(sgFileName, Encoding.Unicode))
                    {
                        var data = bFs.ReadToEnd();
                        sgjson = JsonConvert.DeserializeObject<SG_JSON>(data);
                    }
                }
                else
                {
                    /// <summary>
                    ///異常終了
                    /// </summary>
                    MessageBox.Show("'" + sgFileName + "'がありません。終了");
                    Environment.Exit(0);                    //プログラム終了
                }
            }
            catch
            {
                /// <summary>
                ///異常終了
                /// </summary>
                Console.WriteLine("SG読み込み : 失敗 : UNICODE文字コードチェックを！");
                Environment.Exit(0);
                Console.ReadKey();
            }

            /// <summary>
            ///設定
            /// </summary>
            Console.WriteLine(">>> Setting <<<");
            Console.WriteLine($"sgINDIR                           :   {sgjson.sgINDIR}");
            Console.WriteLine($"sgOUTDIR                          :   {sgjson.sgOUTDIR }");
            Console.WriteLine($"sgORGSUFFIX                       :   {sgjson.sgORGSUFFIX}");
            Console.WriteLine($"sgCOMMONSUFFIX                    :   {sgjson.sgCOMMONSUFFIX }");
            Console.WriteLine($"sgMERGESUFFIX                     :   {sgjson.sgMERGESUFFIX }");

            Console.WriteLine("SG読み込み : 完了");
            return sgjson;
        }
    }
}
