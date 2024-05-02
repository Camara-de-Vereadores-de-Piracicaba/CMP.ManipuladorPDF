using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Salva o documento para um caminho no disco.
        /// </summary>
        /// <param name="documento">Documento PDF para salvar.</param>
        /// <param name="caminho">Caminho no disco.</param>

        public static void Salvar(this DocumentoPDF documento, string caminho)
        {
            File.WriteAllBytes(caminho, documento.ByteArray);
        }

        /// <summary>
        /// Converte um documento PDF para um MemoryStream
        /// </summary>
        /// <param name="documento">Documento PDF para converter.</param>
        /// <returns></returns>

        public static MemoryStream ConverterParaMemoryStream(this DocumentoPDF documento)
        {
            return new MemoryStream(documento.ByteArray);
        }

        /// <summary>
        /// Converte um documento PDF para um ByteArray
        /// </summary>
        /// <param name="documento">Documento PDF para converter.</param>
        /// <returns></returns>

        public static byte[] ConverterParaByteArray(this DocumentoPDF documento)
        {
            return documento.ByteArray;
        }

    }
}