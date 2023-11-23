using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MakeConst.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MakeConstAnalyzer : DiagnosticAnalyzer
{
    public const string MakeConstDiagnosticId = "MakeConst";

    private static readonly DiagnosticDescriptor MakeConstRule =
        new DiagnosticDescriptor(
#pragma warning disable RS2008
            MakeConstDiagnosticId,
#pragma warning restore RS2008
            "Make Constant",
            "Can be made const",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MakeConstRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.LocalDeclarationStatement);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var localDeclaration = (LocalDeclarationStatementSyntax) context.Node;

        if (CanBeMadeConst(localDeclaration, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(MakeConstRule, context.Node.GetLocation()));
        }
    }

    private static bool CanBeMadeConst(LocalDeclarationStatementSyntax localDeclaration, SemanticModel semanticModel)
    {
        // already const?
        if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return false;
        }

        // ensure that all variables in the local declaration have initializers that
        // are assigned
        foreach (var variable in localDeclaration.Declaration.Variables)
        {
            var initializer = variable.Initializer;

            if (initializer is null)
            {
                return false;
            }

            var constantValue = semanticModel.GetConstantValue(initializer.Value);
            if (!constantValue.HasValue)
            {
                return false;
            }

            var variableTypeName = localDeclaration.Declaration.Type;
            var variableType = semanticModel.GetTypeInfo(variableTypeName).ConvertedType;
            if (variableType is null)
            {
                return false;
            }

            // ensure that the initializer value can be converted to the type of the
            // local declaration without a user-defined conversion.
            var conversion = semanticModel.ClassifyConversion(initializer.Value, variableType);
            if (!conversion.Exists || conversion.IsUserDefined)
            {
                return false;
            }

            // Special cases:
            //   * If the constant value is a string, the type of the local declaration
            //     must be System.String.
            //   * If the constant value is null, the type of the local declaration must
            //     be a reference type.
            if (constantValue.Value is string && variableType.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            if (variableType.IsReferenceType && constantValue.Value != null)
            {
                return false;
            }
        }

        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(localDeclaration);

        // retrieve the local symbol for each variable in the local declaration
        // and ensure that it is not written outside of the data flow analysis region.
        foreach (var variable in localDeclaration.Declaration.Variables)
        {
            var variableSymbol = semanticModel.GetDeclaredSymbol(variable);

            if (dataFlowAnalysis is not null &&
                variableSymbol is not null &&
                dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
            {
                return false;
            }
        }

        return true;
    }
}
