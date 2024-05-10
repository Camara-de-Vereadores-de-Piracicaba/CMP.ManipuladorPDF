using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Pdf;
using iText.Signatures;
using System.Collections.Generic;
using System;
using System.IO;
using iText.Commons.Bouncycastle.Asn1.X500;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        
        private static List<AssinantesDocumento> DevolverAssinantes(
        this DocumentoPDF documento
        )
        {
            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
            IList<String> names = signatureUtil.GetSignatureNames();
            List<AssinantesDocumento> assinantes = new List<AssinantesDocumento>();
            foreach (String signatureName in names)
            {
                PdfPKCS7 signature = signatureUtil.ReadSignatureData(signatureName);
                if (!signature.IsTsp())
                {
                    IX509Certificate certificate = signature.GetCertificates()[0];
                    CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(certificate);
                    assinantes.Add(new AssinantesDocumento()
                    {
                        Certificado = certificate,
                        Nome = info.GetField("CN"),
                        Email = info.GetField("E"),
                        Data = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G"),
                        Razao = signature.GetReason()
                    });
                }
            }
            return assinantes;
        }

        /// <summary>
        /// Devolve todos os assinantes de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cujos assinantes serão devolvidos.</param>

        public static List<AssinantesDocumento> Assinantes(
            this DocumentoPDF documento
        )
        {
            return documento.DevolverAssinantes();
        }

    }

    public class AssinantesDocumento
    {
        public IX509Certificate Certificado { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Razao { get; set; }
        public string Data { get; set; }
    }

}
