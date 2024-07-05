using System.Globalization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

using PropertyDefinition = Mono.Cecil.PropertyDefinition;

public class MonoDefaultValueParser
{
    public readonly TypeDefinition Type;

    public readonly Dictionary<MemberReference, string> MemberMapping = new();

    public MonoDefaultValueParser(TypeDefinition typeDefinition)
    {
        Type = typeDefinition;

        Build();
    }

    private void Build()
    {
        var defaultConstructor = Type.Methods.FirstOrDefault(x => x.IsConstructor && x is { HasBody: true, HasParameters: false });

        if(defaultConstructor == null)
        {
            return;
        }

        // get all default value of fields or back end fields
        foreach (var instruction in defaultConstructor.Body.Instructions.Where(instruction => instruction.OpCode == OpCodes.Stfld))
        {
            if(instruction.Operand is FieldDefinition field && field.DeclaringType == Type)
            {
                var previousInstruction = instruction.Previous;

                ParseDefaultValue(field, previousInstruction);
            }
        }
    }

    private void ParseDefaultValue(FieldDefinition fieldDefinition, Instruction previousInstruction)
    {
        MemberReference? member = fieldDefinition;
        var fieldType = fieldDefinition.FieldType;

        // should be property back end field?
        if(fieldDefinition.Name.StartsWith('<') && fieldDefinition.Name.EndsWith(">k__BackingField"))
        {
            var propertyName = fieldDefinition.Name.Substring(1, fieldDefinition.Name.IndexOf('>')-1).Trim();
            var property = fieldDefinition.DeclaringType.Properties.FirstOrDefault(x => x.Name == propertyName);
            member = property;

            if(property != null)
            {
                fieldType = property.PropertyType;
            }
        }

        if(member == null)
        {
            Logger.LogD("Failed find real property of : {0}", fieldDefinition);
            return;
        }

        var value = "";
        switch (previousInstruction.OpCode.Code)
        {
            case Code.Ldc_I4_0:                    
            case Code.Ldc_I4_1:
            case Code.Ldc_I4_2:
            case Code.Ldc_I4_3:
            case Code.Ldc_I4_4:
            case Code.Ldc_I4_5:
            case Code.Ldc_I4_6:
            case Code.Ldc_I4_7:
            case Code.Ldc_I4_8:
                value = ((int)previousInstruction.OpCode.Code - (int)Code.Ldc_I4_0).ToString();
                break;
            case Code.Ldc_I4_S:
                value = ((sbyte)previousInstruction.Operand).ToString();
                break;
            case Code.Ldc_I4:
                value = ((int)previousInstruction.Operand).ToString();
                break;
            case Code.Ldc_R4:
                value = ((float)previousInstruction.Operand).ToString(CultureInfo.InvariantCulture);
                break;
            case Code.Ldc_R8:
                value = ((double)previousInstruction.Operand).ToString(CultureInfo.InvariantCulture);
                break;
            case Code.Ldstr:
                value = previousInstruction.Operand?.ToString() ?? "";
                break;
        }

        if(value.IsNotNullOrEmpty())
        {
            var fieldTypeDefinition = fieldType.Resolve();

            if(fieldTypeDefinition.IsEnum)
            {
                var stringValue = MonoTypeDefinitionExtensions.ConvertEnumValueToEnumString(fieldTypeDefinition, long.Parse(value));
                MemberMapping.Add(member, stringValue);
            }
            else
            {
                MemberMapping.Add(member, value);
            }                
        }
    }

    public string? GetDefaultValueText(FieldDefinition fieldDefinition)
    {
        return GetDefaultValueTextInner(fieldDefinition);
    }

    public string? GetDefaultValueText(PropertyDefinition propertyDefinition)
    {
        return GetDefaultValueTextInner(propertyDefinition);
    }

    public string? GetDefaultValueText(MemberReference memberReference)
    {
        return GetDefaultValueTextInner(memberReference);
    }

    private string? GetDefaultValueTextInner(MemberReference memberReference)
    {
        MemberMapping.TryGetValue(memberReference, out var value);
        return value;
    }
}