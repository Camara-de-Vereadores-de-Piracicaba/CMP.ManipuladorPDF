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
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf.Annot;
using CMP.ManipuladorPDF.Patch;
using S = System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static List<AssinanteDocumento> DevolverAssinantes(
            this DocumentoPDF documento,
            X509VerificationFlags flags,
            X509RevocationFlag revocation,
            X509RevocationMode mode)
        {
            documento = documento.DesencriptarCasoNecessario();

            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
            IList<String> names = signatureUtil.GetSignatureNames();
            List<AssinanteDocumento> assinantes = new List<AssinanteDocumento>();
            foreach (String signatureName in names)
            {
                try
                {

                    DateTime currentDate = DateTime.UtcNow;

                    List<string> status = new List<string>();

                    PdfPKCS7 signature = signatureUtil.ReadSignatureData(signatureName);

                    if (!signature.IsTsp())
                    {

                        IX509Certificate certificate = signature.GetSignCertificateChain().First();

                        S.X509Certificate2 x509 = new S.X509Certificate2(certificate.GetEncoded());

                        CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(certificate);
                        CertificateInfo.X500Name issuer = CertificateInfo.GetIssuerFields(certificate);

                        string name = info.GetField("CN");
                        string email = info.GetField("E");
                        string iss = issuer.GetField("CN");

                        TipoCertificado tipo = GetCertificateType(certificate.GetEncoded());

                        if (tipo == TipoCertificado.A1 || tipo == TipoCertificado.A3 || tipo == TipoCertificado.GOVBR)
                        {
                            try
                            {
                                var asn1 = certificate.GetExtensionValue("2.5.29.17");
                                byte[] extensions = asn1.GetOctets();
                                Asn1InputStream inputStream = new Asn1InputStream(extensions);
                                if (inputStream != null)
                                {
                                    Asn1Object obj = inputStream.ReadObject();
                                    Asn1Sequence sequence = Asn1Sequence.GetInstance(obj);
                                    string rfcemail = "";
                                    for (int i = 0; i <= sequence.Count; i++)
                                    {
                                        rfcemail = Encoding.UTF8.GetString(sequence[i].GetDerEncoded()).Substring(2).ToLower();
                                        if (rfcemail.Contains('@'))
                                            break;
                                    }
                                    email ??= rfcemail;
                                }
                                name = name.Split(':')[0].ToTitleCase();
                            }
                            catch (Exception) { }

                            ValidacaoCertificado vc = x509.ValidarCertificado(flags);

                        }

                        if (tipo == TipoCertificado.CMP)
                        {
                            certificate = signature.GetCertificates().First();
                            info = CertificateInfo.GetSubjectFields(certificate);
                            issuer = CertificateInfo.GetIssuerFields(certificate);
                            name = info.GetField("CN")?.ToTitleCase();
                            email = info.GetField("E")?.ToLower();
                            iss = issuer.GetField("CN");
                        }

                        AssinanteDocumento assinante = new AssinanteDocumento()
                        {
                            Documento = documento,
                            Certificado = certificate,
                            Nome = name,
                            Email = email,
                            Data = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G"),
                            Razao = signature.GetReason(),
                            Emissor = iss,
                            Tipo = tipo,
                            Validacao = x509.ValidarCertificado(flags),
                            ValidacaoCompleta = x509.ValidarCertificado(X509VerificationFlags.NoFlag),
                            ValidacaoParcial = x509.ValidarCertificado(X509VerificationFlags.AllFlags)
                        };

                        assinante.Patch();

                        assinantes.Add(assinante);
                    }

                }
                catch (Exception)
                {
                    //Não faz nada. É só para evitar erros quando o documento é encriptado,
                    //já que a Adobe adiciona uma assinatura específica que não pode ser lida.
                }

            }

            return assinantes;
        }

        public static ValidacaoCertificado ValidarCertificado(
            this X509Certificate2 certificate, 
            X509VerificationFlags flags = X509VerificationFlags.AllFlags,
            X509RevocationFlag revocation = X509RevocationFlag.EntireChain,
            X509RevocationMode mode = X509RevocationMode.Online)
        {
            using X509Chain chain = new X509Chain();

            ValidacaoCertificado vc = new ValidacaoCertificado();

            chain.ChainPolicy.RevocationMode = mode;
            chain.ChainPolicy.RevocationFlag = revocation;
            chain.ChainPolicy.VerificationFlags = flags;
            chain.ChainPolicy.VerificationTime = DateTime.UtcNow;
            
            vc.Valido = chain.Build(certificate);
            vc.Status = new List<X509ChainStatusFlags>();

            if (!vc.Valido)
            {
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    vc.Status.Add(status.Status);
                }
            }

            return vc;

        }



        private static List<CampoAssinatura> ListarCamposDeAssinatura(
            this DocumentoPDF documento
        )
        {

            documento = documento.DesencriptarCasoNecessario();

            using MemoryStream outputStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, false);

            List <CampoAssinatura> list = new List<CampoAssinatura>();

            if (form != null)
            {
                IDictionary<String, PdfFormField> fields = form.GetAllFormFields();
                foreach (var fieldEntry in fields)
                {
                    PdfFormField field = fieldEntry.Value;
                    if (field.GetFormType() == PdfName.Sig)
                    {
                        PdfWidgetAnnotation annotation = field.GetWidgets().FirstOrDefault();

                        if (annotation != null)
                        {
                            PdfArray positions = annotation.GetRectangle();

                            if (positions != null)
                            {

                                float X = float.Parse(positions.Get(0).ToString());
                                float Y = float.Parse(positions.Get(1).ToString());
                                float W = float.Parse(positions.Get(2).ToString());
                                float H = float.Parse(positions.Get(3).ToString());

                                CampoAssinatura _field = new CampoAssinatura()
                                {
                                    Nome = fieldEntry.Key,
                                    X = X,
                                    Y = Y,
                                    W = W - X,
                                    H = H - Y
                                };

                                list.Add(_field);
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Devolve todos os campos de assinatura de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cujos campos de assinatura serão devolvidos.</param>

        public static List<CampoAssinatura> ObterCamposDeAssinatura(
            this DocumentoPDF documento
        )
        {
            return documento.ListarCamposDeAssinatura();
        }

        /// <summary>
        /// Checa se um PDF tem um carimbo lateral antigo
        /// </summary>
        /// <param name="documento">Documento para checar.</param>

        public static bool TemCarimboAntigo(
            this DocumentoPDF documento
        )
        {
            List<CampoAssinatura> campos = documento.ListarCamposDeAssinatura();
            foreach(CampoAssinatura campo in campos)
            {
                if (
                    campo.X == 535 &&
                    campo.Y == 30 &&
                    campo.W == 50 &&
                    campo.H == 800
                )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Devolve todos os assinantes de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cujos assinantes serão devolvidos.</param>
        /// <param name="flags">Objeto X509VerificationFlags que definem especificidades da validação da assinatura.</param>
        /// <param name="revocation">Objeto X509RevocationFlag que indica se a validação será feita em toda a cadeia, ou em certificados específicos.</param>
        /// <param name="mode">Objeto X509RevocationMode que define se o teste de revogação será online, offline ou não será checado.</param>

        public static List<AssinanteDocumento> Assinantes(
            this DocumentoPDF documento,
            X509VerificationFlags flags = X509VerificationFlags.AllFlags,
            X509RevocationFlag revocation = X509RevocationFlag.EntireChain,
            X509RevocationMode mode = X509RevocationMode.Online
        )
        {
            return documento.DevolverAssinantes(flags,revocation,mode);
        }

    }

    public class AssinanteDocumento
    {
        public DocumentoPDF Documento { get; set; }
        public IX509Certificate Certificado { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Razao { get; set; }
        public string Data { get; set; }
        public string Emissor { get; set; }
        public ValidacaoCertificado Validacao { get; set; }
        public ValidacaoCertificado ValidacaoCompleta { get; set; }
        public ValidacaoCertificado ValidacaoParcial { get; set; }
        public TipoCertificado Tipo { get; set; }
    }

    public class CampoAssinatura
    {
        public string Nome { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }
    }

    public class ValidacaoCertificado
    {
        public bool Valido { get; set; }
        public List<X509ChainStatusFlags> Status { get; set; }

    }

    public enum ValidacaoAssinatura
    {
        NORMAL,
        COMPLETA,
        PARCIAL
    }

}