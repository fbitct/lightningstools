using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.JScript;

public class CSSCodeProvider
{
    public static ICodeCompiler CreateCompiler(string sourceFile)
    {
        string sourceFileExtension = Path.GetExtension(sourceFile);
        if (CodeDomProvider.IsDefinedExtension(sourceFileExtension))
        {
            string language = CodeDomProvider.GetLanguageFromExtension(sourceFileExtension);
            CodeDomProvider provider = CodeDomProvider.CreateProvider(language);
            return provider.CreateCompiler();
        }
        else
        {
            throw new InvalidOperationException(string.Format("No compiler defined for code files with extension {0}", sourceFileExtension));
        }
    }
}

