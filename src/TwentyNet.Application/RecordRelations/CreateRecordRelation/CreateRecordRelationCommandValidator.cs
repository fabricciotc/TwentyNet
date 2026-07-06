using FluentValidation;

namespace TwentyNet.Application.RecordRelations.CreateRecordRelation;

public sealed class CreateRecordRelationCommandValidator : AbstractValidator<CreateRecordRelationCommand>
{
    public CreateRecordRelationCommandValidator()
    {
        RuleFor(x => x.SourceObjectName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetObjectName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RelationType).NotEmpty().MaximumLength(100);
    }
}
