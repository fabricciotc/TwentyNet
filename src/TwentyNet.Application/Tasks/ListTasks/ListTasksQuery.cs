using MediatR;

namespace TwentyNet.Application.Tasks.ListTasks;

public sealed record ListTasksQuery(
    Guid? CompanyId,
    Guid? PersonId) : IRequest<IReadOnlyList<TaskDto>>;
