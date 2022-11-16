using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class ValidationService
    {
        private readonly IServiceProvider serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="model"/> is valid.
        /// Otherwise returns <c>false</c> and populates <paramref name="validationResults"/>.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="validationResults"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryValidate(object model, out IEnumerable<ValidationResult> validationResults)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var context = new ValidationContext(model, serviceProvider, items: null);
            var validationResultsList = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, validationResultsList, validateAllProperties: true);

            if (isValid)
            {
                validationResults = Enumerable.Empty<ValidationResult>();
                return true;
            }
            else
            {
                validationResults = validationResultsList;
                return false;
            }
        }

        /// <summary>
        /// Throws <see cref="ServiceException"/> if <paramref name="model"/> is invalid.
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="ServiceException"></exception>
        public void Validate(object model)
        {
            if (!TryValidate(model, out var validationResults))
            {
                throw new ServiceException(validationResults);
            }
        }

        /// <summary>
        /// If any <paramref name="models"/> are invalid this will
        /// throw <see cref="ServiceException"/> after validating all 
        /// <paramref name="models"/>.
        /// </summary>
        /// <param name="models"></param>
        /// <exception cref="ServiceException"></exception>
        public void ValidateAll(IEnumerable<object> models)
        {
            var validationResults = new List<ValidationResult>();

            foreach (var model in models)
            {
                if (!TryValidate(model, out var results))
                {
                    validationResults.AddRange(results);
                }
            }

            if (validationResults.Any())
            {
                throw new ServiceException(validationResults);
            }
        }

        /// <summary>
        /// If any <paramref name="models"/> are invalid this will
        /// throw <see cref="ServiceException"/> after validating all 
        /// <paramref name="models"/>.
        /// </summary>
        /// <param name="models"></param>
        /// <exception cref="ServiceException"></exception>
        public void ValidateAll(params object[] models)
        {
            ValidateAll((IEnumerable<object>)models);
        }
    }
}