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
}
