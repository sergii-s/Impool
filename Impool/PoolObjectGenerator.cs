using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Impool
{
    public class PoolObjectGenerator : ICodeGenerator
    {
        private readonly AttributeData _attributeData;

        public PoolObjectGenerator(AttributeData attributeData)
        {
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context,
            IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            var @class = (TypeDeclarationSyntax) context.ProcessingNode;

            var constructor = (ConstructorDeclarationSyntax)
                @class.ChildNodes().First(n => n.Kind() == SyntaxKind.ConstructorDeclaration);

            var classPartial = ClassPartial(constructor, @class);
            var objectPoolExtension = PoolExtension(constructor, @class);

            // Return our modified copy. It will be added to the user's project for compilation.
            results = results.Add(classPartial);
            results = results.Add(objectPoolExtension);
            return Task.FromResult<SyntaxList<MemberDeclarationSyntax>>(results);
        }

        private static TypeDeclarationSyntax ClassPartial(ConstructorDeclarationSyntax constructor,
            TypeDeclarationSyntax @class)
        {
            var classPartial = @class.Kind() == SyntaxKind.ClassDeclaration
                ? (TypeDeclarationSyntax) ClassDeclaration(@class.Identifier.WithoutTrivia())
                    .AddModifiers(Token(SyntaxKind.PartialKeyword))
                : (TypeDeclarationSyntax) StructDeclaration(@class.Identifier.WithoutTrivia())
                    .AddModifiers(Token(SyntaxKind.PartialKeyword));

            var arguments = constructor.ParameterList.Parameters.Select(x => Argument(IdentifierName(x.Identifier)));

            var parameterSyntax = Parameter(Identifier("pool")).WithType(ParseName("ObjectPoolContext"));
            var paramsList = new List<ParameterSyntax> {parameterSyntax};
            paramsList.AddRange(constructor.ParameterList.Parameters);

            var newObject = ObjectCreationExpression(IdentifierName(@class.Identifier))
                .WithArgumentList(ArgumentList(SeparatedList(arguments)));

            var argumentForPool = new List<ArgumentSyntax>();
            argumentForPool.Add(Argument(ParseExpression(
                $"() => new {@class.Identifier}({string.Join(",", constructor.ParameterList.Parameters.Select(x => x.Identifier))})")));
            argumentForPool.Add(Argument(ParseExpression(
                $"x => x.Update({string.Join(",", constructor.ParameterList.Parameters.Select(x => x.Identifier))})")));

            return classPartial
                    .AddMembers(
                        MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Update"))
                            .WithParameterList(constructor.ParameterList)
                            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                            .WithBody(constructor.Body)
                    )
                    .AddMembers(
                        MethodDeclaration(IdentifierName(@class.Identifier), "Get" + @class.Identifier)
                            .AddModifiers(Token(SyntaxKind.PublicKeyword))
                            .AddModifiers(Token(SyntaxKind.StaticKeyword))
                            .WithParameterList(ParameterList(SeparatedList(paramsList)))
                            .WithBody(Block(ReturnStatement(
                                InvocationExpression(ParseName($"pool.Get<{@class.Identifier}>"),
                                    ArgumentList(SeparatedList(argumentForPool)))
                            )))
                    )
                ;
        }

        private static ClassDeclarationSyntax PoolExtension(ConstructorDeclarationSyntax constructor,
            TypeDeclarationSyntax @class)
        {
            var arguments = new List<ArgumentSyntax>();
            var poolParameterIdentifier = Identifier("pool");

            arguments.Add(Argument(IdentifierName(poolParameterIdentifier)));
            arguments.AddRange(
                constructor.ParameterList.Parameters.Select(x => Argument(IdentifierName(x.Identifier))));

            var parameterSyntax = Parameter(poolParameterIdentifier).WithType(ParseName("ObjectPoolContext"))
                .AddModifiers(Token(SyntaxKind.ThisKeyword));
            var paramsList = new List<ParameterSyntax> {parameterSyntax};
            paramsList.AddRange(constructor.ParameterList.Parameters);

            var objectPoolExtension =
                    ClassDeclaration(Identifier("PoolExtension" + @class.Identifier))
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddModifiers(Token(SyntaxKind.StaticKeyword))
                        .AddMembers(
                            MethodDeclaration(IdentifierName(@class.Identifier), "Get" + @class.Identifier)
                                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                .AddModifiers(Token(SyntaxKind.StaticKeyword))
                                .WithParameterList(ParameterList(SeparatedList(paramsList)))
                                .WithBody(Block(ReturnStatement(
                                    InvocationExpression(ParseName($"{@class.Identifier}.Get{@class.Identifier}"),
                                        ArgumentList(SeparatedList(arguments)))
                                )))
                        )
                ;
            return objectPoolExtension;
        }
    }
}