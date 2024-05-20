using CMP.ManipuladorPDF;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using SysadminsLV.Asn1Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Certificados
{

    public class Certificado
    {
        public byte[] ByteArray { get; set; }
        public string Senha { get; set; }
        public string Serial { get; set; }
        public string Atributos { get; set; }
        public DateTime Vencimento { get; set; }
        public PrivateKeySignature PKS { get; set; }
        public IX509Certificate[] Chain { get; set; }

        private static Pkcs12Store GetStore(Certificado certificado)
        {
            Stream stream = new MemoryStream(certificado.ByteArray);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            try
            {
                store.Load(stream, certificado.Senha.ToCharArray());
            }
            catch (Exception exception)
            {
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                {
                    throw new CertificateWrongPasswordException();
                }
            }

            return store;
        }

        private static string GetAlias(Pkcs12Store store)
        {
            string alias = null;
            foreach (object _alias in store.Aliases)
            {
                alias = (string)_alias;
                if (store.IsKeyEntry(alias))
                    break;
            }

            if (alias == null)
            {
                throw new CertificateInvalidException();
            }

            return alias;
        }

        private static PrivateKeySignature GetPKS(Certificado certificado)
        {
            Pkcs12Store store = GetStore(certificado);
            string alias = GetAlias(store);
            return new PrivateKeySignature(
                new PrivateKeyBC(store.GetKey(alias).Key),
                DigestAlgorithms.SHA256
            );
        }

        private static IX509Certificate[] GetChain(Certificado certificado)
        {
            Pkcs12Store store = GetStore(certificado);
            string alias = GetAlias(store);
            X509CertificateEntry[] certificateEntries = store.GetCertificateChain(alias);
            List<X509CertificateBC> _chain = new List<X509CertificateBC>();
            foreach (X509CertificateEntry certificateEntry in certificateEntries)
            {
                _chain.Add(new X509CertificateBC(certificateEntry.Certificate));
            }

            IX509Certificate[] chain = _chain.ToArray();
            return chain;
        }

        internal byte[] GenerateSerial()
        {
            byte[] serial = new byte[20];
            Random random = new Random();
            random.NextBytes(serial);
            return serial;
        }

        internal SecureString GetSecureString(string senha)
        {
            SecureString secureString = new SecureString();
            foreach (Char _char in senha)
            {
                secureString.AppendChar(_char);
            }

            return secureString;
        }

        internal string GetSerial(Certificado certificado)
        {
            Pkcs12Store store = GetStore(certificado);
            string alias = GetAlias(store);
            X509CertificateEntry target = store.GetCertificateChain(alias)[0];
            X509Certificate2 certificate = new X509Certificate2(DotNetUtilities.ToX509Certificate(target.Certificate));
            return certificate.GetSerialNumberString();
        }

        internal string GetAttributes(Certificado certificado)
        {
            Pkcs12Store store = GetStore(certificado);
            string alias = GetAlias(store);
            X509CertificateEntry target = store.GetCertificateChain(alias)[0];
            X509Certificate2 certificate = new X509Certificate2(DotNetUtilities.ToX509Certificate(target.Certificate));
            X509CertificateBC bc = new X509CertificateBC(DotNetUtilities.FromX509Certificate(certificate));
            CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(bc);
            string attributes = "/" + GetCertificateAttributes(
                info.GetField("CN"),
                info.GetField("E")
            ).Replace(",", "/");
            return attributes;
        }

        internal DateTime GetNotAfter(Certificado certificado)
        {
            X509Certificate2 _certificado = new X509Certificate2(certificado.ByteArray, certificado.Senha);
            return DateTime.Parse(_certificado.GetExpirationDateString());
        }

        internal string GetCertificateAttributes(
            string CN,
            string E,
            string L = "Piracicaba",
            string ST = "SP",
            string C = "BR",
            string O = "CMP",
            string DC = "camarapiracicaba.sp.gov.br"
        )
        {
            string CA = $@"L={L},ST={ST},C={C},E={E},O={O},";
            CA += string.Join(",", DC.Split('.').Select(w => String.Format("DC={0}", w)));
            CA += $",CN={CN}";
            return CA;
        }

        internal void SetAttributes(Certificado certificado)
        {
            PKS = GetPKS(certificado);
            Chain = GetChain(certificado);
            Serial = GetSerial(certificado);
            Atributos = GetAttributes(certificado);
            Vencimento = GetNotAfter(certificado);
        }

        private (byte[] byteArray, string serial, string atributos) GerarCertificado(
            Certificado raiz,
            string nome,
            string email,
            string senha,
            int tempoExpiracao = 1
        )
        {
            X509Certificate2 certificadoRaiz = new X509Certificate2(raiz.ByteArray, raiz.Senha, X509KeyStorageFlags.Exportable);
            RSA rsaKey = RSA.Create();
            string dadosCertificado = GetCertificateAttributes(nome, email);
            CertificateRequest certificateRequest = new CertificateRequest(dadosCertificado, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            X509Extension ocspExtension = OCSPExtension(PadroesCertificado.OCSP);
            certificateRequest.CertificateExtensions.Add(ocspExtension);
            //X509Extension crlExtension = CRLExtension(PadroesCertificado.CRL); //Não é necessário.
            //certificateRequest.CertificateExtensions.Add(crlExtension); //Não é necessário.
            byte[] serial = GenerateSerial();
            DateTimeOffset notBefore = DateTimeOffset.UtcNow;
            DateTimeOffset notAfter = notBefore.AddYears(tempoExpiracao);
            X509Certificate2 certificado = certificateRequest.Create(certificadoRaiz, notBefore, notAfter, serial).CopyWithPrivateKey(rsaKey);
            SecureString secureString = GetSecureString(senha);
            X509Certificate2Collection join = new X509Certificate2Collection();
            join.Add(certificadoRaiz);
            join.Add(certificado);
            byte[] exportedChain = join.Export(X509ContentType.Pfx, senha);
            return (
                exportedChain,
                certificado.GetSerialNumberString(),
                "/" + dadosCertificado.ToString().Replace(",", "/")
            );
        }

        private static X509Extension OCSPExtension(string url)
        {
            Byte[] encodedData = Asn1Builder.Create()
                .AddSequence(x => x.AddObjectIdentifier(new Oid("1.3.6.1.5.5.7.48.1"))
                .AddImplicit(6, Encoding.ASCII.GetBytes(url), true))
                .GetEncoded();
            return new X509Extension("1.3.6.1.5.5.7.1.1", encodedData, false);
        }

        private static X509Extension CRLExtension(string url)
        {
            byte[] encodedUrl = Encoding.ASCII.GetBytes(url);
            byte[] payload = new byte[encodedUrl.Length + 10];
            int offset = 0;
            payload[offset++] = 0x30;
            payload[offset++] = (byte)(encodedUrl.Length + 8);
            payload[offset++] = 0x30;
            payload[offset++] = (byte)(encodedUrl.Length + 6);
            payload[offset++] = 0xA0;
            payload[offset++] = (byte)(encodedUrl.Length + 4);
            payload[offset++] = 0xA0;
            payload[offset++] = (byte)(encodedUrl.Length + 2);
            payload[offset++] = 0x86;
            payload[offset++] = (byte)encodedUrl.Length;
            Buffer.BlockCopy(encodedUrl, 0, payload, offset, encodedUrl.Length);
            return new X509Extension("2.5.29.31", payload, critical: false);
        }

        public Certificado(
            byte[] certificado,
            string senha
        )
        {
            ByteArray = certificado;
            Senha = senha;
            SetAttributes(this);
        }

        public Certificado(
            string certificado,
            string senha
        )
        {
            try
            {
                ByteArray = File.ReadAllBytes(certificado);
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Could not find file"))
                {
                    throw new CertificateNotFoundException();
                }
            }

            Senha = senha;
            SetAttributes(this);
        }

        public Certificado(
            MemoryStream certificado,
            string senha
        )
        {
            ByteArray = certificado.ToArray();
            Senha = senha;
            SetAttributes(this);
        }

        public Certificado(
            Certificado raiz,
            string nome,
            string email,
            string senha
        )
        {
            (byte[] byteArray, string serial, string atributos) = GerarCertificado(raiz, nome, email, senha);
            ByteArray = byteArray;
            Senha = senha;
            SetAttributes(this);
        }

    }

    public static class PadroesCertificado
    {
        public static string OCSP { get; set; } = "https://ocsp.camarapiracicaba.sp.gov.br";
        public static string CRL { get; set; } = "https://ocsp.camarapiracicaba.sp.gov.br";
    }

    public static class CertificadoExtensionMethods
    {
        public static byte[] ToArray(this Certificado certificado)
        {
            return certificado.ByteArray;
        }

        public static MemoryStream ToMemoryStream(this Certificado certificado)
        {
            return new MemoryStream(certificado.ByteArray);
        }

        public static void SaveToDisk(this Certificado certificado, string caminho)
        {
            File.WriteAllBytes(caminho, certificado.ByteArray);
        }

        public static async Task AdicionarOCSP(this Certificado certificado)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"status", "V" },
                {"serial", certificado.Serial },
                {"notAfter", certificado.Vencimento.ToString("yyMMddHHmmssZ") },
                {"attributes", certificado.Atributos }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/add?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async Task RemoverOCSP(this Certificado certificado)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", certificado.Serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/remove?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async Task RevogarOCSP(this Certificado certificado)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", certificado.Serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/revoke?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }
        public static async Task DesrevogarOCSP(this Certificado certificado)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", certificado.Serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/unrevoke?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async Task<bool> ValidarOCSP(this Certificado certificado)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", certificado.Serial }
            };
            string response = await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/valid?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return response == "\"1\"";
        }

    }
}