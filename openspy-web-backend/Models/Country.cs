using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class Country
    {
        public class CountryRegion
        {
            public CountryRegion(string countrycode, string countryname, System.Int32 region)
            {
                this.countrycode = countrycode;
                this.countryname = countryname;
                this.region = region;
            }
            public string countrycode;
            public string countryname;
            public System.Int32 region;
        };
        protected static readonly int REGIONID_AMERICAS = 1;
        protected static readonly int REGIONID_NORTH_AMERICA = 2;
        protected static readonly int REGIONID_CARIBBEAN = 4;
        protected static readonly int REGIONID_CENTRAL_AMERICA = 8;
        protected static readonly int REGIONID_SOUTH_AMERICA = 16;
        protected static readonly int REGIONID_AFRICA = 32;
        protected static readonly int REGIONID_CENTRAL_AFRICA = 64;
        protected static readonly int REGIONID_EAST_AFRICA = 128;
        protected static readonly int REGIONID_NORTH_AFRICA = 256;
        protected static readonly int REGIONID_SOUTH_AFRICA = 512;
        protected static readonly int REGIONID_WEST_AFRICA = 1024;
        protected static readonly int REGIONID_ASIA = 2048;
        protected static readonly int REGIONID_EAST_ASIA = 4096;
        protected static readonly int REGIONID_PACIFIC = 8192;
        protected static readonly int REGIONID_SOUTH_ASIA = 16384;
        protected static readonly int REGIONID_SOUTH_EAST_ASIA = 32768;
        protected static readonly int REGIONID_EUROPE = 65536;
        protected static readonly int REGIONID_BALTIC_STATES = 131072;
        protected static readonly int REGIONID_CIS = 262144;
        protected static readonly int REGIONID_EASTERN_EUROPE = 524288;
        protected static readonly int REGIONID_MIDDLE_EAST = 1048576;
        protected static readonly int REGIONID_SOUTH_EAST_EUROPE = 2097152;
        protected static readonly int REGIONID_WESTERN_EUROPE = 4194304;
        static CountryRegion[] countries = {
                new CountryRegion("BI","Burundi",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("CM","Cameroon",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("CF","Central African Republic",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("TD","Chad",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("CG","Congo",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("GQ","Equatorial Guinea",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("RW","Rwanda",REGIONID_AFRICA|REGIONID_CENTRAL_AFRICA),
                new CountryRegion("DJ","Djibouti",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("ER","Eritrea",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("ET","Ethiopia",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("KE","Kenya",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("SC","Seychelles",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("SO","Somalia",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("SH","St. Helena",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("SD","Sudan",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("TZ","Tanzania",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("UG","Uganda",REGIONID_AFRICA|REGIONID_EAST_AFRICA),
                new CountryRegion("DZ","Algeria",REGIONID_AFRICA|REGIONID_NORTH_AFRICA),
                new CountryRegion("EG","Egypt",REGIONID_AFRICA|REGIONID_NORTH_AFRICA),
                new CountryRegion("LY","Libya",REGIONID_AFRICA|REGIONID_NORTH_AFRICA),
                new CountryRegion("MA","Morocco",REGIONID_AFRICA|REGIONID_NORTH_AFRICA),
                new CountryRegion("TN","Tunisia",REGIONID_AFRICA|REGIONID_NORTH_AFRICA),
                new CountryRegion("AO","Angola",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("BW","Botswana",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("BV","Bouvet Island",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("KM","Comoros",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("HM","Heard and McDonald Islands",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("LS","Lesotho",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("MG","Madagascar",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("MW","Malawi",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("MU","Mauritius",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("YT","Mayotte",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("MZ","Mozambique",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("NA","Namibia",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("RE","Reunion",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("ZA","South Africa",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("SZ","Swaziland",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("ZM","Zambia",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("ZW","Zimbabwe",REGIONID_AFRICA|REGIONID_SOUTH_AFRICA),
                new CountryRegion("BJ","Benin",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("BF","Burkina Faso",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("CV","Cape Verde",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("CI","Cote D`ivoire",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("GA","Gabon",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("GM","Gambia",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("GH","Ghana",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("GN","Guinea",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("GW","Guinea-Bissau",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("LR","Liberia",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("ML","Mali",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("MR","Mauritania",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("NE","Niger",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("NG","Nigeria",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("ST","Sao Tome and Principe",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("SN","Senegal",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("SL","Sierra Leone",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("TG","Togo",REGIONID_AFRICA|REGIONID_WEST_AFRICA),
                new CountryRegion("AI","Anguilla",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("AG","Antigua and Barbuda",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("AW","Aruba",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("BS","Bahamas",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("BB","Barbados",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("BM","Bermuda",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("KY","Cayman Islands",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("CU","Cuba",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("DM","Dominica",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("DO","Dominican Republic",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("GD","Grenada",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("GP","Guadeloupe",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("HT","Haiti",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("JM","Jamaica",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("MQ","Martinique",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("MS","Montserrat",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("AN","Netherlands Antilles",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("PR","Puerto Rico",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("VC","Saint Vincent and The Grenadines",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("KN","St Kitts-Nevis",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("LC","St Lucia",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("TT","Trinidad & Tobago",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("TC","Turks & Caicos Islands",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("VG","Virgin Islands (British)",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("VI","Virgin Islands (US)",REGIONID_AMERICAS|REGIONID_CARIBBEAN),
                new CountryRegion("BZ","Belize",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("CR","Costa Rica",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("SV","El Salvador",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("GT","Guatemala",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("HN","Honduras",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("MX","Mexico",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA), //gamespy says this not me!!
				new CountryRegion("NI","Nicaragua",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("PA","Panama",REGIONID_AMERICAS|REGIONID_CENTRAL_AMERICA),
                new CountryRegion("CA","Canada",REGIONID_AMERICAS|REGIONID_NORTH_AMERICA),
                new CountryRegion("GL","Greenland",REGIONID_AMERICAS|REGIONID_NORTH_AMERICA),
                new CountryRegion("PM","St. Pierre and Miquelon",REGIONID_AMERICAS|REGIONID_NORTH_AMERICA),
                new CountryRegion("US","United States",REGIONID_AMERICAS|REGIONID_NORTH_AMERICA),
                new CountryRegion("UM","US Minor Outlying Islands",REGIONID_AMERICAS|REGIONID_NORTH_AMERICA),
                new CountryRegion("AR","Argentina",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("BO","Bolivia",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("BR","Brazil",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("CL","Chile",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("CO","Colombia",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("EC","Ecuador",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("GF","S. Georgia and S. Sandwich Islands",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("SR","Suriname",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("UY","Uruguay",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("VE","Venezuela",REGIONID_AMERICAS|REGIONID_SOUTH_AMERICA),
                new CountryRegion("CN","China",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("HK","Hong Kong",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("JP","Japan",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("MO","Macao",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("MN","Mongolia",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("KP","North Korea",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("KR","South Korea",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("TW","Taiwan",REGIONID_ASIA|REGIONID_EAST_ASIA),
                new CountryRegion("AS","American Samoa",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("AU","Australia",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("CK","Cook Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("FJ","Fiji",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("PF","French Polynesia",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("GU","Guam",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("KI","Kiribati",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("MH","Marshall Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("FM","Micronesia",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("NR","Nauru",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("NC","New Caledonia",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("NZ","New Zealand",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("NU","Niue",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("NF","Norfolk Island",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("MP","Northern Mariana Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("PG","Papua New Guinea",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("PN","Pitcairn Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("EH","Samoa",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("SB","Solomon Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("TO","Tonga",REGIONID_ASIA|REGIONID_PACIFIC), //duplicate on gamespy :X
				new CountryRegion("TK","Tonga",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("TV","Tuvalu",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("VU","Vanuatu",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("WF","Wallis and Futuna Islands",REGIONID_ASIA|REGIONID_PACIFIC),
                new CountryRegion("AF","Afghanistan",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("BD","Bangladesh",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("BT","Bhutan",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("IO","British Indian Ocean Territory",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("IN","India",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("MV","Maldives",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("NP","Nepal",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("PK","Pakistan",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("LK","Sri Lanka",REGIONID_ASIA|REGIONID_SOUTH_ASIA),
                new CountryRegion("BN","Brunei Darussalam",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("KH","Cambodia",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("CX","Christmas Islands",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("CC","Cocos (Keeling Islands)",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("TP","East Timor",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("ID","Indonesia",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("LA","Laos",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("MY","Malaysia",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("MM","Myanmar",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("PW","Palau",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("PH","Philippines",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("SG","Singapore",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("TH","Thailand",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("VN","Vietnam",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("PH","Philippines",REGIONID_ASIA|REGIONID_SOUTH_EAST_ASIA),
                new CountryRegion("EE","Estonia",REGIONID_EUROPE|REGIONID_BALTIC_STATES),
                new CountryRegion("LV","Latvia",REGIONID_EUROPE|REGIONID_BALTIC_STATES),
                new CountryRegion("LT","Lithuania",REGIONID_EUROPE|REGIONID_BALTIC_STATES),
                new CountryRegion("AM","Armenia",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("AZ","Azerbaijan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("BY","Belarus",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("GE","Georgia",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("KZ","Kazakstan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("KG","Kyrgyzstan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("MD","Moldova",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("RU","Russian Federation",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("TJ","Tajikistan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("TM","Turkmenistan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("UA","Ukraine",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("UZ","Uzbekistan",REGIONID_EUROPE|REGIONID_CIS),
                new CountryRegion("CZ","Czech Republic",REGIONID_EUROPE|REGIONID_EASTERN_EUROPE),
                new CountryRegion("HU","Hungary",REGIONID_EUROPE|REGIONID_EASTERN_EUROPE),
                new CountryRegion("RO","Romania",REGIONID_EUROPE|REGIONID_EASTERN_EUROPE),
                new CountryRegion("SK","Slovak Republic",REGIONID_EUROPE|REGIONID_EASTERN_EUROPE),
				//middle east under europe?
				new CountryRegion("BH","Bahrain",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("IR","Iran",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("IQ","Iraq",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("IL","Israel/Occupied Territories",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("JO","Jordan",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("KW","Kuwait",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("LB","Lebanon",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("OM","Oman",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("QA","Qatar",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("SA","Saudi Arabia",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("SY","Syria",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("AE","United Arab Emirates",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
                new CountryRegion("YE","Yemen",REGIONID_EUROPE|REGIONID_MIDDLE_EAST),
				//
				new CountryRegion("AL","Albania",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("BA","Bosnia-Herzegovina",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("BG","Bulgaria",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("HR","Croatia",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("CY","Cyprus",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("GR","Greece",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("MK","Macedonia",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("MT","Malta",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("SI","Slovenia",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("TR","Turkey",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("YU","Yugoslavia",REGIONID_EUROPE|REGIONID_SOUTH_EAST_EUROPE),
                new CountryRegion("AD","Andorra",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("AT","Austria",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("BE","Belgium",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("DK","Denmark",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("FO","Faroe Islands",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("FI","Finland",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("FR","France",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("DE","Germany",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("GI","Gibraltar",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("IS","Iceland",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("IR","Ireland",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("IT","Italy",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("LI","Liechtenstein",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("LU","Luxembourg",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("MC","Monaco",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("NL","Netherlands",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("NO","Norway",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("PT","Portugal",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("SM","San Marino",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("ES","Spain",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("SJ","Svalbard and Jan Mayen Islands",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("SE","Sweden",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("CH","Switzerland",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("UK","United Kingdom",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE),
                new CountryRegion("VA","Vatican",REGIONID_EUROPE|REGIONID_WESTERN_EUROPE)
            };
        public static List<CountryRegion> GetCountries()
        {            
            return countries.OrderBy(g => g.countryname).ToList();
        }
    }
}
