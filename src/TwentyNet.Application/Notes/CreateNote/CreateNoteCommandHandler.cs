using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Notes.CreateNote;

public sealed class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<Person> _personRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IPublisher _publisher;

    public CreateNoteCommandHandler(
        IRepository<Note> noteRepository,
        IRepository<Company> companyRepository,
        IRepository<Person> personRepository,
        IMapper mapper,
        IAuthContext authContext,
        IPublisher publisher)
    {
        _noteRepository = noteRepository;
        _companyRepository = companyRepository;
        _personRepository = personRepository;
        _mapper = mapper;
        _authContext = authContext;
        _publisher = publisher;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var userId = _authContext.UserId.Value;

        await ValidateRecordExistsAsync(request, workspaceId, cancellationToken);

        var note = new Note
        {
            Title = request.Title,
            Content = request.Content,
            WorkspaceId = workspaceId,
            CreatedByUserId = userId,
            CompanyId = request.CompanyId,
            PersonId = request.PersonId
        };

        await _noteRepository.AddAsync(note, cancellationToken);
        await _noteRepository.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new NoteCreatedEvent(workspaceId, note.Id, request.CompanyId, request.PersonId, userId),
            cancellationToken);

        return _mapper.Map<NoteDto>(note);
    }

    private async Task ValidateRecordExistsAsync(CreateNoteCommand request, Guid workspaceId, CancellationToken cancellationToken)
    {
        if (request.CompanyId.HasValue)
        {
            var companies = await _companyRepository.ListAsync(
                c => c.Id == request.CompanyId.Value && c.WorkspaceId == workspaceId,
                cancellationToken);

            if (!companies.Any())
            {
                throw new KeyNotFoundException($"Company with id {request.CompanyId.Value} not found.");
            }
        }

        if (request.PersonId.HasValue)
        {
            var people = await _personRepository.ListAsync(
                p => p.Id == request.PersonId.Value && p.WorkspaceId == workspaceId,
                cancellationToken);

            if (!people.Any())
            {
                throw new KeyNotFoundException($"Person with id {request.PersonId.Value} not found.");
            }
        }
    }
}
