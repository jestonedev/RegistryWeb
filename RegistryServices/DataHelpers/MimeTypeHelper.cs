using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.DataHelpers
{
    public static class MimeTypeHelper
    {
        public static string OdsMime { get => "application/vnd.oasis.opendocument.spreadsheet"; }
        public static string OdtMime { get => "application/vnd.oasis.opendocument.text"; }
        public static string XlsxMime { get => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        public static string DocxMime { get => "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; }
        public static string ZipMime { get => "application/zip"; }
        public static string CsvMime { get => "text/csv"; }
        public static string PdfMime { get => "application/pdf"; }
    }
}
