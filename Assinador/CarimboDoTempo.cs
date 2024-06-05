using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Pdf;
using iText.Signatures;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        
        private static List<CarimboDoTempo> DevolverCarimboDoTempo(
        this DocumentoPDF documento
        )
        {
            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
            IList<String> names = signatureUtil.GetSignatureNames();
            List<CarimboDoTempo> carimbos = new List<CarimboDoTempo>();
            foreach (String signatureName in names)
            {
                PdfPKCS7 signature = signatureUtil.ReadSignatureData(signatureName);
                if (signature.IsTsp())
                {
                    IX509Certificate certificate = signature.GetCertificates().Last();
                    CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(certificate);
                    string name = info.GetField("CN");
                    carimbos.Add(new CarimboDoTempo()
                    {
                        Certificado = certificate,
                        Nome = name,
                        Data = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G")
                    });
                }
            }

            return carimbos;
        }

        /// <summary>
        /// Devolve todos os carimbos do tempo de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cujos carimbos do tempo serão devolvidos.</param>

        public static List<CarimboDoTempo> CarimbosDoTempo(
            this DocumentoPDF documento
        )
        {
            return documento.DevolverCarimboDoTempo();
        }

    }

    public class CarimboDoTempo
    {
        public IX509Certificate Certificado { get; set; }
        public string Nome { get; set; }
        public string Data { get; set; }
    }

}