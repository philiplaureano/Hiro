using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinFu.Reflection.Emit;
using Mono.Cecil;

namespace Hiro.Compilers
{
    public class FieldBuilder
    {
        public FieldDefinition AddField(TypeDefinition targetType, string fieldName, TypeReference fieldType)
        {
            var module = targetType.Module;

            var targetField = new FieldDefinition(fieldName, fieldType, FieldAttributes.Private);
            targetType.Fields.Add(targetField);

            return targetField;
        }
    }
}
