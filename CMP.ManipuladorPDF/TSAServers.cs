using System;
using System.Collections.Generic;
using System.Text;

namespace CMP.ManipuladorPDF
{
    public class TSAServers
    {
        public static string TSA_DEFAULT { get; set; } = "https://rfc3161.ai.moda";
        public static string TSA_GLOBALSIGN { get; set; } = "http://rfc3161timestamp.globalsign.com/advanced";
        public static string TSA_GLOBALSIGN2 { get; set; } = "http://timestamp.globalsign.com/tsa/r6advanced1";
        public static string TSA_GLOBALSIGN3 { get; set; } = "http://aatl-timestamp.globalsign.com/tsa/aohfewat2389535fnasgnlg5m23";
        public static string TSA_DIGICERT { get; set; } = "http://timestamp.digicert.com";
        public static string TSA_SECTIGO { get; set; } = "https://timestamp.sectigo.com";
        public static string TSA_ENTRUST { get; set; } = "http://timestamp.entrust.net/TSS/RFC3161sha2TS";
        public static string TSA_DOCUSIGN { get; set; } = "http://kstamp.keynectis.com/KSign/";
        public static string TSA_QUOVADIS { get; set; } = "http://ts.quovadisglobal.com/eu";
        public static string TSA_SSLDOTCOM { get; set; } = "http://ts.ssl.com";
        public static string TSA_IDENTRUST { get; set; } = "http://timestamp.identrust.com";

    }
}
