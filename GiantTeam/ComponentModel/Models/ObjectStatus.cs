﻿using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel.Models
{
    public class ObjectStatus
    {
        /// <summary>
        /// The client request lacks valid authentication credentials for the requested resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ObjectStatus Unauthorized(string message)
        {
            return new(401, "Unauthorized", message, new());
        }

        /// <summary>
        /// Indicates that the server understands the request but refuses to authorize it.
        /// This status is similar to 401, but for the 403 Forbidden status code, re-authenticating makes no difference.
        /// The access is tied to the application logic, such as insufficient rights to a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ObjectStatus Forbidden(string message)
        {
            return new(403, "Forbidden", message, new());
        }

        public static ObjectStatus NotFound(string message)
        {
            return new(404, "Not Found", message, new());
        }

        public static ObjectStatus InvalidRequest(string message)
        {
            return InvalidRequest(message, Enumerable.Empty<ObjectStatusDetail>());
        }

        public static ObjectStatus InvalidRequest(string message, IEnumerable<ObjectStatusDetail> details)
        {
            return new(400, "Invalid Request", message, new(details));
        }

        public static ObjectStatus InvalidRequest(IEnumerable<ValidationResult> validationResults)
        {
            var count = validationResults.Count();

            if (count == 0)
            {
                throw new ArgumentException($"The {nameof(validationResults)} argument must contain at least one item.", nameof(validationResults));
            }

            return InvalidRequest(count == 1 ? validationResults.First().ErrorMessage! : $"{validationResults.First().ErrorMessage!.TrimEnd('.')} and {count - 1} other inputs were invalid.", validationResults.Select(vr => new ObjectStatusDetail(vr.ErrorMessage!, vr.MemberNames)));
        }

        public ObjectStatus(int status, string statusText, string message, List<ObjectStatusDetail> details)
        {
            Status = status;
            StatusText = statusText;
            Message = message;
            Details = details;
        }

        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Message { get; set; }
        public List<ObjectStatusDetail> Details { get; }
    }
}
