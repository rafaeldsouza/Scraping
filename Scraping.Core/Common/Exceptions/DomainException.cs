using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping.Core.Common.Exceptions
{
   public class DomainException : Exception
    {
        public DomainException() { }

        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception ex) : base(message,ex) { }
    }
}
