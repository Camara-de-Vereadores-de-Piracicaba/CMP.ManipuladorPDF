using CMP.ManipuladorPDF;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using SysadminsLV.Asn1Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace CMP.Certificados
{

    public class Certificado
    {
        public byte[] ByteArray { get; set; }
        public string Senha { get; set; }
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

            if(alias == null)
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

        public static void ExpandChain(Certificado certificado)
        {
            Stream stream = new MemoryStream(File.ReadAllBytes(PadroesCertificado.CaminhoCA));
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, PadroesCertificado.SenhaCA.ToCharArray());
            string alias = GetAlias(store);
            X509CertificateEntry[] certificateEntries = store.GetCertificateChain(alias);
            List<X509CertificateBC> _rootChain = new List<X509CertificateBC>();
            _rootChain.Add(new X509CertificateBC(certificateEntries[0].Certificate));
            IX509Certificate[] cachain = _rootChain.ToArray();
            List<IX509Certificate> _chain = new List<IX509Certificate>();
            _chain.Add(certificado.Chain[0]);
            _chain.Add(cachain[0]);
            IX509Certificate[] chain = _chain.ToArray();
            certificado.Chain = chain;
        }

        internal byte[] GetSerial()
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

        internal string GetCertificateAttributes(
            string CN,
            string E,
            string L = "Piracicaba",
            string ST = "SP",
            string C = "BR",
            string O = "CV Piracicaba",
            string DC = "camarapiracicaba.sp.gov.br"
        )
        {
            string CA = $@"L={L},ST={ST},C={C},E={E},O={O},";
            CA += string.Join(",", DC.Split('.').Select(w => String.Format("DC={0}", w)));
            CA += $",CN={CN}";
            return CA;
        }

        internal void SetPKSAndChain(Certificado certificado)
        {
            PKS = GetPKS(certificado);
            Chain = GetChain(certificado);
            ExpandChain(certificado);
        }

        private byte[] GerarCertificado(
            Certificado raiz,
            string nome,
            string email,
            string senha,
            int tempoExpiracao = 9
        )
        {
            X509Certificate2 certificadoRaiz = new X509Certificate2(raiz.ByteArray, raiz.Senha);
            RSA rsaKey = RSA.Create();
            string dadosCertificado = GetCertificateAttributes(nome, email);
            CertificateRequest certificateRequest = new CertificateRequest(dadosCertificado, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            X509Extension ocspExtension = OCSPExtension(PadroesCertificado.OCSP);
            X509Extension crlExtension = CRLExtension(PadroesCertificado.CRL);
            certificateRequest.CertificateExtensions.Add(ocspExtension);
            certificateRequest.CertificateExtensions.Add(crlExtension);
            byte[] serial = GetSerial();
            DateTimeOffset notBefore = DateTimeOffset.UtcNow;
            DateTimeOffset notAfter = notBefore.AddYears(tempoExpiracao);
            X509Certificate2 certificado = certificateRequest.Create(certificadoRaiz, notBefore, notAfter, serial).CopyWithPrivateKey(rsaKey);
            SecureString secureString = GetSecureString(senha);
            return certificado.Export(X509ContentType.Pfx, secureString);
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
            SetPKSAndChain(this);
        }

        public Certificado(
            string certificado,
            string senha
        )
        {
            try
            {
                ByteArray = File.ReadAllBytes(certificado);
            }catch ( Exception exception) {
                if (exception.Message.Contains("Could not find file"))
                {
                    throw new CertificateNotFoundException();
                }
            }

            Senha = senha;
            SetPKSAndChain(this);
        }

        public Certificado(
            MemoryStream certificado,
            string senha
        )
        {
            ByteArray = certificado.ToArray();
            Senha = senha;
            SetPKSAndChain(this);
        }

        public Certificado(
            Certificado raiz,
            string nome,
            string email,
            string senha
        )
        {
            ByteArray = GerarCertificado(raiz, nome, email, senha);
            Senha = senha;
            SetPKSAndChain(this);
        }

    }


    public static class PadroesCertificado
    {
        public static string CaminhoCA { get; set; } = "C:\\arquivos\\certificados\\ca.pfx";
        public static string SenhaCA { get; set; } = "C@m@r@1025";
        public static string OCSP { get; set; } = "http://localhost:51576";
        public static string CRL { get; set; } = "http://localhost:51576";
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

    }

}
