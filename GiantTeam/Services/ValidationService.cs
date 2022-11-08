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
        /// Throws if <paramref name="model"/> is invalid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceException"></exception>
        public void Validate<T>(T model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var context = new ValidationContext(model, serviceProvider, items: null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, validationResults);

            if (!isValid)
            {
                throw new ServiceException(validationResults);
            }
        }
    }
}