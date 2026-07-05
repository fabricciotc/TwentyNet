using AutoMapper;
using TwentyNet.Application.Mappings;

namespace TwentyNet.Application.Tests;

public static class MapperTestHelper
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>());
        return config.CreateMapper();
    }
}
