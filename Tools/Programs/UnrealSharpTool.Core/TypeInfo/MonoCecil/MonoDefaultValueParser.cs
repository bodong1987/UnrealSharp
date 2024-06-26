using Mono.Cecil;
using Mono.Cecil.Cil;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil
{
    using PropertyDefinition = Mono.Cecil.PropertyDefinition;

    public class MonoDefaultValueParser
    {
        public readonly TypeDefinition Type;

        public readonly Dictionary<MemberReference, string> MemberMapping = new Dictionary<MemberReference, string>();

        public MonoDefaultValueParser(TypeDefinition typeDefinition)
        {
            Type = typeDefinition;

            Build();
        }

        private void Build()
        {
            var DefaultConstructor = Type.Methods.FirstOrDefault(x => x.IsConstructor && x.HasBody && !x.HasParameters);

            if(DefaultConstructor == null)
            {
                return;
            }

            // get all default value of fields or back end fields
            foreach (var instruction in DefaultConstructor.Body.Instructions)
            {
                if(instruction.OpCode == OpCodes.Stfld)
                {
                    FieldDefinition? field = instruction.Operand as FieldDefinition;

                    if(field != null && field.DeclaringType == Type)
                    {
                        Instruction previousInstruction = instruction.Previous;

                        ParseDefaultValue(field, previousInstruction);
                    }                    
                }
            }
        }

        private void ParseDefaultValue(FieldDefinition fieldDefinition, Instruction previousInstruction)
        {
            MemberReference? member = fieldDefinition;
            TypeReference fieldType = fieldDefinition.FieldType;

            // should be property back end field?
            if(fieldDefinition.Name.StartsWith("<") && fieldDefinition.Name.EndsWith(">k__BackingField"))
            {
                string propertyName = fieldDefinition.Name.Substring(1, fieldDefinition.Name.IndexOf('>')-1).Trim();
                var Property = fieldDefinition.DeclaringType.Properties.FirstOrDefault(x => x.Name == propertyName);
                member = Property;

                if(Property != null)
                {
                    fieldType = Property.PropertyType;
                }
            }

            if(member == null)
            {
                Logger.LogD("Failed find real property of : {0}", fieldDefinition);
                return;
            }

            string value = "";
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
                    value = ((float)previousInstruction.Operand).ToString();
                    break;
                case Code.Ldc_R8:
                    value = ((double)previousInstruction.Operand).ToString();
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
                    string stringValue = MonoTypeDefinitionExtensions.ConvertEnumValueToEnumString(fieldTypeDefinition, Int64.Parse(value));
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
}
