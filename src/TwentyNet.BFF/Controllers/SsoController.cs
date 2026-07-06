using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Auth;
using TwentyNet.Application.Auth.ProvisionSsoUser;
using TwentyNet.Application.Sso.CreateSsoProvider;
using TwentyNet.Application.Sso.DeleteSsoProvider;
using TwentyNet.Application.Sso.GetSsoProviderById;
using TwentyNet.Application.Sso.ListSsoProviders;
using TwentyNet.Application.Sso.UpdateSsoProvider;
using TwentyNet.BFF.Services;
using TwentyNet.Contracts.Sso;
using TwentyNet.Domain.Enums;
using TwentyNet.Persistence;
using DomainEnums = TwentyNet.Domain.Enums;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Route("api/sso")]
public sealed class SsoController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly SamlService _samlService;

    public SsoController(ISender sender, IMapper mapper, AppDbContext context, SamlService samlService)
    {
        _sender = sender;
        _mapper = mapper;
        _context = context;
        _samlService = samlService;
    }

    [HttpGet("providers")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<IReadOnlyList<SsoProviderResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListSsoProvidersQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<SsoProviderResponse>>(result));
    }

    [HttpGet("providers/{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<SsoProviderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSsoProviderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<SsoProviderResponse>(result));
    }

    [HttpPost("providers")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<SsoProviderResponse>> Create(
        [FromBody] CreateSsoProviderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSsoProviderCommand(
            request.Name,
            MapType(request.Type),
            request.ClientId,
            request.ClientSecret,
            request.AuthorizationEndpoint,
            request.TokenEndpoint,
            request.UserInfoEndpoint,
            request.EntityId,
            request.SingleSignOnUrl,
            request.Certificate,
            request.MetadataUrl);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<SsoProviderResponse>(result));
    }

    [HttpPut("providers/{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<SsoProviderResponse>> Update(
        Guid id,
        [FromBody] UpdateSsoProviderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSsoProviderCommand(
            id,
            request.Name,
            request.IsActive,
            MapType(request.Type),
            request.ClientId,
            request.ClientSecret,
            request.AuthorizationEndpoint,
            request.TokenEndpoint,
            request.UserInfoEndpoint,
            request.EntityId,
            request.SingleSignOnUrl,
            request.Certificate,
            request.MetadataUrl);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<SsoProviderResponse>(result));
    }

    [HttpDelete("providers/{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteSsoProviderCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("saml/{providerId:guid}/login")]
    [AllowAnonymous]
    public async Task<ActionResult<SamlLoginResponse>> SamlLogin(Guid providerId, CancellationToken cancellationToken)
    {
        var provider = await _context.SsoProviders.FindAsync(new object[] { providerId }, cancellationToken);
        if (provider is null || !provider.IsActive || provider.Type != SsoProviderType.Saml)
        {
            return NotFound("SAML provider not found or inactive.");
        }

        var acsUrl = Url.Action("SamlAcs", "Sso", new { providerId }, Request.Scheme)
            ?? throw new InvalidOperationException("Could not generate ACS URL.");

        var xml = _samlService.BuildAuthnRequest(provider, acsUrl);
        var samlRequest = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

        return Ok(new SamlLoginResponse(provider.SingleSignOnUrl ?? string.Empty, samlRequest, providerId.ToString()));
    }

    [HttpPost("saml/{providerId:guid}/acs")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> SamlAcs(
        Guid providerId,
        [FromForm] string samlResponse,
        CancellationToken cancellationToken)
    {
        var provider = await _context.SsoProviders.FindAsync(new object[] { providerId }, cancellationToken);
        if (provider is null || !provider.IsActive || provider.Type != SsoProviderType.Saml)
        {
            return NotFound("SAML provider not found or inactive.");
        }

        var saml = _samlService.ParseSamlResponse(samlResponse);
        var command = new ProvisionSsoUserCommand(
            saml.Email,
            saml.FirstName,
            saml.LastName,
            provider.WorkspaceId,
            WorkspaceRole.Member);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    private static DomainEnums.SsoProviderType MapType(string type)
    {
        if (!Enum.TryParse<DomainEnums.SsoProviderType>(type, true, out var result))
        {
            throw new ArgumentException($"Invalid SSO provider type '{type}'.", nameof(type));
        }

        return result;
    }
}
