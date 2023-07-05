using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using iText.IO.Source;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;

namespace ConversorHTML
{
    public static class HtmlToPDF
    {
        public static MemoryStream ConvertToPDF(this string html)
        {
            ConverterProperties converterProperties = new ConverterProperties();
            MemoryStream stream = new MemoryStream();
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(stream));
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            HtmlConverter.ConvertToPdf(html, pdfDocument, converterProperties);
            stream.Close();
            return stream;
        }

        public static MemoryStream NumerarPDF(this MemoryStream sourceFile, int numeracaoInicial = 1)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                var outputStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs), writer);

                Document document = new Document(pdfDoc);

                for (var numPage = 1; numPage <= pdfDoc.GetNumberOfPages(); numPage++)
                {
                    var page = pdfDoc.GetPage(numPage);

                    // Adiciona o número da página no canto superior direito
                    Paragraph pageNumber = new Paragraph($"Página {numeracaoInicial}")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetMargin(0)
                        .SetFixedPosition(450, 760, 50);

                    document.ShowTextAligned(pageNumber, numPage * 20, 70, numPage, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);

                    numeracaoInicial++;
                }

                document.Close();
                pdfDoc.Close();

                return outputStream;
            }
            catch (Exception ex)
            {
                foreach (var fs in fileStreams)
                {
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            finally
            {
                foreach (var fs in fileStreams)
                {
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            return sourceFile;
        }

        public static int QuantidadePaginasPDF(this MemoryStream sourceFile)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                var outputStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs), writer);
                Document document = new Document(pdfDoc);

                return pdfDoc.GetNumberOfPages();
            }
            catch (Exception ex)
            {
                foreach (var fs in fileStreams)
                {
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            finally
            {
                foreach (var fs in fileStreams)
                {
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            return 0;
        }
    }
}
