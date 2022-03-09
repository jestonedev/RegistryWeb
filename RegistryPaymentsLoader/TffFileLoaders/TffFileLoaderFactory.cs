using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    public static class TffFileLoaderFactory
    {
        public static TffFileLoader CreateFileLoader(Stream dataStream)
        {
            var streamReader = new StreamReader(dataStream, Encoding.GetEncoding(1251));
            if (streamReader.EndOfStream) return null;
            var str = streamReader.ReadLine();
            if (string.IsNullOrWhiteSpace(str)) return null;
            var strParts = str.Split("|");
            if (strParts.Length != 6 || strParts[0] != "FK") return null;
            var fileVersion = strParts[1];
            if (string.IsNullOrWhiteSpace(fileVersion) ||
                !(new string[] { "TXBD", "TXZF", "TXVT" })
                    .Contains(fileVersion.Substring(0, 4))) return null;
            switch (fileVersion)
            {
                case "TXBD210101":
                    return new TXBD210101FileLoader();
                case "TXBD220401":
                    return new TXBD220401FileLoader();
                case "TXZF210101":
                    return new TXZF210101FileLoader();
                case "TXVT170101":
                    return new TXVT170101FileLoader();
            }
            throw new ApplicationException("Неподдерживаемая версия формата файла "+fileVersion);
        }
    }
}
