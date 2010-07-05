using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Cecil.Cil;

namespace Hiro
{
    /// <summary>
    /// A class that extends the <see cref="ILProcessor"/> class
    /// with helper methods that make it easier to save
    /// information about the method currently being implemented.
    /// </summary>
    public static class ILProcessorExtensions
    {
        /// <summary>
        /// Emits a Console.WriteLine call to using the current ILProcessor that will only be called if the contents
        /// of the target variable are null at runtime.
        /// </summary>
        /// <param name="IL">The target ILProcessor.</param>
        /// <param name="text">The text that will be written to the console.</param>
        /// <param name="targetVariable">The target variable that will be checked for null at runtime.</param>
        public static void EmitWriteLineIfNull(this ILProcessor IL, string text, VariableDefinition targetVariable)
        {
            var skipWrite = IL.Create(OpCodes.Nop);
            IL.Emit(OpCodes.Ldloc, targetVariable);
            IL.Emit(OpCodes.Brtrue, skipWrite);
            IL.EmitWriteLine(text);
            IL.Append(skipWrite);
        }

        /// <summary>
        /// Emits a Console.WriteLine call using the current ILProcessor.
        /// </summary>
        /// <param name="IL">The target ILProcessor.</param>
        /// <param name="text">The text that will be written to the console.</param>
        public static void EmitWriteLine(this ILProcessor IL, string text)
        {
            var body = IL.Body;
            var method = body.Method;
            var declaringType = method.DeclaringType;
            var module = declaringType.Module;

            var writeLineMethod = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            IL.Emit(OpCodes.Ldstr, text);
            IL.Emit(OpCodes.Call, module.Import(writeLineMethod));
        }
    }
}
