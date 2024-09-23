using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using iText.Kernel.XMP.Properties;
using iText.Kernel.XMP;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using System.Globalization;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF InserirMetadados(this DocumentoPDF documento, List<Metadado> metadados)
        {
            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);

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

            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();

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

            pdfDocument.Close();

            return new DocumentoPDF(outputStream);

        }

        private static List<Metadado> ObterMetadados(this DocumentoPDF documento)
        {
            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            using PdfDocument pdfDocument = new PdfDocument(pdfReader);

            byte[] xmpBytes = pdfDocument.GetXmpMetadata();
            List <Metadado> metadata = new List<Metadado>();

            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
            PdfDictionary infoDictionary = pdfDocument.GetTrailer().GetAsDictionary(PdfName.Info);
            foreach (PdfName key in infoDictionary.KeySet())
            {

                string name = key.ToString().Replace("/", "");
                string value = infoDictionary.GetAsString(key).ToString();

                switch (name)
                {
                    case "Author":
                        value = info.GetAuthor().ToString();
                        break;
                    case "Creator":
                        value = info.GetCreator().ToString();
                        break;
                    case "Keywords":
                        value = info.GetKeywords().ToString();
                        break;
                    case "Producer":
                        value = info.GetProducer().ToString();
                        break;
                    case "Subject":
                        value = info.GetSubject().ToString();
                        break;
                    case "Title":
                        value = info.GetTitle().ToString();
                        break;
                    case "Trapped":
                        value = info.GetTrapped().ToString();
                        break;
                    case "CreationDate":
                        value = DateTime.ParseExact(info
                                .GetMoreInfo("CreationDate").ToString().Replace("D:", ""), 
                                "yyyyMMddHHmmssZ", 
                                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToString();
                        break;
                    case "ModDate":
                        value = DateTime.ParseExact(info
                                .GetMoreInfo("CreationDate").ToString().Replace("D:", ""),
                                "yyyyMMddHHmmssZ",
                                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToString();
                        break;
                }
                metadata.Add(new Metadado(name,value));
            }

            return metadata;

        }

        /// <summary>
        /// Adiciona metadados a um documento PDF.
        /// </summary>
        /// <param name="documento">Documento PDF no qual serão adicionados os metadados.</param>
        /// <param name="metadados">Uma lista de metadados a serem adicionados.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF AdicionarMetadados(this DocumentoPDF documento, List<Metadado> metadados)
        {
            return InserirMetadados(documento, metadados);
        }

        /// <summary>
        /// Adiciona metadados a um documento PDF.
        /// </summary>
        /// <param name="documento">Documento PDF no qual serão adicionados os metadados.</param>
        /// <param name="metadados">Uma array de strings [nome,valor] a serem adicionados.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF AdicionarMetadados(this DocumentoPDF documento, string[,] metadados)
        {
            List<Metadado> _metadados = new List<Metadado>();
            for (int i = 0; i < metadados.GetLength(0); i++)
            {
                _metadados.Add(new Metadado(metadados[i,0], metadados[i,1]));
            }

            return InserirMetadados(documento, _metadados);
        }

        /// <summary>
        /// Adiciona metadados a um documento PDF.
        /// </summary>
        /// <param name="documento">Documento PDF no qual serão adicionados os metadados.</param>
        /// <param name="metadado">Metadado a ser adicionado.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF AdicionarMetadado(this DocumentoPDF documento, Metadado metadado)
        {
            List<Metadado> metadados = new List<Metadado>();
            metadados.Add(metadado);
            return InserirMetadados(documento, metadados);
        }

        /// <summary>
        /// Adiciona metadados a um documento PDF.
        /// </summary>
        /// <param name="documento">Documento PDF no qual serão adicionados os metadados.</param>
        /// <param name="nome">Nome do metadado.</param>
        /// <param name="valor">Valor do metadado.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF AdicionarMetadado(this DocumentoPDF documento, string nome, string valor)
        {
            List<Metadado> metadados = new List<Metadado>();
            metadados.Add(new Metadado(nome,valor));
            return InserirMetadados(documento, metadados);
        }

        /// <summary>
        /// Obtém metadados de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento a se obter os metadados</param>
        /// <returns>List<Metadado></returns>

        public static List<Metadado> ObtemMetadados(this DocumentoPDF documento)
        {
            return ObterMetadados(documento);
        }

    }

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

}
