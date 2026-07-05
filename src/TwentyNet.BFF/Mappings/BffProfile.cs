using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.People;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.People;

namespace TwentyNet.BFF.Mappings;

public sealed class BffProfile : Profile
{
    public BffProfile()
    {
        CreateMap<CompanyDto, CompanyResponse>();
        CreateMap<PersonDto, PersonResponse>();
    }
}
