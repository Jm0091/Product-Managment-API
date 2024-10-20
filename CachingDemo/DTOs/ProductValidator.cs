using FluentValidation;
using FluentValidation.AspNetCore;

namespace CachingDemo.DTOs
{
    public class ProductValidator : AbstractValidator<ProductDTO>
    {

        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .Length(1, 100).WithMessage("Product name length must be between 1 and 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Product price must be greater than zero.");
        }
    }


}
