﻿using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class FileInfo
    {
        [XmlElement(ElementName = "file_id")]
        public int FileId { get; set; } //Идентификатор файла
        [XmlElement(ElementName = "name", IsNullable = true)]
        public string Name { get; set; } //Название файла
        [XmlElement(ElementName = "extension", IsNullable = true)]
        public string Extension { get; set; } //Расширение файла
        [XmlElement(ElementName = "size")]
        public int Size { get; set; } //Размер файла
        [XmlElement(ElementName = "create_date")]
        public DateTime CreateDate { get; set; } //Дата загрузки файла
    }
}