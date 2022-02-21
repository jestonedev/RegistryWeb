using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum UnitOfMeasureEnum
    {
        [XmlEnum(Name = "1")]
        id1 = 1, //кв.м

        [XmlEnum(Name = "2")]
        id2, //пог.м

        [XmlEnum(Name = "3")]
        id3, //шт.

        [XmlEnum(Name = "4")]
        id4, //куб.м

        [XmlEnum(Name = "5")]
        id5, //Гкал

        [XmlEnum(Name = "6")]
        id6, //Гкал/кв.м

        [XmlEnum(Name = "7")]
        id7, //Гкал/час

        [XmlEnum(Name = "8")]
        id8, //Гкал*час/кв.м

        [XmlEnum(Name = "9")]
        id9, //Гкал/год

        [XmlEnum(Name = "10")]
        id10, //чел.

        [XmlEnum(Name = "11")]
        id11, //ед.

        [XmlEnum(Name = "12")]
        id12, //руб.

        [XmlEnum(Name = "13")]
        id13, //%

        [XmlEnum(Name = "14")]
        id14, //°С*сут

        [XmlEnum(Name = "15")]
        id15, //км

        [XmlEnum(Name = "16")]
        id16, //куб.м/сут.

        [XmlEnum(Name = "17")]
        id17, //куб.м/квартира

        [XmlEnum(Name = "18")]
        id18, //куб.м/чел.в мес.

        [XmlEnum(Name = "19")]
        id19, //Вт/куб.м

        [XmlEnum(Name = "20")]
        id20, //кВт

        [XmlEnum(Name = "21")]
        id21, //кВА

        [XmlEnum(Name = "22")]
        id22, //Вт/(куб.м*°С)

        [XmlEnum(Name = "23")]
        id23, //час

        [XmlEnum(Name = "24")]
        id24, //дн.

        [XmlEnum(Name = "25")]
        id25, //тыс.руб.

        [XmlEnum(Name = "26")]
        id26, //м

        [XmlEnum(Name = "27")]
        id27, //кг

        [XmlEnum(Name = "28")]
        id28, //кг/куб.м

        [XmlEnum(Name = "29")]
        id29, //мВт

        [XmlEnum(Name = "30")]
        id30, //кВт/куб.м

        [XmlEnum(Name = "31")]
        id31, //кВт/ч

        [XmlEnum(Name = "32")]
        id32, //кВт*ч

        [XmlEnum(Name = "33")]
        id33, //руб/куб.м

        [XmlEnum(Name = "34")]
        id34, //куб.м/кв.м

        [XmlEnum(Name = "35")]
        id35, //кВт.ч/кв.м

        [XmlEnum(Name = "36")]
        id36, //руб./Гкал

        [XmlEnum(Name = "37")]
        id37, //руб./кВт.ч

        [XmlEnum(Name = "38")]
        id38, //Гкал/чел.

        [XmlEnum(Name = "39")]
        id39, //Гкал/куб.м

        [XmlEnum(Name = "40")]
        id40, //кВт/чел.

        [XmlEnum(Name = "41")]
        id41, //кВт*ч/чел.в мес.

        [XmlEnum(Name = "42")]
        id42, //руб./1000куб.м.

        [XmlEnum(Name = "43")]
        id43, //куб.м/кв.м общ. Имущества в мес.

        [XmlEnum(Name = "44")]
        id44, //кВт*ч/кв.м общ. Имущества в мес.

        [XmlEnum(Name = "45")]
        id45, //кВт/кв.м

        [XmlEnum(Name = "46")]
        id46, //Гкал/кв.м в мес.

        [XmlEnum(Name = "47")]
        id47, //Гкал/кв.м. в год

        [XmlEnum(Name = "48")]
        id48, //руб./чел. В мес.

        [XmlEnum(Name = "49")]
        id49, //руб./кв.м

        [XmlEnum(Name = "50")]
        id50, //руб./кг

        [XmlEnum(Name = "51")]
        id51, //1 пролёт

        [XmlEnum(Name = "52")]
        id52, //10 погонных метров

        [XmlEnum(Name = "53")]
        id53, //10 шт.

        [XmlEnum(Name = "54")]
        id54, //100 кв.м.

        [XmlEnum(Name = "55")]
        id55, //100 куб.м.

        [XmlEnum(Name = "56")]
        id56, //100 погонных метров

        [XmlEnum(Name = "57")]
        id57, //100 шт.

        [XmlEnum(Name = "58")]
        id58, //1000 кв.м.

        [XmlEnum(Name = "59")]
        id59, //1 м фальца

        [XmlEnum(Name = "60")]
        id60, //Квартира

        [XmlEnum(Name = "61")]
        id61, //руб/ч

        [XmlEnum(Name = "62")]
        id62, //руб/шт

        [XmlEnum(Name = "63")]
        id63, //Комплекс работ/мес.

        [XmlEnum(Name = "64")]
        id64, //кв.м./мес.

        [XmlEnum(Name = "65")]
        id65, //погонный метр/мес.

        [XmlEnum(Name = "66")]
        id66, //шт./мес.

        [XmlEnum(Name = "67")]
        id67, //этаж/мес.

        [XmlEnum(Name = "68")]
        id68, //куб.м./мес.

        [XmlEnum(Name = "69")]
        id69, //1000 куб.м.

        [XmlEnum(Name = "70")]
        id70, //куб.м./чел. В год

        [XmlEnum(Name = "71")]
        id71, //кг/чел. в мес.

        [XmlEnum(Name = "72")]
        id72, //кВт.ч

        [XmlEnum(Name = "73")]
        id73, //куб. м/чел.

        [XmlEnum(Name = "74")]
        id74, //кВт.ч/сут

        [XmlEnum(Name = "75")]
        id75, //кВт/м

        [XmlEnum(Name = "76")]
        id76, //тыс кВт.ч

        [XmlEnum(Name = "77")]
        id77, //ГВт.ч

        [XmlEnum(Name = "78")]
        id78, //ккал

        [XmlEnum(Name = "79")]
        id79, //ккал/ч

        [XmlEnum(Name = "80")]
        id80, //млн т

        [XmlEnum(Name = "81")]
        id81, //нор. м3

        [XmlEnum(Name = "82")]
        id82 //КХ
    }
}