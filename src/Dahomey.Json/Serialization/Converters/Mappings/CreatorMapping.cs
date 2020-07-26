using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class CreatorMapping : ICreatorMapping
    {
        private readonly IObjectMapping _objectMapping;
        private readonly Delegate _delegate;
        private readonly ParameterInfo[] _parameters;
        private List<ReadOnlyMemory<byte>>? _memberNames = null;
        private List<object?>? _defaultValues = null;

        public IReadOnlyCollection<ReadOnlyMemory<byte>>? MemberNames => _memberNames;

        public CreatorMapping(IObjectMapping objectMapping, ConstructorInfo constructorInfo)
        {
            _objectMapping = objectMapping;
            _delegate = constructorInfo.CreateDelegate();
            _parameters = constructorInfo.GetParameters();
        }

        public CreatorMapping(IObjectMapping objectMapping, Delegate @delegate)
        {
            _objectMapping = objectMapping;
            _delegate = @delegate;
            _parameters = @delegate.Method.GetParameters();
        }

        public CreatorMapping(IObjectMapping objectMapping, MethodInfo method)
        {
            _objectMapping = objectMapping;
            _delegate = method.CreateDelegate();
            _parameters = method.GetParameters();
        }

        public void SetMemberNames(IReadOnlyCollection<ReadOnlyMemory<byte>> memberNames)
        {
            _memberNames = memberNames.ToList();
        }

        public void SetMemberNames(params string[] memberNames)
        {
            _memberNames = memberNames
                .Select(m => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(m)))
                .ToList();
        }

        object ICreatorMapping.CreateInstance(Dictionary<ReadOnlyMemory<byte>, object> values)
        {
            if (_memberNames == null || _defaultValues == null)
            {
                throw new JsonException("Initialize has not been called");
            }

            object?[] args = new object?[_memberNames.Count];

            for (int i = 0; i < _memberNames.Count; i++)
            {
                if (values.TryGetValue(_memberNames[i], out object? value))
                {
                    args[i] = value;
                }
                else
                {
                    args[i] = _defaultValues[i];
                }
            }

            return _delegate.DynamicInvoke(args) ?? throw new JsonException("Cannot instantiate type");
        }

        public void Initialize()
        {
            bool createMemberNames = _memberNames == null;

            IReadOnlyCollection<IMemberMapping> memberMappings = _objectMapping.MemberMappings;
            if (_memberNames == null)
            {
                _memberNames = new List<ReadOnlyMemory<byte>>(_parameters.Length);
            }
            else if (_memberNames.Count != _parameters.Length)
            {
                throw new JsonException("Size mismatch between creator parameters and member names");
            }

            _defaultValues = new List<object?>(_parameters.Length);

            for (int i = 0; i < _parameters.Length; i++)
            {
                ParameterInfo parameter = _parameters[i];
                IMemberMapping? memberMapping;

                if (createMemberNames)
                {
                    // Infer JSON property names: .ctor parameter --> CLR property (MemberInfo.Name) --> JSON property (MemberName)
                    // Assumes .ctor parameter name is using the same name as CLR property. They can differ in casing.
                    memberMapping = memberMappings
                        .FirstOrDefault(m => string.Compare(m.MemberInfo?.Name ?? m.MemberName, parameter.Name, StringComparison.OrdinalIgnoreCase) == 0);

                    if (memberMapping == null || memberMapping.MemberName == null)
                    {
                        _memberNames.Add(new ReadOnlyMemory<byte>());
                    }
                    else
                    {
                        _memberNames.Add(new ReadOnlyMemory<byte>(Encoding.ASCII.GetBytes(memberMapping.MemberName)));
                    }
                }
                else
                {
                    // JsonConstructorExAttribute present: .ctor parameter --> JSON property (Specified in JsonConstructorExAttribute)
                    string memberName = Encoding.UTF8.GetString(_memberNames[i].Span);
                    memberMapping = memberMappings
                        .FirstOrDefault(m => string.Compare(m.MemberName, memberName, StringComparison.OrdinalIgnoreCase) == 0);

                    if (memberMapping == null)
                    {
                        throw new JsonException($"Cannot find a field or property named {memberName} on type {_objectMapping.ObjectType.FullName}");
                    }
                }

                if (memberMapping != null)
                {
                    if (memberMapping.MemberType != parameter.ParameterType)
                    {
                        throw new JsonException($"Type mismatch between creator argument and field or property named {parameter.Name} on type {_objectMapping.ObjectType.FullName}");
                    }

                    _defaultValues.Add(memberMapping.DefaultValue);
                }
                else
                {
                    _defaultValues.Add(parameter.ParameterType.GetDefaultValue());
                }
            }
        }

        public void PostInitialize()
        {
        }
    }
}
