// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace PageGenerator
{
    public class Program
    {
        private const int NumArgs = 3;

        public void Main(string[] args)
        {
            if (args.Length != NumArgs)
            {
                throw new ArgumentException(string.Format("Requires {0} argument (Namespace, Base Class, Views Directory), {1} given", NumArgs, args.Length));
            }
            var @namespace = args[0];
            var defaultBaseClass = args[1];
            var viewDir = args[2];

            var fileCount = 0;
            Console.WriteLine();

            if (!Directory.Exists(viewDir))
            {
                Console.WriteLine("Directory could not be found.");
                Console.WriteLine("{0} files successfully generated.", fileCount);
                return;
            }

            Console.WriteLine("  Generating code files for views in {0}", viewDir);

            var cshtmlFiles = Directory.EnumerateFiles(viewDir, "*.cshtml");

            if (!cshtmlFiles.Any())
            {
                Console.WriteLine("  No .cshtml files were found.");
            }

            foreach (var fileName in cshtmlFiles)
            {
                Console.WriteLine("    Generating code file for view {0}...", Path.GetFileName(fileName));
                GenerateCodeFile(fileName, @namespace, defaultBaseClass);
                Console.WriteLine("      Done!");
                fileCount++;
            }

            Console.WriteLine();
            Console.WriteLine("{0} files successfully generated.", fileCount);
            Console.WriteLine();
        }

        private static void GenerateCodeFile(string cshtmlFilePath, string rootNamespace, string defaultBaseClass)
        {
            var basePath = Path.GetDirectoryName(cshtmlFilePath);
            var fileName = Path.GetFileName(cshtmlFilePath);
            var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);
            var codeLang = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(codeLang);
            
            host.GeneratedClassContext = new GeneratedClassContext(
                executeMethodName: GeneratedClassContext.DefaultExecuteMethodName,
                writeMethodName: GeneratedClassContext.DefaultWriteMethodName,
                writeLiteralMethodName: GeneratedClassContext.DefaultWriteLiteralMethodName,
                writeToMethodName: "WriteTo",
                writeLiteralToMethodName: "WriteLiteralTo",
                templateTypeName: "HelperResult",
                defineSectionMethodName: "DefineSection",
                generatedTagHelperContext: new GeneratedTagHelperContext());
            var engine = new RazorTemplateEngine(host);

            var source = File.ReadAllText(cshtmlFilePath);

            var modelType = GetModelType(source);
            if (!string.IsNullOrEmpty(modelType))
            {
                host.DefaultBaseClass = $"{defaultBaseClass}<{modelType}>"; // BaseClass<T>
            }
            else
            {
                host.DefaultBaseClass = defaultBaseClass;
            }

            source = RemoveAnnotations(source, rootNamespace);

            using (var fileStream = File.OpenText(cshtmlFilePath))
            {
                var code = engine.GenerateCode(
                    input: new StringReader(source),
                    className: fileNameNoExtension,
                    rootNamespace: rootNamespace,
                    sourceFileName: fileName);

                var output = code.GeneratedCode;
                output = InlineIncludedFiles(basePath, output);
                File.WriteAllText(Path.Combine(basePath, string.Format("{0}.cs", fileNameNoExtension)), output);
            }
        }

        private static string GetModelType(string source)
        {
            var firstLine = source.Substring(0, source.IndexOf(Environment.NewLine));

            if (!firstLine.StartsWith("@model ", StringComparison.Ordinal))
            {
                return null;
            }

            var modelType = firstLine.Substring(7);
            return modelType;
        }

        private static string RemoveAnnotations(string source, string rootNamespace)
        {
            var removeLinesStartingWith = new string[]
            {
                "/*ignore*/",
                "@model",
                $"@using {rootNamespace}"
            };

            var removeBlocks = new string[]
            {
                $"@functions {{{Environment.NewLine}}}"
            };

            var builder = new StringBuilder();

            var removedAnnotatedLines = source
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(line => removeLinesStartingWith.All(s => !line.Trim().StartsWith(s)))
                .Aggregate(builder, (b, s) => b.AppendLine(s))
                .ToString();

            var removedBlocks = removeBlocks
                .Aggregate(removedAnnotatedLines, (working, @this) => working.Replace(@this, string.Empty))
                .Trim();

            return removedBlocks;
        }

        private static string InlineIncludedFiles(string basePath, string source)
        {
            var startIndex = 0;
            while (startIndex < source.Length)
            {
                var startMatch = @"<%$ include: ";
                var endMatch = @" %>";
                startIndex = source.IndexOf(startMatch, startIndex);
                if (startIndex == -1)
                {
                    break;
                }
                var endIndex = source.IndexOf(endMatch, startIndex);
                if (endIndex == -1)
                {
                    break;
                }
                var includeFileName = source.Substring(startIndex + startMatch.Length, endIndex - (startIndex + startMatch.Length));
                includeFileName = SanitizeFileName(includeFileName);
                Console.WriteLine("      Inlining file {0}", includeFileName);
                var replacement = File.ReadAllText(Path.Combine(basePath, includeFileName)).Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                source = source.Substring(0, startIndex) + replacement + source.Substring(endIndex + endMatch.Length);
                startIndex = startIndex + replacement.Length;
            }

            return source;
        }

        private static string SanitizeFileName(string fileName)
        {
            // The Razor generated code sometimes splits strings across multiple lines
            // which can hit the include file name, so we need to strip out the non-filename chars.
            //ErrorPage.j" +
            //"s

            var invalidChars = new List<char>(Path.GetInvalidFileNameChars());
            Console.WriteLine($"InvalidChars are {invalidChars}");
            invalidChars.Add('+');
            invalidChars.Add(' ');
            //These are already in the list on windows, but for other platforms
            //it seems like some of them are missing, so we add them explicitly
            invalidChars.Add('"');
            invalidChars.Add('\'');
            invalidChars.Add('\r');
            invalidChars.Add('\n');

            return string.Join(string.Empty, fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        }
    }
}
