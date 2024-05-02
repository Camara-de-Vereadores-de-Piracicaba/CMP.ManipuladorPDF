using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {
        private static MemoryStream InserirMetadados(
            PdfReader pdfReader,
            List<Metadado> metadados
        )
        {
            
            using var outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter);

            string[] unique =
            {
                "Author",
                "CreationDate",
                "Creator",
                "Keywords",
                "ModDate",
                "MoreInfo",
                "Producer",
                "Subject",
                "Title",
                "Trapped"
            };

            PdfDocumentInfo info = pdfDoc.GetDocumentInfo();

            foreach (Metadado metadado in metadados)
            {

                if (unique.Contains(metadado.Nome))
                {
                    switch (metadado.Nome)
                    {
                        case "Author":
                            info.SetAuthor(metadado.Valor);
                            break;
                        case "Creator":
                            info.SetCreator(metadado.Valor);
                            break;
                        case "Keywords":
                            info.SetKeywords(metadado.Valor);
                            break;
                        case "Producer":
                            info.SetProducer(metadado.Valor);
                            break;
                        case "Subject":
                            info.SetSubject(metadado.Valor);
                            break;
                        case "Title":
                            info.SetTitle(metadado.Valor);
                            break;
                        case "Trapped":
                            info.SetTrapped(new PdfName(metadado.Valor));
                            break;
                    }
                }
                else
                {
                    info.SetMoreInfo(metadado.Nome, metadado.Valor);
                }

            }

            pdfDoc.Close();

            return outputStream;

        }

        private static List<Metadado> LerMetadados(PdfReader pdfReader)
        {
            
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            PdfDictionary trailer = pdfDocument.GetTrailer();
            PdfDictionary metadata = trailer.GetAsDictionary(PdfName.Info);
            ICollection<PdfName> keys = metadata.KeySet();

            List<Metadado> metadados = new List<Metadado>();

            foreach (PdfName key in keys)
            {
                string value = ((PdfString)metadata.Get(key)).GetValue();
                metadados.Add(new Metadado(key.GetValue(), value));
            }

            return metadados;

        }

        public static MemoryStream AdicionarMetadados(MemoryStream sourceFile, List<Metadado> metadados)
        {
            sourceFile.Seek(0, SeekOrigin.Begin);
            MemoryStream _stream = InserirMetadados(new PdfReader(sourceFile), metadados);
            return new MemoryStream(_stream.ToArray());
        }
        public static MemoryStream AdicionarMetadados(string sourceFile, List<Metadado> metadados)
        {
            MemoryStream _stream = InserirMetadados(new PdfReader(sourceFile), metadados);
            return new MemoryStream(_stream.ToArray());
        }

        public static List<Metadado> ObterMetadados(MemoryStream sourceFile)
        {
            sourceFile.Seek(0, SeekOrigin.Begin);
            return LerMetadados(new PdfReader(sourceFile));
        }
        public static List<Metadado> ObterMetadados(string sourceFile)
        {
            return LerMetadados(new PdfReader(sourceFile));
        }

        /*
        public class Metadado
        {
            public string Nome { get; set; }
            public string Valor { get; set; }

            public Metadado(string nome, string valor)
            {
                Nome = nome;
                Valor = valor;
            }
        }
        */

    }
}
