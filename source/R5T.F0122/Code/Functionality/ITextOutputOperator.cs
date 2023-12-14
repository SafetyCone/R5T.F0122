using System;
using System.Collections.Generic;
using System.Linq;

using R5T.F0000;
using R5T.F0000.Extensions;
using R5T.F0000.Extensions.ForObject;
using R5T.F0121;
using R5T.L0065.T000;
using R5T.T0132;
using R5T.T0161;
using R5T.T0161.Extensions;
using R5T.T0170;

using InstanceDescriptor = R5T.T0170.InstanceDescriptor;


namespace R5T.F0122
{
    [FunctionalityMarker]
    public partial interface ITextOutputOperator : IFunctionalityMarker
    {
        public string Format(IEnumerable<InstanceDescriptor> instanceDescriptors)
        {
            var texts = instanceDescriptors
                .Select(instanceDescriptor =>
                {
                    var kind = Instances.MemberNameOperator.Get_KindMarker(instanceDescriptor.IdentityString.Value.ToKindMarkedFullMemberName());

                    var text = kind.Value switch
                    {
                        IKindMarkers.Property_Constant => this.Format_Property(instanceDescriptor),
                        IKindMarkers.Method_Constant => this.Format_Method(instanceDescriptor),
                        _ => throw new Exception($"Unhandled kind: {kind}")
                    };

                    return text;
                })
                ;

            var text = Instances.TextOperator.Join_Lines(texts);
            return text;
        }

        public string Format_Method(InstanceDescriptor instanceDescriptor)
        {
            var signature = Instances.SignatureStringOperator.Get_Signature(instanceDescriptor.SignatureString);

            var methodSignature = signature as MethodSignature;

            var output = this.Format_Method_ToString(instanceDescriptor, methodSignature);
            return output;
        }

        public string Format_PropertiesAndMethods(IEnumerable<InstanceDescriptor> instances)
        {
            var declaringTypeInstances = Instances.Operations.Enumerate_SignaturesAndInstancesAs<IHasDeclaringType>(instances);

            var groupedByDeclaringType = Instances.Operations.Group_ByDeclaringType(declaringTypeInstances);

            static IEnumerable<string> Format_Property(InstanceDescriptor instance, PropertySignature signature)
            {
                var isObsoleteToken = instance.IsObsolete
                    // Note: include prefixing space.
                    ? " [Obsolete]"
                    : Instances.Strings.Empty
                    ;

                var propertyTypeSignature = Instances.SignatureOperator.Get_SignatureStringValue(signature.PropertyType);

                var output = Instances.EnumerableOperator.From($"{Instances.Characters.Tab}{signature.PropertyName}{isObsoleteToken} ~{propertyTypeSignature}");

                // Append the XML documentation, if it exists.
                if (instance.DescriptionXml.Value != null)
                {
                    var indentedDocumentation = $"{Instances.Characters.Tab}{Instances.Characters.Tab}{instance.DescriptionXml.Value}"
                        // Handle new lines in the input.
                        .Replace(
                            Instances.Strings.NewLine_NonWindows,
                            $"{Instances.Characters.Tab}{Instances.Characters.Tab}"
                        )
                        ;

                    output = output.Append(indentedDocumentation);
                }

                return output;
            }

            static IEnumerable<string> Format_Method(InstanceDescriptor instance, MethodSignature signature)
            {
                var isObsoleteToken = instance.IsObsolete
                    // Note: include prefixing space.
                    ? " [Obsolete]"
                    : Instances.Strings.Empty
                    ;

                var returnTypeSignature = Instances.SignatureOperator.Get_SignatureStringValue(signature.ReturnType);

                var parameterTypeNameLines = signature.Parameters
                    .Select(parameter => Instances.SignatureOperator.Get_SignatureStringValue(parameter.ParameterType))
                    .Select(value => $"{Instances.Characters.Tab}{Instances.Characters.Tab}{value}")
                    ;

                var output = Instances.EnumerableOperator.Empty<string>()
                    .Append($"{Instances.Characters.Tab}{signature.MethodName}{isObsoleteToken}")
                    .AppendIf(parameterTypeNameLines.Any(),
                        Instances.EnumerableOperator.From($"{Instances.Characters.Tab}(")
                            .Append(parameterTypeNameLines)
                            .Append($"{Instances.Characters.Tab})")
                    )
                    .AppendIf(parameterTypeNameLines.None(),
                        "()")
                    .Append($"{Instances.Characters.Tab}=>")
                    .Append($"{Instances.Characters.Tab}{returnTypeSignature}")
                    ;

                // Append the XML documentation, if it exists.
                if (instance.DescriptionXml.Value != null)
                {
                    var indentedDocumentation = $"{Instances.Characters.Tab}{Instances.Characters.Tab}{instance.DescriptionXml.Value}"
                        // Handle new lines in the input.
                        .Replace(
                            Instances.Strings.NewLine_NonWindows,
                            $"{Instances.Characters.Tab}{Instances.Characters.Tab}"
                        )
                        ;

                    output = output.Append(indentedDocumentation);
                }

                return output;
            }

            var output = Instances.StringOperator.New()
                .AppendLines(groupedByDeclaringType
                    .OrderAlphabetically(group =>
                    {
                        var typeIdentityString = Instances.SignatureOperator.Get_IdentityString(group.Key);
                        var projectFilePath = group.First().Instance.ProjectFilePath;

                        var key = $"{projectFilePath}:{typeIdentityString}";
                        return key;
                    })
                    .SelectMany(group =>
                    {
                        var typeIdentityString = Instances.SignatureOperator.Get_IdentityString(group.Key);
                        var projectFilePath = group.First().Instance.ProjectFilePath;

                        var isFirst = true;

                        var output = Instances.EnumerableOperator.From(group.Key.TypeName)
                            .Append($"{Instances.Characters.Tab}{typeIdentityString}")
                            .Append($"{Instances.Characters.Tab}---")
                            .Append(group
                                .OrderAlphabetically(pair =>
                                {
                                    var output = pair.Signature switch
                                    {
                                        // Sort properties first.
                                        PropertySignature propertySignature => "1" + propertySignature.PropertyName,
                                        MethodSignature methodSignature => "2" + methodSignature.MethodName,
                                        // Only handled properties and methods.
                                        _ => throw Instances.SwitchOperator.Get_UnrecognizedSwitchTypeExpression(pair.Signature)
                                    };

                                    return output;
                                })
                                .SelectMany(pair =>
                                {
                                    var output = pair.Signature switch
                                    {
                                        PropertySignature propertySignature => Format_Property(pair.Instance, propertySignature),
                                        MethodSignature methodSignature => Format_Method(pair.Instance, methodSignature),
                                        // Only handled properties and methods.
                                        _ => throw Instances.SwitchOperator.Get_UnrecognizedSwitchTypeExpression(pair.Signature)
                                    };

                                    if (isFirst)
                                    {
                                        isFirst = false;
                                    }
                                    else
                                    {
                                        output = output.Prepend(Instances.Strings.Empty);
                                    }

                                    return output;
                                }))
                            .Append($"{Instances.Characters.Tab}---")
                            .Append($"{Instances.Characters.Tab}{projectFilePath}")
                            .Append(Instances.Strings.Empty)
                            ;

                        return output;
                    }))
                .ToString();

            return output;
        }

        public IEnumerable<string> Format_Method_ToLines(InstanceDescriptor instanceDescriptor, MethodSignature methodSignature)
        {
            //var kindMarkedFullMethodName = instanceDescriptor.IdentityString.Value.ToKindMarkedFullMethodName();

            //var (simplestMethodName, simpleMethodName, namespacedTypedMethodName, namespacedTypedParameterizedMethodName, fullMethodName)
            //    = Instances.MemberNameOperator.Get_SimplestMethodName(kindMarkedFullMethodName);

            //var namespacedTypeName = Instances.MemberNameOperator.Get_NamespacedTypeName(namespacedTypedMethodName);

            var typeSignatureString = Instances.SignatureOperator.Get_SignatureStringValue(methodSignature.DeclaringType);
            var returnTypeSignatureString = Instances.SignatureOperator.Get_SignatureStringValue(methodSignature.ReturnType);

            var isObsoleteToken = instanceDescriptor.IsObsolete
                // Note: include prefixing space.
                ? " [Obsolete]"
                : Instances.Strings.Empty
                ;

            //var outputType = Instances.MemberNameOperator.Get_OutputTypeName(fullMethodName);

            //var (parameters, _) = Instances.MemberNameOperator.Get_Parameters(namespacedTypedParameterizedMethodName);

            var parameterTypeNames = methodSignature.Parameters
                .Select(parameter => Instances.SignatureOperator.Get_SignatureStringValue(parameter.ParameterType))
                //.Select(parameter => Instances.ParameterOperator.Get_TypeName(parameter))
                ;

            var output = Instances.EnumerableOperator.Empty<string>()
                .Append($"{methodSignature.MethodName}{isObsoleteToken}")
                //.AppendLine($"{simpleMethodName}{isObsoleteToken}")
                .Append($"{Instances.Characters.Tab}{typeSignatureString}")
                //.AppendLine($"{Instances.Characters.Tab}{namespacedTypeName}")
                .Append($"{Instances.Characters.Tab}---")
                .ModifyIf(
                    parameterTypeNames.Any(),
                    enumerable => enumerable
                        .Append(parameterTypeNames
                            .Select(parameterTypeName => $"{Instances.Characters.Tab}{parameterTypeName}")))
                .Append($"{Instances.Characters.Tab}=>")
                .Append($"{Instances.Characters.Tab}{returnTypeSignatureString}")
                //.AppendLine($"{Instances.Characters.Tab}{outputType}")
                .Append(instanceDescriptor.DescriptionXml.Value)
                .Append(instanceDescriptor.ProjectFilePath.Value)
                ;

            return output;
        }

        public string Format_Method_ToString(InstanceDescriptor instanceDescriptor, MethodSignature methodSignature)
        {
            var lines = this.Format_Method_ToLines(instanceDescriptor, methodSignature);

            var output = Instances.StringOperator.New()
                .AppendLines(lines)
                .ToString();

            return output;
        }

        public string Format_Methods(IEnumerable<InstanceDescriptor> instanceDescriptors)
        {
            var texts = instanceDescriptors
                .Select(x => this.Format_Method(x))
                ;

            var text = Instances.TextOperator.Join_Lines(texts);
            return text;
        }

        public string Format_Property(InstanceDescriptor instance)
        {
            var signature = Instances.SignatureStringOperator.Get_Signature(instance.SignatureString);

            var propertySignature = signature as PropertySignature;

            var isObsoleteToken = instance.IsObsolete
                    // Note: include prefixing space.
                    ? " [Obsolete]"
                    : Instances.Strings.Empty
                    ;

            var propertyTypeSignature = Instances.SignatureOperator.Get_SignatureStringValue(propertySignature.PropertyType);

            var declaringTypeSignature = Instances.SignatureOperator.Get_SignatureStringValue(propertySignature.DeclaringType);

            //var kindMarkedFullPropertyName = instance.IdentityString.Value.ToKindMarkedFullPropertyName();

            //var (simpleTypeName, namespacedTypeName, namespacedTypedPropertyName, fullPropertyName)
            //    = Instances.MemberNameOperator.Get_SimpleTypeName(kindMarkedFullPropertyName);

            //var simplePropertyName = Instances.MemberNameOperator.Get_SimplePropertyName(
            //    namespacedTypedPropertyName);

            //var outputType = Instances.MemberNameOperator.Get_OutputTypeName(fullPropertyName);

            var output = Instances.StringOperator.New()
                //.AppendLine($"{simplePropertyName}{isObsoleteToken}")
                .AppendLine($"{propertySignature.PropertyName}{isObsoleteToken}")
                //.AppendLine($"{Instances.Characters.Tab}=> {outputType}")
                .AppendLine($"{Instances.Characters.Tab}=> {propertyTypeSignature}")
                //.AppendLine($"{Instances.Characters.Tab}{namespacedTypeName}")
                .AppendLine($"{Instances.Characters.Tab}{declaringTypeSignature}")
                .AppendLine($"{Instances.Characters.Tab}---")
                .ModifyIf(
                    instance.DescriptionXml.Value != null,
                    stringBuilder =>
                    {
                        var description = instance.DescriptionXml.Value.Replace(
                            Instances.Strings.NewLine_NonWindows,
                            Instances.Strings.NewLine_NonWindows + Instances.Strings.Tab);

                        var line = $"{Instances.Characters.Tab}{description}";

                        stringBuilder.AppendLine(line);
                    })
                .AppendLine($"{Instances.Characters.Tab}{instance.ProjectFilePath.Value}")
                .ToString();

            return output;
        }

        public string Format_Properties(IEnumerable<InstanceDescriptor> instances)
        {
            var propertySignatureInstances = Instances.Operations.Enumerate_SignaturesAndInstancesAs<PropertySignature>(instances);

            var groupedByDeclaringType = Instances.Operations.Group_ByDeclaringType(propertySignatureInstances);

            var output = Instances.StringOperator.New()
                .AppendLines(groupedByDeclaringType
                    .OrderAlphabetically(group =>
                    {
                        var typeIdentityString = Instances.SignatureOperator.Get_IdentityString(group.Key);
                        var projectFilePath = group.First().Instance.ProjectFilePath;

                        var key = $"{projectFilePath}:{typeIdentityString}";
                        return key;
                    })
                    .SelectMany(group =>
                    {
                        var typeIdentityString = Instances.SignatureOperator.Get_IdentityString(group.Key);
                        var projectFilePath = group.First().Instance.ProjectFilePath;

                        var output = Instances.EnumerableOperator.From(group.Key.TypeName)
                            .Append($"{Instances.Characters.Tab}{typeIdentityString}")
                            .Append($"{Instances.Characters.Tab}---")
                            .Append(group
                                .OrderAlphabetically(pair => pair.Signature.PropertyName)
                                .SelectMany(pair =>
                                {
                                    var isObsoleteToken = pair.Instance.IsObsolete
                                        // Note: include prefixing space.
                                        ? " [Obsolete]"
                                        : Instances.Strings.Empty
                                        ;

                                    var propertyTypeSignature = Instances.SignatureOperator.Get_SignatureString(pair.Signature.PropertyType);

                                    var output = Instances.EnumerableOperator.From($"{Instances.Characters.Tab}{pair.Signature.PropertyName}{isObsoleteToken} ~{propertyTypeSignature}");

                                    // Append the XML documentation, if it exists.
                                    if (pair.Instance.DescriptionXml.Value != null)
                                    {
                                        var indentedDocumentation = $"{Instances.Characters.Tab}{Instances.Characters.Tab}{pair.Instance.DescriptionXml.Value}"
                                            // Handle new lines in the input.
                                            .Replace(
                                                Instances.Strings.NewLine_NonWindows,
                                                $"{Instances.Characters.Tab}{Instances.Characters.Tab}"
                                            )
                                            ;

                                        output = output.Append(indentedDocumentation);
                                    }

                                    return output;
                                }))
                            .Append($"{Instances.Characters.Tab}---")
                            .Append($"{Instances.Characters.Tab}{projectFilePath}")
                            .Append(Instances.Strings.NewLine)
                            ;

                        return output;
                    }))
                .ToString();

            return output;
        }

        public string Format_Properties_Old(IEnumerable<InstanceDescriptor> instanceDescriptors)
        {
            var propertiesAndDeclaringType = Instances.Operations.Group_PropertiesAndDeclaringType(instanceDescriptors);

            var output = Instances.StringOperator.New()
                .AppendLines(propertiesAndDeclaringType
                    .OrderAlphabetically(pair => pair.ContainingTypeInformation.Key)
                    .SelectMany(pair =>
                    {
                        var output = Instances.EnumerableOperator.From(pair.ContainingTypeInformation.SimpleTypeName.Value)
                            .Append($"{Instances.Characters.Tab}{pair.ContainingTypeInformation.NamespacedTypeName.Value}")
                            .Append($"{Instances.Characters.Tab}---")
                            .Append(pair.InstancesInformation
                                .OrderAlphabetically(instanceInformation => instanceInformation.SimplePropertyName.Value)
                                .SelectMany(instanceInformation =>
                                {
                                    var isObsoleteToken = instanceInformation.Instance.IsObsolete
                                        // Note: include prefixing space.
                                        ? " [Obsolete]"
                                        : Instances.Strings.Empty
                                        ;

                                    var output = Instances.EnumerableOperator.From($"{Instances.Characters.Tab}{instanceInformation.SimplePropertyName.Value}{isObsoleteToken}");

                                    // Append the XML documentation, if it exists.
                                    if (instanceInformation.Instance.DescriptionXml.Value != null)
                                    {
                                        var indentedDocumentation = $"{Instances.Characters.Tab}{Instances.Characters.Tab}{instanceInformation.Instance.DescriptionXml.Value}"
                                            // Handle new lines in the input.
                                            .Replace(
                                                Instances.Strings.NewLine_NonWindows,
                                                $"{Instances.Characters.Tab}{Instances.Characters.Tab}"
                                            )
                                            ;

                                        output = output.Append(indentedDocumentation);
                                    }

                                    return output;
                                }))
                            .Append($"{Instances.Characters.Tab}---")
                            .Append($"{Instances.Characters.Tab}{pair.ContainingTypeInformation.ProjectFilePath}")
                            .Append(Instances.Strings.NewLine)
                            ;

                        return output;
                    }))
                .ToString();

            return output;
        }
    }
}
