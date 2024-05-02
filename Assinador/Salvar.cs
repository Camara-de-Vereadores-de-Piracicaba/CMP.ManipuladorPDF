using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        public static void Salvar(this DocumentoPDF documento, string caminho)
        {
            File.WriteAllBytes(caminho, documento.ByteArray);
        }

    }
}