using System;
using System.CodeDom.Compiler;
using System.IO;

public class CSSCodeProvider
{
    public static ICodeCompiler CreateCompiler(string sourceFile)
    {
        var sourceFileExtension = Path.GetExtension(sourceFile);
        if (CodeDomProvider.IsDefinedExtension(sourceFileExtension))
        {
            var language = CodeDomProvider.GetLanguageFromExtension(sourceFileExtension);
            var provider = CodeDomProvider.CreateProvider(language);
            return provider.CreateCompiler();
        }
        else
        {
            throw new InvalidOperationException(string.Format("No compiler defined for code files with extension {0}",
                                                              sourceFileExtension));
        }
    }
}