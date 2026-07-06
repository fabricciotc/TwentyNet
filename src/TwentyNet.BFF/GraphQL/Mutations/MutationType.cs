using HotChocolate.Resolvers;
using HotChocolate.Types;
using MediatR;
using TwentyNet.Application.Auth.LoginUser;
using TwentyNet.Application.Companies.CreateCompany;
using TwentyNet.Application.Companies.DeleteCompany;
using TwentyNet.Application.Companies.UpdateCompany;
using TwentyNet.Application.People.CreatePerson;
using TwentyNet.Application.People.DeletePerson;
using TwentyNet.Application.People.UpdatePerson;
using TwentyNet.BFF.GraphQL.Extensions;
using TwentyNet.BFF.GraphQL.Inputs;
using TwentyNet.BFF.GraphQL.Types;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.GraphQL.Mutations;

public sealed class MutationType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");
        descriptor.Field("login")
            .Type<NonNullType<ObjectType<AuthPayloadType>>>()
            .Argument("input", a => a.Type<NonNullType<InputObjectType<LoginInput>>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var input = ctx.ArgumentValue<LoginInput>("input");

                var result = await sender.Send(
                    new LoginUserCommand(input.Email, input.Password, input.WorkspaceId),
                    ctx.RequestAborted);

                return new AuthPayloadType(
                    result.AccessToken,
                    result.RefreshToken,
                    result.ExpiresIn,
                    result.UserId,
                    result.WorkspaceId,
                    result.Role.ToString());
            });

        descriptor.Field("createCompany")
            .Type<NonNullType<ObjectType<CompanyType>>>()
            .Argument("input", a => a.Type<NonNullType<InputObjectType<CreateCompanyInput>>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var input = ctx.ArgumentValue<CreateCompanyInput>("input");
                var result = await sender.Send(
                    new CreateCompanyCommand(input.Name, input.DomainName, input.Address),
                    ctx.RequestAborted);

                return result.ToGraphQL();
            });

        descriptor.Field("updateCompany")
            .Type<NonNullType<ObjectType<CompanyType>>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Argument("input", a => a.Type<NonNullType<InputObjectType<UpdateCompanyInput>>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var input = ctx.ArgumentValue<UpdateCompanyInput>("input");
                var result = await sender.Send(
                    new UpdateCompanyCommand(ctx.ArgumentValue<Guid>("id"), input.Name, input.DomainName, input.Address),
                    ctx.RequestAborted);

                return result.ToGraphQL();
            });

        descriptor.Field("deleteCompany")
            .Type<NonNullType<BooleanType>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                await sender.Send(new DeleteCompanyCommand(ctx.ArgumentValue<Guid>("id")), ctx.RequestAborted);
                return true;
            });

        descriptor.Field("createPerson")
            .Type<NonNullType<ObjectType<PersonType>>>()
            .Argument("input", a => a.Type<NonNullType<InputObjectType<CreatePersonInput>>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var input = ctx.ArgumentValue<CreatePersonInput>("input");
                var result = await sender.Send(
                    new CreatePersonCommand(input.FirstName, input.LastName, input.Email, input.Phone, input.CompanyId),
                    ctx.RequestAborted);

                return result.ToGraphQL();
            });

        descriptor.Field("updatePerson")
            .Type<NonNullType<ObjectType<PersonType>>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Argument("input", a => a.Type<NonNullType<InputObjectType<UpdatePersonInput>>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                var input = ctx.ArgumentValue<UpdatePersonInput>("input");
                var result = await sender.Send(
                    new UpdatePersonCommand(ctx.ArgumentValue<Guid>("id"), input.FirstName, input.LastName, input.Email, input.Phone, input.CompanyId),
                    ctx.RequestAborted);

                return result.ToGraphQL();
            });

        descriptor.Field("deletePerson")
            .Type<NonNullType<BooleanType>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Resolve(async (IResolverContext ctx) =>
            {
                var sender = ctx.Services.GetRequiredService<ISender>();
                var authContext = ctx.Services.GetRequiredService<IAuthContext>();

                if (authContext.WorkspaceId is null)
                {
                    throw new UnauthorizedAccessException("Workspace not selected.");
                }

                await sender.Send(new DeletePersonCommand(ctx.ArgumentValue<Guid>("id")), ctx.RequestAborted);
                return true;
            });
    }
}
