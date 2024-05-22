using System;
using System.Collections.Generic;
using System.Text;

namespace CMP.ManipuladorPDF
{

    public class AssinaturaException : Exception
    {
        public AssinaturaException(){}
        public AssinaturaException(string message) : base(message) {}
        public AssinaturaException(string message, Exception inner) : base(message, inner) {}
    }

    public class FontNotExistException : Exception
    {
        public FontNotExistException() { }
        public FontNotExistException(string message) : base(message) { }
        public FontNotExistException(string message, Exception inner) : base(message, inner) { }
    }

    public class CertificateWrongPasswordException : Exception
    {
        public CertificateWrongPasswordException() { }
    }

    public class CertificateNotFoundException : Exception
    {
        public CertificateNotFoundException() { }
    }

    public class CertificateInvalidException : Exception
    {
        public CertificateInvalidException() { }
    }

    public class FontDirectoryEmptyException : Exception
    {
        public FontDirectoryEmptyException() { }
    }

}
