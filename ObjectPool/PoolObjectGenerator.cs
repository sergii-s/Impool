using System;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ObjectPool
{
    public class PoolObjectGenerator : ICodeGenerator
    {
        private readonly AttributeData _attributeData;

        public PoolObjectGenerator(AttributeData attributeData)
        {
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            // Our generator is applied to any class that our attribute is applied to.
            var applyToClass = (ClassDeclarationSyntax)context.ProcessingNode;

            // Apply a suffix to the name of a copy of the class.
            var copy = applyToClass
                .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + "toto"));

            // Return our modified copy. It will be added to the user's project for compilation.
            results = results.Add(copy);
            return Task.FromResult<SyntaxList<MemberDeclarationSyntax>>(results);
        }
    }
}