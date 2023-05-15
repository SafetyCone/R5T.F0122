using System;
using System.Collections.Generic;
using System.Linq;
using R5T.F0000;
using R5T.F0000.Extensions;
using R5T.F0000.Extensions.ForObject;
using R5T.T0132;
using R5T.T0161;
using R5T.T0161.Extensions;
using R5T.T0170;


namespace R5T.F0122
{
    [FunctionalityMarker]
    public partial interface ITextOutputOperator : IFunctionalityMarker
    {
        public string Format_Method(InstanceDescriptor instanceDescriptor)
        {
            var kindMarkedFullMethodName = instanceDescriptor.KindMarkedFullMemberName.Value.ToKindMarkedFullMethodName();

            var (simplestMethodName, simpleMethodName, namespacedTypedMethodName, namespacedTypedParameterizedMethodName, fullMethodName)
                = Instances.MemberNameOperator.Get_SimplestMethodName(kindMarkedFullMethodName);

            var namespacedTypeName = Instances.MemberNameOperator.Get_NamespacedTypeName(namespacedTypedMethodName);

            var isObsoleteToken = instanceDescriptor.IsObsolete
                // Note: include prefixing space.
                ? " [Obsolete]"
                : Instances.Strings.Empty
                ;

            var outputType = Instances.MemberNameOperator.Get_OutputTypeName(fullMethodName);

            var (parameters, _) = Instances.MemberNameOperator.Get_Parameters(namespacedTypedParameterizedMethodName);

            var parameterTypeNames = parameters
                .Select(parameter => Instances.MemberNameOperator.Get_TypeName(parameter))
                ;

            var output = Instances.StringOperator.New()
                .AppendLine($"{simpleMethodName}{isObsoleteToken}")
                .AppendLine($"{Instances.Characters.Tab}{namespacedTypeName}")
                .AppendLine($"{Instances.Characters.Tab}---")
                .ModifyIf(
                    parameterTypeNames.Any(),
                    stringBuilder => stringBuilder
                        .AppendLines(parameterTypeNames
                            .Select(parameterTypeName => $"{Instances.Characters.Tab}{parameterTypeName}")))
                .AppendLine($"{Instances.Characters.Tab}=>")
                .AppendLine($"{Instances.Characters.Tab}{outputType}")
                .AppendLine(instanceDescriptor.DescriptionXml.Value)
                .AppendLine(instanceDescriptor.ProjectFilePath.Value)
                .ToString();

            return output;
        }

        public string Format_Property(InstanceDescriptor instanceDescriptor)
        {
            var kindMarkedFullPropertyName = instanceDescriptor.KindMarkedFullMemberName.Value.ToKindMarkedFullPropertyName();

            var (simpleTypeName, namespacedTypeName, namespacedTypedPropertyName, fullPropertyName)
                = Instances.MemberNameOperator.Get_SimpleTypeName(kindMarkedFullPropertyName);

            var simplePropertyName = Instances.MemberNameOperator.Get_SimplePropertyName(
                namespacedTypedPropertyName);

            var isObsoleteToken = instanceDescriptor.IsObsolete
                // Note: include prefixing space.
                ? " [Obsolete]"
                : Instances.Strings.Empty
                ;

            var outputType = Instances.MemberNameOperator.Get_OutputTypeName(fullPropertyName);

            var output = Instances.StringOperator.New()
                .AppendLine($"{simplePropertyName}{isObsoleteToken}")
                .AppendLine($"{Instances.Characters.Tab}=> {outputType}")
                .AppendLine($"{Instances.Characters.Tab}{namespacedTypeName}")
                .AppendLine($"{Instances.Characters.Tab}---")
                .ModifyIf(
                    instanceDescriptor.DescriptionXml.Value != null,
                    stringBuilder =>
                    {
                        var description = instanceDescriptor.DescriptionXml.Value.Replace(
                            Instances.Strings.NewLine_NonWindows,
                            Instances.Strings.NewLine_NonWindows + Instances.Strings.Tab);

                    var line = $"{Instances.Characters.Tab}{description}";

                    stringBuilder.AppendLine(line);
                })
                .AppendLine($"{Instances.Characters.Tab}{instanceDescriptor.ProjectFilePath.Value}")
                .ToString();

            return output;
        }

        public string Format_Properties(IEnumerable<InstanceDescriptor> instanceDescriptors)
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
                                    if(instanceInformation.Instance.DescriptionXml.Value != null)
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
