using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public static MemoryStream NumerarPDF(this MemoryStream sourceFile, int numeracaoInicial = 1, int left = 20, int fontSize = 9)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fs = sourceFile.GerarFileStream();
                fileStreams.Add(fs.FileStream);
                fileNames.Add(fs.FileName);

                var outputStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs.FileStream), writer);

                Document document = new Document(pdfDoc);

                for (var numPage = 1; numPage <= pdfDoc.GetNumberOfPages(); numPage++)
                {
                    // Adiciona o número da página no canto superior direito
                    Paragraph pageNumber = new Paragraph($"Página {numeracaoInicial}")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetMargin(0)
                        .SetFontSize(fontSize)
                        .SetFixedPosition(left, 760, 100);

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

        public static MemoryStream NumerarPDF(this string sourceFile, int numeracaoInicial = 1, int left = 450, int fontSize = 9)
        {
            var outputStream = new MemoryStream();

            PdfWriter writer = new PdfWriter(outputStream);
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(sourceFile), writer);

            Document document = new Document(pdfDoc);

            for (var numPage = 1; numPage <= pdfDoc.GetNumberOfPages(); numPage++)
            {
                // Adiciona o número da página no canto superior direito
                Paragraph pageNumber = new Paragraph($"Página {numeracaoInicial}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMargin(0)
                    .SetFontSize(fontSize)
                    .SetFixedPosition(left, 760, 100);

                document.ShowTextAligned(pageNumber, numPage * 20, 70, numPage, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);

                numeracaoInicial++;
            }

            document.Close();
            pdfDoc.Close();

            return outputStream;
        }

        private static (FileStream FileStream, string FileName) GerarFileStream(this MemoryStream source)
        {
            var fileName = System.IO.Path.GetTempFileName() + ".pdf";
            File.WriteAllBytes(fileName, source.ToArray());
            var fs = File.OpenRead(fileName);

            return (fs, fileName);
        }

        public static MemoryStream RetornarApenasUmaPaginaPDF(this MemoryStream sourceFile, int pagina = 1)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fs = sourceFile.GerarFileStream();
                fileStreams.Add(fs.FileStream);
                fileNames.Add(fs.FileName);

                var outputStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs.FileStream));
                PdfDocument newPdfDoc = new PdfDocument(writer);

                pdfDoc.CopyPagesTo(pagina, pagina, newPdfDoc);

                pdfDoc.Close();
                newPdfDoc.Close();

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

        public static MemoryStream RetornarApenasUmaPaginaPDF(this string sourceFile, int pagina = 1)
        {
            var outputStream = new MemoryStream();

            PdfWriter writer = new PdfWriter(outputStream);
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(sourceFile));
            PdfDocument newPdfDoc = new PdfDocument(writer);

            pdfDoc.CopyPagesTo(pagina, pagina, newPdfDoc);

            pdfDoc.Close();
            newPdfDoc.Close();

            return outputStream;
        }

        public static int QuantidadePaginasPDF(this MemoryStream sourceFile)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fs = sourceFile.GerarFileStream();
                fileStreams.Add(fs.FileStream);
                fileNames.Add(fs.FileName);

                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs.FileStream));
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

        public static int QuantidadePaginasPDF(this string file)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(file));
            return pdfDoc.GetNumberOfPages();
        }

        public static MemoryStream TornarSemEfeito(this string file)
        {
            var ms = new MemoryStream();

            using (PdfReader pdfReader = new PdfReader(file))
            {
                using (PdfWriter pdfWriter = new PdfWriter(ms))
                {
                    PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                    Document doc = new Document(pdfDocument);

                    // Obtenha as dimensões da página
                    Rectangle pageSize = pdfDocument.GetDefaultPageSize();

                    // Defina o texto a ser adicionado
                    string texto = "SEM EFEITO";

                    // Defina a fonte e o tamanho do texto
                    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    float fontSize = 80f;

                    // Percorra todas as páginas do documento
                    for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                    {
                        PdfPage page = pdfDocument.GetPage(pageNumber);
                        PdfCanvas canvas = new PdfCanvas(page);

                        // Calcule a posição do texto no meio da página (diagonal)
                        float x = (pageSize.GetLeft() + pageSize.GetRight()) / 2 - 150;
                        float y = (pageSize.GetBottom() + pageSize.GetTop()) / 2 - 150;

                        // Rotacione o texto em 45 graus
                        canvas.SaveState()
                              .BeginText()
                              .SetFontAndSize(font, fontSize)
                              .SetFillColor(DeviceGray.GRAY)
                              .MoveText(x, y)
                              .SetTextMatrix(0.7071f, 0.7071f, -0.7071f, 0.7071f, x, y)
                              .ShowText(texto)
                              .EndText()
                              .RestoreState();
                    }

                    // Feche o documento
                    doc.Close();
                }
            }

            return ms;
        }

        public static MemoryStream TornarSemEfeito(this MemoryStream sourceFile)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fs = sourceFile.GerarFileStream();
                fileStreams.Add(fs.FileStream);
                fileNames.Add(fs.FileName);
                var ms = new MemoryStream();

                using (PdfReader pdfReader = new PdfReader(fs.FileStream))
                {
                    using (PdfWriter pdfWriter = new PdfWriter(ms))
                    {
                        PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                        Document doc = new Document(pdfDocument);

                        // Obtenha as dimensões da página
                        Rectangle pageSize = pdfDocument.GetDefaultPageSize();

                        // Defina o texto a ser adicionado
                        string texto = "SEM EFEITO";

                        // Defina a fonte e o tamanho do texto
                        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        float fontSize = 80f;

                        // Percorra todas as páginas do documento
                        for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                        {
                            PdfPage page = pdfDocument.GetPage(pageNumber);
                            PdfCanvas canvas = new PdfCanvas(page);

                            // Calcule a posição do texto no meio da página (diagonal)
                            float x = (pageSize.GetLeft() + pageSize.GetRight()) / 2 - 150;
                            float y = (pageSize.GetBottom() + pageSize.GetTop()) / 2 - 150;

                            // Rotacione o texto em 45 graus
                            canvas.SaveState()
                                  .BeginText()
                                  .SetFontAndSize(font, fontSize)
                                  .SetFillColor(DeviceGray.GRAY)
                                  .MoveText(x, y)
                                  .SetTextMatrix(0.7071f, 0.7071f, -0.7071f, 0.7071f, x, y)
                                  .ShowText(texto)
                                  .EndText()
                                  .RestoreState();
                        }

                        // Feche o documento
                        doc.Close();
                    }
                }

                return ms;
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

            return null;
        }

        public static MemoryStream AdicionarRodape(this MemoryStream sourceFile, string texto, int left = 20, int fontSize = 9)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fs = sourceFile.GerarFileStream();
                fileStreams.Add(fs.FileStream);
                fileNames.Add(fs.FileName);

                var outputStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs.FileStream), writer);

                Document document = new Document(pdfDoc);

                for (var numPage = 1; numPage <= pdfDoc.GetNumberOfPages(); numPage++)
                {
                    // Adiciona o número da página no canto superior direito
                    Paragraph pageNumber = new Paragraph(texto)
                        .SetMargin(0)
                        .SetFontSize(fontSize)
                        .SetFixedPosition(left, 760, 300);

                    document.ShowTextAligned(pageNumber, numPage * 20, 70, numPage, TextAlignment.LEFT, VerticalAlignment.TOP, 0);
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

        public static MemoryStream AdicionarRodape(this string sourceFile, string texto, int left = 20, int fontSize = 9)
        {
            var outputStream = new MemoryStream();

            PdfWriter writer = new PdfWriter(outputStream);
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(sourceFile), writer);

            Document document = new Document(pdfDoc);

            for (var numPage = 1; numPage <= pdfDoc.GetNumberOfPages(); numPage++)
            {
                // Adiciona o número da página no canto superior direito
                Paragraph pageNumber = new Paragraph(texto)
                        .SetMargin(0)
                        .SetFontSize(fontSize)
                        .SetFixedPosition(left, 760, 300);

                document.ShowTextAligned(pageNumber, numPage * 20, 70, numPage, TextAlignment.LEFT, VerticalAlignment.TOP, 0);
            }

            document.Close();
            pdfDoc.Close();

            return outputStream;
        }

        public static MemoryStream JuntarArquivosPDF(this List<MemoryStream> sourceFiles)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var outputStream = new MemoryStream();
                PdfWriter writer = new PdfWriter(outputStream);
                PdfDocument newPdfDoc = new PdfDocument(writer);
                foreach (var sourceFile in sourceFiles)
                {
                    var fs = sourceFile.GerarFileStream();
                    fileStreams.Add(fs.FileStream);
                    fileNames.Add(fs.FileName);

                    PdfDocument pdfDoc = new PdfDocument(new PdfReader(fs.FileStream));
                    pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), newPdfDoc);
                    pdfDoc.Close();
                }

                newPdfDoc.Close();
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

            return null;
        }

        public static MemoryStream JuntarArquivosPDF(this List<string> sourceFiles)
        {
            var outputStream = new MemoryStream();
            PdfWriter writer = new PdfWriter(outputStream);
            PdfDocument newPdfDoc = new PdfDocument(writer);

            foreach (var sourceFile in sourceFiles)
            {
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(sourceFile));
                pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), newPdfDoc);
                pdfDoc.Close();
            }

            newPdfDoc.Close();
            return outputStream;
        }
    }
}
