using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json
{
    public class MemberJsonExceptionEx : JsonException
    {
        private readonly List<string> _context;

        public MemberJsonExceptionEx(string member, Exception innerException)
            : base(null, innerException)
        {
            if (innerException is MemberJsonExceptionEx ex)
            {
                _context = new List<string>(ex._context);
            }
            else
            {
                _context = new List<string>();
            }

            _context.Insert(0, member);
        }

        public override string Message
        {
            get
            {
                Exception inner = this;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }

                var path = String.Join(".", _context);
                return $"{path}: {inner.Message}";
            }
        }
    }
}
