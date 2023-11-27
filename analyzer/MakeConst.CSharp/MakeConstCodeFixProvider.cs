using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace MakeConst.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeConstCodeFixProvider)), Shared]
public sealed class MakeConstCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MakeConstAnalyzer.MakeConstDiagnosticId);

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var declaration = root.FindToken(diagnosticSpan.Start).Parent!
            .AncestorsAndSelf()
            .OfType<LocalDeclarationStatementSyntax>()
            .First();

        var codeAction = CodeAction.Create(
            "Make constant",
            c => MakeConstantAsync(context.Document, declaration, c),
            equivalenceKey: nameof(MakeConstCodeFixProvider));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> MakeConstantAsync(
        Document document,
        LocalDeclarationStatementSyntax localDeclaration,
        CancellationToken cancellationToken)
    {
        // remove leading trivia from local declaration
        var firstToken = localDeclaration.GetFirstToken();
        var leadingTrivia = firstToken.LeadingTrivia;

        var trimmedLocal = localDeclaration.ReplaceToken(
            firstToken,
            firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

        // create a const token with the leading trivia
        var constToken = SyntaxFactory.Token(
            leadingTrivia,
            SyntaxKind.ConstKeyword,
            SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

        // insert the const token into the modifiers list, creating a new modifiers list
        var newModifiers = trimmedLocal.Modifiers.Insert(0, constToken);

        // if the type of declaration is 'var', create a new type name for the
        // type inferred for 'var'
        var variableDeclaration = localDeclaration.Declaration;
        var variableTypeName = variableDeclaration.Type;

        if (variableTypeName.IsVar)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // special case: Ensure that 'var' isn't actually an alias to another type
            // (e.g. using var = System.String).
            var aliasInfo = semanticModel?.GetAliasInfo(variableTypeName, cancellationToken);

            if (semanticModel is not null && aliasInfo is null)
            {
                var type = semanticModel.GetTypeInfo(variableTypeName, cancellationToken).ConvertedType;

                // special case: Ensure that 'var' isn't actually a type named 'var'.
                if (type is not null && type.Name != "var")
                {
                    var typeName = SyntaxFactory.ParseTypeName(type.ToDisplayString())
                        .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
                        .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());

                    // add an annotation to simplify the type name.
                    var simplifiedTypeName = typeName.WithAdditionalAnnotations(Simplifier.Annotation);

                    // replace the type in the variable declaration.
                    variableDeclaration = variableDeclaration.WithType(simplifiedTypeName);
                }
            }
        }

        var newLocal = trimmedLocal
            .WithModifiers(newModifiers)
            .WithDeclaration(variableDeclaration);

        var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(localDeclaration, formattedLocal);

        return document.WithSyntaxRoot(newRoot);
    }
}
