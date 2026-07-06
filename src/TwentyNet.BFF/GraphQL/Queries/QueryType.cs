using HotChocolate.Resolvers;
using HotChocolate.Types;
using MediatR;
using TwentyNet.Application.Companies.GetCompanyById;
using TwentyNet.Application.Companies.ListCompanies;
using TwentyNet.Application.People.GetPersonById;
using TwentyNet.Application.People.ListPeople;
using TwentyNet.Application.Views.ListViews;
using TwentyNet.BFF.GraphQL.Extensions;
using TwentyNet.BFF.GraphQL.Types;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.GraphQL.Queries;

public sealed class QueryType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Query");
        descriptor.Field("companies")
            .Type<ListType<NonNullType<ObjectType<CompanyType>>>>()
            .Argument("search", a => a.Type<StringType>().DefaultValue(null))
            .Argument("skip", a => a.Type<IntType>().DefaultValue(0))
            .Argument("take", a => a.Type<IntType>().DefaultValue(50))
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var result = await sender.Send(
                    new ListCompaniesQuery(
                        null,
                        ctx.ArgumentValue<string?>("search"),
                        null,
                        null,
                        ctx.ArgumentValue<int>("skip"),
                        ctx.ArgumentValue<int>("take")),
                    ctx.RequestAborted);

                return result.Items.Select(i => i.ToGraphQL()).ToList();
            });

        descriptor.Field("company")
            .Type<ObjectType<CompanyType>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var result = await sender.Send(
                    new GetCompanyByIdQuery(ctx.ArgumentValue<Guid>("id")),
                    ctx.RequestAborted);

                return result?.ToGraphQL();
            });

        descriptor.Field("people")
            .Type<ListType<NonNullType<ObjectType<PersonType>>>>()
            .Argument("search", a => a.Type<StringType>().DefaultValue(null))
            .Argument("skip", a => a.Type<IntType>().DefaultValue(0))
            .Argument("take", a => a.Type<IntType>().DefaultValue(50))
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var result = await sender.Send(
                    new ListPeopleQuery(
                        null,
                        ctx.ArgumentValue<string?>("search"),
                        null,
                        null,
                        ctx.ArgumentValue<int>("skip"),
                        ctx.ArgumentValue<int>("take")),
                    ctx.RequestAborted);

                return result.Items.Select(i => i.ToGraphQL()).ToList();
            });

        descriptor.Field("person")
            .Type<ObjectType<PersonType>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var result = await sender.Send(
                    new GetPersonByIdQuery(ctx.ArgumentValue<Guid>("id")),
                    ctx.RequestAborted);

                return result?.ToGraphQL();
            });

        descriptor.Field("views")
            .Type<ListType<NonNullType<ObjectType<ViewType>>>>()
            .Argument("objectName", a => a.Type<StringType>().DefaultValue(null))
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var result = await sender.Send(
                    new ListViewsQuery(ctx.ArgumentValue<string?>("objectName")),
                    ctx.RequestAborted);

                return result.Select(v => v.ToGraphQL()).ToList();
            });
    }
}
