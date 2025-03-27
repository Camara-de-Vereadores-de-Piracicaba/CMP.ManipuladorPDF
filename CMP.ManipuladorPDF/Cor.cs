using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using System;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF ConverterParaEspacoDeCor(
            this DocumentoPDF documento,
            string espacoDeCor
        )
        {

            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            using (PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                DeviceGray targetColorSpace = DeviceGray.GRAY;
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    PdfPage page = pdfDoc.GetPage(i);
                }
            }

            return new DocumentoPDF(outputStream.ToArray());

        }

        /// <summary>
        /// Converte o espaço de cor de um documento
        /// </summary>
        /// <param name="documento">Documento para ter o espaço de cor convertido.</param>

        public static DocumentoPDF ConverterParaSRGB(
            this DocumentoPDF documento
        )
        {
            try
            {
                return ConverterParaEspacoDeCor(documento, "sRGB");
            }
            catch(Exception exception)
            {
                throw new PdfColorConversionErrorException(exception.Message);
            }
        }


    }
}