using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Rules;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Move
{
    // The four status moves of an unforeseen request (design §6.3, F-30). Each is a thin command over the
    // shared UnforeseenRequestMover, which validates the (action, status) pair, the holder office and the
    // remark, and writes one T-49 movement with the T-48 status/holder update. AUTHORISE is its own slice
    // (P4-12) because it also creates provisions and a work code.

    public sealed record SubmitUnforeseenCommand(long Id, string? Remark) : IRequest<Response>;
    public sealed record ForwardUnforeseenCommand(long Id, string? Remark) : IRequest<Response>;
    public sealed record ReturnUnforeseenCommand(long Id, string? Remark) : IRequest<Response>;
    public sealed record ResubmitUnforeseenCommand(long Id, string? Remark) : IRequest<Response>;

    public sealed class SubmitUnforeseenHandler(UnforeseenRequestMover mover) : IRequestHandler<SubmitUnforeseenCommand, Response>
    {
        public Task<Response> Handle(SubmitUnforeseenCommand r, CancellationToken ct) =>
            mover.MoveAsync(r.Id, UnforeseenAction.Submit, r.Remark, ct);
    }

    public sealed class ForwardUnforeseenHandler(UnforeseenRequestMover mover) : IRequestHandler<ForwardUnforeseenCommand, Response>
    {
        public Task<Response> Handle(ForwardUnforeseenCommand r, CancellationToken ct) =>
            mover.MoveAsync(r.Id, UnforeseenAction.Forward, r.Remark, ct);
    }

    public sealed class ReturnUnforeseenHandler(UnforeseenRequestMover mover) : IRequestHandler<ReturnUnforeseenCommand, Response>
    {
        public Task<Response> Handle(ReturnUnforeseenCommand r, CancellationToken ct) =>
            mover.MoveAsync(r.Id, UnforeseenAction.Return, r.Remark, ct);
    }

    public sealed class ResubmitUnforeseenHandler(UnforeseenRequestMover mover) : IRequestHandler<ResubmitUnforeseenCommand, Response>
    {
        public Task<Response> Handle(ResubmitUnforeseenCommand r, CancellationToken ct) =>
            mover.MoveAsync(r.Id, UnforeseenAction.Resubmit, r.Remark, ct);
    }

    public sealed class UnforeseenMoveEndpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // The office-level right of each move is enforced by the holder-office check in the mover (design
            // §6.3); the endpoint gate is the F-30 right the acting level holds.
            app.MapPost("/api/unforeseen-requests/{id:long}/submit",
                    async (long id, TransitionRemarkDto? dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new SubmitUnforeseenCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:F30.Submit").WithName("SubmitUnforeseen").WithTags("UnforeseenWorks")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/unforeseen-requests/{id:long}/forward",
                    async (long id, TransitionRemarkDto? dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new ForwardUnforeseenCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:F30.Examine").WithName("ForwardUnforeseen").WithTags("UnforeseenWorks")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/unforeseen-requests/{id:long}/return",
                    async (long id, TransitionRemarkDto? dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new ReturnUnforeseenCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("ReturnUnforeseen").WithTags("UnforeseenWorks")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/unforeseen-requests/{id:long}/resubmit",
                    async (long id, TransitionRemarkDto? dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new ResubmitUnforeseenCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:F30.Submit").WithName("ResubmitUnforeseen").WithTags("UnforeseenWorks")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
