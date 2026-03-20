using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.GenerateWorkCodes.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GenerateWorkCodes
{
    public class GenerateWorkCodeHandler
        : IRequestHandler<GenerateWorkCodeQuery, Response<GenerateWorkCodeResponseDto>>
    {
        private readonly BudgetContext _context;

        public GenerateWorkCodeHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<GenerateWorkCodeResponseDto>> Handle(
            GenerateWorkCodeQuery request,
            CancellationToken cancellationToken)
        {
            var response = new Response<GenerateWorkCodeResponseDto>();

            // 🔎 Validate Division Exists
            var divisionExists = await _context.Divisions
                .FirstOrDefaultAsync(x => x.DivisionCode == request.DivisionCode, cancellationToken);

            if ( divisionExists is null)
            {
                response.IsSucced = false;
                response.Message = $"DivisionCode {request.DivisionCode} does not exist";
                return response;
            }

            // 🔎 Get Last WorkCode for that Division
            var lastWorkCode = await _context.Set<Work>()
                .Where(x => x.DivisionId == divisionExists.Id)
                .OrderByDescending(x => x.WorkCode)
                .Select(x => x.WorkCode)
                .FirstOrDefaultAsync(cancellationToken);

            int nextSequence = 1;

            if (!string.IsNullOrWhiteSpace(lastWorkCode.ToString()))
            {
                var divisionLength = request.DivisionCode.ToString().Length;

                if (lastWorkCode.ToString().Length > divisionLength)
                {
                    var sequencePart = lastWorkCode.ToString().Substring(divisionLength);

                    if (int.TryParse(sequencePart, out var parsed))
                    {
                        nextSequence = parsed + 1;
                    }
                }
            }

            // 🔥 Always 4 digit padded
            var newWorkCode = $"{request.DivisionCode}{nextSequence:D4}";

            response.IsSucced = true;
            response.Message = "WorkCode generated successfully";
            response.Data = new GenerateWorkCodeResponseDto
            {
                WorkCode = newWorkCode,
                DivisionCode = request.DivisionCode,
                SequenceNumber = nextSequence
            };

            return response;
        }
    }
}