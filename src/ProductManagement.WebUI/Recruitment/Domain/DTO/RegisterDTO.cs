using System.ComponentModel.DataAnnotations;

namespace Domain.DTO
{
    public class RegisterDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        public string VietnameseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Place of birth is required")]
        public string PlaceOfBirth { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID Card/Passport number is required")]
        public string IdCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of issue is required")]
        public DateTime? DateOfIssue { get; set; }

        public string PlaceOfIssue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [ValidateNotEmpty(ErrorMessage = "Please select a city")]
        public string City { get; set; } = string.Empty;

        public string CurrentAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Office location is required")]
        [ValidateNotEmpty(ErrorMessage = "Please select an office location")]
        public string PreferableOfficeLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "First preference is required")]
        [ValidateNotEmpty(ErrorMessage = "Please select first preference")]
        public string FirstPreference { get; set; } = string.Empty;

        public string? SecondPreference { get; set; }
    }

    public class ValidateNotEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            var stringValue = value.ToString();
            return !string.IsNullOrWhiteSpace(stringValue);
        }
    }

}