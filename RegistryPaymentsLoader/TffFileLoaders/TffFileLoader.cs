using RegistryPaymentsLoader.TffStrings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    public abstract class TffFileLoader
    {
        public abstract string Version { get; }
        public DateTime? NoticeDate { get; protected set; }
        public virtual List<TffString> Load(Stream dataStream)
        {
            var result = new List<TffString>();
            var streamReader = new StreamReader(dataStream, Encoding.GetEncoding(1251));
            while (!streamReader.EndOfStream)
            {
                var fileString = streamReader.ReadLine();
                if (string.IsNullOrWhiteSpace(fileString)) continue;
                var tffStringParts = fileString.Split("|");
                var tffString = LoadString(tffStringParts);
                if (tffString != null)
                {
                    if (tffString is TffStringBDST)
                    {
                        var prevString = result.Last();
                        if (prevString is TffStringBD)
                        {
                            ((TffStringBD)prevString).CredentialString = (TffStringBDST)tffString;
                        }
                        else
                            throw new BDFormatException(
                                string.Format("Отсутствует строка BD для строки {0}", fileString));
                    } else
                    if (tffString is TffStringZF)
                    {
                        var prevString = result.Last();
                        if (prevString is TffStringZFZF)
                        {
                            ((TffStringZF)tffString).ZfString = (TffStringZFZF)prevString;
                            result.Remove(prevString);
                        }
                        result.Add(tffString);
                    }
                    else
                        result.Add(tffString);
                }
            }
            return result;
        }

        protected abstract TffString LoadString(string[] tffStringParts);
    }
}
