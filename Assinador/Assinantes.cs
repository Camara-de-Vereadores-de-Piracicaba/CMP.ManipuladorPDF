using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Pdf;
using iText.Signatures;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using static CMP.Certificados.Certificado;
using Org.BouncyCastle.Asn1;
using System.Text;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        
        private static List<AssinanteDocumento> DevolverAssinantes(
        this DocumentoPDF documento
        )
        {
            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
            IList<String> names = signatureUtil.GetSignatureNames();
            List<AssinanteDocumento> assinantes = new List<AssinanteDocumento>();
            foreach (String signatureName in names)
            {
                PdfPKCS7 signature = signatureUtil.ReadSignatureData(signatureName);
                if (!signature.IsTsp())
                {
                    IX509Certificate certificate = signature.GetCertificates().Last();

                    CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(certificate);
                    CertificateInfo.X500Name issuer = CertificateInfo.GetIssuerFields(certificate);

                    string name = info.GetField("CN");
                    string email = info.GetField("E");
                    string iss = issuer.GetField("CN");

                    TipoCertificado tipo = GetCertificateType(certificate.GetEncoded());

                    if(tipo==TipoCertificado.A1 || tipo == TipoCertificado.A3)
                    {
                        byte[] extensions = certificate.GetExtensionValue("2.5.29.17").GetOctets();
                        Asn1InputStream inputStream = new Asn1InputStream(extensions);
                        Asn1Object obj = inputStream.ReadObject();
                        Asn1Sequence sequence = Asn1Sequence.GetInstance(obj);

                        string rfcemail = "";

                        for (int i = 0; i <= sequence.Count; i++)
                        {
                            rfcemail = Encoding.UTF8.GetString(sequence[i].GetDerEncoded()).Substring(2).ToLower();
                            if(rfcemail.Contains('@')) break;
                        }

                        email ??= rfcemail;
                        name = name.Split(':')[0].ToTitleCase();
                    }

                    if (tipo == TipoCertificado.CMP)
                    {
                        certificate = signature.GetCertificates().First();
                        info = CertificateInfo.GetSubjectFields(certificate);
                        issuer = CertificateInfo.GetIssuerFields(certificate);
                        name = info.GetField("CN").ToTitleCase();
                        email = info.GetField("E").ToLower();
                        iss = issuer.GetField("CN");
                    }

                    assinantes.Add(new AssinanteDocumento()
                    {
                        Certificado = certificate,
                        Nome = name,
                        Email = email,
                        Data = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G"),
                        Razao = signature.GetReason(),
                        Emissor = iss,
                        Tipo = tipo
                    });
                }
            }

            return assinantes;
        }

        /// <summary>
        /// Devolve todos os assinantes de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cujos assinantes serão devolvidos.</param>

        public static List<AssinanteDocumento> Assinantes(
            this DocumentoPDF documento
        )
        {
            return documento.DevolverAssinantes();
        }

    }

    public class AssinanteDocumento
    {
        public IX509Certificate Certificado { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Razao { get; set; }
        public string Data { get; set; }
        public string Emissor { get; set; }
        public TipoCertificado Tipo { get; set; }
    }

}



