using CMP.ManipuladorPDF;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Signatures;
using Org.BouncyCastle.Asn1;
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
using CertificateRequest = System.Security.Cryptography.X509Certificates.CertificateRequest;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509CertificateParser = Org.BouncyCastle.X509.X509CertificateParser;
using X509CertificateStructure = Org.BouncyCastle.Asn1.X509.X509CertificateStructure;

namespace CMP.Certificados
{

    public class Certificado
    {
        public byte[] ByteArray { get; set; }
        public string Senha { get; set; } = null;
        public string Serial { get; set; }
        public string Atributos { get; set; }
        public DateTime Vencimento { get; set; }
        public PrivateKeySignature PKS { get; set; } = null;
        public X509Certificate2RSASignature RSASignature { get; set; } = null;
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
                throw new Exception(exception.Message);
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
                throw new CertificateWithoutAliasException();
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

        private static Certificado GetExternalCertificate(string name)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            string distinguishedName = new X500DistinguishedName(name).Name;
            X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, distinguishedName, false);
            if (collection.Count == 0)
                throw new CertificateStoreNotFoundException();

            List<X509CertificateBC> _chain = new List<X509CertificateBC>();
            X509Certificate2 certificate = collection[0];
            X509Certificate _bcCertificate = new X509CertificateParser().ReadCertificate(certificate.GetRawCertData());
            _chain.Add(new X509CertificateBC(_bcCertificate));
            IX509Certificate[] chain = _chain.ToArray();
            X509Certificate2RSASignature signature = new X509Certificate2RSASignature(certificate);

            return new Certificado()
            {
                ByteArray = certificate.GetRawCertData(),
                RSASignature = signature,
                Chain = chain
            };

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

        public static Dictionary<string, string[]> GetExtensionsOIDList(X509Certificate2 certificate)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            
            foreach (X509Extension extension in certificate.Extensions)
            {
                try
                {
                    var inputStream = new Asn1InputStream(extension.RawData).ReadObject();
                    Queue<Asn1Sequence> queue = new Queue<Asn1Sequence>();
                    List<DerObjectIdentifier> objectIdentifiers = new List<DerObjectIdentifier>();
                    if (inputStream is Asn1Sequence)
                    {
                        queue.Enqueue(inputStream as Asn1Sequence);
                    }
                    else if (inputStream is DerObjectIdentifier)
                    {
                        objectIdentifiers.Add(inputStream as DerObjectIdentifier);
                    }
                    while (queue.Any())
                    {
                        Asn1Sequence sequence = queue.Dequeue();
                        objectIdentifiers.AddRange(sequence.OfType<DerObjectIdentifier>());
                        foreach (Asn1Sequence s in sequence.OfType<Asn1Sequence>())
                        {
                            queue.Enqueue(s);
                        }
                    }
                    if (objectIdentifiers.Any())
                    {
                        string[] oids = string.Join("#", objectIdentifiers.Select(j => j.Id)).Split('#');
                        result.Add(extension.Oid.Value, oids);
                    }
                    else
                    {
                        result.Add(extension.Oid.Value, null);
                    }
                }
                catch (Exception exception){}
            }
            return result;
        }

        public static TipoCertificado GetCertificateType(byte[] byteArray)
        {
            TipoCertificado retorno = TipoCertificado.OTHER;

            X509Certificate2 certificate = new X509Certificate2(byteArray);
            Dictionary<string, string[]> oids = GetExtensionsOIDList(certificate);
            string[] policy = oids.Where(x => x.Key == "2.5.29.32").FirstOrDefault().Value;
            if (policy != null)
            {
                if (policy[0].Contains("2.16.76.1.2.3."))
                {
                    retorno = TipoCertificado.A3;
                }
                if (policy[0].Contains("2.16.76.1.2.1."))
                {
                    retorno = TipoCertificado.A1;
                }
            }

            if (
                certificate.Issuer.Contains("O=Camara Municipal de Piracicaba,") ||
                certificate.Issuer.Contains("O=CV Piracicaba,")
            )
            {
                retorno = TipoCertificado.CMP;
            }
            return retorno;
        }
        public enum TipoCertificado
        {
            CMP = 0,
            A1 = 1,
            A2 = 2,
            A3 = 3,
            A4 = 4,
            OTHER = 99
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

        public Certificado() { }

        public Certificado(
            string nome
        )
        {
            Certificado _certificado = GetExternalCertificate(nome);
            ByteArray = _certificado.ByteArray;
            Chain = _certificado.Chain;
            RSASignature = _certificado.RSASignature;
        }

        public Certificado(
            byte[] certificado
        )
        {
            ByteArray = certificado;
            SetAttributes(this);
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

    public static class CertificadoOCSP
    {
        public static async void AdicionarOCSP(
            string serial,
            DateTime vencimento,
            string atributos
        )
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"status", "V" },
                {"serial", serial },
                {"notAfter", vencimento.ToString("yyMMddHHmmssZ") },
                {"attributes", atributos }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/add?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async void RevogarOCSP(string serial)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/revoke?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async void RemoverOCSP(string serial)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/remove?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }
        public static async void DesrevogarOCSP(string serial)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", serial }
            };
            await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/unrevoke?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return;
        }

        public static async Task<bool> ValidarOCSP(string serial)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"serial", serial }
            };
            string response = await APIRequest.Post(PadroesCertificado.OCSP + "/certificate/valid?key=nUZJ85MDV8D52S23Ro65KDqSt9eLaqAs", values);
            return response == "\"1\"";
        }
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

    public class X509Certificate2RSASignature : IExternalSignature
    {
        public X509Certificate2RSASignature(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public X509Certificate[] GetChain()
        {
            var bcCertificate = new X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            return new X509Certificate[] { bcCertificate };
        }

        public string GetSignatureAlgorithmName()
        {
            return "RSA";
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return null;
        }

        public string GetDigestAlgorithmName()
        {
            return "SHA512";
        }

        public byte[] Sign(byte[] message)
        {
            using RSA rsa = certificate.GetRSAPrivateKey();
            if (rsa == null)
                throw new RSAPrivateKeyNotFoundException();

            return rsa.SignData(message, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        X509Certificate2 certificate;
    }

}