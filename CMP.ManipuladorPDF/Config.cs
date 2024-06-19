using System;
using System.Collections;

namespace CMP.ManipuladorPDF
{
    public static class DocumentoPDFConfig
    {
        public static string FONT_PATH { get; set; } = "/app/fonts";
        public static string SIGNATURE_DEFAULT_FONT { get; set; } = "aptos";
        public static string SIGNATURE_DEFAULT_FONT_BOLD { get; set; } = "aptos-bold";

        public static void DefinirDiretorioDeFontes(
            string diretorio = null
        )
        {
            string _path = "/app/fonts";
            if(diretorio == null)
            {
                string mode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (mode == "Development")
                {
                    _path = "C:\\app\\fonts";
                }
            }
            else
            {
                _path = diretorio;
            }

            FONT_PATH = _path;
            
        }
    }
}
