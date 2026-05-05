using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Application.Confessions.Commands.CreateConfession
{
    public class CreateConfessionValidator : AbstractValidator<CreateConfessionCommand>
    {
        public CreateConfessionValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Confession text is required.");

            RuleFor(x => x.Alias)
                .NotEmpty().WithMessage("Alias is required.");

            RuleFor(x => x.Intensity)
                .InclusiveBetween((short)1, (short)5);

            RuleFor(x => x.Category)
                .IsInEnum();
        }
    }
}
