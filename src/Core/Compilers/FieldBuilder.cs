using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that adds a <see cref="FieldDefinition"/> to a target type.
    /// </summary>
    public class FieldBuilder
    {
        /// <summary>
        /// Adds a <see cref="FieldDefinition"/> to a target type.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldType">The field type.</param>
        /// <returns>The newly-created field.</returns>
        public FieldDefinition AddField(TypeDefinition targetType, string fieldName, TypeReference fieldType)
        {
            var module = targetType.Module;

            var targetField = new FieldDefinition(fieldName, FieldAttributes.Private, fieldType);
            targetType.Fields.Add(targetField);

            return targetField;
        }
    }
}
