using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sync.ListEmailMessages;

public sealed class ListEmailMessagesQueryHandler : IRequestHandler<ListEmailMessagesQuery, IReadOnlyList<EmailMessageDto>>
{
    private readonly IRepository<EmailMessage> _repository;
    private readonly IMapper _mapper;

    public ListEmailMessagesQueryHandler(IRepository<EmailMessage> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailMessageDto>> Handle(ListEmailMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _repository.ListAsync(
            m => m.ConnectedAccountId == request.ConnectedAccountId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<EmailMessageDto>>(messages);
    }
}
