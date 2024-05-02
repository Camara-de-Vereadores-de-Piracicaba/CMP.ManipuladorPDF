using System.IO;

namespace CMP.ManipuladorPDF
{

    public partial class DocumentoPDF
    {
        public byte[] ByteArray { get; set; }
        public string ValidadorURL { get; set; } = "https://validar.camarapiracicaba.sp.gov.br";


        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="sourceStream">MemoryStream do arquivo PDF.</param>

        public DocumentoPDF(MemoryStream sourceStream)
        {
            ByteArray = sourceStream.ToArray();
        }


        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="filePath">Caminho absoluto do arquivo PDF.</param>

        public DocumentoPDF(string filePath)
        {
            ByteArray = File.ReadAllBytes(filePath);
        }


        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="documentByteArray">ByteArray do arquivo PDF.</param>

        public DocumentoPDF(byte[] documentByteArray)
        {
            ByteArray = documentByteArray;
        }

    }

}
