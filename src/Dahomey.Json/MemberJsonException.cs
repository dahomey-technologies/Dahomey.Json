using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json
{
    public class MemberJsonException : JsonException
    {
        public Type MemberType { get; }

        public MemberJsonException(string memberName, Type memberType, Exception innerException)
            : base(BuildMessage(memberName, memberType, innerException),
                  BuildPath(memberName, innerException), null, null, BuildInnerException(innerException))
        {
            MemberType = BuildMemberType(memberType, innerException);
        }

        private static string BuildPath(string memberName, Exception innerException)
        {
            if (innerException is MemberJsonException memberJsonException && memberJsonException.Path !=null)
            {
                return $"$.{memberName}.{memberJsonException.Path.Substring(2)}";
            }
            else
            {
                return $"$.{memberName}";
            }
        }

        private static string BuildMessage(string memberName, Type memberType, Exception innerException)
        {
            var sb = new StringBuilder();
            var rootMemberType = BuildMemberType(memberType, innerException);
            sb.Append("The JSON value could not be converted to ");
            sb.Append(rootMemberType);
            if (rootMemberType.IsPrimitive) {
                sb.Append(".");
            } else {
                sb.Append(" due to: ");
                sb.Append(innerException.Message);
            }
            sb.Append(" Path: ");
            sb.Append(BuildPath(memberName, innerException));
            return sb.ToString();
        }

        private static Type BuildMemberType(Type memberType, Exception innerException)
        {
            if (innerException is MemberJsonException memberJsonException)
            {
                return memberJsonException.MemberType;
            }
            else
            {
                return memberType;
            }
        }

        private static Exception? BuildInnerException(Exception innerException)
        {
            if (innerException is MemberJsonException memberJsonException)
            {
                return memberJsonException.InnerException;
            }
            else
            {
                return innerException;
            }
        }
    }
}
