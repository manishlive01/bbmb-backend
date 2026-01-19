using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public class UpdateEstimateHandler : IRequestHandler<UpdateEstimateCommand, Response<string>>
    {
        private readonly BudgetContext _context;
        private readonly IValidator<UpdateEstimateDto> _validator;

        public UpdateEstimateHandler(BudgetContext context, IValidator<UpdateEstimateDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Response<string>> Handle(UpdateEstimateCommand request, CancellationToken cancellationToken)
        {
            // Validate
            var validationResult = await _validator.ValidateAsync(request.Dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new Response<string>
                {
                    IsSucced = false,
                    Message = errors
                };
            }

            // Find estimate
            var estimate = await _context.Estimates
                .FirstOrDefaultAsync(e => e.WorkCode == request.WorkCode && e.EstimateCode == request.EstimateCode, cancellationToken);

            if (estimate == null)
            {
                return new Response<string>
                {
                    IsSucced = false,
                    Message = "Estimate not found"
                };
            }

            // Update fields
            estimate.EstimateDesc = request.Dto.EstimateDesc;
            estimate.EstimateAmt = request.Dto.EstimateAmt;
            estimate.EstimateNarr = request.Dto.EstimateNarr;
            estimate.FinYearEnd = request.Dto.FinYearEnd;
            estimate.DivisionCode = request.Dto.DivisionCode;

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>
            {
                IsSucced = true,
                Message = "Estimate updated successfully"
            };
        }
    }
}
